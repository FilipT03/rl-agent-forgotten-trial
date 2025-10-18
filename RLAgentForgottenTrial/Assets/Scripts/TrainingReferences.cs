using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TrainingReferences : MonoBehaviour
{
    private List<Target> targets;
    public Player player;

    private void Awake()
    {
        targets = GetComponentsInChildren<Target>().ToList();
    }

    public List<GameObject> GetAllArrows() 
        => GetComponentsInChildren<Arrow>().Select(arrow => arrow.gameObject).ToList();

    public List<Target> GetAllTargets()
        => targets;
}
