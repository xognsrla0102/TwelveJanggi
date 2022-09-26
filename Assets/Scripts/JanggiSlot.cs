using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class JanggiSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField] private int heightNum;
    [SerializeField] private int widthNum;
    [SerializeField] private GameObject shadowJanggi;

    private Image image;
    private RectTransform rectTransform;

    private void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
    }

    // 색깔 진하게 변경
    public void OnPointerEnter(PointerEventData eventData)
    {
        switch (heightNum)
        {
            // 0번째는 적 진영
            case 0:
                image.color = new Color(230f / 255f, 90f / 255f, 90f / 255f);
                break;
            // 3번째는 내 진영
            case 3:
                image.color = new Color(100f / 255f, 200f / 255f, 100f / 255f);
                break;
            default:
                image.color = new Color(200f / 255f, 200f / 255f, 200f / 255f);
                break;
        }
    }

    // 원래 색으로 변경
    public void OnPointerExit(PointerEventData eventData)
    {
        switch (heightNum)
        {
            // 0번째는 적 진영
            case 0:
                image.color = new Color(255f / 255f, 130f / 255f, 130f / 255f);
                break;
            // 3번째는 내 진영
            case 3:
                image.color = new Color(140f / 255f, 255f / 255f, 140f / 255f);
                break;
            default:
                image.color = new Color(255f / 255f, 255f / 255f, 255f / 255f);
                break;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            eventData.pointerDrag.transform.SetParent(transform);
            eventData.pointerDrag.transform.SetAsLastSibling();
            eventData.pointerDrag.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;

            SoundManager.Instance.PlaySND(SSfxName.JANGGI_DROP_SFX);
        }
    }

    public void SetActiveShadowJanggi(bool isActive)
    {
        shadowJanggi.SetActive(isActive);
    }
}
