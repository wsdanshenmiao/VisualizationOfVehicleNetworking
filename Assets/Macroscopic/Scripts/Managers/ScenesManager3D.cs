using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Runtime.InteropServices;

// ÆúÓÃ
public class ScenesManager3D : Singleton<ScenesManager3D>
{
    [DllImport("__Internal")]
    private static extern void SendMessageToParent(bool status);

    private RaycastHit m_HitInfo;
    private bool m_EnableSwitch = false;

    // Update is called once per frame
    void Update()
    {
        if(m_EnableSwitch)
            SwitchMicScenes();
    }


    private void SwitchMicScenes()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out m_HitInfo);

        if (Input.GetMouseButtonDown(0) && m_HitInfo.collider)
        {
            if (m_HitInfo.collider.CompareTag("TwoPathNode"))
            {
                SceneManager.LoadSceneAsync(1);
            }
            else if (m_HitInfo.collider.CompareTag("ThreePathNode"))
            {
                SceneManager.LoadSceneAsync(2);
            }
            else if (m_HitInfo.collider.CompareTag("FourPathNode"))
            {
                SceneManager.LoadSceneAsync(3);
            }
        }
    }

    public void GetUserID(string UserID)
    {
        if (UserID.Length == 0)
            return;
        if (UserID[0] == '0')
            m_EnableSwitch = true;
        else
            m_EnableSwitch = false;
    }
}
