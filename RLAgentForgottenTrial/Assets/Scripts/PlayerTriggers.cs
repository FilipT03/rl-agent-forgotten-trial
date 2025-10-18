using UnityEngine;

public class PlayerTriggers : MonoBehaviour
{
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Goal"))
        {
            Checkpoint c = other.GetComponent<Checkpoint>();
            if (c == null || c.isGoal)
                GetComponentInParent<TrainingReferences>().player.Win();
            else
                GetComponentInParent<TrainingReferences>().player.Checkpoint(other.gameObject.GetInstanceID());
        }
        else if (other.CompareTag("Water"))
            GetComponentInParent<TrainingReferences>().player.TouchedWater();
    }
}
