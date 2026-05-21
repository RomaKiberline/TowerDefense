using UnityEngine;

public class PooledObject : MonoBehaviour
{
    public GameObject prefab;

    public void ReturnToPool()
    {
        if (ObjectPool.instance != null && prefab != null)
        {
            ObjectPool.instance.ReturnObject(prefab, gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}