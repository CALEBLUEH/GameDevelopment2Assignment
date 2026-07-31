using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingPanel;
    [SerializeField] protected AudioSource source;

    [SerializeField] protected GameObject taskUIPrefab;
    protected List<TaskUI> activeTasks = new List<TaskUI>();

    private bool isPaused = false;

    private void Awake()
    {
        InitializePanels();
        InitializeTimeScale();
    }

    private void InitializePanels()
    {
        SetPanelState(victoryPanel, false);
        SetPanelState(defeatPanel, false);
        SetPanelState(pausePanel, false);
    }

    protected virtual void Update()
    {
        HandlePauseInput();
        source.volume = VolumeHolder.SFXVolume;
    }

    private void HandlePauseInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !IsGameEnded())
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        PlayerScript.IsPaused = isPaused;
        SetPauseState(isPaused);
    }

    private void SetPauseState(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        SetPanelState(pausePanel, paused);

        if (!IsGameEnded())
        {
            Cursor.visible = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
        }
    }

    private void SetPanelState(GameObject panel, bool state)
    {
        if (panel) panel.SetActive(state);
    }

    private bool IsGameEnded()
    {
        return (victoryPanel && victoryPanel.activeSelf) ||
               (defeatPanel && defeatPanel.activeSelf);
    }

    protected void ShowEndGamePanel(GameObject panel)
    {
        if (!panel) return;

        Time.timeScale = 0f;
        SetPanelState(panel, true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    protected int GetEnemyCountByTag(string tag)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);
        return enemies.Length;
    }
    protected void CreateTaskEntry(string description, Transform parent)
    {
        GameObject taskObj = Instantiate(taskUIPrefab, parent);
        TaskUI taskUI = taskObj.GetComponent<TaskUI>();
        taskUI.SetDescription(description);
        activeTasks.Add(taskUI);
    }

    protected void UpdateTaskStatus(int index, bool isComplete)
    {
        if (index >= 0 && index < activeTasks.Count)
        {
            activeTasks[index].SetCompleted(isComplete);
        }
    }

    protected virtual bool CheckVictoryCondition() => false;

    protected void Victory()
    {
        Debug.Log("Victory!");
        ShowEndGamePanel(victoryPanel);
    }

    public void Defeat()
    {
        Debug.Log("Defeat...");
        ShowEndGamePanel(defeatPanel);
    }

    public void ResumeGame()
    {
        if (IsGameEnded()) return;
        TogglePause();
        source.Play();
    }

    public void OpenSetting()
    {
        source.Play();
        SetPanelState(pausePanel, false);
        settingPanel.SetActive(true);
    }

    public void CloseSetting()
    {
        source.Play();
        settingPanel.SetActive(false);
        SetPanelState(pausePanel, true);
    }

    public void ReturnToMainMenu()
    {
        Time.timeScale = 1f;
        source.Play();
        SceneManager.LoadScene(0);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        source.Play();
        SceneManager.LoadScene(sceneName);
    }

    private void InitializeTimeScale()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public void QuitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}
