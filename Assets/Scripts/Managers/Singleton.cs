using UnityEngine;
using Photon.Pun;

public class Singleton<T> : MonoBehaviourPunCallbacks where T : MonoBehaviourPunCallbacks
{
    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                var obj = FindObjectOfType<T>();
                if (obj != null)
                {
                    instance = obj;
                }
                else
                {
                    Debug.Assert(
                        false,
                        $"싱글턴 {typeof(T)} 개체가 없습니다.\n" +
                        $"InitScene에 개체가 있는지 확인하세요."
                        );
                }
            }
            return instance;
        }
    }
}
