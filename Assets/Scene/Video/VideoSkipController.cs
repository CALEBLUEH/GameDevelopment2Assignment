using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Handles the "Hold Space to Skip" video functionality with a circular progress bar.
/// Attach to a GameObject in the Video scene alongside the skip UI.
/// </summary>
public class VideoSkipController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private Image circleProgressBar;
    [SerializeField] private TMP_Text skipText;
    [SerializeField] private CanvasGroup skipUIGroup;

    [Header("Settings")]
    [SerializeField] private float holdDuration = 2.0f;
    [SerializeField] private int targetSceneIndex = 0;
    [SerializeField] private float fadeInDelay = 1.0f;

    private float holdTimer = 0f;
    private bool isSkipping = false;
    private float fadeTimer = 0f;

    private void Start()
    {
        // Configure the circle progress bar as a radial fill
        if (circleProgressBar != null)
        {
            circleProgressBar.type = Image.Type.Filled;
            circleProgressBar.fillMethod = Image.FillMethod.Radial360;
            circleProgressBar.fillOrigin = (int)Image.Origin360.Top;
            circleProgressBar.fillClockwise = true;
            circleProgressBar.fillAmount = 0f;
        }

        if (skipUIGroup != null)
        {
            skipUIGroup.alpha = 0f;
        }
    }

    private void Update()
    {
        if (isSkipping) return;

        // Fade in the skip UI after a short delay
        fadeTimer += Time.unscaledDeltaTime;
        if (fadeTimer > fadeInDelay && skipUIGroup != null && skipUIGroup.alpha < 1f)
        {
            skipUIGroup.alpha = Mathf.MoveTowards(skipUIGroup.alpha, 1f, Time.unscaledDeltaTime * 2f);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            holdTimer += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(holdTimer / holdDuration);

            if (circleProgressBar != null)
            {
                circleProgressBar.fillAmount = progress;
            }

            if (progress >= 1f)
            {
                isSkipping = true;
                SkipVideo();
            }
        }
        else
        {
            // Smoothly reset progress when space is released
            holdTimer = Mathf.MoveTowards(holdTimer, 0f, Time.unscaledDeltaTime * 3f);
            if (circleProgressBar != null)
            {
                circleProgressBar.fillAmount = Mathf.Clamp01(holdTimer / holdDuration);
            }
        }
    }

    /// <summary>
    /// Skips the video and loads the target scene.
    /// </summary>
    private void SkipVideo()
    {
        if (videoPlayer != null)
        {
            videoPlayer.Stop();
        }

        SceneManager.LoadScene(targetSceneIndex);
    }
}
