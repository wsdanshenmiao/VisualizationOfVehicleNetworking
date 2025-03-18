using OfficeOpenXml;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ImportRoadData : MonoBehaviour
{
    public GameObject Road;
    public GameObject Prefab;

    private Shader m_Shader;
    private Dictionary<Vector4, GameObject> m_Roads = new Dictionary<Vector4, GameObject>();

    private void Awake()
    {
        m_Shader = Shader.Find("Traffic/ChangeColorByWeight");
    }

    // Start is called before the first frame update
    void Start()
    {
        string fileName = Application.dataPath + "/NodeData/node_data.xlsx";
        FileInfo fileInfo = new FileInfo(fileName);

        using (ExcelPackage package = new ExcelPackage(fileInfo))
        {
            ExcelWorksheet worksheet = package.Workbook.Worksheets[1];

            int count = 0;
            for (int i = 2; i <= 39; ++i)
            {
                double x1 = (double)worksheet.Cells[i, 2].Value;
                double y1 = (double)worksheet.Cells[i, 3].Value;
                string nearPoint = worksheet.Cells[i, 5].Value.ToString();

                char[] numChar = new char[2];
                int k = 0;

                for (int j = 0; j <= nearPoint.Length; ++j)
                {
                    if (j != nearPoint.Length && nearPoint[j] != '、')
                    {
                        numChar[k++] = nearPoint[j];
                    }
                    else
                    {
                        k = 0;
                        string s = new string(numChar);
                        numChar = new char[2];
                        int num = int.Parse(s);
                        double x2 = (double)worksheet.Cells[num + 1, 2].Value;
                        double y2 = (double)worksheet.Cells[num + 1, 3].Value;

                        Vector4 key1 = new Vector4((float)x1, (float)y1, (float)x2, (float)y2);
                        Vector4 key2 = new Vector4((float)x2, (float)y2, (float)x1, (float)y1);
                        if (!m_Roads.ContainsKey(key1) && !m_Roads.ContainsKey(key2))
                        {
                            GameObject newRoad = Instantiate(Prefab);
                            newRoad.name = "Road" + (count++);
                            newRoad.tag = "Straight";
                            LineRenderer roadRenderer = newRoad.GetComponent<LineRenderer>();
                            roadRenderer.positionCount = 2;
                            roadRenderer.SetPosition(0, new Vector3((float)x1, (float)y1, 0));
                            roadRenderer.SetPosition(1, new Vector3((float)x2, (float)y2, 0));
                            roadRenderer.startWidth = 0.3f;
                            roadRenderer.endWidth = 0.3f;
                            roadRenderer.alignment = LineAlignment.TransformZ;
                            roadRenderer.useWorldSpace = true;
                            roadRenderer.sortingLayerName = "TrafficRoad";

                            EdgeCollider2D edgeCollider = newRoad.GetComponent<EdgeCollider2D>();
                            Vector2 star = new Vector2((float)x1, (float)y1);
                            Vector2 end = new Vector2((float)x2, (float)y2);
                            Vector2 dir = (end - star).normalized;
                            List<Vector2> points = new List<Vector2>();
                            points.Add(star + dir * 1.2f);
                            points.Add(end - dir * 1.2f);
                            edgeCollider.SetPoints(points);
                            edgeCollider.edgeRadius = 0.15f;

                            m_Roads.Add(key1, newRoad);
                        }
                    }

                }
            }
        }

        foreach (var road in m_Roads.Values)
        {
            road.transform.SetParent(Road.transform);
        }
#if UNITY_EDITOR

        PrefabUtility.SaveAsPrefabAssetAndConnect(Road, "Assets/Prefabs/" + Road.name + ".prefab", InteractionMode.UserAction);
#endif
    }
}
