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

public class ObserverCoinControl : MonoBehaviour
{

    //游戏信息保存
    private int[] isShowWalls = new int[24];//记录墙体
    public GameObject[] walls = new GameObject[24];//存放墙体实体

    private int wallCount = 0;//墙体数量

    //unity组件-这是16个点击区域
    public GameObject[] buttonArea = new GameObject[16];//unity中的16个区域
    
    public GameObject[] coins = new GameObject[5];//五个金币实体
    private int[] coinPosition = new int[5];//存放金币在哪个块上，初始为-1，在unity中设置初始化
    public static int coinIndex = 0;//用来标记生成金币得索引，当前正在生成哪个金币
    //public int coinAcquireIndex;//用来接收金币销毁得索引

    public GameObject player;   //游戏者
    private float x, y; //存放player的位置信息
    private float reX, reY;//接收的坐标信息

    public Text tipText;    //用户提示信息（提示是否进入游戏以及游戏是否结束。。。）
    public Text tipHelp;    //用户提示信息（游戏中操作提示。。。）
    public Text gameStateText; //提示当前的状态信息，金币剩余量，
    public Text gameStateText2;//提示当前的状态信息,观察者还可以制造金币数量
    public Text gameStateText3;//显示玩家当前收集了多少金币

    public GameObject buttonCoin;//添加金币按钮
    public GameObject buttonTrap;//添加陷阱按钮 
    private bool buttonCoinClick;

    Socket socket;//连接服务端接收信息
    
    byte[] buffer = new byte[1024];


    int leftCount = 2, canMakeCoinNum = 3, collectionNum = 0;


