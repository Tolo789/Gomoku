using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject stones;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Debug.Log("Mouse click");
			Vector3 pos = Input.mousePosition;
			pos = Camera.main.ScreenToWorldPoint(pos);
			pos.z = 0;
			Instantiate(stones, pos, Quaternion.identity);
		}
	}
}
