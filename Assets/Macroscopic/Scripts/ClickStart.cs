using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChilkStart : MonoBehaviour
{
    private Button m_StartButton;
    private Canvas m_Canvas;

    public MainMacroscopic mainMacroscopic; 

    private void Awake()
    {
        mainMacroscopic.enabled = false;
        m_StartButton = GetComponentInChildren<Button>();
        m_Canvas = GetComponent<Canvas>();
    }

    private void Start()
    {
        m_StartButton.onClick.AddListener(StartAction);
    }

    private void StartAction()
    {
        mainMacroscopic.enabled = true;
        m_Canvas.enabled = false;
        m_StartButton.enabled = false;
    }

}
