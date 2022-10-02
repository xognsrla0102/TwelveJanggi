using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InitScene : MonoBehaviour
{
    public static string nextSceneName = SSceneName.MAIN_SCENE;

    [SerializeField] private GameObject[] dontdestroyObjs;

    private void Start()
    {
#if UNITY_ANDROID
        Application.targetFrameRate = 60;
#endif

        foreach (var obj in dontdestroyObjs)
        {
            DontDestroyOnLoad(obj);
        }

        StartCoroutine(WaitForInitCoroutine());
    }

    private IEnumerator WaitForInitCoroutine()
    {
        // donDestroy오브젝트 초기화 2 프레임동안 대기
        for (int i = 0; i < 2; i++)
        {
            yield return new WaitForEndOfFrame();
        }

        NetworkManager.Instance.ConnectMasterServer();
    }
}
