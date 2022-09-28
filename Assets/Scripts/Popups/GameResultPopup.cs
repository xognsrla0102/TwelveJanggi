using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
        winnerUserProfileImage.texture = winnerUserProfileTexture;
        winnerUserNameText.text = winnerUserName;
    }

    private void OnClickExitBtn()
    {
        NetworkManager.Instance.LeaveRoom();
    }
}
