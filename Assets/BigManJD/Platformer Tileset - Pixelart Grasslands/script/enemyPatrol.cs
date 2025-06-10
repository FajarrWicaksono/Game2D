using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class enemyPatrol : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float patrolDistance = 5f;   // Jarak patrol (meter)
    private float startX;               // Titik awal enemy
    private bool movingRight = true;

    [Header("Jump Settings")]
    public bool canJump = false;       // Apakah enemy boleh lompat?
    public float jumpForce = 5f;       // Besar gaya lompat
    public float jumpInterval = 3f;    // Berapa detik sekali lompat
    private float jumpTimer;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private int animationState = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startX = transform.position.x;
    }

    void Update()
    {
        Patrol();
        HandleJump();
        UpdateAnimation();
    }

    void Patrol()
    {
        // Hitung jarak yang sudah ditempuh
        float distanceFromStart = Mathf.Abs(transform.position.x - startX);

        // Balik arah kalau sudah melewati batas patrol
        if (distanceFromStart >= patrolDistance)
        {
            Flip();
        }

        // Tambahan Raycast jika perlu mendeteksi tanah atau dinding di depan
        RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        Vector2 frontDirection = movingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallInfo = Physics2D.Raycast(groundCheck.position, frontDirection, 0.2f, obstacleLayer);

        if (!groundInfo.collider || wallInfo.collider)
        {
            Flip();
        }

        // Gerak horizontal
        float moveDirection = movingRight ? 1 : -1;
        rb.velocity = new Vector2(moveSpeed * moveDirection, rb.velocity.y);
    }

    void HandleJump()
    {
        if (!canJump) return;

        jumpTimer += Time.deltaTime;
        if (jumpTimer >= jumpInterval && IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpTimer = 0;
        }
    }

    bool IsGrounded()
    {
        RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, groundCheckDistance, groundLayer);
        return groundInfo.collider != null;
    }

    void Flip()
    {
        movingRight = !movingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        // Reset titik awal agar enemy selalu bolak-balik relatif terhadap startX
        startX = transform.position.x;
    }

    void UpdateAnimation()
    {
        if (Mathf.Abs(rb.velocity.y) > 0.1f)
        {
            animationState = rb.velocity.y > 0 ? 1 : 2;
        }
        else
        {
            animationState = 0;
        }

        animator.SetInteger("jalan", animationState);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.gameObject.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                playerMovement.Die();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * groundCheckDistance);
            Gizmos.color = Color.blue;
            Vector3 frontDirection = movingRight ? Vector3.right : Vector3.left;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + frontDirection * 0.2f);
        }
    }
}