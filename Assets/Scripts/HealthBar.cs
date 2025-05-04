using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image hpFiller;
    [SerializeField] private TextMeshProUGUI hpText;

    private void Start() {
        hpFiller.fillAmount = 1f;
    }
    
    public void UpdateHp(float currentHp, float maxHp) {
        hpText.text = currentHp + " / " + maxHp;
        hpFiller.fillAmount = Mathf.Clamp(currentHp / maxHp, 0f, 1f); 
    }
}
