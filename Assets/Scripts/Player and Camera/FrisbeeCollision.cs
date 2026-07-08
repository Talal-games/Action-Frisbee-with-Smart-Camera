using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrisbeeCollision : MonoBehaviour
{
    PlayerStateController player;
    private void Start()
    {
        player = GetComponentInParent<PlayerStateController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        player.OnFrisbeeCollisionEnter(collision.gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<TargetCollisionHandler>() == null) return;//only send triggers through if theyre from targets
        player.OnFrisbeeCollisionEnter(other.gameObject);
    }
}
