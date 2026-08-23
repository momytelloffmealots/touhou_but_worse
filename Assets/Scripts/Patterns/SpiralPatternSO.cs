using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "SpiralPattern", menuName = "Danmaku/Patterns/Spiral Pattern")]
public class SpiralPatternSO : BulletPatternSO
{
    [Header("Spiral Pattern Settings")]
    public float angleStep = 7f;
    public int arms = 1; // Số cánh xoắn ốc (1: đơn, 2: kép, N: nhiều cánh)

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        float angle = 0f;
        float armStep = 360f / Mathf.Max(1, arms);

        do
        {
            for (int arm = 0; arm < arms; arm++)
            {
                spawner.SpawnBulletDirection(angle + (arm * armStep), bulletSpeed, bulletPoolTag);
            }

            angle += angleStep;
            if (angle >= 360f) angle -= 360f;

            yield return new WaitForSeconds(fireRate);

        } while (isLooping);
    }
}
