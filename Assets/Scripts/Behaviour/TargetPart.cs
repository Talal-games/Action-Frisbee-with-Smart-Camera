using UnityEngine;

public class TargetPart : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        TargetCollisionHandler handler = GetComponentInParent<TargetCollisionHandler>();
        if (handler != null && collision.gameObject.GetComponentInParent<FrisbeeCollision>())
        {
            if (collision.contactCount == 0) return;

            ContactPoint contact = collision.contacts[0];
            handler.CollisionBehavior(contact.point);
        }
    }

    private void OnTriggerEnter(Collider collider)
    {
        TargetCollisionHandler handler = GetComponentInParent<TargetCollisionHandler>();
        if (handler != null && collider.gameObject.GetComponentInParent<FrisbeeCollision>())
        {
            handler.CollisionBehavior(transform.position);
        }
    }
}
