using Managers;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject bloodParent;
    [SerializeField] GameObject bloodPrefab;
    [SerializeField] TextMeshProUGUI healthText;
    
    private int health = 1000;
    private readonly Vector2Int startingGridPos = new(6, 4);
    public Vector2Int currentGridPos;
    private SpawnText spawnText;
    
    void Start()
    {
        spawnText = FindAnyObjectByType<SpawnText>();
        SetGridPosition(startingGridPos);
        UpdateHeathText();
    }
    
    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
    }

    public void TakeDamage(int damage)
    {
        Instantiate(bloodPrefab, bloodParent.transform.position, Quaternion.identity);
        spawnText.SpawnFloatingText(damage.ToString(), transform.position);
        health -= damage;
        UpdateHeathText();
        if (health <= 0)
        {
            Debug.LogFormat("ENEMY DIED");
        }
    }

    private void UpdateHeathText()
    {
        healthText.text = health + " / 1000";
    }

    private void OnMouseDown()
    {
        ActionManager.Instance.PerformAction();
    }
}
