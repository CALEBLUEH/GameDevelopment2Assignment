using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

/// <summary>
/// Handles automatic scene transition when the video finishes or after a timeout.
/// </summary>
public class Holder : MonoBehaviour
{
    private const float FALLBACK_TIMEOUT = 60.0f;
    private const int MAIN_MENU_SCENE_INDEX = 0;

    [SerializeField] private VideoPlayer videoPlayer;

    private void Start()
    {
        if (videoPlayer != null)
        {
            videoPlayer.loopPointReached += OnVideoFinished;
        }

        // Fallback in case video events don't fire
        StartCoroutine(FallbackLoadScene());
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        SceneManager.LoadScene(MAIN_MENU_SCENE_INDEX);
    }

    IEnumerator FallbackLoadScene()
    {
        yield return new WaitForSeconds(FALLBACK_TIMEOUT);
        SceneManager.LoadScene(MAIN_MENU_SCENE_INDEX);
    }
}
