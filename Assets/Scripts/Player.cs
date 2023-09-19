using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using System;

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
    [SerializeField] float acs;
    [SerializeField] float gravityScale;
    Vector2 inputVec;
    bool playerLookRight;

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
    bool wallJump = false;
    Transform groundTransform;
    Vector2 groundOffset;

    [Header("대쉬")]
    [SerializeField] float dashForce;
    [SerializeField] bool isDashing;
    Vector2 dashVelocity;
    [SerializeField] float dashDrag;
    [SerializeField] float dashLength;

    [Header("벽잡기")]
    [SerializeField] bool isWallHold;
    [SerializeField] Transform handColiderHint;
    [SerializeField] float targetWallSlideVelocity, wallSlideLerpMulti;
    Transform holdedTransform;
    [SerializeField] Vector2 handSize, handOffset;
    float holdtime = 2;

    [Header("애니메이션")]
    [SerializeField] string currentState;

    public Vector2 plusmove, oldPlus;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        gravityScale = rb.gravityScale;
        DoubleClickSetup();
    }

    void FixedUpdate()
    {
        rb.velocity -= oldPlus;

        WallHold();
        FixedMove();
        DashRedy();

        oldPlus = plusmove;

        if (plusmove.y > 0 && !isWallHold && !isTopHold)
            plusmove.y = 0;

        //if (onGround || isWallHold)
            rb.velocity += plusmove;
    }
    private void LateUpdate()
    {
        DoubleClickUpdate();
        UiManager.PlayerVelocityUI(rb.velocity.y);
        handColiderHint.localPosition = handOffset;
    }

    float trailAlpha = 0;
    void Update()
    {
        InputUpdate();
        //Look();
        Landing();
        Jump(true);

        if (onGround || isDashing)
            holdtime = 2;

        anim.gameObject.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, Convert.ToInt32(!isDashing));

        if (isDashing)
            trailAlpha = 1;
        else if (trailAlpha > 0)
            trailAlpha -= 10 * Time.deltaTime;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0.0f), new GradientColorKey(Color.white, 1.0f) },
            new GradientAlphaKey[] { new GradientAlphaKey(trailAlpha, 0.0f), new GradientAlphaKey(0, 1.0f) }
        );

        anim.transform.GetChild(0).GetComponent<TrailRenderer>().colorGradient = gradient;


        if (!isWallHold)
        {
            if (!isWallHold && !isDashing && !wallJump)
            {
                if (moveSpeed.x > 0.1f)
                    PlayerLookDirectionChange(true);
                else if (moveSpeed.x < -0.1f)
                    PlayerLookDirectionChange(false);
            }
            else
            {
                if (inputVec.x > 0)
                    PlayerLookDirectionChange(true);
                else if (inputVec.x != 0)
                    PlayerLookDirectionChange(false);
            }
        }
    }
    void DoubleClickSetup()
    {
        doubleClickAction.Add(KeyCode.Z, () => { });
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
        if (isDoubleClick)
        {
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
        if (Input.GetKey(KeyCode.Z))
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
                    DoubleClickCheck(KeyCode.Z);
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
    void DoubleClickUpdate()
    {
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

    Vector2 moveSpeed = Vector2.zero;
    void FixedMove()
    {
        moveSpeed = rb.velocity;

        if (isWallHold)
        {
            if (onGround)
            {
                holdReset();
            }
            rb.gravityScale = 0;
            TryStateChange("Grap");
        }
        else if(isDashing)
            rb.gravityScale = 0;
        else
            rb.gravityScale = gravityScale;

        if (isDashing)
        {
            TryStateChange("Run");
            anim.SetFloat("runSpeed", Mathf.Abs(rb.velocity.x) / (speed * Time.fixedDeltaTime));
            //dashVelocity.x = Mathf.Lerp(dashVelocity.x, 0, Time.fixedDeltaTime * dashDrag);
            //rb.velocity = new Vector2(rb.velocity.x * 0.95f, rb.velocity.y);
            //Debug.Log(rb.velocity);
            if (((rb.velocity.x >= -5f && rb.velocity.x <= 5f) && (rb.velocity.y >= -5f && rb.velocity.y <= 5f)) || (Vector2.Distance(DashStartPoint, transform.position) > dashLength))
            {
                isDashing = false;
                rb.velocity = Vector2.zero;
            }

            return;
        }
        else
            anim.SetFloat("runSpeed", 1);

        if (onGround &&  !isWallHold && !isTopHold)
        {
            Vector2 mathspeed = moveSpeed;

            //Debug.Log(mathspeed.x);

            if (mathspeed.x > 0.1f || mathspeed.x < -0.1f)
            {
                if(inputVec.x != 0 
                    && ((anim.GetCurrentAnimatorStateInfo(0).IsName("Run") && (mathspeed.x > 6f || mathspeed.x < -6f))
                    || (anim.GetCurrentAnimatorStateInfo(0).IsName("break") && (mathspeed.x > 0.1f || mathspeed.x < -0.1f)))
                    && !(new Vector2(inputVec.x,0).normalized.x == new Vector2(mathspeed.x,0).normalized.x))
                    TryStateChange("break");
                else
                    TryStateChange("Run");

            }
            else
                TryStateChange("Idle");
        }

        if (inputVec.x != 0)
        {

            moveSpeed += new Vector2(inputVec.x * acs * Time.fixedDeltaTime, 0);

            moveSpeed.x = Mathf.Clamp(moveSpeed.x, -speed, speed);
        }

        if (currentState == "Folling")
        {
            if (rb.velocity.y < 0)
            {
                anim.SetFloat("runSpeed", 1f);
            }
            else
            {
                anim.SetFloat("runSpeed", 0);
            }
        }

        if (!isDashing && !isWallHold && !isTopHold && onGround)
        {
            if (inputVec.x == 0)
            {
                moveSpeed -= new Vector2((1 * moveSpeed.x) * (acs * 0.5f) * Time.fixedDeltaTime, 0);
            }
        }

        rb.velocity = moveSpeed;

        if (isWallHold)
            rb.velocity = new Vector2((playerLookRight ? 400 : -400) * Time.fixedDeltaTime, 0);
        if(isTopHold)
            rb.velocity = new Vector2(0, 10);
    }
    void Jump(bool stateCheck)
    {
        //onGround = false;

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
            holdReset();
            PlayerLookDirectionChange(!playerLookRight);
            rb.velocity = new Vector2((playerLookRight ? 1 : -1) * (speed * 2), jumpForce);
            wallJump = true;
            //Dash(!playerLookRight, true);
            return;
        }
        else if (isTopHold)
        {
            holdReset();
            rb.velocity = new Vector2(inputVec.x * (speed * 2), -jumpForce);
            wallJump = true;
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
    void DashRedy()
    {
        if (Input.GetKey(KeyCode.X))
        {
            rb.velocity = Vector2.zero;
            rb.gravityScale = 0;
        }

        if (Input.GetKeyUp(KeyCode.X))
        {
            Dash(playerLookRight);
            rb.gravityScale = 0;
        }
    }

    Vector2 DashStartPoint;

    public void Dash(bool isRight, bool isWallClimbing = false)
    {
        DashStartPoint = transform.position;

        float velocityY = rb.velocity.y;
        print("Dash-" + (isRight ? "right" : "left"));
        rb.velocity = dashForce * inputVec;
        //dashVelocity = new Vector2(dashForce * (isRight ? 1 : -1) * (isWallClimbing ? 0.6f : 1), velocityY);
        isDashing = true;

        /*if (rb.velocity.x > 0)
            PlayerLookDirectionChange(true);
        else
            PlayerLookDirectionChange(false);*/

        holdReset();
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(rb.position + lookVec, 0.1f);
    }

    RaycastHit2D rayHit, rayHit2;
    void ray()
    {
        //Debug.DrawRay(new Vector2(rb.position.x + 0.4f, rb.position.y), Vector3.down * 0.8f, new Color(1, 1, 0));

        rayHit = Physics2D.Raycast(new Vector2(rb.position.x + 0.4f, rb.position.y), Vector3.down, 0.8f, LayerMask.GetMask("Floor"));

        //Debug.DrawRay(new Vector2(rb.position.x - 0.4f, rb.position.y), Vector3.down * 0.8f, new Color(0, 1, 1));

        rayHit2 = Physics2D.Raycast(new Vector2(rb.position.x - 0.4f, rb.position.y), Vector3.down, 0.8f, LayerMask.GetMask("Floor"));

        if (rayHit.collider != null || rayHit2.collider != null)
        {
            onGround = true;

        }
        else
        {
            onGround = false;
        }
    }

    RaycastHit2D rayHitup, rayHit2up;

    bool isTopHold = false, upredy = false;
    void rayup()
    {
        //Debug.DrawRay(new Vector2(rb.position.x + 0.4f, rb.position.y), Vector3.down * 0.8f, new Color(1, 1, 0));

        rayHitup = Physics2D.Raycast(new Vector2(rb.position.x + 0.4f, rb.position.y), Vector3.up, 0.8f, LayerMask.GetMask("Floor"));

        //Debug.DrawRay(new Vector2(rb.position.x - 0.4f, rb.position.y), Vector3.down * 0.8f, new Color(0, 1, 1));

        rayHit2up = Physics2D.Raycast(new Vector2(rb.position.x - 0.4f, rb.position.y), Vector3.up, 0.8f, LayerMask.GetMask("Floor"));

        if ((rayHitup.collider != null || rayHit2up.collider != null) && !onGround && !isWallHold && holdtime > 0 && !isTopHold && upredy)
        {
            isTopHold = true;
            upredy = false;
        }

        if (rayHitup.collider == null && rayHit2up.collider == null)
        {
            upredy = true;
        }
    }

    void Landing() // 착지를 검사하고 땅에있다면 점프 횟수를 추가하는 메소드
    {
        ray();

        if (onGround)
        {
            ableJumpCount = maxJumpCount;
            wallJump = false;
        }
        else
        {
            if (!isWallHold && !isTopHold)
                TryStateChange("Folling");
        }
    }
    void WallHold()
    {
        rayup();

        if (onGround || holdtime <= 0)
            return;

        if (!isDashing)
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
                if ((inputVec.x != 0 && onGround == false && rb.velocity.y <= 0) || isDashing || wallJump)
                {
                    for (int i = 0; i < wallHits.Length; i++)
                    {
                        if (wallHits[i].CompareTag("Floor"))
                        {
                            //Debug.Log("grab");
                            //holdtime = 2;
                            isWallHold = true;
                            isDashing = false;
                            wallJump = false;
                            rb.velocity = Vector2.zero;
                            holdedTransform = wallHits[i].transform;
                            break;
                        }
                    }
                }
            }
            else
            {
                holdtime -= Time.deltaTime;

                if (holdtime <= 0)
                {
                    holdReset();
                }
            }
        }

        if (isTopHold)
        {
            holdtime -= Time.deltaTime;

            if (holdtime <= 0)
            {
                holdReset();
            }

            TryStateChange("topGrap");
        }
    }

    void holdReset()
    {
        holdedTransform = null;
        isWallHold = false;
        isTopHold = false;
        //upredy = true;
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