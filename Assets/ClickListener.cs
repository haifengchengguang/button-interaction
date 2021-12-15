using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
//using OpenCvSharp;//***************************************************************************************
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using System.Threading;
using OpenCVForUnity;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.ImgcodecsModule;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.UnityUtils;
//***************************************************************************************删除OpenCvSharp.dll那一套******************

public class ClickListener : MonoBehaviour
{
    //透视变换
    private List<Vector2> corners = new List<Vector2>();
    private byte[] imgBytes;
    private byte[] transformedImgBytes;
    private Texture2D transformTexture;
    private bool transformed = false;
    private bool selecting = false;

    //透视变换点选
    private bool fourSelected = false;
    Vector2 tl;
    Vector2 tr;
    Vector2 br;
    Vector2 bl;

    //墙体按钮信息
    private ColorBlock defaultColor = new ColorBlock();
    private ColorBlock clickedColor = new ColorBlock();
    public static int[] wallsState = new int[24];

    //玩家开始信息
    public static bool isVRPlayer = true;

    int count = 0;
    bool photoed = false;
    bool open = false;
    private int updatePosition = -1;


    //场景对象
    public RawImage rawImage;
    public Text tipText;
    public GameObject wallPanel;
    public Button[] wallButtons = new Button[24];

    //屏幕信息
    int Swidth, Sheight;
    int borderRawImage;

    //当前运行的相机
    private WebCamTexture currentWebCam;


    private void Awake()
    {
        XRSettings.enabled = false;
    }
    // Start is called before the first frame update
    void Start()
    {
        Swidth = Screen.width;//2160
        Sheight = Screen.height;//1080
        borderRawImage = Sheight - 280;//800
        //rawImage.GetComponent<RectTransform>().sizeDelta = new Vector2(borderRawImage, borderRawImage);
        Debug.Log(Screen.width);
        Debug.Log(Screen.height);

        //默认设置墙体按钮隐藏
        wallPanel.SetActive(false);

        //初始化提示文本
        tipText.text = Input.touchSupported.ToString();

        //初始化按钮颜色
        defaultColor.normalColor = new Color(217, 171, 171, 18);
        defaultColor.pressedColor = new Color(217, 171, 171, 18);
        defaultColor.highlightedColor = new Color(217, 171, 171, 18);
        defaultColor.disabledColor = new Color(217, 171, 171, 18);
        defaultColor.colorMultiplier = 0.15f;
        clickedColor.normalColor = new Color(0, 0, 0, 255);
        clickedColor.pressedColor = new Color(0, 0, 0, 255);
        clickedColor.highlightedColor = new Color(0, 0, 0, 255);
        clickedColor.disabledColor = new Color(0, 0, 0, 255);
        clickedColor.colorMultiplier = 1;
    }

    public IEnumerator Call()
    {
        // 请求权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam) && WebCamTexture.devices.Length > 0)
        {
            // 创建相机贴图
            currentWebCam = new WebCamTexture(WebCamTexture.devices[0].name, 400, 400, 60);
            rawImage.texture = currentWebCam;
            open = true;
            currentWebCam.Play();
            //前置后置摄像头需要旋转一定角度,否则画面是不正确的,必须置于Play()函数后
            // rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -currentWebCam.videoRotationAngle);
        }
    }

    // 开始游戏按钮监听
    public void StartGame()
    {
        //身份判定！！！传给另一个场景代码
        //传迷宫信息
        UnityEngine.SceneManagement.SceneManager.LoadScene("HelloVR");
    }

    // 拍照按钮监听
    public void TakePhoto()
    {
        if (count % 3 == 0)
        {

            StartCoroutine(Call());
            count++;
        }
        else if (count % 3 == 1)
        {

            StartCoroutine(GetTexture());
            count++;
            photoed = true;
        }
        else
        {
            currentWebCam.Stop();
            rawImage.color = new Color(255, 255, 255, 255);
            Texture2D inputTexture = new Texture2D(borderRawImage, borderRawImage);
            inputTexture.LoadImage(imgBytes);
            rawImage.texture = inputTexture;
            count++;
            open = false;
        }
    }

