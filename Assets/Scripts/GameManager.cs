using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    public LayerMask blockedLayer;
    public GameObject gatosParentObject;

    private List<GameObject> allCats = new List<GameObject>();

    void Start()
    {
        if (gatosParentObject == null)
        {
            gatosParentObject = GameObject.Find("Gatos");
            if (gatosParentObject == null)
            {
                Debug.LogError("Objeto 'Gatos' não encontrado na cena!");
                return;
            }
        }

        allCats.Clear();
        Transform parentTransform = gatosParentObject.transform;

        for (int i = 0; i < parentTransform.childCount; i++)
        {
            allCats.Add(parentTransform.GetChild(i).gameObject);
        }
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
        Debug.Log("🏁 Fase vencida! Ir para próxima...");
    }

    public void GameOver()
    {
        Debug.Log("💥 Game Over! Um gato fugiu!");
    }
}
