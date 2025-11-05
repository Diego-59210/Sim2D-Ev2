using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SimpleCollider2D))]
public class TriggerZone2D : MonoBehaviour
{
    public string targetTag = "Player";

    [System.Serializable] public class ColliderEvent : UnityEvent<SimpleCollider2D> { }

    [Header("Events")]
    public ColliderEvent OnObjectEnter;
    public ColliderEvent OnObjectStay;
    public ColliderEvent OnObjectExit;

    private HashSet<SimpleCollider2D> objectsInside = new HashSet<SimpleCollider2D>();
    private SimpleCollider2D zoneCollider;

    void Awake()
    {
        zoneCollider = GetComponent<SimpleCollider2D>();
        zoneCollider.isTrigger = true;
    }

    void Update()
    {
        var allColliders = CustomCollisionManager.instance.GetAllColliders().ToArray();

        foreach (var col in allColliders)
        {
            if (col == null) continue; 
            if (!col.gameObject.activeInHierarchy) continue;
            if (!string.IsNullOrEmpty(targetTag) && !col.CompareTag(targetTag)) continue;

            bool isOverlapping = CustomCollisionManager.CheckOverlap(zoneCollider, col);

            if (isOverlapping && !objectsInside.Contains(col))
            {
                objectsInside.Add(col);
                OnObjectEnter?.Invoke(col);
            }
            else if (isOverlapping)
            {
                OnObjectStay?.Invoke(col);
            }
            else if (!isOverlapping && objectsInside.Contains(col))
            {
                objectsInside.Remove(col);
                OnObjectExit?.Invoke(col);
            }
        }
    }

}