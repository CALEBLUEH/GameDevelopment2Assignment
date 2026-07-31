using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizTrigger : MonoBehaviour
{
    public GameObject interactText;

    private bool playerNearby = false;

    //Google Form link
    public string googleFormURL = "https://forms.gle/fsfTmWLbWuqhK3Q69";

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Application.OpenURL(googleFormURL);
        }

        if (playerNearby && Input.GetKeyDown(KeyCode.F))
        {
            SceneManager.LoadScene("Quiz");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            interactText.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            interactText.SetActive(false);
        }
    }
}