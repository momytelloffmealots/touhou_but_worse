using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "TargetedPattern", menuName = "Danmaku/Patterns/Targeted Pattern")]
public class TargetedPatternSO : BulletPatternSO
{
    [Header("Targeted Pattern Settings")]
    public int streamCount = 5;
    public float spreadAngle = 85f;
    public float intervalBetweenBursts = 1f;

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        do
        {
            if (target != null)
            {
                Vector2 dirToTarget = target.position - spawner.transform.position;
                float baseAngle = Mathf.Atan2(dirToTarget.y, dirToTarget.x) * Mathf.Rad2Deg;

                float startAngle = baseAngle - (spreadAngle / 2f);
                float angleStep = streamCount > 1 ? spreadAngle / (streamCount - 1) : 0f;

                for (int i = 0; i < streamCount; i++)
                {
                    float currentAngle = startAngle + (angleStep * i);
                    spawner.SpawnBulletDirection(currentAngle, bulletSpeed, bulletPoolTag);
                }
            }

            yield return new WaitForSeconds(intervalBetweenBursts);

        } while (isLooping);
    }
}
