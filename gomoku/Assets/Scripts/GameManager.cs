using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public GameObject[] stones;
	public Sprite[] stoneSprites;

	public Sprite notAllowedSprite;

	public GameObject emptyButton;

	public GameObject startBoard;
	public Text playerPlaying;

	public Canvas canvas;

	public int size = 19;

	public Text[] listPlayers;
	public bool moveIntoCapture = false;

	private const int EMPTY_VALUE = 0;
	private const int P1_VALUE = 1;
	private const int P2_VALUE = 2;
	private const int DT_P1_VALUE = -1;
	private const int DT_P2_VALUE = -2;
	private const int DT_P_VALUE = -3;
	private const int NA_P1_VALUE = -4;
	private const int NA_P2_VALUE = -5;
	private const int NA_P_VALUE = -6;

	private int currentPlayerIndex = 0;
	private int currentPlayerVal = P1_VALUE;
	private int otherPlayerVal = P2_VALUE;
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

	void DispalyBoard() {
		for (int y = 0; y < size; y++) {
			string str = "";
			for (int x = 0; x < size; x++) {
				if (x != 0)
					str += " ";
				str += map[y, x].ToString();
			}
			Debug.Log(str);
		}
	}

	public void TryPutStone(GameObject button) {
	}

	public void DeleteStone(int yCoord, int xCoord) {
		map[yCoord, xCoord] = EMPTY_VALUE;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = null;
		button.transform.localScale = new Vector3(1, 1, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 0;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = true;
	}

	public void PutStone(int yCoord, int xCoord) {
		// Actually put the stone
		map[yCoord, xCoord] = currentPlayerVal;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = stoneSprites[currentPlayerIndex];
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 255;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;

		// check capture
		CheckStone(yCoord, xCoord);

		// End turn, next player to play
		currentPlayerIndex = 1 - currentPlayerIndex;
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;
		playerPlaying.text = "Player actually playing: Player" + currentPlayerVal;

		// TODO: update double-tree in map
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (map[y, x] != P1_VALUE && map[y, x] != P2_VALUE) {
					DeleteStone(y, x);
					UpdateDoubleTree(y, x);
					if (!moveIntoCapture)
						UpdateCaptureProhibited(y, x);
				}
			}
		}
		
		DispalyBoard();
	}

	private void CheckStone(int yCoord, int xCoord) {
		// Left
		if (xCoord - 3 >= 0 && map[yCoord, xCoord - 3] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, 0, -1, true);
		}
		// Top
		if (yCoord - 3 >= 0 && map[yCoord - 3, xCoord] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, -1, 0, true);
		}
		// Bot
		if (yCoord + 3 < size && map[yCoord + 3, xCoord] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, 1, 0, true);
		}
		// Right
		if (xCoord + 3 < size && map[yCoord, xCoord + 3] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, 0, 1, true);
		}
		// Bot right
		if (xCoord + 3 < size && yCoord + 3 < size && map[yCoord + 3, xCoord + 3] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, 1, 1, true);
		}
		// Bot left
		if (xCoord - 3 >= 0 && yCoord + 3 < size && map[yCoord + 3, xCoord - 3] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, 1, -1, true);
		}
		// Top left
		if (xCoord - 3 >= 0 && yCoord - 3 >= 0 && map[yCoord - 3, xCoord - 3] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, -1, -1, true);
		}
		// Top right
		if (xCoord + 3 < size && yCoord - 3 >= 0 && map[yCoord - 3, xCoord + 3] == currentPlayerVal) {
			CheckCapture(yCoord, xCoord, -1, 1, true);
		}
	}

	private bool CheckCapture(int yCoord, int xCoord, int yCoeff, int xCoeff, bool doCapture = false) {
		int y1 = yCoord + yCoeff * 1;
		int y2 = yCoord + yCoeff * 2;
		int x1 = xCoord + xCoeff * 1;
		int x2 = xCoord + xCoeff * 2;

		if (map[y1, x1] == otherPlayerVal && map[y2, x2] == otherPlayerVal) {
			if (doCapture) {
				DeleteStone(y1, x1);
				DeleteStone(y2, x2);
				playerScores[currentPlayerIndex] += 2;
				listPlayers[currentPlayerIndex].text = "Player" + currentPlayerVal + ": " + playerScores[currentPlayerIndex];
				if (playerScores[currentPlayerIndex] == 10) {
					// TODO: display winner and stop playing
					Debug.Log("You won");
				}
			}
			return true;
		}
		return false;
	}

	private void UpdateDoubleTree(int yCoord, int xCoord) {
		// TODO: do checks for both players

	}

	private bool CheckSelfCapture(int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal) {
		int y1 = yCoord + yCoeff * 1;
		int y2 = yCoord + yCoeff * 2;
		int y3 = yCoord + yCoeff * -1;
		int x1 = xCoord + xCoeff * 1;
		int x2 = xCoord + xCoeff * 2;
		int x3 = xCoord + xCoeff * -1;

		// Debug.Log(map[y1, x1]);
		// Debug.Log(map[y2, x2]);
		if (map[y1, x1] == myVal && map[y2, x2] == enemyVal && map[y3, x3] == enemyVal) {
			return true;
		}
		return false;
	}

	private void UpdateCaptureProhibited(int yCoord, int xCoord) {
		bool currentProhibited = false;
		bool otherProhibited = false;

		bool checkLeft = xCoord - 2 >= 0 && xCoord + 1 < size;
		bool checkRight = xCoord + 2 < size && xCoord - 1 >= 0;
		bool checkTop = yCoord - 2 >= 0 && yCoord + 1 < size;
		bool checkBot = yCoord + 2 < size && yCoord - 1 >= 0;
		// Left
		if (checkLeft) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, 0, -1, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, 0, -1, otherPlayerVal, currentPlayerVal);
		}
		// Top
		if (checkTop) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, -1, 0, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, -1, 0, otherPlayerVal, currentPlayerVal);			
		}
		// Bot
		if (checkBot) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, 1, 0, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, 1, 0, otherPlayerVal, currentPlayerVal);
		}
		// Right
		if (checkRight) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, 0, 1, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, 0, 1, otherPlayerVal, currentPlayerVal);
		}
		// Bot right
		if (checkBot && checkRight) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, 1, 1, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, 1, 1, otherPlayerVal, currentPlayerVal);
		}
		// Bot left
		if (checkBot && checkLeft) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, 1, -1, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, 1, -1, otherPlayerVal, currentPlayerVal);
		}
		// Top left
		if (checkTop && checkLeft) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, -1, -1, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, -1, -1, otherPlayerVal, currentPlayerVal);
		}
		// Top right
		if (checkTop && checkRight) {
			currentProhibited = currentProhibited || CheckSelfCapture(yCoord, xCoord, -1, 1, currentPlayerVal, otherPlayerVal);
			otherProhibited = otherProhibited || CheckSelfCapture(yCoord, xCoord, -1, 1, otherPlayerVal, currentPlayerVal);
		}

		if (currentProhibited) {
			Debug.Log("Current player can't play in " + yCoord + " " + xCoord);
				GameObject button = buttonsMap[yCoord, xCoord].gameObject;
				button.GetComponent<Image>().sprite = notAllowedSprite;
				button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
				Color buttonColor = button.GetComponent<Image>().color;
				buttonColor.a = 255;
				button.GetComponent<Image>().color = buttonColor;
				button.GetComponent<PutStone>().isEmpty = false;

				if (otherProhibited) {
					map[yCoord, xCoord] = NA_P_VALUE;
					Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
				}
				else
					map[yCoord, xCoord] = (currentPlayerIndex == 0) ? NA_P1_VALUE : NA_P2_VALUE;
		}
		else if (otherProhibited) {
			DeleteStone(yCoord, xCoord);
			map[yCoord, xCoord] = (currentPlayerIndex == 0) ? NA_P2_VALUE : NA_P1_VALUE;
			Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
		}
	}
}
