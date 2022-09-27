using Photon.Pun;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class IngameScene : MonoBehaviour
{
    [Header("내 프로필")]
    [SerializeField] private TextMeshProUGUI userNameText;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private GameObject myTurnOutLine;

    [Header("상대 프로필")]
    [SerializeField] private TextMeshProUGUI enemyUserNameText;
    [SerializeField] private RawImage enemyProfileImage;
    [SerializeField] private GameObject enemyTurnOutLine;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI scoreText;

    [SerializeField] private Button surrenderBtn;

    [SerializeField] private GameObject dimObj;
    [SerializeField] private TextMeshProUGUI countDownText;

    [HideInInspector] public bool isMasterTurn;

    private const float TURN_TIME = 30f;
    private const int COUNT_DOWN_TIME = 5;

    private JanggiSlot[,] janggiSlots = new JanggiSlot[4, 3];

    private float turnStartTime;

    private bool stopTimer;

    private bool isBeepSound;

    private void Awake()
    {
        surrenderBtn.onClick.AddListener(OnClickSurrenderBtn);

        SoundManager.Instance.PlayBGM(SBgmName.INGAME_BGM);

        profileImage.texture = UserManager.Instance.profileTexture;
        userNameText.text = UserManager.Instance.userName;

        // 내 프로필 정보 상대에게 전달
        NetworkManager.Instance.SendMyProfileInfo();

        // 장기 슬롯 담기
        Transform slotGridTransform = GameObject.Find("SlotGrid").transform;
        for (int i = 0; i < 12; i++)
        {
            int heightNum = i / 3;
            int widthNum = i % 3;

            janggiSlots[heightNum, widthNum] = slotGridTransform.GetChild(i).GetComponent<JanggiSlot>();

            janggiSlots[heightNum, widthNum].heightNum = heightNum;
            janggiSlots[heightNum, widthNum].widthNum = widthNum;
        }

        // 방장이라면 7,9,10,11번 슬롯.
        // 아니라면 0,1,2,4번 슬롯의 장기임
        int[] myJanggiNums = PhotonNetwork.IsMasterClient ? new int[] {7,9,10,11} : new int[] {0,1,2,4};
        for (int i = 0; i < myJanggiNums.Length; i++)
        {
            slotGridTransform.GetChild(myJanggiNums[i])
                .Find("Janggi").GetComponent<Janggi>().isMyJanggi = true;
        }

        // 방장과 반대 방향으로 보드판을 돌림
        if (PhotonNetwork.IsMasterClient == false)
        {
            slotGridTransform.Rotate(new Vector3(0f, 0f, 180f));
        }

        StartGame();
    }

    public void StartGame()
    {
        myTurnOutLine.SetActive(false);
        enemyTurnOutLine.SetActive(false);

        timerText.text = "00.00";
        stopTimer = true;

        StartCoroutine(StartGameCoroutine());
    }

    private IEnumerator StartGameCoroutine()
    {
        dimObj.SetActive(true);
        countDownText.gameObject.SetActive(true);

        int count = COUNT_DOWN_TIME;
        while (true)
        {
            SoundManager.Instance.PlaySND(SSfxName.COUNT_DOWN_SFX);
            countDownText.text = $"{count}";
            yield return new WaitForSeconds(1f);

            count--;
            if (count == 0)
            {
                SoundManager.Instance.PlaySND(SSfxName.GAME_START_SFX);
                countDownText.text = "START!!";
                yield return new WaitForSeconds(1f);

                break;
            }
        }

        dimObj.SetActive(false);
        countDownText.gameObject.SetActive(false);

        // 방장이 랜덤으로 선 정함
        if (PhotonNetwork.IsMasterClient)
        {
            NetworkManager.Instance.SelectFirstTurnUser();
        }
    }

    private void OnDestroy()
    {
        surrenderBtn.onClick.RemoveAllListeners();
    }

    private void OnClickSurrenderBtn()
    {        
        NetworkManager.Instance.LeaveRoom();
    }

    public void SetTurn(bool isMasterTurn)
    {
        this.isMasterTurn = isMasterTurn;

        // 현재 턴이 방장인지 유무와 내가 방장인지 유무 상태가 같다면 내 턴
        bool isMyTurn = this.isMasterTurn == PhotonNetwork.IsMasterClient;

        // 현재 턴 유저의 아웃라인 오브젝트 활성화
        myTurnOutLine.SetActive(isMyTurn);
        enemyTurnOutLine.SetActive(isMyTurn == false);        

        // 내 장기 옮길 수 있는지 세팅
        for (int height = 0; height < 4; height++)
        {
            for (int width = 0; width < 3; width++)
            {
                Transform janggiTransform = janggiSlots[height, width].transform.Find("Janggi");
                if (janggiTransform != null)
                {
                    Janggi janggi = janggiTransform.GetComponent<Janggi>();
                    if (janggi.isMyJanggi)
                    {
                        janggi.canvasGroup.blocksRaycasts = isMyTurn;
                    }
                    else
                    {
                        janggi.canvasGroup.blocksRaycasts = false;
                    }
                }
            }
        }

        // 타이머 작동 시작
        turnStartTime = Time.time;
        stopTimer = false;

        isBeepSound = false;
    }

    private void Update()
    {
        if (stopTimer == false)
        {
            float remainSec = TURN_TIME - (Time.time - turnStartTime);
            timerText.text = $"{remainSec:00.00}";

            if (isBeepSound == false && remainSec <= 10f)
            {
                SoundManager.Instance.PlaySND(SSfxName.TIMER_SFX);
                isBeepSound = true;
            }

            // 현재 타이머가 종료되었을 경우
            if (remainSec <= 0)
            {
                stopTimer = true;
                timerText.text = "00.00";

                // 방장이 턴 종료 처리 (다음 턴 해야 하나.. 게임오버 해야 하나..)
                if (PhotonNetwork.IsMasterClient)
                {
                    int nextUserIdx = ((isMasterTurn ? 1 : 0) + 1) % 2;
                    NetworkManager.Instance.SelectNextTurnUser(nextUserIdx == 1);
                }
            }
        }
    }

    public void SetEnemyProfile(string userName, string profileImageUrl)
    {
        enemyUserNameText.text = userName;

        if (string.IsNullOrWhiteSpace(profileImageUrl) == false)
        {
            StartCoroutine(GetTextureCoroutine(profileImageUrl));
        }
    }

    private IEnumerator GetTextureCoroutine(string profileImageUrl)
    {
        surrenderBtn.interactable = false;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(profileImageUrl);

        yield return request.SendWebRequest();

        surrenderBtn.interactable = true;
        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning($"이미지 로드 실패. 원인 : {request.error}");
            yield break;
        }

        enemyProfileImage.texture = (request.downloadHandler as DownloadHandlerTexture).texture;
    }

    public void OnDropJanggi(int srcHeightNum, int srcWidthNum, int destHeightNum, int destWidthNum)
    {
        print($"{srcHeightNum},{srcWidthNum}의 장기를 {destHeightNum},{destWidthNum} 위치로 둡니다.");

        SoundManager.Instance.PlaySND(SSfxName.JANGGI_DROP_SFX);

        RectTransform janggiRectTransform = janggiSlots[srcHeightNum, srcWidthNum].transform.Find("Janggi").GetComponent<RectTransform>();
        janggiRectTransform.SetParent(janggiSlots[destHeightNum, destWidthNum].transform);
        janggiRectTransform.SetAsFirstSibling();
        janggiRectTransform.anchoredPosition = Vector3.zero;
    }

    public void ShowShadowJanggi(int slotHeightNum, int slotWidthNum, EJanggiType janggiType)
    {
        int deltaY, deltaX;
        int nowY, nowX;
        Transform slotJanggiTransform;

        switch (janggiType)
        {
            case EJanggiType.JANG:
                for (int height = -1; height <= 1; height++)
                {
                    for (int width = -1; width <= 1; width++)
                    {
                        if (height == -1)
                        {
                            if (width == -1 || width == 1)
                            {
                                continue;
                            }
                        }
                        else if (height == 0)
                        {
                            if (width == 0)
                            {
                                continue;
                            }
                        }
                        else
                        {
                            if (width == -1 || width == 1)
                            {
                                continue;
                            }
                        }

                        nowY = slotHeightNum + height;
                        nowX = slotWidthNum + width;

                        if (nowY < 0 || nowY > 3 || nowX < 0 || nowX > 2)
                        {
                            continue;
                        }

                        // 해당 슬롯에 내 장기가 있을 경우 쉐도우 장기 활성화 안 함
                        slotJanggiTransform = janggiSlots[nowY, nowX].transform.Find("Janggi");
                        if (slotJanggiTransform != null)
                        {
                            if (slotJanggiTransform.GetComponent<Janggi>().isMyJanggi)
                            {
                                continue;
                            }
                        }

                        janggiSlots[nowY, nowX].shadowJanggi.gameObject.SetActive(true);
                        janggiSlots[nowY, nowX].shadowJanggi.SetJanggi(janggiType);
                    }
                }
                break;
            case EJanggiType.SANG:
                for (int height = -1; height <= 1; height++)
                {
                    for (int width = -1; width <= 1; width++)
                    {
                        if (height == -1)
                        {
                            if (width == 0)
                            {
                                continue;
                            }
                        }
                        else if (height == 0)
                        {
                            continue;
                        }
                        else
                        {
                            if (width == 0)
                            {
                                continue;
                            }
                        }

                        nowY = slotHeightNum + height;
                        nowX = slotWidthNum + width;

                        if (nowY < 0 || nowY > 3 || nowX < 0 || nowX > 2)
                        {
                            continue;
                        }

                        // 해당 슬롯에 내 장기가 있을 경우 쉐도우 장기 활성화 안 함
                        slotJanggiTransform = janggiSlots[nowY, nowX].transform.Find("Janggi");
                        if (slotJanggiTransform != null)
                        {
                            if (slotJanggiTransform.GetComponent<Janggi>().isMyJanggi)
                            {
                                continue;
                            }
                        }

                        janggiSlots[nowY, nowX].shadowJanggi.gameObject.SetActive(true);
                        janggiSlots[nowY, nowX].shadowJanggi.SetJanggi(janggiType);
                    }
                }
                break;
            case EJanggiType.WANG:
                for (int height = -1; height <= 1; height++)
                {
                    for (int width = -1; width <= 1; width++)
                    {
                        if (height == 0 && width == 0)
                        {
                            continue;
                        }

                        nowY = slotHeightNum + height;
                        nowX = slotWidthNum + width;

                        if (nowY < 0 || nowY > 3 || nowX < 0 || nowX > 2)
                        {
                            continue;
                        }

                        // 해당 슬롯에 내 장기가 있을 경우 쉐도우 장기 활성화 안 함
                        slotJanggiTransform = janggiSlots[nowY, nowX].transform.Find("Janggi");
                        if (slotJanggiTransform != null)
                        {
                            if (slotJanggiTransform.GetComponent<Janggi>().isMyJanggi)
                            {
                                continue;
                            }
                        }

                        janggiSlots[nowY, nowX].shadowJanggi.gameObject.SetActive(true);
                        janggiSlots[nowY, nowX].shadowJanggi.SetJanggi(janggiType);
                    }
                }
                break;
            case EJanggiType.JA:
                deltaY = PhotonNetwork.IsMasterClient ? -1 : 1;
                nowY = slotHeightNum + deltaY;

                if (nowY < 0 || nowY > 3)
                {
                    break;
                }

                // 해당 슬롯에 내 장기가 있을 경우 쉐도우 장기 활성화 안 함
                slotJanggiTransform = janggiSlots[nowY, slotWidthNum].transform.Find("Janggi");
                if (slotJanggiTransform != null)
                {
                    if (slotJanggiTransform.GetComponent<Janggi>().isMyJanggi)
                    {
                        break;
                    }
                }

                janggiSlots[nowY, slotWidthNum].shadowJanggi.gameObject.SetActive(true);
                janggiSlots[nowY, slotWidthNum].shadowJanggi.SetJanggi(janggiType);
                break;
            case EJanggiType.HU:
                // 후 해야함
                break;
            default:
                Debug.Assert(false);
                break;
        }
    }

    public void HideShadowJanggi()
    {
        for (int height = 0; height < 4; height++)
        {
            for (int width = 0; width < 3; width++)
            {
                janggiSlots[height, width].shadowJanggi.gameObject.SetActive(false);
            }
        }
    }
}
