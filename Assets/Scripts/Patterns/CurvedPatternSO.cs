using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "CurvedPattern", menuName = "Danmaku/Patterns/P07 - Curved Pattern")]
public class CurvedPatternSO : BulletPatternSO
{
    [Header("Curved Motion Settings")]
    [Tooltip("Góc uốn cong mỗi giây (độ/giây). Giá trị dương xoay trái, âm xoay phải")]
    public float curveAngularSpeed = 45f;

    [Tooltip("Số luồng bắn xoắn uốn cong")]
    public int streamCount = 4;

    [Tooltip("Tốc độ xoay của nòng bắn khi phát đạn")]
    public float emitterSpinSpeed = 12f;

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        float currentEmitterAngle = 0f;
        float streamStep = 360f / Mathf.Max(1, streamCount);

        do
        {
            for (int i = 0; i < streamCount; i++)
            {
                float angle = currentEmitterAngle + (i * streamStep);
                spawner.SpawnBulletAdvanced(angle, bulletSpeed, bulletPoolTag, curveAngularSpeed, 0f, false, null);
            }

            currentEmitterAngle += emitterSpinSpeed;
            if (currentEmitterAngle >= 360f) currentEmitterAngle -= 360f;

            yield return new WaitForSeconds(fireRate);

        } while (isLooping);
    }
}
