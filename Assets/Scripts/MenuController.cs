using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject pauseMenuUI;
    public GameObject helpMenuUI;
    public GameObject winLevelUI;
    public GameObject loseLevelUI;
    public GameObject endingScreen;
    public GameObject gameHUD;
    public GameObject gameManager;
    public MusicManager musicManager;

    private LevelManager levelManager;
    private bool isPaused = false;
    private bool openedHelpFromPause = false;

    void Start()
    {
        levelManager = gameManager.GetComponent<LevelManager>();
        ShowMainMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (isPaused)
                ResumeGame();
            else if (!isPaused && gameHUD.activeSelf)
                PauseGame();
        }
    }

    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);
        gameHUD.SetActive(false);
        winLevelUI.SetActive(false);
        loseLevelUI.SetActive(false);
        Time.timeScale = 0f;
        isPaused = false;
        musicManager.PlayMenuMusic();
    }

    public void StartGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        if (gameHUD != null)
            gameHUD.SetActive(true);

        if (winLevelUI != null)
            winLevelUI.SetActive(false);

        if (levelManager != null)
            levelManager.LoadLevel(0);

        if (levelManager != null)
            loseLevelUI.SetActive(false);

        Time.timeScale = 1f;
        isPaused = false;
        musicManager.PlayGameMusic();

        if (helpMenuUI != null && levelManager.CurrentLevelIndex == 0)
        {
            helpMenuUI.SetActive(true);
            openedHelpFromPause = false;
        }
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        gameHUD.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        gameHUD.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void CloseHelper()
    {
        helpMenuUI.SetActive(false);

        if (openedHelpFromPause)
        {
            pauseMenuUI.SetActive(true);
        }
        else
        {
            Time.timeScale = 1f;
            isPaused = false;
        }
    }

    public void ShowHelp()
    {
        if (helpMenuUI != null)
            helpMenuUI.SetActive(true);
    }

    public void ShowHelpFromPause()
    {
        if (helpMenuUI != null)
            helpMenuUI.SetActive(true);

        openedHelpFromPause = true;
        pauseMenuUI.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        gameManager.GetComponent<GameManager>().ResetGame();

        int currentLevel = levelManager.CurrentLevelIndex;
        levelManager.LoadLevel(currentLevel);

        ResumeGame();
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        gameManager.GetComponent<GameManager>().ResetGame();
        ShowMainMenu();
    }

    public void ShowLevelCompleteMessage()
    {
        if (winLevelUI != null)
            winLevelUI.SetActive(true);
    }

    public void HideLevelCompleteMessage()
    {
        if (winLevelUI != null)
            winLevelUI.SetActive(false);
    }

    public void ShowLoseLevelMessage()
    {
        if (loseLevelUI != null)
            loseLevelUI.SetActive(true);
    }

    public void HideLoseLevelMessage()
    {
        if (loseLevelUI != null)
            loseLevelUI.SetActive(false);
    }

    public void showEndingMessage()
    {
        gameHUD.SetActive(false);
        endingScreen.SetActive(true);
        musicManager.PlayMenuMusic();
    }
}
