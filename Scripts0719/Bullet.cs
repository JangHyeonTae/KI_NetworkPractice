using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private float bulletSpeed;
    void Start()
    {
        rigidbody.AddForce(transform.forward * bulletSpeed,ForceMode.Impulse);
        Destroy(gameObject,2f);
    }

    public void ApplyLagCompensation(float lag)
    {
        rigidbody.velocity = transform.forward * bulletSpeed;
        rigidbody.position += rigidbody.velocity * lag;
    }


}
