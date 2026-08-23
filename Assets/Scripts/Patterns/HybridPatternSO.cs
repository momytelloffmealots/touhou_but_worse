using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "HybridPattern", menuName = "Danmaku/Patterns/P10 - Hybrid Composite Pattern")]
public class HybridPatternSO : BulletPatternSO
{
    [Header("Hybrid Settings")]
    [Tooltip("Danh sách các Pattern thành phần sẽ được kích hoạt song song cùng lúc")]
    public List<BulletPatternSO> subPatterns = new List<BulletPatternSO>();

    public override IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target)
    {
        if (subPatterns == null || subPatterns.Count == 0)
        {
            Debug.LogWarning($"[HybridPatternSO] Danh sách subPatterns trống trên asset: {name}");
            yield break;
        }

        List<Coroutine> activeCoroutines = new List<Coroutine>();

        // Chạy tất cả các pattern thành phần song song thông qua Coroutine của Spawner
        foreach (var pattern in subPatterns)
        {
            if (pattern != null)
            {
                Coroutine c = spawner.StartCoroutine(pattern.ExecutePattern(spawner, target));
                activeCoroutines.Add(c);
            }
        }

        // Giữ Coroutine chính chạy nếu loop, hoặc chờ hoàn tất
        while (isLooping)
        {
            yield return new WaitForSeconds(1f);
        }

        // Nếu dừng loop, stop toàn bộ sub-coroutines
        foreach (var c in activeCoroutines)
        {
            if (c != null) spawner.StopCoroutine(c);
        }
    }
}
