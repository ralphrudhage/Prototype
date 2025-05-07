using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image hpFiller;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI apText;

    private void Start() {
        hpFiller.fillAmount = 1f;
    }

    public void BarColor(Color32 color)
    {
        hpFiller.color = color;
    }
    
    public void UpdateHp(float currentHp, float maxHp, int currentAP) {
        hpText.text = currentHp + " / " + maxHp;
        hpFiller.fillAmount = Mathf.Clamp(currentHp / maxHp, 0f, 1f);
        apText.text = "AP: " + currentAP;
    }
}
