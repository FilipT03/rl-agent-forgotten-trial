using System;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

public class PlayerAgent : Agent
{
    [Header("References")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Terrain terrain;
    [SerializeField] private Transform goal;
    [SerializeField] private EnemyManager enemyManager;
    [SerializeField] private Renderer successIndicator;

    [Header("Options")]
    [SerializeField] private bool heuristic = false;
    [SerializeField] private float heuristicLookSensitivity = 0.0005f;
    [SerializeField] private float meaningfulDistance = 15f;

    [Header("Boundaries")]
    [SerializeField] private float killY = -10f;
    [SerializeField] private Vector3 minStart, maxStart;

    [Header("Rewards")]
    [SerializeField] private float winReward;
    [SerializeField] private float dieReward, existenceReward, timeoutReward, damageReward = -0.05f;
    [SerializeField] private float penaltyNearEnemy = -0.005f;
    [SerializeField] private float dangerDistance = 3f;

    private Vector2 horizontal, look;
    private bool jumped;

    public override void Initialize()
    {
        minStart += transform.position;
        maxStart += transform.position;
    }

    // goalAngle, goalDistance, enemyExistsFlag, closestEnemyAngle, closestEnemyDistance  =  5 
    public override void CollectObservations(VectorSensor sensor)
    {
        if (sensor == null) return;

        Transform player = playerMovement.transform;

        sensor.AddObservation(AngleBetween(player, goal, 0f));    // [-1,1]
        sensor.AddObservation(DistanceBetween(player, goal, 0f)); // [ 0,1]


        Transform nearestEnemy = null;
        if (enemyManager != null && enemyManager.GetEnemies().Count > 0)
            nearestEnemy = GetNearestEnemy(player.position);

        sensor.AddObservation(nearestEnemy == null ? 0 : 1); // does enemy exist
        sensor.AddObservation(AngleBetween(player, nearestEnemy, 0f));    // [-1,1]
        sensor.AddObservation(DistanceBetween(player, nearestEnemy, -1f)); // [0,1] or -1

        sensor.AddObservation(playerMovement.CanJump() ? 1 : 0);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.moveX], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.moveZ], -1f, 1f);
        float lookX = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.lookX], -1f, 1f);
        float lookY = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerContinuousAction.lookZ], -1f, 1f);

        bool jump = actionBuffers.DiscreteActions[(int)PlayerDiscreteAction.jump] > 0;

        Vector3 playerPosition = playerMovement.transform.position;

        if (!heuristic)
        {
            lookX = ConvertExponent(lookX, 3f);
            lookY = ConvertExponent(lookY, 3f);
        }

        playerMovement.OnMove(new Vector2(moveX, moveZ));
        playerMovement.OnLook(new Vector2(lookX, lookY));

        if (jump)
            playerMovement.OnJump();

        Transform nearest = GetNearestEnemy(playerPosition);
        if (Vector3.Distance(nearest.position, playerPosition) < dangerDistance)
            AddReward(penaltyNearEnemy);

        if (playerPosition.y < killY)
            OnLose(dieReward);
        else if (StepCount == MaxStep - 1)
            OnLose(timeoutReward);
        else
            AddReward(existenceReward);

    }

    public override void OnEpisodeBegin()
    {
        Vector3 start = GenerateRandomPosition(), 
            goalStart = GenerateRandomPosition(),
            enemyStart = GenerateRandomPosition();

        start.y = terrain.SampleHeight(start) + 0.5f;
        goalStart.y = terrain.SampleHeight(goalStart) + 0.5f;
        enemyStart.y = terrain.SampleHeight(enemyStart) + 0.5f;

        playerMovement.MoveTo(start);
        goal.transform.position = goalStart;
        GetNearestEnemy(playerMovement.transform.position).position = enemyStart; // TODO: replace with proper enemy spawning
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[(int)PlayerContinuousAction.moveZ] = horizontal.y; // Z axis
        continuousActionsOut[(int)PlayerContinuousAction.moveX] = horizontal.x; // X axis
        continuousActionsOut[(int)PlayerContinuousAction.lookX] = look.x * heuristicLookSensitivity;
        continuousActionsOut[(int)PlayerContinuousAction.lookZ] = look.y * heuristicLookSensitivity;

        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[(int)PlayerDiscreteAction.jump] = jumped ? 1 : 0;
        jumped = false;
    }

    public void OnMove(InputValue value)
    {
        if (!heuristic) return;
        horizontal = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!heuristic) return;
        print(value.Get<float>());
        jumped = value.Get<float>() > 0.5;
    }

    public void OnLook(InputValue value)
    {
        if (!heuristic) return;
        look = value.Get<Vector2>();
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Vector3 position = new Vector3(playerMovement.transform.position.x, killY, playerMovement.transform.position.z);
        Gizmos.DrawWireCube(position, new Vector3(50, 0.1f, 50));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((minStart + maxStart) / 2, maxStart - minStart);
    }

    public void OnWin()
    {
        SetReward(winReward);
        successIndicator.material.color = Color.softGreen;
        EndEpisode();
    }

    public void OnLose(float reward)
    {
        SetReward(reward);
        successIndicator.material.color = Color.softRed;
        EndEpisode();
    }

    public void OnTakeDamage()
    {
        AddReward(damageReward);
    }

    private Transform GetNearestEnemy(Vector3 agentPos)
    {
        return enemyManager.GetEnemies()
            .OrderBy(e => Vector3.Distance(e.position, agentPos))
            .First();
    }
    
    Vector3 GenerateRandomPosition()
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
        } while (terrain.SampleHeight(result) <= killY && attempts > 0);
        return result;
    }

    float ConvertSqrt(float x)
    {
        return ConvertExponent(x, 0.5f);
    }

    float ConvertExponent(float x, float exp)
    {
        return Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), exp);
    }

    float AngleBetween(Transform player, Transform target, float defaultValue)
    {
        if (player == null || target == null)
            return defaultValue;

        Vector3 toTarget = target.position - player.position;
        toTarget.y = 0f;
        
        float angle = Vector3.SignedAngle(player.forward, toTarget, Vector3.up);
        return angle / 180f; // normalize to [-1,1]
    }
    float DistanceBetween(Transform player, Transform target, float defaultValue)
    {
        if (player == null || target == null)
            return defaultValue;
        float rawDistance = Vector3.Distance(player.position, target.position);
        return (float)Math.Tanh(rawDistance / meaningfulDistance); // normalize to [0, 1]
    }
}
