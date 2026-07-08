using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetPart : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        // Find the parent TargetCollisionHandler
        TargetCollisionHandler handler = GetComponentInParent<TargetCollisionHandler>();
        if (handler != null&&collision.gameObject.GetComponentInParent<FrisbeeCollision>())
        {
            if (collision.contactCount == 0) return;
            ContactPoint contact = collision.contacts[0];
            handler.CollisionBehavior(contact.point);
        }
    }
    private void OnTriggerEnter(Collider collider)
    {
        // Find the parent TargetCollisionHandler
        TargetCollisionHandler handler = GetComponentInParent<TargetCollisionHandler>();
        if (handler != null && collider.gameObject.GetComponentInParent<FrisbeeCollision>())
        {
            handler.CollisionBehavior(transform.position);
        }
    }
}
