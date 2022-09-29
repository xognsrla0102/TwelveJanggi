using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;

public class GameResultPopup : MonoBehaviour
{
    [SerializeField] private GameObject dimObj;
    [SerializeField] private Button exitBtn;

    [SerializeField] private RawImage winnerUserProfileImage;
    [SerializeField] private TextMeshProUGUI winnerUserNameText;

    private void Start()
    {
        exitBtn.onClick.AddListener(OnClickExitBtn);
    }

    private void OnDestroy()
    {
        exitBtn.onClick.RemoveAllListeners();
    }

    private void OnEnable()
    {
        dimObj.SetActive(true);
    }

    private void OnDisable()
    {
        dimObj.SetActive(false);
    }

    public void SetResultPopup(Texture winnerUserProfileTexture, string winnerUserName)
    {
        SoundManager.Instance.StopBGM();

        if (PhotonNetwork.LocalPlayer.NickName.Equals(winnerUserName))
        {
            SoundManager.Instance.PlaySND(SSfxName.WIN_SFX);
        }
        else
        {
            SoundManager.Instance.PlaySND(SSfxName.LOSE_SFX);
        }

        winnerUserProfileImage.texture = winnerUserProfileTexture;
        winnerUserNameText.text = winnerUserName;
    }

    private void OnClickExitBtn()
    {
        NetworkManager.Instance.LeaveRoom();
    }
}
