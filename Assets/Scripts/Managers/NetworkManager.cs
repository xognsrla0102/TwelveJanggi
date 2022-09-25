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
        // 호스트가 씬을 이동할 때, 다른 클라이언트들도 씬을 이동하게 하면서 동시에, 씬을 동기화시켜줌.
        // (서로 씬이 달라서 같은 포톤 뷰 개체를 못 찾아서 RPC함수 호출이 씹히는 문제를 막을 수 있음[RPC 손실 방지])
        PhotonNetwork.AutomaticallySyncScene = true;

        // 동기화 속도 늘려서 유저 이동이 끊어져 보이지 않게 함
        PhotonNetwork.SendRate = 60;
        PhotonNetwork.SerializationRate = 30;
    }

    public void ConnectMasterServer()
    {
        print("마스터 서버 접속 시도");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("마스터 서버에 연결 완료");
        PhotonNetwork.JoinLobby();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        print($"연결 끊김. 이유 [{cause}]");

        SceneManager.LoadScene(SSceneName.MAIN_SCENE);

        //switch (cause)
        //{
        //    case DisconnectCause.DisconnectByClientLogic:
        //        print("타이틀로 이동");
        //        SceneManager.LoadScene(SSceneName.MAIN_SCENE);
        //        break;
        //    default:
        //        OKPopup popup = Popup.CreateErrorPopup("Server Disconnected", $"{cause}") as OKPopup;
        //        popup.SetOKBtnAction(() =>
        //        {
        //            print("서버 끊김으로 인한 타이틀 씬 이동");
        //            LoadingManager.LoadScene(SSceneName.TITLE_SCENE);
        //        });
        //        break;
        //}
    }

    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        print("로비 서버 접속 완료");

        print("메인 씬으로 이동");
        SceneManager.LoadScene(SSceneName.MAIN_SCENE);
    }

    public void LeaveLobby()
    {
        print("로비 떠나기 시도");
        PhotonNetwork.LeaveLobby();
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        print("로비 떠남");

        print("마스터 서버 연결 해제");
        PhotonNetwork.Disconnect();
    }

    public void OnJoinRandomRoom()
    {
        print("방 참가 혹은 생성");

        // 2명 참가 가능한 방
        PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom,
            null, null, null, new RoomOptions { MaxPlayers = 2}, null);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log($"방 생성 실패 :\n코드 : {returnCode}\n메세지 : {message}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log($"방 참가 실패 :\n코드 : {returnCode}\n메세지 : {message}");
    }

    // OnCreateRoom 함수 호출 뒤 이곳으로 들어옴
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        print("방 참가 완료");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        Debug.Log($"{newPlayer.NickName} 참가");

        PhotonNetwork.LoadLevel(SSceneName.INGAME_SCENE);
    }

    public void LeaveRoom()
    {
        print("방 떠나기 시도");

        Debug.Assert(PhotonNetwork.InRoom == true);
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();

        print("방 떠남, 자동으로 게임 서버 연결 해제 후 마스터 서버 접속 시도..");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        base.OnPlayerLeftRoom(otherPlayer);

        print($"{otherPlayer.NickName}가 떠남");

        print("누군가 방을 나가면 나도 방 나감, 자동으로 메인 씬으로 이동");
        LeaveRoom();
    }
}
