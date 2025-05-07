using Managers;
using Model;
using TMPro;
using UnityEngine;

public class SelectedAction : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI infoValue;
    public GameAction gameAction;
    private CursorManager cursorManager;
    private Enemy enemy;
    private bool isSelected;

    private void Start()
    {
        cursorManager = FindAnyObjectByType<CursorManager>();
    }

    public void SetUpAction(GameAction selectedAction)
    {
        gameAction = selectedAction;
        
        actionText.text = gameAction.type == ActionType.MOVE ? "Move" : "Attack";
        energyText.text = gameAction.cost.ToString();

        infoText.text = gameAction.type == ActionType.MOVE ? "Range" : "Damage";

        infoValue.text = gameAction switch
        {
            MoveAction action => action.range.ToString(),
            AttackAction action => action.damage.ToString(),
            _ => infoValue.text
        };
    }

    public void InitAction()
    {
        if (PartyManager.Instance.currentPlayer.GetCurrentAP() >= gameAction.cost)
        {
            ActionManager.Instance.SetCurrentAction(this);
            isSelected = true;

            switch (gameAction)
            {
                case MoveAction action:
                    GridManager.Instance.ShowMoveCircles(PartyManager.Instance.currentPlayer.currentGridPos, action.range);
                    break;

                case AttackAction:
                    cursorManager.SetCrossHair();
                    break;
            }
        }
        else
        {
            Debug.Log("Not enough energy");
        }
    }
    
    public void ConsumeAction()
    {
        if (!isSelected) return;

        switch (gameAction)
        {
            case MoveAction:
                // Movement logic goes here
                break;

            case AttackAction action:
                PartyManager.Instance.currentPlayer.AttackEnemy(
                    ActionManager.Instance.GetCurrentEnemy().targetPos,
                    action.damage
                );
                cursorManager.SetCrossHair();
                break;
        }

        PartyManager.Instance.currentPlayer.UseAP(gameAction.cost);
        ActionManager.Instance.DiscardSelectedAction(gameAction);
    }
}