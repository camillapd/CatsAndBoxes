using UnityEngine;
using System.Collections;

public class CatGetPrey : MonoBehaviour
{
    public float chaseSpeed = 2f;
    public LayerMask wallsLayer;
    public LayerMask outsideLayer;

    private Transform preyTransform;
    private bool chasingPrey;
    private GameManager GM;
    private Animator anim;
    private Vector2 runDirection; // Guarda a direção atual do movimento

    public void InitChase(Transform prey)
    {
        GM = Object.FindAnyObjectByType<GameManager>();

        Debug.Log("🐱 InitChase foi chamado!");
        transform.position = RoundToGrid(transform.position);
        preyTransform = prey;
        chasingPrey = true;
        StopAllCoroutines();
        StartCoroutine(RunAfterPrey());

        anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", true);
            anim.SetInteger("direction", DirectionToInt(runDirection));
        }
    }

    IEnumerator RunAfterPrey()
    {
        yield return new WaitForSeconds(0.1f);

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
            Debug.Log($"🐱 Cat at {catPos}, prey at {preyPos}");

            Vector2 dir = preyPos - catPos;

            if (dir == Vector2.zero)
            {
                chasingPrey = false;
                if (anim != null) anim.SetBool("isRunning", false);
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

            // Atualiza a direção do movimento para animação
            runDirection = moveDir;
            if (anim != null)
            {
                anim.SetInteger("direction", DirectionToInt(runDirection));
            }

            Vector2 nextPos = catPos + moveDir;
            Debug.Log($"🐱 Tentando mover para {nextPos} (direção {moveDir})");
            Collider2D walls = Physics2D.OverlapCircle(nextPos, 0.1f, wallsLayer);

            if (walls != null)
            {
                Collider2D outside = Physics2D.OverlapCircle(nextPos, 0.1f, outsideLayer);

                if (outside != null)
                {
                    Debug.Log("💨 O gato fugiu pela " + outside.name + "!");
                    if (anim != null) anim.SetBool("isRunning", false);
                    Destroy(gameObject);
                    GM.GameOver();
                    yield break;
                }

                chasingPrey = false;
                if (anim != null) anim.SetBool("isRunning", false);
                Debug.Log("🐾 Gato parou de perseguir.");
                GM.CheckVictory();
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

    private int DirectionToInt(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            return dir.x > 0 ? 3 : 2; // direita : esquerda
        }
        else
        {
            return dir.y > 0 ? 0 : 1; // cima : baixo
        }
    }
}
