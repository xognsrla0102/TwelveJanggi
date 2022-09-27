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

public enum EDirType
{
    LEFT_TOP,
    CENTER_TOP,
    RIGHT_TOP,
    LEFT_MID,
    RIGHT_MID,
    LEFT_BOTTOM,
    CENTER_BOTTOM,
    RIGHT_BOTTOM,
    NUMS
}

public class Janggi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public bool isMyJanggi;

    [SerializeField] private EJanggiType janggiType;
    [SerializeField] private Text janggiText;

    [HideInInspector] public CanvasGroup canvasGroup;
    [HideInInspector] public Transform originParent;


    private bool isShadowJanggi;

    private GameObject[] dirs = new GameObject[(int)EDirType.NUMS];
    private Transform gameUiCanvas;
    private RectTransform rectTransform;
    private Outline outLine;

    private void Start()
    {
        Transform dir = transform.Find("Dir");
        for (int i = 0; i < 8; i++)
        {
            dirs[i] = dir.GetChild(i).gameObject;
        }

        canvasGroup = GetComponent<CanvasGroup>();
        gameUiCanvas = GameObject.Find("GameUI").transform;
        rectTransform = GetComponent<RectTransform>();
        outLine = GetComponent<Outline>();

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

        SetJanggi();
    }

    public void SetJanggi()
    {
        foreach (var dir in dirs)
        {
            dir.SetActive(false);
        }

        switch (janggiType)
        {
            // 전,후,좌,우
            case EJanggiType.JANG:
                janggiText.text = "將";
                dirs[(int)EDirType.CENTER_TOP].SetActive(true);
                dirs[(int)EDirType.CENTER_BOTTOM].SetActive(true);
                dirs[(int)EDirType.LEFT_MID].SetActive(true);
                dirs[(int)EDirType.RIGHT_MID].SetActive(true);
                break;
            // 대각선 4방향
            case EJanggiType.SANG:
                janggiText.text = "相";
                dirs[(int)EDirType.LEFT_TOP].SetActive(true);
                dirs[(int)EDirType.RIGHT_TOP].SetActive(true);
                dirs[(int)EDirType.LEFT_BOTTOM].SetActive(true);
                dirs[(int)EDirType.RIGHT_BOTTOM].SetActive(true);
                break;
            // 모든 방향
            case EJanggiType.WANG:
                janggiText.text = "王";
                foreach (var dir in dirs)
                {
                    dir.SetActive(true);
                }
                break;
            // 전
            case EJanggiType.JA:
                janggiText.text = "子";
                dirs[(int)EDirType.CENTER_TOP].SetActive(true);
                break;
            // 뒤쪽 대각선 제외 모든 방향
            case EJanggiType.HU:
                janggiText.text = "候";
                foreach (var dir in dirs)
                {
                    dir.SetActive(true);
                }
                dirs[(int)EDirType.LEFT_BOTTOM].SetActive(false);
                dirs[(int)EDirType.RIGHT_BOTTOM].SetActive(false);
                break;
            default:
                Debug.Assert(false);
                break;
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
