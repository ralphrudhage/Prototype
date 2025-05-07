using System.Collections.Generic;
using DefaultNamespace;
using Managers;
using Model;
using TMPro;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject playerCircle;
    [SerializeField] private GameObject projectilePrefab;
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
    public PlayerClass playerClass;
    public Queue<Card> drawPile = new();
    public List<Card> hand = new();
    public List<Card> discarded = new();

    private void OnEnable()
    {
        textSpawner = FindAnyObjectByType<TextSpawner>();
        healthBar = textSpawner.SpawnHealthBar();
        healthBar.GetComponent<HealthBar>().BarColor(GameUtils.lightBlue);
        
        InitializeDeck();
    }

    void Start()
    {
        currentHp = maxHp;
        currentAP = maxActionPoints;
        
        currentGridPos = GridManager.Instance.GetGridPositionFromWorld(transform.position);
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
        
        RefreshPlayerUI();
        
        Debug.Log($"{playerClass} set at {currentGridPos}");
    }
    
    private void InitializeDeck()
    {
        List<Card> actions = new();

        switch (playerClass)
        {
            case PlayerClass.WARRIOR:
                actions.AddRange(new Card[]
                {
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20)
                });
                break;

            case PlayerClass.MAGE:
                actions.AddRange(new Card[]
                {
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20)
                });
                break;

            case PlayerClass.PRIEST:
                actions.AddRange(new Card[]
                {
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new MoveCard(1, 1),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20),
                    new AttackCard(1, 20)
                });
                break;
        }

        actions.Shuffle();
        drawPile = new Queue<Card>(actions);
    }
    
    private void RefreshPlayerUI()
    {
        PlayerUI.Instance.UpdatePlayerUI(this);
        
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
            Debug.Log($"{playerClass} DIED");
        }
    }
    
    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
        RefreshPlayerUI();
        
        Debug.Log($"{playerClass} set at {currentGridPos}");
    }

    public void UseAP(int actionPoint)
    {
        currentAP -= actionPoint;
        RefreshPlayerUI();
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
        RefreshPlayerUI();
    }

    public void AttackEnemy(Vector2 enemyPosition, int damage)
    {
        var bullet = Instantiate(projectilePrefab, playerTarget.transform.position, Quaternion.identity);
        bullet.GetComponent<Projectile>().Initialize(enemyPosition, damage, true);
    }
    
    private void OnMouseDown()
    {
        PartyManager.Instance.SetCurrentPlayer(this);
        Debug.Log($"{playerClass} selected");
    }

    public void SelectedCircle()
    {
        var circles = GameObject.FindGameObjectsWithTag("circle");
        foreach (var circle in circles) circle.SetActive(false);
        playerCircle.SetActive(true);
    }

    public void Deselect()
    {
        playerCircle.SetActive(false);
    }
}