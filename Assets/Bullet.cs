using System.Collections;
using UnityEngine;

namespace _Project._Scripts.Gameplay.Projectiles
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        // =========================================================
        // BULLET TYPE
        // =========================================================

        public enum BulletBehavior
        {
            Straight,
            Homing,
            Explosive,
            Spinner
        }


        // =========================================================
        // BASIC SETTINGS
        // =========================================================

        [Header("Thông số cơ bản")]

        [SerializeField]
        private float speed = 10f;

        [SerializeField]
        private int damage = 1;

        [Tooltip("False = đạn Player, True = đạn Enemy")]
        [SerializeField]
        private bool isEnemyBullet = false;

        public int Damage
        {
            get { return damage; }
        }

        public float Speed
        {
            get;
            private set;
        }


        // =========================================================
        // LIFETIME
        // =========================================================

        [Header("Vòng đời đạn")]

        [SerializeField]
        private float lifetime = 5f;

        private Coroutine lifetimeCoroutine;


        // =========================================================
        // BEHAVIOR
        // =========================================================

        [Header("Hành vi đạn")]

        [SerializeField]
        private BulletBehavior behavior =
            BulletBehavior.Straight;


        // =========================================================
        // HOMING
        // =========================================================

        [Header("Homing")]

        [SerializeField]
        private float rotationSpeed = 200f;

        private Transform homingTarget;


        // =========================================================
        // EXPLOSION
        // =========================================================

        [Header("Explosion")]

        [SerializeField]
        private GameObject explosionVFXPrefab;


        // =========================================================
        // SPINNER
        // =========================================================

        [Header("Spinner")]

        [SerializeField]
        private float spinnerAngularSpeed = 720f;


        // =========================================================
        // INTERNAL
        // =========================================================

        private Rigidbody2D rb;
        private Camera mainCamera;

        private bool canBeDisabledOffscreen = true;


        // =========================================================
        // UNITY - AWAKE
        // =========================================================

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            mainCamera = Camera.main;

            Speed = speed;
        }


        // =========================================================
        // ON ENABLE
        // =========================================================

        private void OnEnable()
        {
            canBeDisabledOffscreen = true;

            Speed = speed;

            // Reset velocity
            rb.velocity = Vector2.zero;
            rb.angularVelocity = 0f;


            // -----------------------------------------------------
            // BULLET MOVEMENT
            // -----------------------------------------------------

            switch (behavior)
            {
                case BulletBehavior.Straight:

                case BulletBehavior.Explosive:

                case BulletBehavior.Spinner:

                    rb.velocity =
                        transform.up * Speed;

                    rb.angularVelocity = 0f;

                    break;


                case BulletBehavior.Homing:

                    FindHomingTarget();

                    rb.velocity =
                        transform.up * Speed;

                    break;
            }


            // -----------------------------------------------------
            // LIFETIME
            // -----------------------------------------------------

            if (lifetime > 0f)
            {
                lifetimeCoroutine =
                    StartCoroutine(
                        LifetimeRoutine()
                    );
            }
        }


        // =========================================================
        // ON DISABLE
        // =========================================================

        private void OnDisable()
        {
            if (lifetimeCoroutine != null)
            {
                StopCoroutine(
                    lifetimeCoroutine
                );

                lifetimeCoroutine = null;
            }

            if (rb != null)
            {
                rb.velocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }

            homingTarget = null;
        }


        // =========================================================
        // UPDATE
        // =========================================================

        private void Update()
        {
            if (behavior ==
                BulletBehavior.Homing)
            {
                MoveHoming();
            }


            if (behavior ==
                BulletBehavior.Spinner)
            {
                transform.Rotate(
                    0f,
                    0f,
                    spinnerAngularSpeed *
                    Time.deltaTime
                );
            }


            if (canBeDisabledOffscreen)
            {
                CheckIfOffScreen();
            }
        }


        // =========================================================
        // HOMING
        // =========================================================

        private void FindHomingTarget()
        {
            string targetTag;


            if (isEnemyBullet)
            {
                targetTag = "Player";
            }
            else
            {
                targetTag = "Boss";
            }


            GameObject target =
                GameObject.FindGameObjectWithTag(
                    targetTag
                );


            if (target != null)
            {
                homingTarget =
                    target.transform;
            }
            else
            {
                homingTarget = null;
            }
        }


        private void MoveHoming()
        {
            // Không có mục tiêu
            // -> bay thẳng
            if (homingTarget == null)
            {
                rb.velocity =
                    transform.up * Speed;

                rb.angularVelocity = 0f;

                return;
            }


            Vector2 direction =
                (Vector2)homingTarget.position -
                rb.position;


            if (direction.sqrMagnitude <= 0.001f)
            {
                return;
            }


            direction.Normalize();


            float rotateAmount =
                Vector3.Cross(
                    direction,
                    transform.up
                ).z;


            rb.angularVelocity =
                -rotateAmount *
                rotationSpeed;


            rb.velocity =
                transform.up * Speed;
        }


        // =========================================================
        // COLLISION
        // =========================================================

        private void OnTriggerEnter2D(
            Collider2D other
        )
        {
            bool validTarget = false;


            // -----------------------------------------------------
            // PLAYER BULLET
            // -----------------------------------------------------

            if (!isEnemyBullet &&
                other.CompareTag("Boss"))
            {
                validTarget = true;

                // Không cần BossHealth.cs
                // Nếu Boss có hàm TakeDamage(int)
                // thì Unity sẽ tự gọi.
                other.SendMessage(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver
                );
            }


            // -----------------------------------------------------
            // ENEMY BULLET
            // -----------------------------------------------------

            else if (isEnemyBullet &&
                     other.CompareTag("Player"))
            {
                validTarget = true;

                // Không cần PlayerState.cs
                // Nếu Player có hàm TakeDamage(int)
                // thì Unity sẽ tự gọi.
                other.SendMessage(
                    "TakeDamage",
                    damage,
                    SendMessageOptions.DontRequireReceiver
                );
            }


            // -----------------------------------------------------
            // HIT
            // -----------------------------------------------------

            if (validTarget)
            {
                CreateExplosion();

                // Trả đạn về Pool
                gameObject.SetActive(false);
            }
        }


        // =========================================================
        // EXPLOSION
        // =========================================================

        private void CreateExplosion()
        {
            if (behavior !=
                BulletBehavior.Explosive)
            {
                return;
            }


            if (explosionVFXPrefab == null)
            {
                return;
            }


            Instantiate(
                explosionVFXPrefab,
                transform.position,
                Quaternion.identity
            );
        }


        // =========================================================
        // OFF SCREEN
        // =========================================================

        private void CheckIfOffScreen()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }


            if (mainCamera == null)
            {
                return;
            }


            Vector3 viewportPosition =
                mainCamera.WorldToViewportPoint(
                    transform.position
                );


            if (
                viewportPosition.x < -0.1f ||
                viewportPosition.x > 1.1f ||
                viewportPosition.y < -0.1f ||
                viewportPosition.y > 1.1f
            )
            {
                gameObject.SetActive(false);
            }
        }


        // =========================================================
        // LIFETIME
        // =========================================================

        private IEnumerator LifetimeRoutine()
        {
            yield return new WaitForSeconds(
                lifetime
            );


            gameObject.SetActive(false);
        }
    }
}