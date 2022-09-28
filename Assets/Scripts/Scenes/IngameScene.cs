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
    [SerializeField] private GameResultPopup gameResultPopup;
    [SerializeField] private TextMeshProUGUI countDownText;

    [HideInInspector] public bool isMasterTurn;

    private const float TURN_TIME = 30f;
    private const int COUNT_DOWN_TIME = 5;

    private JanggiSlot[,] janggiSlots = new JanggiSlot[4, 3];

    private float turnStartTime;

    private bool stopTimer;

    private bool isBeepSound;

    private int myScore;
    private int enemyScore;

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

        // 장기 생성
        Transform slotGridTransform = GameObject.Find("SlotGrid").transform;
        Janggi janggiPrefab = Resources.Load<Janggi>("Janggi");

        // 방장이라면 7,9,10,11번 슬롯 장기가 내것
        // 아니라면 0,1,2,4번 슬롯 장기가 내것
        int[] myJanggiNums = PhotonNetwork.IsMasterClient ? new int[] { 7, 9, 10, 11 } : new int[] { 0, 1, 2, 4 };

        int myJanggiNum = 0;
        for (int i = 0; i < 12; i++)
        {
            // 현재 슬롯에 장기가 이미 있다면 비우고
            Transform nowSlotJanggi = slotGridTransform.GetChild(i).Find("Janggi");
            if (nowSlotJanggi != null)
            {
                Destroy(nowSlotJanggi.gameObject);
            }

            // 초기 위치 슬롯의 장기만 다시 둠
            if (i == 0 || i == 1 || i == 2 || i == 4 | i == 7 || i == 9 || i == 10 || i == 11)
            {
                Janggi janggi = Instantiate(janggiPrefab);
                RectTransform janggiRectTransform = janggi.GetComponent<RectTransform>();

                janggi.transform.SetParent(slotGridTransform.GetChild(i));
                janggi.transform.SetAsFirstSibling();
                janggi.gameObject.name = "Janggi";
                janggiRectTransform.anchoredPosition = Vector3.zero;
                janggiRectTransform.sizeDelta = janggiPrefab.GetComponent<RectTransform>().sizeDelta;
                janggiRectTransform.localScale = Vector3.one;

                switch (i)
                {
                    case 0:
                    case 11:
                        janggi.SetJanggi(EJanggiType.JANG);
                        break;
                    case 1:
                    case 10:
                        janggi.SetJanggi(EJanggiType.WANG);
                        break;
                    case 2:
                    case 9:
                        janggi.SetJanggi(EJanggiType.SANG);
                        break;
                    case 4:
                    case 7:
                        janggi.SetJanggi(EJanggiType.JA);
                        break;
                    default:
                        Debug.Assert(false);
                        break;
                }

                if (myJanggiNum < myJanggiNums.Length && i == myJanggiNums[myJanggiNum])
                {
                    janggi.isMyJanggi = true;
                    myJanggiNum++;
                }
            }
        }

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
        bool isMasterWin = !PhotonNetwork.IsMasterClient;
        NetworkManager.Instance.EndGame(isMasterWin);
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

                // 패배 처리
                if (PhotonNetwork.IsMasterClient)
                {
                    bool isMasterWin = isMasterTurn == false;
                    NetworkManager.Instance.StopGame(isMasterWin);
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

    public void OnDropJanggi(int srcHeightNum, int srcWidthNum, int destHeightNum, int destWidthNum, bool isKill)
    {
        print($"{srcHeightNum},{srcWidthNum}의 장기를 {destHeightNum},{destWidthNum} 위치로 둡니다.");

        SoundManager.Instance.PlaySND(isKill ? SSfxName.KILL_JANGGI_SFX : SSfxName.JANGGI_DROP_SFX);

        // 목적 슬롯의 장기를 먹은 경우, 해당 장기 삭제
        if (isKill)
        {
            Destroy(janggiSlots[destHeightNum, destWidthNum].transform.Find("Janggi").gameObject);
        }

        // 시작 슬롯의 장기를 목적 슬롯으로 이동
        RectTransform janggiRectTransform = janggiSlots[srcHeightNum, srcWidthNum].transform.Find("Janggi").GetComponent<RectTransform>();
        janggiRectTransform.SetParent(janggiSlots[destHeightNum, destWidthNum].transform);
        janggiRectTransform.SetAsFirstSibling();
        janggiRectTransform.anchoredPosition = Vector3.zero;


        // 장기가 자이면서 목적 슬롯이 적 진영이면 장기 타입을 후로 변경
        Janggi janggi = janggiRectTransform.GetComponent<Janggi>();
        if (janggi.JanggiType == EJanggiType.JA)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // 적 진영
                if (destHeightNum == 0)
                {
                    janggi.SetJanggi(EJanggiType.HU);
                }
            }
            else
            {
                if (destHeightNum == 3)
                {
                    janggi.SetJanggi(EJanggiType.HU);
                }
            }
        }
        // 반대로 후이면서 목적 슬롯이 적 진영 벗어나면 자로 변경
        else if (janggi.JanggiType == EJanggiType.HU)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                if (destHeightNum != 0)
                {
                    janggi.SetJanggi(EJanggiType.JA);
                }
            }
            else
            {
                if (destHeightNum != 3)
                {
                    janggi.SetJanggi(EJanggiType.JA);
                }
            }
        }
    }

    public void ShowShadowJanggi(int slotHeightNum, int slotWidthNum, EJanggiType janggiType)
    {
        int deltaY;
        int nowY, nowX;
        Transform slotJanggiTransform;

        switch (janggiType)
        {
            #region 장
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
            #endregion
            #region 상
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
            #endregion
            #region 왕
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
            #endregion
            #region 자
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

                // 적 진영으로 이동하게 될 자는 후로 보이도록 설정
                if (PhotonNetwork.IsMasterClient)
                {
                    janggiSlots[nowY, slotWidthNum].shadowJanggi.SetJanggi(nowY == 0 ? EJanggiType.HU : janggiType);
                }
                else
                {
                    janggiSlots[nowY, slotWidthNum].shadowJanggi.SetJanggi(nowY == 3 ? EJanggiType.HU : janggiType);
                }
                break;
            #endregion
            #region 후
            case EJanggiType.HU:
                for (int height = -1; height <= 1; height++)
                {
                    for (int width = -1; width <= 1; width++)
                    {
                        if (PhotonNetwork.IsMasterClient)
                        {
                            if (height == 1)
                            {
                                if (width == -1 || width == 1)
                                {
                                    continue;
                                }
                            }
                        }
                        else
                        {
                            if (height == -1)
                            {
                                if (width == -1 || width == 1)
                                {
                                    continue;
                                }
                            }
                        }

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

                        // 적 진영을 나오게 될 후는 자로 설정
                        if (PhotonNetwork.IsMasterClient)
                        {
                            janggiSlots[nowY, nowX].shadowJanggi.SetJanggi(nowY != 0 ? EJanggiType.JA : janggiType);
                        }
                        else
                        {
                            janggiSlots[nowY, nowX].shadowJanggi.SetJanggi(nowY != 3 ? EJanggiType.JA : janggiType);
                        }
                    }
                }
                break;
            #endregion
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

    public void StopGame(bool isMasterWin)
    {
        HideShadowJanggi();

        bool isMyWin = PhotonNetwork.IsMasterClient == isMasterWin;

        if (isMyWin)
        {
            myScore++;
        }
        else
        {
            enemyScore++;
        }

        scoreText.text = $"<color=#00ff00>{myScore}</color> : <color=#ff0000>{enemyScore}</color>";

        if (myScore >= 2 || enemyScore >= 2)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                NetworkManager.Instance.EndGame(isMasterWin);
            }
        }
        else
        {
            StartGame();
        }
    }

    public void EndGame(bool isMasterWin)
    {
        stopTimer = true;

        dimObj.SetActive(true);

        bool isMyWin = PhotonNetwork.IsMasterClient == isMasterWin;

        if (isMyWin)
        {
            gameResultPopup.SetResultPopup(profileImage.texture, userNameText.text);
        }
        else
        {
            gameResultPopup.SetResultPopup(enemyProfileImage.texture, enemyUserNameText.text);
        }
        gameResultPopup.gameObject.SetActive(true);
    }
}
