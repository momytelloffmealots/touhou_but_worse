using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float focusSpeed = 2.5f;

    [Header("Dash")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;

    [Header("Movement Bounds")]
    [SerializeField] private Vector2 horizontalBounds = new Vector2(-8f, 8f);
    [SerializeField] private Vector2 verticalBounds = new Vector2(-4.5f, 4.5f);

    [Header("Visual")]
    [SerializeField] private Transform playerVisuals;
    [SerializeField] private Transform hitboxIndicator;

    [Header("Tilt")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 10f;

    [Header("Victory")]
    [SerializeField] private float victoryPullForce = 2f;
    [SerializeField] private float victoryPullStartY = 0f;

    // COMPONENTS
    private Rigidbody2D rb;
    private Animator animator;
    private PlayerShooting playerShooting;

    // MOVEMENT
    private Vector2 moveInput;
    private float currentSpeed;
    private float speedMultiplier = 1f;

    private bool canMove = true;
    private bool canShoot = true;

    // DASH
    private bool canDash = true;
    private bool isDashing = false;

    // VICTORY
    private bool isVictoryExiting = false;

    // FOCUS
    public bool IsFocused { get; private set; }

    // ANIMATION
    private static readonly int MoveXAnimID =
        Animator.StringToHash("MoveX");

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        // =========================================
        // QUAN TRỌNG: TẮT GRAVITY
        // =========================================
        rb.gravityScale = 0f;

        // Không cho Rigidbody tự xoay
        rb.freezeRotation = true;

        // Tìm PlayerShooting
        playerShooting = GetComponent<PlayerShooting>();

        // Tìm Animator
        if (playerVisuals != null)
        {
            animator = playerVisuals.GetComponentInChildren<Animator>();
        }
        else
        {
            animator = GetComponentInChildren<Animator>();
        }

        currentSpeed = moveSpeed;

        // Tắt hitbox khi bắt đầu
        if (hitboxIndicator != null)
        {
            hitboxIndicator.gameObject.SetActive(false);
        }
    }

    private void Update()
    {
        if (isDashing)
            return;

        ReadInput();
        UpdateFocus();
        UpdateAnimator();
        UpdateTilt();

        // SPACE = DASH
        if (canMove && Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Dash());
        }

        // VICTORY
        if (Input.GetKeyDown(KeyCode.V))
        {
            StartVictoryExitSequence();
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();

        if (isVictoryExiting)
        {
            HandleVictoryPull();
        }
    }

    private void ReadInput()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            IsFocused = false;
            return;
        }

        // DI CHUYỂN
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        moveInput = new Vector2(horizontal, vertical);

        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }

        // SHIFT = FOCUS
        IsFocused =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);

        // Z = BẮN
        if (canShoot && Input.GetKey(KeyCode.Z))
        {
            Shoot();
        }

        // CHUỘT TRÁI = BẮN
        if (canShoot && Input.GetMouseButton(0))
        {
            Shoot();
        }

        // X = BOM
        if (Input.GetKeyDown(KeyCode.X))
        {
            UseBomb();
        }

        // E = BẤT TỬ
        if (Input.GetKeyDown(KeyCode.E))
        {
            UseInvincibility();
        }
    }

    private void HandleMovement()
    {
        if (isDashing)
            return;

        if (!canMove)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        float finalSpeed = currentSpeed * speedMultiplier;

        // =========================================
        // SET VELOCITY TRỰC TIẾP
        // Gravity đã bị tắt nên nhân vật không rơi
        // =========================================
        Vector2 targetVelocity = moveInput * finalSpeed;

        rb.velocity = targetVelocity;

        // =========================================
        // GIỚI HẠN MÀN HÌNH
        // =========================================
        Vector2 newPosition = rb.position;

        newPosition.x = Mathf.Clamp(
            newPosition.x,
            horizontalBounds.x,
            horizontalBounds.y
        );

        newPosition.y = Mathf.Clamp(
            newPosition.y,
            verticalBounds.x,
            verticalBounds.y
        );

        rb.position = newPosition;

        // Nếu chạm biên thì xóa vận tốc theo hướng
        // đang bị giới hạn
        if (newPosition.x <= horizontalBounds.x &&
            rb.velocity.x < 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if (newPosition.x >= horizontalBounds.y &&
            rb.velocity.x > 0)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }

        if (newPosition.y <= verticalBounds.x &&
            rb.velocity.y < 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }

        if (newPosition.y >= verticalBounds.y &&
            rb.velocity.y > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
        }
    }

    private void UpdateFocus()
    {
        if (IsFocused)
        {
            currentSpeed = focusSpeed;
        }
        else
        {
            currentSpeed = moveSpeed;
        }

        if (hitboxIndicator != null)
        {
            hitboxIndicator.gameObject.SetActive(IsFocused);
        }
    }

    private void UpdateAnimator()
    {
        if (animator == null)
            return;

        animator.SetFloat(
            MoveXAnimID,
            moveInput.x
        );
    }

    private void UpdateTilt()
    {
        if (playerVisuals == null)
            return;

        float targetAngle =
            -moveInput.x * maxTiltAngle;

        Quaternion targetRotation =
            Quaternion.Euler(
                0f,
                0f,
                targetAngle
            );

        playerVisuals.rotation =
            Quaternion.Slerp(
                playerVisuals.rotation,
                targetRotation,
                Time.deltaTime * tiltSpeed
            );
    }

    private IEnumerator Dash()
    {
        if (!canDash)
            yield break;

        canDash = false;
        isDashing = true;

        Vector2 dashDirection =
            moveInput.normalized;

        // Không có hướng -> dash lên
        if (dashDirection == Vector2.zero)
        {
            dashDirection = Vector2.up;
        }

        rb.velocity =
            dashDirection * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.velocity = Vector2.zero;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    private void Shoot()
    {
        if (playerShooting == null)
        {
            playerShooting =
                GetComponent<PlayerShooting>();
        }

        if (playerShooting != null)
        {
            playerShooting.TryToShoot();
        }
        else
        {
            Debug.LogWarning(
                "[PlayerController] Không tìm thấy PlayerShooting!"
            );
        }
    }

    private void UseBomb()
    {
        Debug.Log("Player sử dụng Bomb!");
    }

    private void UseInvincibility()
    {
        Debug.Log("Player sử dụng Invincibility!");

        StartCoroutine(
            InvincibilityCoroutine()
        );
    }

    private IEnumerator InvincibilityCoroutine()
    {
        Debug.Log("Player bất tử!");

        yield return new WaitForSeconds(3f);

        Debug.Log("Hết bất tử!");
    }

    public void StartVictoryExitSequence()
    {
        SetPlayerControl(true);

        isVictoryExiting = true;

        Debug.Log("Victory Exit Started!");
    }

    private void HandleVictoryPull()
    {
        if (transform.position.y > victoryPullStartY)
        {
            rb.AddForce(
                Vector2.up * victoryPullForce,
                ForceMode2D.Force
            );
        }
    }

    public void SetPlayerControl(bool isEnabled)
    {
        canMove = isEnabled;
        canShoot = isEnabled;

        if (!isEnabled)
        {
            moveInput = Vector2.zero;
            rb.velocity = Vector2.zero;
        }

        Debug.Log(
            "Player Control: " +
            isEnabled
        );
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }

    public Transform GetVisualsTransform()
    {
        return playerVisuals;
    }
}