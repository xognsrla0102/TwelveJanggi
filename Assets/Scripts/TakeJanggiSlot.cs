using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TakeJanggiSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private bool isEnemyTakeJanggiSlot;

    [HideInInspector] public bool janggiExist;

    private Color normalFieldColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
    private Color darkNormalFieldColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

    private Image image;

    private void Start()
    {
        image = GetComponent<Image>();
        image.color = normalFieldColor;
    }

    // 색깔 진하게 변경
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isEnemyTakeJanggiSlot)
        {
            return;
        }

        image.color = darkNormalFieldColor;
    }

    // 원래 색으로 변경
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isEnemyTakeJanggiSlot)
        {
            return;
        }

        image.color = normalFieldColor;
    }
}
