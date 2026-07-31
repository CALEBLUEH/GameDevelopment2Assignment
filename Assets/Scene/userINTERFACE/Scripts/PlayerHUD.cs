using UnityEngine;
using UnityEngine.UI;

public class PlayerHUD : MonoBehaviour
{
    public Image HealthBar;
    // Update is called once per frame
    void Update()
    {
        HealthBar.fillAmount = (float)GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().currentHealth / (float)GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerScript>().maxHealth;
    }
}
