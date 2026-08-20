using UnityEngine;

public class Bullet : MonoBehaviour
{
    private Vector3 direction;
    private float speed;
    private string poolTag;
    private float maxLifetime = 5f; // Tự động trả về pool sau 5 giây đề phòng đạn bị kẹt
    private float lifetimeTimer;

    public void Initialize(Vector3 dir, float moveSpeed, string tag)
    {
        direction = dir.normalized;
        speed = moveSpeed;
        poolTag = tag;
        lifetimeTimer = 0f;

        // Xoay viên đạn theo hướng di chuyển (giả định sprite gốc hướng lên)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle - 90f, Vector3.forward);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;

        // Trở lại pool khi hết thời gian sống hoặc ra khỏi giới hạn màn hình
        lifetimeTimer += Time.deltaTime;
        if (lifetimeTimer >= maxLifetime || IsOutOfBounds())
        {
            ReturnToPool();
        }
    }

    private bool IsOutOfBounds()
    {
        // Kiểm tra đơn giản: đạn bay quá xa tâm màn hình (ví dụ 20 đơn vị)
        return transform.position.magnitude > 20f;
    }

    private void ReturnToPool()
    {
        if (ObjectPool.Instance != null && !string.IsNullOrEmpty(poolTag))
        {
            ObjectPool.Instance.ReturnToPool(poolTag, gameObject);
        }
        else
        {
            gameObject.SetActive(false); // Phương án dự phòng nếu thiếu ObjectPool
        }
    }
}
