using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
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
	public int uppestMove;
	public int downestMove;
	public int leftestMove;
	public int rightestMove;

	public State(State state) {
		this.map = new int[GomokuPlay.SIZE, GomokuPlay.SIZE];
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
		this.uppestMove = state.uppestMove;
		this.downestMove = state.downestMove;
		this.leftestMove = state.leftestMove;
		this.rightestMove = state.rightestMove;

		for (int y = 0; y < GomokuPlay.SIZE; y++) {
			for (int x = 0; x < GomokuPlay.SIZE; x++) {
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
	public int uppestMove;
	public int downestMove;
	public int leftestMove;
	public int rightestMove;

	public bool swapped;
	public bool putTwoMoreStones;

	public NetworkInstanceId currentNetId;
}

public class GomokuPlay : MonoBehaviour  {
	[Header("Assign the correct one")]
	public GameManager offlineManager;
	public MatchManager onlineManager;

	[Header("Game Settings")]
	public int AI_DEPTH = 3;
	public float AI_SEARCH_TIME = 0.5f;
	public int AI_MAX_SEARCHES_PER_DEPTH = 30;
	public bool DOUBLE_THREE_RULE = true;
	public bool SELF_CAPTURE_RULE = false;
	public int CAPTURES_NEEDED_TO_WIN = 10;
	public int HEURISTIC_ALIGN_COEFF = 5;
	public int HEURISTIC_CAPTURE_COEFF = 67;
	public int HANDICAP = 1;

	[Header("Map values")]
	// Map values
	public static int SIZE = 19;
	public const int EMPTY_VALUE = 0;
	public const int P1_VALUE = 1;
	public const int P2_VALUE = 2;
	public const int DT_P1_VALUE = -1;
	public const int DT_P2_VALUE = -2;
	public const int DT_P_VALUE = -3;
	public const int NA_P1_VALUE = -4;
	public const int NA_P2_VALUE = -5;
	public const int NA_P_VALUE = -6;
	public const int HANDICAP_CANT_PlAY = -7;

	// Game logic vars
	[HideInInspector] public bool isGameLoaded = false;
	[HideInInspector] public bool isGameEnded = false;
	[HideInInspector] public bool isGamePaused = false;
	[HideInInspector] public float startSearchTime;
	[HideInInspector] public float searchTime;
	[HideInInspector] public int currentPlayerIndex = 0;
	[HideInInspector] public NetworkInstanceId currentPlayerNetId = NetworkInstanceId.Invalid;
	[HideInInspector] public NetworkInstanceId p1NetId = NetworkInstanceId.Invalid;
	[HideInInspector] public NetworkInstanceId p2NetId = NetworkInstanceId.Invalid;
	[HideInInspector] public int currentPlayerVal = P1_VALUE;
	[HideInInspector] public int otherPlayerVal = P2_VALUE;
	[HideInInspector] public int[,] boardMap;
	[HideInInspector] public int[] playerScores;
	[HideInInspector] public bool[] isHumanPlayer;
	[HideInInspector] public bool isAIPlaying;
	[HideInInspector] public Vector3Int bestMove;
	[HideInInspector] public List<Vector2Int> counterMoves; // Moves that can break a winning align
	[HideInInspector] public List<Vector3Int> studiedMoves; // Used as debug to see AI studied moves
	[HideInInspector] public List<Vector2Int> lastMoves;
	[HideInInspector] public Vector2Int highlightedMove;
	[HideInInspector] public int mostUpMove;
	[HideInInspector] public int mostDownMove;
	[HideInInspector] public int mostLeftMove;
	[HideInInspector] public int mostRightMove;
	[HideInInspector] public bool moveIsReady = false;
	[HideInInspector] public bool simulatingMove = false;
	[HideInInspector] public bool alignmentHasBeenDone = false;

	[HideInInspector] public bool swappedColors = false;
	[HideInInspector] public bool playedTwoMoreStones = false;

	[HideInInspector] public int nbrOfMoves = 0;

	[HideInInspector] public List<int> allowedSpacesP1;
	[HideInInspector] public List<int> allowedSpacesP2;

	[HideInInspector] public List<BackupState> backupStates;

#region MainFunctions
	public void Init() {
		isGameLoaded = false;
		isGameEnded = false;
		isGamePaused = false;

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
		backupStates = new List<BackupState>();
		boardMap = new int[SIZE, SIZE];
		playerScores = new int[2];
		for (int i = 0; i < playerScores.Length; i++) {
			playerScores[i] = 0;
		}
		isHumanPlayer = new bool[2];
		isHumanPlayer[0] = true;
		isHumanPlayer[1] = true;
		isAIPlaying = false;
		counterMoves = new List<Vector2Int>();
		studiedMoves = new List<Vector3Int>();
		bestMove = new Vector3Int();
		lastMoves = new List<Vector2Int>();
		highlightedMove = new Vector2Int(-1, -1);
		moveIsReady = false;
		simulatingMove = false;
		alignmentHasBeenDone = false;
		swappedColors = false;
		allowedSpacesP1 = new List<int>();
		allowedSpacesP1.Add(GomokuPlay.EMPTY_VALUE);
		allowedSpacesP1.Add(GomokuPlay.DT_P2_VALUE);
		allowedSpacesP1.Add(GomokuPlay.NA_P2_VALUE);
		allowedSpacesP2 = new List<int>();
		allowedSpacesP2.Add(GomokuPlay.EMPTY_VALUE);
		allowedSpacesP2.Add(GomokuPlay.DT_P1_VALUE);
		allowedSpacesP2.Add(GomokuPlay.NA_P1_VALUE);
		nbrOfMoves = 0;
		mostUpMove = -1; // Setting one to -1 will trigger the setting of all of them

		// Can play against AI only in offline
		if (offlineManager != null) {
			if (PlayerPrefs.HasKey(CommonDefines.IS_P1_IA) && PlayerPrefs.GetInt(CommonDefines.IS_P1_IA) == 1) {
				isHumanPlayer[0] = false;
			}
			if (PlayerPrefs.HasKey(CommonDefines.IS_P2_IA) && PlayerPrefs.GetInt(CommonDefines.IS_P2_IA) == 1) {
				isHumanPlayer[1] = false;
			}
		}

		// Handle who starts first
		if (PlayerPrefs.HasKey(CommonDefines.FIRST_PLAYER_PLAYING)) {
			currentPlayerIndex = PlayerPrefs.GetInt(CommonDefines.FIRST_PLAYER_PLAYING);
			if (currentPlayerIndex == 2)
				currentPlayerIndex = Mathf.RoundToInt(UnityEngine.Random.Range(0f, 1f));
		}

		currentPlayerVal = (currentPlayerIndex == 0) ? GomokuPlay.P1_VALUE : GomokuPlay.P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;
		currentPlayerNetId = (currentPlayerIndex == 0) ? p1NetId : p2NetId;

		// Update scores
		if (offlineManager != null) {
			offlineManager.UpdateHasSwapped(false);
			offlineManager.UpdateScore(0, playerScores[0]);
			offlineManager.UpdateScore(1, playerScores[1]);
			offlineManager.UpdateActivePlayer(currentPlayerIndex);
		}
		else if (onlineManager != null) {
			onlineManager.UpdateHasSwapped(false);
			onlineManager.UpdateScore(0, playerScores[0]);
			onlineManager.UpdateScore(1, playerScores[1]);
			onlineManager.UpdateActivePlayer(currentPlayerIndex);
		}

	}

	void DispalyBoard(int[,] map) {
		for (int y = 0; y < SIZE; y++) {
			string str = y + " ->  ";
			for (int x = 0; x < SIZE; x++) {
				if (x != 0)
					str += " ";
				str += map[y, x].ToString();
			}
			Debug.Log(str);
		}
	}

	public int[,] CopyMap(int[,] map) {
		int[,] newMap = new int[SIZE, SIZE];
		for (int y = 0; y < SIZE; y++) {
			for (int x = 0; x < SIZE; x++) {
				newMap[y,x] = map[y,x];
			}
		}
		return newMap;
	}

	public void ClearHighligtedStone() {
		if (highlightedMove.y != -1 && highlightedMove.x != -1) {
			if (offlineManager != null)
				offlineManager.DeleteStone(highlightedMove.y, highlightedMove.x);
			else if (onlineManager != null)
				onlineManager.DeleteStone(highlightedMove.y, highlightedMove.x);
			highlightedMove.y = -1;
			highlightedMove.x = -1;
		}
	}

	public void DeleteStone(int[,] map, int yCoord, int xCoord, bool isAiSimulation = false) {
		map[yCoord, xCoord] = EMPTY_VALUE;
		if (isAiSimulation)
			return;

		if (offlineManager != null)
			offlineManager.DeleteStone(yCoord, xCoord);
		else if (onlineManager != null)
			onlineManager.DeleteStone(yCoord, xCoord);
	}

	public bool IsAiTurn() {
		if (isGameLoaded && !isGameEnded && !isGamePaused && !isAIPlaying && !isHumanPlayer[currentPlayerIndex])
			return true;
		return false;

	}

	public bool PlayerCanPutStone() {
		if (!isGameLoaded || isGameEnded || isGamePaused || simulatingMove || !isHumanPlayer[currentPlayerIndex])
			return false;
		return true;
	}
#endregion

#region PutStone
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
			newBackup.putTwoMoreStones = playedTwoMoreStones;
			newBackup.swapped = swappedColors;
			newBackup.uppestMove = mostUpMove;
			newBackup.downestMove = mostDownMove;
			newBackup.leftestMove = mostLeftMove;
			newBackup.rightestMove = mostRightMove;

			newBackup.counterMoves = new List<Vector2Int>();
			for (int i = 0; i < counterMoves.Count; i++) {
				newBackup.counterMoves.Insert(i, counterMoves[i]);
			}

			newBackup.lastMoves = new List<Vector2Int>();
			for (int i = 0; i < lastMoves.Count; i++) {
				newBackup.lastMoves.Insert(i, lastMoves[i]);
			}

			newBackup.currentNetId = currentPlayerNetId;

			backupStates.Insert(0, newBackup);
		}

		// If any, clear highligted stone
		ClearHighligtedStone();

		// Update last move tracker
		if (lastMoves.Count > 0) {
			if (offlineManager != null)
				offlineManager.ToggleStoneHighlight(lastMoves[0].y, lastMoves[0].x, false);
			else if (onlineManager != null)
				onlineManager.ToggleStoneHighlight(lastMoves[0].y, lastMoves[0].x, false);
		}
		lastMoves.Insert(0, new Vector2Int(xCoord, yCoord));
		if (lastMoves.Count > 3)
			lastMoves.RemoveAt(3);

		// Update most Up/Down/Left/Right moves
		if (mostUpMove == -1) {
			mostUpMove = yCoord;
			mostDownMove = yCoord;
			mostLeftMove = xCoord;
			mostRightMove = xCoord;
		}
		else {
			if (xCoord > mostRightMove)
				mostRightMove = xCoord;
			if (xCoord < mostLeftMove)
				mostLeftMove = xCoord;
			if (yCoord < mostDownMove)
				mostDownMove = yCoord;
			if (yCoord > mostUpMove)
				mostUpMove = yCoord;
		}

		// Actually put the stone
		boardMap[yCoord, xCoord] = currentPlayerVal;

		if (offlineManager != null)
			offlineManager.PutStoneUI(currentPlayerIndex, yCoord, xCoord);
		else if (onlineManager != null)
			onlineManager.PutStoneUI(currentPlayerIndex, yCoord, xCoord);

		nbrOfMoves++;

		// Do captures
		int captures = CheckCaptures(boardMap, yCoord, xCoord, currentPlayerVal, otherPlayerVal, doCapture: true);
		if (captures > 0) {
			playerScores[currentPlayerIndex] += captures;

			if (offlineManager != null)
				offlineManager.UpdateScore(currentPlayerIndex, playerScores[currentPlayerIndex]);
			else if (onlineManager != null)
				onlineManager.UpdateScore(currentPlayerIndex, playerScores[currentPlayerIndex]);

			if (playerScores[currentPlayerIndex] == CAPTURES_NEEDED_TO_WIN) {
				if (offlineManager != null)
					offlineManager.DisplayWinner(currentPlayerIndex, true);
				else if (onlineManager != null)
					onlineManager.DisplayWinner(currentPlayerIndex, true);
				isGameEnded = true;
				return;
			}
		}

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
				if (offlineManager != null)
					offlineManager.DisplayWinner(1 - currentPlayerIndex, false);
				else if (onlineManager != null)
					onlineManager.DisplayWinner(1 - currentPlayerIndex, false);
				isGameEnded = true;
				return;
			}
		}

		// check if a winning allignement has been done in current PutStone and if there is a possible countermove
		counterMoves.Clear();
		alignmentHasBeenDone = false;
		if (IsWinByAlignment(boardMap, yCoord, xCoord, currentPlayerVal, otherPlayerVal, playerScores[1 - currentPlayerIndex], ref alignmentHasBeenDone, ref counterMoves)) {
			if (offlineManager != null)
				offlineManager.DisplayWinner(currentPlayerIndex, false);
			else if (onlineManager != null)
				onlineManager.DisplayWinner(currentPlayerIndex, false);
			isGameEnded = true;
			return;
		}

		// End turn, next player to play
		currentPlayerIndex = 1 - currentPlayerIndex;
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;

		// update allowed movements in map
		bool thereIsAvailableMove = false;
		bool isInVerticalZone;
		bool isInCheckZone;
		for (int y = 0; y < SIZE; y++) {
			isInVerticalZone = (y <= mostUpMove + 2) && (y >= mostDownMove - 2);
			for (int x = 0; x < SIZE; x++) {
				isInCheckZone = isInVerticalZone && (x <= mostRightMove + 2) && (x >= mostLeftMove - 2);
				if (boardMap[y, x] != P1_VALUE && boardMap[y, x] != P2_VALUE) {
					DeleteStone(boardMap, y, x);

					if (DOUBLE_THREE_RULE && isInCheckZone)
						UpdateDoubleThree(boardMap, y, x, currentPlayerVal, otherPlayerVal);
					if (SELF_CAPTURE_RULE && isInCheckZone)
						UpdateSelfCapture(boardMap, y, x, currentPlayerVal, otherPlayerVal);
				}

				//Chef if it's a draw
				if (!thereIsAvailableMove) {
					if (currentPlayerVal == P1_VALUE && allowedSpacesP1.Contains(boardMap[y, x])) {
						thereIsAvailableMove = true;
					}
					else if (currentPlayerVal == P2_VALUE && allowedSpacesP2.Contains(boardMap[y, x])) {
						thereIsAvailableMove = true;
					}
				}
			}
		}

		// If no move available, display draw
		if (!thereIsAvailableMove) {
			if (offlineManager != null)
				offlineManager.DisplayWinner(-1, false);
			else if (onlineManager != null)
				onlineManager.DisplayWinner(-1, false);
			isGameEnded = true;
			return;
		}

		// Apply opening rules
		OpeningRules();

		// Swap highlight color
		if ((HANDICAP == 4 || HANDICAP == 5) && nbrOfMoves < 3) {
			return ;
		}
		else if (HANDICAP == 5 && nbrOfMoves < 5 && playedTwoMoreStones) {
			return ;
		}
		else if (HANDICAP == 5 && nbrOfMoves == 5 && playedTwoMoreStones) {
			if (offlineManager != null) {
				offlineManager.UpdateActivePlayer(1 - currentPlayerIndex);
			}
			else if (onlineManager != null) {
				onlineManager.UpdateActivePlayer(1 - currentPlayerIndex);
			}
		}
		else {
			if (offlineManager != null) {
				offlineManager.UpdateActivePlayer(currentPlayerIndex);
			}
			else if (onlineManager != null) {
				onlineManager.UpdateActivePlayer(currentPlayerIndex);
			}
		}
		currentPlayerNetId = (currentPlayerNetId == p1NetId) ? p2NetId : p1NetId;
		
		// DispalyBoard(boardMap);
	}

	public void FakePutStone(ref State state, int yCoord, int xCoord) {
		// Update last moves
		state.lastStones.Insert(0, new Vector2Int(xCoord, yCoord));
		if (state.lastStones.Count > 3)
			state.lastStones.RemoveAt(3);

		// Update most Up/Down/Left/Right moves
		if (state.uppestMove == -1) {
			state.uppestMove = yCoord;
			state.downestMove = yCoord;
			state.leftestMove = xCoord;
			state.rightestMove = xCoord;
		}
		else {
			if (xCoord > state.rightestMove)
				state.rightestMove = xCoord;
			if (xCoord < state.leftestMove)
				state.leftestMove = xCoord;
			if (yCoord < state.downestMove)
				state.downestMove = yCoord;
			if (yCoord > state.uppestMove)
				state.uppestMove = yCoord;
		}

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
		bool isInVerticalZone;
		bool isInCheckZone;
		for (int y = 0; y < SIZE; y++) {
			isInVerticalZone = (y <= state.uppestMove + 2) && (y >= state.downestMove - 2);
			for (int x = 0; x < SIZE; x++) {
				isInCheckZone = isInVerticalZone && (x <= state.rightestMove + 2) && (x >= state.leftestMove - 2);
				if (state.map[y, x] != P1_VALUE && state.map[y, x] != P2_VALUE) {
					DeleteStone(state.map, y, x, isAiSimulation: true);

					if (DOUBLE_THREE_RULE && isInCheckZone)
						UpdateDoubleThree(state.map, y, x, state.myVal, state.enemyVal, isAiSimulation: true);
					if (SELF_CAPTURE_RULE && isInCheckZone)
						UpdateSelfCapture(state.map, y, x, state.myVal, state.enemyVal, isAiSimulation: true);
				}

				//Chef if it's a draw
				if (!thereIsAvailableMove) {
					if (currentPlayerVal == P1_VALUE && allowedSpacesP1.Contains(boardMap[y, x])) {
						thereIsAvailableMove = true;
					}
					else if (currentPlayerVal == P2_VALUE && allowedSpacesP2.Contains(boardMap[y, x])) {
						thereIsAvailableMove = true;
					}
				}
			}
		}


		if (!thereIsAvailableMove) {
			state.winner = -1;
			return;
		}
	}

