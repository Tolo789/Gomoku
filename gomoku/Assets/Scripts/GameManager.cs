using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	public Sprite[] stoneSprites;

	public Sprite notAllowedSprite;

	public GameObject emptyButton;

	public GameObject startBoard;
	public Text playerPlaying;

	public GameObject playSettings;

	public Canvas canvas;

	public int size = 19;

	public Text[] listPlayers;
	public bool moveIntoCapture = false;
	[HideInInspector]
	public bool isGameEnded = false;

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
	private bool[] isHumanPlayer;
	private bool[] isAIPlaying;
	private int playerStarting;
	private List<Vector2Int> counterMoves;

	void Start () {
		// init game variables
		map = new int[size, size];
		buttonsMap = new PutStone[size, size];
		playerScores = new int[2];
		for (int i = 0; i < playerScores.Length; i++) {
			playerScores[i] = 0;
		}
		isHumanPlayer = new bool[2];
		isHumanPlayer[0] = true;
		isAIPlaying = new bool[2];
		isAIPlaying[0] = false;
		isAIPlaying[1] = false;
		counterMoves = new List<Vector2Int>();

		// Handle who starts first
		if (PlayerPrefs.HasKey(CommonDefines.VERSUS_IA) && PlayerPrefs.GetInt(CommonDefines.VERSUS_IA) == 1) {
			isHumanPlayer[1] = false;
		}
		else {
			isHumanPlayer[1] = true;
		}

		if (PlayerPrefs.HasKey(CommonDefines.FIRST_PLAYER_PLAYING)) {
			currentPlayerIndex = PlayerPrefs.GetInt(CommonDefines.FIRST_PLAYER_PLAYING);
			if (currentPlayerIndex == 2) {
				 currentPlayerIndex = Random.Range(0, 1);
			}
		}
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;
		playerPlaying.text = "Player actually playing: Player" + currentPlayerVal;

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
			tmpPos.x = startPos.x;
			x = 0;
			while (x < size) {
				map[y, x] = EMPTY_VALUE;
				GameObject newButton = GameObject.Instantiate(emptyButton, tmpPos, Quaternion.identity);
				newButton.transform.position = tmpPos;
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
		}
	}
	
	void Update () {
		if (!isGameEnded && !isHumanPlayer[currentPlayerIndex] && !isAIPlaying[currentPlayerIndex]) {
			isAIPlaying[currentPlayerIndex] = true;

			// start AI decision making
			StartCoroutine(StartMinMax());			
		}
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

	public bool IsHumanTurn() {
		return isHumanPlayer[currentPlayerIndex];
	}

#region MainFunctions
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

		// check captures
		CheckCaptures(yCoord, xCoord, currentPlayerVal, otherPlayerVal, doCapture: true);
		if (isGameEnded)
			return;

		// If player needed to play a counter move and didnt do it, then he has lost
		if (counterMoves.Count != 0) {
			bool hasCountered = false;
			foreach (Vector2Int counterMove in counterMoves) {
				if (counterMove.x == xCoord && counterMove.y == yCoord) {
					hasCountered = true;
					break;
				}
			}
			if (hasCountered) {
				counterMoves.Clear();
			}
			else {
				DisplayWinner(1 - currentPlayerIndex);
				return;
			}
		}

		// check if win by allignement
		if (IsWinByAlignment(yCoord, xCoord)) {
			DisplayWinner(currentPlayerIndex);
			return;
		}

		// End turn, next player to play
		currentPlayerIndex = 1 - currentPlayerIndex;
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;
		playerPlaying.text = "Player actually playing: Player" + currentPlayerVal;

		// update allowed movements in map
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (map[y, x] != P1_VALUE && map[y, x] != P2_VALUE) {
					DeleteStone(y, x);
					if (x > 0 && x < size -1 && y > 0 && y < size -1) // Can't have a free-tree in the borders
						UpdateDoubleThree(y, x);
					if (!moveIntoCapture)
						UpdateSelfCapture(y, x);
				}
			}
		}
		
		// DispalyBoard();
	}

	private void DeleteStone(int yCoord, int xCoord) {
		map[yCoord, xCoord] = EMPTY_VALUE;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = null;
		button.transform.localScale = new Vector3(1, 1, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 0;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = true;
	}

	private void DisplayWinner(int winnerIndex) {
		// TODO: display winner and stop playing
		int winner = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		playSettings.SetActive(true);
		Debug.Log("Player " + winner + " won !");
		isGameEnded = true;
	}

	#endregion

#region AI
	private IEnumerator StartMinMax() {
		Debug.Log("Start MinMax");
		yield return new WaitForSeconds(3.0f);

		// TODO: Take best move and play it
		bool found = false;
		List<int> allowedSpaces = new List<int>();
		allowedSpaces.Add(EMPTY_VALUE);
		if (currentPlayerIndex == 0) {
			allowedSpaces.Add(DT_P2_VALUE);
			allowedSpaces.Add(NA_P2_VALUE);
		}
		else {
			allowedSpaces.Add(DT_P1_VALUE);
			allowedSpaces.Add(NA_P1_VALUE);
		}

		int bestY = 0;
		int bestX = 0;

		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (allowedSpaces.Contains(map[y, x])) {
					found = true;
					bestY = y;
					bestX = x;
					break;
				}
			}
			if (found)
				break;
		}

		Debug.Log("End MinMax");
		isAIPlaying[currentPlayerIndex] = false;
		PutStone(bestY, bestX);
	}
	#endregion

