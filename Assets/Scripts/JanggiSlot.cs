using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Photon.Pun;

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
        if (eventData.pointerDrag == null)
        {
            return;
        }

        Janggi draggingJanggi = eventData.pointerDrag.GetComponent<Janggi>();
        TakeJanggi draggingTakeJanggi = eventData.pointerDrag.GetComponent<TakeJanggi>();

        // 일반 장기를 드랍하는 경우
        if (draggingJanggi != null)
        {
            Transform originParent = draggingJanggi.originParent;

            // 원래 슬롯으로 제자리 두는 경우는 무시
            if (transform == originParent)
            {
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
                    // 왕을 죽일 경우 게임 오버 처리
                    if (janggi.JanggiType == EJanggiType.WANG)
                    {
                        isGameOver = true;
                    }
                    // 다른 타입일 경우 포로 획득 처리
                    else
                    {
                        NetworkManager.Instance.GetEnemyJanggi(PhotonNetwork.IsMasterClient, janggi.JanggiType);
                    }

                    // 해당 장기 오브젝트 삭제
                    Destroy(mySlotJanggiTransform.gameObject);
                    isKill = true;
                }
            }

            // 옮기고 있는 장기가 자 이고 현재 드랍한 위치가 상대 진영일 경우 후로 변경
            if (draggingJanggi.JanggiType == EJanggiType.JA)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (heightNum == 0)
                    {
                        draggingJanggi.SetJanggi(EJanggiType.HU);
                    }
                }
                else
                {
                    if (heightNum == 3)
                    {
                        draggingJanggi.SetJanggi(EJanggiType.HU);
                    }
                }
            }
            // 후일 경우 반대로 상대 진영 벗어나면 자로 변경
            else if (draggingJanggi.JanggiType == EJanggiType.HU)
            {
                if (PhotonNetwork.IsMasterClient)
                {
                    if (heightNum != 0)
                    {
                        draggingJanggi.SetJanggi(EJanggiType.JA);
                    }
                }
                else
                {
                    if (heightNum != 3)
                    {
                        draggingJanggi.SetJanggi(EJanggiType.JA);
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
        // 먹은 장기를 드랍하는 경우
        else if (draggingTakeJanggi != null)
        {
            
        }
    }

    private void DropJanggi(GameObject janggi, Transform parent)
    {
        janggi.transform.SetParent(parent);
        janggi.transform.SetAsFirstSibling();
        janggi.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
    }
}
