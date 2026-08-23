using UnityEngine;

public class DanmakuSpawner : MonoBehaviour
{
    [Header("Active Pattern Configuration")]
    [Tooltip("Gán asset ScriptableObject dạng bắn vào đây (Tạo từ Create -> Danmaku -> Patterns)")]
    public BulletPatternSO activePattern;

    [Header("Targeting")]
    [Tooltip("Transform mục tiêu (ví dụ: Player) dành cho các pattern bắn đuổi")]
    public Transform targetTransform;

    [Header("Auto Play")]
    public bool autoStartFiring = true;

    private Coroutine firingCoroutine;

    void Start()
    {
        if (autoStartFiring && activePattern != null)
        {
            StartFiring();
        }
    }

    /// <summary>
    /// Bắt đầu bắn theo activePattern đang gán.
    /// </summary>
    public void StartFiring()
    {
        StopFiring();

        if (activePattern != null)
        {
            firingCoroutine = StartCoroutine(activePattern.ExecutePattern(this, targetTransform));
        }
        else
        {
            Debug.LogWarning($"[DanmakuSpawner] Chưa gán ActivePattern trên GameObject: {gameObject.name}");
        }
    }

    /// <summary>
    /// Đổi dạng bắn linh hoạt ngay trong game (rất phù hợp cho Boss chuyển Phase / Spellcard).
    /// </summary>
    public void SetPattern(BulletPatternSO newPattern, bool startImmediately = true)
    {
        activePattern = newPattern;
        if (startImmediately)
        {
            StartFiring();
        }
    }

    /// <summary>
    /// Dừng bắn pattern hiện tại.
    /// </summary>
    public void StopFiring()
    {
        if (firingCoroutine != null)
        {
            StopCoroutine(firingCoroutine);
            firingCoroutine = null;
        }
    }

    /// <summary>
    /// Helper method bắn đạn đơn giản.
    /// </summary>
    public void SpawnBulletDirection(float angleDegrees, float speed, string poolTag)
    {
        SpawnBulletCustom(transform.position, angleDegrees, speed, poolTag, 0f, 0f, 0f, false, null, 1f);
    }

    /// <summary>
    /// Helper method bắn đạn nâng cao cơ bản.
    /// </summary>
    public void SpawnBulletAdvanced(float angleDegrees, float speed, string poolTag, float curveSpeed = 0f, float delayTime = 0f, bool aimOnLaunch = false, Transform target = null)
    {
        SpawnBulletCustom(transform.position, angleDegrees, speed, poolTag, curveSpeed, 0f, delayTime, aimOnLaunch, target, 1f);
    }

    /// <summary>
    /// Helper method bắn đạn linh hoạt đầy đủ tùy chỉnh (Vị trí, Tỏa đạn, Đóng băng, Nhắm bắn, Tốc độ phóng).
    /// </summary>
    public void SpawnBulletCustom(Vector3 spawnPosition, float angleDegrees, float speed, string poolTag, float curveSpeed = 0f, float expandTime = 0f, float freezeTime = 0f, bool aimOnLaunch = false, Transform target = null, float launchSpeedMult = 1f)
    {
        if (ObjectPool.Instance == null)
        {
            Debug.LogError("[DanmakuSpawner] Chưa khởi tạo ObjectPool trong Scene!");
            return;
        }

        float radian = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);

        GameObject bulletObj = ObjectPool.Instance.SpawnFromPool(poolTag, spawnPosition, Quaternion.identity);
        if (bulletObj != null)
        {
            Bullet bulletScript = bulletObj.GetComponent<Bullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(direction, speed, poolTag, curveSpeed, expandTime, freezeTime, aimOnLaunch, target, launchSpeedMult);
            }
        }
    }
}
