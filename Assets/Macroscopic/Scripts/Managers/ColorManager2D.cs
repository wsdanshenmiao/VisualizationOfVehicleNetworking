using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;
using UnityEngine.Networking;


public class ColorManager2D : Singleton<ColorManager2D>
{
    [System.Serializable]
    public struct NodeWeight
    {
        public Vector3[] PosWeight;
    }

    private struct NodeData
    {
        public SpriteRenderer NodeRenderer;
        public Vector2 Pos;
    }

    public GameObject RoadNodes;
    public GameObject Roads;
    public Texture2D RoadTexture;

    //private string m_FilePath = Application.dataPath + "/Macroscopic/Scripts/TestData/node_json.json";
    private string m_Address = "http://8.138.170.116:8080/macroscopic/get_weights";
    private string m_Json;

    private bool m_Initialized = false;

    private float m_RequestTime = 0.5f;
    private float m_CurrentRequestTime = 0;

    private NodeWeight m_PosWeights;
    private List<NodeData> m_NodeDataList = new();
    private List<LineRenderer> m_RoadDataList = new();

    private void Start()
    {
        StartCoroutine(WaitToInit());
    }

    private void Update()
    {
        //Debug.Log(m_Initialized);
        if (!m_Initialized)
            return;

        //隔一段时间接收新的节点权重
        m_CurrentRequestTime += Time.deltaTime;
        if (m_CurrentRequestTime < m_RequestTime)
            return;
        else
            m_CurrentRequestTime = 0;

        StartCoroutine(UpdateWeight());
    }

    /// <summary>
    /// ��ʼ����·
    /// </summary>
    /// <param name="shader"></param> ��·ʹ�õ�Shader
    private void InitRoad(Shader shader)
    {
        List<Transform> childRoad = new();
        Roads.GetComponentsInChildren<Transform>(childRoad);
        foreach (Transform t in childRoad)
        {
            if (t.name != Roads.name)
            {
                LineRenderer roadRenderer = t.GetComponent<LineRenderer>();
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
        List<SpriteRenderer> childNode = new();
        RoadNodes.GetComponentsInChildren<SpriteRenderer>(childNode);

        foreach (SpriteRenderer t in childNode)
        {
            if (t.name != RoadNodes.name)
            {
                NodeData data = new NodeData();
                data.NodeRenderer = t;
                data.NodeRenderer.material = new Material(shader);
                data.NodeRenderer.material.SetTexture("_ColorTex", RoadTexture);
                Vector3 posWeight = Array.Find<Vector3>(m_PosWeights.PosWeight, data =>
                {
                    return data.x == t.transform.position.x && data.y == t.transform.position.y;
                });
                if (posWeight != null && posWeight.z != float.NaN)
                    data.Pos = new Vector2(posWeight.x, posWeight.y);
                else
                    m_Initialized = false;

                m_NodeDataList.Add(data);
            }
        }
    }

    /// <summary>
    /// ���µ�·��ɫ
    /// </summary>
    private void UpdateRoadColor(float minWeight, float difference)
    {
        foreach (LineRenderer renderer in m_RoadDataList)
        {
            Vector3 pos = renderer.GetPosition(0);
            Vector4 pos1 = new Vector4(pos.x, pos.y, pos.z, 0);
            pos = renderer.GetPosition(1);
            Vector4 pos2 = new Vector4(pos.x, pos.y, pos.z, 0);

            float w1 = Array.Find<Vector3>(m_PosWeights.PosWeight, data =>
            {
                return data.x == pos1.x && data.y == pos1.y;
            }).z;
            float w2 = Array.Find<Vector3>(m_PosWeights.PosWeight, data =>
            {
                return data.x == pos2.x && data.y == pos2.y;
            }).z;
            if (w1 != float.NaN && w2 != float.NaN)
            {
                w1 = (w1 - minWeight) / difference;
                w2 = (w2 - minWeight) / difference;
                UpdateMaterial(renderer.material, w1, w2, pos1, pos2);
            }
        }
    }

    /// <summary>
    /// ���½ڵ�Ȩ������ɫ
    /// </summary>
    private void UpdateNodeWeight(float minWeight, float difference)
    {
        //����ȡ��Ȩ�ظ����ڵ㲢���²���
        foreach (Vector3 posWeight in m_PosWeights.PosWeight)
        {
            NodeData nodeData = m_NodeDataList.Find(data =>
            {
                return V3EqualsV2(data.Pos, posWeight);
            });
            if (nodeData.Pos != null && nodeData.NodeRenderer != null)
            {
                float weight = (posWeight.z - minWeight) / difference;
                Vector4 pos = new Vector4(nodeData.Pos.x, nodeData.Pos.y, 0, 0);
                UpdateMaterial(nodeData.NodeRenderer.material, weight, weight, pos, pos);
            }
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
    IEnumerator UpdateWeight()
    {
        UnityWebRequest webRequest = UnityWebRequest.Get(m_Address);

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
            m_PosWeights = JsonUtility.FromJson<NodeWeight>(m_Json);
            if (m_PosWeights.PosWeight != null)
            {
                bool update = true;
                foreach (Vector3 posWeight in m_PosWeights.PosWeight)
                {
                    if (posWeight == null || posWeight.z == float.NaN)
                    {
                        update = false;
                        break;
                    }

                }
                if (update)
                {
                    Array.Sort<Vector3>(m_PosWeights.PosWeight, (a, b) => { return (int)(a.z - b.z); });
                    float minWeight = m_PosWeights.PosWeight.First().z;
                    float maxWeight = m_PosWeights.PosWeight.Last().z;
                    float difference = maxWeight - minWeight;

                    // 根据收到的权重更新节点颜色
                    UpdateNodeWeight(minWeight, difference);
                    UpdateRoadColor(minWeight, difference);
                }
            }
        }
    }

    /// <summary>
    /// 等待接收数据以初始化节点与车辆
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitToInit()
    {

        yield return new WaitForSeconds(2.0f);

        UnityWebRequest webRequest = UnityWebRequest.Get(m_Address);

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
            m_PosWeights = JsonUtility.FromJson<NodeWeight>(m_Json);

            if (m_PosWeights.PosWeight != null)
            {
                bool init = true;
                foreach (var posWeight in m_PosWeights.PosWeight)
                {
                    if (posWeight == null || posWeight.z == float.NaN)
                    {
                        init = false;
                        break;
                    }
                }
                if (init)
                {
                    m_Initialized = true;
                    Shader shader = Shader.Find("Traffic/ChangeColorByWeight");
                    InitNode(shader);
                    InitRoad(shader);
                }
            }
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
