using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using System.Collections;
using UnityEngine.SceneManagement;

public class GalleryManager : MonoBehaviour
{
    [SerializeField]
    GameObject Return;
    [SerializeField]
    VolumeProfile profile;

    public static bool Cleared1 = false;
    public static bool Cleared2 = false;
    public static bool Cleared3 = false;

    private Volume PostProcessing;
    private Vignette vign;
    private LensDistortion lens;
    private bool once = false;
    private bool postProcessingCached = false;

    // Cloned profile so we don't modify the original asset
    private VolumeProfile runtimeProfile;

    private Level1Script level1Script;
    private Level1Script level2Script;
    private Level1Script level3Script;

    void Start()
    {
        Cleared1 = false;
        Cleared2 = false;
        Cleared3 = false;

        ClearPostProcessingEffects();
        CacheLevelScripts();
    }

    /// <summary>
    /// Resets the PostProcessing Volume to a clean state so no leftover
    /// vignette / lens distortion from the level-entry transition persists.
    /// </summary>
    private void ClearPostProcessingEffects()
    {
        GameObject ppObj = GameObject.FindGameObjectWithTag("PostProcessing");
        if (ppObj == null) return;

        Volume vol = ppObj.GetComponent<Volume>();
        if (vol == null) return;

        // Remove any runtime profile that may have been left behind
        vol.profile = null;
    }

    private void CacheLevelScripts()
    {
        GameObject level1Obj = GameObject.FindGameObjectWithTag("Level1");
        if (level1Obj != null) level1Script = level1Obj.GetComponentInChildren<Level1Script>();

        GameObject level2Obj = GameObject.FindGameObjectWithTag("Level2");
        if (level2Obj != null) level2Script = level2Obj.GetComponentInChildren<Level1Script>();

        GameObject level3Obj = GameObject.FindGameObjectWithTag("Level3");
        if (level3Obj != null) level3Script = level3Obj.GetComponentInChildren<Level1Script>();
    }

    private void CachePostProcessing()
    {
        if (postProcessingCached) return;

        GameObject ppObj = GameObject.FindGameObjectWithTag("PostProcessing");
        if (ppObj == null) return;

        PostProcessing = ppObj.GetComponent<Volume>();

        // Clone the profile so modifications don't persist on the asset
        runtimeProfile = Object.Instantiate(profile);
        PostProcessing.profile = runtimeProfile;
        runtimeProfile.TryGet<Vignette>(out vign);
        runtimeProfile.TryGet<LensDistortion>(out lens);
        postProcessingCached = true;
    }

    void Update()
    {
        if (Level1Manager.Cleared && level1Script != null)
        {
            level1Script.Cleared = true;
            Cleared1 = true;
        }
        if (Level2Manager.Cleared && level2Script != null)
        {
            level2Script.Cleared = true;
            Cleared2 = true;
        }
        if (Level3Manager.Cleared && level3Script != null)
        {
            level3Script.Cleared = true;
            Cleared3 = true;
        }

        if (Cleared1 && Cleared2 && Cleared3)
        {
            CachePostProcessing();

            if (!once)
            {
                vign.intensity.value = 0;
                lens.intensity.value = 0;
                StartCoroutine(LoadScene());
                once = true;
            }

            lens.intensity.value -= Time.deltaTime * 0.2f;
            vign.intensity.value += Time.deltaTime * 0.1f;
        }
    }

    IEnumerator LoadScene()
    {
        yield return new WaitForSeconds(10.0f);

        // Clean up runtime profile
        if (runtimeProfile != null)
        {
            Destroy(runtimeProfile);
        }

        SceneManager.LoadScene(5);
    }
}
