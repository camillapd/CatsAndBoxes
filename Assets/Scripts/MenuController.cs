using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public GameObject mainMenuUI;
    public GameObject pauseMenuUI;
    public GameObject gameHUD;
    public GameObject gameManager;

    private LevelManager levelManager;
    private bool isPaused = false;

    void Start()
    {
        ShowMainMenu();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (isPaused)
                ResumeGame();
            else if (!isPaused) // && gameHUD.activeSelf
                PauseGame();
        }
    }

    public void ShowMainMenu()
    {
        mainMenuUI.SetActive(true);
        pauseMenuUI.SetActive(false);
        // gameHUD.SetActive(false);
        Time.timeScale = 0f;
        isPaused = false;
    }

    public void StartGame()
    {
        if (mainMenuUI != null)
            mainMenuUI.SetActive(false);

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // if (gameHUD != null)
        //     gameHUD.SetActive(true);

        if (levelManager != null)
            levelManager.LoadLevel(0); // inicia primeira fase

        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);
        // gameHUD.SetActive(false);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false);
        // gameHUD.SetActive(true);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void QuitGame()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit();
    }

    public void RestartLevel()
    {
        Time.timeScale = 1f;
        gameManager.GetComponent<GameManager>().ResetGame();
        levelManager = gameManager.GetComponent<LevelManager>();

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
}
