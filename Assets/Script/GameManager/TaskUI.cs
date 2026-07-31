using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TaskUI : MonoBehaviour
{
    [SerializeField] TMP_Text descriptionText;
    [SerializeField] Image checkmarkImage;
    [SerializeField] Sprite incompleteSprite;
    [SerializeField] Sprite completeSprite;

    public void SetDescription(string text)
    {
        descriptionText.text = text;
    }

    public void SetCompleted(bool completed)
    {
        checkmarkImage.sprite = completed ? completeSprite : incompleteSprite;
    }
}
