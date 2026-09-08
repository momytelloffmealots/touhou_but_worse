using UnityEngine;

namespace _Project._Scripts.Gameplay.Projectiles
{
    public class BouncingBullet : Bullet
    {
        [Header("Bouncing Bullet Settings")]

        [Tooltip("Số lần đạn được phép nảy.")]
        [SerializeField] private int maxBounces = 3;

        [Tooltip("Thời gian tồn tại của đạn.")]
        [SerializeField] private float bulletLifetime = 8f;

        [Tooltip("Đạn của Enemy hay Player.")]
        [SerializeField] private bool isEnemyBullet = false;

        private int currentBounces;

        private Rigidbody2D rb;
        private float timer;

        private bool alreadyHit = false;


        // =========================================================
        // UNITY
        // =========================================================

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            if (rb == null)
            {
                rb = gameObject.AddComponent<Rigidbody2D>();
            }
        }


        private void OnEnable()
        {
            ResetBullet();
        }


        private void Update()
        {
            timer += Time.deltaTime;

            // Tự tắt sau một khoảng thời gian
            if (timer >= bulletLifetime)
            {
                gameObject.SetActive(false);
                return;
            }

            // Đảm bảo đạn vẫn bay
            if (rb != null &&
                rb.velocity.sqrMagnitude < 0.01f)
            {
                rb.velocity =
                    transform.up * Speed;
            }
        }


        // =========================================================
        // RESET
        // =========================================================

        private void ResetBullet()
        {
            currentBounces = maxBounces;

            timer = 0f;

            alreadyHit = false;

            if (rb == null)
            {
                rb = GetComponent<Rigidbody2D>();
            }

            if (rb == null)
                return;


            // Rigidbody
            rb.gravityScale = 0f;

            rb.isKinematic = false;

            rb.constraints =
                RigidbodyConstraints2D.None;


            rb.angularVelocity = 0f;

            rb.velocity =
                transform.up * Speed;
        }


        // =========================================================
        // TRIGGER COLLISION
        // Dùng cho Player / Boss nếu Collider là Trigger
        // =========================================================

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            HandleTargetHit(other.gameObject);
        }


        // =========================================================
        // PHYSICAL COLLISION
        // Dùng cho tường / môi trường
        // =========================================================

        private void OnCollisionEnter2D(
            Collision2D collision
        )
        {
            GameObject other =
                collision.gameObject;


            // Nếu đụng Player hoặc Boss
            // thì xử lý damage
            if (other.CompareTag("Player") ||
                other.CompareTag("Boss"))
            {
                HandleTargetHit(other);

                return;
            }


            // Nếu là môi trường
            // thì cho phép nảy
            HandleBounce(collision);
        }


        // =========================================================
        // DAMAGE
        // =========================================================

        private void HandleTargetHit(
            GameObject target
        )
        {
            if (alreadyHit)
                return;


            // -----------------------------------------------------
            // Bullet Player -> Boss
            // -----------------------------------------------------

            if (!isEnemyBullet &&
                target.CompareTag("Boss"))
            {
                alreadyHit = true;

                target.SendMessage(
                    "TakeDamage",
                    Damage,
                    SendMessageOptions.DontRequireReceiver
                );

                gameObject.SetActive(false);

                return;
            }


            // -----------------------------------------------------
            // Bullet Enemy -> Player
            // -----------------------------------------------------

            if (isEnemyBullet &&
                target.CompareTag("Player"))
            {
                alreadyHit = true;

                target.SendMessage(
                    "TakeDamage",
                    Damage,
                    SendMessageOptions.DontRequireReceiver
                );

                gameObject.SetActive(false);

                return;
            }
        }


        // =========================================================
        // BOUNCE
        // =========================================================

        private void HandleBounce(
            Collision2D collision
        )
        {
            currentBounces--;


            // Hết lượt nảy
            if (currentBounces <= 0)
            {
                gameObject.SetActive(false);

                return;
            }


            // -----------------------------------------------------
            // TÍNH HƯỚNG NẢY
            // -----------------------------------------------------

            if (collision.contactCount <= 0)
                return;


            ContactPoint2D contact =
                collision.GetContact(0);


            Vector2 incomingDirection =
                rb.velocity.normalized;


            Vector2 bounceDirection =
                Vector2.Reflect(
                    incomingDirection,
                    contact.normal
                );


            // Đổi hướng đạn
            rb.velocity =
                bounceDirection * Speed;


            // Đảm bảo đạn quay theo hướng bay
            float angle =
                Mathf.Atan2(
                    bounceDirection.y,
                    bounceDirection.x
                ) *
                Mathf.Rad2Deg;


            transform.rotation =
                Quaternion.Euler(
                    0f,
                    0f,
                    angle - 90f
                );
        }


        // =========================================================
        // RESET WHEN DISABLED
        // =========================================================

        private void OnDisable()
        {
            if (rb != null)
            {
                rb.velocity =
                    Vector2.zero;

                rb.angularVelocity = 0f;
            }
        }
    }
}