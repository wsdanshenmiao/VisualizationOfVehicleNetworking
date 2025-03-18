using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;

/*
// 弃用
public class ColorManager3D : Singleton<ColorManager3D>
{
    [System.Serializable]
    public struct NodeWeight
    {
        public Vector3[] PosWeight;
    }

    public struct NodeData
    {
        public Renderer NodeRenderer;
        public Vector2 Pos;
    }

    public GameObject RoadNodes;
    public GameObject Roads;
    public Texture2D RoadTexture;

    private string m_FilePath = Application.dataPath + "/Macroscopic/Scripts/TestData/node_json.json";
    private string m_IPAddress = "http://139.159.156.117:8080/get_position";
    private string m_Json;

    private NativeWebSocket.WebSocket m_WebSocket;

    private NodeWeight m_PosWeights;
    private List<NodeData> m_NodeDataList = new();
    private List<Renderer> m_RoadDataList = new();

    private void Start()
    {
        //StartCoroutine(GetRequestFromServer());
        //m_PosWeights = JsonUtility.FromJson<NodeWeight>(m_Json); ;
        
        m_PosWeights = ReadFromJson(m_FilePath);

        Shader shader = Shader.Find("Traffic/ChangeColorByWeight");
        InitNode(shader);
        InitRoad(shader);
    }

    private void Update()
    {
        //隔一段时间接收新的节点权重
        //StartCoroutine(GetRequestFromServer());
        //m_PosWeights = JsonUtility.FromJson<NodeWeight>(m_Json); ;

        m_PosWeights = ReadFromJson(m_FilePath);

        // ����ڵ��ҵ������Сֵ
        Array.Sort<Vector3>(m_PosWeights.PosWeight, (a, b) => { return (int)(a.z - b.z); });
        float minWeight = m_PosWeights.PosWeight.First().z;
        float maxWeight = m_PosWeights.PosWeight.Last().z;
        float difference = maxWeight - minWeight;

        //����Shader�е�Ȩ��
        UpdateNodeWeight(minWeight, difference);
        UpdateRoadColor(minWeight, difference);
    }

    /// <summary>
    /// ��ʼ����·
    /// </summary>
    /// <param name="shader"></param> ��·ʹ�õ�Shader
    private void InitRoad(Shader shader)
    {
        List<Transform> childRoad = new();
        Roads.GetComponentsInChildren<Transform>(childRoad);
        foreach(Transform t in childRoad)
        {
            if(t.name != "Road")
            {
                Renderer roadRenderer = t.GetComponent<Renderer>();
                roadRenderer.material = new Material(shader);
                roadRenderer.material.SetTexture("_ColorTex", RoadTexture);

                m_RoadDataList.Add(roadRenderer);
            }
        }
    }

    /// <summary>
    /// ��ʼ���ڵ�
    /// </summary>
    /// <param name="shader"></param> �ڵ��Shader
    private void InitNode(Shader shader)
    {
        List<Transform> childNode = new();
        RoadNodes.GetComponentsInChildren<Transform>(childNode);

        foreach (Vector3 node in m_PosWeights.PosWeight)
        {
            NodeData nodeData = new NodeData();
            Transform nodeTransform = childNode.Find(pos =>
            {
                return pos.position.x.Equals(node.x) && pos.position.y.Equals(node.y);
            });
            nodeData.NodeRenderer = nodeTransform.GetComponent<Renderer>();
            nodeData.NodeRenderer.material = new Material(shader);
            nodeData.NodeRenderer.material.SetTexture("_ColorTex", RoadTexture);
            nodeData.Pos = new Vector2(node.x, node.y);

            m_NodeDataList.Add(nodeData);
        }
    }

    /// <summary>
    /// ���µ�·��ɫ
    /// </summary>
    private void UpdateRoadColor(float minWeight, float difference)
    {
        foreach (Renderer renderer in m_RoadDataList)
        {
            NodePos nodePos = renderer.gameObject.GetComponent<NodePos>();
            Vector3 node1 = nodePos.Node1.transform.position;
            Vector3 node2 = nodePos.Node2.transform.position;
            Vector4 pos1 = node1;
            Vector4 pos2 = node2;

            float w1 = Array.Find<Vector3>(m_PosWeights.PosWeight, data =>
            {
                return data.x == node1.x && data.y == node1.y;
            }).z;
            float w2 = Array.Find<Vector3>(m_PosWeights.PosWeight, data =>
            {
                return data.x == node2.x && data.y == node2.y;
            }).z;
            w1 = (w1 - minWeight) / difference;
            w2 = (w2 - minWeight) / difference;

            UpdateMaterial(renderer.material, w1, w2, pos1, pos2);
        }
    }

    /// <summary>
    /// ���½ڵ�Ȩ������ɫ
    /// </summary>
    private void UpdateNodeWeight(float minWeight, float difference)
    {
        //����ȡ��Ȩ�ظ����ڵ㲢���²���
        foreach(Vector3 posWeight in m_PosWeights.PosWeight)
        {
            NodeData nodeData = m_NodeDataList.Find(data =>
            {
                return V3EqualsV2(data.Pos, posWeight);
            });

            float weight = (posWeight.z - minWeight) / difference;
            Vector4 pos = nodeData.Pos;
            UpdateMaterial(nodeData.NodeRenderer.material, weight, weight, pos, pos);
        }
    }

    /// <summary>
    /// ���²���
    /// </summary>
    /// <param name="material"></param> Ҫ���µĲ���
    /// <param name="w1"></param> Ȩ��1
    /// <param name="w2"></param> Ȩ��2 
    /// <param name="pos1"></param> 
    /// <param name="pos2"></param>
    private void UpdateMaterial(Material material, float w1, float w2, Vector4 pos1, Vector4 pos2)
    {
        material.SetFloat("_NodeWeight1", w1);
        material.SetFloat("_NodeWeight2", w2);
        material.SetVector("_NodePos1", pos1);
        material.SetVector("_NodePos2", pos2);
    }

    /// <summary>
    /// 向服务器发送请求并读取数据
    /// </summary>
    /// <returns></returns>
    private IEnumerator GetRequestFromServer()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(m_IPAddress);
        webRequest.timeout = 10;

        // 发送请求并等待响应
        yield return webRequest.SendWebRequest();

        if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.LogError("错误: " + webRequest.error);
        }
        else
        {
            // 请求成功，处理响应数据
            m_Json = webRequest.downloadHandler.text;
        }
    }

    /// <summary>
    /// ��Json�ļ���ȡ����
    /// </summary>
    private NodeWeight ReadFromJson(string filePath)
    {
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<NodeWeight>(json);
        }
        else
        {
            throw new Exception("δ�ҵ�JSON�ļ���·����" + filePath);
        }

    }
    
    
    private bool V3EqualsV2(Vector2 two, Vector3 three)
    {
        return two.x.Equals(three.x) && two.y.Equals(three.y);
    }
}

*/