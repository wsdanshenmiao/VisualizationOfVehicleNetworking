using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ScenesManager2D : DontDestorySingleton<ScenesManager2D>
{
    [DllImport("__Internal")]
    private static extern void SendMessageToParent(bool status, int num);

    [SerializeField] private bool m_EnableSwitch = false;
    private RaycastHit2D m_HitInfo;

    public string UserID;
    public List<int> CarCounts;
    public int SceneAsync = 0;

    protected override void Awake()
    {
        base.Awake();
        UserID = null;
    }

    // Update is called once per frame
    void Update()
    {
        if (m_EnableSwitch)
            SwitchMicScenes();
    }

    private void SwitchMicScenes()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        m_HitInfo = Physics2D.Raycast(ray.origin, ray.direction);
        if (m_HitInfo.collider == null) return;
        GameObject gameObject = m_HitInfo.collider.gameObject;
        string name = gameObject.name;

        if (Input.GetMouseButtonDown(0))
        {
            switch(m_HitInfo.collider.tag){
                case "Straight":{
                    SceneAsync = 1;
                    CarCounts = GetPathCounts(gameObject);
                    name = "0";
                    break;
                }
                case "ThreePathNode":{
                    SceneAsync = 2;
                    CarCounts = GetNodeCounts(gameObject);
                    break;
                }
                case "FourPathNode":{
                    SceneAsync = 3;
                    CarCounts = GetNodeCounts(gameObject);
                    break;
                }
                default:{
                    SceneAsync = 0;
                    break;
                }
            }
            if (SceneAsync != 0) {
                SendMessageToParent(true, int.Parse(name));
                SceneManager.LoadSceneAsync(SceneAsync);
            }
        }
    }

    private List<int> GetCarCounts(Vector2 star, List<Vector2> ends)
    {
        List<int> counts = new List<int>() { 0, 0, 0, 0 };
        foreach (MainMacroscopic.car car in MainMacroscopic.Instance.cars) {
            Vector3 position = car.sphereObject.transform.position;
            position.z = 0;
            Vector3 sToc = (position - new Vector3(star.x, star.y, 0)).normalized;
            // 遍历周围的点
            for (int i = 0; i < ends.Count; ++i) {
                Vector2 end = ends[i];
                Vector3 cToe = (new Vector3(end.x, end.y, 0) - position).normalized;
                if (Vector3.Dot(sToc, cToe) >= 0.99 && (end - star).magnitude > sToc.magnitude)
                    ++counts[i];
            }
        }
        return counts;
    }

    private List<int> GetNodeCounts(GameObject gameObject)
    {
        Vector2 star = gameObject.transform.position;
        List<Vector2> ends = new();
        // 获取点击节点周围的点
        for (int i = 0; i < 4; ++i) {
            NodeData nodesData = NodesData.Instance.Nodes;
            Vector4 nearPoint = nodesData.NearNode[int.Parse(gameObject.name) - 1];
            if(nearPoint[i] != -1){
                ends.Add(nodesData.Poss[(int)nearPoint[i] - 1]);
            }
        }
        // 获取车辆数量
        return GetCarCounts(star, ends);
    }

    private List<int> GetPathCounts(GameObject gameObject)
    {
        EdgeCollider2D edgeCollider = gameObject.GetComponent<EdgeCollider2D>();
        // 获取碰撞体的端点
        List<Vector2> points = new();
        edgeCollider.GetPoints(points);
        Vector2 star = points[0];
        Vector2 end = points[1];
        Vector2 dir = (end - star).normalized;

        // 获取原始点
        star -= dir * 1.2f;
        end += dir * 1.2f;
        return GetCarCounts(star, new() { end });
    }

    public void GetUserID(string userID)
    {
        UserID = userID;
        if (userID.Length == 0)
            return;
        if (userID[0] == '0')
            m_EnableSwitch = true;
        else
            m_EnableSwitch = false;
    }

}
