using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScorpion : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 2f;
    public float patrolDistance = 5f;   // Jarak patrol (meter)
    private float startX;               // Titik awal enemy
    private bool movingLeft = true;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;
    public LayerMask obstacleLayer;

    private Rigidbody2D rb;
    private Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        startX = transform.position.x;
    }

    void Update()
    {
        Patrol();
        // Tidak ada UpdateAnimation() karena animasi selalu jalan
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
        Vector2 frontDirection = movingLeft ? Vector2.left : Vector2.right;
        RaycastHit2D wallInfo = Physics2D.Raycast(groundCheck.position, frontDirection, 0.2f, obstacleLayer);

        if (!groundInfo.collider || wallInfo.collider)
        {
            Flip();
        }

        // Gerak horizontal
        float moveDirection = movingLeft ? -1 : 1;
        rb.velocity = new Vector2(moveSpeed * moveDirection, rb.velocity.y);
    }

    void Flip()
    {
        movingLeft = !movingLeft;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
        startX = transform.position.x;
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
            Vector3 frontDirection = movingLeft ? Vector3.left : Vector3.right;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + frontDirection * 0.2f);
        }
    }
}