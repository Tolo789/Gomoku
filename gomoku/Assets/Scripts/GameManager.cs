using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;

public struct State {
	public int[,] map;
	public int rootPlayerScore;

	public int otherPlayerScore;
	public int myVal; 
	public int enemyVal; 
	public int rootVal;

	public int winner;

	public int depth;

	public State(State state) {
		this.map = new int[GameManager.size, GameManager.size];
		this.rootPlayerScore = state.rootPlayerScore;
		this.otherPlayerScore = state.otherPlayerScore;
		this.myVal = state.myVal;
		this.enemyVal = state.enemyVal;
		this.rootVal = state.rootVal;
		this.winner = state.winner;
		this.depth = state.depth;
		for (int y = 0; y < GameManager.size; y++) {
			for (int x = 0; x < GameManager.size; x++) {
				this.map[y,x] = state.map[y,x];
			}
		}
	}
}

public class GameManager : MonoBehaviour {


	public Sprite[] stoneSprites;
	public Sprite notAllowedSprite;
	public Sprite doubleThreeSprite;
	public GameObject emptyButton;
	public GameObject startBoard;
	public Text playerPlaying;
	public GameObject playSettings;
	public Canvas canvas;
	public Text[] listPlayers;
	public Text AiTimer;

	public static int size = 19;
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

