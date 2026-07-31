using System.Collections;
using UnityEngine;

public class BulletProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 2f;
    [SerializeField] private int damage = 5;
    [SerializeField] private float accuracy = 0.95f;

    private Vector3 moveDirection;
    private Rigidbody rb;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        rb.linearVelocity = moveDirection * speed;

        if (moveDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(moveDirection);
        }
    }

    public void Initialize(Transform target)
    {
        Vector3 baseDirection = (target.position - transform.position).normalized;
        moveDirection = ApplyAccuracyOffset(baseDirection);
    }

    private Vector3 ApplyAccuracyOffset(Vector3 originalDirection)
    {
        float offsetRange = 1 - accuracy;
        return Quaternion.Euler(
            Random.Range(-offsetRange, offsetRange) * 10,
            Random.Range(-offsetRange, offsetRange) * 10,
            0
        ) * originalDirection;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            PlayerScript playerHealth = other.gameObject.GetComponent<PlayerScript>();
            if (playerHealth != null)
            {
                //Debug.Log("BulletProjectile.OnCollisionEnter player hit");
                playerHealth.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

}
