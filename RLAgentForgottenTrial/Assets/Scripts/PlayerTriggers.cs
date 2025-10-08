using UnityEngine;

public class PlayerTriggers : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
            GetComponentInParent<TrainingReferences>().player.Win();
        else if (other.CompareTag("Water"))
            GetComponentInParent<TrainingReferences>().player.TouchedWater();
    }
}
