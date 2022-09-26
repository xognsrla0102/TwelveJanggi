using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    public bool isInitialized;

    [HideInInspector] public string userName;
    [HideInInspector] public string profileImageUrl;
    [HideInInspector] public Texture profileTexture;
}
