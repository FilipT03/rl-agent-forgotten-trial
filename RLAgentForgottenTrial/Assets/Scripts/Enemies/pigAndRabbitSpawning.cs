using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pigAndRabbitSpawning : MonoBehaviour
{
    GameObject[] pigSpots, rabbitSpots;
    public GameObject pigPrefab, rabbitPrefab;
    public Transform player;
    int randId;

    private void Start()
    {
        GameObject[] pigSpots = GameObject.FindGameObjectsWithTag("PigSpot");
        GameObject[] rabbitSpots = GameObject.FindGameObjectsWithTag("RabbitSpot");
    }

    public void SpawnPig()
    {
        randId = Random.Range(0, pigSpots.Length);
        if (Vector3.Distance(player.position, pigSpots[randId].transform.position) >= 60)
            Instantiate(pigPrefab, pigSpots[randId].transform);
        else Invoke(nameof(SpawnPig), 10f);
    }
    public void SpawnRabbit()
    {
        randId = Random.Range(0, rabbitSpots.Length);
        if (Vector3.Distance(player.position, rabbitSpots[randId].transform.position) >= 60)
            Instantiate(rabbitPrefab, rabbitSpots[randId].transform);
        else Invoke(nameof(SpawnRabbit), 10f);
    }




}
