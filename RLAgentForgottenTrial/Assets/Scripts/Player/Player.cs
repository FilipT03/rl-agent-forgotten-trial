using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [HideInInspector] public GameObject player;
    public GameObject head;
    public Transform normalCameraParent;
    public Transform freeCameraParent;
    private PlayerAgent agent;

    private void Awake()
    {
        player = gameObject;
        freeCameraParent.gameObject.SetActive(false);
    }

    private void Start()
    {
        agent = GetComponentInParent<PlayerAgent>();
    }

    public void TakeDamage()
    {
        agent.OnTakeDamage();
    }

    public void Win()
    {
        agent.OnWin();
    }
}
