using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MatchLoadingPopup : MonoBehaviour
{
    [SerializeField] private GameObject dimObj;
    [SerializeField] private RawImage loadingImage;
    [SerializeField] private Button cancelBtn;

    private const float ROTATE_SPD = 50f;

    private void Start()
    {
        cancelBtn.onClick.AddListener(OnClickCancelBtn);
    }

    private void OnDestroy()
    {
        cancelBtn.onClick.RemoveAllListeners();
    }

    private void OnEnable()
    {
        dimObj.SetActive(true);
        loadingImage.transform.rotation = Quaternion.identity;
        StartCoroutine(ActiveCancelBtnCoroutine());
    }

    private IEnumerator ActiveCancelBtnCoroutine()
    {
        cancelBtn.interactable = false;

        while (PhotonNetwork.NetworkClientState != Photon.Realtime.ClientState.Joined)
        {
            yield return null;
        }

        cancelBtn.interactable = true;
    }

    private void OnDisable()
    {
        dimObj.SetActive(false);
    }

    private void Update()
    {
        loadingImage.transform.Rotate(new Vector3(0f, 0f, ROTATE_SPD) * Time.deltaTime);
    }

    private void OnClickCancelBtn()
    {
        NetworkManager.Instance.LeaveRoom();

        gameObject.SetActive(false);
    }
}
