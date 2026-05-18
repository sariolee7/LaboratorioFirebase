using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float maxLifetime = 0.8f;

    private Vector3 direction;

    void Start()
    {
        Destroy(gameObject, maxLifetime);
    }

    public void SetDirection(Vector3 dir)
    {
        direction = dir;
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    // Ahora la bala es quien aplica el daño al llegar
    void OnTriggerEnter(Collider other)
    {
        Enemy enemy = other.GetComponent<Enemy>();
        if (enemy == null) return;

        enemy.ReceiveHit();
        Destroy(gameObject);
    }
}