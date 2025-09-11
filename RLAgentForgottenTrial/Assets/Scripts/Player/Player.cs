using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }

    [HideInInspector] public GameObject player;
    public Camera normalCamera;
    public Transform normalCameraPoint;
    public Transform freeCameraPoint;

    private void Awake()
    {
        instance = this;
        player = gameObject;
    }
}
