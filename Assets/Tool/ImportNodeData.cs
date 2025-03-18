using System.IO;
using UnityEngine;
using OfficeOpenXml;
using UnityEditor;
using System.Text.RegularExpressions;
using TMPro;

public class ImportNodeData : MonoBehaviour
{
    public GameObject Node;
    public GameObject Circle;

    private Shader m_Shader;

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

            for (int i = 2; i <= 39; ++i)
            {
                int num = (int)(double)worksheet.Cells[i, 1].Value;
                Vector2 pos = Vector2.zero;
                pos.x = (float)(double)worksheet.Cells[i, 2].Value;
                pos.y = (float)(double)worksheet.Cells[i, 3].Value;
                string name = worksheet.Cells[i, 4].Value.ToString();
                Vector4 nearNode = new Vector4(-1, -1, -1, -1);
                string nearPoint = worksheet.Cells[i, 5].Value.ToString();
                int count = 0;
                int index = 0;
                int length = 1;
                foreach(var c in nearPoint){
                    if (c != '、') {
                        count = count * 10 + c - '0';
                    }
                    else{
                        nearNode[index++] = count;
                        count = 0;
                    }
                    if(length == nearPoint.Length){
                        nearNode[index++] = count;
                    }
                    ++length;
                }
                // NodeData nodes = NodesData.Instance.Nodes;
                // nodes.Nums.Add(num);
                // nodes.Poss.Add(pos);
                // nodes.Names.Add(name);
                // nodes.NearNode.Add(nearNode);

                GameObject gameObject = Instantiate(Circle);
                gameObject.name = num.ToString();

                gameObject.transform.position = new Vector3(pos.x, pos.y, 0);
                gameObject.transform.localScale = new Vector3(1, 1, 1);

                switch (Regex.Matches(nearPoint, "、").Count)
                {
                    case 1: gameObject.tag = "TwoPathNode"; break;
                    case 2: gameObject.tag = "ThreePathNode"; break;
                    case 3: gameObject.tag = "FourPathNode"; break;
                }

                Transform transform = gameObject.GetComponentInChildren<Transform>();
                TextMeshProUGUI text = transform.GetComponentInChildren<TextMeshProUGUI>();
                text.text = name;

                gameObject.transform.SetParent(Node.transform);
            }
        }
#if UNITY_EDITOR
        PrefabUtility.SaveAsPrefabAssetAndConnect(Node, "Assets/Prefabs/" + Node.name + ".prefab", InteractionMode.UserAction);
#endif
    }

}
