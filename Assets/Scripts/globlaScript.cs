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
    public static int[] walls = new int[24];//记录墙体

    public int wallCount = 0;//墙体数量
    public int[] tempWalls = new int[24];

    public int startWall = 6;
    public int endWall = 0;


    //unity组件
    public GameObject[] wallGameObjects = new GameObject[24];
    public GameObject[] edgeGameObjects = new GameObject[16];
    public GameObject[] coinGameObjects = new GameObject[5];
    public Text warnText;
    public Text info1;
    public Text info2;
    public AudioSource coinSound;
    

    //public Camera camera;
    Socket socket;
    bool isVrPlayer;
    byte[] buffer = new byte[1024];

    //头
    float x = -1, y = 1.6f, z = 2.5f;
    //手
    float handx = -1, handy = 1.6f, handz = 2.5f;
    bool isHandClose = true;
    Vector3[] coinPositions = new Vector3[5];
    //接收金币个数 随机生成两个从2开始 2,3,4
    private int receiveNum = 2;
    int leftCount = 5, getCount = 0;
    //高度
    private float height=ClickListener.playerHeight/100+0.2f;
    //玩家数量
    private bool OnlyVR = !ClickListener.is2Player;
    //陷阱位置
    private int trapIndex=0;
    private int trapX=999;
    private float trapZ=999.0f;
    private void Awake()
    {

    }
