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
    [SerializeField]
    private Vector2 horizontalBounds =
        new Vector2(-8f, 8f);

    [SerializeField]
    private Vector2 verticalBounds =
        new Vector2(-4.5f, 4.5f);

    [Header("Visual")]
    [SerializeField] private Transform playerVisuals;
    [SerializeField] private Transform hitboxIndicator;

    [Header("Tilt")]
    [SerializeField] private float maxTiltAngle = 15f;
    [SerializeField] private float tiltSpeed = 10f;

    [Header("Victory")]
    [SerializeField] private float victoryPullForce = 2f;
    [SerializeField] private float victoryPullStartY = 0f;

    // =========================================================
    // COMPONENTS
    // =========================================================

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerShooting playerShooting;

    // =========================================================
    // MOVEMENT
    // =========================================================

    private Vector2 moveInput;

    private float currentSpeed;
    private float speedMultiplier = 1f;

    private bool canMove = true;
    private bool canShoot = true;

    // =========================================================
    // DASH
    // =========================================================

    private bool canDash = true;
    private bool isDashing = false;

    // =========================================================
    // VICTORY
    // =========================================================

    private bool isVictoryExiting = false;

    // =========================================================
    // FOCUS
    // =========================================================

    public bool IsFocused { get; private set; }

    // =========================================================
    // ANIMATION
    // =========================================================

    private static readonly int MoveXAnimID =
        Animator.StringToHash("MoveX");


    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }


        // Tìm PlayerShooting
        playerShooting =
            GetComponent<PlayerShooting>();


        // Tìm Animator
        if (playerVisuals != null)
        {
            animator =
                playerVisuals.GetComponentInChildren<Animator>();
        }
        else
        {
            animator =
                GetComponentInChildren<Animator>();
        }


        currentSpeed = moveSpeed;


        // Tắt hitbox khi bắt đầu
        if (hitboxIndicator != null)
        {
            hitboxIndicator.gameObject.SetActive(false);
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (isDashing)
            return;


        ReadInput();

        UpdateFocus();

        UpdateAnimator();

        UpdateTilt();


        // =====================================================
        // SPACE = DASH
        // =====================================================

        if (canMove &&
            Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine(Dash());
        }


        // =====================================================
        // VICTORY
        // =====================================================

        if (Input.GetKeyDown(KeyCode.V))
        {
            StartVictoryExitSequence();
        }
    }


    // =========================================================
    // FIXED UPDATE
    // =========================================================

    private void FixedUpdate()
    {
        HandleMovement();


        if (isVictoryExiting)
        {
            HandleVictoryPull();
        }
    }


    // =========================================================
    // INPUT
    // =========================================================

    private void ReadInput()
    {
        if (!canMove)
        {
            moveInput = Vector2.zero;
            IsFocused = false;

            return;
        }


        // -----------------------------------------------------
        // DI CHUYỂN
        // WASD / ARROW
        // -----------------------------------------------------

        float horizontal =
            Input.GetAxisRaw("Horizontal");

        float vertical =
            Input.GetAxisRaw("Vertical");


        moveInput =
            new Vector2(
                horizontal,
                vertical
            );


        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }


        // =====================================================
        // SHIFT = FOCUS / ĐI CHẬM
        // =====================================================

        IsFocused =
            Input.GetKey(KeyCode.LeftShift) ||
            Input.GetKey(KeyCode.RightShift);


        // =====================================================
        // Z = BẮN
        // =====================================================

        if (canShoot &&
            Input.GetKey(KeyCode.Z))
        {
            Shoot();
        }


        // =====================================================
        // CHUỘT TRÁI = BẮN
        // =====================================================

        if (canShoot &&
            Input.GetMouseButton(0))
        {
            Shoot();
        }


        // =====================================================
        // X = BOM
        // =====================================================

        if (Input.GetKeyDown(KeyCode.X))
        {
            UseBomb();
        }


        // =====================================================
        // E = BẤT TỬ
        // =====================================================

        if (Input.GetKeyDown(KeyCode.E))
        {
            UseInvincibility();
        }
    }


    // =========================================================
    // MOVEMENT
    // =========================================================

    private void HandleMovement()
    {
        if (isDashing)
            return;


        if (!canMove)
        {
            rb.velocity = Vector2.zero;

            return;
        }


        float finalSpeed =
            currentSpeed *
            speedMultiplier;


        Vector2 targetVelocity =
            moveInput *
            finalSpeed;


        rb.velocity =
            targetVelocity;


        // -----------------------------------------------------
        // GIỚI HẠN MÀN HÌNH
        // -----------------------------------------------------

        Vector2 newPosition =
            rb.position;


        newPosition.x =
            Mathf.Clamp(
                newPosition.x,
                horizontalBounds.x,
                horizontalBounds.y
            );


        newPosition.y =
            Mathf.Clamp(
                newPosition.y,
                verticalBounds.x,
                verticalBounds.y
            );


        rb.position =
            newPosition;
    }


    // =========================================================
    // FOCUS / ĐI CHẬM
    // =========================================================

    private void UpdateFocus()
    {
        if (IsFocused)
        {
            currentSpeed =
                focusSpeed;
        }
        else
        {
            currentSpeed =
                moveSpeed;
        }


        // Hiện hitbox khi giữ Shift
        if (hitboxIndicator != null)
        {
            hitboxIndicator.gameObject.SetActive(
                IsFocused
            );
        }
    }


    // =========================================================
    // ANIMATION
    // =========================================================

    private void UpdateAnimator()
    {
        if (animator == null)
            return;


        animator.SetFloat(
            MoveXAnimID,
            moveInput.x
        );
    }


    // =========================================================
    // TILT
    // =========================================================

    private void UpdateTilt()
    {
        if (playerVisuals == null)
            return;


        float targetAngle =
            -moveInput.x *
            maxTiltAngle;


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
                Time.deltaTime *
                tiltSpeed
            );
    }


    // =========================================================
    // DASH
    // =========================================================

    private IEnumerator Dash()
    {
        if (!canDash)
            yield break;


        canDash = false;

        isDashing = true;


        // Lấy hướng đang di chuyển
        Vector2 dashDirection =
            moveInput.normalized;


        // Nếu không di chuyển
        // thì dash lên trên
        if (dashDirection == Vector2.zero)
        {
            dashDirection =
                Vector2.up;
        }


        // Tốc độ dash
        rb.velocity =
            dashDirection *
            dashSpeed;


        yield return new WaitForSeconds(
            dashDuration
        );


        // Dừng dash
        rb.velocity =
            Vector2.zero;


        isDashing = false;


        // Cooldown
        yield return new WaitForSeconds(
            dashCooldown
        );


        canDash = true;
    }


    // =========================================================
    // SHOOT
    // =========================================================

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


    // =========================================================
    // BOMB
    // =========================================================

    private void UseBomb()
    {
        Debug.Log(
            "Player sử dụng Bomb!"
        );
    }


    // =========================================================
    // INVINCIBILITY
    // =========================================================

    private void UseInvincibility()
    {
        Debug.Log(
            "Player sử dụng Invincibility!"
        );


        StartCoroutine(
            InvincibilityCoroutine()
        );
    }


    private IEnumerator InvincibilityCoroutine()
    {
        Debug.Log(
            "Player bất tử!"
        );


        yield return new WaitForSeconds(3f);


        Debug.Log(
            "Hết bất tử!"
        );
    }


    // =========================================================
    // VICTORY
    // =========================================================

    public void StartVictoryExitSequence()
    {
        SetPlayerControl(true);

        isVictoryExiting = true;


        Debug.Log(
            "Victory Exit Started!"
        );
    }


    private void HandleVictoryPull()
    {
        if (transform.position.y >
            victoryPullStartY)
        {
            rb.AddForce(
                Vector2.up *
                victoryPullForce,
                ForceMode2D.Force
            );
        }
    }


    // =========================================================
    // PUBLIC FUNCTIONS
    // =========================================================

    public void SetPlayerControl(
        bool isEnabled
    )
    {
        canMove =
            isEnabled;

        canShoot =
            isEnabled;


        if (!isEnabled)
        {
            moveInput =
                Vector2.zero;

            rb.velocity =
                Vector2.zero;
        }


        Debug.Log(
            "Player Control: " +
            isEnabled
        );
    }


    public void SetSpeedMultiplier(
        float multiplier
    )
    {
        speedMultiplier =
            multiplier;
    }


    public Transform GetVisualsTransform()
    {
        return playerVisuals;
    }
}