#region Captures
	private bool CheckCaptures(int yCoord, int xCoord, int myVal, int enemyVal, bool doCapture = true) {
		bool canCapture = false;

		// Left
		if (xCoord - 3 >= 0 && map[yCoord, xCoord - 3] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, 0, -1, enemyVal, doCapture) || canCapture;
		}
		// Top
		if (yCoord - 3 >= 0 && map[yCoord - 3, xCoord] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, -1, 0, enemyVal, doCapture) || canCapture;
		}
		// Bot
		if (yCoord + 3 < size && map[yCoord + 3, xCoord] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, 1, 0, enemyVal, doCapture) || canCapture;
		}
		// Right
		if (xCoord + 3 < size && map[yCoord, xCoord + 3] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, 0, 1, enemyVal, doCapture) || canCapture;
		}
		// Bot right
		if (xCoord + 3 < size && yCoord + 3 < size && map[yCoord + 3, xCoord + 3] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, 1, 1, enemyVal, doCapture) || canCapture;
		}
		// Bot left
		if (xCoord - 3 >= 0 && yCoord + 3 < size && map[yCoord + 3, xCoord - 3] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, 1, -1, enemyVal, doCapture) || canCapture;
		}
		// Top left
		if (xCoord - 3 >= 0 && yCoord - 3 >= 0 && map[yCoord - 3, xCoord - 3] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, -1, -1, enemyVal, doCapture) || canCapture;
		}
		// Top right
		if (xCoord + 3 < size && yCoord - 3 >= 0 && map[yCoord - 3, xCoord + 3] == myVal) {
			canCapture = CanCapture(yCoord, xCoord, -1, 1, enemyVal, doCapture) || canCapture;
		}

		return canCapture;
	}

	private bool CanCapture(int yCoord, int xCoord, int yCoeff, int xCoeff, int enemyVal, bool doCapture = false) {
		int y1 = yCoord + yCoeff * 1;
		int y2 = yCoord + yCoeff * 2;
		int x1 = xCoord + xCoeff * 1;
		int x2 = xCoord + xCoeff * 2;

		if (map[y1, x1] == enemyVal && map[y2, x2] == enemyVal) {
			if (doCapture) {
				DeleteStone(y1, x1);
				DeleteStone(y2, x2);
				playerScores[currentPlayerIndex] += 2;
				listPlayers[currentPlayerIndex].text = "Player" + currentPlayerVal + ": " + playerScores[currentPlayerIndex];
				if (playerScores[currentPlayerIndex] == 10) {
					// CODE ICI
					DisplayWinner(currentPlayerIndex);
				}
			}
			return true;
		}
		return false;
	}
	#endregion

