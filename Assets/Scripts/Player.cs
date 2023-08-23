using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Timers;
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
    [Header("컴포넌트")]
    [SerializeField] Rigidbody2D rb;
    bool inputX_p, inputX_m, inputY_p, inputY_m;
    Camera mainCamera;

    [Header("입력")]
    List<InputTimeLine> inputs = new List<InputTimeLine>();
    Dictionary<KeyCode, System.Action> doubleClickAction = new Dictionary<KeyCode, System.Action>();
    [SerializeField] float doubleClickTime;

    [Header("이동")]
    Vector2 inputVec;
    [SerializeField] float speed;

    [Header("마우스")]
    Vector2 lookVec;
    float angle;

    [Header("점프")]
    public int ableJumpCount;
    [SerializeField] int maxJumpCount;
    [SerializeField] float jumpForce;
    [SerializeField] Vector2 footSize, footOffset;
    float jumpDelay;
    [SerializeField] float minJumpDelay;

    [Header("대쉬")]
    bool isDashing;
    [SerializeField] float dashForce;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        DoubleClickSetup();
    }

    void FixedUpdate()
    {
        FixedMove();
        Landing();
    }
    private void LateUpdate()
    {
        DoubleClickUpdate();
    }
    void Update()
    {
        InputUpdate();
        Look();
        Jump(true);
    }
    void DoubleClickSetup()
    {
        doubleClickAction.Add(KeyCode.W, () => { });
        doubleClickAction.Add(KeyCode.A, () => Dash(false));
        doubleClickAction.Add(KeyCode.S, () => { });
        doubleClickAction.Add(KeyCode.D, () => Dash(true));
    }
    bool DoubleClickCheck(KeyCode key)
    {
        bool isDoubleClick = false;
        for (int i = 0; i < inputs.Count; i++)
        {
            if (inputs[i].key == key)
            {
                isDoubleClick = true;
                inputs.RemoveAt(i);
                break;
            }
        }
        if (isDoubleClick) {
            doubleClickAction[key].Invoke();
        }
        else
        {
            inputs.Add(new InputTimeLine { key = key, leftTime = doubleClickTime });
        }
        return isDoubleClick;
    }
    void InputUpdate()
    {
        inputVec = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        if (inputVec.x != 0)
        {
            if (inputVec.x < 0)
            {
                if (inputX_m == false)
                {
                    // 더블 클릭인지 확인하고 이미 눌렀으면 더블 클릭 발동하는 함수 제작하셈 
                    DoubleClickCheck(KeyCode.A);
                }
                else
                    UiManager.PlayerInputUI(-1, 1, -1, 0);
                inputX_m = true;
            }
            else
            {
                if (inputX_p == false)
                {
                    DoubleClickCheck(KeyCode.D);
                }
                else
                    UiManager.PlayerInputUI(-1, 0, -1, 1);
                inputX_p = true;
            }
        }
        else
        {
            UiManager.PlayerInputUI(-1, 0, -1, 0);
            inputX_p = false;
            inputX_m = false;
        }
        if (inputVec.y != 0)
        {
            if (inputVec.y < 0)
            {
                if (inputY_m == false)
                {
                    DoubleClickCheck(KeyCode.S);
                }
                else
                    UiManager.PlayerInputUI(0, -1, 1, -1);
                inputY_m = true;
            }
            else
            {
                if (inputY_p == false)
                {
                    Jump(false);
                    DoubleClickCheck(KeyCode.W);
                }
                else 
                    UiManager.PlayerInputUI(1, -1, 0, -1);
                inputY_p = true;
            }
        }
        else
        {
            UiManager.PlayerInputUI(0, -1, 0, -1);
            inputY_p = false;
            inputY_m = false;
        }
    }
    void DoubleClickUpdate() {
        if (inputs.Count > 0)
        {
            for (int i = inputs.Count - 1; i >= 0; i--)
            {
                if (inputs[i].leftTime > 0)//-= Time.deltaTime;{
                {
                    inputs[i] = new InputTimeLine { key = inputs[i].key, leftTime = inputs[i].leftTime - Time.deltaTime };

                    switch (inputs[i].key)
                    {
                        case KeyCode.W:
                            UiManager.PlayerInputUI(2, -1, -1, -1);
                            break;
                        case KeyCode.A:
                            UiManager.PlayerInputUI(-1, 2, -1, -1);
                            break;
                        case KeyCode.S:
                            UiManager.PlayerInputUI(-1, -1, 2, -1);
                            break;
                        case KeyCode.D:
                            UiManager.PlayerInputUI(-1, -1, -1, 2);
                            break;
                    }
                }
                else
                {
                    switch (inputs[i].key)
                    {
                        case KeyCode.W:
                            UiManager.PlayerInputUI(0, -1, -1, -1);
                            break;
                        case KeyCode.A:
                            UiManager.PlayerInputUI(-1, 0, -1, -1);
                            break;
                        case KeyCode.S:
                            UiManager.PlayerInputUI(-1, -1, 0, -1);
                            break;
                        case KeyCode.D:
                            UiManager.PlayerInputUI(-1, -1, -1, 0);
                            break;
                    }
                    inputs.RemoveAt(i);
                }
            }
        }
    }
    void FixedMove()
    {
        if (isDashing)
        {
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            if (rb.velocity.x >= -5f && rb.velocity.x <= 5f)
            {
                isDashing = false;
            }
            return;
        }
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
    void Jump(bool stateCheck)
    {
        if (stateCheck)
        {
            if (jumpDelay < 0)
            {
                jumpDelay = 0;
            }
            else
            {
                jumpDelay -= Time.deltaTime;
            }
            return;
        }
        if (ableJumpCount > 0)
        {
            if (ableJumpCount == maxJumpCount)
                jumpDelay = minJumpDelay;
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
    public void Dash(bool isRight)
    {
        float velocityY = rb.velocity.y;
        print("Dash-" + (isRight ? "right" : "left"));
        rb.velocity = new Vector2(dashForce * (isRight ? 1 : -1), velocityY);
        isDashing = true;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(rb.position + lookVec, 0.1f);
    }
    void Landing() // 착지를 검사하고 땅에있다면 점프 횟수를 추가하는 메소드
    {
        if (jumpDelay > 0)
            return;
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
