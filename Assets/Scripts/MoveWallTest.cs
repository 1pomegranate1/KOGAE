using UnityEngine;
using System;

public class MoveWallTest : MonoBehaviour
{
    [SerializeField] AnimationCurve smoothCurve;
    [SerializeField] float maxMoveTime;
    [SerializeField] Transform movePoint1, movePoint2;
    [SerializeField]Rigidbody2D rb;
    Player player;
    bool moveToOne = true, onPlayer = false;
    float moveTime;

    private Vector3 oldPosition;
    private Vector3 currentPosition;
    Vector3 dir;
    private double velocity;

    private void Start()
    {
        moveTime = maxMoveTime;
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        currentPosition = transform.position;
        var dis = (currentPosition - oldPosition);
        var distance = Math.Sqrt(Math.Pow(dis.x, 2) + Math.Pow(dis.y, 2) + Math.Pow(dis.z, 2));
        velocity = distance / Time.deltaTime;

        dir = (currentPosition - oldPosition).normalized;

        oldPosition = currentPosition;

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

        if (onPlayer)
        {
            player.plusmove = dir * (float)velocity;
        }
    }

    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player = collision.gameObject.GetComponent<Player>();

            onPlayer = true;
        }
    }
    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            player.plusmove = Vector2.zero;

            onPlayer = false;
        }
    }
}
