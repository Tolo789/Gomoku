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
	public bool alignementDone;
	public List<Vector2Int> captureMoves;
	public List<Vector2Int> lastStones;

	public State(State state) {
		this.map = new int[GameManager.size, GameManager.size];
		this.rootPlayerScore = state.rootPlayerScore;
		this.otherPlayerScore = state.otherPlayerScore;
		this.myVal = state.myVal;
		this.enemyVal = state.enemyVal;
		this.rootVal = state.rootVal;
		this.winner = state.winner;
		this.depth = state.depth;
		this.alignementDone = state.alignementDone;
		this.captureMoves = new List<Vector2Int>();
		this.lastStones = new List<Vector2Int>();

		for (int y = 0; y < GameManager.size; y++) {
			for (int x = 0; x < GameManager.size; x++) {
				this.map[y,x] = state.map[y,x];
			}
		}

		foreach (Vector2Int move in state.captureMoves) {
			this.captureMoves.Add(move);
		}

		for (int i = 0; i < state.lastStones.Count; i++) {
			this.lastStones.Insert(i, state.lastStones[i]);
		}
	}
}
public struct BackupState {
	public int[,] map;
	public int[] playerScores;

	public int currentPlayerIndex;

	public bool alignmentHasBeenDone;
	public List<Vector2Int> counterMoves;
	public List<Vector2Int> lastMoves;
}

public class GameManager : MonoBehaviour {


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

	public static int size = 19;
	[HideInInspector]
	public bool isGameEnded = false;
	public bool isGamePaused = false;

	// Game settings
	private int AI_DEPTH = 3;
	private float AI_SEARCH_TIME = 0.5f;
	private int AI_MAX_SEARCHES_PER_DEPTH = 30;
	private bool DOUBLE_THREE_RULE = true;
	private bool SELF_CAPTURE_RULE = false;
	private int CAPTURES_NEEDED_TO_WIN = 10;
	private int HEURISTIC_ALIGN_COEFF = 5;
	private int HEURISTIC_CAPTURE_COEFF = 50;
	private int HANDICAP = 1;

	// Map values
	private const int EMPTY_VALUE = 0;
	private const int P1_VALUE = 1;
	private const int P2_VALUE = 2;
	private const int DT_P1_VALUE = -1;
	private const int DT_P2_VALUE = -2;
	private const int DT_P_VALUE = -3;
	private const int NA_P1_VALUE = -4;
	private const int NA_P2_VALUE = -5;
	private const int NA_P_VALUE = -6;
	private const int HANDICAP_CANT_PlAY = -7;

	// Private var
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
	private Vector3Int bestMove;
	private List<Vector2Int> counterMoves; // Moves that can break a winning align
	private List<Vector3Int> studiedMoves; // Used as debug to see AI studied moves
	private List<Vector2Int> lastMoves;
	private Vector2Int highlightedMove;
	private bool moveIsReady = false;
	private bool simulatingMove = false;
	private bool alignmentHasBeenDone = false;

	private bool swappedColors = false;
	private bool playedTwoMoreStones = false;

	private int nbrOfMoves = 0;

	private List<BackupState> backupStates;
	private List<int> allowedSpacesP1;
	private List<int> allowedSpacesP2;

