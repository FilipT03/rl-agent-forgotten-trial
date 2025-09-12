using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player instance { get; private set; }

    [HideInInspector] public GameObject player;
    public GameObject head;
    public Transform normalCameraParent;
    public Transform freeCameraParent;

    private void Awake()
    {
        instance = this;
        player = gameObject;
        freeCameraParent.gameObject.SetActive(false);
    }
}
