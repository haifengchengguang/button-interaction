using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class demotwo : MonoBehaviour {

	// Use this for initialization
	public GameObject[] WallObjs;
	public int[] WallData = {1,0,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1,1};
	public GameObject Coin;

	// Use this for initialization
	void Start () {
		InitMap();
	}

	void InitMap() {
		for(int i = 0; i < WallData.Length; i++) {
			if(WallData[i] != 1) {
				WallObjs[i].SetActive(false);
			} 
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