    //透视变换
    public void Transform()
    {
        if(photoed)
        {
            try
            {
                /*
                Debug.Log("start");
                Mat img = Imgcodecs.imread(Application.persistentDataPath + "/photo.jpg");
                Mat result = new Mat(borderRawImage, borderRawImage, CvType.CV_8UC1);
                Debug.Log("1");
                Point[] srcPoints = new Point[] {
                    new Point(50, 730),
                    new Point(750, 760),
                    new Point(770, 30),
                    new Point(10, 10),
                };
                Point[] dstPoints = new Point[] {
                    new Point(30, 770),
                    new Point(770, 770),
                    new Point(770, 30),
                    new Point(30, 30),
                };
                Debug.Log("2");
                Mat pointsMat = Imgproc.getAffineTransform(new MatOfPoint2f(srcPoints), new MatOfPoint2f(dstPoints));//卡主!!!!!!!!!!!!!!!!!!!!!!!!
                Debug.Log("3");
                Mat transformMat = Imgproc.getPerspectiveTransform(new Mat(), new Mat());
                Imgproc.warpPerspective(img, result, pointsMat, result.size());
                Debug.Log("4");
                Imgcodecs.imwrite(Application.persistentDataPath + "/transformed.jpg", result);
                Debug.Log("end");
                */
                /*
                //Mat img = new Mat(Application.persistentDataPath + "/photo.jpg");
                textTest.text = "start";
                Thread.Sleep(500);
                textTest.text = "after sleep";
                Mat img = Cv2.ImRead(Application.persistentDataPath + "/photo.jpg", ImreadModes.Unchanged);
                Mat result = new Mat(400, 400,MatType.CV_8UC1);
                var srcPoints = new Point2f[] {
                new Point2f(50, 330),
                new Point2f(350, 360),
                new Point2f(370, 30),
                new Point2f(10, 10),
                };
                //变换后的四点
                var dstPoints = new Point2f[] {
                new Point2f(30, 370),
                new Point2f(370, 370),
                new Point2f(370, 30),
                new Point2f(30, 30),
                };
                textTest.text = "point 2f";
                Thread.Sleep(500);
                //根据变换前后四个点坐标,获取变换矩阵
                Mat mm = Cv2.GetPerspectiveTransform(srcPoints, dstPoints);
                //进行透视变换
                //Cv2.WarpPerspective(ImageIn, ImageOut, mm, GrayImage.Size());
                Cv2.WarpPerspective(img, result, mm, result.Size());
                //result.ImWrite(Application.persistentDataPath + "transform_photo.jpg");
                //result.WriteToStream(new FileStream(Application.persistentDataPath + "/transform_photo.jpg", FileMode.Create));
                textTest.text = "end-1";
                Thread.Sleep(500);
                Cv2.ImWrite(Application.persistentDataPath + "/transform.jpg", result);
                textTest.text = "end";
                File.WriteAllBytes(Application.persistentDataPath+"/transformm.jpg",result.ToBytes());
                */

                /*
                Texture2D inputTexture = new Texture2D(borderRawImage, borderRawImage);
                inputTexture.LoadImage(imgBytes);
                Mat inputMat = new Mat(inputTexture.height, inputTexture.width, CvType.CV_8UC4);
                Mat outputMat = new Mat(inputTexture.height, inputTexture.width, CvType.CV_8UC4);
                Utils.texture2DToMat(inputTexture, inputMat);
                */
                //原代码-自动识别

                //Texture2D inputTexture = Resources.Load("inputTexture") as Texture2D;
                //Mat inputMat = new Mat(inputTexture.height, inputTexture.width, CvType.CV_8UC4);
                //Mat outputMat = new Mat(inputTexture.height, inputTexture.width, CvType.CV_8UC4);
                //Utils.texture2DToMat(inputTexture, inputMat);
                /*
                Imgproc.cvtColor(inputMat, outputMat, Imgproc.COLOR_RGB2GRAY);

                Imgproc.Canny(outputMat, outputMat, 100, 150);

                Mat lines = new Mat();
                //第五个参数是阈值，通过调整阈值大小可以过滤掉一些干扰线
                //Imgproc.HoughLinesP(outputMat, lines, 1, Mathf.PI / 180, 60, 50, 10);
                Imgproc.HoughLinesP(outputMat, lines, 1, Mathf.PI / 120, 100, 100, 30);
                //计算霍夫曼线外围的交叉点
                int[] linesArray = new int[lines.cols() * lines.rows() * lines.channels()];
                lines.get(0, 0, linesArray);
                Debug.Log("length of lineArray " + linesArray.Length);
                List<int> a = new List<int>();
                List<int> b = new List<int>();
                for (int i = 0; i < linesArray.Length - 4; i = i + 4)
                {
                    Imgproc.line(inputMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(0, 255, 0), 5);//绿色
                }


                Texture2D testTexture = new Texture2D(inputMat.cols(), inputMat.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(inputMat, testTexture);
                testRaw.texture = testTexture;//显示到rawImage上。


                for (int i = 0; i < linesArray.Length; i = i + 4)
                {
                    a.Add(linesArray[i + 0]);
                    a.Add(linesArray[i + 1]);
                    a.Add(linesArray[i + 2]);
                    a.Add(linesArray[i + 3]);
                    for (int j = i + 4; j < linesArray.Length; j = j + 4)
                    {
                        b.Add(linesArray[j + 0]);
                        b.Add(linesArray[j + 1]);
                        b.Add(linesArray[j + 2]);
                        b.Add(linesArray[j + 3]);

                        Vector2 temp = ComputeIntersect(a, b);//计算交点坐标
                        b.Clear();

                        if (temp.x > 0 && temp.y > 0 && temp.x < 1000 && temp.y < 1000)
                        {
                            corners.Add(temp);
                        }
                    }
                    a.Clear();
                }
                CullIllegalPoint(ref corners, 20);//剔除重合的点和不合理的点
                if (corners.Count != 4)
                {
                    Debug.Log("The object is not quadrilateral  " + corners.Count);
                }
                print("continue");
                Vector2 center = Vector2.zero;
                
                                //for (int i = 0; i < corners.Count; i++)
                                //{
                                //    center += corners[i];
                                //}
                                //center *= 0.25f;
                                //SortCorners(ref corners, center);
                

                for (int i = 0; i < 4; i++)
                {
                    center += corners[i];
                }
                center *= 0.25f;
                SortCorners(ref corners, center);
                
                //计算转换矩阵
                Vector2 tl = corners[0];
                Vector2 tr = corners[1];
                Vector2 br = corners[2];
                Vector2 bl = corners[3];
                */

                /*
                //以上部分是自动识别四个点的代码，但是好像有一点问题。
                //测试像素点--真正用手动/自动还要改代码，这些数值是因为目前图片大小设置的是800。tl=top_left点,br=bottom_right点。
                Vector2 tl = new Vector2(200,600);
                Vector2 tr = new Vector2(500,700);
                Vector2 br = new Vector2(700,200);
                Vector2 bl = new Vector2(100,50);
                */

                updatePosition = 0;
                //Thread thread = new Thread(waitForSelect);
                //thread.Start(inputMat);
                //StartCoroutine(waitForSelect(inputMat));
                /*
                Vector2 tl = Input.mousePosition;
                Vector2 tr = Input.mousePosition;
                Vector2 br = Input.mousePosition;
                Vector2 bl = Input.mousePosition;
                */
                /*
                Vector2 tl = Input.GetTouch(0).position;
                Vector2 tr = Input.GetTouch(0).position;
                Vector2 br = Input.GetTouch(0).position;
                Vector2 bl = Input.GetTouch(0).position;
                */
                /*
                print(tl.x + "  " + tl.y);
                print(tr.x + "  " + tr.y);
                print(br.x + "  " + br.y);
                print(bl.x + "  " + bl.y);
                //Vector2 tl = new Vector2(200, 600);
                //Vector2 tr = new Vector2(500, 700);
                //Vector2 br = new Vector2(700, 200);
                //Vector2 bl = new Vector2(100, 50);

                Mat srcRectMat = new Mat(4, 1, CvType.CV_32FC2);
                Mat dstRectMat = new Mat(4, 1, CvType.CV_32FC2);

                srcRectMat.put(0, 0, tl.x, tl.y, tr.x, tr.y, br.x, br.y, bl.x, bl.y);
                //dstRectMat.put(0, 0, 0.0, inputMat.rows(), inputMat.cols(), inputMat.rows(), inputMat.rows(), 0, 0.0, 0.0);
                dstRectMat.put(0, 0, 100, 700, 700, 700, 700, 100, 100, 100);

                Mat perspectiveTransform = Imgproc.getPerspectiveTransform(srcRectMat, dstRectMat);
                Mat outputMat0 = inputMat.clone();
                /*
                //圈出四个顶点
                Point t = new Point(tl.x, tl.y);
                Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);
                t = new Point(tr.x, tr.y);
                Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);
                t = new Point(bl.x, bl.y);
                Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);
                t = new Point(br.x, br.y);
                Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);

                print("circled");
                */
                //进行透视转换
                /*
                Imgproc.warpPerspective(inputMat, outputMat0, perspectiveTransform, new Size(inputMat.rows(), inputMat.cols()));

                Texture2D outputTexture = new Texture2D(outputMat0.cols(), outputMat0.rows(), TextureFormat.RGBA32, false);
                Utils.matToTexture2D(outputMat0, outputTexture);

                rawImage.texture = outputTexture;//显示到rawImage上。
                                                 //rawImage.material.mainTexture = outputTexture;
                                                 //transformTexture = outputTexture;
                                                 //transformed = true;
                                                 //gameObject.GetComponent<Renderer>().material.mainTexture = outputTexture;
                transformedImgBytes = outputTexture.EncodeToJPG();
                File.WriteAllBytes(Application.persistentDataPath + "/transformed.jpg", transformedImgBytes);
                fourSelected = false;
                */
            }
            catch (Exception e)
            {

            }
        }
    }

