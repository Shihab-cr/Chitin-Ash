using TMPro;
using UnityEngine;

public class HandleCurrencyUI : MonoBehaviour
{
    private TextMeshProUGUI currText;
    private void Start()
    {
        currText = GetComponent<TextMeshProUGUI>();
    }
    public void UpdateUI(int value)
    {
        if(currText!=null)
        currText.text = $"{value}";
    }
}
