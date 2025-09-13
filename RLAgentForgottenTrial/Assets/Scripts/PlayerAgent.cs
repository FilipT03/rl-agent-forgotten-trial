using System;
using System.Linq;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
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

    [Header("Options")]
    [SerializeField] private bool heuristic = false;
    [SerializeField] private float heuristicLookSensitivity = 0.0005f;

    [Header("Boundaries")]
    [SerializeField] private float killY = -10f;
    [SerializeField] private Vector3 minStart, maxStart;

    [Header("Rewards")]
    [SerializeField] private float winReward;
    [SerializeField] private float dieReward, existenceReward, timeoutReward;
    [SerializeField] private float penaltyNearEnemy = -0.005f;
    [SerializeField] private float dangerDistance = 3f;

    private Vector2 horizontal, look;
    private bool jumped;

    public override void Initialize()
    {
        minStart += transform.position;
        maxStart += transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (sensor == null) return;
        
        Vector3 agentPos = playerMovement.transform.position;
        sensor.AddObservation(agentPos);

        if (goal != null)
        {
            Vector3 goalPos = goal.position;
            sensor.AddObservation(goalPos - agentPos);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
        }

        if (enemyManager != null && enemyManager.GetEnemies().Count > 0)
        {
            Transform nearestEnemy = GetNearestEnemy(agentPos);
            Vector3 enemyOffset = nearestEnemy.position - agentPos;
            float enemyDistance = enemyOffset.magnitude;

            sensor.AddObservation(enemyOffset);
            sensor.AddObservation(enemyDistance);
        }
        else
        {
            sensor.AddObservation(Vector3.zero);
            sensor.AddObservation(0f);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float moveX = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerAction.moveX], -1f, 1f);
        float moveZ = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerAction.moveZ], -1f, 1f);
        float lookX = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerAction.lookX], -1f, 1f);
        float lookY = Mathf.Clamp(actionBuffers.ContinuousActions[(int)PlayerAction.lookZ], -1f, 1f);

        Vector3 playerPosition = playerMovement.transform.position;

        if(!heuristic)
        {
            lookX = ConvertExponent(lookX, 3f);
            lookY = ConvertExponent(lookY, 3f);
        }

        playerMovement.OnMove(new Vector2(moveX, moveZ));
        playerMovement.OnLook(new Vector2(lookX, lookY));

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
        Vector3 start = new(
            Random.Range(minStart.x, maxStart.x),
            Random.Range(minStart.y, maxStart.y),
            Random.Range(minStart.z, maxStart.z));

        Vector3 goalStart = new(
            Random.Range(minStart.x, maxStart.x),
            Random.Range(minStart.y, maxStart.y),
            Random.Range(minStart.z, maxStart.z));
        
        start.y = terrain.SampleHeight(start) + 0.5f;
        goalStart.y = terrain.SampleHeight(goalStart) + 0.5f;

        playerMovement.MoveTo(start);
        goal.transform.position = goalStart;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = horizontal.y; // Z axis
        continuousActionsOut[1] = horizontal.x; // X axis
        continuousActionsOut[2] = look.x * heuristicLookSensitivity;
        continuousActionsOut[3] = look.y * heuristicLookSensitivity;
    }
     
    public void OnMove(InputValue value)
    {
        if (!heuristic) return;
        horizontal = value.Get<Vector2>();
    }

    public void OnJump(InputValue value)
    {
        if (!heuristic) return;
        jumped = false;
    }

    public void OnLook(InputValue value)
    {
        if (!heuristic) return;
        look = value.Get<Vector2>();
    }

    public void OnDrawGizmos()
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
        terrain.materialTemplate.color = Color.softGreen;
        EndEpisode();
    }

    public void OnLose(float reward)
    {
        SetReward(reward);
        terrain.materialTemplate.color = Color.softRed;
        EndEpisode();
    }
 
    private Transform GetNearestEnemy(Vector3 agentPos)
    {
        return enemyManager.GetEnemies()
            .OrderBy(e => Vector3.Distance(e.position, agentPos))
            .First();
    }

    float ConvertSqrt(float x)
    {
        return ConvertExponent(x, 0.5f);
    }

    float ConvertExponent(float x, float exp)
    {
        return Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), exp);
    }
}
