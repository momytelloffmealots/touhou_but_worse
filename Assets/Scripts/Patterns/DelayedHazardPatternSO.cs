using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "DelayedHazardPattern", menuName = "Danmaku/Patterns/P09 - Static Hazard Pattern")]
public class DelayedHazardPatternSO : BulletPatternSO
{
    [Header("Static Hazard Core Settings")]
    [Tooltip("Thời gian đạn bay tỏa ra ban đầu trước khi đứng yên đóng băng (giây). Đặt = 0.2s - 0.5s để tạo hiệu ứng bắn xòe rồi dừng lơ lửng")]
    public float expandDuration = 0.3f;

    [Tooltip("Thời gian đạn đứng yên đóng băng lơ lửng trước khi đồng loạt lao đi (giây)")]
    public float freezeDuration = 1.5f;

    [Tooltip("Bán kính vòng đạn nếu muốn sinh ra sẵn xung quanh Spawner (dùng khi expandDuration = 0)")]
    public float spawnRadius = 0f;

    [Header("Launch Behavior")]
    [Tooltip("Nếu true: Đạn từ vị trí đóng băng sẽ quay đầu ngắm về phía Player khi lao đi")]
    public bool aimAtPlayerOnLaunch = true;

    [Tooltip("Hệ số nhân tốc độ đạn khi lao đi sau khi hết đóng băng (ví dụ: 1.4 = lao đi nhanh hơn 40%)")]
    public float launchSpeedMultiplier = 1.4f;

    [Header("Pattern Structure")]
    [Tooltip("Số đạn trong một vòng đóng băng")]
    public int bulletCount = 18;

    [Tooltip("Thời gian giữa các đợt tạo vòng đạn đóng băng mới")]
    public float burstInterval = 2.5f;

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        do
        {
            float angleStep = 360f / Mathf.Max(1, bulletCount);
            float startAngle = Random.Range(0f, 360f);

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + (i * angleStep);
                float rad = angle * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);

                Vector3 spawnPos = spawner.transform.position;
                if (spawnRadius > 0f)
                {
                    spawnPos += dir * spawnRadius;
                }

                spawner.SpawnBulletCustom(
                    spawnPos, 
                    angle, 
                    bulletSpeed, 
                    bulletPoolTag, 
                    0f, 
                    expandDuration, 
                    freezeDuration, 
                    aimAtPlayerOnLaunch, 
                    target, 
                    launchSpeedMultiplier
                );
            }

            yield return new WaitForSeconds(burstInterval);

        } while (isLooping);
    }
}
