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
    public Card card;
    private CursorManager cursorManager;

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
        card = selectedAction;

        actionText.text = card.type == CardType.MOVE ? "Move" : "Attack";
        energyText.text = card.cost.ToString();

        infoText.text = card.type == CardType.MOVE ? "Range" : "Damage";

        infoValue.text = card switch
        {
            MoveCard action => action.range.ToString(),
            AttackCard action => action.damage.ToString(),
            _ => infoValue.text
        };
    }

    public void ConsumeCard()
    {
        switch (card)
        {
            case MoveCard:
                GridManager.Instance.ClearHighlights();
                break;

            case AttackCard action:
                PartyManager.Instance.currentPlayer.AttackEnemy(
                    CardManager.Instance.GetCurrentEnemy().targetPos,
                    action.damage
                );
                // cursorManager.SetCrossHair();
                break;
        }

        PartyManager.Instance.currentPlayer.UseAP(card.cost);
        CardManager.Instance.DiscardSelectedCard(card);
        Destroy(gameObject);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (PartyManager.Instance.currentPlayer.GetCurrentAP() < card.cost) return;

        originalPosition = transform.position;
        isDragging = true;
        
        CardManager.Instance.SetCurrentAction(this);
        
        cardImage.color = new Color(cardImage.color.r, cardImage.color.g, cardImage.color.b, 0.5f);

        switch (card)
        {
            case MoveCard move:
                GridManager.Instance.ShowMoveCircles(PartyManager.Instance.currentPlayer.currentGridPos, move.range);
                break;

            case AttackCard:
                // cursorManager.SetCrossHair();
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
            GridManager.Instance.ClearHighlights();
            transform.position = originalPosition;
        }
    }

    private bool CanConsume()
    {
        Vector3 droppedWorldPos = transform.position;
        
        if (card is MoveCard)
        {
            Vector2Int droppedGridPos = GridManager.Instance.GetGridPositionFromWorld(droppedWorldPos);
        
            bool canMove = GridManager.Instance.IsValidMoveTile(droppedGridPos);
            if (canMove)
            {
                PartyManager.Instance.currentPlayer.SetGridPosition(droppedGridPos);
                return true;
            }

            return false;
        }
        
        if (card is AttackCard)
        {
            Collider2D hit = Physics2D.OverlapPoint(droppedWorldPos);
            if (hit != null && hit.TryGetComponent<Enemy>(out var enemy))
            {
                Vector2Int enemyPos = enemy.currentGridPos;
                Vector2Int playerPos = PartyManager.Instance.currentPlayer.currentGridPos;
                int range = PartyManager.Instance.currentPlayer.GetCurrentRange();

                int dx = Mathf.Abs(enemyPos.x - playerPos.x);
                int dy = Mathf.Abs(enemyPos.y - playerPos.y);
                int manhattanDistance = dx + dy;

                if (manhattanDistance <= range)
                {
                    CardManager.Instance.SetCurrentEnemy(enemy); // Set this so ConsumeCard can use it
                    return true;
                }
            }
        }

        // Add future checks for AttackCard, etc.
        return false;
    }

}