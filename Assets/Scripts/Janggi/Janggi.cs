using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Janggi : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    protected enum Janggi_Type
    {
        JA,
        HOO,
        JANG,
        SANG,
        WANG,
        JANGGI_TYPE_CNT
    }
    protected enum Team_Type
    {
        RED,
        BLUE,
        TEAM_TYPE_CNT
    }

    protected enum Move_Dir
    {
        LEFT_FRONT,
        FRONT,
        RIGHT_FRONT,

        LEFT,
        RIGHT,

        LEFT_BACK,
        BACK,
        RIGHT_BACK,

        MOVE_DIR_CNT
    }

    [SerializeField] protected Janggi_Type janggiType;
    [SerializeField] protected Team_Type teamType;
    [SerializeField] protected Text janggiName;

    [SerializeField] protected GameObject[] dirs;

    #region 드래그 & 드롭 속성
    private Transform gamePanel;
    private Transform originParent;
    private RectTransform rectTransform;
    // 장기 하위 오브젝트들도 변화해야하므로 캔버스 그룹으로 처리
    private CanvasGroup canvasGroup;
    #endregion

    private void Awake()
    {
        gamePanel = GameObject.Find("GamePanel").GetComponent<Transform>();
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        foreach (var dir in dirs)
        {
            dir.SetActive(false);
        }
    }

    protected virtual void Start()
    {
        #region 이름 초기화
        switch (janggiType)
        {
            case Janggi_Type.JA: janggiName.text = "子"; break;
            case Janggi_Type.HOO: janggiName.text = "侯"; break;
            case Janggi_Type.JANG: janggiName.text = "將"; break;
            case Janggi_Type.SANG: janggiName.text = "相"; break;
            case Janggi_Type.WANG: janggiName.text = "王"; break;
            default: Debug.Assert(false); break;
        }
        #endregion

        #region 방향 초기화
        switch (janggiType)
        {
            // 자는 위로만 이동 가능
            case Janggi_Type.JA: dirs[(int)Move_Dir.FRONT].SetActive(true); break;
            // 후는 뒤쪽 대각선 제외 모든 방향 이동 가능
            case Janggi_Type.HOO:
                dirs[(int)Move_Dir.LEFT_FRONT].SetActive(true);
                dirs[(int)Move_Dir.FRONT].SetActive(true);
                dirs[(int)Move_Dir.RIGHT_FRONT].SetActive(true);

                dirs[(int)Move_Dir.LEFT].SetActive(true);
                dirs[(int)Move_Dir.RIGHT].SetActive(true);

                dirs[(int)Move_Dir.BACK].SetActive(true);
                break;
            // 장은 전,후,좌,우 이동 가능
            case Janggi_Type.JANG:
                dirs[(int)Move_Dir.FRONT].SetActive(true);
                dirs[(int)Move_Dir.BACK].SetActive(true);
                dirs[(int)Move_Dir.LEFT].SetActive(true);
                dirs[(int)Move_Dir.RIGHT].SetActive(true);
                break;
            // 상은 대각선 4방향 이동 가능
            case Janggi_Type.SANG:
                dirs[(int)Move_Dir.LEFT_FRONT].SetActive(true);
                dirs[(int)Move_Dir.RIGHT_FRONT].SetActive(true);
                dirs[(int)Move_Dir.LEFT_BACK].SetActive(true);
                dirs[(int)Move_Dir.RIGHT_BACK].SetActive(true);
                break;
            // 왕은 모든 방향 이동 가능
            case Janggi_Type.WANG:
                for (int moveDir = 0; moveDir < (int)Move_Dir.MOVE_DIR_CNT; moveDir++)
                    dirs[moveDir].SetActive(true);
                break;
            default: Debug.Assert(false); break;
        }
        #endregion

        #region 색상 초기화
        var outLine = GetComponent<Outline>();
        switch (teamType)
        {
            case Team_Type.RED: outLine.effectColor = Color.red; break;
            case Team_Type.BLUE: outLine.effectColor = new Color(0, 140 / 255f, 1); break;
            default: Debug.Assert(false); break;
        }
        #endregion
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        // 이상한 곳 드롭 시 원래 위치 되돌아갈 위치 저장
        originParent = transform.parent;

        // 부모 변경 및 최상단 이동(UI 가림 처리)
        transform.SetParent(gamePanel);
        transform.SetAsLastSibling();

        // 투명 처리 및 레이캐스트 무시
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        // 장기 위치를 마우스 위치로 이동
        rectTransform.position = eventData.position;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        // 드래그 종료 시에도 부모가 gamePanel 그대로라면
        // 드롭을 다른 곳에 제대로 안해서 부모 설정이 안됬다는 뜻
        if (transform.parent == gamePanel)
        {
            // 기존 부모 설정 및 위치 되돌림
            transform.SetParent(originParent);
            rectTransform.position = originParent.GetComponent<RectTransform>().position;
        }

        // 캔버스 그룹 설정 되돌림
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;
    }
}
