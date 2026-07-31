using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class AnswerScript : MonoBehaviour
{
    public bool isCorrect = false;
    public QuizManager quizManager;

    public Color startColor;

    private Image img;

    private void Start()
    {
        img = GetComponent<Image>();
        startColor = img.color;
    }

    public void Answer()
    {
        if (quizManager.answered)
            return;

        quizManager.answered = true;

        StartCoroutine(AnswerRoutine());
    }

    IEnumerator AnswerRoutine()
    {
        if (isCorrect)
        {
            img.color = Color.green;

            yield return new WaitForSeconds(1f);

            img.color = startColor;

            quizManager.correct();
        }
        else
        {
            img.color = Color.red;

            yield return new WaitForSeconds(1f);

            img.color = startColor;

            quizManager.wrong();
        }
    }
}