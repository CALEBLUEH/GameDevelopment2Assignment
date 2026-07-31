using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public int damage = 10;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float lifeTime = 1f;

    [Header("VFX Settings")]
    [Tooltip("VFX prefab to spawn when the bullet hits an enemy.")]
    [SerializeField] private GameObject enemyHitVFX;
    [Tooltip("How long the VFX lives before being destroyed.")]
    [SerializeField] private float vfxLifeTime = 2f;

    private Vector3 direction;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void FixedUpdate()
    {
        if (rb != null && direction != Vector3.zero)
        {
            rb.linearVelocity = direction * speed;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }

    /// <summary>
    /// Sets the bullet's direction, speed, and lifetime.
    /// </summary>
    public void Initialize(Vector3 shootDirection, float speed, float lifetime)
    {
        direction = shootDirection.normalized;
        this.speed = speed;
        lifeTime = lifetime;

        if (rb != null)
        {
            rb.linearVelocity = direction * speed;
        }
        else
        {
            Debug.LogError("Rigidbody not initialized");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyStatus enemyHealth = collision.gameObject.GetComponent<EnemyStatus>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(damage);
            }

            SpawnEnemyHitVFX(collision);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Spawns the enemy hit VFX at the collision contact point, oriented along the contact normal.
    /// </summary>
    private void SpawnEnemyHitVFX(Collision collision)
    {
        if (enemyHitVFX == null) return;

        ContactPoint contact = collision.GetContact(0);
        GameObject vfx = Instantiate(enemyHitVFX, contact.point, Quaternion.LookRotation(contact.normal));
        Destroy(vfx, vfxLifeTime);
    }
}
