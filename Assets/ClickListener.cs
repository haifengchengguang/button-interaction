using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ClickListener : MonoBehaviour
{

    int count = 0;
    // 图片组件
    public RawImage rawImage;

    //图形组件父实体
    public RectTransform imageParent;

    //当前相机索引
    private int index = 0;

    //当前运行的相机
    private WebCamTexture currentWebCam;

    // Start is called before the first frame update
    void Start()
    {
        // GetComponent<Button>().onClick.AddListener(startGame);
    }

    public IEnumerator Call()
    {
        // 请求权限
        yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
        if (Application.HasUserAuthorization(UserAuthorization.WebCam) && WebCamTexture.devices.Length > 0)
        {
            // 创建相机贴图
            currentWebCam = new WebCamTexture(WebCamTexture.devices[index].name, 400, 400, 60);
            rawImage.texture = currentWebCam;
            currentWebCam.Play();
            //前置后置摄像头需要旋转一定角度,否则画面是不正确的,必须置于Play()函数后
            // rawImage.rectTransform.localEulerAngles = new Vector3(0, 0, -currentWebCam.videoRotationAngle);
        }
    }

    // 开始游戏按钮监听
    public void startGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("HelloVR");
    }

    // 拍照按钮监听
    public void takePhoto()
    {
        if (count % 2 == 0)
        {
            StartCoroutine(Call());
            count++;
        }
        else
        {
            currentWebCam.Pause();
            StartCoroutine(getTexture());
            count++;
        }
    }

    private IEnumerator getTexture()
    {
        yield return new WaitForEndOfFrame();
        Texture2D texture2D = new Texture2D(400, 400);
        texture2D.ReadPixels(new Rect(200, 80, 400, 400), 0, 0, false);
        texture2D.Apply();
        byte[] byt = texture2D.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/photo.jpg", byt);

        Debug.Log(Application.dataPath);
    }

    // 
    // Update is called once per frame
    void Update()
    {
        
    }
}
