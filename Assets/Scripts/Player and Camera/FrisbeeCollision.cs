using UnityEngine;

public class FrisbeeCollision : MonoBehaviour
{
    private PlayerController player;
    private void Start()
    {
        player = GetComponentInParent<PlayerController>();
    }
    private void OnCollisionEnter(Collision collision)
    {
        player.HandleFrisbeeCollision(collision.gameObject);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.GetComponentInParent<TargetCollisionHandler>() == null) return;//only send triggers through if theyre from targets
        player.HandleFrisbeeCollision(other.gameObject);
    }
}
