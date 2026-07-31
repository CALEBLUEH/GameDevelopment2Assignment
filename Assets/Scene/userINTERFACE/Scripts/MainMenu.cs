using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    public GameObject QRCode;
    public Image Qr;
    public TextMeshProUGUI text;
    public Image PostImage;
    public GameObject Setting;
    public GameObject Credit;

    private void Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if(GalleryManager.Cleared1 && GalleryManager.Cleared2 && GalleryManager.Cleared3)
        {
            QRCode.SetActive(true);
            Qr.sprite = PostImage.sprite;
            text.text = "Post-Survey";
        }
    }

    public void OpenQr()
    {
        QRCode.SetActive(true);
    }

    public void CloseQr()
    {
        QRCode.SetActive(false);
    }

    public void LoadScene()
    {
        int SceneIndex = SceneManager.GetActiveScene().buildIndex;

        SceneManager.LoadScene(SceneIndex + 1);

    }

    public void OpenSetting()
    {
        Setting.SetActive(true);
    }

    public void CloseSetting()
    {
        Setting.SetActive(false);
    }

    public void OpenCredit()
    {
        Credit.SetActive(true);
    }

    public void CloseCredit()
    {
        Credit.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

}
