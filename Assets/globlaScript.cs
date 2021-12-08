﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System;
using System.Text;

public class globlaScript : MonoBehaviour
{
    //public Camera camera;
    Socket socket;
    bool isVrPlayer;

    byte[] buffer = new byte[1024];
    int i = 0;

    private void Awake()
    {
        Debug.Log("Awake------------------------------------------------");
        IPAddress ip = IPAddress.Parse("127.0.0.1");
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        //socket.Connect(new IPEndPoint(ip, 18189)); //配置服务器IP与端口
        socket.Connect(new IPEndPoint(ip, 8885));

        Thread myThread = new Thread(ListenMessage);
        myThread.Start(socket);
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

                if (instruction.Equals("./identity"))//判定身份---应该是自己选择身份
                {
                    int l = me.Receive(buffer);
                    string info = Encoding.ASCII.GetString(buffer, 0, l);
                    isVrPlayer = Convert.ToBoolean(info);
                }
                else if (instruction.Equals("./wall"))//收墙体信息
                {

                }
                else if (instruction.Equals("./location"))//收位置信息
                {
                    /*
                    int lx = me.Receive(buffer);
                    double x = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                    int ly = me.Receive(buffer);
                    double y = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    int lz = me.Receive(buffer);
                    double z = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));
                    Debug.Log("./location x=" + x + "      y=" + y + "     z=" + z);

                    //检测
                    */
                }
                else if (instruction.Equals("./hand"))//收手势信息
                {
                    int lx = me.Receive(buffer);
                    String str = Encoding.ASCII.GetString(buffer, 0, lx);
                    //Debug.Log("收到的第一个数据:"+str);//**************************************************Receive的时候，服务端发送了n条消息，一下全接收了
                    double x = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                    int ly = me.Receive(buffer);
                    double y = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    int lz = me.Receive(buffer);
                    double z = Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));
                    Debug.Log("./location x=" + x + "      y=" + y + "     z=" + z);
                }
                //Encoding.ASCII.GetBytes("Server Say Hello");
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
        
    }

    // Update is called once per frame
    void Update()
    {
        //i++;
        transform.Translate(Vector3.forward*(i%5), Space.World);//Slef和World都是一直在反复运动
        //每隔deltaTime长时间更新坐标
    }
}
