using Managers;
using Model;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedCard : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI energyText;
    [SerializeField] private TextMeshProUGUI infoText;
    [SerializeField] private TextMeshProUGUI infoValue;
    public Card Card;
    private CursorManager cursorManager;
    private Enemy enemy;
    private bool isSelected;

    private Vector3 originalPosition;
    private bool isDragging;
    private Image cardImage;

    private void Start()
    {
        cardImage = GetComponent<Image>();
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

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PartyManager.Instance.currentPlayer.GetCurrentAP() < Card.cost) return;

        originalPosition = transform.position;
        isDragging = true;
        isSelected = true;
        CardManager.Instance.SetCurrentAction(this);
        
        cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 0.5f);

        switch (Card)
        {
            case MoveCard move:
                GridManager.Instance.ShowMoveCircles(PartyManager.Instance.currentPlayer.currentGridPos, move.range);
                break;

            case AttackCard:
                cursorManager.SetCrossHair();
                break;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        Vector3 newPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        newPosition.z = 0f; // Assuming 2D
        transform.position = newPosition;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!isDragging) return;
        isDragging = false;

        if (CanConsume())
        {
            ConsumeCard();
        }
        else
        {
            cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 1f);
            GridManager.Instance.ClearCircles();
            transform.position = originalPosition;
        }
    }

    private bool CanConsume()
    {
        // TODO: Replace this with proper validation:
        // E.g., is over a valid target (enemy, tile, etc.)
        return false;
    }
}