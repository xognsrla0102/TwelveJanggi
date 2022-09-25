using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class IngameScene : MonoBehaviour
{
    [SerializeField] private RawImage profileImage;
    [SerializeField] private TextMeshProUGUI userNameText;

    private void Start()
    {
        SoundManager.Instance.PlayBGM(SBgmName.MAIN_BGM);

        profileImage.texture = UserManager.Instance.profileTexture;
        userNameText.text = UserManager.Instance.userName;

        // 선 정하기
        //NetworkManager.Instance.
    }

}
