using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanmakuSpawner : MonoBehaviour
{
    [Header("=== THANH CHỈNH TỐC ĐỘ ĐẠN BOSS (SPEED SLIDER) ===")]
    [Tooltip("Kéo thanh này để tăng/giảm tốc độ đạn Boss (0.5 = chậm một nửa, 1.0 = chuẩn, 2.0 = gấp đôi)")]
    [Range(0.1f, 3f)]
    public float bulletSpeedMultiplier = 1f;

    [Header("=== THỜI GIAN BOSS CHỜ KHI VÀO GAME ===")]
    [Tooltip("Thời gian Boss chờ khi vừa vào game trước khi bắt đầu bắn đợt đạn đầu tiên (giây)")]
    public float initialDelay = 3f;

    [Header("Active Pattern Configuration")]
    public BulletPatternSO BulletPattern;

    [Header("Touhou 8 Pattern Rotation")]
    [Tooltip("Bật chế độ tự động đổi Pattern ngẫu nhiên như Touhou")]
    public bool usePatternRotation = true;

    [Tooltip("Danh sách các Pattern đạn sẵn có của Boss để chọn ngẫu nhiên")]
    public List<BulletPatternSO> patternList = new List<BulletPatternSO>();

    [Tooltip("Thời gian bắn mỗi Pattern trước khi đổi (giây)")]
    public float patternDuration = 8f;

    [Tooltip("Thời gian nghỉ giữa các đợt bắn / Spell Card (giây)")]
    public float restBetweenPatterns = 2f;

    [Header("Targeting")]
    [Tooltip("Transform mục tiêu (ví dụ: Player) dành cho các pattern bắn đuổi")]
    public Transform targetTransform;

    [Header("Auto Play")]
    public bool autoStartFiring = true;

    private Coroutine firingCoroutine1;
    private Coroutine firingCoroutine2;
    private Coroutine rotationCoroutine;

//    void Awake()
//    {
//#if UNITY_EDITOR
//        if (patternList == null || patternList.Count == 0)
//        {
//            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:BulletPatternSO");
//            patternList = new List<BulletPatternSO>();
//            foreach (string g in guids)
//            {
//                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(g);
//                BulletPatternSO p = UnityEditor.AssetDatabase.LoadAssetAtPath<BulletPatternSO>(path);
//                if (p != null && !path.Contains("HybridPattern") && !path.Contains("CurvedPattern"))
//                {
//                    patternList.Add(p);
//                }
//            }
//        }
//#endif
//    }

    void Start()
    {
        if (autoStartFiring)
        {
            if (usePatternRotation && patternList != null && patternList.Count > 0)
            {
                StartPatternRotation();
            }
            else if (BulletPattern != null)
            {
                StartCoroutine(InitialDelayFiringRoutine());
            }
        }
    }

    private IEnumerator InitialDelayFiringRoutine()
    {
        if (initialDelay > 0f)
        {
            yield return new WaitForSeconds(initialDelay);
        }
        StartFiring();
    }

    void OnDisable()
    {
        StopPatternRotation();
    }

    /// <summary>
    /// Bắt đầu vòng lặp đổi Pattern ngẫu nhiên phong cách Touhou 8 (bắn đồng thời 2 pattern cùng lúc).
    /// </summary>
    public void StartPatternRotation()
    {
        StopPatternRotation();
        rotationCoroutine = StartCoroutine(PatternRotationRoutine());
    }

    public void StopPatternRotation()
    {
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
        StopFiring();
    }

    private IEnumerator PatternRotationRoutine()
    {
        if (initialDelay > 0f)
        {
            Debug.Log($"<color=yellow>[Touhou Boss]</color> Boss xuất hiện! Chuẩn bị giao tranh trong {initialDelay}s...");
            yield return new WaitForSeconds(initialDelay);
        }

        int lastIndexA = -1;
        int lastIndexB = -1;

        while (true)
        {
            if (patternList != null && patternList.Count > 0)
            {
                if (patternList.Count == 1)
                {
                    BulletPattern = patternList[0];
                    StartFiring();
                    Debug.Log($"<color=cyan>[Touhou Boss]</color> Kích hoạt Pattern: <b>{patternList[0].name}</b> ({patternDuration}s)");
                }
                else
                {
                    // Chọn ngẫu nhiên 2 pattern khác nhau
                    int indexA = Random.Range(0, patternList.Count);
                    int indexB = Random.Range(0, patternList.Count);
                    while (indexB == indexA)
                    {
                        indexB = Random.Range(0, patternList.Count);
                    }

                    // Tránh trùng lặp cặp pattern của lượt vừa rồi
                    if (patternList.Count >= 3)
                    {
                        int attempts = 0;
                        while (((indexA == lastIndexA && indexB == lastIndexB) || (indexA == lastIndexB && indexB == lastIndexA)) && attempts < 10)
                        {
                            indexA = Random.Range(0, patternList.Count);
                            indexB = Random.Range(0, patternList.Count);
                            while (indexB == indexA)
                            {
                                indexB = Random.Range(0, patternList.Count);
                            }
                            attempts++;
                        }
                    }

                    lastIndexA = indexA;
                    lastIndexB = indexB;

                    BulletPatternSO p1 = patternList[indexA];
                    BulletPatternSO p2 = patternList[indexB];
                    BulletPattern = p1;

                    StartDualFiring(p1, p2);
                    Debug.Log($"<color=cyan>[Touhou Boss]</color> Kích hoạt đồng thời 2 Pattern: <b>{p1.name}</b> + <b>{p2.name}</b> (Thời gian: {patternDuration}s)");
                }

                // Chờ thời gian bắn của đợt này
                yield return new WaitForSeconds(patternDuration);

                // Tạm dừng bắn (khoảng nghỉ đặc trưng của Touhou giữa các đợt đạn)
                StopFiring();
                Debug.Log($"<color=yellow>[Touhou Boss]</color> Nghỉ thở ({restBetweenPatterns}s) trước khi đổi cặp Pattern tiếp theo...");
                yield return new WaitForSeconds(restBetweenPatterns);
            }
            else
            {
                yield return new WaitForSeconds(1f);
            }
        }
    }

    /// <summary>
    /// Bắt đầu bắn đồng thời 2 Pattern cùng lúc.
    /// </summary>
    public void StartDualFiring(BulletPatternSO p1, BulletPatternSO p2)
    {
        StopFiring();

        if (p1 != null)
        {
            firingCoroutine1 = StartCoroutine(p1.ExecutePattern(this, targetTransform));
        }

        if (p2 != null)
        {
            firingCoroutine2 = StartCoroutine(p2.ExecutePattern(this, targetTransform));
        }
    }

    /// <summary>
    /// Bắt đầu bắn theo BulletPattern đơn lẻ.
    /// </summary>
    public void StartFiring()
    {
        StopFiring();

        if (BulletPattern != null)
        {
            firingCoroutine1 = StartCoroutine(BulletPattern.ExecutePattern(this, targetTransform));
        }
        else
        {
            Debug.LogWarning($"[DanmakuSpawner] Chưa gán BulletPattern trên GameObject: {gameObject.name}");
        }
    }

    /// <summary>
    /// Đổi dạng bắn linh hoạt ngay trong game (rất phù hợp cho Boss chuyển Phase / Spellcard).
    /// </summary>
    public void SetPattern(BulletPatternSO newPattern, bool startImmediately = true)
    {
        BulletPattern = newPattern;
        if (startImmediately)
        {
            StartFiring();
        }
    }

    /// <summary>
    /// Dừng toàn bộ các pattern đang bắn.
    /// </summary>
    public void StopFiring()
    {
        if (firingCoroutine1 != null)
        {
            StopCoroutine(firingCoroutine1);
            firingCoroutine1 = null;
        }

        if (firingCoroutine2 != null)
        {
            StopCoroutine(firingCoroutine2);
            firingCoroutine2 = null;
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

        float finalSpeed = speed * bulletSpeedMultiplier;
        float radian = angleDegrees * Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Cos(radian), Mathf.Sin(radian), 0f);

        GameObject bulletObj = ObjectPool.Instance.SpawnFromPool(poolTag, spawnPosition, Quaternion.identity);
        if (bulletObj != null)
        {
            EnemyBullet bulletScript = bulletObj.GetComponent<EnemyBullet>();
            if (bulletScript != null)
            {
                bulletScript.Initialize(direction, finalSpeed, poolTag, curveSpeed, expandTime, freezeTime, aimOnLaunch, target, launchSpeedMult);
            }
        }
    }
}
