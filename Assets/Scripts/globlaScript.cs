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


    public Text tipText;
    

    //public Camera camera;
    Socket socket;
    bool isVrPlayer;

    byte[] buffer = new byte[1024];
    //int i = 0;

    float x = -1, y = 1.6f, z = 2.5f;

    private void Awake()
    {

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
                    double handx = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                    int ly = me.Receive(buffer);
                    double handy = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    int lz = me.Receive(buffer);
                    double handz = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));
                    
                    //判断放在update
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
        XRSettings.enabled = true;
        try
        {
            //ClickListener._instance.tipText.text = "1";
            tipText.text = "1";
            IPAddress ip = IPAddress.Parse("127.0.0.1");
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(new IPEndPoint(ip, 18189)); //配置服务器IP与端口
            //ClickListener._instance.tipText.text = "2";
            tipText.text = "2";
            socket.Connect(new IPEndPoint(ip, 8885));
            //ClickListener._instance.tipText.text = "3";
            tipText.text = "3";
            Thread myThread = new Thread(ListenMessage);
            myThread.Start(socket);
            //ClickListener._instance.tipText.text = "4";
            tipText.text = "4";


            socket.Send(Encoding.ASCII.GetBytes(ClickListener.isVRPlayer.ToString()));
            Thread.Sleep(50);
            socket.Send(Encoding.ASCII.GetBytes(ClickListener.sureLoadMap.ToString()));
            Thread.Sleep(20);
        }
        catch (Exception e)
        {
            //ClickListener._instance.tipText.text = "catch";
            tipText.text = "catch";
            Debug.Log(e.Message);
            //ClickListener._instance.tipText.text = e.Message;
            tipText.text = e.Message;
        }

        print("ClickListener.sureLoadMap:"+ClickListener.sureLoadMap);
        if (ClickListener.sureLoadMap)
        {
            wallCount = 0;
            for (int i = 0; i < 24; i++)
            {
                print("ClickListener.wallsState[i]:" + ClickListener.wallsState[i]);
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
                socket.Send(Encoding.ASCII.GetBytes(wallCount.ToString()));
                Thread.Sleep(50);
                for (int i = 0; i < wallCount; i++)
                {
                    socket.Send(Encoding.ASCII.GetBytes(tempWalls[i].ToString()));
                    Thread.Sleep(50);
                }

                //入口index，出口index
                socket.Send(Encoding.ASCII.GetBytes("1"));//入口
                Thread.Sleep(50);
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
        print("wallCount:" + wallCount);
        int tempIndex = 0;
        for ( int i = 0; i < 24; i++)
        {
            print("i:" + i);
            if(i == tempWalls[tempIndex] && tempIndex < wallCount)
            {
                print("wall" + i + "setActive(true)");
                wallGameObjects[i].SetActive(true);
                tempIndex++;
            }
            else
            {
                print("wall" + i + "false");
                wallGameObjects[i].SetActive(false);
            }
        }


        //test
        print(ClickListener.isVRPlayer);
        //ClickListener._instance.tipText.text = "end Awake";
        tipText.text = "end Awake";
    }

    // Update is called once per frame
    void Update()
    {
        //i++;
        //transform.Translate(Vector3.forward*(i%5), Space.World);//Slef和World都是一直在反复运动
        //transform.Translate(new Vector3(x*-1.62f*z, y*1.62f*z, z), Space.World);//1.62太大了，现实中移动了1m，游戏中移动了3m+，不太合理，但是要求是重定向，在游戏内可以走很远。映射到unity里6*6如何，8*8？
        //Debug.Log("./location x=" + x + "             y=" + y + "            z=" + z);
        if(z == 2.5f)
        {

        }
        else
        {
            transform.position = new Vector3(x * -1.2f * z, y * 1.2f * z, z);//1.62时 225cm对应游戏内8m边界，此时z有1.5，z应等于2.5。所以1.62不止225对应240大，对应于1.5与2.5也偏大。
        }
        //每隔deltaTime长时间更新坐标
    }
}
