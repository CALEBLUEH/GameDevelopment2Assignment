using System.Collections;
using UnityEngine;

public class QuickPopOut : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartCoroutine(destroy());
    }

    IEnumerator destroy()
    {
        yield return new WaitForSeconds(5.0f);
        Destroy(this.gameObject);
    }
}
