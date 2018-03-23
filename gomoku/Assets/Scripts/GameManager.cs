using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public class GameManager : MonoBehaviour {

	// Needed to use of common functions of gomoku
	public GomokuPlay gomoku;

	public Sprite[] stoneSprites;
	public Sprite notAllowedSprite;
	public Sprite doubleThreeSprite;
	public GameObject emptyButton;
	public GameObject startBoard;
	public GameObject playSettings;
	public GameObject swapPlayers;

	public GameObject chooseSwapOptions;
	public GameObject player1;
	public GameObject player2;
	public Canvas canvas;
	public Text[] listPlayers;
	public Text AiTimer;
	public Text displayWinner;

	// Sprites square
	public Sprite sqTopLeft;
	public Sprite sqTopRight;
	public Sprite sqBotRight;
	public Sprite sqBotLeft;
	public Sprite sqHorizontal;
	public Sprite sqVertical;


	// Offline only game vars
	private PutStone[,] buttonsMap;

#region MonoFunctions
	void Start () {
		gomoku.Init();
		buttonsMap = new PutStone[GomokuPlay.SIZE, GomokuPlay.SIZE];

		// init board with hidden buttons
		float width = startBoard.GetComponent<RectTransform>().rect.width;
		float height = startBoard.GetComponent<RectTransform>().rect.height;
		Vector3 startPos = startBoard.transform.position;
		startPos.x -= width * canvas.transform.localScale.x / 2;
		startPos.y += height * canvas.transform.localScale.x / 2;
		float step = width * canvas.transform.localScale.x / (GomokuPlay.SIZE - 1);
		float buttonSize = width / (GomokuPlay.SIZE - 1);
		int x = 0;
		int y = 0;
		Vector3 tmpPos = startPos;
		while (y < GomokuPlay.SIZE) {
			tmpPos.x = startPos.x;
			x = 0;
			while (x < GomokuPlay.SIZE) {
				GameObject newButton = GameObject.Instantiate(emptyButton, tmpPos, Quaternion.identity);
				newButton.transform.position = tmpPos;
				newButton.name = y + "-" + x;
				newButton.transform.SetParent(startBoard.transform);
				newButton.transform.localScale = emptyButton.transform.localScale;
				newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonSize, buttonSize);
				buttonsMap[y,x] = newButton.GetComponent<PutStone>();
				buttonsMap[y,x].gameManager = this;
				gomoku.DeleteStone(gomoku.boardMap, y, x);
				x++;
				tmpPos.x += step;
			}
			y++;
			tmpPos.y -= step;
		}

		gomoku.isGameLoaded = true;
	}
	
	void Update () {
		if (Screen.fullScreen == true)
	        Screen.SetResolution(1024, 768, false);
		if (!gomoku.isGameEnded && !gomoku.isHumanPlayer[gomoku.currentPlayerIndex] && !gomoku.isGamePaused) {
			if (!gomoku.isAIPlaying) {
				gomoku.isAIPlaying = true;

				// start AI decision making
				gomoku.StartMinMax();

				gomoku.moveIsReady = true;
			}
		}
	}

	void LateUpdate() {
		if (gomoku.moveIsReady) {
			gomoku.moveIsReady = false;
			gomoku.PutStone(gomoku.bestMove.y, gomoku.bestMove.x);
			if (gomoku.isAIPlaying)
				gomoku.isAIPlaying = false;
		}
	}
#endregion

