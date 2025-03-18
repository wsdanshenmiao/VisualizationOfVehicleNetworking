using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PythonTest : MonoBehaviour
{
    void Start()
    {
        StartCoroutine(RunPythonScript());
    }

    IEnumerator RunPythonScript()
    {
        string pythonPath = "python"; // 或者指定Python解释器的完整路径
        string scriptPath = Application.dataPath + "/Scripts/your_script.py"; // 你的Python脚本路径

        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = pythonPath;
        start.Arguments = scriptPath;
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        start.CreateNoWindow = true;

        using (Process process = Process.Start(start))
        {
            // 读取标准输出流
            string jsonOutput = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            // 解析JSON输出
            List<Vector3> vectors = ParseJsonOutput(jsonOutput);

            // 使用解析后的Vector3数组
            foreach (var vector in vectors)
            {
                UnityEngine.Debug.Log($"Vector3: {vector}");
            }
        }

        yield return null;
    }

    List<Vector3> ParseJsonOutput(string jsonOutput)
    {
        // 定义一个类来匹配JSON结构
        var jsonData = JsonUtility.FromJson<Vector3ArrayWrapper>(jsonOutput);

        List<Vector3> vectors = new List<Vector3>();
        foreach (var data in jsonData.vectors)
        {
            vectors.Add(new Vector3(data.x, data.y, data.z));
        }

        return vectors;
    }

    // 定义一个类来匹配JSON结构
    [System.Serializable]
    class Vector3Data
    {
        public float x;
        public float y;
        public float z;
    }

    [System.Serializable]
    class Vector3ArrayWrapper
    {
        public List<Vector3Data> vectors;
    }
}