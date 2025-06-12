using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LayerMask blockedLayer;

    private GameObject gatosParentObject;
    private List<GameObject> allCats = new List<GameObject>();
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
        foreach (GameObject cat in allCats)
        {
            if (!IsCatBlocked(cat))
            {
                Debug.Log("😺 Ainda tem gato fora da caixa.");
                return;
            }
        }

        Debug.Log("🎉 Todos os gatos estão nas caixas! Vitória!");
        WinGame();
    }

    bool IsCatBlocked(GameObject cat)
    {
        Vector2 pos = RoundToGrid(cat.transform.position);
        Collider2D hit = Physics2D.OverlapPoint(pos, blockedLayer);
        return hit != null;
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    void WinGame()
    {
        Debug.Log("🏁 Fase vencida!");
        StartCoroutine(WaitAndLoadNextLevel());
    }

    private System.Collections.IEnumerator WaitAndLoadNextLevel()
    {
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
                // Aqui você pode mostrar uma tela de final ou voltar ao menu
            }
        }
    }

    public void GameOver()
    {
        Debug.Log("💥 Game Over! Um gato fugiu!");
    }
}