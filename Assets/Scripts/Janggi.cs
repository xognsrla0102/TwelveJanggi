using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum EJanggiType
{
    JANG,
    SANG,
    WANG,
    JA,
    HU,
    NUMS
}

public class Janggi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isMyJanggi;

    [SerializeField] private EJanggiType janggiType;
    [SerializeField] private Text janggiText;

    private bool isShadowJanggi;

    [HideInInspector] public CanvasGroup canvasGroup;
    private Transform gameUiCanvas;
    private RectTransform rectTransform;
    private Outline outLine;

    private Transform originParent;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        gameUiCanvas = GameObject.Find("GameUI").transform;
        rectTransform = GetComponent<RectTransform>();
        outLine = GetComponent<Outline>();

        // 내 장기일 경우만 옮기게 함
        canvasGroup.blocksRaycasts = isMyJanggi;
    }

    private void Start()
    {
        if (isMyJanggi)
        {
            outLine.effectColor = new Color(0f, 140f / 255f, 0f);
            transform.rotation = Quaternion.identity;            
        }
        else
        {
            outLine.effectColor = new Color(1f, 0f, 0f);
            transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
        }

        SetText();
    }

    public void SetText()
    {
        switch (janggiType)
        {
            case EJanggiType.JANG: janggiText.text = "將"; break;
            case EJanggiType.SANG: janggiText.text = "相"; break;
            case EJanggiType.WANG: janggiText.text = "王"; break;
            case EJanggiType.JA: janggiText.text = "子"; break;
            case EJanggiType.HU: janggiText.text = "候"; break;
            default: Debug.Assert(false); break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 드랍 실패 시 되돌아갈 부모 저장
        originParent = transform.parent;

        // 드래그 중인 오브젝트를 하이레키 맨 아래로 옮김
        transform.SetParent(gameUiCanvas);
        transform.SetAsLastSibling();

        // 드랍 시 슬롯에 레이캐스트 충돌 되는 것 방지
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 드랍했는데 여전히 오브젝트 부모가 맨 아래에 있으면
        // 드랍 실패했다는 의미
        if (transform.parent == gameUiCanvas)
        {
            transform.SetParent(originParent);
            rectTransform.anchoredPosition = Vector3.zero;
        }

        // 장기를 잡을 수 있어야 하므로 다시 레이캐스트 충돌 처리
        canvasGroup.blocksRaycasts = true;
    }
}
