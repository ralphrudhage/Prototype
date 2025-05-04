using System.Collections;
using Managers;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private GameObject hpPos;
    [SerializeField] private GameObject bloodParent;
    [SerializeField] private GameObject bloodPrefab;

    private int currentHp;
    private const int maxHp = 100;

    public Vector2Int currentGridPos;
    private TextSpawner textSpawner;
    private GameObject healthBar;
    private PlayerController player;

    void Start()
    {
        EnemyManager.Instance.RegisterEnemy(this);

        player = FindAnyObjectByType<PlayerController>();
        textSpawner = FindAnyObjectByType<TextSpawner>();
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        healthBar = textSpawner.SpawnHealthBar();

        currentHp = maxHp;

        UpdateHeathBar();
    }

    public void TakeDamage(int damage)
    {
        Instantiate(bloodPrefab, bloodParent.transform.position, Quaternion.identity);
        textSpawner.SpawnFloatingText(damage.ToString(), bloodParent.transform.position);
        currentHp -= damage;
        UpdateHeathBar();
        if (currentHp <= 0)
        {
            Debug.LogFormat("ENEMY DIED");
        }
    }

    private void UpdateHeathBar()
    {
        healthBar.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);
        healthBar.GetComponent<HealthBar>().UpdateHp(currentHp, maxHp);
    }

    private void OnMouseDown()
    {
        ActionManager.Instance.PerformAction();
    }

    private void OnDestroy()
    {
        if (EnemyManager.Instance != null)
            EnemyManager.Instance.UnregisterEnemy(this);
    }

    public IEnumerator TakeTurn()
    {
        Debug.Log($"Enemy at {currentGridPos} takes turn...");

        // Example: just wait for 0.5 seconds (simulate thinking or animation)
        yield return new WaitForSeconds(0.5f);

        Vector2Int playerPos = player.currentGridPos;
        Vector2Int direction = (playerPos - currentGridPos);

        // Normalize to -1/0/1 (basic Manhattan direction)
        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        // Try preferred direction first
        Vector2Int targetPos = currentGridPos + direction;

        if (GridManager.Instance.IsWithinBounds(targetPos))
        {
            yield return MoveTo(targetPos);
        }
        else
        {
            Debug.Log($"Enemy at {currentGridPos} can't move toward player");
            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator MoveTo(Vector2Int targetPos)
    {
        currentGridPos = targetPos;
        Vector3 worldTarget = GridManager.Instance.GetWorldPosition(targetPos);

        // Simple snap movement (replace with animation if you want)
        transform.position = worldTarget;
        UpdateHeathBar();
        yield return new WaitForSeconds(0.2f);
    }
}