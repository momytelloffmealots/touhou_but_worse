using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance { get; private set; }


    [System.Serializable]
    public class Pool
    {
        public GameObject prefab;

        [Min(1)]
        public int amount = 20;
    }


    [Header("Các Pool")]
    [SerializeField]
    private List<Pool> pools =
        new List<Pool>();


    // Danh sách object đã tạo
    private Dictionary<
        GameObject,
        List<GameObject>
    > poolDictionary =
        new Dictionary<
            GameObject,
            List<GameObject>
        >();


    private void Awake()
    {
        // Singleton
        if (Instance != null &&
            Instance != this)
        {
            Destroy(gameObject);
            return;
        }


        Instance = this;
    }


    private void Start()
    {
        CreatePools();
    }


    // =========================================================
    // CREATE POOL
    // =========================================================

    private void CreatePools()
    {
        foreach (Pool pool in pools)
        {
            if (pool.prefab == null)
            {
                Debug.LogWarning(
                    "[ObjectPooler] Có Pool chưa gán Prefab!"
                );

                continue;
            }


            if (poolDictionary.ContainsKey(
                pool.prefab))
            {
                continue;
            }


            List<GameObject> objectList =
                new List<GameObject>();


            for (int i = 0;
                 i < pool.amount;
                 i++)
            {
                GameObject obj =
                    CreateObject(pool.prefab);


                objectList.Add(obj);
            }


            poolDictionary.Add(
                pool.prefab,
                objectList
            );
        }
    }


    // =========================================================
    // CREATE OBJECT
    // =========================================================

    private GameObject CreateObject(
        GameObject prefab
    )
    {
        GameObject obj =
            Instantiate(prefab);


        obj.name =
            prefab.name + "_Pooled";


        obj.SetActive(false);


        obj.transform.SetParent(
            transform
        );


        return obj;
    }


    // =========================================================
    // GET OBJECT
    // =========================================================

    public GameObject GetPooledObject(
        GameObject prefab
    )
    {
        if (prefab == null)
        {
            Debug.LogWarning(
                "[ObjectPooler] Prefab = NULL!"
            );

            return null;
        }


        // Nếu prefab chưa có pool
        if (!poolDictionary.ContainsKey(
            prefab))
        {
            poolDictionary.Add(
                prefab,
                new List<GameObject>()
            );
        }


        List<GameObject> objectList =
            poolDictionary[prefab];


        // Tìm object đang tắt
        foreach (GameObject obj in objectList)
        {
            if (!obj.activeInHierarchy)
            {
                return obj;
            }
        }


        // Pool hết -> tạo thêm
        GameObject newObject =
            CreateObject(prefab);


        objectList.Add(newObject);


        return newObject;
    }
}