using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    public static string nextSceneName = SSceneName.MAIN_SCENE;

    [SerializeField] private GameObject[] dontdestroyObjs;

    private void Start()
    {        
        foreach (var obj in dontdestroyObjs)
        {
            DontDestroyOnLoad(obj);
        }

        StartCoroutine(WaitForInitCoroutine());
    }

    private IEnumerator WaitForInitCoroutine()
    {
        // donDestroy������Ʈ �ʱ�ȭ 2 �����ӵ��� ���
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        NetworkManager.Instance.ConnectMasterServer();
    }
}
