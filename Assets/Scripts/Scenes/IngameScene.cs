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

    private JanggiSlot[,] janggiSlots = new JanggiSlot[4, 3];

    private const float TURN_TIME = 30f;

    private float turnStartTime;

    private bool isMasterTurn;
    private bool stopTimer;

    private void Start()
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
            janggiSlots[i / 3, i % 3] = slotGridTransform.GetChild(i).GetComponent<JanggiSlot>();
        }

        StartGame();
    }

    public void StartGame()
    {
        myTurnOutLine.SetActive(false);
        enemyTurnOutLine.SetActive(false);

        timerText.text = "00.00";

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
                }
            }
        }

        // 타이머 작동 시작
        turnStartTime = Time.time;
        stopTimer = false;
    }

    private void Update()
    {
        if (stopTimer == false)
        {
            float remainSec = TURN_TIME - (Time.time - turnStartTime);
            timerText.text = $"{remainSec:00.00}";

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

        bool isMyTurn = isMasterTurn == PhotonNetwork.IsMasterClient;
        if (isMyTurn == false)
        {
            return;
        }

        // 장기 조작 처리
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
}
