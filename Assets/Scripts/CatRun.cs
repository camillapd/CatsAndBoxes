using UnityEngine;
using System.Collections;

public class CatRun : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask wallsLayer;
    public LayerMask boxLayer;
    public LayerMask outsideLayer;

    private Vector2 runDirection;
    private bool runningAway = false;
    private GameManager GM;
    private Animator anim;

    public void InitRun(Vector2 direction)
    {
        GM = Object.FindAnyObjectByType<GameManager>();

        runDirection = direction.normalized;
        runningAway = true;

        GetComponent<Collider2D>().enabled = true;
        GetComponent<SpriteRenderer>().enabled = true;
        
        gameObject.layer = LayerMask.NameToLayer("Default");
        GetComponent<SpriteRenderer>().sortingOrder = 5;

        anim = GetComponent<Animator>();
        if (anim != null)
        {
            anim.SetBool("isRunning", true);
            anim.SetInteger("direction", DirectionToInt(runDirection));
        }

        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        while (runningAway)
        {
            Vector2 nextPos = (Vector2)transform.position + runDirection;

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

                runningAway = false;
                gameObject.layer = LayerMask.NameToLayer("Gatos");
                Debug.Log("🐾 Gato parou ao bater na parede e pode ser pego de novo!");
                if (anim != null) anim.SetBool("isRunning", false);
                yield break;
            }

            Collider2D boxCol = Physics2D.OverlapCircle(nextPos, 0.1f, boxLayer);

            if (boxCol != null)
            {
                BoxState boxState = boxCol.GetComponent<BoxState>();

                if (boxState != null && boxState.hasCatInside)
                {
                    runningAway = false;
                    gameObject.layer = LayerMask.NameToLayer("Gatos");
                    Debug.Log("🐾 Gato encontrou caixa já ocupada e parou antes.");
                    if (anim != null) anim.SetBool("isRunning", false);
                    yield break;
                }

                // Se chegou aqui, significa que a caixa não está ocupada, pode continuar
                Vector2 initPos = transform.position;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * moveSpeed;
                    transform.position = Vector2.Lerp(initPos, nextPos, t);
                    yield return null;
                }
                transform.position = nextPos;

                runningAway = false;
                gameObject.layer = LayerMask.NameToLayer("Gatos");

                boxState.hasCatInside = true;

                CatState catState = GetComponent<CatState>();
                if (catState != null)
                {
                    catState.isInsideBox = true;
                    Debug.Log($"🐾 Gato entrou na caixa! isInsideBox = {catState.isInsideBox}");
                }

                if (anim != null)
                {
                    anim.SetBool("isRunning", false);
                    anim.SetBool("isOnBox", true);
                }

                GM.CheckVictory();

                yield break;
            }
            else
            {
                // Se não tem caixa, o gato pode continuar correndo normalmente (sem parar)
                Vector2 initPos = transform.position;
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * moveSpeed;
                    transform.position = Vector2.Lerp(initPos, nextPos, t);
                    yield return null;
                }
                transform.position = nextPos;
            }
        }
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
