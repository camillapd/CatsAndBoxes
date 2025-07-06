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
    private MenuController menu;

    void Start()
    {
        menu = FindFirstObjectByType<MenuController>();
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

        yield return new WaitForSeconds(1f);

        if (menu != null)
            menu.ShowLevelCompleteMessage();

        yield return new WaitForSeconds(1f);

        if (menu != null)
            menu.HideLevelCompleteMessage();

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
            }
        }
    }

    public void GameOver()
    {
        StartCoroutine(WaitAndLoseLevel());
    }

    private IEnumerator WaitAndLoseLevel()
    {
        if (isGameOver) yield break;

        isGameOver = true;

        if (playerAnimator != null)
            playerAnimator.SetTrigger("loseLevel");

        yield return new WaitForSeconds(1f);

        if (menu != null)
            menu.ShowLoseLevelMessage();
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

        if (menu != null)
            menu.HideLoseLevelMessage();
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