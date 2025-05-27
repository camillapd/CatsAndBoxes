using UnityEngine;
using System.Collections;

public class CatGetPrey : MonoBehaviour
{
    public float chaseSpeed = 2f;
    public LayerMask wallsLayer;

    private Transform preyTransform;
    private bool chasingPrey;

    public void InitChase(Transform prey)
    {
        Debug.Log("🐱 InitChase foi chamado!");
        transform.position = RoundToGrid(transform.position);
        preyTransform = prey;
        chasingPrey = true;
        StopAllCoroutines();
        StartCoroutine(RunAfterPrey());
    }

    IEnumerator RunAfterPrey()
    {
        yield return new WaitForSeconds(0.5f);

        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        while (chasingPrey)
        {
            if (preyTransform == null)
            {
                chasingPrey = false;
                yield break;
            }

            Vector2 catPos = RoundToGrid(transform.position);
            Vector2 preyPos = RoundToGrid(preyTransform.position);
            Debug.Log($"🐱 Cat at {catPos}, prey at {preyPos}");

            Vector2 dir = preyPos - catPos;

            if (dir == Vector2.zero)
            {
                chasingPrey = false;
                Debug.Log("🐾 Gato chegou na presa, parando perseguição.");
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

            Vector2 nextPos = catPos + moveDir;
            Debug.Log($"🐱 Tentando mover para {nextPos} (direção {moveDir})");


            if (Physics2D.OverlapCircle(nextPos, 0.1f, wallsLayer))
            {
                chasingPrey = false;
                Debug.Log("🐾 Gato parou de perseguir.");
                yield break;
            }

            yield return StartCoroutine(Mover(moveDir));
        }
    }

    IEnumerator Mover(Vector2 direction)
    {
        Vector2 startPos = transform.position;
        Vector2 endPos = startPos + direction;
        float duration = 1f / chaseSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
    }

    Vector2 RoundToGrid(Vector2 pos)
    {
        return new Vector2(Mathf.Floor(pos.x) + 0.5f, Mathf.Floor(pos.y) + 0.5f);
    }
}