#endregion

#region GameHelpers
	public void SimulateAiMove() {
		if (PlayerCanPutStone()) {
			simulatingMove = true;
			StartMinMax();

			// Save highlighted stone position
			ClearHighligtedStone();
			highlightedMove.y = bestMove.y;
			highlightedMove.x = bestMove.x;

			if (offlineManager != null) {
				offlineManager.PutHighlightedStone(highlightedMove.y, highlightedMove.x);
			}
			else if (onlineManager != null) {
				onlineManager.PutHighlightedStone(highlightedMove.y, highlightedMove.x);
			}

			simulatingMove = false;
		}
	}

	public void GoBack() {

		if (backupStates.Count == 0 || isAIPlaying || (!isHumanPlayer[currentPlayerIndex] && !isGameEnded))
			return ;
		isGameLoaded = false;
		if (isGameEnded) {
			isGameEnded = false;
		}
		BackupState oldState = backupStates[0];

		boardMap = CopyMap(oldState.map);
		playerScores[0] = oldState.playerScores[0];
		playerScores[1] = oldState.playerScores[1];
		mostUpMove = oldState.uppestMove;
		mostDownMove = oldState.downestMove;
		mostLeftMove = oldState.leftestMove;
		mostRightMove = oldState.rightestMove;
		alignmentHasBeenDone = oldState.alignmentHasBeenDone;
		playedTwoMoreStones = oldState.putTwoMoreStones;
		swappedColors = oldState.swapped;
		// Send swap state to all multi clients
		if (offlineManager != null)
			offlineManager.UpdateHasSwapped(swappedColors);
		if (onlineManager != null)
			onlineManager.UpdateHasSwapped(swappedColors);

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
		currentPlayerNetId = oldState.currentNetId;
		currentPlayerVal = (currentPlayerIndex == 0) ? P1_VALUE : P2_VALUE;
		otherPlayerVal = (currentPlayerIndex == 0) ? P2_VALUE : P1_VALUE;
		

		// first reset everything and put stones back
		int playerIndex = -1;
		int tmpVal = EMPTY_VALUE;
		for (int y = 0; y < SIZE; y++) {
			for (int x = 0; x < SIZE; x++) {
				tmpVal = boardMap[y, x];
				DeleteStone(boardMap, y, x);

				playerIndex = -1;
				if (tmpVal == P1_VALUE)
					playerIndex = 0;
				else if (tmpVal == P2_VALUE)
					playerIndex = 1;

				if (playerIndex >= 0) {
					if (offlineManager != null)
						offlineManager.PutStoneUI(playerIndex, y, x);
					else if (onlineManager != null)
						onlineManager.PutStoneUI(playerIndex, y, x);

					if (lastMoves[0].y == y && lastMoves[0].x == x) {
						if (offlineManager != null)
							offlineManager.ToggleStoneHighlight(y, x, true);
						else if (onlineManager != null)
							onlineManager.ToggleStoneHighlight(y, x, true);
					}
					else {
						if (offlineManager != null)
							offlineManager.ToggleStoneHighlight(y, x, false);
						else if (onlineManager != null)
							onlineManager.ToggleStoneHighlight(y, x, false);
					}

					boardMap[y,x] = tmpVal;
				}
			}
		}

		// second iteration to update allowed moves
		for (int y = 0; y < SIZE; y++) {
			for (int x = 0; x < SIZE; x++) {
				if (boardMap[y, x] != P1_VALUE && boardMap[y, x] != P2_VALUE) {
					if (DOUBLE_THREE_RULE)
						UpdateDoubleThree(boardMap, y, x, currentPlayerVal, otherPlayerVal);
					if (SELF_CAPTURE_RULE)
						UpdateSelfCapture(boardMap, y, x, currentPlayerVal, otherPlayerVal);
				}
			}
		}

		// Change UI
		if (offlineManager != null) {
			offlineManager.UpdateScore(0, playerScores[0]);
			offlineManager.UpdateScore(1, playerScores[1]);
			offlineManager.UpdateActivePlayer(currentPlayerIndex);
		}
		else if (onlineManager != null) {
			onlineManager.UpdateScore(0, playerScores[0]);
			onlineManager.UpdateScore(1, playerScores[1]);
			onlineManager.UpdateActivePlayer(currentPlayerIndex);
		}

		nbrOfMoves = nbrOfMoves - 1;

		// Third iteration to add Handicaps
		if (nbrOfMoves == 2 && HANDICAP == 3) {
			SetForbiddenMove(7, 12);
		}
		else if (nbrOfMoves == 2 && HANDICAP == 2) {
			SetForbiddenMove(5, 14);
		}
		else if (nbrOfMoves == 1 && (HANDICAP == 4 || HANDICAP == 5)) {
			// Force Change UI for swap rule
			if (offlineManager != null) {
				offlineManager.UpdateActivePlayer(1 - currentPlayerIndex);
			}
			else if (onlineManager != null) {
				onlineManager.UpdateActivePlayer(1 - currentPlayerIndex);
			}
		}
		else if (nbrOfMoves == 4 && HANDICAP == 5 && playedTwoMoreStones) {
			// Force Change UI for swap2 rule when played two more stones
			if (offlineManager != null) {
				offlineManager.UpdateActivePlayer(1 - currentPlayerIndex);
			}
			else if (onlineManager != null) {
				onlineManager.UpdateActivePlayer(1 - currentPlayerIndex);
			}
		}

		backupStates.RemoveAt(0);
		isGameLoaded = true;
	}
