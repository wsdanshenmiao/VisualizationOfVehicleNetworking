using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SwitchMacScenes : MonoBehaviour
{
    [DllImport("__Internal")]
    private static extern void SendMessageToParent(bool status, int num);


    public Button ReturnButton;

    // Start is called before the first frame update
    void Start()
    {
        ReturnButton.onClick.AddListener(ReturnMainScene);

    }

    private void ReturnMainScene()
    {
        ScenesManager2D.Instance.SceneAsync = 0;
        SendMessageToParent(false, 0);
        SceneManager.LoadSceneAsync(0);
    }

}