    private void ContinueTransform()
    {
        Texture2D inputTexture = new Texture2D(borderRawImage, borderRawImage);
        inputTexture.LoadImage(imgBytes);
        Mat inputMat = new Mat(inputTexture.height, inputTexture.width, CvType.CV_8UC4);
        Mat outputMat = new Mat(inputTexture.height, inputTexture.width, CvType.CV_8UC4);
        Utils.texture2DToMat(inputTexture, inputMat);

        Imgproc.cvtColor(inputMat, outputMat, Imgproc.COLOR_RGB2GRAY);

        Imgproc.Canny(outputMat, outputMat, 100, 150);

        Mat lines = new Mat();
        //第五个参数是阈值，通过调整阈值大小可以过滤掉一些干扰线
        //Imgproc.HoughLinesP(outputMat, lines, 1, Mathf.PI / 180, 60, 50, 10);
        Imgproc.HoughLinesP(outputMat, lines, 1, Mathf.PI / 120, 50, 50, 30);
        //计算霍夫曼线外围的交叉点
        int[] linesArray = new int[lines.cols() * lines.rows() * lines.channels()];
        lines.get(0, 0, linesArray);
        
        for (int i = 0; i < linesArray.Length - 4; i = i + 4)
        {
            Imgproc.line(inputMat, new Point(linesArray[i + 0], linesArray[i + 1]), new Point(linesArray[i + 2], linesArray[i + 3]), new Scalar(0, 255, 0, 255), 5);//绿色
        }
        /*
        //为了检测是不是没点准的问题，画一下自己点的位置。
        Imgproc.circle(inputMat, new Point(tl.x, 800-tl.y), 5, new Scalar(255, 0, 0));
        Imgproc.circle(inputMat, new Point(tr.x, 800-tr.y), 5, new Scalar(255, 0, 0));
        Imgproc.circle(inputMat, new Point(br.x, 800-br.y), 5, new Scalar(255, 0, 0));
        Imgproc.circle(inputMat, new Point(bl.x, 800-bl.y), 5, new Scalar(255, 0, 0));
        */
        Texture2D testT = new Texture2D(borderRawImage, borderRawImage, TextureFormat.RGBA32, false);
        Utils.matToTexture2D(inputMat, testT);
        byte[] testImg = testT.EncodeToJPG();
        File.WriteAllBytes(Application.persistentDataPath + "/selectCircle.jpg", testImg);

        print(tl.x + "  " + tl.y);
        print(tr.x + "  " + tr.y);
        print(br.x + "  " + br.y);
        print(bl.x + "  " + bl.y);

        //Vector2 tl = new Vector2(200, 600);
        //Vector2 tr = new Vector2(500, 700);
        //Vector2 br = new Vector2(700, 200);
        //Vector2 bl = new Vector2(100, 50);

        Mat srcRectMat = new Mat(4, 1, CvType.CV_32FC2);
        Mat dstRectMat = new Mat(4, 1, CvType.CV_32FC2);

        //srcRectMat.put(0, 0, tl.x, tl.y, tr.x, tr.y, br.x, br.y, bl.x, bl.y);
        srcRectMat.put(0, 0, bl.x, bl.y, br.x, br.y, tr.x, tr.y, tl.x, tl.y);
        //dstRectMat.put(0, 0, 0.0, inputMat.rows(), inputMat.cols(), inputMat.rows(), inputMat.rows(), 0, 0.0, 0.0);
        dstRectMat.put(0, 0, 100, 700, 700, 700, 700, 100, 100, 100);

        Mat perspectiveTransform = Imgproc.getPerspectiveTransform(srcRectMat, dstRectMat);
        Mat outputMat0 = inputMat.clone();
        /*
        //圈出四个顶点
        Point t = new Point(tl.x, tl.y);
        Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);
        t = new Point(tr.x, tr.y);
        Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);
        t = new Point(bl.x, bl.y);
        Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);
        t = new Point(br.x, br.y);
        Imgproc.circle(outputMat0, t, 10, new Scalar(0, 0, 255, 255), 2);

        print("circled");
        */
        //进行透视转换
        Imgproc.warpPerspective(inputMat, outputMat0, perspectiveTransform, new Size(inputMat.rows(), inputMat.cols()));

        Texture2D outputTexture = new Texture2D(outputMat0.cols(), outputMat0.rows(), TextureFormat.RGBA32, false);
        Utils.matToTexture2D(outputMat0, outputTexture);

        rawImage.texture = outputTexture;//显示到rawImage上。
                                         //rawImage.material.mainTexture = outputTexture;
                                         //transformTexture = outputTexture;
                                         //transformed = true;
                                         //gameObject.GetComponent<Renderer>().material.mainTexture = outputTexture;
        transformedImgBytes = outputTexture.EncodeToJPG();
        File.WriteAllBytes(Application.persistentDataPath + "/transformed.jpg", transformedImgBytes);
        fourSelected = false;
        print("end of Continue()");
    }

