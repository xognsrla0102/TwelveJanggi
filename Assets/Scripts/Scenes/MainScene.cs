using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    [SerializeField] private Button matchBtn;
    [SerializeField] private Button quitBtn;

    [SerializeField] private GameObject dimObj;
    [SerializeField] private GameObject userNamePopup;
    [SerializeField] private GameObject matchLoadingPopup;

    private void Start()
    {
        matchBtn.onClick.AddListener(MatchGame);
        quitBtn.onClick.AddListener(QuitGame);

        if (string.IsNullOrWhiteSpace(PhotonNetwork.NickName))
        {
            userNamePopup.SetActive(true);
            matchLoadingPopup.SetActive(false);
        }

        SoundManager.Instance.PlayBGM(SBgmName.MAIN_BGM);
    }

    private void OnDestroy()
    {
        matchBtn.onClick.RemoveAllListeners();
        quitBtn.onClick.RemoveAllListeners();
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
        Application.isPlaying = false;
#endif
    }
}
