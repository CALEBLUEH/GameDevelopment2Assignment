using System.Collections;
using UnityEngine;

public class KeyItem : MonoBehaviour
{
    [SerializeField] float rotateSpeed = 60f;

    [SerializeField]
    AudioSource source;

    void Update()
    {
        source.volume = VolumeHolder.SFXVolume;
        transform.Rotate(new Vector3(0, 0, rotateSpeed) * Time.deltaTime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var manager = FindFirstObjectByType<Level2Manager>();
            if (manager != null)
            {
                manager.ObtainKeyItem();
                source.Play();
                StartCoroutine(Wait());
            }
        }
    }

    IEnumerator Wait()
    {
        yield return new WaitForSeconds(0.25f);
        Destroy(gameObject);
    }
}