#endregion

#region AI
	public void StartMinMax() {
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
		if (offlineManager != null) {
			offlineManager.ToggleStudiedHighlight(false);
		}

		studiedMoves = GetAllowedMoves(state);

		if (offlineManager != null) {
			offlineManager.ToggleStudiedHighlight(true);
		}

		// Saving first move as best move
		bestMove = studiedMoves[0];
		bestMove.z = Int32.MinValue;

		// Actually do MinMax
		AlphaBeta(state, Int32.MinValue, Int32.MaxValue, true);

		// Save searchTime
		searchTime = Time.realtimeSinceStartup - startSearchTime;

		if (offlineManager != null) {
			offlineManager.UpdateTimer();
		}
		else if (onlineManager != null) {
			onlineManager.UpdateTimer();
		}
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
			for (int y = 0; y < SIZE; y++) {
				for (int x = 0; x < SIZE; x++) {
					if (allowedSpaces.Contains(state.map[y, x])) {
						heuristicVal = GetMoveHeuristic(state, y, x);
						allowedMoves.Add(new Vector3Int(x, y, heuristicVal));
					}
				}
			}
		}

		int maxSearches = AI_MAX_SEARCHES_PER_DEPTH;
		if (nbrOfMoves == 0 && state.depth == 0)
			maxSearches = Mathf.Min(9, maxSearches);
		else if ((nbrOfMoves == 1 && state.depth == 0) || (nbrOfMoves == 0 && state.depth == 1))
			maxSearches = Mathf.Min(16, maxSearches);
		allowedMoves = allowedMoves.OrderByDescending(move => move.z).Take(maxSearches).ToList();
		return allowedMoves;
	}

	private int GetMoveHeuristic(State state, int yCoord, int xCoord) {
		int score = 0;
		int captures = 0;

		int middle = SIZE / 2;
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
		if ((state.myVal == state.rootVal && captures + state.rootPlayerScore >= CAPTURES_NEEDED_TO_WIN)
			 || (state.myVal != state.rootVal && captures + state.otherPlayerScore >= CAPTURES_NEEDED_TO_WIN)) {
			return Int32.MaxValue;
		}
		score += HEURISTIC_CAPTURE_COEFF * captures;

		captures = CheckCaptures(state.map, yCoord, xCoord, state.enemyVal, state.myVal, doCapture:false, isAiSimulation: true);
		if ((state.enemyVal == state.rootVal && captures + state.rootPlayerScore >= CAPTURES_NEEDED_TO_WIN)
			 || (state.myVal != state.rootVal && captures + state.otherPlayerScore >= CAPTURES_NEEDED_TO_WIN)) {
			return Int32.MaxValue;
		}
		score += HEURISTIC_CAPTURE_COEFF * captures;

		// Increase move value based on neighbours influence
		captures += GetStoneInfluence(state, yCoord, xCoord);
		if (captures == Int32.MaxValue)
			return captures - 1;
		score += captures;

		return score;
	}

	private int GetStoneInfluence(State state, int yCoord, int xCoord) {
		int influence = 0;

		influence = GetRadialStoneInfluence(state, yCoord, xCoord, 0, 1);
		influence += GetRadialStoneInfluence(state, yCoord, xCoord, 1, 0);
		influence += GetRadialStoneInfluence(state, yCoord, xCoord, 1, 1);
		influence += GetRadialStoneInfluence(state, yCoord, xCoord, -1, 1);

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
		while (x >= 0 && x < SIZE && y >= 0 && y < SIZE) {
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
		while (x >= 0 && x < SIZE && y >= 0 && y < SIZE) {
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
			stateScore -= HEURISTIC_CAPTURE_COEFF * state.otherPlayerScore + (int)Mathf.Pow(2, state.otherPlayerScore);

		return stateScore;
	}

	private int GetScoreOfAligns(State state) {
		int score = 0;
		int isPartOfAlign = 0;
		int tmpVal = 0;
		int otherVal = (state.rootVal == P1_VALUE) ? P2_VALUE : P1_VALUE;

		for (int y = 0; y < SIZE; y++) {
			for (int x = 0; x < SIZE; x++) {
				if (state.map[y, x] == P1_VALUE || state.map[y, x] == P2_VALUE) {
					isPartOfAlign = 0;
					score += RadialAlignScore(state, y, x, 0, 1, ref isPartOfAlign);
					score += RadialAlignScore(state, y, x, 1, 0, ref isPartOfAlign);
					score += RadialAlignScore(state, y, x, 1, 1, ref isPartOfAlign);
					score += RadialAlignScore(state, y, x, 1, -1, ref isPartOfAlign);
					if (isPartOfAlign >= 2) {
						if (state.map[y, x] == state.rootVal)
							score += isPartOfAlign;
						else
							score -= isPartOfAlign;
					}
				}
				else if (state.map[y, x] == EMPTY_VALUE) { // Easy case
					tmpVal = CheckCaptures(state.map, y, x, state.rootVal, otherVal, doCapture: false, isAiSimulation: true);
					if (tmpVal > 0) {
						score += (HEURISTIC_CAPTURE_COEFF * tmpVal + (int)Mathf.Pow(2, state.rootPlayerScore)) / 2;
					}
					tmpVal = CheckCaptures(state.map, y, x, otherVal, state.rootVal, doCapture: false, isAiSimulation: true);
					if (tmpVal > 0) {
						score -= (HEURISTIC_CAPTURE_COEFF * tmpVal + (int)Mathf.Pow(2, state.otherPlayerScore)) / 2;
					}
				}
				else if (allowedSpacesP1.Contains(state.map[y, x])) { // Case for P1
					if (state.rootVal == P1_VALUE) {
						tmpVal = CheckCaptures(state.map, y, x, state.rootVal, otherVal, doCapture: false, isAiSimulation: true);
						if (tmpVal > 0) {
							score += (HEURISTIC_CAPTURE_COEFF * tmpVal + (int)Mathf.Pow(2, state.rootPlayerScore)) / 2;
						}
					}
					else {
						tmpVal = CheckCaptures(state.map, y, x, otherVal, state.rootVal, doCapture: false, isAiSimulation: true);
						if (tmpVal > 0) {
							score -= (HEURISTIC_CAPTURE_COEFF * tmpVal + (int)Mathf.Pow(2, state.otherPlayerScore)) / 2;
						}
					}
				}
				else if (allowedSpacesP2.Contains(state.map[y, x])) { // Case for P2
					if (state.rootVal == P2_VALUE) {
						tmpVal = CheckCaptures(state.map, y, x, state.rootVal, otherVal, doCapture: false, isAiSimulation: true);
						if (tmpVal > 0) {
							score += (HEURISTIC_CAPTURE_COEFF * tmpVal + (int)Mathf.Pow(2, state.rootPlayerScore)) / 2;
						}
					}
					else {
						tmpVal = CheckCaptures(state.map, y, x, otherVal, state.rootVal, doCapture: false, isAiSimulation: true);
						if (tmpVal > 0) {
							score -= (HEURISTIC_CAPTURE_COEFF * tmpVal + (int)Mathf.Pow(2, state.otherPlayerScore)) / 2;
						}
					}
				}
			}
		}
		return score;
	}

	private int RadialAlignScore(State state, int yCoord, int xCoord, int yCoeff, int xCoeff, ref int isPartOfAlign) {
		int score = 0;
		bool backBlocked = false;
		bool frontBlocked = false;
		bool jumpedSpace = false;
		int nbrStone = 1;
		int nbrSideStone = 0;
		List<int> allowedSpaces = (state.map[yCoord, xCoord] == P1_VALUE) ? allowedSpacesP1 : allowedSpacesP2;

		int y = yCoord - yCoeff;
		int x = xCoord - xCoeff;
		if (x >= 0 && x < SIZE && y >= 0 && y < SIZE) {
			if (state.map[y, x] == state.map[yCoord, xCoord]) {// Exit if is part of an align we have already evaluated
				isPartOfAlign++;
				return 0;
			}
			if (!allowedSpaces.Contains(state.map[y, x]))
				backBlocked = true;
		}
		else {
			backBlocked = true;
		}

		y = yCoord + yCoeff;
		x = xCoord + xCoeff;
		while (true) {
			if (x < 0 || x >= SIZE || y < 0 || y >= SIZE) {
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

		if (nbrSideStone > 0)
			isPartOfAlign++;

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
		return -score;
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
		if (yCoord + 3 < SIZE && map[yCoord + 3, xCoord] == myVal && CanCapture(map, yCoord, xCoord, 1, 0, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Right
		if (xCoord + 3 < SIZE && map[yCoord, xCoord + 3] == myVal && CanCapture(map, yCoord, xCoord, 0, 1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Bot right
		if (xCoord + 3 < SIZE && yCoord + 3 < SIZE && map[yCoord + 3, xCoord + 3] == myVal && CanCapture(map, yCoord, xCoord, 1, 1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Bot left
		if (xCoord - 3 >= 0 && yCoord + 3 < SIZE && map[yCoord + 3, xCoord - 3] == myVal && CanCapture(map, yCoord, xCoord, 1, -1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Top left
		if (xCoord - 3 >= 0 && yCoord - 3 >= 0 && map[yCoord - 3, xCoord - 3] == myVal && CanCapture(map, yCoord, xCoord, -1, -1, enemyVal, doCapture, isAiSimulation)) {
			canCapture += 2;
		}
		// Top right
		if (xCoord + 3 < SIZE && yCoord - 3 >= 0 && map[yCoord - 3, xCoord + 3] == myVal && CanCapture(map, yCoord, xCoord, -1, 1, enemyVal, doCapture, isAiSimulation)) {
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

		bool checkHorizontal = xCoord > 0 && xCoord < SIZE -1;
		bool checkVertical = yCoord > 0 && yCoord < SIZE -1;

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
				if (offlineManager != null)
					offlineManager.PutDoubleTree(yCoord, xCoord);
				else if (onlineManager != null)
					onlineManager.PutDoubleTree(yCoord, xCoord);
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
		int x = xCoord + xCoeff * 3;
		int y = yCoord + yCoeff * 3;

		// check when coord is middle of free-tree
		if (map[yCoord - yCoeff, xCoord - xCoeff] == myVal) {
			y = yCoord - yCoeff * 2;
			x = xCoord - xCoeff * 2;
			if (x >= 0 && x < SIZE && y >= 0 && y < SIZE && map[y, x] != enemyVal && map[y, x] != myVal) {
				y = yCoord + yCoeff * 2;
				x = xCoord + xCoeff * 2;
				if (x >= 0 && x < SIZE && y >= 0 && y < SIZE) {
					if (map[yCoord + yCoeff, xCoord + xCoeff] == myVal) {
						if (map[y, x] != enemyVal && map[y, x] != myVal && middleCheck) {
							// Debug.Log("Free tree type1 at " + yCoord + " " + xCoord + " coeff " + yCoeff + " " + xCoeff);
							return true;
						}
					}
					else if (map[y, x] == myVal) {
						y += yCoeff;
						x += xCoeff;
						if (x >= 0 && x < SIZE && y >= 0 && y < SIZE && map[y, x] != myVal && map[y, x] != enemyVal) {
							// Debug.Log("Free tree type2 at " + yCoord + " " + xCoord + " coeff " + yCoeff + " " + xCoeff);
							return true;
						}
					}
				}

			} 
		}
		// check when coord is start of free-tree
		else if (y < SIZE && y >= 0 && x < SIZE && x >= 0) {
			x = 0;
			y = 0;
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
				x += xCoord + xCoeff;
				y += yCoord + yCoeff;
				if (x >= 0 && x < SIZE && y >= 0 && y < SIZE)
					if (map[y, x] != enemyVal && map[y, x] != myVal) {
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

		bool checkLeft = xCoord - 2 >= 0 && xCoord + 1 < SIZE;
		bool checkRight = xCoord + 2 < SIZE && xCoord - 1 >= 0;
		bool checkTop = yCoord - 2 >= 0 && yCoord + 1 < SIZE;
		bool checkBot = yCoord + 2 < SIZE && yCoord - 1 >= 0;
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
				if (offlineManager != null)
					offlineManager.PutSelfCapture(yCoord, xCoord);
				else if (onlineManager != null)
					onlineManager.PutSelfCapture(yCoord, xCoord);
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
		List<int> goodSpaces = (myVal == P1_VALUE) ? allowedSpacesP1 : allowedSpacesP2;
		for (int y = 0; y < SIZE; y++) {
			for (int x = 0; x < SIZE; x++) {
				if (goodSpaces.Contains(map[y, x])) {
					if (myScore + CheckCaptures(map, y, x, myVal, enemyVal, doCapture: false, isAiSimulation: true) >= CAPTURES_NEEDED_TO_WIN) {
						return true;
					}
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
		while (x >= 0 && x < SIZE && y >= 0 && y < SIZE && map[y, x] == myVal) {
			neighbours++;
			x -= xCoeff;
			y -= yCoeff;
		}

		// Forward check
		x = xCoord + xCoeff;
		y = yCoord + yCoeff;
		while (x >= 0 && x < SIZE && y >= 0 && y < SIZE && map[y, x] == myVal) {
			neighbours++;
			x += xCoeff;
			y += yCoeff;
		}

		return (neighbours >= 4);
	}

	private bool CanCounterMove(int[,] map, int yCoord, int xCoord, int yCoeff, int xCoeff, int myVal, int enemyVal, ref List<Vector2Int> refCaptureMoves) {
		// 1) Find all possible counterMoves for an alignement
		// Only check if can capture central stones: to counter a 5-alignement any capture is good, for a 6-align only middle 4 captures, for 7-align only middle 3, and so on

		// Find borders to know align length
		Vector2Int minStone = new Vector2Int(xCoord, yCoord);
		Vector2Int maxStone = new Vector2Int(xCoord, yCoord);
		int x = xCoord + xCoeff;
		int y = yCoord + yCoeff;
		while (x >= 0 && x < SIZE && y >= 0 && y < SIZE && map[y, x] == myVal) {
			maxStone.x = x;
			maxStone.y = y;
			x += xCoeff;
			y += yCoeff;
		}
		x = xCoord - xCoeff;
		y = yCoord - yCoeff;
		while (x >= 0 && x < SIZE && y >= 0 && y < SIZE && map[y, x] == myVal) {
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
		bool checkLeft = xCoord - 2 >= 0 && xCoord + 1 < SIZE;
		bool checkRight = xCoord + 2 < SIZE && xCoord - 1 >= 0;
		bool checkTop = yCoord - 2 >= 0 && yCoord + 1 < SIZE;
		bool checkBot = yCoord + 2 < SIZE && yCoord - 1 >= 0;

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

#region Opening Rules
	public void OpeningRules() {
		if (nbrOfMoves == 2 && HANDICAP == 3) {
			SetForbiddenMove(7, 12);
		}
		else if (nbrOfMoves == 2 && HANDICAP == 2) {
			SetForbiddenMove(5, 14);
		}
		else if ((HANDICAP == 4 && nbrOfMoves == 3) || (playedTwoMoreStones && nbrOfMoves == 5)) {
			if (offlineManager != null)
				offlineManager.ShowSwapChoice();
			else if (onlineManager != null)
				onlineManager.ShowSwapChoice();
		}
		else if (HANDICAP == 5 && nbrOfMoves == 3) {
			if (offlineManager != null)
				offlineManager.ShowSwap2Choice();
			else if (onlineManager != null)
				onlineManager.ShowSwap2Choice();
		}
	}

	private void SetForbiddenMove(int min, int max) {
		for (int y = min; y < max; y++) {
			for (int x = min; x < max; x++) {
				if (boardMap[y, x] == EMPTY_VALUE) {
					boardMap[y,x] = HANDICAP_CANT_PlAY;

					if (offlineManager)
						offlineManager.PutHandicap(min, max, y, x);
					else if (onlineManager)
						onlineManager.PutHandicap(min, max, y, x);
				}
			}
		}
	}

	public void DoSwap() {
		swappedColors = true;
		if (offlineManager != null)
			offlineManager.UpdateHasSwapped(true);
		if (onlineManager != null)
			onlineManager.UpdateHasSwapped(true);

		if (playedTwoMoreStones)
			return;

		currentPlayerNetId = (currentPlayerNetId == p1NetId) ? p2NetId : p1NetId;
		if (offlineManager != null)
			offlineManager.UpdateActivePlayer(currentPlayerIndex);
		else if (onlineManager != null)
			onlineManager.UpdateActivePlayer(currentPlayerIndex);
	}
#endregion
}
