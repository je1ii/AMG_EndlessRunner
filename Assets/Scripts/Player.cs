using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Lane Settings")]
    public float laneSpacing = 2.5f; // distance between lanes
    public int totalLanes = 3; // left, middle, right
    private int currentLane = 1; // 0 = left, 1 = middle, 2 = right

    [Header("Movement Settings")]
    public float moveSpeed = 10f; // speed when sliding between lanes

    [Header("Jump Settings")]
    public float jumpHeight = 2f; // how high the jump goes
    public float jumpDuration = 0.6f; // time to complete jump 
    private bool isJumping = false;
    private float jumpTimer = 0f;
    private float startY;

    private float targetX;
    private Animator animator;

    private void Start()
    {
        animator = GetComponentInChildren<Animator>();
        
        startY = transform.position.y;
        targetX = transform.position.x;
    }

    private void Update()
    {
        HandleInput();
        HandleLaneMovement();
        HandleJump();
    }

    private void HandleInput()
    {
        // move left
        if ((Input.GetKeyDown(KeyCode.A) && currentLane > 0))
        {
            animator.SetTrigger("Left");
            currentLane--;
            targetX = (currentLane - 1) * laneSpacing;
        }

        // move right
        if ((Input.GetKeyDown(KeyCode.D) && currentLane < totalLanes - 1))
        {
            animator.SetTrigger("Right");
            currentLane++;
            targetX = (currentLane - 1) * laneSpacing;
        }

        // jump
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            animator.SetTrigger("Jump");
            isJumping = true;
            jumpTimer = 0f;
        }
    }

    private void HandleLaneMovement()
    {
        // smooth X movement toward target lane
        Vector3 pos = transform.position;
        pos.x = Mathf.MoveTowards(pos.x, targetX, moveSpeed * Time.deltaTime);
        transform.position = pos;
    }

    private void HandleJump()
    {
        if (isJumping)
        {
            jumpTimer += Time.deltaTime;
            float t = jumpTimer / jumpDuration;

            if (t >= 1f)
            {
                isJumping = false;
                transform.position = new Vector3(transform.position.x, startY, transform.position.z);
            }
            else
            {
                // parabola curve (0 → 1 → 0)
                float jumpOffset = 4 * jumpHeight * t * (1 - t);
                transform.position = new Vector3(transform.position.x, startY + jumpOffset, transform.position.z);
            }
        }
    }

    public void PlayerDied()
    {
        Debug.Log("Player died");
        Time.timeScale = 0;
        StartCoroutine(EndGame());
    }

    private IEnumerator EndGame()
    {
        yield return new WaitForSecondsRealtime(3f);
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public int CurrentLane => currentLane;
    public bool IsJumping => isJumping || transform.position.y > startY + 0.2f;
}