    public IEnumerator GetTexture()
    {
        yield return new WaitForEndOfFrame();
        Texture2D texture2D = new Texture2D(borderRawImage, borderRawImage);
        texture2D.ReadPixels(new UnityEngine.Rect(300, 140, borderRawImage, borderRawImage), 0, 0, false);
        texture2D.Apply();
        imgBytes = texture2D.EncodeToJPG();
        File.WriteAllBytes(Application.persistentDataPath + "/photo.jpg", imgBytes);
        currentWebCam.Play();
    }

    public void AutoSelect()
    {
        Texture2D img = new Texture2D(800, 800, TextureFormat.RGBA32, false);
        img.LoadImage(transformedImgBytes);
        //print(img.GetPixel(175, 245).r*255 + "       " + img.GetPixel(175, 245).g*255 + "      " + img.GetPixel(175, 245).b*255);
        for (int i = 0; i < 24; i++)
        {
            wallsState[i] = 0;
            wallButtons[i].colors = defaultColor;
        }
        for( int i = 0; i < 24; i++)
        {
            if(i <= 11)//row
            {
                int x = 175 + 150 * (i % 4);
                int y = 250 + 150 * (i / 4);
                Color thisPointColor = img.GetPixel(x, y);
                if (thisPointColor.r * 255 <= 40 && thisPointColor.g * 255 >= 235 && thisPointColor.b * 255 <= 40)
                {
                    wallButtons[i].colors = clickedColor;
                    wallsState[i] = 1;
                }
            }
            else//column
            {
                int x = 250 + 150 * ((i-12) % 3);
                int y = 175 + 150 * ((i-12) / 3);
                Color thisPointColor = img.GetPixel(x, y);
                if (thisPointColor.r * 255 <= 40 && thisPointColor.g * 255 >= 235 && thisPointColor.b * 255 <= 40)
                {
                    wallButtons[i].colors = clickedColor;
                    wallsState[i] = 1;
                }
            }
        }
        wallPanel.SetActive(true);
    }

