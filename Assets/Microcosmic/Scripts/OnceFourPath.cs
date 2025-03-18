using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class OnceFourPath : MonoBehaviour
{

    public GameObject car_1;
    public GameObject car_2;

    private System.Random random; //随机器
    private int CarNum = 0;    // 车的数量
    private int road1 = 0;  // 道路1的车数量
    private int road2 = 0;  // 道路2的车数量
    private int road3 = 0;  // 道路3的车数量
    private int road4 = 0;  // 道路4的车数量
    //private string PutServeUrl = "http://8.138.121.2:8080/put_path_num?path_num=4";   // 存放URL
    private bool isPutSucceed = false;  // 标志需求是否成功
    private string PutServeUrl = "http://8.138.170.116:8081/microscopic/put_sub_position";
    private string GetPathServeUrl = "http://8.138.170.116:8081/microscopic/get_sub_position";  //用来获取路径的URL
    private string ResetUrl =  "http://8.138.170.116:8081/microscopic/reset_counter";
    private bool AllPathSucceed  = false;

    car[] cars;   // 主要的车类

    // Start is called before the first frame update
    void Awake(){
        //Debug.Log("CarNum = " + CarNum);
        //CarNum = ScenesManager2D.Instance.CarCount;
        
        road1 = ScenesManager2D.Instance.CarCounts[0];
        road2 = ScenesManager2D.Instance.CarCounts[1];
        road3 = ScenesManager2D.Instance.CarCounts[2];
        road4 = ScenesManager2D.Instance.CarCounts[3];
        
        /*
        road1 = 8;
        road2 = 8;
        road3 = 8;
        road4 = 8;
        */
        Debug.Log("获取到的车辆数量分别为:" + road1 + "  " + road2 + "  " + road2 + "  " + road2 + "  ");
        StartCoroutine(SendReset(ResetUrl));
    }
    void Start()
    {
        random = new System.Random();
        StartCoroutine(SendPutRequest(PutServeUrl));
    }

    void FixedUpdate(){
        if(isPutSucceed && !AllPathSucceed)
        {
            AllPathSucceed = checkOver(cars);
            Debug.Log("执行if判断后AllPathSucceed为" + AllPathSucceed);
        }
        Debug.Log("FixedUpdate执行中");
        //Debug.Log("AllPathSucceed为" + AllPathSucceed);

    }

    // Update is called once per frame
    void Update()
    {
        
        //if(!AllPathSucceed){
            //AllPathSucceed = checkOver(cars);
        //}

        if(isPutSucceed){
            for (int i = 0; i < CarNum; i++)
                {

                    int index = i;     // 存储索引，防止更新太快变动
                    if (cars[index].isMoving == IsMove.fal)
                    {
                        if (cars[index].pathStatus == status.none)
                        {
                            StartCoroutine(GetPathToServer((value) => cars[index].json = value, (value) => cars[index].pathStatus = value));
                        }
                        else if (cars[index].pathStatus == status.getSucceed)
                        {
                            cars[index].refresh();
                            //cars[index].openRenderer();
                        }
                    }
                    else if (cars[index].isMoving == IsMove.stop){
                        
                    }
                    else
                    {
                        if(AllPathSucceed)
                            cars[index].Move();

                    }
                }
        }
        else{

        }
        
    }

    IEnumerator SendReset(string ResetUrl)
    {
        //UnityWebRequest www = UnityWebRequest.Put(PutServeUrl, "");
        //string requestBody = CarNum.ToString(); // 将 int 类型转换为字符串

        UnityWebRequest www = UnityWebRequest.Get(ResetUrl);
        www.downloadHandler = new DownloadHandlerBuffer();
        //www.SetRequestHeader("Content-Type", "application/json");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("请求失败: " + www.error);
        }
        else
        {
            Debug.Log("重置成功，回复： " + www.downloadHandler.text);

        }

    }

    IEnumerator SendPutRequest(string PutServeUrl)
    {
        //UnityWebRequest www = UnityWebRequest.Put(PutServeUrl, "");
        //string requestBody = CarNum.ToString(); // 将 int 类型转换为字符串


        int path = 4;
        ControlQuantity(ref road1);
        ControlQuantity(ref road2);
        ControlQuantity(ref road3);
        ControlQuantity(ref road4);
        CarNum = road1 + road2 + road3 + road4;
        Debug.Log("准备传输的车辆数量分别为:" + road1 + "  " + road2 + "  " + road2 + "  " + road2 + "  ");
        //ist<int> Car_Num = new List<int> { road1, road2, road3, road4 };
        // 创建一个包含这两个值的 JSON 对象
        ToAI requestBody = new ToAI
        {
            PathNum = path,
            Car_Num = new int[] { road1, road2, road3, road4 }
        };

        // 将 JSON 对象序列化为字符串
        string jsonRequestBody = JsonUtility.ToJson(requestBody);
        Debug.Log("json文件为:"+requestBody);
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
            // put成功后对车辆进行初始化
            cars = new car[CarNum];
            for (int i = 0; i < CarNum; i++)
            {
                cars[i] = new car();
                int randomNumber = random.Next(1, 3); // 1 到 3（不包括3），即 1 或 2
                if(randomNumber == 1){
                    cars[i].initialCar(car_1);
                }
                else{
                    cars[i].initialCar(car_2);
                }

                // 设为子类
                //CarsManager1[i].sphereObject.transform.SetParent(transform, false);
            }

            isPutSucceed = true;
            Debug.Log("收到 'put succeed'，继续执行");

        }

    }

    IEnumerator GetPathToServer(System.Action<string> setString = null, System.Action<status> setStatus = null)
    {

        using (UnityWebRequest webRequest = UnityWebRequest.Get(GetPathServeUrl))
        {
            // 请求超时时间设置，这里设置为 10 秒
            webRequest.timeout = 6000;
            setStatus(status.isGetting);
            Debug.Log("正在请求路径");
            // 发送请求并等待响应
            yield return webRequest.SendWebRequest();
            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                //Debug.LogError("错误: " + webRequest.error);
                Debug.Log("错误: " + webRequest.error);
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

    class car
    {
        public PathWrapper pathWrapper;     // 从AI组中获取到的信息
        public GameObject sphereObject;   // 表示车辆的一个物体
        public int currentWaypointIndex = 0; // 引用索引
        public IsMove isMoving = IsMove.fal;          // 标志是否在移动
        //public bool getNewPathSucceed = false;
        //public bool isGetting = false;
        public status pathStatus = status.none;    // 表示路径状态
        public string json;   // 用来保存json数据

        public void initialCar(GameObject car)
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
                //isMoving = IsMove.fal;
                pathStatus = status.none;
                isMoving = IsMove.stop;
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
            isMoving = IsMove.tr;

            Vector3 direction = (pathWrapper.path[currentWaypointIndex + 1] - pathWrapper.path[currentWaypointIndex]).normalized;

                if (direction != Vector3.zero)
                {
                    sphereObject.transform.rotation = Quaternion.LookRotation(direction);
                }

            //pathStatus = status.none;
        }

    }

    void becomeEven(ref int number){
        if(number < 6){
            if(number % 2 != 0){
                number += 1;
            }
        }
    }

    void ControlQuantity(ref int num){
        if(num < 6){
            num = 6;
        }
    }

    bool checkOver(car[] checkTarget)
    {
        for (int i = 0; i < checkTarget.Length; i++)
        {
            if (checkTarget[i].pathStatus != status.getSucceed)
            {
                return false;
            }
        }
        return true;
    }

    struct PathWrapper
    {
        public Vector3[] path;
    }

    public class ToAI  // 传给AI组的信息
    {
        public int PathNum;
        public int[] Car_Num;
    }


    enum IsMove{
        fal = 0,
        tr = 1,
        stop = 2
    }

    enum status
    {
        none = 0,
        isGetting = 1,
        getSucceed = 2
    }

}
