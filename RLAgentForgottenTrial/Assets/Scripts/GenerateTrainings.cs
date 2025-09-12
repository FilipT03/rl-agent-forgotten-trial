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
        for (int i = 0; i < a; i++)
        {
            for (int j = 0; j < b; j++)
            {
                if (i == 0 && j == 0) continue;
                Instantiate(trainingObject, Vector3.forward * i * size + Vector3.right * j * size, Quaternion.identity);
            }
        }
        mainCamera.gameObject.SetActive(true);
    }
}