    public void SelectWall()
    {
        print("点击了手选");
        selecting = !selecting;
        if(selecting)
        {
            wallPanel.SetActive(true);
        }
        else
        {
            wallPanel.SetActive(false);
        }
    }


    public void Reset()
    {
        for(int i = 0; i < 24; i++)
        {
            wallButtons[i].colors = defaultColor;
            wallsState[i] = 0;
        }
    }








    // Update is called once per frame
    void Update()
    {
        
        if(updatePosition == 0 && Input.GetMouseButtonDown(0))
        {
            tl.x = Input.mousePosition.x - 300;
            tl.y = Input.mousePosition.y - 140;
            updatePosition++;
            print(tl.x + "  " + tl.y);
        }
        else if(updatePosition == 1 && Input.GetMouseButtonDown(0))
        {
            tr.x = Input.mousePosition.x - 300;
            tr.y = Input.mousePosition.y - 140;
            updatePosition++;
            print(tr.x + "  " + tr.y);
        }
        else if(updatePosition == 2 && Input.GetMouseButtonDown(0))
        {
            br.x = Input.mousePosition.x - 300;
            br.y = Input.mousePosition.y - 140;
            updatePosition++;
            print(br.x + "  " + br.y);
        }
        else if(updatePosition == 3 && Input.GetMouseButtonDown(0))
        {
            bl.x = Input.mousePosition.x - 300;
            bl.y = Input.mousePosition.y - 140;
            updatePosition = 4;
            fourSelected = true;
            ContinueTransform();
            print(bl.x + "  " + bl.y);
        }
    }

