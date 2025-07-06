using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] levels;
    public GameManager gameManager;
    public LevelInfo CurrentLevelInfo { get; private set; }
    public int CurrentLevelIndex => levelIndex;
    public int TotalLevels => levels.Length;
    public HUDController hudController;

    private GameObject currentLevel;
    private int levelIndex = 0;

    void Awake()
    {
        if (gameManager == null)
            gameManager = GetComponent<GameManager>();
    }

    public void LoadLevel(int index)
    {
        if (currentLevel != null)
            Destroy(currentLevel);

        if (gameManager != null)
        {
            gameManager.ResetGame();
        }
        else
        {
            Debug.LogError("GameManager não está atribuído no LevelManager!");
        }

        if (index >= 0 && index < levels.Length)
        {
            currentLevel = Instantiate(levels[index]);
            levelIndex = index;

            Animator playerAnimator = currentLevel.GetComponentInChildren<Animator>();
            gameManager.SetPlayerAnimator(playerAnimator);

            CurrentLevelInfo = currentLevel.GetComponent<LevelInfo>();
            if (CurrentLevelInfo != null)
            {
                gameManager.SetGatosParent(CurrentLevelInfo.gatosParent);
            }
            else
            {
                Debug.LogWarning("LevelInfo não encontrado no prefab " + currentLevel.name);
            }

            if (hudController != null)
            {
                hudController.SetLevelNumber(levelIndex);

            }
            
            gameManager.UpdateCatBoxCounter();
        }
        else
        {
            Debug.Log("🎉 Fim das fases!");
        }
    }

    public void NextLevel()
    {
        LoadLevel(levelIndex + 1);
    }
}
