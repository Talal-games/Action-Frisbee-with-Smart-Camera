using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollisionHandler : MonoBehaviour
{
    public static event Action<TargetCollisionHandler> TargetBroken;

    public GameObject particleEffectPrefab;
    [SerializeField] LayerMask targetLayer = 6;
    [SerializeField] LayerMask obstacleLayer = 7;
    [SerializeField] Light light;
    bool isActive = true;
    public bool IsActive => isActive;



    public void CollisionBehavior(Vector3 pos)
    {
        if (!isActive) return;

        isActive = false;
        TargetBroken?.Invoke(this);
        InstantiateParticle(pos);
        SwitchToObstacleLayer();
        BreakTarget(pos);
        

    }
    



    private void InstantiateParticle(Vector3 pos)
    {
        Instantiate(particleEffectPrefab, pos, Quaternion.identity);
    }

    private void SwitchToObstacleLayer()
    {
        if (gameObject.layer == targetLayer)
        {
            gameObject.layer = obstacleLayer;
        }
    }

    private void BreakTarget(Vector3 breakPosition)
    {
        PlayBreakSound(breakPosition);

        Rigidbody[] rigidbodies = GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
        light.enabled = false;
        
    }

    private void PlayBreakSound(Vector3 breakPosition)
    {
        if (FrisbeeFmodAudioManager.Instance == null || FrisbeeFmodSoundDirectory.Instance == null) return;

        FrisbeeFmodAudioManager.Instance.PlayOneShot(FrisbeeFmodSoundDirectory.Instance.TargetBreak, breakPosition);
    }

    private void EndLevel()
    {
        throw new NotImplementedException();
    }
}
