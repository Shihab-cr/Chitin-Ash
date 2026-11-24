using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class PlayerStatSliderUIHandler : MonoBehaviour
{
    private Slider slider;
    private float sliderValue;
    [SerializeField] private float sliderUpgradeRatio = 1.1f;
    [SerializeField] private float maxSliderWidth = 650f;
    private RectTransform rectTransform;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        slider = GetComponent<Slider>();
        sliderValue = 1;
        
    }

    public void UpdateSliderValue(float value, float maxValue)
    {
        float percent = Mathf.Clamp01(value / maxValue);
        sliderValue = percent;
        slider.value = sliderValue;
    }
    public void UpgradeStatSliderLength()
    {
        rectTransform.pivot = new Vector2(0f, rectTransform.pivot.y);
        float currentSliderWidth = rectTransform.rect.width;
        float upgradwidth = Mathf.Min(currentSliderWidth * sliderUpgradeRatio, maxSliderWidth);
        rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, upgradwidth);
    }
}
