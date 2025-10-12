using System;
using System.Collections.Generic;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class PlayerAgent : Agent
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerShooting playerShooting;
    [SerializeField] private Terrain terrain;
    [SerializeField] private Transform goal;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private Renderer successIndicator;
    [SerializeField] private PlayerAgentLogger logger;
    private TrainingReferences trainingReferences;

    [Header("Options")]
    [SerializeField] private bool heuristic = false;
    [SerializeField] private float heuristicLookSensitivity = 0.0005f;
    [SerializeField] private float meaningfulDistance = 15f;
    [SerializeField] private float maxDrawingTime;

    [Header("Boundaries")]
    [SerializeField] private float killY = -10f;
    [SerializeField] private Vector3 minStart, maxStart;
    [SerializeField] private float minSpawnY;
    [SerializeField] private bool divideSpawns;
    [SerializeField] private float horizontalDivider = 0, dividerAngle = 90, gizmoDividerY;

    [Header("Rewards")]
    [SerializeField] private float winReward;
    [SerializeField] private float hitTargetReward, hitAllTargetsReward, hitEnemyReward, killedEnemyReward;
    [SerializeField] private float distanceToGoalReward = 0.1f;

    [Header("Penalties")]
    [SerializeField] private float dieReward;
    [SerializeField] private float existenceReward, timeoutReward, damageReward = -0.05f, riverDieReward = -0.1f;
    [SerializeField] private float penaltyNearEnemy = -0.005f, penaltyJump = -0.005f, penaltyArrow = -0.005f;
    [SerializeField] private float dangerDistance = 3f;


    private Vector2 horizontal, look;
    private bool jumped;
    private double previousDistance = 1f;
    private bool shot;
    private float drawStartTime;
    private readonly HashSet<int> targetsHit = new();
    private int totalTargets;

    public override void Initialize()
    {
        minStart += transform.position;
        maxStart += transform.position;
    }

    private void Start()
    {
        trainingReferences = GetComponentInParent<TrainingReferences>();
        totalTargets = trainingReferences.GetAllTargets().Count;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (sensor == null) return;

        Transform player = playerMovement.transform;

        // Goal observations
        sensor.AddObservation(AngleBetweenHorizontal(player, goal, 0f));  // [-1,1] angle on the horizontal plane
        sensor.AddObservation(AngleBetweenVertical(player, goal, 0f));    // [-1,1] vertical angle between the horizontal plane and the vector between the player and the goal
        sensor.AddObservation(DistanceBetween(player, goal, -1f)); // [ 0,1]

        // Enemy observations
        Transform nearestEnemy = GetNearestEnemy(player.position);

        sensor.AddObservation(nearestEnemy == null ? 0 : 1); // does enemy exist
        sensor.AddObservation(AngleBetweenHorizontal(player, nearestEnemy, 0f));  // [-1,1]
        sensor.AddObservation(AngleBetweenVertical(player, nearestEnemy, 0f));    // [-1,1]
        sensor.AddObservation(DistanceBetween(player, nearestEnemy, -1f)); // [0,1] or -1

        // Target observations
        Transform nearestTarget = GetNearestUnhitTarget(player.position);

        sensor.AddObservation(nearestTarget == null ? 0 : 1); // are there any unhit targets
        sensor.AddObservation(AngleBetweenHorizontal(player, nearestTarget, 0f));  // [-1,1]
        sensor.AddObservation(AngleBetweenVertical(player, nearestTarget, 0f));    // [-1,1]
        sensor.AddObservation(DistanceBetween(player, nearestTarget, -1f)); // [0,1] or -1

        // Actions observations
        sensor.AddObservation(playerMovement.CanJump() ? 1 : 0);
    }


    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        logger.OnStep();

        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.moveX], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.moveZ], -1f, 1f);
        float lookX = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.lookX], -1f, 1f);
        float lookY = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.lookZ], -1f, 1f);
        float shotPower = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.shotPower], -1f, 1f);

        bool jump = actionBuffers.DiscreteActions[(int)PlayerDiscreteAction.jump] > 0;
        bool shoot = actionBuffers.DiscreteActions[(int)PlayerDiscreteAction.shoot] > 0;

        if (!heuristic)
        {
            lookX = ConvertExponent(lookX, 3f);
            lookY = ConvertExponent(lookY, 3f);
        }

        Vector3 playerPosition = playerMovement.transform.position;

        // Move and look
        playerMovement.OnMove(new Vector2(moveX, moveZ));
        playerMovement.OnLook(new Vector2(lookX, lookY));

        // Jumping
        if (jump)
        {
            playerMovement.OnJump();
            AddReward(penaltyJump);
            logger.OnAction(penaltyJump);
        }

        // Bow shooting
        if (shoot)
        {
            float power;
            if (!heuristic)
                power = shotPower;
            else
            {
                float drawTime = Time.fixedTime - drawStartTime;
                power = Math.Min(drawTime, maxDrawingTime) / maxDrawingTime;
            }
            power = ConvertSqrt(power);
            playerShooting.Shoot(power);
            AddReward(penaltyArrow);
            logger.OnAction(penaltyArrow);
            logger.OnShotFired();
        }

        // Enemy proximity
        Transform nearest = GetNearestEnemy(playerPosition);
        if (nearest != null && Vector3.Distance(nearest.position, playerPosition) < dangerDistance)
            OnDetected();

        // Goal proximity
        double distanceToGoal = DistanceBetween(playerMovement.transform, goal, 1f);
        float distanceReward = (float)(previousDistance - distanceToGoal) * distanceToGoalReward;
        AddReward(distanceReward);
        logger.OnAction(distanceReward);
        previousDistance = distanceToGoal;

        // Lose conditions and existence penalty
        if (playerPosition.y < killY)
            Lose(dieReward);
        else if (StepCount == MaxStep - 1)
            Lose(timeoutReward);
        else
            OnExistence();
    }

    public override void OnEpisodeBegin()
    {
        logger.OnEpisodeBegin();
        Vector3 start = GenerateRandomPosition();
        Vector3 goalStart;
        if (divideSpawns)
        {
            bool startIsBefore = IsBeforeDivider(start);
            goalStart = GenerateRandomPosition((newPos) => IsBeforeDivider(newPos) != startIsBefore);
        }
        else
            goalStart = GenerateRandomPosition();

        start.y = terrain.SampleHeight(start) + terrain.transform.position.y + 0.5f;
        goalStart.y = terrain.SampleHeight(goalStart) + terrain.transform.position.y + 0.5f;
        print(start);

        playerMovement.MoveTo(start);
        goal.transform.position = goalStart;
        List<Transform> enemies = enemyManager.GetEnemies(); // TODO: replace with proper enemy spawning
        foreach (Transform enemy in enemies)
        {
            Vector3 enemyStart = GenerateRandomPosition();
            enemyStart.y = terrain.SampleHeight(enemyStart) + terrain.transform.position.y + 0.5f;
            enemy.gameObject.SetActive(true);
            enemy.position = enemyStart;
            enemy.GetComponent<EnemyAI>().ResetValues();
        }

        trainingReferences.GetAllArrows().ForEach(Destroy);
        targetsHit.Clear();
        trainingReferences.GetAllTargets().ForEach(target => target.ResetHitState());
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[(int)PlayerContinuousAction.moveZ] = horizontal.y; // Z axis
        continuousActionsOut[(int)PlayerContinuousAction.moveX] = horizontal.x; // X axis
        continuousActionsOut[(int)PlayerContinuousAction.lookX] = look.x * heuristicLookSensitivity;
        continuousActionsOut[(int)PlayerContinuousAction.lookZ] = look.y * heuristicLookSensitivity;
        continuousActionsOut[(int)PlayerContinuousAction.shotPower] = 0;

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[(int)PlayerDiscreteAction.jump] = jumped ? 1 : 0;
        discreteActionsOut[(int)PlayerDiscreteAction.shoot] = shot ? 1 : 0;
        jumped = false;
        shot = false;
    }

    public void Win()
    {
        AddReward(winReward);
        successIndicator.material.color = Color.softGreen;
        logger.OnAction(winReward);
        logger.OnEpisodeEnd(true);
        EndEpisode();
    }

    public void Lose(float reward)
    {
        AddReward(reward);
        successIndicator.material.color = Color.softRed;
        logger.OnAction(reward);
        logger.OnEpisodeEnd(false);
        EndEpisode();
    }

    public void OnDetected()
    {
        AddReward(penaltyNearEnemy);
        logger.OnAction(penaltyNearEnemy);
        logger.OnDetected();
    }

    public void OnExistence()
    {
        AddReward(existenceReward);
        logger.OnAction(existenceReward);
    }

    public void OnTouchedWater()
    {
        Lose(riverDieReward);
    }

    public void OnTakeDamage()
    {
        AddReward(damageReward);
        logger.OnAction(damageReward);
    }

    public void OnHitTarget(int targetID)
    {
        if (targetsHit.Contains(targetID))
            return;
        targetsHit.Add(targetID);
        if (targetsHit.Count == totalTargets)
        {
            AddReward(hitAllTargetsReward);
            logger.OnAction(hitAllTargetsReward);
            logger.OnHitAllTargets();
        }

        AddReward(hitTargetReward);
        logger.OnAction(hitTargetReward);
        logger.OnHitTarget();
    }

    public void OnKilledEnemy()
    {
        AddReward(killedEnemyReward);
        logger.OnAction(killedEnemyReward);
        logger.OnEnemyKilled();
    }

    public void OnHitEnemy()
    {
        AddReward(hitEnemyReward);
        logger.OnAction(hitEnemyReward);
        logger.OnEnemyHit();
    }

    #region InputActions
    public void OnMove(InputValue value)
    {
        if (!heuristic) return;
        horizontal = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!heuristic) return;
        jumped = value.Get<float>() > 0.5;
    }

    public void OnLook(InputValue value)
    {
        if (!heuristic) return;
        look = value.Get<Vector2>();
    }

    public void OnShootPress(InputValue value)
    {
        if (!heuristic) return;
        shot = false;
        drawStartTime = Time.fixedTime;
    }
    public void OnShootRelease(InputValue value)
    {
        if (!heuristic) return;
        shot = true;
    }
    #endregion


    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.darkRed;
        Vector3 position = new Vector3(playerMovement.transform.position.x, killY, playerMovement.transform.position.z);
        Gizmos.DrawWireCube(position - Vector3.up*0.05f, new Vector3(50, 0.1f, 50));
        Gizmos.DrawLine(position + new Vector3(-25, 0, -25), position + new Vector3(25, 0, 25));
        Gizmos.DrawLine(position + new Vector3(-25, 0, 25), position + new Vector3(25, 0, -25));

        Gizmos.color = Color.green;
        position = new Vector3(playerMovement.transform.position.x, minSpawnY, playerMovement.transform.position.z);
        Gizmos.DrawWireCube(position - Vector3.up * 0.05f, new Vector3(50, 0.1f, 50));
        Gizmos.DrawLine(position + new Vector3(-25, 0, -25), position + new Vector3(25, 0, 25));
        Gizmos.DrawLine(position + new Vector3(-25, 0, 25), position + new Vector3(25, 0, -25));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((minStart + maxStart) / 2, maxStart - minStart);

        Gizmos.color = Color.darkGreen;
        Vector3 divider = Vector3.forward * 100f;
        Vector3 offset = Vector3.right * horizontalDivider;
        divider = Quaternion.Euler(0, dividerAngle, 0) * divider;
        offset = Quaternion.Euler(0, dividerAngle, 0) * offset;
        Vector3 newCenter = transform.position + Vector3.up * gizmoDividerY + offset;
        Gizmos.DrawLine(newCenter - divider / 2, newCenter + divider / 2);
    }

    #region Utility
    private Transform GetNearestEnemy(Vector3 agentPos)
    {
        if (enemyManager.GetEnemies().Count == 0)
            return null;
        return enemyManager.GetEnemies()
            .OrderBy(e => Vector3.Distance(e.position, agentPos))
            .First();
    }

    private Transform GetNearestUnhitTarget(Vector3 agentPos)
    {
        if (trainingReferences.GetAllTargets().Count == 0)
            return null;
        Target result = trainingReferences.GetAllTargets()
            .Where(t => !t.IsHit)
            .OrderBy(t => Vector3.Distance(t.transform.position, agentPos))
            .FirstOrDefault();
        return result == null ? null : result.transform;
    }

    Vector3 GenerateRandomPosition(Func<Vector3, bool> condition = null)
    {
        Vector3 result;
        int attempts = 1000;
        do
        {
            result = new(
                Random.Range(minStart.x, maxStart.x),
                Random.Range(minStart.y, maxStart.y),
                Random.Range(minStart.z, maxStart.z));
            attempts--;
        } while (attempts > 0 && (terrain.SampleHeight(result) + terrain.transform.position.y <= minSpawnY || (condition != null && !condition.Invoke(result))));
        return result;
    }

    bool IsBeforeDivider(Vector3 position)
    {
        Vector3 divider = Vector3.forward * 1f;
        Vector3 offset = Vector3.right * horizontalDivider;
        divider = Quaternion.Euler(0, dividerAngle, 0) * divider;
        offset = Quaternion.Euler(0, dividerAngle, 0) * offset;

        return Vector3.Cross(divider, position - offset - transform.position).y > 0;
    }

    bool IsWithingTerrainBoundingBox(Vector3 position)
    {
        if (terrain == null)
            return false;
        Vector3 min = terrain.GetPosition() + terrain.terrainData.bounds.min;
        Vector3 max = terrain.GetPosition() + terrain.terrainData.bounds.max;
        return !(position.x < min.x) && !(position.x > max.x) && !(position.y < min.y) && !(position.y > max.y);
    }    

    float ConvertSqrt(float x)
    {
        return ConvertExponent(x, 0.5f);
    }

    float ConvertExponent(float x, float exp)
    {
        return Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), exp);
    }

    float AngleBetweenHorizontal(Transform player, Transform target, float defaultValue)
    {
        if (player == null || target == null)
            return defaultValue;

        Vector3 toTarget = target.position - player.position;
        toTarget.y = 0f;
        
        float angle = Vector3.SignedAngle(player.forward, toTarget, Vector3.up);
        return angle / 180f; // normalize to [-1,1]
    }
    float AngleBetweenVertical(Transform player, Transform target, float defaultValue)
    {
        if (player == null || target == null)
            return defaultValue;

        Vector3 toTarget = target.position - player.position;
        Vector3 flat = new Vector3(toTarget.x, 0, toTarget.z);
        Vector3 left = new Vector3(-toTarget.z, 0, toTarget.x);

        if (flat.sqrMagnitude < 0.0001f)
            return target.position.y > player.position.y ? 1f : -1f;

        float angle = Vector3.SignedAngle(flat, toTarget, left);
        return Mathf.Clamp(angle / 90f, -1f, 1f); // normalize to [-1,1]
    }
    float DistanceBetween(Transform player, Transform target, float defaultValue)
    {
        if (player == null || target == null)
            return defaultValue;
        float rawDistance = Vector3.Distance(player.position, target.position);
        return (float)Math.Tanh(rawDistance / meaningfulDistance); // normalize to [0, 1]
    }
    #endregion
}
