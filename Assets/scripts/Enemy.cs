using UnityEngine;

public class Enemy : MonoBehaviour
{
    public int maxHealth = 1;
    public int scoreValue = 10;
    public float speed = 2f;

    private int currentHealth;
    private bool isDead;

    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    void Update()
    {
        if (!GameManager.Instance.IsRunning) return;
        transform.Translate(Vector3.left * speed * Time.deltaTime, Space.World);
    }

    public void ReceiveHit()
    {
        if (isDead) return;
        currentHealth--;
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        GameManager.Instance.AddScore(scoreValue);
        GameManager.Instance.RegisterKill();
        EnemySpawner.Instance.OnEnemyKilled();
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Base") || isDead) return;
        isDead = true;
        GameManager.Instance.EnemyReachedBase();
        EnemySpawner.Instance.OnEnemyKilled();
        Destroy(gameObject);
    }
}