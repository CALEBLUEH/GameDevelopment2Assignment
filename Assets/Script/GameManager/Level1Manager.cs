using System.Collections;
using TMPro;
using UnityEngine;

public class Level1Manager : GameManager
{
    [Header("UI References")]
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] Transform taskListParent;

    private int totalEnemies;
    private bool hasVictoryTriggered = false;

    public static bool Cleared = false;

    void Start()
    {
        Cleared = false;
        CreateTaskEntry("Defeat all enemies", taskListParent);
        UpdateEnemyCounter();
    }

    protected override void Update()
    {
        base.Update();
        source.volume = VolumeHolder.SFXVolume;
        UpdateEnemyCounter();
        CheckVictoryCondition();
    }

    void UpdateEnemyCounter()
    {
        int currentEnemies = GetEnemyCountByTag("Enemy");
        enemyCountText.text = currentEnemies + "";

        UpdateTaskStatus(0, currentEnemies == 0);
    }

    protected override bool CheckVictoryCondition()
    {
        if (hasVictoryTriggered) return true;

        bool isVictory = GetEnemyCountByTag("Enemy") == 0;
        if (isVictory)
        {
            Cleared = true;
            hasVictoryTriggered = true;
            StartCoroutine(VictoryDelay());
        }
        return isVictory;
    }

    IEnumerator VictoryDelay()
    {
        yield return new WaitForSeconds(1f);
        Victory();
    }
}
