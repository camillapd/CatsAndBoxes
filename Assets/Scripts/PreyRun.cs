using UnityEngine;
using System.Collections;
public class PreyRun : MonoBehaviour
{
    public float runSpeed = 8f;
    public LayerMask wallLayer;
    public Vector2 runDirection;

    private bool isRunning = false;
    private Collider2D col;
    private Animator anim;
    private SpriteRenderer visual;

    public bool IsRunning => isRunning;

    void Start()
    {
        col = GetComponent<Collider2D>();
        anim = GetComponentInChildren<Animator>();
        visual = GetComponentInChildren<SpriteRenderer>();
    }
    public void InitRun(Vector2 direction)
    {
        if (isRunning) return;
        isRunning = true;

        StopAllCoroutines(); // ← ESSENCIAL

        transform.position = RoundToGrid(transform.position);

        PreyLoop preyLoop = GetComponent<PreyLoop>();
        if (preyLoop != null)
            preyLoop.isActive = false;

        col.enabled = true;
        runDirection = direction.normalized;
        UpdateAnimationDirection(runDirection);
        StartCoroutine(RunAway());
    }

    IEnumerator RunAway()
    {
        while (isRunning)
        {
            Vector2 currentPos = RoundToGrid(transform.position);
            transform.position = currentPos; // <- CORRIGE a posição antes de começar a andar

            Vector2 nextPos = currentPos + runDirection;

            if (Physics2D.OverlapCircle(nextPos, 0.1f, wallLayer))
            {
                Disappear();
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

    void UpdateAnimationDirection(Vector2 direction)
    {
        if (visual != null)
            visual.flipX = direction.x > 0;
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
    }

    public void Disappear()
    {
        isRunning = false;

        if (visual != null)
            visual.enabled = false;

        Destroy(gameObject, 0.2f);
    }

}
