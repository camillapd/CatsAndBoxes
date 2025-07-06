using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static bool isGameOver = false;
    public HUDController hud;

    private GameObject gatosParentObject;
    private HashSet<GameObject> allCats = new HashSet<GameObject>();
    private Animator playerAnimator;

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
            WinGame();
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
        if (isGameOver) return;

        isGameOver = true;

        if (playerAnimator != null)
            playerAnimator.SetTrigger("loseLevel");
    }

    public void ResetGame()
    {
        isGameOver = false;
        allCats.Clear();
    }

    public void ResetLevel()
    {
        isGameOver = false;
        hud.ResetBoxCounter(allCats.Count);
    }

    public void UpdateCatBoxCounter()
    {
        int placed = 0;
        foreach (var cat in allCats)
        {
            CatState catState = cat.GetComponent<CatState>();
            if (catState != null && catState.isInsideBox)
            {
                placed++;
            }
        }

        if (hud != null)
        {
            hud.UpdateBoxCounter(placed, allCats.Count);
        }
    }

}