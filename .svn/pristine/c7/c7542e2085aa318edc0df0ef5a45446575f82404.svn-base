using UnityEngine;

public class MoveWallTest : MonoBehaviour
{
    [SerializeField] AnimationCurve smoothCurve;
    [SerializeField] float maxMoveTime;
    [SerializeField] Transform movePoint1, movePoint2;
    [SerializeField]Rigidbody2D rb;
    bool moveToOne = true;
    float moveTime;

    private void Start()
    {
        moveTime = maxMoveTime;
    }
    private void FixedUpdate()
    {
        if (moveToOne)
        {
            rb.MovePosition(Vector2.Lerp(movePoint2.position, movePoint1.position, smoothCurve.Evaluate(moveTime / maxMoveTime)));
        }
        else
        {
            rb.MovePosition(Vector2.Lerp(movePoint1.position, movePoint2.position, smoothCurve.Evaluate(moveTime / maxMoveTime)));
        }
        if (moveTime < 0)
        {
            moveTime = maxMoveTime;
            moveToOne = !moveToOne;
        }
        else
            moveTime -= Time.fixedDeltaTime;
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