	void Start () {
		// Retrieve game rules
		if (PlayerPrefs.HasKey(CommonDefines.AI_DEPTH_SETTING)) {
			AI_DEPTH = PlayerPrefs.GetInt(CommonDefines.AI_DEPTH_SETTING);
		}
		if (PlayerPrefs.HasKey(CommonDefines.AI_MOVES_NB_SETTING)) {
			AI_MAX_SEARCHES_PER_DEPTH = PlayerPrefs.GetInt(CommonDefines.AI_MOVES_NB_SETTING);
		}
		if (PlayerPrefs.HasKey(CommonDefines.AI_TIME_SETTING)) {
			AI_SEARCH_TIME = PlayerPrefs.GetFloat(CommonDefines.AI_TIME_SETTING);
		}
		if (PlayerPrefs.HasKey(CommonDefines.DOUBLE_THREE_SETTING)) {
			DOUBLE_THREE_RULE = (PlayerPrefs.GetInt(CommonDefines.DOUBLE_THREE_SETTING) == 1) ? true : false;
		}
		if (PlayerPrefs.HasKey(CommonDefines.SELF_CAPTURE_SETTING)) {
			SELF_CAPTURE_RULE = (PlayerPrefs.GetInt(CommonDefines.SELF_CAPTURE_SETTING) == 1) ? true : false;
		}
		if (PlayerPrefs.HasKey(CommonDefines.OPENING_RULE)) {
			HANDICAP = PlayerPrefs.GetInt(CommonDefines.OPENING_RULE);
		}
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
		counterMoves = new List<Vector2Int>();
		studiedMoves = new List<Vector3Int>();
		bestMove = new Vector3Int();
		lastMoves = new List<Vector2Int>();
		highlightedMove = new Vector2Int(-1, -1);
		moveIsReady = false;
		simulatingMove = false;
		alignmentHasBeenDone = false;
		backupStates = new List<BackupState>();
		allowedSpacesP1 = new List<int>();
		allowedSpacesP1.Add(EMPTY_VALUE);
		allowedSpacesP1.Add(DT_P2_VALUE);
		allowedSpacesP1.Add(NA_P2_VALUE);
		allowedSpacesP2 = new List<int>();
		allowedSpacesP2.Add(EMPTY_VALUE);
		allowedSpacesP2.Add(DT_P1_VALUE);
		allowedSpacesP2.Add(NA_P1_VALUE);


		// Handle who starts first
		if (PlayerPrefs.HasKey(CommonDefines.IS_P1_IA) && PlayerPrefs.GetInt(CommonDefines.IS_P1_IA) == 1) {
			isHumanPlayer[0] = false;
		}
		else {
			isHumanPlayer[0] = true;
		}

		if (PlayerPrefs.HasKey(CommonDefines.IS_P2_IA) && PlayerPrefs.GetInt(CommonDefines.IS_P2_IA) == 1) {
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
		listPlayers[currentPlayerIndex].color = Color.cyan;
		listPlayers[1 - currentPlayerIndex].color = Color.white;

		// init board with hidden buttons
		float width = startBoard.GetComponent<RectTransform>().rect.width;
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
		if (Screen.fullScreen == true)
	        Screen.SetResolution(1024, 768, false);
		if (!isGameEnded && !isHumanPlayer[currentPlayerIndex] && !isGamePaused) {
			if (!isAIPlaying) {
				isAIPlaying = true;

				// start AI decision making
				StartMinMax();

				// Play move
				Debug.Log("Search time: " + searchTime);
				AiTimer.text = "AI Timer: " + searchTime.ToString();
				if (searchTime >= AI_SEARCH_TIME)
					Debug.LogWarning("Ai didnt find a move in time");
				PutStone(bestMove.y, bestMove.x);
				isAIPlaying = false;
			}
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
	private void StartMinMax() {
		// Start searchTime
		startSearchTime = Time.realtimeSinceStartup;

		// Depth 0, make a copy of current game state
		Debug.Log("StartMinMax (Depth: " + AI_DEPTH + ", Moves studied per branch: " + AI_MAX_SEARCHES_PER_DEPTH + ", Max response time: " + AI_SEARCH_TIME + ")");
		State state = new State();
		state.map = CopyMap(boardMap);
		state.myVal = currentPlayerVal;
		state.enemyVal = otherPlayerVal;
		state.rootVal = state.myVal;
		state.rootPlayerScore = playerScores[currentPlayerIndex];
		state.otherPlayerScore = playerScores[1 - currentPlayerIndex];
		state.depth = 0;
		state.winner = 0;
		state.alignementDone = alignmentHasBeenDone;

		state.captureMoves = new List<Vector2Int>();
		for (int i = 0; i < counterMoves.Count; i++) {
			state.captureMoves.Insert(i, counterMoves[i]);
		}

		state.lastStones = new List<Vector2Int>();
		for (int i = 0; i < lastMoves.Count; i++) {
			state.lastStones.Insert(i, lastMoves[i]);
		}

		Debug.Log("Start state value: " + GetStateHeuristic(state));

		// Debug to see moves studied at depth 0
		// /*
		if (studiedMoves.Count > 0) {
			foreach (Vector3Int move in studiedMoves) {
				GameObject button = buttonsMap[move.y, move.x].gameObject;
				button.transform.GetChild(0).gameObject.SetActive(false);
			}
		}
		studiedMoves = GetAllowedMoves(state);
		foreach (Vector3Int move in studiedMoves) {
			GameObject button = buttonsMap[move.y, move.x].gameObject;
			button.transform.GetChild(0).gameObject.SetActive(true); // highlight it
		}
		// */

		// Saving first move as best move
		bestMove = studiedMoves[0];
		bestMove.z = Int32.MinValue;

		// Actually do MinMax
		AlphaBeta(state, Int32.MinValue, Int32.MaxValue, true);

		// Save searchTime
		searchTime = Time.realtimeSinceStartup - startSearchTime;
	}

	private List<Vector3Int> GetAllowedMoves(State state) {
		List<int> allowedSpaces = (state.myVal == P1_VALUE) ? allowedSpacesP1 : allowedSpacesP2;

		List<Vector3Int> allowedMoves = new List<Vector3Int>();
		if (state.captureMoves.Count > 0) { // Do not waste time if only few moves are ok
			foreach (Vector2Int move in state.captureMoves) {
				allowedMoves.Add(new Vector3Int(move.x, move.y, 0));
			}
		}
		else {
			int heuristicVal = 0;
			for (int y = 0; y < size; y++) {
				for (int x = 0; x < size; x++) {
					if (allowedSpaces.Contains(state.map[y, x])) {
						heuristicVal = GetMoveHeuristic(state, y, x);
						allowedMoves.Add(new Vector3Int(x, y, heuristicVal));
					}
				}
			}
		}

		int maxSearches = AI_MAX_SEARCHES_PER_DEPTH;
		if (nbrOfMoves == 0 && state.depth == 0)
			maxSearches = Mathf.Min(10, maxSearches);
		else if (nbrOfMoves == 1 && state.depth == 0)
			maxSearches = Mathf.Min(20, maxSearches);
		allowedMoves = allowedMoves.OrderByDescending(move => move.z).Take(maxSearches).ToList();
		return allowedMoves;
	}

	private int GetMoveHeuristic(State state, int yCoord, int xCoord) {
		int score = 0;
		int captures = 0;

		int middle = size / 2;
		// Score based on board position
		for (int i = 0; i < middle; i++) {
			if (xCoord >= (middle - i) && xCoord <= (middle + i) && yCoord >= (middle - i) && yCoord <= (middle + i)) {
				score += middle - i;
				break;
			}
		}

		// Increase value if its near last moves
		int nearBonus = state.lastStones.Count;
		for (int i = 0; i < state.lastStones.Count; i += 2) { // Only check for moves of the enemy
			if ((xCoord >= (state.lastStones[i].x - 1) && xCoord <= (state.lastStones[i].x + 1)) && (yCoord >= (state.lastStones[i].y - 1) && yCoord <= (state.lastStones[i].y + 1))) {
				score += nearBonus;
			}
			nearBonus -= 2;
		}

		// Increase move value for each capture that can be done
		captures = CheckCaptures(state.map, yCoord, xCoord, state.myVal, state.enemyVal, doCapture:false, isAiSimulation: true);
		if ((state.myVal == state.rootPlayerScore && captures + state.rootPlayerScore >= CAPTURES_NEEDED_TO_WIN)
			 || (captures + state.otherPlayerScore >= CAPTURES_NEEDED_TO_WIN)) {
			return Int32.MaxValue;
		}
		score += HEURISTIC_CAPTURE_COEFF * captures;

		captures = CheckCaptures(state.map, yCoord, xCoord, state.enemyVal, state.myVal, doCapture:false, isAiSimulation: true);
		if ((state.enemyVal == state.rootPlayerScore && captures + state.rootPlayerScore >= CAPTURES_NEEDED_TO_WIN)
			 || (captures + state.otherPlayerScore >= CAPTURES_NEEDED_TO_WIN)) {
			return Int32.MaxValue;
		}
		score += HEURISTIC_CAPTURE_COEFF * captures;

		// Increase move value based on neighbours influence
		captures += GetStoneInfluence(state, yCoord, xCoord);
		if (captures == Int32.MaxValue)
			return captures;
		score += captures;

		return score;
	}

	private int GetStoneInfluence(State state, int yCoord, int xCoord) {
		int influence = 0;
		int tmpScore = 0;

		tmpScore = GetRadialStoneInfluence(state, yCoord, xCoord, 0, 1);
		if (tmpScore == Int32.MaxValue)
			return tmpScore;
		influence += tmpScore;

		tmpScore = GetRadialStoneInfluence(state, yCoord, xCoord, 1, 0);
		if (tmpScore == Int32.MaxValue)
			return tmpScore;
		influence += tmpScore;

		tmpScore = GetRadialStoneInfluence(state, yCoord, xCoord, 1, 1);
		if (tmpScore == Int32.MaxValue)
			return tmpScore;
		influence += tmpScore;

		tmpScore = GetRadialStoneInfluence(state, yCoord, xCoord, -1, 1);
		if (tmpScore == Int32.MaxValue)
			return tmpScore;
		influence += tmpScore;

		return influence;
	}

	private int GetRadialStoneInfluence(State state, int yCoord, int xCoord, int yCoeff, int xCoeff) {
		int score = 0;
		int firstNeighbourVal = 0;
		int secondNeighbourVal = 0;
		int neighbours_1 = 0;
		int neighbours_2 = 0;
		int neighbours_1_jumped = 0;
		int neighbours_2_jumped = 0;
		bool isFirstEmpty = false;

		int x = xCoord + xCoeff;
		int y = yCoord + yCoeff;
		while (x >= 0 && x < size && y >= 0 && y < size) {
			// Lower influence if is one-separated from other stones
			if (state.map[y, x] == EMPTY_VALUE && !isFirstEmpty) {
				isFirstEmpty = true;
			}
			// Exit if is not a stone
			else if (state.map[y, x] != P1_VALUE && state.map[y, x] != P2_VALUE) {
				break;
			}

			// Detect change color
			else if (firstNeighbourVal == 0) {
				firstNeighbourVal = state.map[y, x];
				if (!isFirstEmpty)
					neighbours_1++;
				else
					neighbours_1_jumped++;
			}
			else if (state.map[y, x] != firstNeighbourVal){
				break;
			}
			else {
				if (!isFirstEmpty)
					neighbours_1++;
				else
					neighbours_1_jumped++;
			}
			y += yCoeff;
			x += xCoeff;
		}

		x = xCoord - xCoeff;
		y = yCoord - yCoeff;
		isFirstEmpty = false;
		while (x >= 0 && x < size && y >= 0 && y < size) {
			// Lower influence if is one-separated from other stones
			if (state.map[y, x] == EMPTY_VALUE && !isFirstEmpty) {
				isFirstEmpty = true;
			}
			// Exit if is not a stone
			else if (state.map[y, x] != P1_VALUE && state.map[y, x] != P2_VALUE) {
				break;
			}
			// Detect change color
			else if (secondNeighbourVal == 0) {
				secondNeighbourVal = state.map[y, x];
				if (!isFirstEmpty)
					neighbours_2++;
				else
					neighbours_2_jumped++;
			}
			else if (state.map[y, x] != secondNeighbourVal) {
				break;
			}
			else {
				if (!isFirstEmpty)
					neighbours_2++;
				else
					neighbours_2_jumped++;
			}
			y -= yCoeff;
			x -= xCoeff;
		}

		if (secondNeighbourVal == firstNeighbourVal) {
			neighbours_1 += neighbours_2;
			neighbours_1_jumped += neighbours_2_jumped;
			neighbours_2 = 0;
			neighbours_2_jumped = 0;
		}

		// Force detection of winning aligns, always send intMax because with intMin it would not check the move
		if (neighbours_1 >= 4) {
			return Int32.MaxValue;
		}
		if (neighbours_2 >= 4) {
			return Int32.MaxValue;
		}


		// Workout score
		if (neighbours_1 > 0)
			score = Mathf.RoundToInt((Mathf.Pow(HEURISTIC_ALIGN_COEFF, neighbours_1)));
		if (neighbours_2 > 0)
			score += Mathf.RoundToInt((Mathf.Pow(HEURISTIC_ALIGN_COEFF, neighbours_2)));
		if (neighbours_1_jumped > 0)
			score += Mathf.RoundToInt((Mathf.Pow(HEURISTIC_ALIGN_COEFF, neighbours_1_jumped))) / 2;
		if (neighbours_2_jumped > 0)
			score += Mathf.RoundToInt((Mathf.Pow(HEURISTIC_ALIGN_COEFF, neighbours_2_jumped))) / 2;

		return score;
	}

	private int AlphaBeta(State state, int alpha, int beta, bool maximizingPlayer) {
		if (GameEnded(state)) {
			return GetStateHeuristic(state);
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
				return v;
			}
		}
	}

	private bool GameEnded(State state) {
		if (state.depth == AI_DEPTH || state.winner != 0 || Time.realtimeSinceStartup - startSearchTime >= AI_SEARCH_TIME) {
			return true;
		}
		return false;
	}

	private int GetStateHeuristic(State state) {
		// Exit instantly if we know that there is a winner
		if (state.winner == 1) {
			return Int32.MaxValue;
		}
		else if (state.winner == 2) {
			return Int32.MinValue;
		}
		else if (state.winner == -1) {
			return 0;
		}

		int stateScore = GetScoreOfAligns(state);

		if (state.rootPlayerScore > 0)
			stateScore += HEURISTIC_CAPTURE_COEFF * state.rootPlayerScore + (int)Mathf.Pow(2, state.rootPlayerScore);
		if (state.otherPlayerScore > 0)
			stateScore -= (HEURISTIC_CAPTURE_COEFF * state.otherPlayerScore) + (int)Mathf.Pow(2, state.otherPlayerScore);

		return stateScore;
	}

	private int GetScoreOfAligns(State state) {
		int score = 0;
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (state.map[y, x] == P1_VALUE || state.map[y, x] == P2_VALUE) {
					score += RadialAlignScore(state, y, x, 0, 1);
					score += RadialAlignScore(state, y, x, 1, 0);
					score += RadialAlignScore(state, y, x, 1, 1);
					score += RadialAlignScore(state, y, x, 1, -1);
				}
			}
		}
		return score;
	}

