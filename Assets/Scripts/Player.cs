using DefaultNamespace;
using Managers;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private GameObject hpPos;
    [SerializeField] public GameObject playerTarget;
    [SerializeField] private GameObject bloodPrefab;
    
    public Vector2Int currentGridPos;

    private const int maxHp = 100;
    private int currentHp;
    
    private const int maxActionPoints = 3;
    private int currentAP;
    
    private const int currentRange = 6;
    
    private TextSpawner textSpawner;
    private GameObject healthBar;
    
    public static Player Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    
    void Start()
    {
        currentHp = maxHp;
        currentAP = maxActionPoints;
        
        textSpawner = FindAnyObjectByType<TextSpawner>();
        healthBar = textSpawner.SpawnHealthBar();
        healthBar.GetComponent<HealthBar>().BarColor(GameUtils.lightBlue);
        
        energyText.text = "";
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        
        RefreshPlayerUI();
        
        Debug.Log($"Player set at {currentGridPos}");
    }

    private void RefreshPlayerUI()
    {
        healthBar.transform.position = Camera.main.WorldToScreenPoint(hpPos.transform.position);
        healthBar.GetComponent<HealthBar>().UpdateHp(currentHp, maxHp);
    }
    
    public void TakeDamage(int damageTaken)
    {
        Instantiate(bloodPrefab, playerTarget.transform.position, Quaternion.identity);
        textSpawner.SpawnFloatingText(damageTaken.ToString(), playerTarget.transform.position);
        currentHp -= damageTaken;
        RefreshPlayerUI();
        if (currentHp <= 0)
        {
            Debug.LogFormat("PLAYER DIED");
        }
    }
    
    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
        RefreshPlayerUI();
        
        Debug.Log($"Player set at {currentGridPos}");
    }

    public void UseAP(int actionPoint)
    {
        currentAP -= actionPoint;
        energyText.text = currentAP.ToString();
    }

    public int GetCurrentAP()
    {
        return currentAP;
    }

    public int GetCurrentRange()
    {
        return currentRange;
    }

    public void ResetAP()
    {
        currentAP = 3;
        energyText.text = currentAP.ToString();
    }

    public void AttackEnemy(Vector2 enemyPosition, int damage)
    {
        var bullet = Instantiate(projectilePrefab, playerTarget.transform.position, Quaternion.identity);
        bullet.GetComponent<Projectile>().Initialize(enemyPosition, damage, true);
    }
}