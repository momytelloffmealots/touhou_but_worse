using UnityEngine;

/// <summary>
/// Quản lý đường bay và sát thương của đạn do Player bắn ra.
/// </summary>
public class PlayerBullet : MonoBehaviour
{
    public float speed = 16f;
    public float damage = 10f;
    public Vector3 direction = Vector3.up;
    public string poolTag = "PlayerBullet";

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Tự biến mất khi bay ra ngoài phạm vi màn hình
        if (transform.position.magnitude > 25f)
        {
            Despawn();
        }
    }

    public void Despawn()
    {
        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
