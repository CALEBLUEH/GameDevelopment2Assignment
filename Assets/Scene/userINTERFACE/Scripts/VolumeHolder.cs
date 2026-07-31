using UnityEngine;

public class VolumeHolder : MonoBehaviour
{
    public static float BGMVolume = 0.5f;
    public static float SFXVolume = 0.5f;

    private static VolumeHolder instance;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Sets the background music volume.
    /// </summary>
    public void GetBGM(float value)
    {
        BGMVolume = value;
    }

    /// <summary>
    /// Sets the sound effects volume.
    /// </summary>
    public void GetSFX(float value)
    {
        SFXVolume = value;
    }
}
