using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Di chuyển Enemy ngẫu nhiên giữa các điểm (Waypoints) đã setup sẵn.
/// Hỗ trợ mượt mà (SmoothStep / Ease-In-Out) và tự động chờ giữa các lần di chuyển.
/// </summary>
public class EnemyWaypointMovement : MonoBehaviour
{
    [Header("Waypoints Setup")]
    [Tooltip("Danh sách các vị trí (Transform) mà Enemy có thể di chuyển tới")]
    public List<Transform> waypoints = new List<Transform>();

    [Header("Movement Settings")]
    [Tooltip("Tốc độ di chuyển")]
    public float moveSpeed = 4f;

    [Tooltip("Thời gian chờ tối thiểu tại mỗi vị trí (giây)")]
    public float minWaitTime = 1f;

    [Tooltip("Thời gian chờ tối đa tại mỗi vị trí (giây)")]
    public float maxWaitTime = 2.5f;

    [Tooltip("Dùng chuyển động SmoothStep (Ease In/Out) giúp di chuyển mượt hơn")]
    public bool useSmoothEasing = true;

    [Header("Gizmos Debug Settings")]
    public Color gizmoColor = Color.cyan;
    public float gizmoRadius = 0.4f;

    private int currentWaypointIndex = -1;
    //private bool isMoving = false;
    private Coroutine movementCoroutine;

    void Start()
    {
        if (waypoints != null && waypoints.Count > 0)
        {
            StartMovement();
        }
        else
        {
            Debug.LogWarning($"[EnemyWaypointMovement] Chưa gán danh sách Waypoints trên GameObject: {gameObject.name}");
        }
    }

    public void StartMovement()
    {
        StopMovement();
        movementCoroutine = StartCoroutine(MoveRoutine());
    }

    public void StopMovement()
    {
        if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
        //isMoving = false;
    }

    private IEnumerator MoveRoutine()
    {
        while (true)
        {
            // Chọn vị trí tiếp theo không trùng với vị trí vừa đứng (nếu có từ 2 waypoints trở lên)
            int nextIndex = GetRandomNextWaypointIndex();
            if (nextIndex == -1) yield break;

            currentWaypointIndex = nextIndex;
            Transform targetWaypoint = waypoints[currentWaypointIndex];

            if (targetWaypoint != null)
            {
                Vector3 startPos = transform.position;
                Vector3 targetPos = targetWaypoint.position;
                float distance = Vector3.Distance(startPos, targetPos);

                if (distance > 0.01f)
                {
                    //isMoving = true;
                    float duration = distance / Mathf.Max(0.1f, moveSpeed);
                    float elapsedTime = 0f;

                    while (elapsedTime < duration)
                    {
                        elapsedTime += Time.deltaTime;
                        float t = Mathf.Clamp01(elapsedTime / duration);

                        if (useSmoothEasing)
                        {
                            // SmoothStep giúp tăng tốc nhẹ lúc xuất phát và giảm tốc mượt lúc dừng
                            t = Mathf.SmoothStep(0f, 1f, t);
                        }

                        transform.position = Vector3.Lerp(startPos, targetPos, t);
                        yield return null;
                    }

                    transform.position = targetPos;
                    //isMoving = false;
                }
            }

            // Dừng lại chờ ngẫu nhiên giữa minWaitTime và maxWaitTime
            float waitTime = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private int GetRandomNextWaypointIndex()
    {
        if (waypoints == null || waypoints.Count == 0) return -1;
        if (waypoints.Count == 1) return 0;

        int newIndex = currentWaypointIndex;
        while (newIndex == currentWaypointIndex)
        {
            newIndex = Random.Range(0, waypoints.Count);
        }
        return newIndex;
    }

    //public bool IsMoving ;

    // Hiển thị trực quan các vị trí Waypoint trong cửa sổ Scene View của Unity
    //private void OnDrawGizmosSelected()
    //{
    //    if (waypoints == null || waypoints.Count == 0) return;
    
    //    Gizmos.color = gizmoColor;

    //    for (int i = 0; i < waypoints.Count; i++)
    //    {
    //        if (waypoints[i] != null)
    //        {
    //            Gizmos.DrawWireSphere(waypoints[i].position, gizmoRadius);

    //            for (int j = i + 1; j < waypoints.Count; j++)
    //            {
    //                if (waypoints[j] != null)
    //                {
    //                    Gizmos.DrawLine(waypoints[i].position, waypoints[j].position);
    //                }
    //            }
    //        }
    //    }
    //}
}
