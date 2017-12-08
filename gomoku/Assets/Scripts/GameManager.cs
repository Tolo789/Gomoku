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

	public Text[] listPlayers;

	private const int EMPTY_VALUE = -1;

	private int playerPlaying = 0;
	private int[,] map;
	private PutStone[,] buttonsMap;
	private int[] playerScores;

	// Use this for initialization
	void Start () {
		// init game variables
		map = new int[size, size];
		buttonsMap = new PutStone[size, size];
		playerScores = new int[stoneSprites.Length];
		for (int i = 0; i < playerScores.Length; i++) {
			playerScores[i] = 0;
		}

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
				map[y, x] = EMPTY_VALUE;
				GameObject newButton = Instantiate(emptyButton, tmpPos, Quaternion.identity);
				newButton.name = y + "-" + x;
				newButton.transform.SetParent(startBoard.transform);
				newButton.transform.localScale = emptyButton.transform.localScale;
				newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonSize, buttonSize);
				buttonsMap[y,x] = newButton.GetComponent<PutStone>();
				buttonsMap[y,x].gameManager = this;
				x++;
				tmpPos.x += step;
			}
			y++;
			tmpPos.y -= step;
			tmpPos.x = startPos.x;
			x = 0;
		}
		// InitBoard();
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

	void InitBoard() {
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				Debug.Log(map[y, x]);
			}
		}
	}

	public void TryPutStone(GameObject button) {
		string[] coords = button.name.Split('-');
		int yCoord = int.Parse(coords[0]);
		int xCoord = int.Parse(coords[1]);
		if (!validMove(button.name))
			return;
		PutStone(yCoord, xCoord);
	}

	private bool validMove(string position) {
		// TODO
		return true;
	}

	public void DeleteStone(int yCoord, int xCoord) {
		map[yCoord, xCoord] = -1;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = null;
		button.transform.localScale = new Vector3(1, 1, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 0;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = true;
	}

	private void PutStone(int yCoord, int xCoord) {
		// Actually put the stone
		map[yCoord, xCoord] = playerPlaying;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = stoneSprites[playerPlaying];
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 255;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;

		// check capture
		CheckStone(yCoord, xCoord);

		// End turn, next player to play
		playerPlaying = (playerPlaying + 1) % stoneSprites.Length;
	}

	private void CheckStone(int yCoord, int xCoord) {
		// Left
		if (xCoord - 3 >= 0 && map[yCoord, xCoord - 3] == playerPlaying) {
			CheckCapture(yCoord, xCoord, 0, -1, true);
		}
		// Top
		if (yCoord - 3 >= 0 && map[yCoord - 3, xCoord] == playerPlaying) {
			CheckCapture(yCoord, xCoord, -1, 0, true);
		}
		// Bot
		if (yCoord + 3 < size && map[yCoord + 3, xCoord] == playerPlaying) {
			CheckCapture(yCoord, xCoord, 1, 0, true);
		}
		// Right
		if (xCoord + 3 < size && map[yCoord, xCoord + 3] == playerPlaying) {
			CheckCapture(yCoord, xCoord, 0, 1, true);
		}
		// Bot right
		if (xCoord + 3 < size && yCoord + 3 < size && map[yCoord + 3, xCoord + 3] == playerPlaying) {
			CheckCapture(yCoord, xCoord, 1, 1, true);
		}
		// Bot left
		if (xCoord - 3 >= 0 && yCoord + 3 < size && map[yCoord + 3, xCoord - 3] == playerPlaying) {
			CheckCapture(yCoord, xCoord, 1, -1, true);
		}
		// Top left
		if (xCoord - 3 >= 0 && yCoord - 3 >= 0 && map[yCoord - 3, xCoord - 3] == playerPlaying) {
			CheckCapture(yCoord, xCoord, -1, -1, true);
		}
		// Top right
		if (xCoord + 3 < size && yCoord - 3 >= 0 && map[yCoord - 3, xCoord + 3] == playerPlaying) {
			CheckCapture(yCoord, xCoord, -1, 1, true);
		}
	}

	private bool CheckCapture(int yCoord, int xCoord, int yCoeff, int xCoeff, bool doCapture = false) {
		int y1 = yCoord + yCoeff * 1;
		int y2 = yCoord + yCoeff * 2;
		int x1 = xCoord + xCoeff * 1;
		int x2 = xCoord + xCoeff * 2;

		if (map[y1, x1] != playerPlaying && map[y2, x2] != playerPlaying) {
			if (map[y1, x1] != EMPTY_VALUE && map[y2, x2] != EMPTY_VALUE) {
				if (doCapture) {
					DeleteStone(y1, x1);
					DeleteStone(y2, x2);
					playerScores[playerPlaying] += 2;
					listPlayers[playerPlaying].text = "Player" + (playerPlaying + 1) + ": " + playerScores[playerPlaying];
					if (playerScores[playerPlaying] == 10) {
						Debug.Log("You won");
					}
				}
				return true;
			}
		}
		return false;
	}
}
