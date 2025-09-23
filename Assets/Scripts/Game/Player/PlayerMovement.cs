using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerInputActions inputActions;
    private Animator animator;

    private Vector2 moveInput;

    [SerializeField] private bool isGrounded;
    [SerializeField] private bool jumpHeld;

    [SerializeField] private bool canDash = true;
    [SerializeField] private bool isDashing;

    [SerializeField] private bool isFacingRight;

    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [SerializeField] private float speed = 9f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float jumpForce = 7f;

    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    //[SerializeField] private float dashingCooldown = 1f;
    private Vector2 dashDirection;

    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;

    [SerializeField] private float airControlMultiplier = 0.5f;

    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.8f, 0.2f);
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;


    void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        rb = GetComponent<Rigidbody2D>();

        inputActions = new PlayerInputActions();
        inputActions.Enable();

        inputActions.Player.Move.performed += OnMove;
        inputActions.Player.Move.canceled += OnMove;

        inputActions.Player.Jump.started += OnJumpStart;
        inputActions.Player.Jump.canceled += OnJumpCancel;

        inputActions.Player.Dash.started += OnDash;
    }    

    void Update()
    {
        if (isGrounded && !isDashing) canDash = true;
        // Update animations
        //animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
        //animator.SetBool("isGrounded", isGrounded);
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0f, groundLayer);

        // Horizontal deplacement lissé
        Vector2 velocity = rb.linearVelocity;
        float control = isGrounded ? 1f : airControlMultiplier;
        velocity.x = Mathf.Lerp(velocity.x, moveInput.x * speed, Time.fixedDeltaTime * acceleration * control);
        rb.linearVelocity = velocity;

        // Realistic jump
        if (rb.linearVelocity.y < 0)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;

        // Coyote time
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.fixedDeltaTime;

        // Jump Buffer
        if (jumpBufferCounter > 0) jumpBufferCounter -= Time.fixedDeltaTime;

        // Jump
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
        }

        // Regard du personnage 
        if (moveInput.x > 0.1f) isFacingRight = true;
        else if (moveInput.x < -0.1f) isFacingRight = false;
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnJumpStart(InputAction.CallbackContext context)
    {
        jumpBufferCounter = jumpBufferTime;
        jumpHeld = true;
    }

    void OnJumpCancel(InputAction.CallbackContext context)
    {
        jumpHeld = false;
    }

    void OnDash(InputAction.CallbackContext context)
    {
        if (context.started && canDash)
        {
            StartCoroutine(Dash(moveInput));
        }
    }


    public IEnumerator Dash(Vector2 inputDirection)
    {
        if (canDash)
        {
            if (inputDirection.sqrMagnitude < 0.1f) // Pas d'input
            {
                // Dash vers la direction du joueur (droite ou gauche)
                float facingDir = isFacingRight ? 1f : -1f;
                dashDirection = new Vector2(facingDir, 0f);
            }
            else dashDirection = inputDirection.normalized;

            // Lancement du dash
            canDash = false;
            isDashing = true;

            float originalGravity = rb.gravityScale;
            if (dashDirection.y > 0f) rb.gravityScale = 2f;
            else rb.gravityScale = 0f;

            rb.linearVelocity = dashDirection * dashingPower;

            yield return new WaitForSeconds(dashingTime);

            // Fin du dash
            rb.gravityScale = originalGravity;
            isDashing = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
/*
 * Bugs :
 * - Si je laisse le boutton de saut appuyer le dash va plus loin
 * -  
*/