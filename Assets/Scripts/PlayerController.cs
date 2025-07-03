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

        // Alinha posição ao grid na inicialização
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
                pullBoxes.TryPullBox();
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

        Vector3 currentPos = new Vector3(
            RoundToGrid(transform.position.x),
            RoundToGrid(transform.position.y),
            transform.position.z
        );

        Vector3 newTargetPos = currentPos + new Vector3(input.x, input.y, 0);

        // Verifica se o movimento cruza com o rato (prey)
        bool blockedByPrey = false;
        if (holdCats != null && holdCats.preyObject != null)
        {
            var preyObj = holdCats.preyObject;
            var preyLoop = preyObj.GetComponentInChildren<PreyLoop>();

            if (preyLoop != null)
            {
                Vector3 preyPos = preyObj.transform.position;
                Vector3 preyNextPos = preyPos + (Vector3)preyLoop.chosenDir;

                blockedByPrey = IsCrossingMovement(currentPos, newTargetPos, preyPos, preyNextPos);

                if (blockedByPrey)
                    Debug.Log($"Movimento bloqueado: Jogador {currentPos}→{newTargetPos}, Rato {preyPos}→{preyNextPos}");
            }
        }

        if (pullBoxes != null && pullBoxes.IsPulling)
        {
            pullBoxes.HandleBoxMovement(input, this);
        }
        else
        {
            Vector2 gridCenter = new Vector2(RoundToGrid(newTargetPos.x), RoundToGrid(newTargetPos.y));
            Vector2 boxSize = new Vector2(0.8f, 0.8f);

            bool isBlocked = Physics2D.OverlapBox(gridCenter, boxSize, 0f, collisionLayer);

            if (!isBlocked && !blockedByPrey)
                StartCoroutine(MoveTo(newTargetPos));
        }

        // Atualiza animação e visual
        if (animator != null)
            animator.SetBool("isMoving", input != Vector2.zero);

        UpdateVisualDirection(lastDirection);

        if (holdCats != null)
            holdCats.NotifyMoveIntent(lastDirection);
    }

    bool IsCrossingMovement(Vector3 playerCurrent, Vector3 playerNext, Vector3 preyCurrent, Vector3 preyNext)
    {
        // Troca simples de posição
        if (playerNext == preyCurrent && preyNext == playerCurrent)
            return true;

        // Mesmo eixo X, distância 1 tile, movendo-se verticalmente em direções opostas
        if (Mathf.Approximately(playerCurrent.x, preyCurrent.x))
        {
            float distY = Mathf.Abs(playerCurrent.y - preyCurrent.y);
            if (Mathf.Approximately(distY, 1f))
            {
                float playerDirY = playerNext.y - playerCurrent.y;
                float preyDirY = preyNext.y - preyCurrent.y;
                if (playerDirY * preyDirY < 0)
                    return true;
            }
        }

        // Mesmo eixo Y, distância 1 tile, movendo-se horizontalmente em direções opostas
        if (Mathf.Approximately(playerCurrent.y, preyCurrent.y))
        {
            float distX = Mathf.Abs(playerCurrent.x - preyCurrent.x);
            if (Mathf.Approximately(distX, 1f))
            {
                float playerDirX = playerNext.x - playerCurrent.x;
                float preyDirX = preyNext.x - preyCurrent.x;
                if (playerDirX * preyDirX < 0)
                    return true;
            }
        }

        return false;
    }

    IEnumerator MoveTo(Vector3 destination)
    {
        if (!IsTileFree(destination))
        {
            Debug.Log("Movimento bloqueado! Tile ocupado por rato.");
            yield break;
        }

        isMoving = true;

        while (Vector3.Distance(transform.position, destination) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
        isMoving = false;

        if (holdCats != null)
            holdCats.NotifyArrived();
    }

    float RoundToGrid(float value) => Mathf.Floor(value) + 0.5f;

    void UpdateVisualDirection(Vector2 dir)
    {
        if (dir == Vector2.zero || visual == null)
            return;

        visual.localScale = Vector3.one;
        visual.rotation = Quaternion.identity;

        if (dir.x < 0)
            visual.localScale = new Vector3(-1, 1, 1);
        else if (dir.x > 0)
            visual.localScale = new Vector3(1, 1, 1);
        else if (dir.y > 0)
            visual.rotation = Quaternion.Euler(0, 0, 360); // rotaciona para cima
        else if (dir.y < 0)
        {
            visual.localScale = new Vector3(-1, 1, 1);
            visual.rotation = Quaternion.Euler(0, 0, 360); // rotaciona para baixo
        }
    }

    bool IsTileFree(Vector3 pos)
    {
        float checkRadius = 0.4f;
        LayerMask preyLayer = LayerMask.GetMask("Prey");
        Collider2D hit = Physics2D.OverlapCircle(pos, checkRadius, preyLayer);
        return hit == null;
    }
}