	private int RadialAlignScore(State state, int yCoord, int xCoord, int yCoeff, int xCoeff) {
		int score = 0;
		bool backBlocked = false;
		bool frontBlocked = false;
		bool jumpedSpace = false;
		int nbrStone = 1;
		int nbrSideStone = 0;
		List<int> allowedSpaces = (state.map[yCoord, xCoord] == P1_VALUE) ? allowedSpacesP1 : allowedSpacesP2;

		int y = yCoord - yCoeff;
		int x = xCoord - xCoeff;
		if (x >= 0 && x < size && y >= 0 && y < size) {
			if (state.map[y, x] == state.map[yCoord, xCoord]) // Exit if is part of an align we have already evaluated
				return 0;
			if (!allowedSpaces.Contains(state.map[y, x]))
				backBlocked = true;
		}
		else {
			backBlocked = true;
		}

		y = yCoord + yCoeff;
		x = xCoord + xCoeff;
		while (true) {
			if (x < 0 || x >= size || y < 0 || y >= size) {
				frontBlocked = true;
				break;
			}

			if (state.map[y, x] == state.map[yCoord, xCoord]) {
				if (jumpedSpace)
					nbrSideStone++;
				else
					nbrStone++;
			}
			else {
				// Do not break if it is first jumpSpace
				if (allowedSpaces.Contains(state.map[y, x])) {
					if (jumpedSpace)
						break;
					jumpedSpace = true;
				}
				else {
					frontBlocked = true;
					break;
				}
			}
			y += yCoeff;
			x += xCoeff;
		}

		// TODO: delete me ?
		// if (nbrStone >= 5 || (nbrStone == 4 && !frontBlocked && !backBlocked))
		// 	return (state.map[yCoord, xCoord] == state.rootVal) ? Int32.MaxValue / 30 : Int32.MinValue / 10; // Divide because not 100% sure that it is a win/loss

		// TODO: If depth is uneven number, then we may under-estimate enemy alignements
		// TODO: If depth is even number, then we may under-estimate our alignements
		if ((state.depth % 2 == 0 && state.map[yCoord, xCoord] == state.rootVal) ||
			(state.depth % 2 == 1 && state.map[yCoord, xCoord] != state.rootVal)) {
			if (nbrSideStone > 0) {
				nbrStone += (nbrSideStone + 1);
				nbrSideStone = 0;
			}
			else if (nbrStone > 1 && (!backBlocked || !frontBlocked)) {
				nbrStone++;
			}

			if (nbrStone >= 5) {
				score = Mathf.RoundToInt(Mathf.Pow(HEURISTIC_ALIGN_COEFF, nbrStone));
				nbrStone = 0;
			}
		}

		// Get score for simple alignement
		if (nbrStone > 1) {
			if (backBlocked || ((frontBlocked && !jumpedSpace))) // align blocked by both sides
				score = nbrStone;
			else {
				score = Mathf.RoundToInt(Mathf.Pow(HEURISTIC_ALIGN_COEFF, nbrStone));
				if (backBlocked || (frontBlocked && !jumpedSpace)) {
					score /= 2;
				}
			}
		}

		// if (state.depth == 0 && state.map[yCoord, xCoord] == state.rootVal && xCoeff == 1 && yCoeff == 0)
		// 	Debug.Log("Score: " + score);

		// Get score with jump
		if (nbrSideStone > 0) {
			nbrStone += nbrSideStone;
			if (nbrStone >= 4)
				score = Mathf.RoundToInt(Mathf.Pow(HEURISTIC_ALIGN_COEFF, nbrStone));
			else if (backBlocked || frontBlocked) { // align blocked by both sides
				score += nbrStone;
			}
			else {
				if (backBlocked || frontBlocked) {
					score += (HEURISTIC_ALIGN_COEFF * nbrStone) / 2;
				}
				else
					score += HEURISTIC_ALIGN_COEFF * nbrStone;
			}
		}

		// if (state.depth == 0 && state.map[yCoord, xCoord] == state.rootVal && xCoeff == 1 && yCoeff == 0)
		// 	Debug.Log("Score: " + score);


		// Choose if is advantagious alignment
		if (state.map[yCoord, xCoord] == state.rootVal)
			return score;
		return -score; // TODO: test this return
		// return (score > 0) ? -score - 1: 0; // stones alone will always give 0
	}

