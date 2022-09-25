using UnityEngine;

public class UserManager : Singleton<UserManager>
{
    [HideInInspector] public string userName;
    [HideInInspector] public Texture profileTexture;
    public bool isInitialized;
}
