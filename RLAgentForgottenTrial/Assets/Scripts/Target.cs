using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private GameObject targetCollider;
    public bool IsHit { get; private set; }

    public void OnHit()
    {
        targetCollider.tag = "Uninteractable";
        IsHit = true;
    }

    public void ResetHitState()
    {
        targetCollider.tag = "Target";
        IsHit = false;
    }
}
