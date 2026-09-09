using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    [Header("Vị trí các điểm bắn")]
    [SerializeField] private Transform firePointHead;
    [SerializeField] private Transform firePointLeft;
    [SerializeField] private Transform firePointRight;

    [Header("Prefabs các loại đạn")]
    [SerializeField] private GameObject straightShotPrefab;
    [SerializeField] private GameObject diagonalShotPrefab;
    [SerializeField] private GameObject homingShotPrefab;

    [Header("Thông số Bắn")]
    [SerializeField] private float fireRate = 0.1f;

    [Tooltip("Góc bắn của đạn chéo")]
    [SerializeField] private float diagonalAngle = 15f;

    [Header("Input")]
    [SerializeField] private bool shootWithMouse = true;
    [SerializeField] private bool shootWithZ = true;

    // =========================================================
    // BIẾN
    // =========================================================

    private float fireTimer;

    [Header("Số lượng đạn các loại (Bắn đồng thời tất cả)")]
    [Tooltip("Số đạn thẳng (1 viên ở giữa hoặc 2 viên 2 bên)")]
    [SerializeField] private int currentStraightShots = 2;

    [Tooltip("Số cặp đạn chéo bắn xòe 2 bên")]
    [SerializeField] private int currentDiagonalShots = 1;

    [Tooltip("Số đạn tự tìm mục tiêu Boss")]
    [SerializeField] private int currentHomingShots = 1;

    [Header("Component References (Kéo trực tiếp vào Inspector)")]
    [SerializeField] private ObjectPool objectPool;
    [SerializeField] private PlayerHealth playerHealth;

    // =========================================================
    // AWAKE & START
    // =========================================================

    private void Awake()
    {
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }
    }

    private void Start()
    {
        if (objectPool == null)
        {
            objectPool = ObjectPool.Instance;
        }

        // Cho phép bắn ngay lập tức khi bắt đầu game
        fireTimer = fireRate;

        if (objectPool == null)
        {
            Debug.LogError(
                "[PlayerShooting] KHÔNG TÌM THẤY ObjectPool!"
            );
        }
    }


    // =========================================================
    // UPDATE
    // =========================================================

    private void Update()
    {
        if (playerHealth != null && playerHealth.IsDead) return;
        if (UIManager.Instance != null && UIManager.Instance.IsGameOver) return;

        fireTimer += Time.deltaTime;

        // ==========================================
        // CLICK CHUỘT TRÁI
        // ==========================================

        if (shootWithMouse &&
            Input.GetMouseButton(0))
        {
            TryToShoot();
        }


        // ==========================================
        // GIỮ PHÍM Z
        // ==========================================

        if (shootWithZ &&
            Input.GetKey(KeyCode.Z))
        {
            TryToShoot();
        }
    }


    // =========================================================
    // TRY TO SHOOT
    // =========================================================

    public void TryToShoot()
    {
        if (!enabled) return;
        if (playerHealth != null && playerHealth.IsDead) return;
        if (UIManager.Instance != null && UIManager.Instance.IsGameOver) return;

        if (fireTimer < fireRate)
            return;

        if (objectPool == null)
        {
            objectPool = ObjectPool.Instance;

            if (objectPool == null)
            {
                Debug.LogError(
                    "[PlayerShooting] ObjectPool chưa tồn tại!"
                );

                return;
            }
        }

        Shoot();

        fireTimer = 0f;
    }


    // =========================================================
    // SHOOT
    // =========================================================

    private void Shoot()
    {
        HandleStraightShots();

        HandleDiagonalShots();

        HandleHomingShots();
    }


    // =========================================================
    // ĐẠN THẲNG
    // =========================================================

    private void HandleStraightShots()
    {
        if (currentStraightShots <= 0)
            return;

        if (straightShotPrefab == null)
        {
            Debug.LogError(
                "[PlayerShooting] CHƯA GÁN Straight Shot Prefab!"
            );

            return;
        }


        // 1 viên
        if (currentStraightShots == 1)
        {
            SpawnProjectile(
                straightShotPrefab,
                firePointHead
            );

            return;
        }


        // 2 viên
        SpawnProjectile(
            straightShotPrefab,
            firePointLeft
        );

        SpawnProjectile(
            straightShotPrefab,
            firePointRight
        );


        // 3 viên trở lên
        if (currentStraightShots >= 3)
        {
            SpawnProjectile(
                straightShotPrefab,
                firePointHead
            );
        }
    }


    // =========================================================
    // ĐẠN CHÉO
    // =========================================================

    private void HandleDiagonalShots()
    {
        if (currentDiagonalShots <= 0)
            return;

        if (diagonalShotPrefab == null)
            return;


        for (int i = 0;
             i < currentDiagonalShots;
             i++)
        {
            float angle =
                diagonalAngle * (i + 1);


            Quaternion leftRotation =
                firePointLeft.rotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    -angle
                );


            Quaternion rightRotation =
                firePointRight.rotation *
                Quaternion.Euler(
                    0f,
                    0f,
                    angle
                );


            SpawnProjectile(
                diagonalShotPrefab,
                firePointLeft,
                leftRotation
            );


            SpawnProjectile(
                diagonalShotPrefab,
                firePointRight,
                rightRotation
            );
        }
    }


    // =========================================================
    // HOMING
    // =========================================================

    private void HandleHomingShots()
    {
        if (currentHomingShots <= 0)
            return;

        if (homingShotPrefab == null)
            return;


        for (int i = 0;
             i < currentHomingShots;
             i++)
        {
            SpawnProjectile(
                homingShotPrefab,
                firePointHead
            );
        }
    }



    // =========================================================
    // SPAWN PROJECTILE
    // =========================================================

    private void SpawnProjectile(
        GameObject prefab,
        Transform spawnPoint
    )
    {
        if (spawnPoint == null)
        {
            Debug.LogError(
                "[PlayerShooting] FIRE POINT CHƯA ĐƯỢC GÁN!"
            );

            return;
        }


        SpawnProjectile(
            prefab,
            spawnPoint,
            spawnPoint.rotation
        );
    }


    private void SpawnProjectile(
        GameObject prefab,
        Transform spawnPoint,
        Quaternion rotation
    )
    {
        if (prefab == null)
            return;

        if (spawnPoint == null)
        {
            Debug.LogError(
                "[PlayerShooting] Fire Point = NULL!"
            );

            return;
        }


        if (objectPool == null)
        {
            Debug.LogError(
                "[PlayerShooting] ObjectPool = NULL!"
            );

            return;
        }


        GameObject projectile =
            objectPool.GetPooledObject(
                prefab
            );


        if (projectile == null)
        {
            Debug.LogWarning(
                "[PlayerShooting] Không lấy được projectile từ Pool!"
            );

            return;
        }


        // Đặt vị trí
        projectile.transform.position =
            spawnPoint.position;


        // Đặt hướng
        projectile.transform.rotation =
            rotation;


        // Bật đạn
        //
        // QUAN TRỌNG:
        // Không reset velocity về Vector2.zero
        // sau dòng này.
        //
        // Bullet.cs / BouncingBullet.cs
        // sẽ tự thiết lập velocity trong OnEnable().
        projectile.SetActive(true);
    }


    // =========================================================
    // UPGRADE
    // =========================================================

    //public void AddStraightShot(int amount)
    //{
    //    currentStraightShots += amount;
    //}


    //public void AddDiagonalShot(int amount)
    //{
    //    currentDiagonalShots += amount;
    //}


    //public void AddHomingShot(int amount)
    //{
    //    currentHomingShots += amount;
    //}


    //// =========================================================
    //// RESET WEAPON
    //// =========================================================

    //public void ResetWeapon()
    //{
    //    currentStraightShots = 2;

    //    currentDiagonalShots = 1;

    //    currentHomingShots = 1;
    //}
}