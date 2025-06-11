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

        if (holdCats == null)
            Debug.LogError("HoldCats script não encontrado no jogador!");
    }

    void Update()
    {
        if (isMoving)
            return;

        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        // Prioriza o eixo com maior valor absoluto (movimento 4 direções)
        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            input.y = 0;
        else
            input.x = 0;

        if (input != Vector2.zero)
            lastDirection = input.normalized;

        // Interações
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!pullBoxes.IsPulling)
            {
                pullBoxes.TryPullBox();
            }
            else
                pullBoxes.ReleaseBox();
            return;
        }

        if (Input.GetKeyDown(KeyCode.T))
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
            LayerMask blockedOrCollided = collisionLayer | LayerMask.GetMask("Travado");

            Vector2 gridCenter = new Vector2(RoundToGrid(newTargetPos.x), RoundToGrid(newTargetPos.y));
            bool isBlocked = Physics2D.OverlapCircle(gridCenter, 0.1f, blockedOrCollided);

            if (!isBlocked)
                StartCoroutine(MoveTo(newTargetPos));
            else
                Debug.Log("Movimento bloqueado por obstáculo.");
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
}