using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Node Data", menuName = "Node")]

public class NodeData : ScriptableObject
{
    public List<int> Nums = new();
    public List<Vector2> Poss = new();
    public List<string> Names = new();
    public List<Vector4> NearNode = new List<Vector4>();

}
