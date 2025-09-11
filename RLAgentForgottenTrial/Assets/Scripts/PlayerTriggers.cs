using UnityEngine;

public class PlayerTriggers : MonoBehaviour
{
    [SerializeField] private PlayerAgent agent;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            agent.Win();
        }
    }
}
