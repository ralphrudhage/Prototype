using DefaultNamespace;
using Managers;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] public GameObject playerTarget;
    
    public Vector2Int currentGridPos;
    
    private int currentAP;
    
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
        energyText.text = "";
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
    }
    
    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
    }

    public void UseAP(int actionPoint)
    {
        currentAP -= actionPoint;
        energyText.text = currentAP.ToString();
    }

    public int GetCurrentEnergy()
    {
        return currentAP;
    }

    public void ResetAP()
    {
        currentAP = 3;
        energyText.text = currentAP.ToString();
    }

    public void AttackEnemy(Vector2 enemyPosition, int damage)
    {
        var bullet = Instantiate(projectilePrefab, playerTarget.transform.position, Quaternion.identity);
        bullet.GetComponent<Projectile>().Initialize(enemyPosition, damage);
    }
}