	private State ResultOfMove(State state, Vector3Int move) {
		State newState = new State(state);
		FakePutStone(ref newState, move.y, move.x);
		newState.depth++;
		return newState;
	}

	private void DebugState(State state) {
		Debug.Log("--- Start debug state ------------");
		Debug.Log("Depth: " + state.depth);
		Debug.Log("Score: " + state.rootPlayerScore + "-" + state.otherPlayerScore);
		// DispalyBoard(state.map);
		Debug.Log("------------ End debug state ---");
	}
#endregion

#region MainFunctions
	public void PutStone(int yCoord, int xCoord) {
		// Backup moves only if it is a human move
		if (isHumanPlayer[currentPlayerIndex]) {
			BackupState newBackup = new BackupState();
			newBackup.map = CopyMap(boardMap);
			newBackup.playerScores = new int[2];
			newBackup.playerScores[0] = playerScores[0];
			newBackup.playerScores[1] = playerScores[1];
			newBackup.currentPlayerIndex = currentPlayerIndex;
			newBackup.alignmentHasBeenDone = alignmentHasBeenDone;

			newBackup.counterMoves = new List<Vector2Int>();
			for (int i = 0; i < counterMoves.Count; i++) {
				newBackup.counterMoves.Insert(i, counterMoves[i]);
			}

			newBackup.lastMoves = new List<Vector2Int>();
			for (int i = 0; i < lastMoves.Count; i++) {
				newBackup.lastMoves.Insert(i, lastMoves[i]);
			}

			backupStates.Insert(0, newBackup);
		}

		// If any, clear highligted stone
		ClearHighligtedStone();

		// Update last move tracker
		if (lastMoves.Count > 0) {
			buttonsMap[lastMoves[0].y, lastMoves[0].x].transform.GetChild(0).gameObject.SetActive(false);
		}
		lastMoves.Insert(0, new Vector2Int(xCoord, yCoord));
		if (lastMoves.Count > 3)
			lastMoves.RemoveAt(3);

		// Actually put the stone
		boardMap[yCoord, xCoord] = currentPlayerVal;
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.GetComponent<Image>().sprite = stoneSprites[currentPlayerIndex];
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Color buttonColor = button.GetComponent<Image>().color;
		buttonColor.a = 1;
		button.GetComponent<Image>().color = buttonColor;
		button.GetComponent<PutStone>().isEmpty = false;
		button.transform.GetChild(0).gameObject.SetActive(true); // highlight it
		nbrOfMoves+=1;

		// Do captures
		if (swappedColors) {
			playerScores[1 - currentPlayerIndex] += CheckCaptures(boardMap, yCoord, xCoord, currentPlayerVal, otherPlayerVal, doCapture: true);
			listPlayers[1 - currentPlayerIndex].text = "Player " + otherPlayerVal + ": " + playerScores[1 - currentPlayerIndex];
		}
		else {
			playerScores[currentPlayerIndex] += CheckCaptures(boardMap, yCoord, xCoord, currentPlayerVal, otherPlayerVal, doCapture: true);
			listPlayers[currentPlayerIndex].text = "Player " + currentPlayerVal + ": " + playerScores[currentPlayerIndex];
		}
		if (playerScores[currentPlayerIndex] == CAPTURES_NEEDED_TO_WIN) {
			DisplayWinner(currentPlayerIndex, true);
			return;
		}

		// Debug.Log("Alignment has been done: " + alignmentHasBeenDone);
		// If player needed to play a counter move and didnt do it, then he has lost
		if (alignmentHasBeenDone) {
			bool hasCountered = false;
			if (counterMoves.Count != 0) {
				foreach (Vector2Int counterMove in counterMoves) {
					if (counterMove.x == xCoord && counterMove.y == yCoord) {
						hasCountered = true;
						break;
					}
				}
			}
			if (!hasCountered) {
				DisplayWinner(1 - currentPlayerIndex, false);
				return;
			}
		}

		// check if a winning allignement has been done in current PutStone and if there is a possible countermove
		counterMoves.Clear();
		alignmentHasBeenDone = false;
		if (IsWinByAlignment(boardMap, yCoord, xCoord, currentPlayerVal, otherPlayerVal, playerScores[1 - currentPlayerIndex], ref alignmentHasBeenDone, ref counterMoves)) {
			DisplayWinner(currentPlayerIndex, false);
			return;
		}

		// End turn, next player to play
		currentPlayerIndex = 1 - currentPlayerIndex;
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;

		// update allowed movements in map
		bool thereIsAvailableMoves = false;
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (boardMap[y, x] != P1_VALUE && boardMap[y, x] != P2_VALUE) {
					DeleteStone(boardMap, y, x);
					if (DOUBLE_THREE_RULE)
						UpdateDoubleThree(boardMap, y, x, currentPlayerVal, otherPlayerVal);
					if (SELF_CAPTURE_RULE)
						UpdateSelfCapture(boardMap, y, x, currentPlayerVal, otherPlayerVal);
				}

				//Chef if it's a draw
				if (currentPlayerIndex == 0 && (boardMap[y, x] == EMPTY_VALUE || boardMap[y, x] == DT_P2_VALUE || boardMap[y, x] == NA_P2_VALUE)) {
					thereIsAvailableMoves = true;
				}
				if (currentPlayerIndex == 1 && (boardMap[y, x] == EMPTY_VALUE || boardMap[y, x] == DT_P2_VALUE || boardMap[y, x] == NA_P2_VALUE)) {
					thereIsAvailableMoves = true;
				}
			}
		}

		SwapPlayerTextColor();
		// Opening rules
		OpeningRules();

		if (!thereIsAvailableMoves) {
			DisplayWinner(-1, false);
			return;
		}
		
		// DispalyBoard(boardMap);
	}
	public void FakePutStone(ref State state, int yCoord, int xCoord) {
		// Actually put the stone
		state.map[yCoord, xCoord] = state.myVal;

		// Do captures
		int capturedStone = CheckCaptures(state.map, yCoord, xCoord, state.myVal, state.enemyVal, doCapture: true, isAiSimulation: true);
		if (state.myVal == state.rootVal) {
			state.rootPlayerScore += capturedStone;
			if (state.rootPlayerScore >= CAPTURES_NEEDED_TO_WIN) {
				state.winner = 1;
				return;
			}
		}
		else {
			state.otherPlayerScore += capturedStone;
			if (state.otherPlayerScore >= CAPTURES_NEEDED_TO_WIN) {
				state.winner = 2;
				return;
			}
		}

		// If player needed to play a counter move and didnt do it, then he has lost
		if (state.alignementDone) {
			bool hasCountered = false;
			if (state.captureMoves.Count != 0) {
				foreach (Vector2Int counterMove in state.captureMoves) {
					if (counterMove.x == xCoord && counterMove.y == yCoord) {
						hasCountered = true;
						break;
					}
				}
			}
			if (!hasCountered) {
				if (state.myVal == state.rootVal) {
					state.winner = 2;
				}
				else {
					state.winner = 1;
				}
				return;
			}
		}

		// check if a winning allignement has been done in current PutStone and if there is a possible countermove
		state.captureMoves.Clear();
		state.alignementDone = false;
		if (state.myVal == state.rootVal) {
			if (IsWinByAlignment(state.map, yCoord, xCoord, state.myVal, state.enemyVal, state.otherPlayerScore, ref state.alignementDone, ref state.captureMoves)) {
				state.winner = 1;
				return;
			}
		}
		else {
			if (IsWinByAlignment(state.map, yCoord, xCoord, state.myVal, state.enemyVal, state.rootPlayerScore, ref state.alignementDone, ref state.captureMoves)) {
				state.winner = 2;
				return;
			}
		}

		// End turn, next player to play
		int tmp = state.myVal;
		state.myVal = state.enemyVal;
		state.enemyVal = tmp;

		// update allowed movements in map
		bool thereIsAvailableMove = false;
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (state.map[y, x] != P1_VALUE && state.map[y, x] != P2_VALUE) {
					DeleteStone(state.map, y, x, isAiSimulation: true);
					if (DOUBLE_THREE_RULE)
						UpdateDoubleThree(state.map, y, x, state.myVal, state.enemyVal, isAiSimulation: true);
					if (SELF_CAPTURE_RULE)
						UpdateSelfCapture(state.map, y, x, state.myVal, state.enemyVal, isAiSimulation: true);
				}

				//Chef if it's a draw
				if (currentPlayerIndex == 0 && (boardMap[y, x] == EMPTY_VALUE || boardMap[y, x] == DT_P2_VALUE || boardMap[y, x] == NA_P2_VALUE)) {
					thereIsAvailableMove = true;
				}
				if (currentPlayerIndex == 1 && (boardMap[y, x] == EMPTY_VALUE || boardMap[y, x] == DT_P2_VALUE || boardMap[y, x] == NA_P2_VALUE)) {
					thereIsAvailableMove = true;
				}
			}
		}


		if (!thereIsAvailableMove) {
			state.winner = -1;
			return;
		}
	}

	private void DeleteStone(int[,] map, int yCoord, int xCoord, bool isAiSimulation = false) {
		map[yCoord, xCoord] = EMPTY_VALUE;
		if (isAiSimulation)
			return;
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

	public bool PlayerCanPutStone() {
		if (!IsHumanTurn() || isGameEnded || simulatingMove || isGamePaused)
			return false;
		return true;
	}
	public void SavePlayerMove(int yCoord, int xCoord) {
		bestMove = new Vector3Int(xCoord, yCoord, -1);
		moveIsReady = true;
	}

	public void SimulateAiMove() {
		if (PlayerCanPutStone()) {
			simulatingMove = true;
			StartMinMax();

			// Timing stuff
			Debug.Log("Search time: " + searchTime);
			AiTimer.text = "AI Timer: " + searchTime.ToString();
			if (searchTime > AI_SEARCH_TIME) {
				Debug.LogWarning("Ai didnt find a move in time");
				AiTimer.color = Color.red;
			}
			else
				AiTimer.color = Color.white;

			// Save highlighted stone position
			ClearHighligtedStone();
			highlightedMove.y = bestMove.y;
			highlightedMove.x = bestMove.x;

			// Put highlighted sprite
			GameObject button = buttonsMap[highlightedMove.y, highlightedMove.x].gameObject;
			button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
			Image buttonImage = button.GetComponent<Image>();
			Color newColor = buttonImage.color;
			newColor.a = 0.7f;
			buttonImage.color = newColor;
			buttonImage.sprite = stoneSprites[currentPlayerIndex];

			simulatingMove = false;
		}
	}

	private void ClearHighligtedStone() {
		if (highlightedMove.y != -1 && highlightedMove.x != -1) {
			DeleteStone(boardMap, highlightedMove.y, highlightedMove.x);
			highlightedMove.y = -1;
			highlightedMove.x = -1;
		}
	}

	public void GoBack() {
		if (backupStates.Count == 0 || isAIPlaying || (!isHumanPlayer[currentPlayerIndex] && !isGameEnded))
			return ;
		if (isGameEnded) {
			isGameEnded = false;
		}
		BackupState oldState = backupStates[0];

		boardMap = CopyMap(oldState.map);
		playerScores[0] = oldState.playerScores[0];
		playerScores[1] = oldState.playerScores[1];
		alignmentHasBeenDone = oldState.alignmentHasBeenDone;

		counterMoves.Clear();
		for (int i = 0; i < oldState.counterMoves.Count; i++) {
			counterMoves.Insert(i, oldState.counterMoves[i]);
		}

		lastMoves.Clear();
		for (int i = 0; i < oldState.lastMoves.Count; i++) {
			lastMoves.Insert(i, oldState.lastMoves[i]);
		}

		// Player playing logic
		currentPlayerIndex = oldState.currentPlayerIndex;
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;
		

		// first reset everything and put stones back
		int playerIndex = -1;
		int tmpVal = EMPTY_VALUE;
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				tmpVal = boardMap[y, x];
				DeleteStone(boardMap, y, x);

				playerIndex = -1;
				if (tmpVal == P1_VALUE)
					playerIndex = 0;
				else if (tmpVal == P2_VALUE)
					playerIndex = 1;

				if (playerIndex >= 0) {
					GameObject button = buttonsMap[y, x].gameObject;
					button.GetComponent<Image>().sprite = stoneSprites[playerIndex];
					button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
					Color buttonColor = button.GetComponent<Image>().color;
					buttonColor.a = 1;
					button.GetComponent<Image>().color = buttonColor;
					button.GetComponent<PutStone>().isEmpty = false;

					if (lastMoves[0].y == y && lastMoves[0].x == x) {
						button.transform.GetChild(0).gameObject.SetActive(true); // highlight it
					}
					else {
						button.transform.GetChild(0).gameObject.SetActive(false);
					}

					boardMap[y,x] = tmpVal;
				}
			}
		}

		// second iteration to update allowed moves
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (boardMap[y, x] != P1_VALUE && boardMap[y, x] != P2_VALUE) {
					if (DOUBLE_THREE_RULE)
						UpdateDoubleThree(boardMap, y, x, currentPlayerVal, otherPlayerVal);
					if (SELF_CAPTURE_RULE)
						UpdateSelfCapture(boardMap, y, x, currentPlayerVal, otherPlayerVal);
				}
			}
		}

		// Change UI
		listPlayers[0].text = "Player 1" + ": " + playerScores[0];
		listPlayers[1].text = "Player 2" + ": " + playerScores[1];




		backupStates.RemoveAt(0);

		//OpeningRules RULES UNDO
		nbrOfMoves = nbrOfMoves - 1;
		if  (HANDICAP == 4 && nbrOfMoves == 2 || HANDICAP == 5 && nbrOfMoves == 4) {
			player1.GetComponentInChildren<Image>().sprite = stoneSprites[0];
			player2.GetComponentInChildren<Image>().sprite = stoneSprites[1];
		}
		if (nbrOfMoves == 2 && (HANDICAP == 3 || HANDICAP == 2)) {
			if (HANDICAP == 3)
				SetForbiddenMove(7, 12);
			else if (HANDICAP == 2)
				SetForbiddenMove(5, 14);
		}
		if ((HANDICAP == 4 && nbrOfMoves == 3) || (playedTwoMoreStones && nbrOfMoves == 5)) {
			player1.GetComponentInChildren<Image>().sprite = stoneSprites[0];
			player2.GetComponentInChildren<Image>().sprite = stoneSprites[1];
			playedTwoMoreStones = false;
			swapPlayers.SetActive(true);
			swappedColors = false;
		}
		else if (HANDICAP == 5 && nbrOfMoves == 2) {
			chooseSwapOptions.SetActive(true);
			swappedColors = false;
		}

	}

	private void SwapPlayerTextColor() {
		if ((HANDICAP == 4 || HANDICAP == 5) && (nbrOfMoves < 3 || (nbrOfMoves < 5 && playedTwoMoreStones))) {
			return ;
		}
		else {
			if (HANDICAP < 4) {
					listPlayers[currentPlayerIndex].color = Color.cyan;
					listPlayers[1 - currentPlayerIndex].color = Color.white;
			}
			else {
				if (nbrOfMoves > 3 && swappedColors) {
					listPlayers[1 - currentPlayerIndex].color = Color.cyan;
					listPlayers[currentPlayerIndex].color = Color.white;
				}
				else {
					listPlayers[currentPlayerIndex].color = Color.cyan;
					listPlayers[1 - currentPlayerIndex].color = Color.white;
				}
			}
		}
	}

	private void DisplayWinner(int winnerIndex, bool byCapture) {
		// TODO: display winner and stop playing
		int winner = (winnerIndex == 0) ? P1_VALUE : P2_VALUE;
		if (swappedColors) {
			winner = (winnerIndex == 0) ? P2_VALUE : P1_VALUE;
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
	private int CheckCaptures(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, bool doCapture = true, bool isAiSimulation = false) {
		int canCapture = 0;

		// Left
		if (xCoord - 3 >= 0 && map[yCoord, xCoord - 3] == myVal && CanCapture(map, yCoord, xCoord, 0, -1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Top
		if (yCoord - 3 >= 0 && map[yCoord - 3, xCoord] == myVal && CanCapture(map, yCoord, xCoord, -1, 0, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Bot
		if (yCoord + 3 < size && map[yCoord + 3, xCoord] == myVal && CanCapture(map, yCoord, xCoord, 1, 0, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Right
		if (xCoord + 3 < size && map[yCoord, xCoord + 3] == myVal && CanCapture(map, yCoord, xCoord, 0, 1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Bot right
		if (xCoord + 3 < size && yCoord + 3 < size && map[yCoord + 3, xCoord + 3] == myVal && CanCapture(map, yCoord, xCoord, 1, 1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Bot left
		if (xCoord - 3 >= 0 && yCoord + 3 < size && map[yCoord + 3, xCoord - 3] == myVal && CanCapture(map, yCoord, xCoord, 1, -1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Top left
		if (xCoord - 3 >= 0 && yCoord - 3 >= 0 && map[yCoord - 3, xCoord - 3] == myVal && CanCapture(map, yCoord, xCoord, -1, -1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Top right
		if (xCoord + 3 < size && yCoord - 3 >= 0 && map[yCoord - 3, xCoord + 3] == myVal && CanCapture(map, yCoord, xCoord, -1, 1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}

		return canCapture;
	}

	private bool CanCapture(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int enemyVal, bool doCapture = false, bool isAiSimulation = false) {
		int y1 = yCoord + yCoeff;
		int y2 = yCoord + yCoeff * 2;
		int x1 = xCoord + xCoeff;
		int x2 = xCoord + xCoeff * 2;

		if (map[y1, x1] == enemyVal && map[y2, x2] == enemyVal) {
			if (doCapture) {
				DeleteStone(map, y1, x1, isAiSimulation);
				DeleteStone(map, y2, x2, isAiSimulation);
			}
			return true;
		}
		return false;
	}
	#endregion

#region FreeTree
	private void UpdateDoubleThree(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, bool isAiSimulation = false) {
		// check in every direction and count number of free three, if n >= 2 break and add prohibition
		int currentPlayerFreeTree = 0;
		int otherPlayerFreeTree = 0;

		bool checkHorizontal = xCoord > 0 && xCoord < size -1;
		bool checkVertical = yCoord > 0 && yCoord < size -1;

		// Left
		if (checkHorizontal) {
			if (IsFreeThree(map, yCoord, xCoord, 0, -1, myVal, enemyVal, true)) {
				currentPlayerFreeTree++;
			}
			if (IsFreeThree(map, yCoord, xCoord, 0, -1, enemyVal, myVal, true)) {
				otherPlayerFreeTree++;
			}
		}
		// Right
		if (checkHorizontal) {
			if (IsFreeThree(map, yCoord, xCoord, 0, 1, myVal, enemyVal)) {
				currentPlayerFreeTree++;
			}
			if (IsFreeThree(map, yCoord, xCoord, 0, 1, enemyVal, myVal)) {
				otherPlayerFreeTree++;
			}
		}
		// Top
		if (checkVertical) {
			if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 0, myVal, enemyVal, true)) {
				currentPlayerFreeTree++;
			}
			if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 0, enemyVal, myVal, true)) {
				otherPlayerFreeTree++;
			}
		}
		// Bot
		if (checkVertical) {
			if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 0, myVal, enemyVal)) {
				currentPlayerFreeTree++;
			}
			if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 0, enemyVal, myVal)) {
				otherPlayerFreeTree++;
			}
		}
		// Top Left
		if (checkVertical && checkHorizontal) {
			if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, -1, myVal, enemyVal, true)) {
				currentPlayerFreeTree++;
			}
			if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, -1, enemyVal, myVal, true)) {
				otherPlayerFreeTree++;
			}
		}
		// Top Right
		if (checkVertical && checkHorizontal) {
			if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 1, myVal, enemyVal, true)) {
				currentPlayerFreeTree++;
			}
			if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, -1, 1, enemyVal, myVal, true)) {
				otherPlayerFreeTree++;
			}
		}
		// Bot Left
		if (checkVertical && checkHorizontal) {
			if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, -1, myVal, enemyVal)) {
				currentPlayerFreeTree++;
			}
			if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, -1, enemyVal, myVal)) {
				otherPlayerFreeTree++;
			}
		}
		// Bot Right
		if (checkVertical && checkHorizontal) {
			if (currentPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 1, myVal, enemyVal)) {
				currentPlayerFreeTree++;
			}
			if (otherPlayerFreeTree != 2 && IsFreeThree(map, yCoord, xCoord, 1, 1, enemyVal, myVal)) {
				otherPlayerFreeTree++;
			}
		}

		// Is not double-three if there is a capture
		if (currentPlayerFreeTree == 2 && CheckCaptures(map, yCoord, xCoord, myVal, enemyVal, doCapture: false) > 0) {
			currentPlayerFreeTree = 0;
		}
		if (otherPlayerFreeTree == 2 && CheckCaptures(map, yCoord, xCoord, enemyVal, myVal, doCapture: false) > 0) {
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
				map[yCoord, xCoord] = (myVal == P1_VALUE) ? DT_P1_VALUE : DT_P2_VALUE;
			}

		}
		else if (otherPlayerFreeTree == 2) {
			if (!isAiSimulation)
				Debug.Log("Other player has double-three in " + yCoord + " " + xCoord);
			map[yCoord, xCoord] = (myVal == P1_VALUE) ? DT_P2_VALUE : DT_P1_VALUE;

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
			y = yCoord - yCoeff * 2;
			x = xCoord - xCoeff * 2;
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
		// Exit if this map val is already a double tree for current player
		int currentPlayerDoubleTreeVal = (myVal == P1_VALUE) ? DT_P1_VALUE : DT_P2_VALUE;
		if (map[yCoord, xCoord] == currentPlayerDoubleTreeVal || map[yCoord, xCoord] == DT_P_VALUE)
			return;

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
				map[yCoord, xCoord] = (myVal == P1_VALUE) ? NA_P1_VALUE : NA_P2_VALUE;
			}
		}
		else if (otherProhibited) {
			// TODO: what if this is double-tree for current player ??
			map[yCoord, xCoord] = (myVal == P1_VALUE) ? NA_P2_VALUE : NA_P1_VALUE;
			if (!isAiSimulation)
				Debug.Log("Other player can't play in " + yCoord + " " + xCoord);
		}
	}

	private bool CheckSelfCapture(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal) {
		if (map[yCoord + yCoeff, xCoord + xCoeff] == myVal &&
			map[yCoord + yCoeff * 2, xCoord + xCoeff * 2] == enemyVal &&
			map[yCoord - yCoeff, xCoord - xCoeff] == enemyVal) {
			return true;
		}
		return false;
	}
	#endregion

