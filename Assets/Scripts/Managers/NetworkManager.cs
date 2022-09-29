using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

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

    #region 마스터 서버
    public void ConnectMasterServer()
    {
        print("마스터 서버 접속 시도");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        print("마스터 서버에 연결 완료");

        print("메인 씬으로 이동");
        SceneManager.LoadScene(SSceneName.MAIN_SCENE);
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        base.OnDisconnected(cause);
        print("연결 끊김. 방으로 이동.\n" +
            $"이유 [{cause}]");

        SceneManager.LoadScene(SSceneName.MAIN_SCENE);
    }
    #endregion

    #region 방
    public void OnJoinRandomRoomOrCreateRoom()
    {
        print("방 참가 혹은 생성");

        // 2명 참가 가능한 방
        PhotonNetwork.JoinRandomOrCreateRoom(null, 0, MatchmakingMode.FillRoom,
            null, null, null, new RoomOptions { MaxPlayers = 2}, null);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        Debug.Log("방 생성 실패 :\n" +
            $"코드 : {returnCode}\n" +
            $"메세지 : {message}");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.Log("방 참가 실패 :\n" +
            $"코드 : {returnCode}\n" +
            $"메세지 : {message}");
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        print("방 생성 완료");
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
        print($"{newPlayer.NickName}가 방에 참가");

        // 게임 시작 후에는 유저가 못 들어옴 (게임 종료 후, 유저 나가면 다른 유저 들어오는 것 방지)
        PhotonNetwork.CurrentRoom.IsOpen = false;

        print("게임 씬으로 이동");
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
    }
    #endregion

    #region 인게임
    public void SendMyProfileInfo()
    {
        photonView.RPC(nameof(SendMyProfileInfoRPC), RpcTarget.Others,
            UserManager.Instance.userName,
            UserManager.Instance.profileImageUrl);
    }

    [PunRPC] private void SendMyProfileInfoRPC(string userName, string profileImageUrl)
    {
        print("상대 프로필 정보 받음.");
        print($"유저 이름 : {userName}\n" +
            $"프로필 이미지 URL : {profileImageUrl}");

        FindObjectOfType<IngameScene>().SetEnemyProfile(userName, profileImageUrl);
    }

    public void SelectFirstTurnUser()
    {
        // 방장이 랜덤으로 0, 1 중에 하나 정해서 RPC로 전달
        bool isMasterFirstTurn = Random.Range(0, 2) == 0;

        photonView.RPC(nameof(SelectNowTurnUserRPC), RpcTarget.All, isMasterFirstTurn);
    }

    public void SelectNextTurnUser(bool isNextTurnMaster)
    {
        photonView.RPC(nameof(SelectNowTurnUserRPC), RpcTarget.All, isNextTurnMaster);
    }

    [PunRPC] private void SelectNowTurnUserRPC(bool isMasterTurn)
    {
        FindObjectOfType<IngameScene>().SetTurn(isMasterTurn);
    }

    public void DropJanggi(int srcHeightNum, int srcWidthNum, int destHeightNum, int destWidthNum, bool isKill)
    {
        photonView.RPC(nameof(DropJanggiRPC), RpcTarget.Others,
            srcHeightNum, srcWidthNum, destHeightNum, destWidthNum, isKill);
    }

    [PunRPC] private void DropJanggiRPC(int srcHeightNum, int srcWidthNum, int destHeightNum, int destWidthNum, bool isKill)
    {
        FindObjectOfType<IngameScene>().OnDropJanggi(srcHeightNum, srcWidthNum, destHeightNum, destWidthNum, isKill);
    }

    public void DropTakeJanggi(int siblingIndex, int destHeightNum, int destWidthNum)
    {
        photonView.RPC(nameof(DropTakeJanggiRPC), RpcTarget.Others,
            siblingIndex, destHeightNum, destWidthNum);
    }

    [PunRPC] private void DropTakeJanggiRPC(int siblingIndex, int destHeightNum, int destWidthNum)
    {
        FindObjectOfType<IngameScene>().OnDropTakeJanggi(siblingIndex, destHeightNum, destWidthNum);
    }

    public void GetEnemyJanggi(bool isMasterGetEnemyJanggi, EJanggiType janggiType)
    {
        photonView.RPC(nameof(GetEnemyJanggiRPC), RpcTarget.All, isMasterGetEnemyJanggi, janggiType);
    }

    [PunRPC] private void GetEnemyJanggiRPC(bool isMasterGetEnemyJanggi, EJanggiType janggiType)
    {
        FindObjectOfType<IngameScene>().GetEnemyJanggi(isMasterGetEnemyJanggi, janggiType);
    }

    public void StopGame(bool isMasterWin)
    {
        photonView.RPC(nameof(StopGameRPC), RpcTarget.All, isMasterWin);
    }

    [PunRPC] private void StopGameRPC(bool isMasterWin)
    {
        FindObjectOfType<IngameScene>().StopGame(isMasterWin);
    }

    public void EndGame(bool isMasterWin)
    {
        photonView.RPC(nameof(EndGameRPC), RpcTarget.All, isMasterWin);
    }

    [PunRPC] private void EndGameRPC(bool isMasterWin)
    {
        FindObjectOfType<IngameScene>().EndGame(isMasterWin);
    }
    #endregion
}
