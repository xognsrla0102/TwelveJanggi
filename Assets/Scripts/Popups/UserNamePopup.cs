using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;

public class UserNamePopup : MonoBehaviour
{
    [SerializeField] private GameObject dimObj;
    [SerializeField] private TMP_InputField userNameInputField;
    [SerializeField] private Button okBtn;

    private void Start()
    {
        okBtn.onClick.AddListener(OnClickOkBtn);
    }

    private void OnDestroy()
    {
        okBtn.onClick.RemoveAllListeners();
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

        PhotonNetwork.NickName = userNameInputField.text;
        print($"닉네임 : {PhotonNetwork.NickName}로 설정");

        gameObject.SetActive(false);
    }
}
