public struct SSceneName
{
    public const string INIT_SCENE = "InitScene";
    public const string MAIN_SCENE = "MainScene";
    public const string INGAME_SCENE = "IngameScene";    
}

public struct SPrefsKey
{
    public const string BGM_VOLUME = "BgmVolume";
    public const string SOUND_VOLUME = "SoundVolume";

    public const string IS_BGM_MUTE = "IsBgmMute";
    public const string IS_SFX_MUTE = "IsSfxMute";
}

public struct SBgmName
{
    public const string MAIN_BGM = "Main";
    public const string INGAME_BGM = "Ingame";
}

public struct SSfxName
{
    public const string BUTTON_OVER_SFX = "BtnOver";
    public const string BUTTON_CLICK_SFX = "BtnClick";
}

public struct SRoomPropertyKey
{
    public const string MASTER_CLIENT = "MasterClient";
    public const string ROOM_STATE = "RoomState";
}

// 방에 참가한 유저에 대한 속성 키들
public struct SPlayerPropertyKey
{
}

public struct SResourceLoadPath
{
    public const string IMAGE = "Images/";
    public const string PREFAB = "Prefabs/";
}