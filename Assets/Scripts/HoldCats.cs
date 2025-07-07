using UnityEngine;
using System.Collections;

public class HoldCats : MonoBehaviour
{
    public LayerMask collisionLayer;
    public LayerMask catLayer;
    public LayerMask catRunLayer;
    public LayerMask boxLayer;
    public GameObject preyObject;
    public Transform visual;

    private GameObject heldCat;
    private PreyLoop loopScript;
    private PreyRun preyScript;
    private bool isHoldingCat = false;
    private Vector2 lastDirection = Vector2.down;
    private GameManager GM;
    private Animator animator;
    private GameObject preyLoopObject;
    private HUDController hud;

    void Start()
    {
        GM = Object.FindAnyObjectByType<GameManager>();

        if (preyObject != null)
        {
            preyScript = preyObject.GetComponentInChildren<PreyRun>();
            loopScript = preyObject.GetComponentInChildren<PreyLoop>();
            preyLoopObject = preyObject.GetComponentInChildren<PreyLoop>()?.gameObject;
        }

        if (visual != null)
            animator = visual.GetComponent<Animator>();
    }

    void Awake()
    {
        hud = FindObjectOfType<HUDController>();
    }

    void CheckIfPlayerOnPreyPath()
    {
        if (heldCat == null || preyLoopObject == null) return;

        Vector2 playerPos = RoundToGrid(transform.position);
        Vector2 preyPos = RoundToGrid(preyLoopObject.transform.position);
        Vector2 preyChosenDir = loopScript.chosenDir;
        Vector2 preyNextPos = preyPos + preyChosenDir;

        float distanceNow = (playerPos - preyPos).sqrMagnitude;
        float distanceNext = (playerPos - preyNextPos).sqrMagnitude;

        bool isMovingTowardsCat = distanceNow > distanceNext;
        bool preyCatSameAxis = preyPos.y == playerPos.y;

        if (preyCatSameAxis && isMovingTowardsCat)
        {
            Vector2 fleeDir = -preyChosenDir;
            Vector2 rawDir = preyPos - playerPos;
            Vector2 dirPlayerToPrey = Vector2.zero;

            if (Mathf.Abs(rawDir.x) > Mathf.Abs(rawDir.y))
                dirPlayerToPrey = new Vector2(Mathf.Sign(rawDir.x), 0);
            else
                dirPlayerToPrey = new Vector2(0, Mathf.Sign(rawDir.y));

            Vector2 catStartPos = playerPos + dirPlayerToPrey;

            heldCat.transform.position = new Vector3(catStartPos.x, catStartPos.y, -0.1f);
            heldCat.SetActive(true);
            heldCat.GetComponent<SpriteRenderer>().enabled = true;
            heldCat.GetComponent<Collider2D>().enabled = true;

            CatGetPrey catScript = heldCat.GetComponent<CatGetPrey>();
            if (catScript != null)
            {
                catScript.InitChase(preyLoopObject.transform);
            }

            preyScript.InitRun(fleeDir);
            heldCat = null;
            isHoldingCat = false;
            hud.SetHoldingCat(isHoldingCat);
        }
    }

    void CheckIfPlayerOnCatRun()
    {
        if (!isHoldingCat || heldCat == null) return;

        Vector2 pos = new Vector2(
            RoundToGrid(transform.position.x),
            RoundToGrid(transform.position.y)
        );

        float radius = 0.05f;

        if (Physics2D.OverlapCircle(pos, radius, catRunLayer))
        {
            Vector2 oppDirection = lastDirection;
            if (oppDirection == Vector2.zero) oppDirection = Vector2.down;

            Vector2 startRun = (Vector2)transform.position + oppDirection;

            heldCat.transform.position = new Vector3(startRun.x, startRun.y, -0.1f);
            heldCat.SetActive(true);

            heldCat.GetComponent<SpriteRenderer>().enabled = true;
            heldCat.GetComponent<Collider2D>().enabled = true;

            CatRun scriptCatRun = heldCat.GetComponent<CatRun>();
            if (scriptCatRun != null)
            {
                scriptCatRun.InitRun(oppDirection);
            }

            heldCat = null;
            isHoldingCat = false;
            hud.SetHoldingCat(isHoldingCat);
        }
    }

    public void TryHoldCat()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        foreach (var dir in directions)
        {
            Vector2 checkPos = RoundToGrid((Vector2)transform.position + dir);
            Collider2D catCol = Physics2D.OverlapBox(checkPos, boxSize, 0f, catLayer);

            Collider2D[] catHits = Physics2D.OverlapBoxAll(checkPos, boxSize, 0f, catLayer);

            foreach (var hit in catHits)
            {
                CatState catState = hit.GetComponent<CatState>();
                if (catState != null && !catState.isInsideBox)
                {
                    heldCat = hit.gameObject;
                    heldCat.transform.position = RoundToGrid(transform.position);
                    heldCat.GetComponent<SpriteRenderer>().enabled = false;
                    heldCat.GetComponent<Collider2D>().enabled = false;
                    isHoldingCat = true;
                    CatSFX sfx = heldCat.GetComponent<CatSFX>();
                    if (sfx != null)
                    {
                        sfx.PlayPickSound();
                    }

                    hud.SetHoldingCat(isHoldingCat);
                    return;
                }
            }

            if (catHits.Length > 0)
            {
                animator.SetTrigger("tryPullBlocked");
            }
        }
    }

    public void ReleaseCat()
    {
        if (heldCat == null) return;

        Vector2 dropDir = lastDirection;
        if (dropDir == Vector2.zero) dropDir = Vector2.down;

        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        Vector2 dropPos = (Vector2)transform.position + dropDir;
        bool isBlocked = Physics2D.OverlapBox(dropPos, boxSize, 0f, collisionLayer);
        Collider2D boxCol = Physics2D.OverlapBox(dropPos, boxSize, 0f, boxLayer);

        bool boxOccupied = false;
        if (boxCol != null)
        {
            BoxState boxState = boxCol.GetComponent<BoxState>();
            if (boxState != null && boxState.hasCatInside) boxOccupied = true;
        }

        if (!isBlocked && !boxOccupied)
        {
            if (boxCol != null)
            {
                BoxState boxState = boxCol.GetComponent<BoxState>();
                if (boxState != null && boxState.hasCatInside)
                {
                    return;
                }

                // Soltar dentro da caixa
                heldCat.transform.position = new Vector3(dropPos.x, dropPos.y, -0.1f);
                heldCat.GetComponent<SpriteRenderer>().enabled = true;
                heldCat.GetComponent<Collider2D>().enabled = true;
                heldCat.GetComponent<SpriteRenderer>().sortingOrder = 10;

                if (boxState != null)
                {
                    boxState.hasCatInside = true;
                }

                heldCat.GetComponent<CatState>().isInsideBox = true;
                SetIsOnBox();
                SFXManager.Instance.PlaySound(SFXManager.Instance.catOnBox);
                GM.UpdateCatBoxCounter();
                GM.CheckVictory();
            }
            else
            {
                heldCat.transform.position = new Vector3(dropPos.x, dropPos.y, -0.1f);
                heldCat.GetComponent<SpriteRenderer>().enabled = true;
                heldCat.GetComponent<Collider2D>().enabled = true;
            }

            heldCat = null;
            isHoldingCat = false;
            hud.SetHoldingCat(isHoldingCat);
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

    public void SetIsOnBox()
    {
        if (isHoldingCat)
        {
            Animator catAnimator = heldCat.GetComponent<Animator>();
            catAnimator.SetBool("isOnBox", true);
        }

    }
}
