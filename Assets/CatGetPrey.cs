using UnityEngine;
using System.Collections;

public class CatGetPrey : MonoBehaviour
{
    public float chaseSpeed = 3f; 
    public LayerMask wallsLayer;

    private Transform mouseTransform;
    private bool chasingMouse;

    void Start()
    {
        // Opcional: cache componentes se necessário
    }

    public void InitChase(Transform mouse)
    {
        mouseTransform = mouse;
        chasingMouse = true;
        StopAllCoroutines();
        StartCoroutine(RunAfterMouse());
    }

    IEnumerator RunAfterMouse()
    {
        Vector2[] dirs = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

        while (chasingMouse)
        {
            if (mouseTransform == null)
            {
                chasingMouse = false;
                yield break;
            }

            Vector2 catPos = RoundToGrid(transform.position);
            Vector2 mousePos = RoundToGrid(mouseTransform.position);

            Vector2 dir = (mousePos - catPos).normalized;

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

            if (Physics2D.OverlapCircle(nextPos, 0.1f, wallsLayer) || Vector2.Distance(nextPos, mousePos) < 1f)
            {
                chasingMouse = false;
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
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * chaseSpeed;
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
