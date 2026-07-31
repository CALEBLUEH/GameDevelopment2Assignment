using UnityEngine;

public class VolumeChanger : MonoBehaviour
{

    public void GetBGM(float value)
    {
        VolumeHolder.BGMVolume = value;
    }

    public void GetSFX(float value)
    {
        VolumeHolder.SFXVolume = value;
    }
}
