using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.Networking;
using System.Collections;
using Unity.VisualScripting.Antlr3.Runtime;

public class UserNamePopup : MonoBehaviour
{
    [SerializeField] private GameObject dimObj;

    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private InputField profileTextureInputField;

    [SerializeField] private RawImage profileImage;
    [SerializeField] private Texture defaultTexture;

    [SerializeField] private Button okBtn;
    [SerializeField] private Button applyProfileBtn;

    private void Start()
    {
        okBtn.onClick.AddListener(OnClickOkBtn);
        applyProfileBtn.onClick.AddListener(OnClickApplyProfileBtn);

        profileImage.texture = defaultTexture;
    }

    private void OnDestroy()
    {
        okBtn.onClick.RemoveAllListeners();
        applyProfileBtn.onClick.RemoveAllListeners();
    }

    private void OnEnable()
    {
        dimObj.SetActive(true);
    }

    private void OnDisable()
    {
        dimObj.SetActive(false);
    }

    private void OnClickOkBtn()
    {
        if (string.IsNullOrWhiteSpace(userNameInputField.text))
        {
            return;
        }

        print($"닉네임 : {UserManager.Instance.userName}로 설정");
        UserManager.Instance.userName = userNameInputField.text;
        PhotonNetwork.NickName = UserManager.Instance.userName;

        print("프로필 이미지 설정");
        UserManager.Instance.profileTexture = profileImage.texture;
        FindObjectOfType<MainScene>().SetProfileInfo();

        print("프로필 초기화 완료");
        UserManager.Instance.isInitialized = true;
        gameObject.SetActive(false);
    }

    private void OnClickApplyProfileBtn()
    {
        if (string.IsNullOrWhiteSpace(profileTextureInputField.text))
        {
            profileImage.texture = defaultTexture;
            return;
        }

        StartCoroutine(GetTextureCoroutine());
    }

    private IEnumerator GetTextureCoroutine()
    {
        GameObject.Find("UI").GetComponent<CanvasGroup>().interactable = false;

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(profileTextureInputField.text);

        yield return request.SendWebRequest();

        GameObject.Find("UI").GetComponent<CanvasGroup>().interactable = true;
        if (request.result == UnityWebRequest.Result.ProtocolError || request.result == UnityWebRequest.Result.ConnectionError)
        {
            Debug.LogWarning($"이미지 로드 실패. 기본 이미지로 대체함\n" +
                $"원인 : {request.error}");

            profileImage.texture = defaultTexture;
            yield break;
        }

        print("프로필 이미지 URL 설정");
        UserManager.Instance.profileImageUrl = profileTextureInputField.text;

        profileImage.texture = (request.downloadHandler as DownloadHandlerTexture).texture;
    }
}
