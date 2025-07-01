using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public GameObject[] levels;               // Prefabs das fases
    public GameManager gameManager;           // Referência via Inspector

    private GameObject currentLevel;
    private int levelIndex = 0;

    public LevelInfo CurrentLevelInfo { get; private set; }

    public int CurrentLevelIndex => levelIndex;
    public int TotalLevels => levels.Length;

    void Awake()
    {
        // Garantir referência ao GameManager no mesmo GameObject
        if (gameManager == null)
            gameManager = GetComponent<GameManager>();
    }

    void Start()
    {
        LoadLevel(levelIndex);
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
                if (gameManager != null)
                {
                    gameManager.SetGatosParent(CurrentLevelInfo.gatosParent);
                }
                else
                {
                    Debug.LogError("GameManager não está atribuído no LevelManager!");
                }
            }
            else
            {
                Debug.LogWarning("LevelInfo não encontrado no prefab " + currentLevel.name);
            }
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
