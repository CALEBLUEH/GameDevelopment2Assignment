using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Level2Manager : GameManager
{
    [Header("UI References")]
    [SerializeField] TMP_Text enemyCountText;
    [SerializeField] Transform taskListParent;

    private bool hasKeyItem;
    private int totalEnemies;
    private bool hasVictoryTriggered = false;

    public static bool Cleared = false;

    void Start()
    {
        Cleared = false;
        CreateTaskEntry("Defeat all enemies", taskListParent);
        CreateTaskEntry("Obtain the Suitcase", taskListParent);
        UpdateEnemyCounter();
    }

    protected override void Update()
    {
        base.Update();
        source.volume = VolumeHolder.SFXVolume;
        UpdateEnemyCounter();
        UpdateTaskStatus();

        if (hasVictoryTriggered) return;

        if (CheckVictoryCondition())
        {
            Cleared = true;
            hasVictoryTriggered = true;
            StartCoroutine(VictoryDelay());
        }
    }

    IEnumerator VictoryDelay()
    {
        yield return new WaitForSeconds(1f);
        Victory();
    }

    public void ObtainKeyItem()
    {
        hasKeyItem = true;
        UpdateTaskStatus();
    }

    void UpdateEnemyCounter()
    {
        int currentEnemies = GetEnemyCountByTag("Enemy");
        enemyCountText.text = currentEnemies + "";
    }

    void UpdateTaskStatus()
    {
        UpdateTaskStatus(0, GetEnemyCountByTag("Enemy") == 0);
        UpdateTaskStatus(1, hasKeyItem);
    }

    protected override bool CheckVictoryCondition()
    {
        return GetEnemyCountByTag("Enemy") == 0 && hasKeyItem;
    }
}
