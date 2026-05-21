using UnityEngine;

public class Tower : MonoBehaviour
{
    public int cost = 100;
    public float range = 3f;
    public float fireRate = 1f;
    private float fireCountdown = 0f;

    [Header("Налаштування стрільби")]
    public GameObject projectilePrefab;

    void Update()
    {
        GameObject target = FindTarget();

        if (target != null)
        {
            fireCountdown -= Time.deltaTime;

            if (fireCountdown <= 0f)
            {
                Shoot(target);
                fireCountdown = 1f / fireRate;
            }
        }
    }

    GameObject FindTarget()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        GameObject bestTarget = null;
        float maxProgress = -1f;

        foreach (GameObject enemy in enemies)
        {
            if (!enemy.activeInHierarchy) continue;

            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance <= range)
            {
                EnemyMovement movement = enemy.GetComponent<EnemyMovement>();

                if (movement != null)
                {
                    float currentProgress = movement.GetProgress();

                    if (currentProgress > maxProgress)
                    {
                        maxProgress = currentProgress;
                        bestTarget = enemy;
                    }
                }
            }
        }

        return bestTarget;
    }

    void Shoot(GameObject target)
    {
        if (projectilePrefab == null) return;

        GameObject bulletGO;

        if (ObjectPool.instance != null)
        {
            bulletGO = ObjectPool.instance.GetObject(
                projectilePrefab,
                transform.position,
                Quaternion.identity
            );
        }
        else
        {
            bulletGO = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        }

        Projectile projectile = bulletGO.GetComponent<Projectile>();

        if (projectile != null)
        {
            projectile.Seek(target.transform);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
    }
}