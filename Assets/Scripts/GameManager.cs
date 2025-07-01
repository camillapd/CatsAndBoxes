using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isGameOver = false;

    private GameObject gatosParentObject;
    private HashSet<GameObject> allCats = new HashSet<GameObject>();
    private Animator playerAnimator;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            LevelManager lm = FindFirstObjectByType<LevelManager>();
            if (lm != null)
            {
                Debug.Log("🔄 Reiniciando a fase atual...");
                lm.LoadLevel(lm.CurrentLevelIndex);
            }
            else
            {
                Debug.LogError("❌ LevelManager não encontrado!");
            }
        }
    }

    public void SetPlayerAnimator(Animator animator)
    {
        playerAnimator = animator;
    }

    public void SetGatosParent(Transform parent)
    {
        gatosParentObject = parent.gameObject;
        allCats.Clear();

        for (int i = 0; i < parent.childCount; i++)
        {
            allCats.Add(parent.GetChild(i).gameObject);
        }

        Debug.Log($"🐱 {allCats.Count} gatos encontrados nesta fase.");
    }

    public void CheckVictory()
    {
        if (AllCatsInBoxes())
        {
            Debug.Log("🎉 Todos os gatos estão nas caixas! Vitória!");
            WinGame();
        }
        else
        {
            Debug.Log("😺 Ainda tem gato fora da caixa.");
        }
    }

    bool AllCatsInBoxes()
    {
        foreach (var cat in allCats)
        {
            CatState catState = cat.GetComponent<CatState>();
            if (catState == null || !catState.isInsideBox)
            {
                return false;
            }
        }
        return true;
    }

    void WinGame()
    {
        Debug.Log("🏁 Fase vencida!");
        StartCoroutine(WaitAndLoadNextLevel());
    }

    private IEnumerator WaitAndLoadNextLevel()
    {
        if (playerAnimator != null)
            playerAnimator.SetTrigger("winLevel");

        yield return new WaitForSeconds(5f);

        LevelManager lm = FindFirstObjectByType<LevelManager>();
        if (lm != null)
        {
            int next = lm.CurrentLevelIndex + 1;
            if (next < lm.TotalLevels)
            {
                Debug.Log("➡️ Indo para a próxima fase...");
                lm.NextLevel();
            }
            else
            {
                Debug.Log("🎉 Jogo completo! Todas as fases vencidas!");
                // Exibir tela final, créditos, voltar ao menu, etc.
            }
        }
    }

    public void GameOver()
    {
        Debug.Log("💥 Game Over! Um gato fugiu!");

        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("💀 Game Over");

        if (playerAnimator != null)
            playerAnimator.SetTrigger("loseLevel");
    }

    public void ResetGame()
    {
        isGameOver = false;
        allCats.Clear();
        // Resetar outras variáveis de estado, se houver
    }


}