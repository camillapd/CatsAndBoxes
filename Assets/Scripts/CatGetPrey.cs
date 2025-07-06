using UnityEngine;
using System.Collections;

public class CatGetPrey : MonoBehaviour
{
    public float chaseSpeed = 6f;
    public LayerMask wallsLayer;
    public LayerMask outsideLayer;

    private Transform preyTransform;
    private bool chasingPrey;
    private GameManager GM;
    private Animator anim;
    private Vector2 runDirection; // direção atual do movimento
    private SpriteRenderer spriteRenderer;

    public void InitChase(Transform prey)
    {
        GM = Object.FindAnyObjectByType<GameManager>();

        transform.position = RoundToGrid(transform.position);
        preyTransform = prey;
        chasingPrey = true;
        StopAllCoroutines();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        StartCoroutine(RunAfterPrey());

        anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", true);
        }

        CatSFX sfx = GetComponent<CatSFX>();
        sfx.PlaySeeMouseSound();
    }

    IEnumerator RunAfterPrey()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        while (chasingPrey)
        {
            if (preyTransform == null)
            {
                chasingPrey = false;
                if (anim != null) anim.SetBool("isRunning", false);
                yield break;
            }

            Vector2 catPos = RoundToGrid(transform.position);
            Vector2 preyPos = RoundToGrid(preyTransform.position);
            Vector2 dir = preyPos - catPos;

            if (dir == Vector2.zero)
            {
                chasingPrey = false;
                if (anim != null) anim.SetBool("isRunning", false);

                if (preyTransform != null)
                {
                    PreyRun preyRunScript = preyTransform.GetComponent<PreyRun>();
                    if (preyRunScript != null)
                        preyRunScript.Disappear();
                    else
                        Destroy(preyTransform.gameObject);
                }

                yield break;
            }

            dir = dir.normalized;

            Vector2 moveDir = Vector2.zero;
            float maxDot = -1f;

            foreach (var d in dirs)
            {
                float dot = Vector2.Dot(dir, d);
                if (dot > maxDot)
                {
                    maxDot = dot;
                    moveDir = d;
                }
            }

            runDirection = moveDir;

            if (anim != null)
                anim.SetInteger("direction", DirectionToInt(runDirection));

            if (spriteRenderer != null)
                spriteRenderer.flipX = runDirection.x < 0;

            Vector2 nextPos = catPos + moveDir;

            Collider2D walls = Physics2D.OverlapCircle(nextPos, 0.1f, wallsLayer);

            if (walls != null)
            {
                Collider2D outside = Physics2D.OverlapCircle(nextPos, 0.1f, outsideLayer);

                if (outside != null)
                {
                    if (anim != null) anim.SetBool("isRunning", false);
                    Destroy(gameObject);
                    GM.GameOver();
                    yield break;
                }

                chasingPrey = false;
                if (anim != null) anim.SetBool("isRunning", false);
                GM.CheckVictory();
                yield break;
            }

            yield return StartCoroutine(Mover(moveDir));
        }
    }

    IEnumerator Mover(Vector2 direction)
    {
        Vector2 endPos = (Vector2)transform.position + direction;
        while (Vector2.Distance(transform.position, endPos) > 0.01f)
        {
            transform.position = Vector2.MoveTowards(transform.position, endPos, chaseSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = endPos;
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
    }

    private int DirectionToInt(Vector2 dir)
    {
        // Força animação apenas para esquerda/direita
        if (Mathf.Abs(dir.x) >= Mathf.Abs(dir.y))
            return dir.x > 0 ? 3 : 2; // direita : esquerda
        else
            return dir.y > 0 ? 0 : 1; // cima : baixo (se precisar manter)
    }
}
