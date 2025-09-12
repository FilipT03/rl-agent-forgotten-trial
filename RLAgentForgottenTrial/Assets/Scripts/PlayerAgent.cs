using System;
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
    [SerializeField] private Renderer floorRender;
    [SerializeField] private Transform goal;

    [Header("Options")]
    [SerializeField] private bool heuristic = false;
    [SerializeField] private float heuristicLookSensitivity = 0.0005f;

    [Header("Boundaries")]
    [SerializeField] private float killY = -10f;
    [SerializeField] private Vector3 minStart, maxStart;

    [Header("Rewards")]
    [SerializeField] private float winReward;
    [SerializeField] private float dieReward, existenceReward;

    private Vector2 horizontal, look;
    private bool jumped;

    public override void Initialize()
    {
        minStart += transform.position;
        maxStart += transform.position;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        float actionZ = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float actionX = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);
        float lookX = Mathf.Clamp(actionBuffers.ContinuousActions[2], -1f, 1f);
        float lookY = Mathf.Clamp(actionBuffers.ContinuousActions[3], -1f, 1f);

        lookX = ConvertExponent(lookX, 3f);
        lookY = ConvertExponent(lookY, 3f);

        playerMovement.OnMove(new Vector2(actionX, actionZ));
        playerMovement.OnLook(new Vector2(lookX, lookY));

        if (playerMovement.transform.position.y <= killY)
        {
            SetReward(dieReward);
            floorRender.material.color = Color.softRed;
            EndEpisode();
        }
        else
            SetReward(existenceReward);
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
        continuousActionsOut[2] = look.x * heuristicLookSensitivity;
        continuousActionsOut[3] = look.y * heuristicLookSensitivity;
        continuousActionsOut[2] = look.x > 0 ? 0.25f : 0f;
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
        SetReward(winReward);
        floorRender.material.color = Color.softGreen;
        EndEpisode();
    }

    float ConvertSqrt(float x)
    {
        return Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), 0.5f);
    }
    float ConvertExponent(float x, float exp)
    {
        return Mathf.Sign(x) * Mathf.Pow(Mathf.Abs(x), exp);
    }
}
