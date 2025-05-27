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
    public GameObject preyObject;
    private PreyRun preyScript;

    private Vector2 lastDirection = Vector2.down;

    void Start()
    {
        if (preyObject != null)
        {
            preyScript = preyObject.GetComponent<PreyRun>();
        }
    }

    void CheckIfPlayerOnPreyPath()
    {
        if (heldCat == null || preyObject == null || preyScript == null)
            return;

        Vector2 playerPos = RoundToGrid(transform.position);
        Vector2 preyPos = RoundToGrid(preyObject.transform.position);
        Vector2 preyDir = preyScript.runDirection.normalized;
        Vector2 preyNextPos = preyPos + preyDir;

        bool movingHorizontally = Mathf.Abs(preyDir.x) > 0.1f;
        bool movingVertically = Mathf.Abs(preyDir.y) > 0.1f;

        if ((movingHorizontally || movingVertically) && playerPos == preyNextPos && heldCat != null)
        {
            Debug.Log("🐱 Jogador passou no próximo tile da presa segurando o gato.");

            preyScript.StopMoving();

            Vector2 fleeDir = -preyDir;
            Vector2 dirPlayerToPrey = (preyPos - playerPos).normalized;
            Vector2 catStartPos = playerPos + dirPlayerToPrey;

            heldCat.transform.position = new Vector3(catStartPos.x, catStartPos.y, -0.1f);
            heldCat.SetActive(true);
            heldCat.GetComponent<SpriteRenderer>().enabled = true;
            heldCat.GetComponent<Collider2D>().enabled = true;

            CatGetPrey catScript = heldCat.GetComponent<CatGetPrey>();
            if (catScript != null)
            {
                catScript.InitChase(preyObject.transform);
                Debug.Log("🐱 Gato iniciou perseguição!");
            }

            preyScript.InitRun(fleeDir);
            Debug.Log("🐭 Presa iniciou fuga!");

            heldCat = null;
            isHoldingCat = false;
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
                heldCat.transform.position = RoundToGrid(transform.position);
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
        CheckIfPlayerOnPreyPath();
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
