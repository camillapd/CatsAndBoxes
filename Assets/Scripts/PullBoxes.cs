using UnityEngine;
using System.Collections;

public class PullBoxes : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask collisionLayer;
    public LayerMask boxLayer;
    public bool IsPulling => isPulling;
    public Transform visual;
    public HoldCats holdCats;

    private bool isMoving = false;
    private bool isPulling = false;
    private Transform pulledBox = null;
    private Animator animator;

    void Start()
    {
        holdCats = GetComponent<HoldCats>();

        if (visual != null)
            animator = visual.GetComponent<Animator>();

    }

    public void TryPullBox()
    {
        if (holdCats != null && holdCats.IsHoldingCat())
        {
            Debug.Log("❌ Não é possível puxar caixa carregando um gato.");
            return;
        }

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir;
            Collider2D boxCol = Physics2D.OverlapCircle(checkPos, 0.1f, boxLayer);

            if (boxCol != null)
            {
                BoxState boxState = boxCol.GetComponent<BoxState>();

                if (boxState != null && boxState.hasCatInside)
                {
                    animator.SetTrigger("tryPullBlocked");
                    Debug.Log("❌ Essa caixa está ocupada por um gato. Não pode puxar.");
                    return;
                }

                // Caixa livre pra puxar
                pulledBox = boxCol.transform;
                isPulling = true;
                Debug.Log("Caixa agarrada!");
                animator.SetBool("isPullingIdle", true);
                return;
            }
        }
        Debug.Log("Nenhuma caixa próxima para puxar.");
    }

    public void ReleaseBox()
    {
        isPulling = false;
        pulledBox = null;
        animator.SetBool("isPullingIdle", false);
        Debug.Log("Caixa solta.");
    }

    public void HandleBoxMovement(Vector2 input, MonoBehaviour caller)
    {
        if (!isPulling || isMoving || pulledBox == null)
            return;

        Vector2 relativeDir = ((Vector2)pulledBox.position - (Vector2)transform.position).normalized;
        Vector2 moveDir = input;

        if (Mathf.Abs(moveDir.x) > 0) moveDir.y = 0;
        else if (Mathf.Abs(moveDir.y) > 0) moveDir.x = 0;
        moveDir = moveDir.normalized;

        if (Vector2.Dot(moveDir, -relativeDir) > 0.9f)
        {
            Vector3 newTargetPos = transform.position + new Vector3(moveDir.x, moveDir.y, 0);
            Vector3 newBoxPos = pulledBox.position + new Vector3(moveDir.x, moveDir.y, 0);
            Collider2D[] hits = Physics2D.OverlapCircleAll(newBoxPos, 0.1f, collisionLayer | boxLayer);

            bool canMovePlayer = !Physics2D.OverlapCircle(newTargetPos, 0.1f, collisionLayer);
            bool canMoveBox = true;

            foreach (var hit in hits)
            {
                if (hit.transform == pulledBox)
                    continue;
                canMoveBox = false;
                break;
            }

            if (canMovePlayer && canMoveBox)
            {
                caller.StartCoroutine(MoveWithBox(newTargetPos, newBoxPos));
            }
            else
            {
                Debug.Log("Movimento bloqueado por obstáculo ou caixa.");
            }
        }
        else
        {
            animator.SetBool("isPullingIdle", true);
        }
    }

    IEnumerator MoveWithBox(Vector3 playerDest, Vector3 boxDest)
    {
        isMoving = true;
        animator.SetBool("isPullingBox", true);
        Vector3 startPlayerPos = transform.position;
        Vector3 startBoxPos = pulledBox.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * moveSpeed;
            transform.position = Vector3.Lerp(startPlayerPos, playerDest, t);
            pulledBox.position = Vector3.Lerp(startBoxPos, boxDest, t);
            yield return null;
        }

        transform.position = playerDest;
        pulledBox.position = boxDest;
        isMoving = false;
        animator.SetBool("isPullingBox", false);
    }
}
