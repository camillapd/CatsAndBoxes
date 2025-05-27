using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PreyLoop : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public LayerMask obstacleLayer;

    private PreyRun preyRun;
    private Vector2 startPos;
    private Vector2 chosenDir = Vector2.zero;
    private Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private void Start()
    {
        preyRun = GetComponent<PreyRun>();
        startPos = RoundToGrid(transform.position);
        ChooseInitialDirection();

        if (chosenDir != Vector2.zero)
            StartCoroutine(MovePrey());
        else
            Debug.LogWarning("🐭 Nenhuma direção com 5 tiles livres encontrada!");
    }

    void ChooseInitialDirection()
    {
        foreach (Vector2 dir in directions)
        {
            Vector2 checkPos = startPos;
            bool clear = true;

            for (int i = 1; i <= 5; i++)
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
        while (true)
        {
            if (preyRun != null && preyRun.IsRunning)
            {
                yield return null;
            }

            for (int i = 1; i <= 5; i++)
            {
                Vector2 targetPos = startPos + chosenDir * i;
                yield return StartCoroutine(WaitAndMoveTo(targetPos));
            }

            for (int i = 4; i >= 0; i--)
            {
                Vector2 targetPos = startPos + chosenDir * i;
                yield return StartCoroutine(WaitAndMoveTo(targetPos));
            }
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