    private void ListenMessage(object socket)
    {
        Socket me = (Socket)socket;
        try
        {
            while (true)
            {
                int length = me.Receive(buffer);
                string instruction = Encoding.ASCII.GetString(buffer, 0, length);
                //Debug.Log("收到指令=" + instruction);
                if (instruction.Equals("./location"))//收位置信息
                {
                    try
                    {
                        //收player的x,y坐标
                        int lx = me.Receive(buffer);
                        reX = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, lx));
                        int ly = me.Receive(buffer);
                        reY = (float)Convert.ToDouble(Encoding.ASCII.GetString(buffer, 0, ly));
                    }
                    catch(Exception e)
                    {

                    }
                }
                else if (instruction.Equals("./getCoin"))//当获得金币以后，每次传给一个位置,根据位置进行查找然后删除
                {
                    try
                    {
                        int tempIndex = me.Receive(buffer);
                        //接收得是需要销毁得金币得位置
                        int temP = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, tempIndex));

                        coinPosition[temP] = -1;
                        //销毁
                        coins[temP].SetActive(false);
                        collectionNum++;//收集金币数加一
                        gameStateText3.text = "玩家已经收集了 " + collectionNum + "个金币";
                        leftCount--;
                        gameStateText.text = "场上剩余 " + leftCount;
                    }
                    catch(Exception e)
                    {

                    }
                }
                else if (instruction.Equals("./trap")) //接收陷阱位置
                {

                }
                else if (instruction.Equals("./gameOver"))//收到游戏结束的信息
                {
                    //收游戏结果
                    int num = me.Receive(buffer);
                    string gameState = Encoding.ASCII.GetString(buffer, 0, num);
                    if(gameState.Equals("success"))
                    {
                        tipText.text = "游戏结束，玩家未走出了迷宫";
                    }
                    else if(gameState.Equals("fail"))
                    {
                        tipText.text = "游戏结束，玩家触发了陷阱";
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

    //=============开始
    void Start()
    {
        try
        {
            //设置提示框，提醒用户操作--
            //UnityEditor.EditorUtility.DisplayDialog("标题", "提示内容", "确认", "取消");
            //初始化禁用16个区域的按钮
            for (int i = 0; i < buttonArea.Length; i++)
            {
                buttonArea[i].SetActive(false);//初始16个按钮禁用，当需要时才可以激活
                Debug.Log("禁用按钮"+buttonArea[i].name);
            }

            //墙体初始化--开始全不存在false
            for(int i = 0; i < walls.Length; i++)
            {
                walls[i].SetActive(false);
            }

            //初始化player
            //player.SetActive(false);

            //初始化字幕
            tipText.text = "主角未进场，等待游戏开始-----";
            gameStateText.text = "当前场中还有 " + leftCount + " 个金币";
            gameStateText2.text = "还可以制造 " + canMakeCoinNum + "个金币";
            gameStateText3.text = "玩家已经收集了 " + collectionNum + "个金币";

            //初始化金币,将位置块设置为-1，表示没有初始化
            for(int i = 0; i < 5; i++)
            {
                coins[i].SetActive(false);
                coinPosition[i] = -1;
                //测试-不需要了
                //coins[i].GetComponent<RectTransform>().localPosition = new Vector3(coinRealPositionX[i], coinRealPositionY[i], 0);
                //coins[i].SetActive(true);
            }
            //设置金币得索引
            coinIndex = 0;
            //coinAcquireIndex = -1;

            //给添加金币绑定事件
            buttonCoin.GetComponent<Button>().onClick.AddListener(clickToCoin);
            
            //给添加陷阱按钮绑定事件

            //客户端连接服务器
            IPAddress ip = IPAddress.Parse(ClickListener.serverIP);
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            //socket.Connect(new IPEndPoint(ip, 18189)); //配置服务器IP与端口
            socket.Connect(new IPEndPoint(ip, 8885));


            //开局发俩false-表示不是单人且不是VR
            socket.Send(Encoding.ASCII.GetBytes("False"));
            Thread.Sleep(20);
            socket.Send(Encoding.ASCII.GetBytes("False"));
            Thread.Sleep(20);

            //只发需要的墙体index
            //发送总数量
            wallCount = socket.Receive(buffer);
            wallCount = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, wallCount));
            print("收到墙体数量：" + wallCount);
            int temp = 0;
            int WallIndex = -1;
            //接收每个墙体-将墙体setActive设置为true
            for (int i = 0; i < wallCount; i++)
            {
                temp = socket.Receive(buffer);//获得每个墙的索引
                WallIndex = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, temp));
                walls[WallIndex].SetActive(true);//将该位置的墙体信息显示出来
                isShowWalls[WallIndex] = 1; //记录哪个墙正在显示，显示为1，不显示为0，不是很需要
            }

            int reCoin = 0 ;
            int cointemP = -1;
            //接收金币
            for(int i = 0; i < 2; i++)
            {
                //顺序接收金币的位置
                reCoin = socket.Receive(buffer);
                print(Encoding.ASCII.GetString(buffer, 0, reCoin));
                cointemP = Convert.ToInt32(Encoding.ASCII.GetString(buffer, 0, reCoin));
                coinPosition[coinIndex] = cointemP; //记下该金币的位置
                //将金币放到固定位置
                int[] location = CalculateCoinPosition(cointemP);
                coins[coinIndex].GetComponent<RectTransform>().localPosition
                    = new Vector3(location[0], location[1], 0);
                //显示金币
                coins[coinIndex].SetActive(true);

                //索引增加
                coinIndex++;
            }

            Thread myThread = new Thread(ListenMessage);//该线程负责接收信息
            myThread.Start(socket);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            tipText.text = "系统错误";
        }

    }


    // Update is called once per frame
    void Update()
    {
        //判断是否已经进场-----只有收到xy不为0的时候就已经开始了
        if (x != 0 && y != 0)
        {
            tipText.text = "主角已经进场，游戏开始-----";

            //player坐标--实时更新
            //player坐标映射到2D
            x = reX * 375;
            y = (reY - 1.5f) * -375;
            player.GetComponent<RectTransform>().position = new Vector3(x, y, 0);

            ////当当前的还可以制造金币的时候，点击coinButton，使得16个区域块按钮的active，通过点击就可以制造金币
            //if (canMakeCoinNum <= 0)//------不知道为什么禁用以后还能点击
            //{
            //    // 设置提示框，提醒用户操作--
            //    //UnityEditor.EditorUtility.DisplayDialog("提示", "当前位置不能制造金币", "确认", "取消");
            //    //UnityEditor.EditorUtility.DisplayDialog("提示", "当前不能再制造金币了，添加金币按钮失灵了", "确认", "取消");               
            //    tipHelp.text = "正在游戏中，当前不能制造金币了---";
            //    //将添加金币的按钮禁用
            //    buttonCoin.SetActive(false);
            //    //禁用16个区域的按钮
            //    for (int i = 0; i < buttonArea.Length; i++)
            //    {
            //        buttonArea[i].SetActive(false);//16个按钮禁用，金币不能在添加
            //        //Debug.Log("完全禁用按钮" + buttonArea[i].name);
            //    }
            //}
            
        }
        else
        {
            tipText.text = "主角未进场，等待游戏开始！";
        }

    }

    //当coinButton点击，判断是否可以制造金币，如果不能，则按钮禁用
    public void clickToCoin()
    {
        buttonCoinClick = !buttonCoinClick;
        if (buttonCoinClick)
        {

            //进行判断是否已经超过最大制造数
            //当当前的还可以制造金币的时候，点击coinButton，使得16个区域块按钮的active，通过点击就可以制造金币
            if (canMakeCoinNum <= 0)//------不知道为什么禁用以后还能点击
            {
                // 设置提示框，提醒用户操作--
                //UnityEditor.EditorUtility.DisplayDialog("提示", "当前位置不能制造金币", "确认", "取消");
                //UnityEditor.EditorUtility.DisplayDialog("提示", "当前不能再制造金币了，添加金币按钮失灵了", "确认", "取消");

                tipHelp.text = "放置金币机会已用完";
                
                //禁用16个区域的按钮
                for (int i = 0; i < buttonArea.Length; i++)
                {
                    buttonArea[i].SetActive(false);//16个按钮禁用，金币不能在添加
                    //Debug.Log("完全禁用按钮" + buttonArea[i].name);
                }
                //将添加金币的按钮禁用
                buttonCoin.SetActive(false);
            }
            else
            {
                //修改按钮提示文本，
                buttonCoin.GetComponentInChildren<Text>().text = "结束添加";
                //Debug.Log("输出---coinButton点击了-恢复16个区域的按钮禁用--");
                for (int i = 0; i < buttonArea.Length; i++)
                {
                    buttonArea[i].SetActive(true);
                    //Debug.Log("恢复禁用" + buttonArea[i].name);
                    buttonArea[i].GetComponent<Button>().onClick.AddListener(ClickAreaToCoin);

                }
            }

        }
        else
        {
            buttonCoin.GetComponentInChildren<Text>().text = "添加金币";
            for (int i = 0; i < buttonArea.Length; i++)
            {
                buttonArea[i].SetActive(false);
                //buttonArea[i].GetComponent<Button>().onClick.AddListener(ClickAreaToCoin);
            }
        }
    }

    //给这十六个区域按钮绑定点击函数，点击添加金币------默认他已经添加成功了（因为如果当前可以制造的金币为0，那么该按钮就不能点击了）
    public void ClickAreaToCoin()
    {
        //通过 UnityEngine.EventSystems的底层来获取到当前点击的对象
        var button = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        Debug.Log(button.name);

        //判断当前的canMakeCoinNum,如果不为0，就可以进行执行，如果小于等于0，则返回
        if (canMakeCoinNum > 0)
        {   
            //获取当前的按钮，然后得出他的名称，
            string s = button.name;

            //对名字进行截取，获得块号进行发送
            string s1 = s.Substring(6);
            int s11 = int.Parse(s1);
            Debug.Log("按的是第" + s1 + "区域");

            bool isnew = true;//判断当前位置是否有金币了，默认可以进行添加金币

            //判断当前位置是否已经生成了金币
            for(int i = 0; i < 5; i++)
            {
                if(coinPosition[i] == s11)
                {
                    //UnityEditor.EditorUtility.DisplayDialog("提示", "当前位置已经有金币了，不能再制造金币了！", "确认", "取消");
                    tipHelp.text = "当前位置已经有金币了，不能再制造金币了！";
                    isnew = false;
                }
            }
            if (isnew)
            {
                //在对应的区域上生成一个图标（换成将图标点亮）
                Debug.Log("当前添加的是第" + (coinIndex + 1) + "个");
                int[] location = CalculateCoinPosition(s11);
                coins[coinIndex].GetComponent<RectTransform>().localPosition
                    = new Vector3(location[0], location[1], 0);
                coins[coinIndex].SetActive(true);

                //能够制造金币数目减一
                canMakeCoinNum--;
                leftCount++;

                //每次点击后更新金币数量
                gameStateText.text = "当前场中还有 " + leftCount + " 个金币";
                gameStateText2.text = "还可以制造 " + canMakeCoinNum + "个金币";
                gameStateText3.text = "玩家已经收集了 " + collectionNum + "个金币";
                //指向的金币索引+1
                coinIndex++;
                try
                {
                    //给服务器发送生成金币的区块
                    socket.Send(Encoding.ASCII.GetBytes("./coin"));
                    Thread.Sleep(20);
                    socket.Send(Encoding.ASCII.GetBytes(s1));
                    Thread.Sleep(20);
                }
                catch (Exception e)
                {
                    Debug.Log(e.Message);
                }

            }
            else
            {
                Debug.Log("当前位置不能自造金币了");
            }
        }
        else
        {
            //提示当前不能制造金币了
            //UnityEditor.EditorUtility.DisplayDialog("提示", "当前不能再制造金币了，添加金币按钮失灵了", "确认", "取消");
            Debug.Log("不能制造金币了--");
        }
    }

    private int[] CalculateCoinPosition(int indexOfBlock)
    {
        int[] result = new int[2];
        result[1] = indexOfBlock / 4 * 225 - 337;
        result[0] = indexOfBlock % 4 * (-225) + 337;
        return result;
    }
 
}

