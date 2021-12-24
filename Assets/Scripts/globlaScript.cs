using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;
using UnityEngine.XR;
using Random = System.Random;

public class globlaScript : MonoBehaviour
{
    //游戏信息保存
    public static int[] walls = new int[24]; //记录墙体

    public int wallCount = 0; //墙体数量
    public int[] tempWalls = new int[24];

    public int startWall = 6;
    public int endWall = 0;


    //unity组件
    public GameObject[] wallGameObjects = new GameObject[24];
    public GameObject[] edgeGameObjects = new GameObject[16];
    public GameObject[] coinGameObjects = new GameObject[5];
    public Text warnText;
    float warnTextTime = 0, warnTextMaxTime = 3;
    public Text info1;
    public Text info2;
    public AudioSource coinSound;
    public GameObject theMainCamera;
    public GameObject dog;


    //public Camera camera;
    Socket socket;
    bool isVrPlayer;
    byte[] buffer = new byte[1024];

    //头
    float x = -0.238f, y = 1.2f, z = 1.8f;

    //手
    float handx = 0, handy = 0, handz = 0;
    bool isHandClose = false;

    Vector3[] coinPositions = new Vector3[5];

    //接收金币个数 随机生成两个从2开始 2,3,4
    private int receiveNum = 2;

    int leftCount = 5, getCount = 0;

    //高度
    private float height = ClickListener.playerHeight + 0.2f;

    //玩家数量
    private bool OnlyVR = !ClickListener.is2Player;

    //陷阱位置
    private int trapIndex = 0;
    private int trapX = 999;
    private float trapZ = 999.0f;

    private void Awake()
    {
    }

// Number随机数个数
// minNum随机数下限
// maxNum随机数上限
    public int[] GetRandomArray(int Number, int minNum, int maxNum)
    {
        int j;
        int[] b = new int[Number];
        Random r = new Random();
        for (j = 0; j < Number; j++)
        {
            int i = r.Next(minNum, maxNum + 1);
            int num = 0;
            for (int k = 0; k < j; k++)
            {
                if (b[k] == i)
                {
                    num = num + 1;
                }
            }

            if (num == 0)
            {
                b[j] = i;
            }
            else
            {
                j = j - 1;
            }
        }

        return b;
    }

    private void ListenMessage(object socket) //======================================================收消息
    {
        Socket me = (Socket)socket;
        try
        {
            while (true)
            {
                //instruction
                int length = me.Receive(buffer);
                string instruction = Encoding.ASCII.GetString(buffer, 0, length);
                //print("收到了指令:" + instruction);
                if (instruction.Equals("./location")) //收位置信息
                {
                    try
                    {
                        int lx = me.Receive(buffer);
                        x = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                        int ly = me.Receive(buffer);
                        y = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                        int lz = me.Receive(buffer);
                        z = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));
                    }
                    catch(Exception e1)
                    {
                        print(e1.Message);
                    }

                    //Debug.Log("./location x=" + x + "      y=" + y + "     z=" + z);

                    //修改坐标在update
                }
                else if (instruction.Equals("./hand")) //收手势信息--可以改碰撞模式
                {
                    try
                    {
                        int lx = me.Receive(buffer);
                        handx = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                        int ly = me.Receive(buffer);
                        handy = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                        int lz = me.Receive(buffer);
                        handz = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));

                        isHandClose = true;
                    }
                    catch (Exception e1)
                    {
                        print(e1.Message);
                    }

