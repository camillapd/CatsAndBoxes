using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MouseLoop : MonoBehaviour
{
    public float moveSpeed = 0.5f;
    public LayerMask obstacleLayer;

    private Vector2 startPos;
    private Vector2[] directions = {
        Vector2.up, Vector2.down, Vector2.left, Vector2.right
    };

    private void Start()
    {
        startPos = transform.position;
        StartCoroutine(MoveMouse());
    }

    IEnumerator MoveMouse()
    {
        while (true)
        {
            Vector2 choosenDir = Vector2.zero;
            List<Vector2> possDir = new List<Vector2>();

            foreach (Vector2 dir in directions)
            {
                bool canMove = true;
                Vector2 checkPos = startPos;

                for (int i = 1; i <= 5; i++)
                {
                    checkPos += dir;
                    if (Physics2D.OverlapCircle(checkPos, 0.1f, obstacleLayer))
                    {
                        canMove = false;
                        break;
                    }
                }

                if (canMove)
                    possDir.Add(dir);
            }

            if (possDir.Count == 0)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            choosenDir = possDir[Random.Range(0, possDir.Count)];

            for (int i = 1; i <= 5; i++)
            {
                Vector2 finalPos = startPos + choosenDir * i;
                yield return StartCoroutine(moveTill(finalPos));
            }

            for (int i = 4; i >= 0; i--)
            {
                Vector2 finalPos = startPos + choosenDir * i;
                yield return StartCoroutine(moveTill(finalPos));
            }
        }
    }

    IEnumerator moveTill(Vector2 finalPos)
    {
        Vector2 firstPos = transform.position;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector2.Lerp(firstPos, finalPos, t);
            yield return null;
        }

        transform.position = finalPos;
    }
}
