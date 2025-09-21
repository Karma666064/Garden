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
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    [SerializeField] private float speed = 9f;
    [SerializeField] private float acceleration = 4f;
    [SerializeField] private float jumpForce = 7f;
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
        inputActions.Player.Jump.performed += OnJump;
    }    

    void Update()
    {

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
        else if (rb.linearVelocity.y > 0 && !inputActions.Player.Jump.IsPressed())
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.fixedDeltaTime;

        // Coyote time
        if (isGrounded) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.fixedDeltaTime;

        // Jump Buffer
        if (inputActions.Player.Jump.IsPressed()) jumpBufferCounter = jumpBufferTime;
        else jumpBufferCounter -= Time.fixedDeltaTime;

        // Jump
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0;
        }
    }

    void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    void OnJump(InputAction.CallbackContext context)
    {
        Debug.Log("Debug 1");
        if (context.performed)
        {
            Debug.Log("Debug 2");
            if (!jumpHeld)
            {
                jumpBufferCounter = jumpBufferTime;
                jumpHeld = true;
                Debug.Log("Debug 3");
            }
        }
        else if (context.canceled)
        {
            Debug.Log("Debug 4");
            jumpHeld = false;
        }
        Debug.Log("Debug 5");
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
 * - Si je garde le boutton appuyer je saute dès que je retouche le sol
 * -  
*/