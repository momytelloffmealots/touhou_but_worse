using System.Collections;
using UnityEngine;

public class DanmakuSpawner : MonoBehaviour
{
    [Header("Pool Tag Configuration")]
    public string bulletPoolTag = "EnemyBullet";

    [Header("Target Target (Bắn đuổi Player)")]
    public Transform playerTransform;

    [Header("Bullet Speed Settings")]
    public float bulletSpeed = 7f;

    [Header("Firing Rate (Seconds)")]
    public float fireRate = 0.08f;

    private Coroutine activePattern;

    void Start()
    {
        // Mặc định chạy thử nghiệm quỹ đạo Xoắn ốc kép khi Play
        StartPattern("radial");
    }

    // Bắt đầu một pattern cụ thể
    public void StartPattern(string patternType)
    {
        StopActivePattern();

        switch (patternType.ToLower())
        {
            case "radial":
                activePattern = StartCoroutine(FireRadialPatternRoutine(24));
                break;
            case "spiral":
                activePattern = StartCoroutine(FireSpiralPatternRoutine());
                break;
            case "spiral_double":
                activePattern = StartCoroutine(FireDoubleSpiralPatternRoutine());
                break;
        }
    }

    // Dừng pattern hiện tại
    public void StopActivePattern()
    {
        if (activePattern != null)
        {
            StopCoroutine(activePattern);
            activePattern = null;
        }
    }

    // 1. Quỹ đạo Vòng tròn (Radial Ring)
    private IEnumerator FireRadialPatternRoutine(int bulletCount)
    {
        while (true)
        {
            float angleStep = 360f / bulletCount;
            float angle = 0f;

            for (int i = 0; i < bulletCount; i++)
            {
                SpawnBulletAtAngle(angle);
                angle += angleStep;
            }

            yield return new WaitForSeconds(1.2f); // Bắn vòng mới sau mỗi 1.2s
        }
    }

    // 2. Quỹ đạo Xoắn ốc đơn (Single Spiral)
    private IEnumerator FireSpiralPatternRoutine()
    {
        float angle = 0f;
        while (true)
        {
            SpawnBulletAtAngle(angle);

            angle += 7f; // Góc lệch viên tiếp theo
            if (angle >= 360f) angle -= 360f;

            yield return new WaitForSeconds(fireRate);
        }
    }

    // 3. Quỹ đạo Xoắn ốc kép (Double Spiral)
    private IEnumerator FireDoubleSpiralPatternRoutine()
    {
        float angle = 0f;
        while (true)
        {
            // Bắn 2 viên đối xứng nhau qua tâm (lệch 180 độ)
            SpawnBulletAtAngle(angle);
            SpawnBulletAtAngle(angle + 180f);

            angle += 5f; // Góc lệch xoay tiếp theo
            if (angle >= 360f) angle -= 360f;

            yield return new WaitForSeconds(fireRate);
        }
    }

    // 4. Bắn ngắm trực diện Player (N-Way targeted pattern)
    // Bạn có thể gọi hàm này trực tiếp từ các sự kiện hoặc script AI của Enemy
    public void FireTargetedPattern(int streamCount, float spreadAngle)
    {
        if (playerTransform == null)
        {
            Debug.LogWarning("Chưa gán PlayerTransform để định hướng bắn.");
            return;
        }

        // Tính toán góc hướng tới Player
        Vector2 dirToPlayer = playerTransform.position - transform.position;
        float baseAngle = Mathf.Atan2(dirToPlayer.y, dirToPlayer.x) * Mathf.Rad2Deg;

        float startAngle = baseAngle - (spreadAngle / 2f);
        float angleStep = streamCount > 1 ? spreadAngle / (streamCount - 1) : 0f;

        for (int i = 0; i < streamCount; i++)
        {
            float currentAngle = startAngle + (angleStep * i);
            SpawnBulletAtAngle(currentAngle);
        }
    }

    // Lấy đạn từ Pool ra và cấu hình thông số bay theo góc mong muốn
    private void SpawnBulletAtAngle(float angle)
    {
        if (ObjectPool.Instance == null)
        {
            Debug.LogError("Chưa kéo thả ObjectPool script vào Game Object nào trong Scene!");
            return;
        }

        // Đổi góc sang Vector hướng di chuyển
        float radian = angle * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);

        GameObject bulletObj = ObjectPool.Instance.SpawnFromPool(bulletPoolTag, transform.position, Quaternion.identity);
        if (bulletObj != null)
        {
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(direction, bulletSpeed, bulletPoolTag);
            }
        }
    }
}
