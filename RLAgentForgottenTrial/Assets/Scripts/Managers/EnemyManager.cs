using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyManager : MonoBehaviour
{
    private List<Transform> enemies = new();

    private void Awake()
    {
        enemies = GetComponentsInChildren<EnemyAI>().Select(enemy => enemy.transform).ToList();
    }

    public List<Transform> GetEnemies()
    {
        return enemies;
    }
}