#region MainFunctions
	public void ToggleStudiedHighlight(bool newState) {
		if (gomoku.studiedMoves.Count > 0) {
			foreach (Vector3Int move in gomoku.studiedMoves) {
				GameObject button = buttonsMap[move.y, move.x].gameObject;
				button.transform.GetChild(0).gameObject.SetActive(newState);
			}
		}
	}

	public void ToggleStoneHighlight(int yCoord, int xCoord, bool newState) {
		buttonsMap[yCoord, xCoord].transform.GetChild(0).gameObject.SetActive(newState);
	}

	public void PutStoneUI(int playerIndex, int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = stoneSprites[playerIndex];
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 1;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;
		button.transform.GetChild(0).gameObject.SetActive(true); // highlight it
	}

	public void UpdateScore(int playerIndex, int score) {
		string pName;
		playerIndex = (gomoku.swappedColors) ? 1 - playerIndex : playerIndex;
		pName = (playerIndex == 0) ? "Player " + GomokuPlay.P1_VALUE : "Player " + GomokuPlay.P2_VALUE;

		listPlayers[playerIndex].text = pName + ": " + score;
	}

	public void DisplayWinner(int winnerIndex, bool byCapture) {
		int winner = (winnerIndex == 0) ? GomokuPlay.P1_VALUE : GomokuPlay.P2_VALUE;
		if (gomoku.swappedColors) {
			winner = (winnerIndex == 0) ? GomokuPlay.P2_VALUE : GomokuPlay.P1_VALUE;
		}
		playSettings.SetActive(true);
		if (winnerIndex == -1) {
			displayWinner.text = "Draw !";
		}
		else {
			if (byCapture)
				displayWinner.text = "Player " + winner + " won by capture !";
			else
				displayWinner.text = "Player " + winner + " won by alignment!";
		}
	}

	public void DeleteStone(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(1, 1, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color buttonColor = buttonImage.color;
		buttonColor.a = 0;
		buttonImage.color = buttonColor;
		buttonImage.sprite = null;
		button.GetComponent<PutStone>().isEmpty = true;
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	public void PutDoubleTree(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = doubleThreeSprite;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 1;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;
	}

	public void PutSelfCapture(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = notAllowedSprite;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 1;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;
	}

	public void PutHighlightedStone(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = buttonImage.color;
		newColor.a = 0.7f;
		buttonImage.color = newColor;
		buttonImage.sprite = stoneSprites[gomoku.currentPlayerIndex];
		button.transform.GetChild(0).gameObject.SetActive(true);
	}

	public void PutHandicap(int min, int max, int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(1, 1, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = Color.white;
		newColor.a = 1;

		if (yCoord == min && xCoord == min)
			buttonImage.sprite = sqTopLeft;
		else if (yCoord == min && xCoord == max - 1)
			buttonImage.sprite = sqTopRight;
		else if (yCoord == max -1  && xCoord == max -1)
			buttonImage.sprite = sqBotRight;
		else if (yCoord == max -1  && xCoord == min)
			buttonImage.sprite = sqBotLeft;
		else if (yCoord == min || yCoord == max - 1)
			buttonImage.sprite = sqHorizontal;
		else if (xCoord == min || xCoord == max - 1)
			buttonImage.sprite = sqVertical;
		else {
			buttonImage.sprite = null;
			newColor.a = 0;
		}
		buttonImage.color = newColor;

		buttonsMap[yCoord, xCoord].isEmpty = false;
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	public void UpdateTimer() {
		Debug.Log("Search time: " + gomoku.searchTime);
		AiTimer.text = "AI Timer: " + gomoku.searchTime.ToString();
		if (gomoku.searchTime > gomoku.AI_SEARCH_TIME) {
			Debug.LogWarning("Ai didnt find a move in time");
			AiTimer.color = Color.red;
		}
		else
			AiTimer.color = Color.white;
	}

	public void UpdateActivePlayer(int playerIndex) {
		if (gomoku.swappedColors) {
			listPlayers[1 - playerIndex].color = Color.cyan;
			listPlayers[playerIndex].color = Color.white;
		}
		else {
			listPlayers[playerIndex].color = Color.cyan;
			listPlayers[1 - playerIndex].color = Color.white;
		}
	}

	public void ShowSwapChoice() {
		swapPlayers.SetActive(true);
	}	

	public void ShowSwap2Choice() {
		chooseSwapOptions.SetActive(true);
	}
#endregion

#region UI triggered Func
	// TODO: make UI call directly the GomokuPlay for all (at least most) of the followin func

	public bool PlayerCanPutStone() {
		return gomoku.PlayerCanPutStone();
	}

	public void SavePlayerMove(int yCoord, int xCoord) {
		gomoku.bestMove = new Vector3Int(xCoord, yCoord, -1);
		gomoku.moveIsReady = true;
	}

	// Receives click from UI
	public void SimulateAiMove() {
		// TODO: make UI call directly the GomokuPlay
		gomoku.SimulateAiMove();
	}

	public void GoBack() {
		// TODO: make UI call directly the GomokuPlay
		gomoku.GoBack();
	}

	public void ButtonPlayClick(GameObject playSettings) {
		gomoku.isGamePaused = true;
        playSettings.SetActive(true);
    }

	public void ButtonGoBack(GameObject playSettings) {
		gomoku.isGamePaused = false;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		playSettings.SetActive(false);
    }

	public void SwapColorChoice() {
		player1.GetComponentInChildren<Image>().sprite = stoneSprites[1];
		player2.GetComponentInChildren<Image>().sprite = stoneSprites[0];
		UpdateActivePlayer(1 - gomoku.currentPlayerIndex);
		gomoku.swappedColors = true;
	}

	public void YesToggle(GameObject panel) {
		SwapColorChoice();
		gomoku.isGamePaused = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		panel.SetActive(false);
	}

	public void PlayTwoStones(GameObject panel) {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		gomoku.playedTwoMoreStones = true;
		gomoku.isGamePaused = false;
		panel.SetActive(false);
	}
	public void NoToggle(GameObject panel) {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		gomoku.isGamePaused = false;
		panel.SetActive(false);
	}

	public void chooseWhite(GameObject panel) {
		if (gomoku.currentPlayerIndex == 0) {
			SwapColorChoice();
		}
		gomoku.isGamePaused = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		panel.SetActive(false);
	}

	public void chooseBlack(GameObject panel) {
		if (gomoku.currentPlayerIndex == 1) {
			SwapColorChoice();
		}
		gomoku.isGamePaused = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		panel.SetActive(false);
	}
#endregion

#region oldAI
/*
	private IEnumerator StopSearchTimer() {
		startSearchTime = Time.time;
		yield return new WaitForSecondsRealtime(AI_SEARCH_TIME);
		searchTime = Time.time - startSearchTime;
		AIHasResult = true;
		// Debug.Log("Time's UP !! " + AIHasResult + " " + AI_SEARCH_TIME);
	}

	private IEnumerator StartAlgoSearch() {
		// Depth 0
		Debug.Log("StartMinMax");

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
		bestMove = allowedMoves[0];
		bestMove.z = -10000;
		if (AI_DEPTH >= 1) {
			for (int i = 0; i < MAX_CHOICE_PER_DEPTH; i++) {
				if (i < allowedMoves.Count -1) {
					int[,] newMap = CopyMap(boardMap);
					FakePutStone(newMap, allowedMoves[i].y, allowedMoves[i].x, currentPlayerVal, otherPlayerVal);
					StartCoroutine(DoSearch(newMap, 1, otherPlayerVal, currentPlayerVal, allowedMoves[i], currentPlayerVal));
				}
			}
		}
		yield return new WaitForSeconds(0f);
	}

	private IEnumerator DoSearch(int[,] map, int depth, int myVal, int enemyVal, Vector3Int rootMove, int rootVal) {
		Debug.Log("Depth: " + depth + ", root move: " + rootMove);
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
			depth++;
			for (int i = 0; i < MAX_CHOICE_PER_DEPTH; i++) {
				if (i < allowedMoves.Count -1) {
					int[,] newMap = CopyMap(map);
					FakePutStone(newMap, allowedMoves[i].y, allowedMoves[i].x, myVal, enemyVal);
					rootMove.z += (myVal == rootVal) ? allowedMoves[i].z : -allowedMoves[i].z;
					StartCoroutine(DoSearch(newMap, depth, enemyVal, myVal, new Vector3Int(rootMove.x, rootMove.y, rootMove.z), rootVal));
					rootMove.z -= (myVal == rootVal) ? allowedMoves[i].z : -allowedMoves[i].z;
				}
			}
		}
		else {
			rootMove.z += (myVal == rootVal) ? allowedMoves[0].z : -allowedMoves[0].z;
			Debug.Log("End of search: " + rootMove);
			if (rootMove.z > bestMove.z)
				bestMove = rootMove;
			// Debug.Log("Reached end of MinMax depth");
			searchesCompleted++;
		}
		yield return new WaitForSeconds(0f);
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
*/
	#endregion

}

