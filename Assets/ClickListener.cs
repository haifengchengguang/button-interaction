﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClickListener : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(Click);
    }

    private void Click()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("HelloVR");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
