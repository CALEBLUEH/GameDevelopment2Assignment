using UnityEngine;

/// <summary>
/// Applies a procedural recoil effect to the gun when fired.
/// Attach to the gun GameObject and call Play() from the shooting logic.
/// </summary>
public class GunRecoil : MonoBehaviour
{
    [Header("Recoil Kick")]
    [Tooltip("How far the gun kicks backward (local -Z).")]
    [SerializeField] private float kickBackDistance = 0.08f;

    [Tooltip("Upward rotation kick in degrees (local X).")]
    [SerializeField] private float kickUpRotation = 10f;

    [Header("Timing")]
    [Tooltip("How fast the gun snaps to the recoil pose (seconds).")]
    [SerializeField] private float recoilSpeed = 0.05f;

    [Tooltip("How fast the gun returns to its rest pose (seconds).")]
    [SerializeField] private float returnSpeed = 0.15f;

    private Vector3 restLocalPosition;
    private Quaternion restLocalRotation;
    private Vector3 recoilTargetPosition;
    private Quaternion recoilTargetRotation;

    private float recoilTimer;
    private bool isRecoiling;

    private void Awake()
    {
        restLocalPosition = transform.localPosition;
        restLocalRotation = transform.localRotation;
    }

    private void Update()
    {
        if (!isRecoiling) return;

        recoilTimer += Time.deltaTime;

        if (recoilTimer <= recoilSpeed)
        {
            // Snap toward recoil pose
            float t = Mathf.Clamp01(recoilTimer / recoilSpeed);
            transform.localPosition = Vector3.Lerp(restLocalPosition, recoilTargetPosition, t);
            transform.localRotation = Quaternion.Slerp(restLocalRotation, recoilTargetRotation, t);
        }
        else
        {
            // Return to rest pose
            float returnElapsed = recoilTimer - recoilSpeed;
            float t = Mathf.Clamp01(returnElapsed / returnSpeed);
            transform.localPosition = Vector3.Lerp(recoilTargetPosition, restLocalPosition, t);
            transform.localRotation = Quaternion.Slerp(recoilTargetRotation, restLocalRotation, t);

            if (t >= 1f)
            {
                transform.localPosition = restLocalPosition;
                transform.localRotation = restLocalRotation;
                isRecoiling = false;
            }
        }
    }

    /// <summary>
    /// Triggers the recoil effect. Call this each time the gun fires.
    /// </summary>
    public void Play()
    {
        recoilTargetPosition = restLocalPosition + Vector3.back * kickBackDistance;
        recoilTargetRotation = restLocalRotation * Quaternion.Euler(-kickUpRotation, 0f, 0f);

        recoilTimer = 0f;
        isRecoiling = true;
    }
}