// Number随机数个数
// minNum随机数下限
// maxNum随机数上限
    public int[] GetRandomArray(int Number,int minNum,int maxNum)
    {
        int j;
        int[] b=new int[Number];
        Random r=new Random();
        for(j=0;j<Number;j++)
        {
            int i=r.Next(minNum,maxNum+1);
            int num=0;
            for(int k=0;k<j;k++)
            {
                if(b[k]==i)
                {
                    num=num+1;
                }
            }
            if(num==0 )
            {
                b[j]=i;
            }
            else
            {
                j=j-1;
            }
        }
        return b;
    }
    private void ListenMessage(object socket)//======================================================收消息
    {
        Socket me = (Socket) socket;
        try
        {
            while (true)
            {
                //instruction
                int length = me.Receive(buffer);
                string instruction = Encoding.ASCII.GetString(buffer, 0, length);
                //Debug.Log("收到指令=" + instruction);

                if (instruction.Equals("./location"))//收位置信息
                {
                    
                    int lx = me.Receive(buffer);
                    x = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                    int ly = me.Receive(buffer);
                    y = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    int lz = me.Receive(buffer);
                    z = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));
                    //Debug.Log("./location x=" + x + "      y=" + y + "     z=" + z);

                    //修改坐标在update
                    
                }
                else if (instruction.Equals("./hand"))//收手势信息--可以改碰撞模式
                {
                    int lx = me.Receive(buffer);
                    handx = (float) Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                    int ly = me.Receive(buffer);
                    handy = (float) Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    int lz = me.Receive(buffer);
                    handz = (float) Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));

                    isHandClose = true;
                    //判断放在update
                    print("收到了手");
                }
                //接收金币
                else if(instruction.Equals("./coin")&&OnlyVR==false)
                {
                    int lIndexOfCoin = me.Receive(buffer);
                    int indexOfCoin=(int)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lIndexOfCoin));
                    int coinX = (indexOfCoin % 4 + 1)*2-5;
                    float coinZ = 10.5f-2*(indexOfCoin / 4 + 1);
                    if (receiveNum < 5)
                    {
                        coinPositions[receiveNum]=new Vector3(coinX, height, coinZ);
                        coinGameObjects[receiveNum].transform.position = coinPositions[receiveNum];
                        receiveNum++;
                        leftCount++;

                    }
                    

                }
                // //接收高度
                // else if(instruction.Equals("./height"))
                // {
                //     int lheight = me.Receive(buffer);
                //     height=(float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lheight));
                //
                // }
                // else if(instruction.Equals("./OnlyVR"))
                // {
                //     int lOnlyVR = me.Receive(buffer);
                //     OnlyVR=Convert.ToBoolean(Encoding.ASCII.GetString(buffer, 0, lOnlyVR));
                // }
                else if(instruction.Equals("./trap"))
                {
                    int lTrap = me.Receive(buffer);
                    trapIndex=(int)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lTrap));
                    trapX = (trapIndex % 4 + 1)*2-5;
                    trapZ = 10.5f-2*(trapIndex / 4 + 1);
                    
                }
            }
        } catch (Exception e)
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
        // Random random1 = new Random(Guid.NewGuid().GetHashCode());
        // for(int i=0;i<100;i++)
        // {print("i="+i+" random1.Next(0,15))="+random1.Next(0,15));}
        XRSettings.enabled = true;
        try
        {
            IPAddress ip = IPAddress.Parse(ClickListener.serverIP);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(new IPEndPoint(ip, 18189)); //配置服务器IP与端口
            socket.Connect(new IPEndPoint(ip, 8885));
            Thread myThread = new Thread(ListenMessage);
            myThread.Start(socket);
            socket.Send(Encoding.ASCII.GetBytes("./OnlyVR"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(OnlyVR.ToString()));
            socket.Send(Encoding.ASCII.GetBytes("./isVRPlayer"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(ClickListener.isVRPlayer.ToString()));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes("./sureLoadMap"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(ClickListener.sureLoadMap.ToString()));
            Thread.Sleep(20);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            warnText.text = e.Message;
        }

        if (ClickListener.sureLoadMap)
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
            try
            {
                //只发需要的墙体index
                //发送总数量
                socket.Send(Encoding.ASCII.GetBytes("./wallCount"));
                Thread.Sleep(20);
                socket.Send(Encoding.ASCII.GetBytes(wallCount.ToString()));
                Thread.Sleep(50);
                for (int i = 0; i < wallCount; i++)
                {
                    socket.Send(Encoding.ASCII.GetBytes("./tempWalls"));
                    Thread.Sleep(20);
                    socket.Send(Encoding.ASCII.GetBytes(tempWalls[i].ToString()));
                    Thread.Sleep(50);
                }

                //入口index，出口index
                socket.Send(Encoding.ASCII.GetBytes("./entranceIndex"));
                Thread.Sleep(20);
                socket.Send(Encoding.ASCII.GetBytes("1"));//入口
                Thread.Sleep(50);
                socket.Send(Encoding.ASCII.GetBytes("./exitIndex"));
                Thread.Sleep(20);
                socket.Send(Encoding.ASCII.GetBytes("7"));//出口
                Thread.Sleep(50);
            }
            catch(Exception e)
            {

            }

        }
        else
        {
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
                dataLength = socket.Receive(buffer);
                startWall = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, dataLength));
                dataLength = socket.Receive(buffer);
                endWall = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, dataLength));
            }
            catch(Exception e)
            {

            }
        }

        //更新墙体
        int tempIndex = 0;
        for ( int i = 0; i < 24; i++)
        {
            if(i == tempWalls[tempIndex] && tempIndex < wallCount)
            {
                wallGameObjects[i].SetActive(true);
                tempIndex++;
            }
            else
            {
                wallGameObjects[i].SetActive(false);
            }
        }
        
        
        if (OnlyVR)
        {
           
        }
        else
        {
            leftCount = 2;
        }

        info1.text = "场上剩余 "+leftCount;
        print("coinArrayLength"+leftCount);
        Random random = new Random(Guid.NewGuid().GetHashCode());
        int []coinArray = GetRandomArray(leftCount, 0, 15);
        for (int i = 0; i < leftCount; i++)
        {
            //coinArray[i] = random.Next(0, 15);
            Debug.Log("coinArray["+i+"]"+coinArray[i]);
            int coinX_i = (coinArray[i] % 4 + 1)*2-5;
            float coinZ_i = 10.5f - 2 * (coinArray[i] / 4 + 1);
            coinPositions[i] = new Vector3(coinX_i, height, coinZ_i);
            coinGameObjects[i].transform.position = coinPositions[i];
        }

        if (OnlyVR==false)
        {
            socket.Send(Encoding.ASCII.GetBytes("./twoCoins"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(coinArray[0].ToString()));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(coinArray[1].ToString()));

        }
        // int a = random.Next(0, 15);
        // int b = random.Next(0, 15);
        // Debug.Log("a="+a);
        // Debug.Log("b="+b);
        // int coinX_a = (a / 4 + 1)*2-5;
        // float coinZ_a = 10.5f-2*(a % 4 + 1);
        // int coinX_b = (b / 4 + 1) * 2 - 5;
        // float coinZ_b = 10.5f - 2*(a % 4 + 1);
        // coinPositions[0] = new Vector3(coinX_a, height, coinZ_a);
        // coinPositions[1] = new Vector3(coinX_b, height, coinZ_b);
        // coinGameObjects[0].transform.position = coinPositions[0];
        // coinGameObjects[1].transform.position = coinPositions[1];
        /*
        //随机生成两个金币位置-----------------------------------------------------------------------------------------------有bug，查随机数API---
        coinPositions[0] = new Vector3(new System.Random().Next(-1,2)*2-1, 3, new System.Random().Next(1,4)*2+0.5f);
        coinPositions[1] = new Vector3(new System.Random().Next(-1,2)*2-1, 3, new System.Random().Next(1,4)*2+0.5f);
        coinGameObjects[0].transform.position = coinPositions[0];
        coinGameObjects[1].transform.position = coinPositions[1];
        */
        //因为有bug所以设了几个测试值
        // coinPositions[0] = new Vector3(-1, 2, 2.5f);
        // coinPositions[1] = new Vector3(-1, 2, 2.5f);
        // coinGameObjects[0].transform.position = new Vector3(-1, 2, 2.5f);
        // coinGameObjects[1].transform.position = new Vector3(-1, 2, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        //i++;
        //transform.Translate(Vector3.forward*(i%5), Space.World);//Slef和World都是一直在反复运动
        //transform.Translate(new Vector3(x*-1.62f*z, y*1.62f*z, z), Space.World);//1.62太大了，现实中移动了1m，游戏中移动了3m+，不太合理，但是要求是重定向，在游戏内可以走很远。映射到unity里6*6如何，8*8？
        //Debug.Log("./location x=" + x + "             y=" + y + "            z=" + z);
        if(z == 2.5f)//没有连接kinect时不移动
        {

        }
        else//收到kinect数据导致z！=2.5，按收的数据产生平移---需要设置各参数 比如如下的1.2、3.3等（12.18日晚测试3.3太大，其实是因为忘记考虑1.5的问题了）
        {
            transform.position = new Vector3(x * -1.2f * z, y * 1.2f * z, (z - 1.5f) * 3.3f + 1.5f);//1.62时 225cm对应游戏内8m边界，此时z有1.5，z应等于2.5。所以1.62不止225对应240大，对应于1.5与2.5也偏大。
        }
        //每隔deltaTime长时间更新坐标


        //判断手
        if(isHandClose)
        {
            //映射手的坐标
            /*
            handx = handx * -1.2f * handz;
            handy = handy * 1.2f * handz;
            */
            handx = -1;
            handy = 2;
            handz = 2.5f;
            //print("hand   " +handx+"  "+handy+"  "+handz);


            //判断位置   < 1就判断为捡到了可能太容易了
            for(int i = 0; i < 5; i++)
            {
                if (/*coinGameObjects[i].isActive*/coinPositions[i].y!= -2 && Math.Abs(coinPositions[i].x - handx) < 1 && Math.Abs(coinPositions[i].y - handy) < 1 && Math.Abs(coinPositions[i].z - handz) < 1)//如果y不是-2
                {
                    //发送i
                    socket.Send(Encoding.ASCII.GetBytes("./getCoin"));
                    Thread.Sleep(20);
                    socket.Send(Encoding.ASCII.GetBytes(i.ToString()));
                    //消除金币，y坐标置为-2
                    coinGameObjects[i].transform.position = new Vector3(0, -2, 0);
                    coinPositions[i].y = -2;
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
            //isHandClose = false;
        }

        if (getCount == 5&&Math.Abs(x-3)<1&&Math.Abs(z-8.5)<1)
        {
            //发送游戏结束
            String success = "success";
            socket.Send(Encoding.ASCII.GetBytes("./success"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(success));
        }
        if (getCount == 5&&Math.Abs(x-trapX)<1&&Math.Abs(z-trapZ)<1)
        {
            //发送游戏结束
            String success = "fail";
            socket.Send(Encoding.ASCII.GetBytes("./fail"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes(success));
        }
    }
}
