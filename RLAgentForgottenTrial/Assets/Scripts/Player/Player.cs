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

    public void TakeDamage() => agent.OnTakeDamage();

    public void Win() => agent.Win();

    public void Checkpoint(int instanceID) => agent.Checkpoint(instanceID);

    public void TouchedWater() => agent.OnTouchedWater();

    public void KilledEnemy() => agent.OnKilledEnemy();

    public void HitEnemy() => agent.OnHitEnemy();

    public void HitTarget(int targetID) => agent.OnHitTarget(targetID);
}
