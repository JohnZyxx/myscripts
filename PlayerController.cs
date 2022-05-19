using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    Animator animator;
    SpriteRenderer spriteRenderer;
    Rigidbody2D rb2d;
    
    [Header("Components")]
    public float speed;
    public float jumpForce;
    private float moveInput;

    [Header("Jump")]
    private float jumpTimeCounter;
    public float jumpTime;
    private bool isJumping;

    [Header("Layer Mask")]
    private bool isGrounded;
    public float checkRadius;
    public LayerMask whatIsGround;


    [Header("fall physics")]
    public float fallMultiplier;
    public float lowJumpMultiplier;

    [Header ("Extra Jumps")]
    private int extraJumps;
    public int extraJumpsValue;

    [SerializeField]
    Transform groundCheck;

    [SerializeField]
    Transform groundCheckL;

    [SerializeField]
    Transform groundCheckR;

    private bool isFacingRight = true;
    private bool canDash;
    private bool isDashing;
    public float dashingPower = 50f;
    public float dashingTime = 0.2f;
    public float dashingCooldown = 1f;
    public float dashJumpIncrease;

   [SerializeField] private TrailRenderer tr;

    //Gets Rigidbody component
    void Start()
    {
        canDash = true;
        extraJumps = extraJumpsValue;
        rb2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void FixedUpdate()
    {
        if(isDashing)
        {
            return;
        }
        //Character Movement ==========================================
        moveInput = Input.GetAxisRaw("Horizontal");
        rb2d.velocity = new Vector2(moveInput * speed, rb2d.velocity.y);
        //===============================================================
        if (moveInput < 0f)
        {
            if(isGrounded)
               animator.Play("Player_run");
            spriteRenderer.flipX = false;
        }
        if (moveInput > 0f)
        {
            if(isGrounded)
               animator.Play("Player_run");
        } 
    }
    private void Update()
    {
        // Lets the player jump =================================================
        if (Input.GetKeyDown("space") && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb2d.velocity = Vector2.up * jumpForce;  
        }
        if (Input.GetKey("space") && isJumping == true)
        {
            if(jumpTimeCounter > 0)
            {
                rb2d.velocity = Vector2.up * jumpForce;
                jumpTimeCounter -= Time.deltaTime;
                animator.Play("Player_jump");
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp("space"))
        {
            isJumping = false;
        }

            //Double-Jump =========
        if (isGrounded == true)
        {
            extraJumps = extraJumpsValue;
        }
        if (Input.GetKeyDown("space") && extraJumps > 0)
        {
            isJumping = true;
            jumpTimeCounter = jumpTime;
            rb2d.velocity = Vector2.up * jumpForce;
            extraJumps--;
            animator.Play("Player_jump");
        }
        else if (Input.GetKeyDown("space") && extraJumps == 0 && isGrounded == true)
        {
            rb2d.velocity = Vector2.up * jumpForce;
        }

            // Falling animation ============================

        if (isGrounded == false && isJumping == false)
        {
            animator.Play("Player_desc");  
        }
        //========================================================================
        if(isDashing)
        {
            return;
        }

        // GroundCheck Physics ===============================        
        if((Physics2D.Linecast(transform.position, groundCheck.position, 1 <<LayerMask.NameToLayer("Ground"))) ||
          (Physics2D.Linecast(transform.position, groundCheckL.position, 1 <<LayerMask.NameToLayer("Ground"))) ||
          (Physics2D.Linecast(transform.position, groundCheckR.position, 1 <<LayerMask.NameToLayer("Ground"))))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, whatIsGround);
        //==============================================================

        if(isGrounded && moveInput == 0)
        {
            animator.Play("Player_idle");
        }
        // Let's you dash========================
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(Dash());
        }
        //=======================================

        Flip();
    }

    private void Flip()
    {
        if (isFacingRight && moveInput < 0f || !isFacingRight && moveInput > 0f)
        {
            Vector3 localScale = transform.localScale;
            isFacingRight = !isFacingRight;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

//=============================
// Dash Code
// =============================
    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        animator.Play("Player_dash");
        float originalGravity = rb2d.gravityScale;
        rb2d.gravityScale = 0f;
        rb2d.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds (dashingTime);
        tr.emitting = false;
        rb2d.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        isGrounded = true;
        canDash = true;
    }

}