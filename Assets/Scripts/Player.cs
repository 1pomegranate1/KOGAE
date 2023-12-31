using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
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
    [SerializeField] SpriteRenderer spriteRender;
    [SerializeField] Animator anim;
    bool inputX_p, inputX_m, inputY_p, inputY_m;
    Camera mainCamera;

    [Header("입력")]
    [SerializeField] float doubleClickTime;
    List<InputTimeLine> inputs = new List<InputTimeLine>();
    Dictionary<KeyCode, System.Action> doubleClickAction = new Dictionary<KeyCode, System.Action>();

    [Header("이동")]
    [SerializeField] float speed;
    Vector2 inputVec;
    bool playerLookRight;
    float gravityScale;

    [Header("마우스")]
    Vector2 lookVec;
    float angle;

    [Header("점프")]
    [SerializeField] int maxJumpCount;
    public int ableJumpCount;
    [SerializeField] float jumpForce;
    [SerializeField] Vector2 footSize, footOffset;
    float jumpDelay;
    [SerializeField] float minJumpDelay;
    bool onGround;
    Transform groundTransform;
    Vector2 groundOffset;

    [Header("대쉬")]
    [SerializeField] float dashForce;
    bool isDashing;
    Vector2 dashVelocity;
    [SerializeField] float dashDrag;

    [Header("벽잡기")]
    [SerializeField] bool isWallHold;
    [SerializeField] Transform handColiderHint;
    [SerializeField] float targetWallSlideVelocity, wallSlideLerpMulti;
    Transform holdedTransform;
    [SerializeField] Vector2 handSize, handOffset;

    [Header("애니메이션")]
    [SerializeField] string currentState;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        gravityScale = rb.gravityScale;
        DoubleClickSetup();
    }

    void FixedUpdate()
    {
        FixedMove();
        Landing();
        WallHold();
    }
    private void LateUpdate()
    {
        DoubleClickUpdate();
        UiManager.PlayerVelocityUI(rb.velocity.y);
        handColiderHint.localPosition = handOffset;
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
        if (isWallHold)
        {
            if (onGround)
            {
                isWallHold = false;
            }
            rb.gravityScale = 0;
            TryStateChange("Grap");
        }
        else
            rb.gravityScale = gravityScale;

        if (isDashing)
        {
            TryStateChange("Run");
            anim.SetFloat("runSpeed", Mathf.Abs(rb.velocity.x) / (speed * Time.fixedDeltaTime));
            dashVelocity.x = Mathf.Lerp(dashVelocity.x, 0, Time.fixedDeltaTime * dashDrag);
            rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            Debug.Log(rb.velocity);
            if (rb.velocity.x >= -5f && rb.velocity.x <= 5f)
            {
                isDashing = false;
            }
            return;
        }
        else
            anim.SetFloat("runSpeed", 1);

        float velocityY = isWallHold ? Mathf.Lerp(rb.velocity.y, targetWallSlideVelocity, Time.fixedDeltaTime * wallSlideLerpMulti) : rb.velocity.y;
        if (inputVec.x != 0)
        {
            if (isWallHold == false)
                TryStateChange("Run");
            rb.velocity = new Vector2(
                inputVec.x * speed * Time.fixedDeltaTime,
                velocityY
                );
            if (rb.velocity.x > 0)
                PlayerLookDirectionChange(true);
            else
                PlayerLookDirectionChange(false);   
        }
        else
        {
            TryStateChange("Idle");
            rb.velocity = Vector2.up * velocityY;
        }
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
        if (isWallHold)
        {
            rb.velocity = new Vector2(rb.velocity.x, 3);
            Dash(!playerLookRight, true);
            return;
        }
        else if (ableJumpCount > 0)
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
    void PlayerLookDirectionChange(bool isRight)
    {
            playerLookRight = isRight;
            spriteRender.flipX = !isRight;
    }
    public void Dash(bool isRight, bool isWallClimbing = false)
    {
        float velocityY = rb.velocity.y;
        print("Dash-" + (isRight ? "right" : "left"));
        rb.velocity = new Vector2(dashForce * (isRight ? 1 : -1) * (isWallClimbing ? 0.6f : 1), velocityY);
        dashVelocity = new Vector2(dashForce * (isRight ? 1 : -1) * (isWallClimbing ? 0.6f : 1), velocityY);
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
        Collider2D[] groundHits = Physics2D.OverlapCapsuleAll(rb.position + footOffset, footSize, CapsuleDirection2D.Horizontal, 0);
        if (groundHits.Length > 0) {
            onGround = false;
            for (int i = 0; i < groundHits.Length; i++)
            {
                if(groundTransform != null)
                {
                    if (groundHits[i].transform == groundTransform)
                    {
                        onGround = true;
                        break;
                    }
                }
                else
                {
                    if (groundHits[i].CompareTag("Floor"))
                    {
                        groundTransform = groundHits[i].transform;
                     
                        onGround = true;
                        break;
                    }
                }
            }
            if (onGround)
            {
                /*
                if (groundOffset != Vector2.zero)
                {
                    if (groundOffset != (Vector2)(groundTransform.position - transform.position))
                    {
                        transform.position -= (Vector3)(groundOffset - (Vector2)(groundTransform.position - transform.position));
                    }
                }
                groundOffset = groundTransform.position - transform.position;
                */
                ableJumpCount = maxJumpCount;
            }
            else
            {
                groundOffset = Vector2.zero;
                groundTransform = null;
            }
        }
    }
    void WallHold()
    {
        if (isDashing)
        {
            if (rb.velocity.x > 0)
            {
                if (handOffset.x < 0) //오른쪽 보고있을때 손이 왼쪽에 있으면 반전
                    handOffset.x = -handOffset.x;
                spriteRender.flipX = false;
            }
            else
            {
                if (handOffset.x > 0) //왼쪽 보고있을때 손이 오른쪽에 있으면 반전
                    handOffset.x = -handOffset.x;
                spriteRender.flipX = true;
            }
        }
        else
        { 
            if (playerLookRight)
            {
                if (handOffset.x < 0) //오른쪽 보고있을때 손이 왼쪽에 있으면 반전
                    handOffset.x = -handOffset.x;
            }
            else
            {
                if (handOffset.x > 0) //왼쪽 보고있을때 손이 오른쪽에 있으면 반전
                    handOffset.x = -handOffset.x;
            }
        }

        Collider2D[] wallHits = Physics2D.OverlapCapsuleAll(rb.position + handOffset, handSize, CapsuleDirection2D.Vertical, 0);
        if (wallHits.Length > 0)
        {
            if (holdedTransform == null) 
            {
                if ((playerLookRight ? inputX_p : inputX_m) && onGround == false || isDashing)
                {
                    for (int i = 0; i < wallHits.Length; i++)
                    {
                        if (wallHits[i].CompareTag("Floor"))
                        {
                            isWallHold = true;
                            holdedTransform = wallHits[i].transform;
                            break;
                        }
                    }
                }
            }
            else
            {
                bool holdEnd = true;
                if ((playerLookRight ? inputX_p : inputX_m) || isDashing)
                {
                    for (int i = 0; i < wallHits.Length; i++)
                    {
                        if (wallHits[i].transform == holdedTransform)
                        {
                            holdEnd = false;
                            break;
                        }
                    }
                }
                if(holdEnd == true)
                {
                    holdedTransform = null;
                    isWallHold = false;
                }
            }
        }
    }
    void TryStateChange(string nextState)
    {
        if (currentState == nextState)
            return;
        else
        {
            anim.Play(nextState);
            currentState = nextState;
        }
    }
    private void Reset()
    {
        rb = GetComponent<Rigidbody2D>();
    }
}