#region FreeTree
	private void UpdateDoubleThree(int yCoord, int xCoord) {
		// TODO: do checks for both players

		// check in every direction and count number of free three, if n >= 2 break and add prohibition
		int currentPlayerFreeTree = 0;
		int otherPlayerFreeTree = 0;

		// Left
		if (IsFreeThree(yCoord, xCoord, 0, -1, currentPlayerVal, otherPlayerVal, true)) {
			currentPlayerFreeTree++;
		}
		if (IsFreeThree(yCoord, xCoord, 0, -1, otherPlayerVal, currentPlayerVal, true)) {
			otherPlayerFreeTree++;
		}
		// Right
		if (IsFreeThree(yCoord, xCoord, 0, 1, currentPlayerVal, otherPlayerVal)) {
			currentPlayerFreeTree++;
		}
		if (IsFreeThree(yCoord, xCoord, 0, 1, otherPlayerVal, currentPlayerVal)) {
			otherPlayerFreeTree++;
		}
		// Top
		if (currentPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, -1, 0, currentPlayerVal, otherPlayerVal, true)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, -1, 0, otherPlayerVal, currentPlayerVal, true)) {
			otherPlayerFreeTree++;
		}
		// Bot
		if (currentPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, 1, 0, currentPlayerVal, otherPlayerVal)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, 1, 0, otherPlayerVal, currentPlayerVal)) {
			otherPlayerFreeTree++;
		}
		// Top Left
		if (currentPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, -1, -1, currentPlayerVal, otherPlayerVal, true)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, -1, -1, otherPlayerVal, currentPlayerVal, true)) {
			otherPlayerFreeTree++;
		}
		// Top Right
		if (currentPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, -1, 1, currentPlayerVal, otherPlayerVal, true)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, -1, 1, otherPlayerVal, currentPlayerVal, true)) {
			otherPlayerFreeTree++;
		}
		// Bot Left
		if (currentPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, 1, -1, currentPlayerVal, otherPlayerVal)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, 1, -1, otherPlayerVal, currentPlayerVal)) {
			otherPlayerFreeTree++;
		}
		// Bot Right
		if (currentPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, 1, 1, currentPlayerVal, otherPlayerVal)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(yCoord, xCoord, 1, 1, otherPlayerVal, currentPlayerVal)) {
			otherPlayerFreeTree++;
		}

		// Is not double-three if there is a capture
		if (currentPlayerFreeTree == 2 && CheckCaptures(yCoord, xCoord, currentPlayerVal, otherPlayerVal, doCapture: false)) {
			currentPlayerFreeTree = 0;
		}
		if (otherPlayerFreeTree == 2 && CheckCaptures(yCoord, xCoord, otherPlayerVal, currentPlayerVal, doCapture: false)) {
			otherPlayerFreeTree = 0;
		}

		// Change map values and buttons where needed
		if (currentPlayerFreeTree == 2) {
			GameObject button = buttonsMap[yCoord, xCoord].gameObject;
			button.GetComponent<Image>().sprite = notAllowedSprite; // TODO: use double-three sprite
			button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
			Color buttonColor = button.GetComponent<Image>().color;
			buttonColor.a = 255;
			button.GetComponent<Image>().color = buttonColor;
			button.GetComponent<PutStone>().isEmpty = false;

			if (otherPlayerFreeTree == 2) {
				Debug.Log("Both players have double-three in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = DT_P_VALUE;
			}
			else {
				Debug.Log("Current player has double-three in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = (currentPlayerIndex == 0) ? DT_P1_VALUE : DT_P2_VALUE;
			}

		}
		else if (otherPlayerFreeTree == 2) {
			Debug.Log("Other player has double-three in " + yCoord + " " + xCoord);
			map[yCoord, xCoord] = (currentPlayerIndex == 0) ? DT_P2_VALUE : DT_P1_VALUE;

		}
	}

	private bool IsFreeThree(int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal, bool middleCheck = false) {
		if (map[yCoord - yCoeff, xCoord - xCoeff] == enemyVal || map[yCoord + yCoeff, xCoord + xCoeff] == enemyVal)
			return false;

		// common vars
		int x = 0;
		int y = 0;

		// check when coord is middle of free-tree
		if (map[yCoord - yCoeff, xCoord - xCoeff] == myVal) {
			y = yCoord + yCoeff * -2;
			x = xCoord + xCoeff * -2;
			if (x >= 0 && x < size && y >= 0 && y < size && map[y, x] != enemyVal && map[y, x] != myVal) {
				y = yCoord + yCoeff * 2;
				x = xCoord + xCoeff * 2;
				if (x >= 0 && x < size && y >= 0 && y < size) {
					if (map[yCoord + yCoeff, xCoord + xCoeff] == myVal) {
						if (map[y, x] != enemyVal && map[y, x] != myVal && middleCheck) {
							// Debug.Log("Free tree type1 at " + yCoord + " " + xCoord + " coeff " + yCoeff + " " + xCoeff);
							return true;
						}
					}
					else if (map[y, x] == myVal) {
						y += yCoeff;
						x += xCoeff;
						if (x >= 0 && x < size && y >= 0 && y < size && map[y, x] != myVal && map[y, x] != enemyVal) {
							// Debug.Log("Free tree type2 at " + yCoord + " " + xCoord + " coeff " + yCoeff + " " + xCoeff);
							return true;
						}
					}
				}

			} 
		}
		// check when coord is start of free-tree
		else if (yCoord + yCoeff * 3 < size && yCoord + yCoeff * 3 >= 0 && xCoord + xCoeff * 3 < size && xCoord + xCoeff * 3 >= 0) {
			x = 0;
			x = 0;
			int allyStones = 0;
			while (x <= 3 && x >= -3 && y <= 3 && y >= -3) {
				if (map[yCoord + y, xCoord + x] == enemyVal)
					break;
				if (map[yCoord + y, xCoord + x] == myVal) {
					allyStones++;
					if (allyStones == 2) {
						break;
					}
				}
				x += xCoeff;
				y += yCoeff;
			}
			if (allyStones == 2) {
				x += xCoeff;
				y += yCoeff;
				if (xCoord + x >= 0 && xCoord + x < size && yCoord + y >= 0 && yCoord + y < size)
					if (map[yCoord + y, xCoord + x] != enemyVal && map[yCoord + y, xCoord + x] != myVal) {
						// Debug.Log("Free tree type3 at " + yCoord + " " + xCoord + " coeff " + yCoeff + " " + xCoeff);
						return true;
					}
			}
		}

		return false;
	}
	#endregion

#region SelfCapture
	private void UpdateSelfCapture(int yCoord, int xCoord) {
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
			GameObject button = buttonsMap[yCoord, xCoord].gameObject;
			button.GetComponent<Image>().sprite = notAllowedSprite;
			button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
			Color buttonColor = button.GetComponent<Image>().color;
			buttonColor.a = 255;
			button.GetComponent<Image>().color = buttonColor;
			button.GetComponent<PutStone>().isEmpty = false;

			if (otherProhibited) {
				Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = NA_P_VALUE;
			}
			else {
				Debug.Log("Both players can't play in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = (currentPlayerIndex == 0) ? NA_P1_VALUE : NA_P2_VALUE;
			}
		}
		else if (otherProhibited) {
			// TODO: what if this is double-tree for current player ??
			map[yCoord, xCoord] = (currentPlayerIndex == 0) ? NA_P2_VALUE : NA_P1_VALUE;
			Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
		}
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
	#endregion

#region WinningAlignemts
	private bool IsWinByAlignment(int yCoord, int xCoord) {
		// TODO: find all winning alignements, if any is found check if there is any counter-move available
		List<int[,]> winningAlignements = new List<int[,]>();
		int[,] tmpAlignement;

		// Horizontal check
		tmpAlignement = GetWinningAlignement(0, 1);
		if (tmpAlignement != null) {
			winningAlignements.Add(tmpAlignement);
		}

		if (winningAlignements.Count == 0)
			return false;
		UpdateCounterMoves(winningAlignements);

		return (counterMoves.Count == 0);
	}

	private int[,] GetWinningAlignement(int yCoeff, int xCoeff) {
		// TODO
		return null;
	}

	private void UpdateCounterMoves(List<int[,]> winningAlignements) {
		// TODO
		return ;
	}
	#endregion

}
