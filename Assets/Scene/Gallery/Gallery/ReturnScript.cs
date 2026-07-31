using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

public class ReturnScript : MonoBehaviour
{
    public static bool IsTransitioning = false;

    [SerializeField]
    private VolumeProfile profile;

    private Volume PostProcessing;
    private Vignette vign;
    private LensDistortion lens;

    // Cloned profile so we don't modify the original asset
    private VolumeProfile runtimeProfile;

    private bool once = false;

    private void Start()
    {
        PostProcessing = GameObject.FindGameObjectWithTag("Return").GetComponent<Volume>();

        // Clone the profile so modifications don't persist on the asset
        runtimeProfile = Instantiate(profile);
        PostProcessing.profile = runtimeProfile;
        runtimeProfile.TryGet<Vignette>(out vign);
        runtimeProfile.TryGet<LensDistortion>(out lens);

        IsTransitioning = true;
        vign.intensity.value = 1;
        lens.intensity.value = -1;
    }

    private void Update()
    {
        vign.intensity.value -= Time.deltaTime * 0.6f;

        if (!once)
        {
            lens.intensity.value += Time.deltaTime * 0.8f;
        }
        else
        {
            if (lens.intensity.value <= 0)
            {
                lens.intensity.value = 0;

                // Clean up the cloned profile
                if (runtimeProfile != null)
                {
                    Destroy(runtimeProfile);
                }

                Destroy(gameObject);
            }
            else
            {
                lens.intensity.value -= Time.deltaTime * 0.8f;
            }
        }

        if (lens.intensity.value >= 1)
        {
            once = true;
        }
    }
}
