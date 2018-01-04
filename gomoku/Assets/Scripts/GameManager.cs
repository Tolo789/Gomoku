using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class GameManager : MonoBehaviour {

	public Sprite[] stoneSprites;
	public Sprite notAllowedSprite;
	public GameObject emptyButton;
	public GameObject startBoard;
	public Text playerPlaying;
	public GameObject playSettings;
	public Canvas canvas;
	public Text[] listPlayers;
	public Text AiTimer;

	public int size = 19;
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

	private const int AI_DEPTH = 5;
	private const float AI_SEARCH_TIME = 0.1f;
	private const int MAX_CHOICE_PER_DEPTH = 5;
	private float TOTAL_SEARCHES = Mathf.Pow(MAX_CHOICE_PER_DEPTH, AI_DEPTH - 1);
	private float searchesCompleted;
	private float startSearchTime;
	private float searchTime;

	private int currentPlayerIndex = 0;
	private int currentPlayerVal = P1_VALUE;
	private int otherPlayerVal = P2_VALUE;
	private int[,] boardMap;
	private PutStone[,] buttonsMap;
	private int[] playerScores;
	private bool[] isHumanPlayer;
	private bool isAIPlaying;
	private bool AIHasResult;
	private int playerStarting;
	private List<Vector2Int> counterMoves;
	private Vector3Int bestMove;

	void Start () {
		// init game variables
		boardMap = new int[size, size];
		buttonsMap = new PutStone[size, size];
		playerScores = new int[2];
		for (int i = 0; i < playerScores.Length; i++) {
			playerScores[i] = 0;
		}
		isHumanPlayer = new bool[2];
		isHumanPlayer[0] = true;
		isAIPlaying = false;
		AIHasResult = false;
		counterMoves = new List<Vector2Int>();
		bestMove = new Vector3Int();

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
				boardMap[y, x] = EMPTY_VALUE;
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
		if (!isGameEnded && !isHumanPlayer[currentPlayerIndex]) {
			if (!isAIPlaying) {
				isAIPlaying = true;
				AIHasResult = false;
				bestMove.x = -1;
				bestMove.y = -1;
				bestMove.z = -1;
				searchesCompleted = 0;
				// start AI decision making
				StartCoroutine(StopSearchTimer());
				StartCoroutine(StartMinMaxSearch());
			}
			else if (AIHasResult) {
				StopAllCoroutines();
				Debug.Log("Search time: " + searchTime + " (" + searchesCompleted + "/" + TOTAL_SEARCHES + " searches completed)");
				AiTimer.text = "AI Timer: " + searchTime.ToString();
				if (bestMove.x != -1 && bestMove.y != -1)
					PutStone(bestMove.y, bestMove.x);
				AIHasResult = false;
				isAIPlaying = false;
			}
			else if (searchesCompleted == TOTAL_SEARCHES) {
				searchTime = Time.time - startSearchTime;
				AIHasResult = true;
			}
		}
	}

	void DispalyBoard(int[,] map) {
		for (int y = 0; y < size; y++) {
			string str = y + " ->  ";
			for (int x = 0; x < size; x++) {
				if (x != 0)
					str += " ";
				str += map[y, x].ToString();
			}
			Debug.Log(str);
		}
	}

	private int[,] CopyMap(int[,] map) {
		int[,] newMap = new int[size, size];
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				newMap[y,x] = map[y,x];
			}
		}
		return newMap;
	}

	public bool IsHumanTurn() {
		return isHumanPlayer[currentPlayerIndex];
	}

