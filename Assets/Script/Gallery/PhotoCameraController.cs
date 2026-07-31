using UnityEngine;
using System;
using System.Collections;

/// <summary>
/// Handles smooth camera transitions to focus on gallery photos and return to the player view.
/// </summary>
public class PhotoCameraController : MonoBehaviour
{
    private const float DEFAULT_TRANSITION_DURATION = 1.2f;

    [SerializeField] private float transitionDuration = DEFAULT_TRANSITION_DURATION;

    private Transform cameraTransform;
    private Renderer[] playerRenderers;
    private Vector3 savedLocalPosition;
    private Quaternion savedLocalRotation;
    private bool isTransitioning;
    private Coroutine activeCoroutine;

    /// <summary>
    /// Whether a camera transition is currently in progress.
    /// </summary>
    public bool IsTransitioning => isTransitioning;

    private void Start()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            Debug.LogError("[PhotoCameraController] No main Camera found in the scene.");
            return;
        }

        cameraTransform = cam.transform;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRenderers = player.GetComponentsInChildren<Renderer>();
        }
    }

    /// <summary>
    /// Smoothly moves the camera to face the given photo transform.
    /// </summary>
    public void FocusOnPhoto(Transform photo, float viewDistance, Action onComplete)
    {
        if (isTransitioning || cameraTransform == null) return;

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(FocusCoroutine(photo, viewDistance, onComplete));
    }

    /// <summary>
    /// Smoothly returns the camera to the player's original view.
    /// </summary>
    public void ReturnToPlayer(Action onComplete)
    {
        if (isTransitioning || cameraTransform == null) return;

        if (activeCoroutine != null)
            StopCoroutine(activeCoroutine);

        activeCoroutine = StartCoroutine(ReturnCoroutine(onComplete));
    }

    /// <summary>
    /// Immediately snaps the camera back to its saved local position/rotation.
    /// Use when the interaction is interrupted (e.g., player leaves trigger).
    /// </summary>
    public void ForceReturn()
    {
        if (cameraTransform == null) return;

        if (activeCoroutine != null)
        {
            StopCoroutine(activeCoroutine);
            activeCoroutine = null;
        }

        cameraTransform.localPosition = savedLocalPosition;
        cameraTransform.localRotation = savedLocalRotation;
        SetPlayerVisible(true);
        isTransitioning = false;
    }

    private void SetPlayerVisible(bool visible)
    {
        if (playerRenderers == null) return;

        foreach (Renderer renderer in playerRenderers)
        {
            renderer.enabled = visible;
        }
    }

    private IEnumerator FocusCoroutine(Transform photo, float viewDistance, Action onComplete)
    {
        isTransitioning = true;

        savedLocalPosition = cameraTransform.localPosition;
        savedLocalRotation = cameraTransform.localRotation;

        SetPlayerVisible(false);

        // The pictures are rotated (90, 270, 0) so transform.up points outward from the wall
        Vector3 outward = photo.up;
        Vector3 targetPos = photo.position + outward * viewDistance;
        Quaternion targetRot = Quaternion.LookRotation(-outward, Vector3.up);

        yield return SmoothTransition(cameraTransform.position, targetPos,
                                      cameraTransform.rotation, targetRot);

        cameraTransform.position = targetPos;
        cameraTransform.rotation = targetRot;

        isTransitioning = false;
        activeCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator ReturnCoroutine(Action onComplete)
    {
        isTransitioning = true;

        // Calculate the world-space target from saved local values
        Transform parent = cameraTransform.parent;
        Vector3 targetPos = parent != null
            ? parent.TransformPoint(savedLocalPosition)
            : savedLocalPosition;
        Quaternion targetRot = parent != null
            ? parent.rotation * savedLocalRotation
            : savedLocalRotation;

        yield return SmoothTransition(cameraTransform.position, targetPos,
                                      cameraTransform.rotation, targetRot);

        // Snap to exact local values to eliminate floating-point drift
        cameraTransform.localPosition = savedLocalPosition;
        cameraTransform.localRotation = savedLocalRotation;

        SetPlayerVisible(true);

        isTransitioning = false;
        activeCoroutine = null;
        onComplete?.Invoke();
    }

    private IEnumerator SmoothTransition(Vector3 fromPos, Vector3 toPos,
                                          Quaternion fromRot, Quaternion toRot)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / transitionDuration);

            cameraTransform.position = Vector3.Lerp(fromPos, toPos, t);
            cameraTransform.rotation = Quaternion.Slerp(fromRot, toRot, t);

            yield return null;
        }
    }
}
