using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public GameObject[] stones;
	public GameObject emptyButton;

	public GameObject startBoard;

	public Canvas canvas;

	public int size = 19;

	private int playerPlaying = 0;
	private int[][] map;

	// Use this for initialization
	void Start () {
		float width = startBoard.GetComponent<RectTransform>().rect.width * canvas.transform.localScale.x;
		float height = startBoard.GetComponent<RectTransform>().rect.height * canvas.transform.localScale.x;
		Vector3 startPos = startBoard.transform.position;
		startPos.x -= width / 2;
		startPos.y += height / 2;
		// Vector3 pos = startBoard.transform.position;
		Debug.Log(canvas.transform.localScale.x);
		// Debug.Log(width);
		float step = width / (size - 1);
		Debug.Log("step: " + step);
		// pos.x = pos.x + step;
		int x = 0;
		int y = 0;
		Vector3 tmpPos = startPos;
		while (y < size) {
			while (x < size) {
				GameObject newButton = Instantiate(emptyButton, tmpPos, Quaternion.identity);
				newButton.transform.SetParent(startBoard.transform);
				newButton.transform.localScale = emptyButton.transform.localScale;
				x++;
				tmpPos.x += step;
			}
			y++;
			tmpPos.y -= step;
			tmpPos.x = startPos.x;
			x = 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)) {
			Debug.Log("Mouse click");
			Vector3 pos = Input.mousePosition;
			pos = Camera.main.ScreenToWorldPoint(pos);
			pos.z = 0;
			// Instantiate(stones[0], pos, Quaternion.identity);

		}
	}
}
