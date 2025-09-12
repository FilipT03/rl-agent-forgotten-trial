using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

using Random = UnityEngine.Random;

public class PlayerAgent : Agent
{
    public bool useVecObs;
    [SerializeField] private Transform goal;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Vector3 minStart, maxStart;
    [SerializeField] private Renderer floorRender;

    [SerializeField] private bool heuristic = false;
    private Vector2 horizontal, look;
    private bool jumped;

    [SerializeField] float killY = -10f;

    public override void Initialize()
    {
        minStart += transform.position;
        maxStart += transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVecObs)
        {
            sensor.AddObservation(playerMovement.transform.rotation.z);
            sensor.AddObservation(playerMovement.transform.rotation.x);
            sensor.AddObservation(playerMovement.transform.position - goal.position);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float actionZ = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float actionX = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        playerMovement.OnMove(new Vector2(actionZ, actionX));

        if (playerMovement.transform.position.y <= killY)
        {
            SetReward(-10f);
            floorRender.material.color = Color.softRed;
            EndEpisode();
        }
        else
            SetReward(-0.01f);
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

        playerMovement.MoveTo(start);

        goal.transform.position = goalStart;
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = horizontal.y; // Z axis
        continuousActionsOut[1] = horizontal.x; // X axis
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

    public void Win()
    {
        SetReward(100f);
        floorRender.material.color = Color.softGreen;
        EndEpisode();
    }
}