	private const int AI_DEPTH = 3;
	private const float AI_SEARCH_TIME = 100f;
	private const int AI_MAX_SEARCHES_PER_DEPTH = 20;
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
	private bool moveIsReady = false;
	private Vector2Int lastMove;

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
		lastMove = new Vector2Int(-1, -1);

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
				 currentPlayerIndex = UnityEngine.Random.Range(0, 1);
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
				// boardMap[y, x] = EMPTY_VALUE;
				GameObject newButton = GameObject.Instantiate(emptyButton, tmpPos, Quaternion.identity);
				newButton.transform.position = tmpPos;
				newButton.name = y + "-" + x;
				newButton.transform.SetParent(startBoard.transform);
				newButton.transform.localScale = emptyButton.transform.localScale;
				newButton.GetComponent<RectTransform>().sizeDelta = new Vector2(buttonSize, buttonSize);
				buttonsMap[y,x] = newButton.GetComponent<PutStone>();
				buttonsMap[y,x].gameManager = this;
				DeleteStone(boardMap, y, x);
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
				// start AI decision making
				startSearchTime = Time.realtimeSinceStartup;
				// searchTime = 0;
				// StartCoroutine(StopSearchTimer());
				// StartCoroutine(StartMinMax());
				StartMinMax();
			}
			else if (AIHasResult) {
				searchTime = Time.realtimeSinceStartup - startSearchTime;
				// searchTime += Time.deltaTime;
				StopAllCoroutines();
				Debug.Log("Search time: " + searchTime);
				AiTimer.text = "AI Timer: " + searchTime.ToString();
				if (bestMove.z == -1)
					Debug.LogWarning("Ai didnt find a move");
				PutStone(bestMove.y, bestMove.x);
				AIHasResult = false;
				isAIPlaying = false;
			}
			// else {
			// 	searchTime += Time.deltaTime;
			// }
		}
	}

	void LateUpdate() {
		if (moveIsReady) {
			moveIsReady = false;
			PutStone(bestMove.y, bestMove.x);
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

	public int[,] CopyMap(int[,] map) {
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

#region AI

private IEnumerator StopSearchTimer() {
	Debug.Log("Start countdown for " + AI_SEARCH_TIME);
	while (Time.realtimeSinceStartup - startSearchTime < AI_SEARCH_TIME)
		yield return new WaitForFixedUpdate();
	// searchTime = Time.time - startSearchTime;
	Debug.Log("Time's UP !! " + AIHasResult + " " + (Time.realtimeSinceStartup - startSearchTime).ToString());
	AIHasResult = true;
}

private List<Vector3Int> GetAllowedMoves(State state) {
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
				if (allowedSpaces.Contains(state.map[y, x])) {
					heuristicVal = GetMoveHeuristic(state, y, x);
					allowedMoves.Add(new Vector3Int(x, y, heuristicVal));
				}
			}
		}
		allowedMoves = allowedMoves.OrderByDescending(move => move.z).Take(AI_MAX_SEARCHES_PER_DEPTH).ToList();
		return allowedMoves;
}
private int GetMoveHeuristic(State state, int yCoord, int xCoord) {
	int score = Utility(state);

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

	if (CheckCaptures(state.map, yCoord, xCoord, state.myVal, state.enemyVal, doCapture:false, isAiSimulation: true))
		score += 10;

	if (CheckCaptures(state.map, yCoord, xCoord, state.enemyVal, state.myVal, doCapture:false, isAiSimulation: true))
		score += 10;

	return score;
}

private void StartMinMax() {
	// Depth 0
	Debug.Log("StartMinMax");
	State state = new State();
	state.map = CopyMap(boardMap);
	state.myVal = currentPlayerVal;
	state.enemyVal = otherPlayerVal;
	state.rootVal = state.myVal;
	state.rootPlayerScore = playerScores[currentPlayerIndex];
	state.otherPlayerScore = playerScores[1 - currentPlayerIndex];
	state.depth = 0;
	state.winner = -1;
	List<Vector3Int> allowedMoves = GetAllowedMoves(state);

	bestMove = allowedMoves[0];
	bestMove.z = -1;
	// int v = MaxValue(state, Int32.MinValue, Int32.MaxValue);
	// int outVal = 0;
	AlphaBeta(state, Int32.MinValue, Int32.MaxValue, true);
	// yield return new WaitForSecondsRealtime(AI_SEARCH_TIME - 1f);
	// Debug.Log("MinMax took " + searchTime + " seconds");
	AIHasResult = true;
	// yield break;
}
private int AlphaBeta(State state, int alpha, int beta, bool maximizingPlayer) {
	if (GameEnded(state)) {
		return Utility(state);
	}
	else {
		if (maximizingPlayer) {
			int v = Int32.MinValue;
			foreach (Vector3Int move in GetAllowedMoves(state)) {
				if (Time.realtimeSinceStartup - startSearchTime >= AI_SEARCH_TIME)
					return v;
				int maxValue = AlphaBeta(ResultOfMove(state, move), alpha, beta, false);
				if (Time.realtimeSinceStartup - startSearchTime >= AI_SEARCH_TIME)
					return v;
				if (maxValue > v) {
					v = maxValue;
				}
				if (v > alpha) {
					alpha = v;
					if (state.depth == 0) {
						bestMove = move;
						bestMove.z = alpha;
						Debug.Log("Update best move: " + bestMove);
					}
				}
				if (beta <= alpha) {
					break ;
				}
			}
			// outVal = v;
			return v;
		}
		else {
			int v = Int32.MaxValue;
			foreach (Vector3Int move in GetAllowedMoves(state)) {
				if (Time.realtimeSinceStartup - startSearchTime >= AI_SEARCH_TIME)
					return v;
				int minValue = AlphaBeta(ResultOfMove(state, move), alpha, beta, true);
				if (Time.realtimeSinceStartup - startSearchTime >= AI_SEARCH_TIME)
					return v;
				if (minValue < v) {
					v = minValue;
				}
				if (v < beta) {
					beta = v;
				}
				if (beta <= alpha) {
					break ;
				}
			}
			// outVal = v;
			return v;
		}
	}
	// yield break;
}

private int GetStateHeuristic(State state) {
	int stateScore = 0;

	// Consider both score individually
	stateScore += 100 * state.rootPlayerScore;
	stateScore -= 100 * state.otherPlayerScore;

	return stateScore;
}

private int Utility(State state) {
	if (state.winner == state.rootVal) {
		return Int32.MaxValue;
	}
	else if (state.winner == state.enemyVal) {
		return Int32.MinValue;
	}
	return GetStateHeuristic(state);
}

private bool GameEnded(State state) {
	if (state.depth == AI_DEPTH || Time.realtimeSinceStartup - startSearchTime >= AI_SEARCH_TIME) {
		return true;
	}
	if (state.rootPlayerScore == 10 || CheckIfAlign(state.map, state.rootVal)) {
		state.winner = state.rootVal;
		return true;
	}
	else if (state.otherPlayerScore == 10 || CheckIfAlign(state.map, state.enemyVal)){
		state.winner = state.enemyVal;
		return true;
	}
	return false;
}

private State ResultOfMove(State state, Vector3Int move) {
	State newState = new State(state);
	FakePutStone(ref newState, move.y, move.x);
	newState.depth++;
	return newState;
}

private int MaxValue(State state, int alpha, int beta) {
	if (GameEnded(state)) {
		// Debug.Log("In MaxValue");
		// DebugState(state);
		// Wait();
		return Utility(state);
	}
	int v = Int32.MinValue;
	int i = 0;
	foreach (Vector3Int move in GetAllowedMoves(state)) {
		int minValue = MinValue(ResultOfMove(state, move), alpha, beta);
		// Debug.Log("Depth: " + state.depth + ", check: " + i + ", val: " + minValue);
		v = (minValue > v ) ? minValue : v;
		if (v >= beta) {
			// Debug.Log("Exiting..");
			return v;
		}
		if (v > alpha) {
			alpha = v;
			// Check if need to update best choice
			if (state.depth == 0) {
				bestMove = move;
				bestMove.z = alpha;
				Debug.Log("Update best move: " + bestMove);
			}
		}
		i++;
	}
	return v;
}

private int MinValue(State state, int alpha, int beta) {
	if (GameEnded(state)) {
		// Debug.Log("In MinValue");
		// DebugState(state);
		// Wait();
		return Utility(state);
	}
	int v = Int32.MaxValue;
	foreach (Vector3Int move in GetAllowedMoves(state)) {
		int maxValue = MaxValue(ResultOfMove(state, move), alpha, beta);
		v = (maxValue < v ) ? maxValue : v;
		if (v <= alpha) {
			return v;
		}
		if (v < beta)
			beta = v;
	}
	return v;
}

private void DebugState(State state) {
	Debug.Log("--- Start debug state ------------");
	Debug.Log("Depth: " + state.depth);
	Debug.Log("Score: " + state.rootPlayerScore + "-" + state.otherPlayerScore);
	// DispalyBoard(state.map);
	Debug.Log("------------ End debug state ---");
}

private void Wait() {
	for(int i = 0; i < Int32.MaxValue; i++) {
		// for(int j = Int32.MinValue; j < Int32.MaxValue; j++) {
		// 	for(int k = Int32.MinValue; k < Int32.MaxValue; k++) {
		// 		for(int l = Int32.MinValue; l < Int32.MaxValue; l++) {
		// 			for(int m = Int32.MinValue; m < Int32.MaxValue; m++) {}
		// 		}
		// 	}
		// }
	}
}

#endregion

#region MainFunctions
	public void SavePlayerMove(int yCoord, int xCoord) {
		bestMove = new Vector3Int(xCoord, yCoord, -1);
		moveIsReady = true;
	}

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
		button.transform.GetChild(0).gameObject.SetActive(true);

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

		// check if win by allignement 2
		if (GetWinningAlignement2(currentPlayerVal, otherPlayerVal, boardMap)) {
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

		// Update last move tracker
		if (lastMove.x != -1) {
			buttonsMap[lastMove.y, lastMove.x].transform.GetChild(0).gameObject.SetActive(false);
		}
		lastMove.y = yCoord;
		lastMove.x = xCoord;
		
		// DispalyBoard(boardMap);
	}
	public void FakePutStone(ref State state, int yCoord, int xCoord) {
		// Actually put the stone
		state.map[yCoord, xCoord] = state.myVal;

		// check captures
		if (CheckCaptures(state.map, yCoord, xCoord, state.myVal, state.enemyVal, doCapture: true, isAiSimulation: true)) {
			if (state.myVal == state.rootVal) {
				state.rootPlayerScore += 2;
			}
			else {
				state.otherPlayerScore += 2;
			}
		}

		// If player needed to play a counter move and didnt do it, then he has lost
		// TODO: adapt this to simulation
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

		// check if win by allignement 2
		if (GetWinningAlignement2(state.myVal, state.enemyVal, state.map)) {
			Debug.Log("ALIGNEMENT");
		}



		// End turn, next player to play
		int tmp = state.myVal;
		state.myVal = state.enemyVal;
		state.enemyVal = tmp;

		// update allowed movements in map
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (state.map[y, x] != P1_VALUE && state.map[y, x] != P2_VALUE) {
					DeleteStone(state.map, y, x, isAiSimulation: true);
					if (x > 0 && x < size -1 && y > 0 && y < size -1) // Can't have a free-tree in the borders
						UpdateDoubleThree(state.map, y, x, state.myVal, state.enemyVal, isAiSimulation: true);
					if (!moveIntoCapture)
						UpdateSelfCapture(state.map, y, x, state.myVal, state.enemyVal, isAiSimulation: true);
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
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	private void DisplayWinner(int winnerIndex) {
		// TODO: display winner and stop playing
		int winner = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		playSettings.SetActive(true);
		Debug.Log("Player " + winner + " won !");
		isGameEnded = true;
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
				button.GetComponent<Image>().sprite = doubleThreeSprite;
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
					Debug.Log("Both players can't play in " + yCoord + " " + xCoord);
				map[yCoord, xCoord] = NA_P_VALUE;
			}
			else {
				if (!isAiSimulation)
					Debug.Log("Current player can't play in " + yCoord + " " + xCoord);
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

	private bool CheckIfAlign(int[,] map, int myVal = -1) {
		//MAKE FUNC
		return false;
	}

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

	private bool GetWinningAlignement2(int myVal, int enemyVal, int[,] map) {
		// TODO
		for (int yCoord = 0; yCoord < size; yCoord++) {
			for (int xCoord = 0; xCoord < size; xCoord++) {
				if (map[yCoord, xCoord] == myVal && (RadialCheckAlign(myVal, enemyVal, map, yCoord, xCoord, 1, 0) || RadialCheckAlign(myVal, enemyVal, map, yCoord, xCoord, 0, 1) || RadialCheckAlign(myVal, enemyVal, map, yCoord, xCoord, 1, 1)))
					return true;
			}
		}
		return false;
	}

	private bool LastAlignCheck(int[,] map, int myVal, int enemyVal) {
		if (otherPlayerVal == 8) {
			for (int y = 0; y < 19; y++) {
				for (int x = 0; x < 19; x++) {
					if (CheckCaptures(map, y, x, myVal, enemyVal, false, true)) {
						Debug.Log("ENEMY CAN WIN BY CAPTURE!!");
						return false;
					}
				}
			}
		}
		return true;
	}

	private bool RadialCheckAlign(int myVal, int enemyVal, int[,] map, int yCoord, int xCoord, int xCoeff, int yCoeff) {
		int x1 = xCoord + xCoeff * 1;
		int y1 = yCoord + yCoeff * 1;
		int x2 = xCoord + xCoeff + xCoeff + xCoeff + xCoeff;
		int y2 = yCoord + yCoeff + yCoeff + yCoeff + yCoeff;
		if (x2 < size && y2 < size && map[y2, x2] == myVal) {
			while (y1 <= y2 && x1 <= x2) {
				if (map[y1, x1] != myVal) {
					return false;
				}
				y1+= yCoeff;
				x1+= xCoeff;
			}
			if (xCoord - xCoeff >= 0 && yCoord - yCoeff >= 0) {
				if (map[yCoord - yCoeff, xCoord - xCoeff] == myVal)
					return false;
			}
			return LastAlignCheck(map, myVal, enemyVal);
		}
		return false;
	}

	private void UpdateCounterMoves(List<int[,]> winningAlignements) {
		// TODO
		return ;
	}
	#endregion

}
