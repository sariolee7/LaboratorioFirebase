using UnityEngine;
using UnityEngine.InputSystem;

public class ClickHandler : MonoBehaviour
{
    public Camera mainCamera;
    public LayerMask enemyLayer;

    void Update()
    {
        if (!GameManager.Instance.IsRunning) return;
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, enemyLayer))
        {
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            if (enemy != null) enemy.ReceiveHit();
        }
    }
}