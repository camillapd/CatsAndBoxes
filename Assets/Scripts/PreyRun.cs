using UnityEngine;
using System.Collections;

public class PreyRun : MonoBehaviour
{
    public float runSpeed = 10f;
    public LayerMask wallLayer;
    public Vector2 runDirection;

    private bool isRunning = false;
    private Collider2D col;
    private SpriteRenderer sprite;
    public float startDelay = 0.2f;

    public bool IsRunning => isRunning;

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
        StartCoroutine(RunAwayWithDelay());
    }

    IEnumerator RunAwayWithDelay()
    {
        yield return new WaitForSeconds(startDelay);
        yield return StartCoroutine(RunAway());
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
                sprite.enabled = false;
                Destroy(gameObject, 0.2f);
                yield break;
            }

            float duration = 1f / runSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                transform.position = Vector2.Lerp(currentPos, nextPos, t);
                yield return null;
            }

            transform.position = nextPos;
        }
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        transform.position = RoundToGrid(transform.position);
    }


}