    private void OnGUI()
    {
        if(open)
        {
            GUI.DrawTexture(new UnityEngine.Rect(300, 140, borderRawImage, borderRawImage),currentWebCam,ScaleMode.StretchToFill);
        }
        if(transformed)
        {
            //GUI.DrawTexture(new UnityEngine.Rect(300, 140, borderRawImage, borderRawImage), transformTexture, ScaleMode.StretchToFill);
        }
    }
















    private Vector2 ComputeIntersect(List<int> a, List<int> b)
    {
        int x1 = a[0], y1 = a[1], x2 = a[2], y2 = a[3];
        int x3 = b[0], y3 = b[1], x4 = b[2], y4 = b[3];
        float d = ((float)(x1 - x2) * (y3 - y4)) - (x3 - x4) * (y1 - y2);
        Vector2 temp = Vector2.zero;
        if (d == 0)
        {
            temp.x = -1;
            temp.y = -1;
        }
        else
        {
            temp.x = ((x1 * y2 - y1 * x2) * (x3 - x4) - (x1 - x2) * (x3 * y4 - y3 * x4)) / d;
            temp.y = ((x1 * y2 - y1 * x2) * (y3 - y4) - (y1 - y2) * (x3 * y4 - y3 * x4)) / d;
        }
        return temp;
    }

    private void CullIllegalPoint(ref List<Vector2> corners, float minDis)
    {
        Vector2 a = Vector2.zero;
        Vector2 b = Vector2.zero;
        List<Vector2> removeList = new List<Vector2>();
        for (int i = 0; i < corners.Count; i++)
        {
            a = corners[i];
            for (int j = i + 1; j < corners.Count; j++)
            {
                b = corners[j];
                if (Vector2.Distance(a, b) < minDis)
                {
                    removeList.Add(b);
                }
            }
        }
        for (int i = 0; i < removeList.Count; i++)
        {
            corners.Remove(removeList[i]);
        }
    }

    private void SortCorners(ref List<Vector2> corners, Vector2 center)
    {
        List<Vector2> top = new List<Vector2>();
        List<Vector2> bot = new List<Vector2>();
        for (int i = 0; i < corners.Count; i++)
        {

            if (corners[i].y > center.y)
                top.Add(corners[i]);
            else
                bot.Add(corners[i]);
        }
        if (top.Count < 2)
        {
            Vector2 temp = GetMaxFromList(bot);
            top.Add(temp);
            bot.Remove(temp);
        }
        if (top.Count > 2)
        {
            Vector2 temp = GetMinFromList(top);
            top.Remove(temp);
            bot.Add(temp);
        }
        Vector2 tl = top[0].x > top[1].x ? top[1] : top[0];
        Vector2 tr = top[0].x > top[1].x ? top[0] : top[1];
        Vector2 bl = bot[0].x > bot[1].x ? bot[1] : bot[0];
        Vector2 br = bot[0].x > bot[1].x ? bot[0] : bot[1];
        corners.Clear();
        corners.Add(tl);
        corners.Add(tr);
        corners.Add(br);
        corners.Add(bl);
    }

    private Vector2 GetMaxFromList(List<Vector2> list)
    {
        Vector2 temp = list[0];
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].y > temp.y)
            {
                temp = list[i];
            }
        }
        return temp;
    }

    private Vector2 GetMinFromList(List<Vector2> list)
    {
        Vector2 temp = list[0];
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].y < temp.y)
            {
                temp = list[i];
            }
        }
        return temp;
    }

    public void WallButtonClick()
    {
        print("点击了按钮");
        //获得按钮
        GameObject theButton = UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject;
        int index = Convert.ToInt32(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject.name.Substring(0));
        print("stateColor"+wallsState[index]+"  "+wallButtons[index].colors.normalColor.r);

        //更新状态信息
        if (wallsState[index] == 1)
        {
            wallButtons[index].colors = defaultColor;
            wallsState[index] = 0;
        }
        else
        {
            wallButtons[index].colors = clickedColor;
            wallsState[index] = 1;
        }
    }
}
