using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public GameObject[] stones;
	public Sprite[] stoneSprites;

	public GameObject emptyButton;

	public GameObject startBoard;

	public Canvas canvas;

	public int size = 19;

	private int playerPlaying = 0;
	private int[][] map;

	// Use this for initialization
	void Start () {
		// init board with hidden buttons
		float width = startBoard.GetComponent<RectTransform>().rect.width ;
		float height = startBoard.GetComponent<RectTransform>().rect.height;
		Vector3 startPos = startBoard.transform.position;
		startPos.x -= width * canvas.transform.localScale.x / 2;
		startPos.y += height * canvas.transform.localScale.x / 2;
		float step = width * canvas.transform.localScale.x / (size - 1);
		float buttonSize = width / (size - 1);
		int x = 0;
		int y = 0;
		Vector3 tmpPos = startPos;
		while (y < size) {
			while (x < size) {
				GameObject newButton = Instantiate(emptyButton, tmpPos, Quaternion.identity);
				newButton.name = y + "-" + x;
				newButton.transform.SetParent(startBoard.transform);
				newButton.transform.localScale = emptyButton.transform.localScale;
				newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonSize, buttonSize);
				newButton.GetComponent<PutStone>().gameManager = this;
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
		// if (Input.GetMouseButtonDown(0)) {
		// 	Debug.Log("Mouse click");
		// 	Vector3 pos = Input.mousePosition;
		// 	pos = Camera.main.ScreenToWorldPoint(pos);
		// 	pos.z = 0;
		// 	// Instantiate(stones[0], pos, Quaternion.identity);

		// }
	}

	public void TryPutStone(GameObject button) {
		Debug.Log(button.name);
		button.GetComponent<Image>().sprite = stoneSprites[playerPlaying];
		playerPlaying = (playerPlaying + 1) % stoneSprites.Length;
		button.GetComponent<PutStone>().enabled = false;
	}
}
