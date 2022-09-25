using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using UnityEditor.XR;

public class NetworkManager : Singleton<NetworkManager>
{
    private void Start()
    {
        // ȣ��Ʈ�� ���� �̵��� ��, �ٸ� Ŭ���̾�Ʈ�鵵 ���� �̵��ϰ� �ϸ鼭 ���ÿ�, ���� ����ȭ������.
        // (���� ���� �޶� ���� ���� �� ��ü�� �� ã�Ƽ� RPC�Լ� ȣ���� ������ ������ ���� �� ����[RPC �ս� ����])
        PhotonNetwork.AutomaticallySyncScene = true;

        // ����ȭ �ӵ� �÷��� ���� �̵��� ������ ������ �ʰ� ��
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void ConnectMasterServer()
    {
        print("������ ���� ���� �õ�");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("������ ������ ���� �Ϸ�");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        print($"���� ����. ���� [{cause}]");

        SceneManager.LoadScene(SSceneName.MAIN_SCENE);

        //switch (cause)
        //{
        //    case DisconnectCause.DisconnectByClientLogic:
        //        print("Ÿ��Ʋ�� �̵�");
        //        SceneManager.LoadScene(SSceneName.MAIN_SCENE);
        //        break;
        //    default:
        //        OKPopup popup = Popup.CreateErrorPopup("Server Disconnected", $"{cause}") as OKPopup;
        //        popup.SetOKBtnAction(() =>
        //        {
        //            print("���� �������� ���� Ÿ��Ʋ �� �̵�");
        //            LoadingManager.LoadScene(SSceneName.TITLE_SCENE);
        //        });
        //        break;
        //}
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print("�κ� ���� ���� �Ϸ�");

        print("���� ������ �̵�");
        SceneManager.LoadScene(SSceneName.MAIN_SCENE);
    }

    public void LeaveLobby()
    {
        print("�κ� ������ �õ�");
        PhotonNetwork.LeaveLobby();
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        print("�κ� ����");

        print("������ ���� ���� ����");
        PhotonNetwork.Disconnect();
    }

    public void OnJoinRandomRoom()
    {
        print("�� ���� Ȥ�� ����");

        // 2�� ���� ������ ��
        PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom,
            null, null, null, new RoomOptions { MaxPlayers = 2}, null);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log($"�� ���� ���� :\n�ڵ� : {returnCode}\n�޼��� : {message}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log($"�� ���� ���� :\n�ڵ� : {returnCode}\n�޼��� : {message}");
    }

    // OnCreateRoom �Լ� ȣ�� �� �̰����� ����
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("�� ���� �Ϸ�");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} ����");

        PhotonNetwork.LoadLevel(SSceneName.INGAME_SCENE);
    }

    public void LeaveRoom()
    {
        print("�� ������ �õ�");

        Debug.Assert(PhotonNetwork.InRoom == true);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        print("�� ����, �ڵ����� ���� ���� ���� ���� �� ������ ���� ���� �õ�..");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        print($"{otherPlayer.NickName}�� ����");

        print("������ ���� ������ ���� �� ����, �ڵ����� ���� ������ �̵�");
        LeaveRoom();
    }
}
