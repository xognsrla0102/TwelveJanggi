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
                        $"�̱��� {typeof(T)} ��ü�� �����ϴ�.\n" +
                        $"InitScene�� ��ü�� �ִ��� Ȯ���ϼ���."
                        );
                }
            }
            return instance;
        }
    }
}
