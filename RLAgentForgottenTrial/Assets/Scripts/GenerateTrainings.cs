using UnityEngine;

public class GenerateTrainings : MonoBehaviour
{
    [SerializeField] private GameObject trainingObject;
    [SerializeField] private int a,b;
    [SerializeField] private int size;

    private void Awake()
    {
        Camera mainCamera = trainingObject.GetComponentInChildren<Camera>();
        mainCamera.gameObject.SetActive(false);
        Terrain terrain = trainingObject.GetComponentInChildren<Terrain>();
        Material material = terrain.materialTemplate;
        terrain.materialTemplate = new Material(material);
        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                if (i == 0 && j == 0) continue;
                GameObject obj = Instantiate(trainingObject, Vector3.forward * i * size + Vector3.right * j * size, Quaternion.identity);
                obj.GetComponentInChildren<Terrain>().materialTemplate = new Material(material); 
            }
        }
        mainCamera.gameObject.SetActive(true);
    }
}
