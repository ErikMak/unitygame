using TMPro;
using UnityEngine;
using UnityEngine.UI;

public sealed class DraggedItemUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityText;

    private RectTransform rectTransform;

    private void Awake()
    {
        rectTransform = (RectTransform)transform;
        Hide();
    }

    public void Show(GoodItem item, int quantity, Vector2 screenPosition)
    {
        gameObject.SetActive(true);
        if (icon != null)
        {
            icon.sprite = item != null ? item.Icon : null;
        }

        if (quantityText != null)
        {
            quantityText.text = $"x{quantity}";
        }

        Move(screenPosition);
    }

    public void Move(Vector2 screenPosition)
    {
        if (rectTransform != null)
        {
            rectTransform.position = screenPosition;
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
