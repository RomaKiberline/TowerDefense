using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Transform target;

    public float speed = 12f;
    public int damage = 10;

    [Header("AoE & Effects")]
    public float explosionRadius = 0f;
    public float slowAmount = 0.5f;
    public float slowDuration = 2f;
    public bool isFreezingProjectile = false;

    [Header("Rotation")]
    public float rotationOffset = 0f;

    private PooledObject pooledObject;

    void Awake()
    {
        pooledObject = GetComponent<PooledObject>();
    }

    public void Seek(Transform _target)
    {
        target = _target;
    }

    void OnEnable()
    {
        target = null;
    }

    void Update()
    {
        if (target == null)
        {
            ReturnToPool();
            return;
        }

        Vector3 dir = target.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;

        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle + rotationOffset);

        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
    }

    void HitTarget()
    {
        if (explosionRadius > 0f)
            Explode();
        else
            ApplyEffect(target.gameObject);

        ReturnToPool();
    }

    void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Enemy"))
                ApplyEffect(collider.gameObject);
        }
    }

    void ApplyEffect(GameObject enemyObj)
    {
        EnemyMovement enemy = enemyObj.GetComponent<EnemyMovement>();

        if (enemy != null)
        {
            enemy.TakeDamage(damage);

            if (isFreezingProjectile)
                enemy.Slow(slowAmount, slowDuration);
        }
    }

    void ReturnToPool()
    {
        if (pooledObject != null)
        {
            pooledObject.ReturnToPool();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}