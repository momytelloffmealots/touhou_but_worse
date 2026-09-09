using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private string poolTag;

    [Tooltip("Thời gian sống tối đa trước khi tự hủy nếu chưa bay ra khỏi màn hình (giây)")]
    public float maxLifetime = 15f;
    private float lifetimeTimer;

    // Advanced motion parameters
    private float angularSpeed = 0f;          // Tốc độ bẻ cong hướng bay (độ/giây) (Curved)
    private float expandTimer = 0f;           // Thời gian bay tỏa ra ban đầu trước khi đóng băng
    private float freezeTimer = 0f;           // Thời gian đứng yên đóng băng (Static Hazard)
    private bool isAimedOnLaunch = false;
    private Transform aimTarget = null;
    private float launchSpeedMultiplier = 1f; // Tốc độ đạn sau khi hết đóng băng

    public void Initialize(Vector3 dir, float moveSpeed, string tag, float curve = 0f, float expandTime = 0f, float freezeTime = 0f, bool aimedOnLaunch = false, Transform target = null, float launchSpeedMult = 1f)
    {
        direction = dir.normalized;
        speed = moveSpeed;
        poolTag = tag;
        lifetimeTimer = 0f;
        angularSpeed = curve;
        expandTimer = expandTime;
        freezeTimer = freezeTime;
        isAimedOnLaunch = aimedOnLaunch;
        aimTarget = target;
        launchSpeedMultiplier = launchSpeedMult;

        UpdateRotation();
    }

    void Update()
    {
        // Chỉ đếm thời gian sống khi đạn thực sự đang di chuyển
        if (freezeTimer <= 0f)
        {
            lifetimeTimer += Time.deltaTime;
        }

        // Phase 1: Bay tỏa ra ban đầu trước khi đóng băng (nếu có expandTimer)
        if (expandTimer > 0f)
        {
            expandTimer -= Time.deltaTime;
            transform.position += direction * speed * Time.deltaTime;
            return;
        }

        // Phase 2: Đứng yên đóng băng lơ lửng (nếu có freezeTimer)
        if (freezeTimer > 0f)
        {
            freezeTimer -= Time.deltaTime;

            // Khi vừa kết thúc khoảng thời gian đóng băng
            if (freezeTimer <= 0f)
            {
                speed *= launchSpeedMultiplier; // Tăng tốc đạn khi lao đi

                if (isAimedOnLaunch && aimTarget != null)
                {
                    Vector2 dirToTarget = aimTarget.position - transform.position;
                    if (dirToTarget.sqrMagnitude > 0.001f)
                    {
                        direction = dirToTarget.normalized;
                    }
                }
                UpdateRotation();
            }
            return; // Chưa di chuyển tiếp khi đang đóng băng
        }

        // Phase 3: Bay bình thường (hoặc uốn cong nếu có angularSpeed)
        if (angularSpeed != 0f)
        {
            float currentAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            currentAngle += angularSpeed * Time.deltaTime;
            float rad = currentAngle * Mathf.Deg2Rad;
            direction = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
            UpdateRotation();
        }

        transform.position += direction * speed * Time.deltaTime;

        if (lifetimeTimer >= maxLifetime || IsOutOfBounds())
        {
            ReturnToPool();
        }
    }

    private void UpdateRotation()
    {
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    private bool IsOutOfBounds()
    {
        Vector3 pos = transform.position;
        // Đảm bảo đạn đi hoàn toàn ra khỏi màn hình ở mọi tỷ lệ khung hình (16:9, 16:10, 21:9)
        return pos.x < -15f || pos.x > 15f || pos.y < -9f || pos.y > 9f;
    }

    public void ReturnToPool()
    {
        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
