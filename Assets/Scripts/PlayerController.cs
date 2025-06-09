using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public LayerMask collisionLayer;

    private bool isMoving = false;
    private Vector2 input;
    private Vector2 lastDirection = Vector2.down;
    private Vector3 targetPos;

    private PullBoxes pullBoxes;
    private HoldCats holdCats;

    void Start()
    {
        pullBoxes = GetComponent<PullBoxes>();
        holdCats = GetComponent<HoldCats>();

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

    if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
        input.y = 0;
    else
        input.x = 0;

    if (input != Vector2.zero)
        lastDirection = input.normalized;

    if (Input.GetKeyDown(KeyCode.E))
    {
        if (!pullBoxes.IsPulling)
            pullBoxes.TryPullBox();
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
}