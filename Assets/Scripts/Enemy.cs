using Managers;
using TMPro;
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
    
    void Start()
    {
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
        healthBar.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);;
        healthBar.GetComponent<HealthBar>().UpdateHp(currentHp, maxHp);
    }

    private void OnMouseDown()
    {
        ActionManager.Instance.PerformAction();
    }
}
