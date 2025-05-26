using UnityEngine;
using System.Collections;

public class HoldCats : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask collisionLayer;
    public LayerMask catLayer;
    public LayerMask catRunLayer;
    public LayerMask boxLayer;

    private bool isHoldingCat = false;

    private GameObject heldCat;
    public GameObject mouseObject;   
    private MouseRun mouseScript;

    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        if (mouseObject != null)
        {
            mouseScript = mouseObject.GetComponent<MouseRun>();
        }
    }

    void CheckIfPlayerOnMousePath()
    {
        if (heldCat == null || mouseObject == null || mouseScript == null) return;

        Vector2 playerPos = new Vector2(RoundToGrid(transform.position.x), RoundToGrid(transform.position.y));
        Vector2 mousePos = new Vector2(RoundToGrid(mouseObject.transform.position.x), RoundToGrid(mouseObject.transform.position.y));
        Vector2 mouseRunDir = mouseScript.runDirection;
        Vector2 nextMousePos = mousePos + mouseRunDir;

        if (playerPos == nextMousePos && isHoldingCat)
        {

            // 1. O rato deve fugir na direção oposta da do jogador (já feito no rato)
            Vector2 oppositeDir = -lastDirection;
            if (oppositeDir == Vector2.zero)
                oppositeDir = Vector2.down;

            mouseScript.InitRun(oppositeDir);

            Vector2 playerGridPos = new Vector2(
                RoundToGrid(transform.position.x),
                RoundToGrid(transform.position.y)
            );
            heldCat.transform.position = new Vector3(playerGridPos.x, playerGridPos.y, -0.1f);

            // 2. O gato perde o jogador e começa a correr atrás do rato
            heldCat.SetActive(true);
            heldCat.GetComponent<SpriteRenderer>().enabled = true;
            heldCat.GetComponent<Collider2D>().enabled = true;

            CatGetPrey catScript = heldCat.GetComponent<CatGetPrey>();
            if (catScript != null)
            {
                catScript.InitChase(mouseObject.transform);
                Debug.Log("🐱 Gato perdeu o jogador e está perseguindo o rato!");
            }

            // 3. O jogador perde o gato
            heldCat = null;
            isHoldingCat = false;

            Debug.Log("😿 Jogador perdeu o gato que está perseguindo o rato!");
        }
    }
   
    void CheckIfPlayerOnCatRun()
    {
        if (!isHoldingCat || heldCat == null)
            return;

        Vector2 pos = new Vector2(
            RoundToGrid(transform.position.x),
            RoundToGrid(transform.position.y)
        );

        // Raio bem pequeno para garantir só pegar exatamente o tile embaixo do jogador
        float radius = 0.05f;

        if (Physics2D.OverlapCircle(pos, radius, catRunLayer))
        {
            Vector2 oppDirection = -lastDirection;
            if (oppDirection == Vector2.zero)
                oppDirection = Vector2.down;

            Vector2 startRun = (Vector2)transform.position + oppDirection;

            heldCat.transform.position = new Vector3(startRun.x, startRun.y, -0.1f);
            heldCat.SetActive(true);
            heldCat.GetComponent<SpriteRenderer>().enabled = true;
            heldCat.GetComponent<Collider2D>().enabled = true;

            CatRun scriptCatRun = heldCat.GetComponent<CatRun>();
            if (scriptCatRun != null)
            {
                scriptCatRun.InitRun(oppDirection);
                Debug.Log("😿 Gato se assustou e fugiu!");
            }

            heldCat = null;
            isHoldingCat = false;
        }
    }

    public void TryHoldCat()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir;
            Collider2D catCol = Physics2D.OverlapCircle(checkPos, 0.1f, catLayer);

            if (catCol != null)
            {
                heldCat = catCol.gameObject;
                heldCat.transform.position = RoundToGrid(transform.position); // 👈 Garante que o gato acompanhe o jogador!
                heldCat.GetComponent<SpriteRenderer>().enabled = false;
                heldCat.GetComponent<Collider2D>().enabled = false;
                isHoldingCat = true;

                Debug.Log("🐾 Gato pego!");
                return;
            }
        }
        Debug.Log("Nenhum gato próximo para pegar.");
    }


    public void ReleaseCat()
    {
        if (heldCat == null) return;

        Vector2 dropDir = lastDirection;
        if (dropDir == Vector2.zero) dropDir = Vector2.down;

        Vector2 dropPos = (Vector2)transform.position + dropDir;
        bool isBlocked = Physics2D.OverlapCircle(dropPos, 0.1f, collisionLayer);
        Collider2D boxCol = Physics2D.OverlapCircle(dropPos, 0.1f, boxLayer);          
               
        if (!isBlocked || boxCol != null)
        {
            heldCat.transform.position = new Vector3(dropPos.x, dropPos.y, -0.1f);
            heldCat.GetComponent<SpriteRenderer>().enabled = true;
            heldCat.GetComponent<Collider2D>().enabled = true;

            if (boxCol != null)
            {
                int blockedLayer = LayerMask.NameToLayer("Travado");

                if (boxCol.gameObject.layer == blockedLayer)
                {
                    Debug.Log("❌ Essa caixa já está ocupada. Não é possível soltar o gato aqui.");
                    return;
                }
            
                heldCat.GetComponent<SpriteRenderer>().sortingOrder = 10;              
                boxCol.gameObject.layer = blockedLayer;
                heldCat.layer = blockedLayer;

                Debug.Log("🐾 Gato colocado na caixa. Agora está travado.");
            }
            else
            {
                Debug.Log("🐾 Gato solto normalmente.");
            }

            heldCat = null;
            isHoldingCat = false;
        }
        else
        {
            Debug.Log("❌ Não é possível soltar o gato aqui. Direção bloqueada.");
        }
    }

    public bool IsHoldingCat()
    {
        return isHoldingCat;
    }

    public void NotifyMoveIntent(Vector2 dir)
    {
        lastDirection = dir;
    }

    public void NotifyArrived()
    {
        CheckIfPlayerOnCatRun();
        CheckIfPlayerOnMousePath();
    }

    float RoundToGrid(float value)
    {
        return Mathf.Floor(value) + 0.5f;
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(RoundToGrid(pos.x), RoundToGrid(pos.y));
    }

}
