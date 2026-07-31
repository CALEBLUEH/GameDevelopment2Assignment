using UnityEngine;

public class Music : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        GetComponent<AudioSource>().volume = VolumeHolder.BGMVolume;
    }
}
