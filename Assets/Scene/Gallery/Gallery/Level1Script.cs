using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.SceneManagement;

public class Level1Script : MonoBehaviour
{
    private static readonly int StrengthProperty = Shader.PropertyToID("Strength");

    public static bool IsTransitioning = false;
    public string LevelName;
    [SerializeField]
    private VolumeProfile profile;

    [SerializeField]
    private AudioSource source;
    [SerializeField]
    private AudioClip clip;

    [SerializeField]
    private TextMeshProUGUI Dialogue;
    private Material mat;
    private float strength;
    private bool Pressed;
    private Volume PostProcessing;
    private Vignette vign;
    private LensDistortion lens;
    private bool postProcessingCached = false;

    // Cloned profile so we don't modify the original asset
    private VolumeProfile runtimeProfile;

    private bool InRange = false;
    private bool once = false;

    public bool Cleared = false;
    public string StringToBeTold;

    private void Awake()
    {
        IsTransitioning = false;
        mat = GetComponent<MeshRenderer>().material;
        // Ensure material starts with no spiral effect
        mat.SetFloat(StrengthProperty, 0f);
    }

    private void Update()
    {
        source.volume = VolumeHolder.SFXVolume;

        // When cleared, only show text when in range -- do NOT blank the shared dialogue every frame
        if (Cleared)
        {
            return;
        }

        if (!InRange)
            return;

        if (!Pressed)
        {
            Dialogue.text = "Press Space Bar to get into the world......";
        }
        else
        {
            Dialogue.text = "";
        }

        CachePostProcessing();

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Pressed = true;
            source.PlayOneShot(clip);
            PlayerScript.IsFreezed = true;
        }

        if (!Pressed)
            return;

        IsTransitioning = true;
        if (!once)
        {
            vign.intensity.value = 0;
            lens.intensity.value = 0;
            StartCoroutine(LoadScene(LevelName));
            once = true;
        }

        strength += Time.deltaTime * 2;
        mat.SetFloat(StrengthProperty, strength);
        lens.intensity.value -= Time.deltaTime * 0.2f;
        vign.intensity.value += Time.deltaTime * 0.1f;
    }

    private void CachePostProcessing()
    {
        if (postProcessingCached) return;

        GameObject ppObj = GameObject.FindGameObjectWithTag("PostProcessing");
        if (ppObj == null) return;

        PostProcessing = ppObj.GetComponent<Volume>();

        // Clone the profile so modifications don't persist on the asset across scene loads
        runtimeProfile = Instantiate(profile);
        PostProcessing.profile = runtimeProfile;
        runtimeProfile.TryGet<Vignette>(out vign);
        runtimeProfile.TryGet<LensDistortion>(out lens);

        // Reset to clean values
        vign.intensity.value = 0;
        lens.intensity.value = 0;

        postProcessingCached = true;
    }

    IEnumerator LoadScene(string levelName)
    {
        yield return new WaitForSeconds(7.5f);
        PlayerScript.IsFreezed = false;

        // Clean up runtime profile before leaving
        if (runtimeProfile != null)
        {
            Destroy(runtimeProfile);
        }

        SceneManager.LoadScene(levelName);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            InRange = true;

            // Show the appropriate text only when the player enters range
            if (Cleared)
            {
                Dialogue.text = StringToBeTold;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            InRange = false;
            Dialogue.text = "";
        }
    }
}
