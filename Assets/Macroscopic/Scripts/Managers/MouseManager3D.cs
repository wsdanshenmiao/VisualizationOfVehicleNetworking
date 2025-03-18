using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MouseManager : Singleton<MouseManager>
{
    public bool AutoMove = false;
    public float ZoomMaxBoundary;
    public float ZoomMinBoundary;
    public float ZoomSpeed;

    public float MouseBoundary;
    public float MoveSpeed;

    public float DraggingSensitivity;

    public RectTransform MapRT;
    public RectTransform BackgroundRT;

    private Camera m_Camera;

    private bool m_Dragging = false;
    private Vector3 m_StarMousePos;
    private Vector3 m_StarCameraPos;

    protected override void Awake()
    {
        base.Awake();
        m_Camera = GetComponent<Camera>();
    }

    private void Update()
    {
        Zoom();
        if (AutoMove)
            CameraMoveByPos();
        CameraMoveByDrag();
        Adjust();
    }

    private void Zoom()
    {
        float camaraSize = m_Camera.orthographicSize;
        // Zoom 
        m_Camera.orthographicSize -= ZoomSpeed * Input.mouseScrollDelta.y;
        // Adjust
        if (m_Camera.orthographicSize < ZoomMinBoundary)
        {
            m_Camera.orthographicSize = ZoomMinBoundary;
        }
        if (m_Camera.orthographicSize > ZoomMaxBoundary)
        {
            m_Camera.orthographicSize = ZoomMaxBoundary;
        }
    }

    private void CameraMoveByPos()
    {
        Vector2 mousePos = Input.mousePosition;
        float width = Screen.width;
        float height = Screen.height;

        if (mousePos.x < MouseBoundary)
        {
            transform.Translate(MoveSpeed * Vector2.left * Time.deltaTime);
        }
        if (mousePos.y < MouseBoundary)
        {
            transform.Translate(MoveSpeed * Vector2.down * Time.deltaTime);
        }
        if (mousePos.x > (width - MouseBoundary))
        {
            transform.Translate(MoveSpeed * Vector2.right * Time.deltaTime);
        }
        if (mousePos.y > (height - MouseBoundary))
        {
            transform.Translate(MoveSpeed * Vector2.up * Time.deltaTime);
        }
    }

    private void CameraMoveByDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            m_Dragging = true;
            m_StarMousePos = Input.mousePosition;
            m_StarCameraPos = transform.position;
        }
        if (Input.GetMouseButtonUp(0))
        {
            m_Dragging = false;
        }
        if (m_Dragging)
        {
            Vector3 currentPos = Input.mousePosition;
            Vector3 moveDir = (m_Camera.ScreenToWorldPoint(m_StarMousePos) - m_Camera.ScreenToWorldPoint(currentPos)) * DraggingSensitivity;
            Vector2 newPos = m_StarCameraPos + moveDir;

            transform.position = new Vector3(newPos.x, newPos.y, transform.position.z);
        }
    }

    private void Adjust()
    {
        if (m_Dragging)
            return;
        Vector3[] mapPos = new Vector3[4];
        Vector3[] backgroundPos = new Vector3[4];
        MapRT.GetWorldCorners(mapPos);
        BackgroundRT.GetWorldCorners(backgroundPos);

        float adjustX = Mathf.Max(0, mapPos[0].x - backgroundPos[0].x) - Mathf.Max(0, backgroundPos[3].x - mapPos[3].x);
        float adjustY = Mathf.Max(0, mapPos[0].y - backgroundPos[0].y) - Mathf.Max(0, backgroundPos[1].y - mapPos[1].y);

        transform.Translate(new Vector3(adjustX, adjustY, 0));
    }
}