#region MainFunctions
	public void PutStone(int yCoord, int xCoord) {
		// Actually put the stone
		boardMap[yCoord, xCoord] = currentPlayerVal;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = stoneSprites[currentPlayerIndex];
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 255;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;

		// check captures
		CheckCaptures(boardMap, yCoord, xCoord, currentPlayerVal, otherPlayerVal, doCapture: true);
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
				if (boardMap[y, x] != P1_VALUE && boardMap[y, x] != P2_VALUE) {
					DeleteStone(boardMap, y, x);
					if (x > 0 && x < size -1 && y > 0 && y < size -1) // Can't have a free-tree in the borders
						UpdateDoubleThree(boardMap, y, x, currentPlayerVal, otherPlayerVal);
					if (!moveIntoCapture)
						UpdateSelfCapture(boardMap, y, x, currentPlayerVal, otherPlayerVal);
				}
			}
		}
		
		// DispalyBoard(boardMap);
	}
	public void FakePutStone(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal) {
		// Actually put the stone
		map[yCoord, xCoord] = myVal;

		// check captures
		CheckCaptures(map, yCoord, xCoord, myVal, enemyVal, doCapture: true, isAiSimulation: true);

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
		}

		// update allowed movements in map
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (map[y, x] != P1_VALUE && map[y, x] != P2_VALUE) {
					DeleteStone(map, y, x, isAiSimulation: true);
					if (x > 0 && x < size -1 && y > 0 && y < size -1) // Can't have a free-tree in the borders
						UpdateDoubleThree(map, y, x, myVal, enemyVal, isAiSimulation: true);
					if (!moveIntoCapture)
						UpdateSelfCapture(map, y, x, myVal, enemyVal, isAiSimulation: true);
				}
			}
		}
	}

	private void DeleteStone(int[,] map, int yCoord, int xCoord, bool isAiSimulation = false) {
		map[yCoord, xCoord] = EMPTY_VALUE;
		if (isAiSimulation)
			return;
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
	private IEnumerator StopSearchTimer() {
		startSearchTime = Time.time;
		yield return new WaitForSeconds(AI_SEARCH_TIME);
		searchTime = Time.time - startSearchTime;
		AIHasResult = true;
		// Debug.Log("Time's UP !! " + AIHasResult + " " + AI_SEARCH_TIME);
	}

	private IEnumerator StartMinMaxSearch() {
		// Depth 1;
		yield return new WaitForSeconds(0f);

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

		List<Vector3Int> allowedMoves = new List<Vector3Int>();
		int heuristicVal = 0;
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (allowedSpaces.Contains(boardMap[y, x])) {
					heuristicVal = GetHeuristicValue(boardMap, y, x, currentPlayerVal, otherPlayerVal);
					allowedMoves.Add(new Vector3Int(x, y, heuristicVal));
				}
			}
		}

		// TODO: what if there is no allowed move ?

		allowedMoves = allowedMoves.OrderByDescending(move => move.z).ToList();
		// int i = 1;
		// while (i < allowedMoves.Count) {
		// 	if (allowedMoves[i].z < allowedMoves[0].z) {
		// 		allowedMoves.RemoveRange(i, allowedMoves.Count - i);
		// 		break;
		// 	}
		// 	i++;
		// }
		// bestMove = allowedMoves[Random.Range(0, allowedMoves.Count - 1)];
		bestMove = allowedMoves[0];
		bestMove.z = -10000;
		if (AI_DEPTH > 1) {
			for (int i = 0; i < MAX_CHOICE_PER_DEPTH; i++) {
				if (i < allowedMoves.Count -1) {
					int[,] newMap = CopyMap(boardMap);
					FakePutStone(newMap, allowedMoves[i].y, allowedMoves[i].x, currentPlayerVal, otherPlayerVal);
					StartCoroutine(MinMaxSearch(newMap, 2, otherPlayerVal, currentPlayerIndex, allowedMoves[i], currentPlayerVal));
				}
			}
		}
	}

	private IEnumerator MinMaxSearch(int[,] map, int depth, int myVal, int enemyVal, Vector3Int rootMove, int rootVal) {
		// Debug.Log("Depth: " + depth);
		yield return new WaitForSeconds(0f);
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

		List<Vector3Int> allowedMoves = new List<Vector3Int>();
		int heuristicVal = 0;
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (allowedSpaces.Contains(map[y, x])) {
					// TODO: add to heuristic
					heuristicVal = GetHeuristicValue(map, y, x, myVal, enemyVal);
					allowedMoves.Add(new Vector3Int(x, y, heuristicVal));
				}
			}
		}

		// TODO: what if there is no allowed move ?

		allowedMoves = allowedMoves.OrderByDescending(move => move.z).ToList();
		if (depth != AI_DEPTH) {
			for (int i = 0; i < MAX_CHOICE_PER_DEPTH; i++) {
				if (i < allowedMoves.Count -1) {
					int[,] newMap = CopyMap(map);
					FakePutStone(newMap, allowedMoves[i].y, allowedMoves[i].x, myVal, enemyVal);
					rootMove.z += (myVal == rootVal) ? allowedMoves[0].z : -allowedMoves[0].z;
					StartCoroutine(MinMaxSearch(newMap, depth + 1, enemyVal, myVal, rootMove, rootVal));
					rootMove.z -= (myVal == rootVal) ? allowedMoves[0].z : -allowedMoves[0].z;
				}
			}
		}
		else {
			rootMove.z += (myVal == rootVal) ? allowedMoves[0].z : -allowedMoves[0].z;
			if (rootMove.z > bestMove.z)
				bestMove = rootMove;
			// Debug.Log("Reached end of MinMax depth");
			searchesCompleted++;
		}
	}

	private int GetHeuristicValue(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal) {
		int score = 0;

		// Score based on board position
		if (yCoord != 0 && xCoord != 0 && yCoord != size -1 && xCoord != size -1) {
			if (yCoord == 9 && xCoord == 9)
				score += 4;
			else if (xCoord >= 6 && xCoord <= 12 && yCoord >= 6 && yCoord <= 12)
				score += 3;
			else if (xCoord >= 3 && xCoord <= 15 && yCoord >= 3 && yCoord <= 15)
				score += 2;
			else if (xCoord >= 1 && xCoord <= 17 && yCoord >= 1 && yCoord <= 17)
				score += 1;
		}

		if (CheckCaptures(map, yCoord, xCoord, myVal, enemyVal, doCapture:false, isAiSimulation: true))
			score += 10;

		return score;
	}
	#endregion

