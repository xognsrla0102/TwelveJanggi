using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;
using ExitGames.Client.Photon;

public class JanggiSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IDropHandler
{
    public Janggi shadowJanggi;

    [HideInInspector] public int heightNum;
    [HideInInspector] public int widthNum;

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
            Transform originParent = eventData.pointerDrag.GetComponent<Janggi>().originParent;

            // 원래 슬롯으로 제자리 두는 경우는 위치만 롤백 처리
            if (transform == originParent)
            {
                DropJanggi(eventData.pointerDrag, transform);
                return;
            }

            // 현재 슬롯에 쉐도우 장기가 활성화 안되어있으면 드랍 못함
            if (shadowJanggi.gameObject.activeInHierarchy == false)
            {
                DropJanggi(eventData.pointerDrag, originParent);
                return;
            }

            bool isKill = false;
            bool isGameOver = false;

            // 이미 현재 슬롯에 장기가 있을 경우
            Transform mySlotJanggiTransform = transform.Find("Janggi");
            if (mySlotJanggiTransform != null)
            {
                Janggi janggi = mySlotJanggiTransform.GetComponent<Janggi>();

                // 내 장기이면 드랍 못 함
                if (janggi.isMyJanggi)
                {
                    DropJanggi(eventData.pointerDrag, originParent);
                    return;
                }
                // 상대방 장기일 경우 먹기 처리
                else
                {
                    // 임시로 해당 장기 오브젝트 삭제
                    Destroy(mySlotJanggiTransform.gameObject);
                    isKill = true;

                    // 왕을 죽일 경우 게임 오버 처리
                    if (janggi.janggiType == EJanggiType.WANG)
                    {
                        isGameOver = true;
                    }
                }
            }

            DropJanggi(eventData.pointerDrag, transform);
            SoundManager.Instance.PlaySND(isKill ? SSfxName.KILL_JANGGI_SFX : SSfxName.JANGGI_DROP_SFX);

            // 현재 옮길 장기의 시작 슬롯과, 목표 슬롯 위치를 전달한다.
            JanggiSlot srcJanggiSlot = originParent.GetComponent<JanggiSlot>();
            NetworkManager.Instance.DropJanggi(srcJanggiSlot.heightNum, srcJanggiSlot.widthNum, heightNum, widthNum, isKill);
            
            if (isGameOver)
            {
                NetworkManager.Instance.StopGame(PhotonNetwork.IsMasterClient);
            }
            else
            {
                // 턴 종료 처리
                int nextUserIdx = ((FindObjectOfType<IngameScene>().isMasterTurn ? 1 : 0) + 1) % 2;
                NetworkManager.Instance.SelectNextTurnUser(nextUserIdx == 1);
            }
        }
    }

    private void DropJanggi(GameObject janggi, Transform parent)
    {
        janggi.transform.SetParent(parent);
        janggi.transform.SetAsFirstSibling();
        janggi.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }
}
