using UnityEngine;
using UnityEngine.InputSystem;

public class ClickHandler : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask enemyLayer;
    public GameObject bulletPrefab;
    public Transform firePoint;

    void Update()
    {
        if (!GameManager.Instance.IsRunning) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
        {
            if (hit.collider.GetComponent<Enemy>() == null) return;

            // Solo dispara la bala, ella aplica el daño al llegar
            Vector3 direction = (hit.point - firePoint.position).normalized;
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
            Bullet bulletScript = bullet.GetComponent<Bullet>();
            if (bulletScript != null)
                bulletScript.SetDirection(direction);
        }
    }
}