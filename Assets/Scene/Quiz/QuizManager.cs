using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    public List<QuestionAndAnswers> QnA;
    public GameObject[] options;
    public int currentQuestion;

    public TextMeshProUGUI QuestionText;
    public TextMeshProUGUI ScoreText;

    int totalQuestions = 0;
    public int score;

    public GameObject QuizPanel;
    public GameObject GOPanel;

    public bool answered = false;

    [Header("Name System")]
    public GameObject NamePanel;
    public TMP_InputField nameInput;

    [HideInInspector]
    public string playerName;

    public GoogleSheetsSender sheetSender;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        totalQuestions = QnA.Count;

        NamePanel.SetActive(true);

        QuizPanel.SetActive(false);
        GOPanel.SetActive(false);
    }

    public void StartQuiz()
    {
        playerName = nameInput.text;

        // Optional default name
        if (string.IsNullOrEmpty(playerName))
        {
            playerName = "Player";
        }

        NamePanel.SetActive(false);
        QuizPanel.SetActive(true);

        generateQuestion();
    }

    public void retry()
    {
        SceneManager.LoadScene("Quiz");
    }

    public void backToGallery()
    {
        SceneManager.LoadScene("Gallery");
    }

    public void GameOver()
    {
        QuizPanel.SetActive(false);
        GOPanel.SetActive(true);

        ScoreText.text = score + "/" + totalQuestions;

        sheetSender.SendScore(playerName, score);
    }

    public void correct()
    {
        score += 1;
        QnA.RemoveAt(currentQuestion);

        StartCoroutine(WaitAndGenerate());
    }

    public void wrong()
    {
        QnA.RemoveAt(currentQuestion);
        StartCoroutine(WaitAndGenerate());
    }

    IEnumerator WaitAndGenerate()
    {
        yield return new WaitForSeconds(1f);

        generateQuestion();
    }

    void SetAnswers()
    {
        for (int i = 0; i < options.Length; i++)
        {
            options[i].GetComponent<AnswerScript>().isCorrect = false;
            options[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = QnA[currentQuestion].Answer[i];

            if (QnA[currentQuestion].CorrectAns == i + 1)
            {
                options[i].GetComponent<AnswerScript>().isCorrect = true;
            }
        }
    }

    void generateQuestion()
    {
        answered = false;
        if (QnA.Count > 0)
        {
            currentQuestion = Random.Range(0, QnA.Count);

            QuestionText.text = QnA[currentQuestion].Question;
            SetAnswers();
        }
        else
        {
            Debug.Log("Out of question");
            GameOver();
        }
    }
}