#region Captures
	private bool CheckCaptures(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, bool doCapture = true, bool isAiSimulation = false) {
		bool canCapture = false;

		// Left
		if (xCoord - 3 >= 0 && map[yCoord, xCoord - 3] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, 0, -1, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Top
		if (yCoord - 3 >= 0 && map[yCoord - 3, xCoord] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, -1, 0, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Bot
		if (yCoord + 3 < size && map[yCoord + 3, xCoord] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, 1, 0, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Right
		if (xCoord + 3 < size && map[yCoord, xCoord + 3] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, 0, 1, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Bot right
		if (xCoord + 3 < size && yCoord + 3 < size && map[yCoord + 3, xCoord + 3] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, 1, 1, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Bot left
		if (xCoord - 3 >= 0 && yCoord + 3 < size && map[yCoord + 3, xCoord - 3] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, 1, -1, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Top left
		if (xCoord - 3 >= 0 && yCoord - 3 >= 0 && map[yCoord - 3, xCoord - 3] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, -1, -1, enemyVal, doCapture, isAiSimulation) || canCapture;
		}
		// Top right
		if (xCoord + 3 < size && yCoord - 3 >= 0 && map[yCoord - 3, xCoord + 3] == myVal) {
			canCapture = CanCapture(map, yCoord, xCoord, -1, 1, enemyVal, doCapture, isAiSimulation) || canCapture;
		}

		return canCapture;
	}

	private bool CanCapture(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int enemyVal, bool doCapture = false, bool isAiSimulation = false) {
		int y1 = yCoord + yCoeff * 1;
		int y2 = yCoord + yCoeff * 2;
		int x1 = xCoord + xCoeff * 1;
		int x2 = xCoord + xCoeff * 2;

		if (map[y1, x1] == enemyVal && map[y2, x2] == enemyVal) {
			if (doCapture) {
				DeleteStone(map, y1, x1, isAiSimulation);
				DeleteStone(map, y2, x2, isAiSimulation);
				if (!isAiSimulation) {
					playerScores[currentPlayerIndex] += 2;
					listPlayers[currentPlayerIndex].text = "Player" + currentPlayerVal + ": " + playerScores[currentPlayerIndex];
					if (playerScores[currentPlayerIndex] == 10) {
						DisplayWinner(currentPlayerIndex);
					}
				}
			}
			return true;
		}
		return false;
	}
	#endregion

#region FreeTree
	private void UpdateDoubleThree(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, bool isAiSimulation = false) {
		// TODO: do checks for both players

		// check in every direction and count number of free three, if n >= 2 break and add prohibition
		int currentPlayerFreeTree = 0;
		int otherPlayerFreeTree = 0;

		// Left
		if (IsFreeThree(map, yCoord, xCoord, 0, -1, myVal, enemyVal, true)) {
			currentPlayerFreeTree++;
		}
		if (IsFreeThree(map, yCoord, xCoord, 0, -1, enemyVal, myVal, true)) {
			otherPlayerFreeTree++;
		}
		// Right
		if (IsFreeThree(map, yCoord, xCoord, 0, 1, myVal, enemyVal)) {
			currentPlayerFreeTree++;
		}
		if (IsFreeThree(map, yCoord, xCoord, 0, 1, enemyVal, myVal)) {
			otherPlayerFreeTree++;
		}
		// Top
		if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 0, myVal, enemyVal, true)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 0, enemyVal, myVal, true)) {
			otherPlayerFreeTree++;
		}
		// Bot
		if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 0, myVal, enemyVal)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 0, enemyVal, myVal)) {
			otherPlayerFreeTree++;
		}
		// Top Left
		if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, -1, myVal, enemyVal, true)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, -1, enemyVal, myVal, true)) {
			otherPlayerFreeTree++;
		}
		// Top Right
		if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 1, myVal, enemyVal, true)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 1, enemyVal, myVal, true)) {
			otherPlayerFreeTree++;
		}
		// Bot Left
		if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, -1, myVal, enemyVal)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, -1, enemyVal, myVal)) {
			otherPlayerFreeTree++;
		}
		// Bot Right
		if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 1, myVal, enemyVal)) {
			currentPlayerFreeTree++;
		}
		if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 1, enemyVal, myVal)) {
			otherPlayerFreeTree++;
		}

		// Is not double-three if there is a capture
		if (currentPlayerFreeTree == 2 && CheckCaptures(map, yCoord, xCoord, myVal, enemyVal, doCapture: false)) {
			currentPlayerFreeTree = 0;
		}
		if (otherPlayerFreeTree == 2 && CheckCaptures(map, yCoord, xCoord, enemyVal, myVal, doCapture: false)) {
			otherPlayerFreeTree = 0;
		}

		// Change map values and buttons where needed
		if (currentPlayerFreeTree == 2) {
			if (!isAiSimulation) {
				GameObject button = buttonsMap[yCoord, xCoord].gameObject;
				button.GetComponent<Image>().sprite = notAllowedSprite; // TODO: use double-three sprite
				button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
				Color buttonColor = button.GetComponent<Image>().color;
				buttonColor.a = 255;
				button.GetComponent<Image>().color = buttonColor;
				button.GetComponent<PutStone>().isEmpty = false;
			}

			if (otherPlayerFreeTree == 2) {
				if (!isAiSimulation)
					Debug.Log("Both players have double-three in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = DT_P_VALUE;
			}
			else {
				if (!isAiSimulation)
					Debug.Log("Current player has double-three in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = (currentPlayerIndex == 0) ? DT_P1_VALUE : DT_P2_VALUE;
			}

		}
		else if (otherPlayerFreeTree == 2) {
			if (!isAiSimulation)
				Debug.Log("Other player has double-three in " + yCoord + " " + xCoord);
			map[yCoord, xCoord] = (currentPlayerIndex == 0) ? DT_P2_VALUE : DT_P1_VALUE;

		}
	}

	private bool IsFreeThree(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal, bool middleCheck = false) {
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
	private void UpdateSelfCapture(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, bool isAiSimulation = false) {
		bool currentProhibited = false;
		bool otherProhibited = false;

		bool checkLeft = xCoord - 2 >= 0 && xCoord + 1 < size;
		bool checkRight = xCoord + 2 < size && xCoord - 1 >= 0;
		bool checkTop = yCoord - 2 >= 0 && yCoord + 1 < size;
		bool checkBot = yCoord + 2 < size && yCoord - 1 >= 0;
		// Left
		if (checkLeft) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, 0, -1, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, 0, -1, enemyVal, myVal);
		}
		// Top
		if (checkTop) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, -1, 0, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, -1, 0, enemyVal, myVal);			
		}
		// Bot
		if (checkBot) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, 1, 0, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, 1, 0, enemyVal, myVal);
		}
		// Right
		if (checkRight) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, 0, 1, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, 0, 1, enemyVal, myVal);
		}
		// Bot right
		if (checkBot && checkRight) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, 1, 1, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, 1, 1, enemyVal, myVal);
		}
		// Bot left
		if (checkBot && checkLeft) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, 1, -1, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, 1, -1, enemyVal, myVal);
		}
		// Top left
		if (checkTop && checkLeft) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, -1, -1, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, -1, -1, enemyVal, myVal);
		}
		// Top right
		if (checkTop && checkRight) {
			currentProhibited = currentProhibited || CheckSelfCapture(map, yCoord, xCoord, -1, 1, myVal, enemyVal);
			otherProhibited = otherProhibited || CheckSelfCapture(map, yCoord, xCoord, -1, 1, enemyVal, myVal);
		}

		if (currentProhibited) {
			if (!isAiSimulation) {
				GameObject button = buttonsMap[yCoord, xCoord].gameObject;
				button.GetComponent<Image>().sprite = notAllowedSprite;
				button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
				Color buttonColor = button.GetComponent<Image>().color;
				buttonColor.a = 255;
				button.GetComponent<Image>().color = buttonColor;
				button.GetComponent<PutStone>().isEmpty = false;
			}

			if (otherProhibited) {
				if (!isAiSimulation)
					Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = NA_P_VALUE;
			}
			else {
				if (!isAiSimulation)
					Debug.Log("Both players can't play in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = (currentPlayerIndex == 0) ? NA_P1_VALUE : NA_P2_VALUE;
			}
		}
		else if (otherProhibited) {
			// TODO: what if this is double-tree for current player ??
			map[yCoord, xCoord] = (currentPlayerIndex == 0) ? NA_P2_VALUE : NA_P1_VALUE;
			if (!isAiSimulation)
				Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
		}
	}

	private bool CheckSelfCapture(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal) {
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
