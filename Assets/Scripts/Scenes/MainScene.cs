using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    [SerializeField] private Button matchBtn;
    [SerializeField] private Button quitBtn;

    [SerializeField] private GameObject dimObj;
    [SerializeField] private GameObject userNamePopup;
    [SerializeField] private GameObject matchLoadingPopup;

    [SerializeField] private RawImage profileImage;
    [SerializeField] private TextMeshProUGUI userNameText;

    private void Start()
    {
        matchBtn.onClick.AddListener(MatchGame);
        quitBtn.onClick.AddListener(QuitGame);

        SoundManager.Instance.PlayBGM(SBgmName.MAIN_BGM);

        if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            userNamePopup.SetActive(true);
            matchLoadingPopup.SetActive(false);
        }

        if (UserManager.Instance.isInitialized)
        {
            SetProfileInfo();
        }
    }

    private void OnDestroy()
    {
        matchBtn.onClick.RemoveAllListeners();
        quitBtn.onClick.RemoveAllListeners();
    }

    public void SetProfileInfo()
    {
        profileImage.texture = UserManager.Instance.profileTexture;
        userNameText.text = UserManager.Instance.userName;
    }

    private void MatchGame()
    {
        matchLoadingPopup.SetActive(true);
        NetworkManager.Instance.OnJoinRandomRoom();
    }

    private void QuitGame()
    {
        print("게임 종료");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
