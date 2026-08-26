using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "RadialPattern", menuName = "Danmaku/Patterns/Radial Pattern")]
public class RadialPatternSO : BulletPatternSO
{
    [Header("Radial Pattern Settings")]
    public int bulletCount = 24;
    public float delayBetweenRings = 1.2f;
    public float spinPerRing = 5f; // Góc xoay nhẹ sau mỗi vòng đạn

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        float currentAngleOffset = 0f;

        do
        {
            float angleStep = 360f / Mathf.Max(1, bulletCount);
            float angle = currentAngleOffset;

            for (int i = 0; i < bulletCount; i++)
            {
                spawner.SpawnBulletDirection(angle, bulletSpeed, bulletPoolTag);
                angle += angleStep;
            }

            currentAngleOffset += spinPerRing;
            yield return new WaitForSeconds(delayBetweenRings);

        } while (isLooping);
    }
}
