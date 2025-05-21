using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;
    public float airControlMultiplier = 0.5f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerController playerController;

    private Vector2 moveInput;
    private float mobileInputX = 0f;

    private bool isJumping = false;
    private bool dead = false;
    private Vector2 startPosition;
    Vector2 currentCheckpoint;

    private enum MovementState { idle, walk, jump, fall, run }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    [Header("Double Jump Settings")]
    public int maxJumpCount = 2;
    private int jumpCount;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();
        playerController = new PlayerController();
        startPosition = transform.position;
        currentCheckpoint = startPosition;
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void Update()
    {
        if (dead) return;

        // Gunakan mobile input jika di mobile
        if (Application.isMobilePlatform)
        {
            moveInput = new Vector2(mobileInputX, 0f);
        }
        else
        {
            moveInput = playerController.Movement.Move.ReadValue<Vector2>();
        }

        if (isGrounded())
        {
            jumpCount = 0;
        }
    }

    private void FixedUpdate()
    {
        if (dead) return;

        float horizontalInput = moveInput.x + mobileInputX;
        float horizontalSpeed = isGrounded() ? moveSpeed : moveSpeed * airControlMultiplier;

        Vector2 targetVelocity = new Vector2(horizontalInput * horizontalSpeed, rb.velocity.y);
        rb.velocity = targetVelocity;

        UpdateAnimation();

        if (isGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            isJumping = false;
        }
    }

    private void UpdateAnimation()
    {
        MovementState state;
        float horizontal = moveInput.x != 0 ? moveInput.x : mobileInputX;

        if (horizontal > 0f)
        {
            state = MovementState.walk;
            sprite.flipX = false;
        }
        else if (horizontal < 0f)
        {
            state = MovementState.walk;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.fall;
        }

        anim.SetInteger("state", (int)state);
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        if (dead) return;

        if (jumpCount < maxJumpCount)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpCount++;
        }
    }

    // Fungsi dipanggil oleh UI button kanan
    public void MoveRight(bool isPressed)
    {
        if (isPressed)
            mobileInputX = 1f;
        else if (mobileInputX == 1f)
            mobileInputX = 0f;
    }

    // Fungsi dipanggil oleh UI button kiri
    public void MoveLeft(bool isPressed)
    {
        if (isPressed)
            mobileInputX = -1f;
        else if (mobileInputX == -1f)
            mobileInputX = 0f;
    }

    // Fungsi dipanggil oleh UI button lompat
    public void MobileJump()
    {
        Jump();
    }

    public void SetCheckpoint(Vector2 checkpointPosition)
    {
        currentCheckpoint = checkpointPosition;
    }

    public void Die()
    {
        if (dead) return;
        dead = true;
        rb.velocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Static;
        StartCoroutine(Respawn());
    }

    private IEnumerator Respawn()
    {
        yield return new WaitForSeconds(1f);
        transform.position = currentCheckpoint;
        rb.bodyType = RigidbodyType2D.Dynamic;
        dead = false;
    }
}
