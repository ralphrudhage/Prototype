using Managers;
using TMPro;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI energyText;
    
    private readonly Vector2Int startingGridPos = new(4, 2);
    public Vector2Int currentGridPos;
    
    private int currentEnergy;
    
    void Start()
    {
        energyText.text = "";
        currentGridPos = startingGridPos;
        transform.position = GridManager.Instance.GetWorldPosition(currentGridPos);
    }
    
    public void SetGridPosition(Vector2Int gridPos)
    {
        currentGridPos = gridPos;
        transform.position = GridManager.Instance.GetWorldPosition(gridPos);
    }

    public void UseEnergy(int energy)
    {
        currentEnergy -= energy;
        energyText.text = currentEnergy.ToString();
    }

    public int GetCurrentEnergy()
    {
        return currentEnergy;
    }

    public void ResetEnergy()
    {
        currentEnergy = 300;
        energyText.text = currentEnergy.ToString();
    }
}