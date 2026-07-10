using System;
using UnityEngine;

public class TargetCollisionHandler : MonoBehaviour
{
    public static event Action<TargetCollisionHandler> TargetBroken;

    [Header("Target Layers")]
    [SerializeField] private bool switchLayerWhenHit = true;
    [SerializeField] private int activeTargetLayer = 6;
    [SerializeField] private int brokenObstacleLayer = 7;

    private bool isActive = true;
    public bool IsActive => isActive;
    public Vector3 LastBreakPosition { get; private set; }

    public void CollisionBehavior(Vector3 pos)
    {
        if (!isActive) return;

        isActive = false;
        LastBreakPosition = pos;
        TargetBroken?.Invoke(this);
        SwitchToObstacleLayer();
    }

    private void SwitchToObstacleLayer()
    {
        if (!switchLayerWhenHit) return;

        if (gameObject.layer == activeTargetLayer)
        {
            gameObject.layer = brokenObstacleLayer;
        }
    }
}
