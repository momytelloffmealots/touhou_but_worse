using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "StackedWavePattern", menuName = "Danmaku/Patterns/P08 - Stacked Wave Pattern")]
public class StackedWavePatternSO : BulletPatternSO
{
    [Header("Stacked Wave Settings")]
    [Tooltip("Số lớp/tầng đạn chồng lên nhau trong 1 đợt bắn")]
    public int layerCount = 3;

    [Tooltip("Số đạn trên mỗi vòng")]
    public int bulletsPerRing = 20;

    [Tooltip("Tốc độ đạn tầng nhanh nhất")]
    public float maxSpeed = 10f;

    [Tooltip("Tốc độ đạn tầng chậm nhất")]
    public float minSpeed = 5f;

    [Tooltip("Góc xoay lệch giữa các tầng đạn (độ)")]
    public float layerAngleOffset = 9f;

    [Tooltip("Thời gian chờ giữa các đợt sóng đạn lớn")]
    public float waveInterval = 1.5f;

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        do
        {
            float angleStep = 360f / Mathf.Max(1, bulletsPerRing);

            for (int layer = 0; layer < layerCount; layer++)
            {
                // Tính toán tốc độ xen kẽ từ nhanh tới chậm cho từng layer
                float speed = Mathf.Lerp(maxSpeed, minSpeed, (float)layer / Mathf.Max(1, layerCount - 1));
                float startAngle = layer * layerAngleOffset;

                for (int i = 0; i < bulletsPerRing; i++)
                {
                    float angle = startAngle + (i * angleStep);
                    spawner.SpawnBulletDirection(angle, speed, bulletPoolTag);
                }

                // Chờ một khoảng rất ngắn giữa các tầng đạn
                yield return new WaitForSeconds(0.08f);
            }

            yield return new WaitForSeconds(waveInterval);

        } while (isLooping);
    }
}
