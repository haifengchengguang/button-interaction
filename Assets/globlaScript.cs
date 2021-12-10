using System.Collections;
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
    //int i = 0;

    float x = 0, y = 0, z = 0;

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
                    
                    int lx = me.Receive(buffer);
                    x = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                    int ly = me.Receive(buffer);
                    y = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    int lz = me.Receive(buffer);
                    z = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lz));
                    //Debug.Log("./location x=" + x + "      y=" + y + "     z=" + z);

                    //修改坐标在update
                    
                }
                else if (instruction.Equals("./hand"))//收手势信息
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
        
    }

    // Update is called once per frame
    void Update()
    {
        //i++;
        //transform.Translate(Vector3.forward*(i%5), Space.World);//Slef和World都是一直在反复运动
        transform.Translate(new Vector3(x*-1.62f*z, y*1.62f*z, z), Space.World);//1.62太大了，现实中移动了1m，游戏中移动了3m+，不太合理，但是要求是重定向，在游戏内可以走很远。映射到unity里6*6如何，8*8？
        Debug.Log("./location x=" + x + "             y=" + y + "            z=" + z);
        //每隔deltaTime长时间更新坐标
    }
}
