using System.Collections;
using TMPro;
using UnityEngine;

public class Level3Manager : GameManager
{
    [Header("UI References")]
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] Transform taskListParent;

    [SerializeField] float timeLimit = 60f;
    [SerializeField] private TMP_Text timerText;
    private float remainingTime;
    private bool hasVictoryTriggered = false;

    public static bool Cleared = false;
    void Start()
    {
        Cleared = false;
        remainingTime = timeLimit;
        CreateTaskEntry("Defeat all enemies on time", taskListParent);
        UpdateEnemyCounter();
        UpdateTimerUI();
    }

    protected override void Update()
    {
        base.Update();
        source.volume = VolumeHolder.SFXVolume;
        remainingTime -= Time.deltaTime;
        UpdateEnemyCounter();
        UpdateTimerUI();

        if (hasVictoryTriggered) return;

        if (remainingTime <= 0)
        {
            if (CheckVictoryCondition())
            {
                Cleared = true;
                hasVictoryTriggered = true;
                StartCoroutine(VictoryDelay());
            }
            else
            {
                Defeat();
            }
            enabled = false;
            return;
        }

        if (CheckVictoryCondition())
        {
            Cleared = true;
            hasVictoryTriggered = true;
            StartCoroutine(VictoryDelay());
            enabled = false;
        }
    }

    IEnumerator VictoryDelay()
    {
        yield return new WaitForSeconds(1f);
        Victory();
    }

    protected override bool CheckVictoryCondition()
    {
        return GetEnemyCountByTag("Enemy") == 0;
    }

    void UpdateEnemyCounter()
    {
        int currentEnemies = GetEnemyCountByTag("Enemy");
        enemyCountText.text = currentEnemies + "";

        UpdateTaskStatus(0, currentEnemies == 0);
    }

    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60);
            int seconds = Mathf.FloorToInt(remainingTime % 60);
            timerText.text = $"{minutes:00}:{seconds:00}";

            timerText.color = remainingTime <= 10 ? Color.red : Color.white;
        }
    }
}
