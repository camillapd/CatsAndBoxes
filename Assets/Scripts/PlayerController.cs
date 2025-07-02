using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask collisionLayer;
    public Transform visual;

    private bool isMoving = false;
    private Vector2 input;
    private Vector2 lastDirection = Vector2.down;
    private Vector3 targetPos;

    private PullBoxes pullBoxes;
    private HoldCats holdCats;
    private Animator animator;

    void Start()
    {
        pullBoxes = GetComponent<PullBoxes>();
        holdCats = GetComponent<HoldCats>();

        if (visual != null)
            animator = visual.GetComponent<Animator>();

        transform.position = new Vector3(
            RoundToGrid(transform.position.x),
            RoundToGrid(transform.position.y),
            transform.position.z
        );
    }

    void Update()
    {

        if (GameManager.isGameOver) return;

        if (isMoving) return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Prioriza o eixo com maior valor absoluto (movimento 4 direções)
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            input.y = 0;
        else
            input.x = 0;

        if (input != Vector2.zero)
            lastDirection = input.normalized;

        // Interações
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (!pullBoxes.IsPulling)
            {
                pullBoxes.TryPullBox();
            }
            else
                pullBoxes.ReleaseBox();
            return;
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            if (holdCats != null)
            {
                if (!holdCats.IsHoldingCat())
                    holdCats.TryHoldCat();
                else
                    holdCats.ReleaseCat();
            }
            return;
        }

        // Cálculo de destino
        Vector3 currentPos = new Vector3(
            RoundToGrid(transform.position.x),
            RoundToGrid(transform.position.y),
            transform.position.z
        );

        Vector3 newTargetPos = currentPos + new Vector3(input.x, input.y, 0);

        if (pullBoxes != null && pullBoxes.IsPulling)
        {
            pullBoxes.HandleBoxMovement(input, this);
        }
        else
        {
            LayerMask blockedOrCollided = collisionLayer;

            Vector2 gridCenter = new Vector2(RoundToGrid(newTargetPos.x), RoundToGrid(newTargetPos.y));
            Vector2 boxSize = new Vector2(0.8f, 0.8f); // ou 1.0f, se quiser cobrir exatamente um tile inteiro

            bool isBlocked = Physics2D.OverlapBox(gridCenter, boxSize, 0f, blockedOrCollided);

            if (!isBlocked)
                StartCoroutine(MoveTo(newTargetPos));

            // else
            //     Debug.Log("Movimento bloqueado por obstáculo.");
        }

        // Atualiza animação e visual
        if (animator != null)
            animator.SetBool("isMoving", input != Vector2.zero);

        UpdateVisualDirection(lastDirection);

        if (holdCats != null)
            holdCats.NotifyMoveIntent(lastDirection);
    }

    IEnumerator MoveTo(Vector3 destination)
    {
        if (!IsTileFree(destination))
        {
            Debug.Log("Movimento bloqueado! Tile ocupado por rato.");
            yield break; // cancela o movimento
        }

        isMoving = true;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                destination,
                moveSpeed * Time.deltaTime
            );
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

        if (holdCats != null)
            holdCats.NotifyArrived();
    }

    float RoundToGrid(float value)
    {
        return Mathf.Floor(value) + 0.5f;
    }

    // 👇 Método para girar ou espelhar o visual com base na direção
    void UpdateVisualDirection(Vector2 dir)
    {
        if (dir == Vector2.zero || visual == null)
            return;

        visual.localScale = Vector3.one;
        visual.rotation = Quaternion.identity;

        if (dir.x < 0)
        {
            visual.localScale = new Vector3(-1, 1, 1); // esquerda = espelhado
        }
        else if (dir.x > 0)
        {
            visual.localScale = new Vector3(1, 1, 1); // direita = normal
        }
        else if (dir.y > 0)
        {
            visual.rotation = Quaternion.Euler(0, 0, 360); // cima = 90°
        }
        else if (dir.y < 0)
        {
            visual.localScale = new Vector3(-1, 1, 1); // esquerda = espelhado
            visual.rotation = Quaternion.Euler(0, 0, 360); // baixo = -90°
        }
    }

    bool IsTileFree(Vector3 pos)
    {
        // Ajuste o raio conforme seu grid/tamanho do prey
        float checkRadius = 0.2f;

        // LayerMask do prey (ratos)
        LayerMask preyLayer = LayerMask.GetMask("Prey");

        Collider2D hit = Physics2D.OverlapCircle(pos, checkRadius, preyLayer);
        return hit == null;
    }

}