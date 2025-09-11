using System;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAgent : Agent
{
    //public GameObject ball;
    public bool useVecObs;
    [SerializeField] private Transform goal;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform startPosition;
    [SerializeField] private Vector3 minStart, maxStart;
    [SerializeField] private Renderer floorRender;
    //Rigidbody rb;
    //EnvironmentParameters m_ResetParams;

    [SerializeField] private bool heuristic = false;
    private Vector2 horizontal, look;
    private bool jumped;

    [SerializeField] float killY = -10f;

    public override void Initialize()
    {
        minStart += transform.position;
        maxStart += transform.position;
        //rb = ball.GetComponent<Rigidbody>();
        //m_ResetParams = Academy.Instance.EnvironmentParameters;
        //SetResetParameters();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (useVecObs)
        {
            sensor.AddObservation(playerMovement.transform.rotation.z);
            sensor.AddObservation(playerMovement.transform.rotation.x);
            sensor.AddObservation(playerMovement.transform.position - goal.position);
            //sensor.AddObservation(m_BallRb.linearVelocity);
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //print("Continous " + actionBuffers.ContinuousActions[0]);
        float actionZ = Mathf.Clamp(actionBuffers.ContinuousActions[0], -1f, 1f);
        float actionX = Mathf.Clamp(actionBuffers.ContinuousActions[1], -1f, 1f);

        playerMovement.OnMove(new Vector2(actionZ, actionX));

        //print("Player position: " + playerMovement.transform.position);
        //print("KillY " + killY);
        
        if (playerMovement.transform.position.y <= killY)
        {
            SetReward(-10f);
            floorRender.material.color = Color.softRed;
            EndEpisode();
        } else
            SetReward(-0.01f);

        //if ((gameObject.transform.rotation.z < 0.25f && actionZ > 0f) ||
        //    (gameObject.transform.rotation.z > -0.25f && actionZ < 0f))
        //{
        //    gameObject.transform.Rotate(new Vector3(0, 0, 1), actionZ);
        //}

        //if ((gameObject.transform.rotation.x < 0.25f && actionX > 0f) ||
        //    (gameObject.transform.rotation.x > -0.25f && actionX < 0f))
        //{
        //    gameObject.transform.Rotate(new Vector3(1, 0, 0), actionX);
        //}
        //if ((ball.transform.position.y - gameObject.transform.position.y) < -2f ||
        //    Mathf.Abs(ball.transform.position.x - gameObject.transform.position.x) > 3f ||
        //    Mathf.Abs(ball.transform.position.z - gameObject.transform.position.z) > 3f)
        //{
        //    SetReward(-1f);
        //    EndEpisode();
        //}
        //else
        //{
        //    SetReward(0.1f);
        //}
    }

    public override void OnEpisodeBegin()
    {
        playerMovement.characterController.enabled = false;

        Vector3 start = new Vector3(
            UnityEngine.Random.Range(minStart.x, maxStart.x), 
            UnityEngine.Random.Range(minStart.y, maxStart.y), 
            UnityEngine.Random.Range(minStart.z, maxStart.z));

        Vector3 goalStart = new Vector3(
            UnityEngine.Random.Range(minStart.x, maxStart.x),
            UnityEngine.Random.Range(minStart.y, maxStart.y),
            UnityEngine.Random.Range(minStart.z, maxStart.z));

        playerMovement.transform.position = start;
        playerMovement.characterController.enabled = true;

        goal.transform.position = goalStart;

        //gameObject.transform.rotation = new Quaternion(0f, 0f, 0f, 0f);
        //gameObject.transform.Rotate(new Vector3(1, 0, 0), Random.Range(-10f, 10f));
        //gameObject.transform.Rotate(new Vector3(0, 0, 1), Random.Range(-10f, 10f));
        //m_BallRb.linearVelocity = new Vector3(0f, 0f, 0f);
        //ball.transform.position = new Vector3(Random.Range(-1.5f, 1.5f), 4f, Random.Range(-1.5f, 1.5f))
        //    + gameObject.transform.position;
        //Reset the parameters when the Agent is reset.
        //SetResetParameters();
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
        horizontal = value.Get<Vector2>();
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Vector3 position = new Vector3(playerMovement.transform.position.x, killY, playerMovement.transform.position.z);
        Gizmos.DrawWireCube(position, new Vector3(50, 0.1f, 50));

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube((minStart + maxStart)/2, maxStart - minStart);
    }

    public void Win()
    {
        SetReward(100f);
        floorRender.material.color = Color.softGreen;
        EndEpisode();
    }



    //public void SetBall()
    //{
    //Set the attributes of the ball by fetching the information from the academy
    //m_BallRb.mass = m_ResetParams.GetWithDefault("mass", 1.0f);
    //var scale = m_ResetParams.GetWithDefault("scale", 1.0f);
    //ball.transform.localScale = new Vector3(scale, scale, scale);
    //}

    //public void SetResetParameters()
    //{
    //    SetBall();
    //}
}
