using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private RectTransform fillRect;
    [SerializeField] private TMP_Text labelText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private string label = "PLAYER";

    public void Configure(Image fill, RectTransform fillTransform, TMP_Text labelTarget, TMP_Text valueTarget, string displayLabel)
    {
        fillImage = fill;
        fillRect = fillTransform;
        labelText = labelTarget;
        valueText = valueTarget;
        label = displayLabel;

        if (labelText != null)
        {
            labelText.text = label;
        }
    }

    public void SetValue(int current, int max)
    {
        int safeMax = Mathf.Max(1, max);
        int safeCurrent = Mathf.Clamp(current, 0, safeMax);
        float normalized = (float)safeCurrent / safeMax;

        if (fillImage != null)
        {
            fillImage.fillAmount = normalized;
        }

        if (fillRect != null)
        {
            Vector2 anchorMax = fillRect.anchorMax;
            anchorMax.x = normalized;
            fillRect.anchorMax = anchorMax;
        }

        if (labelText != null)
        {
            labelText.text = label;
        }

        if (valueText != null)
        {
            valueText.text = $"{safeCurrent}/{safeMax}";
        }
    }
}
