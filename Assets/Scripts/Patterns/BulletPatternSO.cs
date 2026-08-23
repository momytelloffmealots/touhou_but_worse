using System.Collections;
using UnityEngine;

/// <summary>
/// Class cơ sở ScriptableObject cho tất cả các dạng bắn đạn (Danmaku Patterns).
/// Giúp tách bạch giữa dữ liệu/logic dạng bắn và GameObject Spawner.
/// </summary>
public abstract class BulletPatternSO : ScriptableObject
{
    [Header("Base Pattern Settings")]
    public string patternName = "Default Pattern";
    public string bulletPoolTag = "EnemyBullet";
    public float bulletSpeed = 7f;
    public float fireRate = 0.08f;
    public bool isLooping = true;

    /// <summary>
    /// Thực thi logic bắn đạn. Các class con ghi đè hàm này để tự định nghĩa dạng bắn.
    /// </summary>
    public abstract IEnumerator ExecutePattern(DanmakuSpawner spawner, Transform target);
}
