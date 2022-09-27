using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

public class JanggiSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    [SerializeField] private int heightNum;
    [SerializeField] private int widthNum;
    [SerializeField] private GameObject shadowJanggi;

    private Color myFieldColor = new Color(140f / 255f, 255f / 255f, 140f / 255f);
    private Color darkMyFieldColor = new Color(100f / 255f, 200f / 255f, 100f / 255f);
    private Color enemyFieldColor = new Color(255f / 255f, 130f / 255f, 130f / 255f);
    private Color darkEnemyFieldColor = new Color(230f / 255f, 90f / 255f, 90f / 255f);
    private Color normalFieldColor = new Color(255f / 255f, 255f / 255f, 255f / 255f);
    private Color darkNormalFieldColor = new Color(200f / 255f, 200f / 255f, 200f / 255f);

    private Image image;
    private int myFieldHeightNum;
    private int enemyFieldHeightNum;

    private void Start()
    {
        image = GetComponent<Image>();

        myFieldHeightNum = PhotonNetwork.IsMasterClient ? 3 : 0;
        enemyFieldHeightNum = PhotonNetwork.IsMasterClient ? 0 : 3;

        if (heightNum == myFieldHeightNum)
        {
            image.color = myFieldColor;
        }
        else if (heightNum == enemyFieldHeightNum)
        {
            image.color = enemyFieldColor;
        }
        else
        {
            image.color = normalFieldColor;
        }
    }

    // 색깔 진하게 변경
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (heightNum == myFieldHeightNum)
        {
            image.color = darkMyFieldColor;
        }
        else if (heightNum == enemyFieldHeightNum)
        {
            image.color = darkEnemyFieldColor;
        }
        else
        {
            image.color = darkNormalFieldColor;
        }
    }

    // 원래 색으로 변경
    public void OnPointerExit(PointerEventData eventData)
    {
        if (heightNum == myFieldHeightNum)
        {
            image.color = myFieldColor;
        }
        else if (heightNum == enemyFieldHeightNum)
        {
            image.color = enemyFieldColor;
        }
        else
        {
            image.color = normalFieldColor;
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

            Transform originParent = eventData.pointerDrag.GetComponent<Janggi>().originParent;

            // 원래 슬롯으로 제자리 둘 경우 무시
            if (transform == originParent)
            {
                return;
            }

            // 현재 옮길 장기의 시작 슬롯과, 목표 슬롯 위치를 전달한다.
            JanggiSlot srcJanggiSlot = originParent.GetComponent<JanggiSlot>();
            NetworkManager.Instance.DropJanggi(srcJanggiSlot.heightNum, srcJanggiSlot.widthNum, heightNum, widthNum);

            // 턴 종료 처리
            int nextUserIdx = ((FindObjectOfType<IngameScene>().isMasterTurn ? 1 : 0) + 1) % 2;
            NetworkManager.Instance.SelectNextTurnUser(nextUserIdx == 1);
        }
    }

    public void SetActiveShadowJanggi(bool isActive)
    {
        shadowJanggi.SetActive(isActive);
    }
}