                    //判断放在update
                    //print("收到了手");
                }
                //接收金币
                else if (instruction.Equals("./coin") && OnlyVR == false)
                {
                    try
                    {
                        int lIndexOfCoin = me.Receive(buffer);
                        int indexOfCoin = (int)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lIndexOfCoin));
                        int coinX = (indexOfCoin % 4 + 1) * 2 - 5;
                        float coinZ = 10.5f - 2 * (indexOfCoin / 4 + 1);
                        if (receiveNum < 5)
                        {
                            coinPositions[receiveNum] = new Vector3(coinX, height, coinZ);
                            coinGameObjects[receiveNum].transform.position = coinPositions[receiveNum];
                            receiveNum++;
                            leftCount++;
                            warnText.text = "有一个新金币生成";
                            warnText.gameObject.SetActive(true);
                        }
                    }
                    catch (Exception e1)
                    {
                        print(e1.Message);
                    }

                }
                else if (instruction.Equals("./trap"))
                {
                    try
                    {
                        //把收到的端点赋值给全局变量，另外修改全局变量的值，比如dogTrapIsActive = true;
                        int lTrap = me.Receive(buffer);
                        trapIndex = (int)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lTrap));
                        trapX = (trapIndex % 4 + 1) * 2 - 5;
                        trapZ = 10.5f - 2 * (trapIndex / 4 + 1);

                        //这两行不用改，留着↓
                        warnText.text = "迷宫中出现了友好小动物，不要踩到它！";
                        warnText.gameObject.SetActive(true);
                    }
                    catch (Exception e1)
                    {
                        print(e1.Message);
                    }
                }
            }
        }
        catch (Exception e)
        {
            try
            {
                Debug.Log(e.Message);
                me.Shutdown(SocketShutdown.Both);
                me.Close();
            }
            catch (Exception ee)
            {
                Debug.Log(ee.Message);
            }
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        gameObject.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        theMainCamera.transform.rotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        if (OnlyVR)
        {
            wallCount = 0;
            for (int i = 0; i < 24; i++)
            {
                if (ClickListener.wallsState[i] == 1)
                {
                    tempWalls[wallCount] = i;
                    wallCount++;
                }
            }
            //直接更新墙体
            int tempIndex = 0;
            for (int i = 0; i < 24; i++)
            {
                if (i == tempWalls[tempIndex] && tempIndex < wallCount)
                {
                    wallGameObjects[i].SetActive(true);
                    tempIndex++;
                }
                else
                {
                    wallGameObjects[i].SetActive(false);
                }
            }
        }
        else
        {
            leftCount = 2;
        }

        info1.text = "场上剩余 " + leftCount;
        print("coinArrayLength" + leftCount);
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int[] coinArray = GetRandomArray(leftCount, 0, 15);
        for (int i = 0; i < leftCount; i++)
        {
            //coinArray[i] = random.Next(0, 15);
            Debug.Log("coinArray[" + i + "]" + coinArray[i]);
            int coinX_i = (coinArray[i] % 4 + 1) * 2 - 5;
            float coinZ_i = 10.5f - 2 * (coinArray[i] / 4 + 1);
            coinPositions[i] = new Vector3(coinX_i, height, coinZ_i);
            coinGameObjects[i].transform.position = coinPositions[i];
        }
        warnText.text = "场上已随机生成  " + leftCount + "  个金币";
        // Random random1 = new Random(Guid.NewGuid().GetHashCode());
        // for(int i=0;i<100;i++)
        // {print("i="+i+" random1.Next(0,15))="+random1.Next(0,15));}
        XRSettings.enabled = true;
        try
        {
            print("连接socket");
            IPAddress ip = IPAddress.Parse(ClickListener.serverIP);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(new IPEndPoint(ip, 18189)); //配置服务器IP与端口
            socket.Connect(new IPEndPoint(ip, 8885));

            print("socket连接成功");
            socket.Send(Encoding.ASCII.GetBytes(OnlyVR.ToString()));
            Thread.Sleep(50);
            if (OnlyVR == true)
            {
                print("only One");
            }
            else
            {
                print("two players");
                //socket.Send(Encoding.ASCII.GetBytes("./OnlyVR"));

                // socket.Send(Encoding.ASCII.GetBytes("./isVRPlayer"));
                // Thread.Sleep(20);
                socket.Send(Encoding.ASCII.GetBytes(ClickListener.isVRPlayer.ToString()));
                Thread.Sleep(50);
                // socket.Send(Encoding.ASCII.GetBytes("./sureLoadMap"));
                // Thread.Sleep(20);
                socket.Send(Encoding.ASCII.GetBytes(ClickListener.sureLoadMap.ToString()));
                Thread.Sleep(50);
                if (ClickListener.sureLoadMap)
                {
                    print("upload map");
                    wallCount = 0;
                    for (int i = 0; i < 24; i++)
                    {
                        if (ClickListener.wallsState[i] == 1)
                        {
                            tempWalls[wallCount] = i;
                            wallCount++;
                        }
                    }

                    try
                    {
                        //只发需要的墙体index
                        //发送总数量
                        print("发送墙体");
                        socket.Send(Encoding.ASCII.GetBytes(wallCount.ToString()));
                        Thread.Sleep(50);
                        for (int i = 0; i < wallCount; i++)
                        {
                            socket.Send(Encoding.ASCII.GetBytes(tempWalls[i].ToString()));
                            Thread.Sleep(50);
                        }
                        print("发完墙体");
                        // //入口index，出口index
                        // socket.Send(Encoding.ASCII.GetBytes("./entranceIndex"));
                        // Thread.Sleep(20);
                        // socket.Send(Encoding.ASCII.GetBytes("1"));//入口
                        // Thread.Sleep(50);
                        // socket.Send(Encoding.ASCII.GetBytes("./exitIndex"));
                        // Thread.Sleep(20);
                        // socket.Send(Encoding.ASCII.GetBytes("7"));//出口
                        // Thread.Sleep(50);
                    }
                    catch (Exception e)
                    {
                    }
                }
                else
                {
                    print("不传墙体，从socket收");
                    try
                    {
                        //如果自己没传则接收墙体
                        int dataLength = 0;
                        dataLength = socket.Receive(buffer);
                        wallCount = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, dataLength));

                        for (int i = 0; i < wallCount; i++)
                        {
                            dataLength = socket.Receive(buffer);
                            tempWalls[i] = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, dataLength));
                        }

                        //dataLength = socket.Receive(buffer);
                        //startWall = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, dataLength));
                        //dataLength = socket.Receive(buffer);
                        //endWall = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, dataLength));
                    }
                    catch (Exception e)
                    {
                    }
                }
                print("更新墙体显示");
                //更新墙体
                int tempIndex = 0;
                for (int i = 0; i < 24; i++)
                {
                    if (i == tempWalls[tempIndex] && tempIndex < wallCount)
                    {
                        wallGameObjects[i].SetActive(true);
                        tempIndex++;
                    }
                    else
                    {
                        wallGameObjects[i].SetActive(false);
                    }
                }
                Thread.Sleep(50);
                socket.Send(Encoding.ASCII.GetBytes(coinArray[0].ToString()));
                Thread.Sleep(50);
                socket.Send(Encoding.ASCII.GetBytes(coinArray[1].ToString()));
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            warnText.text = e.Message;
        }

        Thread myThread = new Thread(ListenMessage);
        myThread.Start(socket);
    }

    // Update is called once per frame
    void Update()
    {
        //  tan35 = 0.70  tan30 = 0.577           tan29 = 0.55    tan22 = 0.404 
        transform.position = new Vector3(x * (0.7f) * z * 3.33f, y * 0.577f * z + ClickListener.kinectHeight, (z - 1.5f) * 3.33f + 1.5f);
        

        //这四行变量都要改为全局变量
        bool dogTrapIsActive = false;
        float speed = 0;//注意deltaTime单位是毫秒
        int start = 9, end = 1;//模拟从socket收到的两端点值
        int direction = 1;
        if(dogTrapIsActive)
        {
            /*
                移动方法1：修改本身坐标
                dog.transform.position = new Vector3(Time.deltaTime * speed + dog.transform.position.x, 0, 0);
                移动方法2：平移--更简单点，因为修改本身坐标你还要计算该往哪走
                dog.transform.Translate(new Vector3(Time.deltaTime * speed * 1, 0, Time.deltaTime * speed * 1),Space.World);
                通过狗的两个端点判断，来确定应该在x轴方向上走还是z轴方向上走
            */
            if(end - start <= 3)//x轴上走
            {
                dog.transform.Translate(new Vector3(Time.deltaTime * speed * direction, 0, 0), Space.World);
            }
            else// if((end-start) % 4 == 0)//在z轴上走
            {
                dog.transform.Translate(new Vector3(0, 0, Time.deltaTime * speed * direction), Space.World);
            }
            if(true)//如果走到头，需要转180
            {
                direction = -direction;
                //dog.transform.rotation = ...//狗的模型也要转
            }
        }


        if (warnText.IsActive() && (warnTextTime < warnTextMaxTime))
        {
            warnTextTime += Time.deltaTime;
        }
        else
        {
            warnTextTime = 0;
            warnText.gameObject.SetActive(false);
        }

        if (isHandClose)
        {
            //print("hand映射前   " + handx + "  " + handy + "  " + handz);
            //映射手的坐标
            handx = handx * (0.7f) * handz * 3.33f;
            handy = handy * 0.577f * handz + ClickListener.kinectHeight;
            handz = (handz - 1.5f) * 3.33f + 1.5f;
            //print("hand映射后   " + handx + "  " + handy + "  " + handz);


            for (int i = 0; i < 5; i++)
            {
                if ( /*coinGameObjects[i].isActive*/coinGameObjects[i].active && coinPositions[i].y != -2 &&
                                                    Math.Abs(coinPositions[i].x - handx) < 0.7 &&
                                                    Math.Abs(coinPositions[i].y - handy) < 0.5 &&
                                                    Math.Abs(coinPositions[i].z - handz) < 0.7) //如果y不是-2
                {
                    //发送i
                    socket.Send(Encoding.ASCII.GetBytes("./getCoin"));
                    Thread.Sleep(50);
                    socket.Send(Encoding.ASCII.GetBytes(i.ToString()));
                    //消除金币，y坐标置为-2
                    coinGameObjects[i].transform.position = new Vector3(0, -2, 0);
                    coinPositions[i].y = -2;
                    coinGameObjects[i].SetActive(false);
                    //coinGameObjects[i].SetActive(false);
                    //播放声音
                    coinSound.Play();
                    //更新画布--金币数值，特定字符串加数字
                    leftCount--;
                    getCount++;
                    info1.text = "场上剩余 " + leftCount;
                    info2.text = "已拾取 " + getCount;
                }
            }
            //关闭手
            isHandClose = false;
        }

        if (getCount == 5 && Math.Abs(x - 3) < 1 && Math.Abs(z - 8.5) < 1)
        {
            //发送游戏结束
            String success = "success";
            socket.Send(Encoding.ASCII.GetBytes("./success"));
            Thread.Sleep(50);
            socket.Send(Encoding.ASCII.GetBytes(success));
        }

        if (getCount == 5 && Math.Abs(x - trapX) < 1 && Math.Abs(z - trapZ) < 1)
        {
            //发送游戏结束
            String success = "fail";
            socket.Send(Encoding.ASCII.GetBytes("./fail"));
            Thread.Sleep(50);
            socket.Send(Encoding.ASCII.GetBytes(success));
        }
    }
}