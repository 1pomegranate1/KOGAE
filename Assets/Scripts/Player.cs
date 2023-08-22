using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
struct InputTimeLine
{
    public KeyCode key;
    public float leftTime;
}
public class Player : MonoBehaviour
{
    [Header("������Ʈ")]
    [SerializeField] Rigidbody2D rb;
    Camera mainCamera;

    [Header("�Է�")]
    List<InputTimeLine> inputs = new List<InputTimeLine>();

    [Header("�̵�")]
    Vector2 inputVec;
    [SerializeField] float speed;

    [Header("���콺")]
    Vector2 lookVec;
    float angle;

    [Header("����")]
    public int ableJumpCount;
    [SerializeField] int maxJumpCount;
    [SerializeField] float jumpForce;
    [SerializeField] Vector2 footSize,footOffset;

    [Header("����")]
    bool isDashing;
    float dashForce;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    void FixedUpdate()
    {
        FixedMove();
        Landing();
    }
    void Update()
    {
        InputUpdate();
        Look();
        Jump();
    }
    void InputUpdate()
    {
        inputVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        for (int i = inputs.Count; i < 0; i--)
        {
            if(inputs[i].leftTime > 0 )//-= Time.deltaTime;{
            {
                inputs[i] = new InputTimeLine { key = inputs[i].key, leftTime = inputs[i].leftTime - Time.deltaTime };
            }
            else
            {
                inputs.RemoveAt(i);
            }

        }
    }
    void FixedMove()
    {
        float velocityY = rb.velocity.y;
        if (inputVec.x != 0)
        {
            rb.velocity = new Vector2(
                inputVec.x * speed * Time.fixedDeltaTime,
                velocityY   
                );
        }
        else
            rb.velocity = Vector2.up * velocityY;
    }
    void Jump()
    {
        if (inputVec.y > 0 && ableJumpCount > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            ableJumpCount--;
        }
    }
    void Look()
    {
        lookVec = ((Vector2)mainCamera.ScreenToWorldPoint(Input.mousePosition) - rb.position).normalized;
        angle = lookVec.ToDegree() - 90;
        UiManager.PlayerAngleUI(angle);
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(rb.position + lookVec, 0.1f);
    }
    void Landing() // ������ �˻��ϰ� �����ִٸ� ���� Ƚ���� �߰��ϴ� �޼ҵ�
    {
        Collider2D[] underHits = Physics2D.OverlapCapsuleAll(rb.position + footOffset, footSize, CapsuleDirection2D.Horizontal, 0);
        if (underHits.Length > 0) {
            bool onGround = false;
            for (int i = 0; i < underHits.Length; i++)
            {
                if (underHits[i].CompareTag("Floor"))
                {
                    onGround = true;
                    break;
                }
            }
            if (onGround)
            {
                ableJumpCount = maxJumpCount;
            }

        }
    }
    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
