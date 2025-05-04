using Managers;
using TMPro;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] GameObject bloodParent;
    [SerializeField] GameObject bloodPrefab;
    [SerializeField] TextMeshProUGUI healthText;
    
    private int health = 1000;
    public Vector2Int currentGridPos;
    private SpawnText spawnText;
    
    void Start()
    {
        spawnText = FindAnyObjectByType<SpawnText>();
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        
        UpdateHeathText();
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
