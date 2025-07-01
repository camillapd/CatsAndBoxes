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
        Vector2 boxSize = new Vector2(0.8f, 0.8f); // ou 1f

        while (runningAway)
        {
            // 👉 Recalcula a cada frame, a partir da posição atual
            Vector2 nextPos = (Vector2)transform.position + runDirection;
            Collider2D walls = Physics2D.OverlapBox(nextPos, boxSize, 0f, wallsLayer);

            // Se bateu numa parede ou saiu da arena
            if (walls != null)
            {
                Collider2D outside = Physics2D.OverlapBox(nextPos, boxSize, 0f, outsideLayer);
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

            // Se encontrou caixa ocupada no próximo passo, para antes de mover
            Collider2D boxCol = Physics2D.OverlapBox(nextPos, boxSize, 0f, boxLayer);
            if (boxCol != null && boxCol.GetComponent<BoxState>()?.hasCatInside == true)
            {
                runningAway = false;
                gameObject.layer = LayerMask.NameToLayer("Gatos");
                Debug.Log("🐾 Gato encontrou caixa já ocupada e parou antes.");
                if (anim != null) anim.SetBool("isRunning", false);
                yield break;
            }

            // Se chegou aqui, o tile está livre: faz a interpolação para nextPos
            Vector2 initPos = transform.position;
            float t = 0f;
            while (t < 1f)
            {
                t += Time.deltaTime * moveSpeed;
                transform.position = Vector2.Lerp(initPos, nextPos, t);
                yield return null;
            }
            transform.position = nextPos;

            // Se entrou numa caixa livre, marca vitória
            if (boxCol != null)
            {
                BoxState boxState = boxCol.GetComponent<BoxState>();
                runningAway = false;
                gameObject.layer = LayerMask.NameToLayer("Gatos");

                boxState.hasCatInside = true;
                var catState = GetComponent<CatState>();
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

            // Continua fugindo até encontrar parede ou caixa
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
