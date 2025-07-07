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
            return;
        }

        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 boxSize = new Vector2(0.8f, 0.8f);

        foreach (var dir in directions)
        {
            Vector2 checkPos = (Vector2)transform.position + dir;
            Collider2D boxCol = Physics2D.OverlapBox(checkPos, boxSize, 0f, boxLayer);

            if (boxCol != null)
            {
                BoxState boxState = boxCol.GetComponent<BoxState>();

                if (boxState != null && boxState.hasCatInside)
                {
                    animator.SetTrigger("tryPullBlocked");
                    return;
                }

                if (isPulling && pulledBox != null && pulledBox != boxCol.transform)
                {
                    ReleaseBox(); 
                }

                pulledBox = boxCol.transform;
                isPulling = true;
                animator.SetBool("isPullingIdle", true);
                return;
            }
        }
    }

    public void ReleaseBox()
    {
        isPulling = false;
        pulledBox = null;
        animator.SetBool("isPullingIdle", false);
    }

    public void HandleBoxMovement(Vector2 input, MonoBehaviour caller)
    {
        if (!isPulling || isMoving || pulledBox == null)
            return;

        Vector2 relativeDir = ((Vector2)pulledBox.position - (Vector2)transform.position).normalized;
        Vector2 moveDir = input;

        // Forçar movimento só em um eixo (horizontal ou vertical)
        if (Mathf.Abs(moveDir.x) > 0) moveDir.y = 0;
        else if (Mathf.Abs(moveDir.y) > 0) moveDir.x = 0;
        moveDir = moveDir.normalized;

        // Checa se movimento é contra a direção da caixa (puxar)
        if (Vector2.Dot(moveDir, -relativeDir) > 0.9f)
        {
            Vector3 newTargetPos = transform.position + new Vector3(moveDir.x, moveDir.y, 0);
            Vector3 newBoxPos = pulledBox.position + new Vector3(moveDir.x, moveDir.y, 0);
            Vector2 boxSize = new Vector2(0.8f, 0.8f);

            Collider2D[] hits = Physics2D.OverlapBoxAll(newBoxPos, boxSize, 0f, collisionLayer | boxLayer);

            bool canMovePlayer = !Physics2D.OverlapBox(newTargetPos, boxSize, 0f, collisionLayer);
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
                animator.SetBool("isPullingIdle", false);
                caller.StartCoroutine(MoveWithBox(newTargetPos, newBoxPos));
            }
            else
            {
                animator.SetBool("isPullingIdle", true);
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
        animator.SetBool("isPullingIdle", false);

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

        transform.position = RoundToGridVector3(playerDest);
        pulledBox.position = RoundToGridVector3(boxDest);

        isMoving = false;
        GetComponent<PlayerController>()?.CheckIfOnWater();

        animator.SetBool("isPullingBox", false);
        animator.SetBool("isPullingIdle", isPulling);
    }

    // Ajusta um Vector3 para o centro do tile (assumindo grid 1x1 com offset 0.5)
    private Vector3 RoundToGridVector3(Vector3 pos)
    {
        float x = Mathf.Floor(pos.x) + 0.5f;
        float y = Mathf.Floor(pos.y) + 0.5f;
        return new Vector3(x, y, pos.z);
    }

}
