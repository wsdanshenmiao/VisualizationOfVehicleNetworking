using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;   // 用于文件调用
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Threading;

public class MainMacroscopic : DontDestorySingleton<MainMacroscopic>
{
    private bool isPutSucceed = false;  // 标志需求是否成功
    private Sprite carSprite;
    public Texture2D carTexture;  // 车所用的贴图
    //string[] json;

    int quantity = 50;  // 车的数量
    //string json;     // 用来临时存储json文件
    string GetServeUrl = "http://8.138.170.116:8080/macroscopic/get_path"; 
    string PutServeUrl = "http://8.138.170.116:8080/macroscopic/put_car?car_num=30";
    public Vector3 sphereScale = new Vector3(1f, 1f, 1f); // 设置球体缩放大小

    //public float testSpeed = 1f;
    //public Vector3[] testPath ={
    //new Vector3(4f,4f,-3f),new Vector3(4f,16f,-3f),new Vector3(16f,14f,-3f)
    //};



    public car[] cars;

    void Start()
    {
        carSprite = Sprite.Create(carTexture, new Rect(0, 0, carTexture.width, carTexture.height), new Vector2(0.5f, 0.5f));
        StartCoroutine(SendPutRequest());    // 向服务器发送车辆的数量

        //StartCoroutine(CheckRequestStatus());
        //InvokeRepeating("Check",0.0f,1.0f);



        //string[] filePath = new string[quantity];
        //filePath[0] = Application.dataPath + "/Macroscopic/Scripts/cars_data2.json"; // JSON文件路径
        //filePath[1] = Application.dataPath + "/Macroscopic/Scripts/cars_data3.json"; // JSON文件路径

        //json = new string[quantity];

        // 初始化各个车辆
        cars = new car[quantity];
        for (int index = 0; index < quantity; index++)
        {
            int i = index;
            cars[i] = new car();
            //cars[i].initialSphere(SphereMaterial, sphereScale, CarModel);
            cars[i].initialCar(sphereScale,carSprite);

            //cars[i].TestRefresh(filePath[i]);
            //cars[i].sphereObject.transform.position = cars[i].fromAI.path[0];

            // 设为子类
            cars[i].sphereObject.transform.SetParent(transform, false);
//
        }


    }

    void FixedUpdate()
    {
        /*
        if (ScenesManager2D.Instance.UserID == null)
            return;
        else if (ScenesManager2D.Instance.UserID[0] != '0')
            return;
        */


        //Thread blockingThread = new Thread(Check);
        //blockingThread.Start();
        if (!isPutSucceed)
        {
            ;
        }
        else
        {
            //Debug.Log("启动！！！！！！！！！！！！！！！！！！！！！！");
            //foreach (var car in cars)
            for (int i = 0; i < quantity; i++)
            {
                if (cars[i].isMoving == true)
                {
                    cars[i].Move();
                }
                else
                {
                    //Debug.Log("进入循环");
                    //StartCoroutine(GetRequestToServer(() => cars[i].json, newValue => cars[i].json = newValue));
                    int index = i; // 使用局部变量来保存 i 的值
                    StartCoroutine(GetRequestToServer((value) => cars[index].json = value,
                    (value) => cars[index].getNewPathSucceed = value));
                    //StartCoroutine(WaitAndPrint(0.02f));
                    //Debug.Log(cars[i].json);
                    if (cars[index].getNewPathSucceed)
                    {
                        cars[index].refresh();
                    }

                }
            }
        }


        RenderCar();
    }



    private void RenderCar()
    {
        bool render = ScenesManager2D.Instance.SceneAsync == 0;
        SpriteRenderer[] childSpriteRenderer = GetComponentsInChildren<SpriteRenderer>();
        foreach (var child in childSpriteRenderer)
        {
            child.enabled = render;
        }
    }

