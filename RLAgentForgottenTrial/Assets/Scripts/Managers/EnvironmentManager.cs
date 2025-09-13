using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [SerializeField] private Terrain terrain;
    [SerializeField] private Transform goal;

    private void Awake() => Instance = this;

    public void ResetEnvironment(PlayerAgent agent)
    {
        Vector3 start = GenerateRandomTerrainPosition();
        Vector3 goalStart = GenerateRandomTerrainPosition();
        
        
        start.y = terrain.SampleHeight(start) + 0.5f;
        goalStart.y = terrain.SampleHeight(goalStart) + 0.5f;


        goal.transform.position = goalStart;
    }


    private Vector3 GenerateRandomTerrainPosition()
    {
        float x = Random.Range(0, terrain.terrainData.size.x);
        float z = Random.Range(0, terrain.terrainData.size.z);
        float y = terrain.SampleHeight(new Vector3(x, 0, z)) + 0.5f;
        return new Vector3(x, y, z);
    }
}
