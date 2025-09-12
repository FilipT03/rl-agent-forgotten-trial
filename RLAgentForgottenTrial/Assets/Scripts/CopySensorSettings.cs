using Unity.MLAgents.Sensors;
using UnityEngine;

public class CopySensorSettings : MonoBehaviour
{
    [SerializeField] private RayPerceptionSensorComponent3D toCopyFrom;
    [SerializeField] private bool disableOriginal = false;
    [Header("What to copy")]
    [SerializeField] private bool raysPerDirection = true;
    [SerializeField] private bool maxRayDegress = true, 
                                    sphereCastRadius = true, 
                                    rayLength = true, 
                                    rayLayerMask = true, 
                                    observationStacks = true, 
                                    alternatingRayOrder = true, 
                                    useBatchedRaycasts = true, 
                                    detectableTags = true;

    private void Awake()
    {
        RayPerceptionSensorComponent3D[] sensors = GetComponents<RayPerceptionSensorComponent3D>();
        foreach (var sensor in sensors)
        {
            if (sensor.GetInstanceID() == toCopyFrom.GetInstanceID())
                continue;
            if (raysPerDirection)
                sensor.RaysPerDirection = toCopyFrom.RaysPerDirection;
            if (maxRayDegress)
                sensor.MaxRayDegrees = toCopyFrom.MaxRayDegrees;
            if (sphereCastRadius)
                sensor.SphereCastRadius = toCopyFrom.SphereCastRadius;
            if (rayLength)
                sensor.RayLength = toCopyFrom.RayLength;
            if (rayLayerMask)
                sensor.RayLayerMask = toCopyFrom.RayLayerMask;
            if (observationStacks)
                sensor.ObservationStacks = toCopyFrom.ObservationStacks;
            if (alternatingRayOrder)
                sensor.AlternatingRayOrder = toCopyFrom.AlternatingRayOrder;
            if (useBatchedRaycasts)
                sensor.UseBatchedRaycasts = toCopyFrom.UseBatchedRaycasts;
            if (detectableTags)
                sensor.DetectableTags.AddRange(toCopyFrom.DetectableTags);
        }
        if (disableOriginal)
            toCopyFrom.enabled = false;
    }
}