    public IEnumerator GetRequestToServer(System.Action<string> setString = null, System.Action<bool> setBool = null)
    //public IEnumerator GetRequestToServer(System.Func<string> getValue, System.Action<string> setValue = null)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(GetServeUrl))
        {
            // 请求超时时间设置，这里设置为 10 秒
            webRequest.timeout = 6000;
            yield return new WaitForSeconds(0.2f);
            // 发送请求并等待响应
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.Log("错误: " + webRequest.error);
                //Debug.Log("错误: ");
            }
            else
            {
                // 请求成功，处理响应数据
                //Debug.Log("回复: " + webRequest.downloadHandler.text);
                // 使用 JsonUtility 在这里解析 JSON
                string temp = webRequest.downloadHandler.text;
                setString(temp);
                setBool(true);

            }

        }
    }

    public class car
    {
        public transmitData fromAI;     // 从AI组中获取到的信息
        public GameObject sphereObject;   // 表示车辆的一个物体
        public int currentWaypointIndex = 1; // 引用索引
        public bool isMoving = false;          // 标志是否在移动
        public bool getNewPathSucceed = false;
        public string json;   // 用来保存json数据
        //public bool GetSucceed = false;

        //public car(){
        //this.initialSphere(SphereMaterial,sphereScale);
        //}

        public void initialCar(Vector3 sphereScale,Sprite carSprite)
        {
            // 创建球体对象
            sphereObject = new GameObject();
            sphereObject.transform.position = new Vector3(-10000f, -10000f, 0);

            // 获取球体的渲染器组件
            SpriteRenderer rend = sphereObject.AddComponent<SpriteRenderer>();
            // 应用材质到球体

            rend.sortingLayerName = "Car";
            rend.sprite = carSprite;

            // 设置球体的缩放
            sphereObject.transform.localScale = sphereScale;
        }




        public void Move()
        {
            // 检查是否还有目标点
            if (currentWaypointIndex < fromAI.path.Length)
            {

                float distance = Vector3.Distance(sphereObject.transform.position, fromAI.path[currentWaypointIndex]);
                float duration = distance / fromAI.speed;
                sphereObject.transform.position = Vector3.Lerp(sphereObject.transform.position, fromAI.path[currentWaypointIndex], Time.deltaTime / duration);

                // 检查是否接近当前目标点，如果接近则切换到下一个目标点
                if (Vector3.Distance(sphereObject.transform.position, fromAI.path[currentWaypointIndex]) < 0.0001f)
                {
                    
                    Vector3 direction = Vector3.zero;
                    if(currentWaypointIndex + 1 < fromAI.path.Length){
                        direction = (fromAI.path[currentWaypointIndex + 1] - fromAI.path[currentWaypointIndex]).normalized;
                    }
                    
                    Quaternion rotation = Quaternion.LookRotation(direction);

                    // 提取 Z 轴的旋转角度
                    float zAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // 创建一个新的旋转角度，只改变 Z 轴，X 和 Y 轴为 0
                    //Vector3 newRotation = new Vector3(0, 0, zAngle);
                    //sphereObject.transform.rotation = Quaternion.LookRotation(newRotation);
                    sphereObject.transform.rotation = Quaternion.Euler(0, 0, zAngle - 90f);
                    //sphereObject.transform.eulerAngles = newRotation;
                    currentWaypointIndex++;
                }
            }
            else
            {
                // 如果已经到达路径末尾，可以执行额外的逻辑或者停止移动
                Debug.Log("已经到达路径末尾");
                isMoving = false;
                // 这里可以根据需要重置
            }
        }

        // 测试使用的更新函数
        public void TestRefresh(string path)
        {

            //string path = Application.dataPath + "/Macroscopic/Scripts/TestData/cars_data2.json"; // JSON文件路径
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                this.fromAI = JsonUtility.FromJson<transmitData>(json);
                Debug.Log("路径点1:" + fromAI.path[0] + "路径点2:" + fromAI.path[1] + "速度为:" + fromAI.speed);
            }
            else
            {
                Debug.LogError("Json没找到: " + path);
            }

        }

        public void refresh()
        {

            fromAI = JsonUtility.FromJson<transmitData>(json);      // 获得新的速度和路径
            sphereObject.transform.position = fromAI.path[0];       // 设置为起点位置
            currentWaypointIndex = 1;
            Vector3 direction = (fromAI.path[currentWaypointIndex] - fromAI.path[currentWaypointIndex - 1]).normalized;

                if (direction != Vector3.zero)
                {
                    Quaternion rotation = Quaternion.LookRotation(direction);

                    // 提取 Z 轴的旋转角度
                    float zAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    // 创建一个新的旋转角度，只改变 Z 轴，X 和 Y 轴为 0
                    //Vector3 newRotation = new Vector3(0, 0, zAngle);
                    //sphereObject.transform.rotation = Quaternion.LookRotation(newRotation);
                    sphereObject.transform.rotation = Quaternion.Euler(0, 0, zAngle - 90f);
                }
            isMoving = true;
            getNewPathSucceed = false;
        }

    }

    // 从AI组拿到的数据
    public struct transmitData
    {
        public float speed;
        public Vector3[] path;
    }

    // 向服务器发送车辆数量
    IEnumerator SendPutRequest()
    {
        UnityWebRequest www = UnityWebRequest.Put(PutServeUrl, "");
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("请求失败: " + www.error);
        }
        else
        {
            Debug.Log("请求成功，回复： " + www.downloadHandler.text);

            // 检查服务器返回的文本是否为 "succeed"
            //if (www.downloadHandler.text == "["put succeed"]")
            //{
            isPutSucceed = true;
            Debug.Log("收到 'put succeed'，继续执行");
            //}
            //else
            //{
            //Debug.Log("未收到 'succeed'，暂停执行");
            //}
        }

    }

    IEnumerator WaitAndPrint(float waitTime)
    {
        // 等待 waitTime 秒
        yield return new WaitForSeconds(waitTime);

    }

    IEnumerator CheckRequestStatus()
    {
        while (!isPutSucceed)
        {
            Debug.Log("等待回复.ing");
            yield return null; // 每帧检查一次
        }

        // 在这里继续执行后续代码
        Debug.Log("继续执行后续代码");
    }

    void Check()
    {
        while (!isPutSucceed)
        {
            Debug.Log("等待回复.ing");

            // 阻塞线程1秒
            Thread.Sleep(1000);
        }
    }

}
