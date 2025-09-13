using System.Collections.Generic;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    [SerializeField] private List<Transform> enemies = new();

    public List<Transform> GetEnemies()
    {
        return enemies;
    }
}
