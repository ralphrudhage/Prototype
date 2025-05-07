using Managers;
using Model;
using TMPro;
using UnityEngine;

public class SelectedCard : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI infoValue;
    public Card Card;
    private CursorManager cursorManager;
    private Enemy enemy;
    private bool isSelected;

    private void Start()
    {
        cursorManager = FindAnyObjectByType<CursorManager>();
    }

    public void SetUpCard(Card selectedAction)
    {
        Card = selectedAction;
        
        actionText.text = Card.type == CardType.MOVE ? "Move" : "Attack";
        energyText.text = Card.cost.ToString();

        infoText.text = Card.type == CardType.MOVE ? "Range" : "Damage";

        infoValue.text = Card switch
        {
            MoveCard action => action.range.ToString(),
            AttackCard action => action.damage.ToString(),
            _ => infoValue.text
        };
    }

    public void InitCard()
    {
        if (PartyManager.Instance.currentPlayer.GetCurrentAP() >= Card.cost)
        {
            CardManager.Instance.SetCurrentAction(this);
            isSelected = true;

            switch (Card)
            {
                case MoveCard action:
                    GridManager.Instance.ShowMoveCircles(PartyManager.Instance.currentPlayer.currentGridPos, action.range);
                    break;

                case AttackCard:
                    cursorManager.SetCrossHair();
                    break;
            }
        }
        else
        {
            Debug.Log("Not enough energy");
        }
    }
    
    public void ConsumeCard()
    {
        if (!isSelected) return;

        switch (Card)
        {
            case MoveCard:
                // Movement logic goes here
                break;

            case AttackCard action:
                PartyManager.Instance.currentPlayer.AttackEnemy(
                    CardManager.Instance.GetCurrentEnemy().targetPos,
                    action.damage
                );
                cursorManager.SetCrossHair();
                break;
        }

        PartyManager.Instance.currentPlayer.UseAP(Card.cost);
        CardManager.Instance.DiscardSelectedCard(Card);
    }
}