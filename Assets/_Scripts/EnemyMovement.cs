using UnityEngine;
using UnityEngine.UI;

public class EnemyMovement : MonoBehaviour
{
    public Transform[] waypoints;
    public float speed = 2f;
    public int health = 30;
    public int goldReward = 50;

    [Header("Ефекти")]
    public GameObject deathEffect;

    [Header("Спрайти")]
    public Sprite normalSprite;
    public Sprite frozenSprite;

    private Image healthBarFill;
    private SpriteRenderer spriteRenderer;
    private PooledObject pooledObject;

    private int waypointIndex = 0;
    private int maxHealth;
    private int startHealth;

    private float currentSpeed;
    private float slowTimer = 0f;

    private bool isReturned = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        pooledObject = GetComponent<PooledObject>();

        startHealth = health;
        maxHealth = health;

        if (spriteRenderer != null && normalSprite == null)
        {
            normalSprite = spriteRenderer.sprite;
        }

        Transform fillTransform = transform.Find("Canvas/Fill");
        if (fillTransform != null)
        {
            healthBarFill = fillTransform.GetComponent<Image>();
        }
    }

    void OnEnable()
    {
        ResetEnemy();
    }

    public void ResetEnemy()
    {
        waypointIndex = 0;

        health = startHealth;
        maxHealth = startHealth;

        currentSpeed = speed;
        slowTimer = 0f;
        isReturned = false;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = 1f;
        }

        if (spriteRenderer != null && normalSprite != null)
        {
            spriteRenderer.sprite = normalSprite;
        }
    }

    void Update()
    {
        if (waypoints == null || waypoints.Length == 0)
            return;

        if (slowTimer > 0)
        {
            slowTimer -= Time.deltaTime;

            if (slowTimer <= 0)
            {
                currentSpeed = speed;

                if (spriteRenderer != null && normalSprite != null)
                {
                    spriteRenderer.sprite = normalSprite;
                }
            }
        }

        if (waypointIndex < waypoints.Length)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                waypoints[waypointIndex].position,
                currentSpeed * Time.deltaTime
            );

            if (Vector2.Distance(transform.position, waypoints[waypointIndex].position) < 0.1f)
            {
                waypointIndex++;
            }
        }
        else
        {
            GameManager.instance.TakeDamage(1);
            ReturnEnemy();
        }
    }

    public void Slow(float pct, float duration)
    {
        if (gameObject.name.Contains("Ghost")) return;

        currentSpeed = speed * pct;
        slowTimer = duration;

        if (spriteRenderer != null && frozenSprite != null)
        {
            spriteRenderer.sprite = frozenSprite;
        }
    }

    public float GetProgress()
    {
        if (waypoints == null || waypoints.Length == 0)
            return waypointIndex;

        if (waypointIndex >= waypoints.Length)
            return waypointIndex;

        return waypointIndex +
               (1f - Vector2.Distance(transform.position, waypoints[waypointIndex].position) / 10f);
    }

    public void TakeDamage(int damage)
    {
        health -= damage;

        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = (float)health / maxHealth;
        }

        if (health <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }

        GameManager.instance.AddGold(goldReward);
        ReturnEnemy();
    }

    void ReturnEnemy()
    {
        if (isReturned) return;

        isReturned = true;
        Spawner.enemiesAlive--;

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