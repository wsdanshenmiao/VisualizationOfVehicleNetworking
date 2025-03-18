using UnityEngine;
using System.Diagnostics;

public class PythonTest2 : MonoBehaviour
{

    string pythonScriptPath;
    void Start()
    {
        // 调用Python脚本并获取输出
        pythonScriptPath = Application.dataPath + "/Python/Tests.py";
        string output = RunPythonScript(pythonScriptPath);

        // 输出结果
        UnityEngine.Debug.Log("Python脚本输出: " + output);
    }

    void Update()
    {
        string output = RunPythonScript(pythonScriptPath);
        UnityEngine.Debug.Log("Python脚本输出: " + output);
    }

    string RunPythonScript(string scriptPath)
    {
        // 创建一个StringBuilder来收集Python脚本的输出
        System.Text.StringBuilder outputBuilder = new System.Text.StringBuilder();

        // 创建一个新的Process实例
        Process process = new Process();

        // 配置Process实例
        process.StartInfo.FileName = "python"; // Python解释器的路径，如果需要，请修改为你的Python解释器路径
        process.StartInfo.Arguments = scriptPath; // Python脚本的路径
        process.StartInfo.UseShellExecute = false; // 必须为false才能重定向IO流
        process.StartInfo.RedirectStandardOutput = true; // 重定向标准输出流
        process.StartInfo.RedirectStandardError = true; // 重定向标准错误流
        process.StartInfo.CreateNoWindow = true; // 不创建窗口

        // 订阅输出和错误事件
        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                outputBuilder.AppendLine(e.Data);
            }
        };
        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                UnityEngine.Debug.LogError("Python报错: " + e.Data);
            }
        };

        // 启动进程
        process.Start();

        // 开始异步读取输出和错误
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        // 等待进程完成
        process.WaitForExit();

        // 获取进程退出代码
        int exitCode = process.ExitCode;
        UnityEngine.Debug.Log("Python脚本退出: " + exitCode);

        // 关闭进程
        process.Close();

        // 返回Python脚本的输出
        return outputBuilder.ToString();
    }
}