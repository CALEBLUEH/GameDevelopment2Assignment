using UnityEngine;
using UnityEngine.AI;

public class Soldier : MonoBehaviour
{
    [Header("Navigation Settings")]
    [SerializeField] private float patrolRadius = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Attack Settings")]
    public float attackRange = 15f;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private Transform firePoint;

    [Header("Target Detection")]
    [SerializeField] private float detectionRange = 15f;
    [SerializeField] private float targetRefreshInterval = 0.5f;
    [SerializeField] private string targetTag = "Player";

    [Header("Dodge Settings")]
    [Tooltip("0 = never dodges, 1 = always dodges incoming bullets.")]
    [Range(0f, 1f)]
    [SerializeField] private float dodgeChance = 0.5f;
    [Tooltip("How far the soldier strafes when dodging.")]
    [SerializeField] private float dodgeDistance = 3f;
    [Tooltip("Movement speed during a dodge.")]
    [SerializeField] private float dodgeSpeed = 8f;
    [Tooltip("Minimum time between dodge attempts.")]
    [SerializeField] private float dodgeCooldown = 2f;
    [Tooltip("How close a bullet must be to trigger a dodge check.")]
    [SerializeField] private float bulletDetectionRadius = 8f;
    [Tooltip("Max angle between bullet direction and soldier for it to count as 'incoming'.")]
    [SerializeField] private float bulletAngleThreshold = 30f;

    private float lastTargetScanTime;
    [HideInInspector] public Transform target;
    private float lastAttackTime;
    private float lastDodgeTime = -Mathf.Infinity;
    private bool isAlerted;

    // Public accessors for states
    public float DodgeDistance => dodgeDistance;
    public float DodgeSpeed => dodgeSpeed;
    public float DefaultMoveSpeed => moveSpeed;
    public float ChaseSpeed => chaseSpeed;
    public bool IsAlerted => isAlerted;

    // Components
    public Animator Anim { get; private set; }
    public NavMeshAgent Agent { get; private set; }

    // State Machine
    public SoldierStateMachine soldierStateMachine { get; private set; }
    public SoldierIdleState IdleState { get; private set; }
    public SoldierWalkState WalkState { get; private set; }
    public SoldierFiringState FiringState { get; private set; }
    public SoldierDodgeState DodgeState { get; private set; }

    private void Awake()
    {
        Anim = GetComponent<Animator>();
        Agent = GetComponent<NavMeshAgent>();

        // Initialize states
        soldierStateMachine = new SoldierStateMachine();
        IdleState = new SoldierIdleState(this, soldierStateMachine, "Idle");
        WalkState = new SoldierWalkState(this, soldierStateMachine, "Walk");
        FiringState = new SoldierFiringState(this, soldierStateMachine, "Firing");
        DodgeState = new SoldierDodgeState(this, soldierStateMachine, "Walk");
    }

    void Start()
    {
        Agent.updateRotation = false;
        Agent.updateUpAxis = false;
        Agent.speed = moveSpeed;

        soldierStateMachine.InitializeState(IdleState);
    }

    void Update()
    {
        soldierStateMachine.CurrentState.LogicalUpdate();
        ScanForTargets();
    }

    void FixedUpdate()
    {
        soldierStateMachine.CurrentState.PhysicsUpdate();
    }

    /// <summary>
    /// Returns a random position on the NavMesh within the patrol radius.
    /// </summary>
    public Vector3 GetRandomNavMeshPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;

        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas);

        return hit.position;
    }

    /// <summary>
    /// Smoothly rotates the soldier toward the given direction.
    /// </summary>
    public void HandleRotation(Vector3 direction)
    {
        if (direction == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }

    private void ScanForTargets()
    {
        if (Time.time - lastTargetScanTime < targetRefreshInterval) return;
        lastTargetScanTime = Time.time;

        // If alerted, keep tracking the player regardless of distance
        if (isAlerted)
        {
            if (target == null || !target.gameObject.activeInHierarchy)
            {
                // Player died or was destroyed, drop aggro
                target = null;
                isAlerted = false;
                if (soldierStateMachine.CurrentState is SoldierFiringState)
                {
                    soldierStateMachine.ChangeState(IdleState);
                }
            }
            return;
        }

        // Normal detection within range
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRange);
        Transform bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (Collider hit in hits)
        {
            if (!IsValidTarget(hit)) continue;

            float distance = Vector3.Distance(transform.position, hit.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                bestTarget = hit.transform;
            }
        }

        target = bestTarget;

        if (target != null)
        {
            // Player entered detection range, become alerted
            isAlerted = true;

            if (!(soldierStateMachine.CurrentState is SoldierFiringState)
                && !(soldierStateMachine.CurrentState is SoldierDodgeState))
            {
                soldierStateMachine.ChangeState(FiringState);
            }
        }
    }

    private bool IsValidTarget(Collider collider)
    {
        return collider.CompareTag(targetTag) && collider.gameObject.activeInHierarchy;
    }

    /// <summary>
    /// Fires a bullet at the current target if the attack cooldown has elapsed.
    /// </summary>
    public void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            ShootBullet();
            lastAttackTime = Time.time;
        }
    }

    private void ShootBullet()
    {
        if (target != null && bulletPrefab != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            BulletProjectile projectile = bullet.GetComponent<BulletProjectile>();

            if (projectile != null)
            {
                projectile.Initialize(target);
            }
        }
    }

    /// <summary>
    /// Checks for incoming player bullets and attempts a dodge based on dodgeChance.
    /// Returns true if a dodge was triggered.
    /// </summary>
    public bool TryDodge()
    {
        if (dodgeChance <= 0f) return false;
        if (Time.time - lastDodgeTime < dodgeCooldown) return false;

        if (!DetectIncomingBullet()) return false;

        // Roll the dice
        if (Random.value > dodgeChance) return false;

        lastDodgeTime = Time.time;
        soldierStateMachine.ChangeState(DodgeState);
        return true;
    }

    private bool DetectIncomingBullet()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, bulletDetectionRadius);

        foreach (Collider col in nearby)
        {
            PlayerBullet bullet = col.GetComponent<PlayerBullet>();
            if (bullet == null) continue;

            Rigidbody bulletRb = col.GetComponent<Rigidbody>();
            if (bulletRb == null) continue;

            Vector3 bulletVelocity = bulletRb.linearVelocity;
            if (bulletVelocity.sqrMagnitude < 0.1f) continue;

            // Check if the bullet is heading toward this soldier
            Vector3 toSoldier = (transform.position - col.transform.position).normalized;
            float angle = Vector3.Angle(bulletVelocity.normalized, toSoldier);

            if (angle < bulletAngleThreshold)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Forces the soldier to acquire the player as a target and engage immediately.
    /// Called when the soldier takes damage from outside its detection range.
    /// </summary>
    public void AlertToAttacker()
    {
        if (isAlerted) return;

        GameObject player = GameObject.FindGameObjectWithTag(targetTag);
        if (player == null) return;

        target = player.transform;
        isAlerted = true;

        if (!(soldierStateMachine.CurrentState is SoldierFiringState)
            && !(soldierStateMachine.CurrentState is SoldierDodgeState))
        {
            soldierStateMachine.ChangeState(FiringState);
        }
    }

    /// <summary>
    /// Returns the distance to the current target, or infinity if no target.
    /// </summary>
    public float GetDistanceToTarget()
    {
        if (target == null) return Mathf.Infinity;
        return Vector3.Distance(transform.position, target.position);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, bulletDetectionRadius);
    }
}
