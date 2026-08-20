using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;

    [System.Serializable]
    public class Pool
    {
        public string tag;          // Tên nhận diện của pool (ví dụ: "EnemyBullet")
        public GameObject prefab;   // Prefab đạn
        public int size;            // Số lượng đạn khởi tạo trước
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializePools();
    }

    void InitializePools()
    {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++)
            {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform); // Nhóm vào ObjectPool để giữ Hierarchy gọn gàng
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    // Lấy đạn từ Pool ra sử dụng
    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
    {
        if (!poolDictionary.ContainsKey(tag))
        {
            Debug.LogWarning("Pool với tag " + tag + " không tồn tại.");
            return null;
        }

        GameObject objectToSpawn;

        // Nếu pool hết đạn trống, tự động sinh thêm để tránh lỗi game
        if (poolDictionary[tag].Count == 0)
        {
            Pool poolConfig = pools.Find(p => p.tag == tag);
            if (poolConfig != null)
            {
                objectToSpawn = Instantiate(poolConfig.prefab);
                objectToSpawn.transform.SetParent(transform);
            }
            else
            {
                return null;
            }
        }
        else
        {
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    // Trả đạn về lại Pool khi không sử dụng
    public void ReturnToPool(string tag, GameObject obj)
    {
        obj.SetActive(false);
        if (!poolDictionary.ContainsKey(tag))
        {
            Destroy(obj);
            return;
        }
        poolDictionary[tag].Enqueue(obj);
    }
}
