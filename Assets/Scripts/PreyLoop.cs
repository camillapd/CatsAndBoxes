using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreyLoop : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public LayerMask obstacleLayer;
    public Vector2 chosenDir = Vector2.zero;
    public bool isActive = true;

    private int PreySteps = 5;
    private PreyRun preyRun;
    private Vector2 startPos;
    private Animator anim;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        preyRun = GetComponent<PreyRun>();
        startPos = RoundToGrid(transform.position);

        anim = GetComponentInChildren<Animator>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        ChooseInitialDirection();

        if (chosenDir != Vector2.zero)
        {
            if (anim != null)
                anim.Play("Running"); // força a tocar Running logo no começo

            StartCoroutine(MovePrey());
        }
        else
        {
            Debug.LogWarning("🐭 Nenhuma direção com 5 tiles livres encontrada!");
        }
    }

    void ChooseInitialDirection()
    {
        Vector2[] directions = {
            Vector2.right, Vector2.left // só horizontal
        };

        foreach (Vector2 dir in directions)
        {
            Vector2 checkPos = startPos;
            bool clear = true;

            for (int i = 0; i < PreySteps; i++)
            {
                checkPos += dir;
                if (Physics2D.OverlapCircle(checkPos, 0.1f, obstacleLayer))
                {
                    clear = false;
                    break;
                }
            }

            if (clear)
            {
                chosenDir = dir;
                return;
            }
        }
    }

    IEnumerator MovePrey()
    {
        while (isActive)
        {
            if (preyRun != null && preyRun.IsRunning)
            {
                yield break;
            }

            for (int i = 0; i < PreySteps; i++)
            {
                startPos += chosenDir;

                if (spriteRenderer != null)
                    spriteRenderer.flipX = chosenDir.x > 0;
                
                yield return StartCoroutine(WaitAndMoveTo(startPos));
            }

            chosenDir *= -1.0f; // inverte direção
            
        }
    }

    IEnumerator WaitAndMoveTo(Vector2 finalPos)
    {
        while (Physics2D.OverlapCircle(finalPos, 0.1f, obstacleLayer))
        {
            yield return new WaitForSeconds(0.1f);
        }

        Vector2 start = transform.position;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector2.Lerp(start, finalPos, t);

            yield return null;
        }
        transform.position = finalPos;

    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
    }
}