#region WinningAlignemts

	private bool IsWinByAlignment(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, int enemyScore, ref bool alignementDone, ref List<Vector2Int> refCaptureMoves) {
		// TODO: find all winning alignements, if any is found check if there is any counter-move available
		bool canWinWithCapture = CanWinWithCapture(map, enemyVal, myVal, enemyScore);

		// Horizontal check
		if (IsWinningAlignement(map, yCoord, xCoord, 0, 1, myVal)) {
			alignementDone = true;
			if (!CanCounterMove(map, yCoord, xCoord, 0, 1, myVal, enemyVal, ref refCaptureMoves) && !canWinWithCapture)
				return true;
		}
		// Vertical check
		if (IsWinningAlignement(map, yCoord, xCoord, 1, 0, myVal)) {
			alignementDone = true;
			if (!CanCounterMove(map, yCoord, xCoord, 1, 0, myVal, enemyVal, ref refCaptureMoves) && !canWinWithCapture)
				return true;
		}
		// Down-Right check
		if (IsWinningAlignement(map, yCoord, xCoord, 1, 1, myVal)) {
			alignementDone = true;
			if (!CanCounterMove(map, yCoord, xCoord, 1, 1, myVal, enemyVal, ref refCaptureMoves) && !canWinWithCapture)
				return true;
		}
		// Up-Right check
		if (IsWinningAlignement(map, yCoord, xCoord, -1, 1, myVal)) {
			alignementDone = true;
			if (!CanCounterMove(map, yCoord, xCoord, -1, 1, myVal, enemyVal, ref refCaptureMoves) && !canWinWithCapture)
				return true;
		}

		// If we end up here it means that there is a way to counter alignement
		return false;
	}

	private bool CanWinWithCapture(int[,] map, int myVal, int enemyVal, int myScore) {
		// Check if Enemy can counterMove by capture
		for (int y = 0; y < size; y++) {
			for (int x = 0; x < size; x++) {
				if (myScore + CheckCaptures(map, y, x, myVal, enemyVal, doCapture: false, isAiSimulation: true) >= CAPTURES_NEEDED_TO_WIN) {
					return true;
				}
			}
		}
		return false;
	}

	private bool IsWinningAlignement(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal) {
		int neighbours = 0;

		// Back check
		int x = xCoord - xCoeff;
		int y = yCoord - yCoeff;
		while (x >= 0 && x < size && y >= 0 && y < size && map[y, x] == myVal) {
			neighbours++;
			x -= xCoeff;
			y -= yCoeff;
		}

		// Forward check
		x = xCoord + xCoeff;
		y = yCoord + yCoeff;
		while (x >= 0 && x < size && y >= 0 && y < size && map[y, x] == myVal) {
			neighbours++;
			x += xCoeff;
			y += yCoeff;
		}

		return (neighbours >= 4);
	}

	private bool CanCounterMove(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal, ref List<Vector2Int> refCaptureMoves) {
		// TODO
		// 1) Find all possible counterMoves for an alignement
		// Only check if can capture central stones: to counter a 5-alignement any capture is good, for a 6-align only middle 4 captures, for 7-align only middle 3, and so on

		// Find borders to know align length
		Vector2Int minStone = new Vector2Int(xCoord, yCoord);
		Vector2Int maxStone = new Vector2Int(xCoord, yCoord);
		int x = xCoord + xCoeff;
		int y = yCoord + yCoeff;
		while (x >= 0 && x < size && y >= 0 && y < size && map[y, x] == myVal) {
			maxStone.x = x;
			maxStone.y = y;
			x += xCoeff;
			y += yCoeff;
		}
		x = xCoord - xCoeff;
		y = yCoord - yCoeff;
		while (x >= 0 && x < size && y >= 0 && y < size && map[y, x] == myVal) {
			minStone.x = x;
			minStone.y = y;
			x -= xCoeff;
			y -= yCoeff;
		}

		int length = (xCoeff != 0) ? maxStone.x - minStone.x : maxStone.y - minStone.y;
		length++;
		length = Mathf.Abs(length); // security in case of negative coeff
		maxStone.x = minStone.x + (4 * xCoeff);
		maxStone.y = minStone.y + (4 * yCoeff);
		minStone.x = minStone.x + ((length - 5) * xCoeff);
		minStone.y = minStone.y + ((length - 5) * yCoeff);

		// Only check if can capture central stones
		List<Vector2Int> tmpCounterMoves = new List<Vector2Int>();
		if (xCoeff != 0) {
			while (minStone.x <= maxStone.x) {
				FindCaptureMoves(map, minStone.y, minStone.x, myVal, enemyVal, ref tmpCounterMoves);
				minStone.x += xCoeff;
				minStone.y += yCoeff;
			}
		}
		else {
			while (minStone.y <= maxStone.y) {
				FindCaptureMoves(map, minStone.y, minStone.x, myVal, enemyVal, ref tmpCounterMoves);
				minStone.y += yCoeff;
			}
		}

		// If no prev counterMoves then add all found counter moves
		if (refCaptureMoves.Count == 0) {
			foreach (Vector2Int tmpMove in tmpCounterMoves) {
				refCaptureMoves.Add(tmpMove);
			}
		}
		// Otherwise, keep only counterMoves in common
		else {
			refCaptureMoves = refCaptureMoves.Intersect(tmpCounterMoves).ToList();
		}

		// Return true if at least one counterMove exists
		return (refCaptureMoves.Count != 0);
	}

	private void FindCaptureMoves(int[,] map, int yCoord, int xCoord, int myVal, int enemyVal, ref List<Vector2Int> captureMoves) {
		bool checkLeft = xCoord - 2 >= 0 && xCoord + 1 < size;
		bool checkRight = xCoord + 2 < size && xCoord - 1 >= 0;
		bool checkTop = yCoord - 2 >= 0 && yCoord + 1 < size;
		bool checkBot = yCoord + 2 < size && yCoord - 1 >= 0;

		if (checkTop) {
			RadialFindCaptureMoves(map, yCoord, xCoord, -1, 0, myVal, enemyVal, ref captureMoves);
		}
		if (checkBot) {
			RadialFindCaptureMoves(map, yCoord, xCoord, 1, 0, myVal, enemyVal, ref captureMoves);
		}
		if (checkLeft) {
			RadialFindCaptureMoves(map, yCoord, xCoord, 0, -1, myVal, enemyVal, ref captureMoves);
		}
		if (checkRight) {
			RadialFindCaptureMoves(map, yCoord, xCoord, 0, 1, myVal, enemyVal, ref captureMoves);
		}
		if (checkLeft && checkTop) {
			RadialFindCaptureMoves(map, yCoord, xCoord, -1, -1, myVal, enemyVal, ref captureMoves);
		}
		if (checkLeft && checkBot) {
			RadialFindCaptureMoves(map, yCoord, xCoord, 1, -1, myVal, enemyVal, ref captureMoves);
		}
		if (checkRight && checkTop) {
			RadialFindCaptureMoves(map, yCoord, xCoord, -1, 1, myVal, enemyVal, ref captureMoves);
		}
		if (checkRight && checkBot) {
			RadialFindCaptureMoves(map, yCoord, xCoord, 1, 1, myVal, enemyVal, ref captureMoves);
		}

	}

	private void RadialFindCaptureMoves(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal, ref List<Vector2Int> captureMoves) {
		// Do more checks only if has neighbour
		if (map[yCoord + yCoeff, xCoord + xCoeff] == myVal) {
			int x;
			int y;
			// Check if at one of the extremities there is an enemy stone and at the other one there is an available space
			if (map[yCoord - yCoeff, xCoord - xCoeff] == enemyVal) {
				x = xCoord + 2 * xCoeff;
				y = yCoord + 2 * yCoeff;
			}
			else if (map[yCoord + 2 * yCoeff, xCoord + 2 * xCoeff] == enemyVal) {
				x = xCoord - xCoeff;
				y = yCoord - yCoeff;
			}
			else {
				// exit if there isn't at least one enemy stone at the boundary
				return;
			}

			if (enemyVal == P1_VALUE) {
				if (map[y, x] == EMPTY_VALUE || map[y, x] == DT_P2_VALUE || map[y, x] == NA_P2_VALUE) {
					captureMoves.Add(new Vector2Int(x, y));
				}
			}
			else if (map[y, x] == EMPTY_VALUE || map[y, x] == DT_P1_VALUE || map[y, x] == NA_P1_VALUE) {
				captureMoves.Add(new Vector2Int(x, y));
			}
		}
	}
	#endregion

	//Handle UI
	public void ButtonPlayClick(GameObject playSettings) {
		isGamePaused = true;
        playSettings.SetActive(true);
    }

	public void ButtonGoBack(GameObject playSettings) {
		isGamePaused = false;
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		playSettings.SetActive(false);
    }

	private void SetForbiddenMove(int min, int max) {
		for (int y = min; y < max; y++) {
			for (int x = min; x < max; x++) {
				if (boardMap[y, x] == EMPTY_VALUE) {
						GameObject button = buttonsMap[y, x].gameObject;
						button.transform.localScale = new Vector3(1, 1, 1);
						Color buttonColor = button.GetComponent<Image>().color;
						buttonColor.a = 255;
						button.GetComponent<Image>().color = buttonColor;
						button.GetComponent<PutStone>().isEmpty = false;
						if (y == min && x == min)
							button.GetComponent<Image>().sprite = sqTopLeft;
						else if (y == min && x == max - 1)
							button.GetComponent<Image>().sprite = sqTopRight;
						else if (y == max -1  && x == max -1)
							button.GetComponent<Image>().sprite = sqBotRight;
						else if (y == max -1  && x == min)
							button.GetComponent<Image>().sprite = sqBotLeft;
						else if (y == min || y == max - 1)
							button.GetComponent<Image>().sprite = sqHorizontal;
						else if (x == min || x == max - 1)
							button.GetComponent<Image>().sprite = sqVertical;
						else {
							buttonColor.a = 0;
							button.GetComponent<Image>().color = buttonColor;
						}
					boardMap[y, x] = HANDICAP_CANT_PlAY;
				}
			}
		}
	}

	//OPENING RULES
	private void OpeningRules() {
		if (nbrOfMoves == 2 && (HANDICAP == 3 || HANDICAP == 2)) {
			if (HANDICAP == 3)
				SetForbiddenMove(7, 12);
			else if (HANDICAP == 2)
				SetForbiddenMove(5, 14);
		}
		if ((HANDICAP == 4 && nbrOfMoves == 3) || (playedTwoMoreStones && nbrOfMoves == 5)) {
			isGamePaused = true;
			swapPlayers.SetActive(true);
		}
		else if (HANDICAP == 5 && nbrOfMoves == 3) {
			isGamePaused = true;
			chooseSwapOptions.SetActive(true);
		}
	}

	public void SwapColorChoice() {
		player1.GetComponentInChildren<Image>().sprite = stoneSprites[1];
		player2.GetComponentInChildren<Image>().sprite = stoneSprites[0];
		listPlayers[1 - currentPlayerIndex].color = Color.cyan;
		listPlayers[currentPlayerIndex].color = Color.white;
		swappedColors = true;
	}

	public void YesToggle(GameObject panel) {
		SwapColorChoice();
		isGamePaused = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		panel.SetActive(false);
	}

	public void PlayTwoStones(GameObject panel) {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		playedTwoMoreStones = true;
		isGamePaused = false;
		panel.SetActive(false);
	}
	public void NoToggle(GameObject panel) {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		isGamePaused = false;
		panel.SetActive(false);
	}

	public void chooseWhite(GameObject panel) {
		if (currentPlayerIndex == 0) {
			SwapColorChoice();
		}
		isGamePaused = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		panel.SetActive(false);
	}

	public void chooseBlack(GameObject panel) {
		if (currentPlayerIndex == 1) {
			SwapColorChoice();
		}
		isGamePaused = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		panel.SetActive(false);
	}

}

