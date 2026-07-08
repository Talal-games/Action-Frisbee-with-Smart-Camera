using UnityEngine;

public class DiskFollower : MonoBehaviour
{
    [SerializeField] Transform target;

    void Update()
    {
        transform.position = target.position; 
    }
}
