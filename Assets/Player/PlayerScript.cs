using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;
public class PlayerScript : MonoBehaviour
{
    private const float GRAVITY_ACCELERATION = -19.6f;
    private float verticalVelocity = 0f;

    private CharacterController CharCon;
    private CinemachineCamera Cam;
    private GameManager gameManager;
    [SerializeField]
    private AudioClip Fire;
    [SerializeField]
    private AudioClip Walk;
    [SerializeField]
    private AudioClip TakenDamage;

    [SerializeField]
    private AudioSource PlayerSource;
    [SerializeField]
    private AudioSource GunSource;
    [SerializeField]
    private AudioSource TakenSource;
    [SerializeField]
    private float MoveSpeed = 2.0f;

    [Header("Camera Settings")]
    public float sensitivityHor = 5.0f;
    public float sensitivityVer = 5.0f;

    public float minimumVer = -45.0f;
    public float maximumVer = 45.0f;

    private float verticalRot = 0.0f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject BulletPrefab;
    [Tooltip("Drag the gun barrel tip GameObject here. Bullets will spawn from its position and fire in its forward direction.")]
    [SerializeField] private Transform BulletSpawnPoint;
    private GunRecoil gunRecoil;
    [SerializeField] private float ShootRange;
    [SerializeField] private float bulletSpeed = 50f;
    [SerializeField] private float bulletLifeTime = 1f;
    [SerializeField] private float fireRateCooldown = 0.25f;
    private float lastFireTime = -Mathf.Infinity;

    [Header("Health Settings")]
    public int maxHealth = 100;
    public float invincibleDuration = 1f;

    public int currentHealth;
    private bool isInvincible = false;
    private Coroutine invincibleCoroutine;

    private bool IsDashing = false;

    public static bool IsFreezed = false;
    public static bool IsPaused = false;
    private void Awake()
    {
        IsFreezed = false;
        IsPaused = false;

        CharCon = GetComponent<CharacterController>();
        Cam = GetComponentInChildren<CinemachineCamera>();
        gunRecoil = GetComponentInChildren<GunRecoil>();
        gameManager = FindAnyObjectByType <GameManager>();
    }

    private void Start()
    {
        currentHealth = maxHealth;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

    }
    // Update is called once per frame
    void Update()
    {
        GunSource.volume = VolumeHolder.SFXVolume;
        PlayerSource.volume = VolumeHolder.SFXVolume;
        TakenSource.volume = VolumeHolder.SFXVolume;

        if(IsPaused)
        {
            CharCon.Move(new Vector3(0, GRAVITY_ACCELERATION * Time.deltaTime, 0));
            return;
        }

        if (SceneManager.GetActiveScene().name == "Gallery")
            MoveSpeed = 8.0f;
        if (IsFreezed)
        {
            CharCon.Move(new Vector3(0, GRAVITY_ACCELERATION * Time.deltaTime, 0));
            return;
        }

        CameraRotation();

        CheckDashing();

        PlayerMovement();

        if (SceneManager.GetActiveScene().name == "Gallery")
        {
            return;
        }

        FirstPersonShooting();
    }

    private void FirstPersonShooting()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        if (Time.time < lastFireTime + fireRateCooldown) return;

        lastFireTime = Time.time;
        GunSource.PlayOneShot(Fire);

        if (gunRecoil != null)
        {
            gunRecoil.Play();
        }

        Vector3 shootPosition;
        Vector3 shootDirection;

        if (BulletSpawnPoint != null)
        {
            shootPosition = BulletSpawnPoint.position;
            shootDirection = Cam.transform.forward;
        }
        else
        {
            shootPosition = Cam.transform.position + Cam.transform.forward * 1.5f;
            shootDirection = Cam.transform.forward;
        }

        GameObject bullet = Instantiate(BulletPrefab, shootPosition, Quaternion.identity);
        bullet.transform.forward = shootDirection;

        PlayerBullet projectile = bullet.GetComponent<PlayerBullet>();
        if (projectile != null)
        {
            projectile.Initialize(shootDirection, bulletSpeed, bulletLifeTime);
        }
    }

    private IEnumerator CreateHitPoint(Vector3 Position)
    {
        GameObject Bullet = Instantiate(BulletPrefab, Position,Quaternion.identity);


        yield return new WaitForSeconds(1.0f);

        Destroy(Bullet);
    }

    private void CheckDashing()
    {
        if(Input.GetKeyDown(KeyCode.LeftShift))
        {
            IsDashing = true;
        }
        if(Input.GetKeyUp(KeyCode.LeftShift))
        {
            IsDashing = false;
        }
    }

    private void PlayerMovement()
    {
        if (CharCon.isGrounded)
        {
            // Small downward force to keep grounded on slopes
            verticalVelocity = -2f;
        }
        else
        {
            verticalVelocity += GRAVITY_ACCELERATION * Time.deltaTime;
        }

        float deltaX = Input.GetAxis("Horizontal") * MoveSpeed;
        float deltaZ = Input.GetAxis("Vertical") * MoveSpeed;
        Vector3 movement = new Vector3(deltaX, 0, deltaZ);
        movement = transform.TransformDirection(movement);
        movement = Vector3.ClampMagnitude(movement, MoveSpeed);
        if (deltaX != 0 || deltaZ != 0)
        {
            if (!PlayerSource.isPlaying)
                PlayerSource.PlayOneShot(Walk);
        }
        else
        {
            PlayerSource.Stop();
        }
        movement.y = verticalVelocity;
        movement *= Time.deltaTime;

        if(IsDashing)
        {
            movement *= 1.5f;
        }
        CharCon.Move(movement);
    }

    private void CameraRotation()
    {
        //both rotation here
        verticalRot -= Input.GetAxis("Mouse Y") * sensitivityVer;
        verticalRot = Mathf.Clamp(verticalRot, minimumVer, maximumVer);

        float delta = Input.GetAxis("Mouse X") * sensitivityHor;
        float horizontalRot = transform.localEulerAngles.y + delta;

        transform.localEulerAngles = new Vector3(verticalRot, horizontalRot, 0);
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible || currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        //Debug.Log($"Player took {damage} damage! Current health: {currentHealth}");
        if(!TakenSource.isPlaying)
        TakenSource.PlayOneShot(TakenDamage);
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            if (invincibleCoroutine != null) StopCoroutine(invincibleCoroutine);
            invincibleCoroutine = StartCoroutine(InvincibleTimer());
        }
    }

    private IEnumerator InvincibleTimer()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibleDuration);
        isInvincible = false;
    }

    private void Die()
    {
        Debug.Log("Player has died!");

        enabled = false;
        CharCon.enabled = false;
        gameManager.Defeat();
    }
}
