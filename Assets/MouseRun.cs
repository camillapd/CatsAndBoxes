using UnityEngine;
using System.Collections;

public class MouseRun : MonoBehaviour
{
    public float runSpeed = 4f;
    public LayerMask wallLayer;
    public Vector2 runDirection;
    private bool isRunning = false;

    private Collider2D col;
    private SpriteRenderer sprite;

    void Start()
    {
        col = GetComponent<Collider2D>();
        sprite = GetComponent<SpriteRenderer>();
    }

    public void InitRun(Vector2 direction)
    {
        if (isRunning) return;

        transform.position = RoundToGrid(transform.position);

        col.enabled = true;
        runDirection = direction.normalized;
        isRunning = true;
        StopAllCoroutines();  
        StartCoroutine(RunAway());
    }

    IEnumerator RunAway()
    {
        while (isRunning)
        {
            Vector2 currentPos = RoundToGrid(transform.position);
            Vector2 nextPos = currentPos + runDirection;

            if (Physics2D.OverlapCircle(nextPos, 0.1f, wallLayer))
            {
                isRunning = false;
                GetComponent<SpriteRenderer>().enabled = false;
                Destroy(gameObject, 0.2f);
                yield break;
            }

            float t = 0f;
            Vector2 startPos = currentPos;

            while (t < 1f)
            {
                t += Time.deltaTime * runSpeed;
                transform.position = Vector2.Lerp(startPos, nextPos, t);
                yield return null;
            }

            transform.position = nextPos;
        }
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
    }

}
