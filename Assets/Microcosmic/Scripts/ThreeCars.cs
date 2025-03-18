using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ThreeCars : MonoBehaviour
{
    public GameObject car_1;

    private int num1 = 0;    // 第一波车的数量
    //private status getNum1Status = status.none;   // 标志num1的状态
    private int road1 = 0;  // 道路1的车数量
    private int road2 = 0;  // 道路2的车数量
    private int road3 = 0;  // 道路3的车数量
    private int road4 = 0;  // 道路4的车数量
    private Vector4 test;
    private int num2 = 0;    // 第二波车的数量   
    //private status getNum2Status = status.none;   // 标志num1的状态
    private car[] CarsManager1;   // 用来储存第一波车辆的数组
    //private status wave1 = status.none;         // 第一波的状态
    private bool wave1 = false;         // 第一波的状态
    private car[] CarsManager2;   // 用来储存第二波车辆的数组
    //private status wave2 = status.none;         // 第二波的状态
    private bool wave2 = true;         // 第二波的状态
    private string PutServeUrl = "http://8.138.121.2:8080/put_sub_position";   // 存放URL
    //private bool isPutSucceed = false;  // 标志需求是否成功
    private string GetPathServeUrl = "http://8.138.121.2:8080/get_sub_position";     // 拿路径的URL
    private string GetCarNumServeUrl = "http://8.138.121.2:8080/get_sub_num";
    private Vector2 carNum = Vector2.zero;    // 存储两波车的数量
    private bool getNumSucceed = false;  // 获取两波车数量的状态

    void Awake(){
        //Debug.Log("Car_Num = " + Car_Num);
        //Car_Num = ScenesManager2D.Instance.CarCount;
        //Debug.Log("Car_Num = " + Car_Num);
        road1 = ScenesManager2D.Instance.CarCounts[0];
        road2 = ScenesManager2D.Instance.CarCounts[1];
        road3 = ScenesManager2D.Instance.CarCounts[2];
        road4 = ScenesManager2D.Instance.CarCounts[3];
        Debug.Log("获取到的车辆数量分别为:" + road1 + "  " + road2 + "  " + road2 + "  " + road2 + "  ");
    }

    void Start()
    {

        StartCoroutine(SendPutRequest());
        //while(!getNumSucceed){
        //Debug.Log("等待回复");
        //}
        //Debug.Log("已发送信息");        

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        if (!getNumSucceed)
        {
            ;
        }
        else
        {
            if (!wave1)
            {
                Debug.Log("进入第一波");
                for (int i = 0; i < num1; i++)
                {

                    int index = i;     // 存储索引，防止更新太快变动
                    if (CarsManager1[index].isMoving == false)
                    {
                        if (CarsManager1[index].pathStatus == status.none)
                        {
                            StartCoroutine(GetPathToServer((value) => CarsManager1[index].json = value, (value) => CarsManager1[index].pathStatus = value));
                        }
                        else if (CarsManager1[index].pathStatus == status.getSucceed)
                        {
                            CarsManager1[index].refresh();
                            CarsManager1[index].pathStatus = status.none;
                            CarsManager1[index].openRenderer();
                        }
                    }
                    else
                    {
                        CarsManager1[index].Move();

                        // 记得在下一次波次1之前改isMoving为false
                    }
                }

                wave1 = checkOver(CarsManager1);   // 若全部跑完则改变wave1为true
                if (wave1)
                {

                    for (int i = 0; i < num1; i++)
                    {
                        // 关闭第一波车的渲染
                        CarsManager1[i].closeRenderer();
                        // 把isMoving改为false
                        CarsManager1[i].isMoving = false;
                    }
                    // 把第二波改为false
                    wave2 = false;
                }
            }

            if (!wave2)
            {
                Debug.Log("进入第二波");
                for (int i = 0; i < num2; i++)
                {

                    int index = i;     // 存储索引，防止更新太快变动
                    if (CarsManager2[index].isMoving == false)
                    {
                        if (CarsManager2[index].pathStatus == status.none)
                        {
                            StartCoroutine(GetPathToServer((value) => CarsManager2[index].json = value, (value) => CarsManager2[index].pathStatus = value));
                        }
                        else if (CarsManager2[index].pathStatus == status.getSucceed)
                        {
                            CarsManager2[index].refresh();
                            CarsManager2[index].pathStatus = status.none;
                            CarsManager2[index].openRenderer();
                        }
                    }
                    else
                    {
                        CarsManager2[index].Move();

                        // 记得在下一次波次1之前改isMoving为false
                    }
                }

                wave2 = checkOver(CarsManager2);   // 若全部跑完则改变wave1为true
                if (wave2)
                {
                    // 关闭第二波车的渲染
                    for (int i = 0; i < num2; i++)
                    {
                        CarsManager2[i].closeRenderer();
                        // 把isMoving改为false
                        CarsManager2[i].isMoving = false;
                    }
                    // 把第一波改为false
                    wave1 = false;
                }

            }
        }

    }

    void Update()
    {

    }

    
    IEnumerator SendPutRequest()
    {
        // 假设你有两个 int 值 CarNum 和 AnotherNum

        int path = 3;
        becomeEven(ref road1);
        becomeEven(ref road2);
        becomeEven(ref road3);
        becomeEven(ref road4);
        List<int> Car_Num = new List<int> { road1, road2, road3, road4 };
        // 创建一个包含这两个值的 JSON 对象
        var requestBody = new
        {
            PathNum = path,
            Car_Num = Car_Num
        };

        // 将 JSON 对象序列化为字符串
        string jsonRequestBody = JsonUtility.ToJson(requestBody);

        //string requestBody = CarNum.ToString(); // 将 int 类型转换为字符串
        UnityWebRequest www = UnityWebRequest.Put(PutServeUrl, jsonRequestBody);
        www.downloadHandler = new DownloadHandlerBuffer();
        www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("请求失败: " + www.error);
        }
        else
        {
            Debug.Log("请求成功，回复： " + www.downloadHandler.text);

            // 检查服务器返回的文本是否为 "succeed"
            //if (www.downloadHandler.text == "["put succeed"]")
            //{
                //isPutSucceed = true;
                Debug.Log("收到 'put succeed'，继续执行");
                StartCoroutine(GetNumToServer(GetCarNumServeUrl));
            //}
            //else
            //{
                //Debug.Log("未收到 'succeed'，暂停执行");
            //}
        }

    }
    

    //IEnumerator GetPathToServer(System.Action<string> setString = null, System.Action<bool> setBool1 = null, System.Action<bool> setBool2 = null)
    IEnumerator GetPathToServer(System.Action<string> setString = null, System.Action<status> setStatus = null)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(GetPathServeUrl))
        {
            // 请求超时时间设置，这里设置为 10 秒
            //webRequest.timeout = 10;
            setStatus(status.isGetting);
            Debug.Log("正在请求路径");
            // 发送请求并等待响应
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                //Debug.LogError("错误: " + webRequest.error);
                Debug.Log("错误: ");
            }
            else
            {
                // 请求成功，处理响应数据
                Debug.Log("回复: " + webRequest.downloadHandler.text);
                // 使用 JsonUtility 在这里解析 JSON
                string temp = webRequest.downloadHandler.text;
                setString(temp);
                setStatus(status.getSucceed);
            }

        }
    }

    IEnumerator GetNumToServer(string url)
    {

        using (UnityWebRequest webRequestNum = UnityWebRequest.Get(url))
        {
            // 请求超时时间设置，这里设置为 10 秒
            //webRequest.timeout = 10;
            // 发送请求并等待响应
            Debug.Log("发送数量请求");
            yield return webRequestNum.SendWebRequest();
            if (webRequestNum.result == UnityWebRequest.Result.ConnectionError || webRequestNum.result == UnityWebRequest.Result.ProtocolError)
            {
                //Debug.LogError("错误: " + webRequest.error);
                Debug.Log("错误: ");
            }
            else
            {
                // 请求成功，处理响应数据
                Debug.Log("回复: " + webRequestNum.downloadHandler.text);
                // 使用 JsonUtility 在这里解析 JSON
                string json = webRequestNum.downloadHandler.text;
                carNum = JsonUtility.FromJson<Vector2>(json);
                getNumSucceed = true;
                num1 = (int)carNum.x;
                num2 = (int)carNum.y;

                // 初始化各个车辆
                CarsManager1 = new car[num1];
                for (int i = 0; i < num1; i++)
                {
                    CarsManager1[i] = new car();
                    CarsManager1[i].initialSphere(car_1);

                    // 设为子类
                    //CarsManager1[i].sphereObject.transform.SetParent(transform, false);
                }

                CarsManager2 = new car[num2];
                for (int i = 0; i < num2; i++)
                {
                    CarsManager2[i] = new car();
                    CarsManager2[i].initialSphere(car_1);

                    // 设为子类
                    //CarsManager2[i].sphereObject.transform.SetParent(transform, false);
                }

            }

        }
    }

    bool checkOver(car[] checkTarget)
    {
        for (int i = 0; i < checkTarget.Length; i++)
        {
            if (checkTarget[i].End == false)
            {
                return false;
            }
        }
        return true;
    }

    class car
    {
        public PathWrapper pathWrapper;     // 从AI组中获取到的信息
        public GameObject sphereObject;   // 表示车辆的一个物体
        public int currentWaypointIndex = 0; // 引用索引
        public bool isMoving = false;          // 标志是否在移动
        //public bool getNewPathSucceed = false;
        //public bool isGetting = false;
        public bool End = false;
        public status pathStatus = 0;    // 表示路径状态
        public string json;   // 用来保存json数据

        public void initialSphere(GameObject car)
        {

            sphereObject = Instantiate(car, new Vector3(-50f, -50f, -50f), Quaternion.identity);
            sphereObject.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);

            // 设置球体的层级
            sphereObject.layer = LayerMask.NameToLayer("Default");
            pathStatus = 0;

        }

        private void RequestData()
        {

            Debug.Log("！！！！！！！！请求数据！！！！！！！！！");

        }



        public void Move()
        {
            // 检查是否还有目标点
            if (currentWaypointIndex < pathWrapper.path.Length - 1)
            {

                Vector3 direction = (pathWrapper.path[currentWaypointIndex + 1] - pathWrapper.path[currentWaypointIndex]).normalized;

                if (direction != Vector3.zero)
                {
                    sphereObject.transform.rotation = Quaternion.LookRotation(direction);
                }

                sphereObject.transform.position = pathWrapper.path[currentWaypointIndex + 1];

                currentWaypointIndex++;
            }
            else
            {
                // 如果已经到达路径末尾，可以执行额外的逻辑或者停止移动
                Debug.Log("已经到达路径末尾");
                //isMoving = false;
                End = true;
                // 这里可以根据需要重置
            }
        }

        public void openRenderer()
        {
            MeshRenderer[] childMeshRenderer = sphereObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var child in childMeshRenderer)
            {
                child.enabled = true;
            }
        }

        public void closeRenderer()
        {
            MeshRenderer[] childMeshRenderer = sphereObject.GetComponentsInChildren<MeshRenderer>();
            foreach (var child in childMeshRenderer)
            {
                child.enabled = false;
            }
        }

        public void refresh()
        {

            pathWrapper = JsonUtility.FromJson<PathWrapper>(json);      // 获得新路径
            sphereObject.transform.position = pathWrapper.path[0];       // 设置为起点位置
            //sphereObject.transform.rotation = new Vector3(-90f,0f,0f);
            currentWaypointIndex = 0;
            isMoving = true;
            End = false;
        }

    }

    void becomeEven(ref int number){
        if(number < 6){
            if(number % 2 != 0){
                number += 1;
            }
        }
    }

    struct PathWrapper
    {
        public Vector3[] path;
    }

    enum status
    {
        none = 0,
        isGetting = 1,
        getSucceed = 2
    }

}
