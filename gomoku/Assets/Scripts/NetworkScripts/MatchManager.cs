using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Linq;
using System;

public class MatchManager : AbstractPlayerInteractable {
	public GomokuPlay gomoku;

	// Prefabs and UI
	public GameObject emptyButton;
	public GameObject startBoard;
	public GameObject menuPanel;
	public GameObject gameEndedPanel;

	public GameObject dialoguePanel;
	public Canvas canvas;
	public Text[] playersNames;
	public Image[] playersStones;
	public Text AiTimer;
	public Text displayWinner;
	public Sprite whiteStoneSprite;
	public Sprite blackStoneSprite;
	public Sprite notAllowedSprite;
	public Sprite doubleThreeSprite;

	// Sprites square
	public Sprite sqTopLeft;
	public Sprite sqTopRight;
	public Sprite sqBotRight;
	public Sprite sqBotLeft;
	public Sprite sqHorizontal;
	public Sprite sqVertical;

	// Server Vars
	private NetworkInstanceId dialogAnswerId = NetworkInstanceId.Invalid;
	private DialogueSubject ongoingSubject = DialogueSubject.None;

	// Client Vars
	string p1Name;
	string p2Name;
	Color p1Color;
	Color p2Color;
	private BoardButton[,] buttonsMap;
	private bool swappedColors = false;
	// private bool isGamePaused = false; // Client dependent

	// Override of base class
	protected override void Init() {
		base.Init();
		player.menuPanel = menuPanel;
	}
	
#region MonoFunctions
	void Update () {
		if (Screen.fullScreen == true)
			Screen.SetResolution(1024, 768, false);
		if (isServer && gomoku.IsAiTurn()) {
			gomoku.isAIPlaying = true;

			// start AI decision making
			gomoku.StartMinMax();

			gomoku.isAIPlaying = false;
		}
	}

	void LateUpdate() {
		if (isServer && gomoku.moveIsReady) {
			gomoku.moveIsReady = false;
			gomoku.PutStone(gomoku.bestMove.y, gomoku.bestMove.x);
			if (gomoku.isAIPlaying)
				gomoku.isAIPlaying = false;
		}
	}
#endregion

#region MainFunctions
	public void ToggleStoneHighlight(int yCoord, int xCoord, bool newState) {
			RpcClearMoveTracker(yCoord, xCoord);
	}

	public void PutStoneUI(int playerIndex, int yCoord, int xCoord) {
		RpcPutStone(playerIndex, yCoord, xCoord);
	}

	public void UpdateScore(int playerIndex, int score) {
		RpcChangePlayerScore(playerIndex, score);
	}

	public void DisplayWinner(int winnerIndex, bool byCapture) {
		RpcDisplayWinner(winnerIndex, byCapture);
	}

	public void DeleteStone(int yCoord, int xCoord) {
		RpcClearButton(yCoord, xCoord);
	}

	public void PutDoubleTree(int yCoord, int xCoord) {
		RpcPutDT(yCoord, xCoord);
	}

	public void PutSelfCapture(int yCoord, int xCoord) {
		RpcPutNA(yCoord, xCoord);
	}

	public void PutHighlightedStone(int yCoord, int xCoord) {
		RpcHighlightStone(gomoku.currentPlayerIndex, yCoord, xCoord);
		
	}

	public void PutHandicap(int min, int max, int yCoord, int xCoord) {
		RpcPutHandicap(min, max, yCoord, xCoord);
	}

	public void UpdateTimer() {
		RpcChangeAiTimer(gomoku.searchTime, gomoku.AI_SEARCH_TIME);
	}

	public void UpdateHasSwapped(bool hasSwapped) {
		RpcHasSwappedColors(hasSwapped);
	}

	public void UpdateActivePlayer(int playerIndex) {
		RpcChangePlayerHiglight(playerIndex);
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

#region Commands functions
	[Command]
	public void CmdRegisterPlayer(NetworkInstanceId playerNetId, string pName, Color pColor, bool serverPlayer) {
		Debug.Log("Registering.. playerId: " + playerNetId + ", player name: '" + pName + "', color: " + pColor + ", serverPlayer: " + serverPlayer);
		if (playerNetId == NetworkInstanceId.Invalid || playerNetId == gomoku.p1NetId || playerNetId == gomoku.p2NetId) // just to be sure
			return;
		if (gomoku.p1NetId == NetworkInstanceId.Invalid && serverPlayer) {
			gomoku.p1NetId = playerNetId;
			NetworkServer.objects[playerNetId].GetComponent<PlayerHandler>().HasBeenRegistered();
			p1Name = pName;
			p1Color = pColor;
		}
		else if (gomoku.p2NetId == NetworkInstanceId.Invalid && !serverPlayer) {
			gomoku.p2NetId = playerNetId;
			NetworkServer.objects[playerNetId].GetComponent<PlayerHandler>().HasBeenRegistered();
			p2Name = pName;
			p2Color = pColor;
		}

		if (gomoku.p1NetId != NetworkInstanceId.Invalid && gomoku.p2NetId != NetworkInstanceId.Invalid) {
			RpcSetP1Info(p1Name, p1Color);
			RpcSetP2Info(p2Name, p2Color);
			CmdStart(firstStart: true);
		}
	}

	[Command]
	private void CmdStart (bool firstStart) {
		gomoku.Init();

		if (firstStart) {
			// Enable buttons for every player
			RpcLoadBoard();
		}
		else {
			// Be sure to have all int set to EMPTY_VALUE
			for (int y = 0; y < GomokuPlay.SIZE; y++) {
				for (int x = 0; x < GomokuPlay.SIZE; x++) {
					RpcClearButton(y, x);
				}
			}
		}


		// Start accepting inputs
		TargetHideDialoguePanel(NetworkServer.objects[gomoku.p1NetId].connectionToClient);
		TargetHideDialoguePanel(NetworkServer.objects[gomoku.p2NetId].connectionToClient);
		gomoku.isGameLoaded = true;
		Debug.Log("Game Loaded !");
	}

	[Command]
	public void CmdTrySavePlayerMove(NetworkInstanceId playerNetId, int yCoord, int xCoord) {
		if (!gomoku.PlayerCanPutStone() || gomoku.currentPlayerNetId != playerNetId)
			return ;
		gomoku.bestMove = new Vector3Int(xCoord, yCoord, -1);
		gomoku.moveIsReady = true;
	}

#endregion

#region RpcFunctions

	[ClientRpc]
	public void RpcSetP1Info(string pName, Color pColor) {
		p1Name = pName;
		p1Color = pColor;
		playersNames[0].text = pName + ": 0";
		playersStones[0].color = (p1Color == Color.black) ? Color.white : pColor;
		playersStones[0].sprite = (p1Color == Color.black) ? blackStoneSprite : whiteStoneSprite;
	}

	[ClientRpc]
	public void RpcSetP2Info(string pName, Color pColor) {
		p2Name = pName;
		p2Color = pColor;
		playersNames[1].text = pName + ": 0";
		playersStones[1].color = (p2Color == Color.black) ? Color.white : pColor;
		playersStones[1].sprite = (p2Color == Color.black) ? blackStoneSprite : whiteStoneSprite;
	}

	[ClientRpc]
	public void RpcLoadBoard() {
		buttonsMap = new BoardButton[GomokuPlay.SIZE, GomokuPlay.SIZE];
		float width = startBoard.GetComponent<RectTransform>().rect.width ;
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
				buttonsMap[y,x] = newButton.GetComponent<BoardButton>();
				buttonsMap[y, x].isEmpty = true;
				buttonsMap[y, x].player = player;


				// Equivalent of RpcClearButton, since its already a Rpc no need to call it
				newButton.transform.localScale = new Vector3(1, 1, 1);
				Image buttonImage = newButton.GetComponent<Image>();
				Color newColor = buttonImage.color;
				newColor.a = 0;
				buttonImage.color = newColor;
				buttonImage.sprite = null;
				newButton.transform.GetChild(0).gameObject.SetActive(false);

				x++;
				tmpPos.x += step;
			}
			y++;
			tmpPos.y -= step;
		}
	}

	[ClientRpc]
	public void RpcClearButton(int yCoord, int xCoord) {		
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		if (button == null) {
			Debug.LogWarning("Button not found: " + yCoord + " " + xCoord);
		}
		button.transform.localScale = new Vector3(1, 1, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = Color.white;
		newColor.a = 0;
		buttonImage.color = newColor;
		buttonImage.sprite = null;
		buttonsMap[yCoord, xCoord].isEmpty = true;
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	[ClientRpc]
	public void RpcPutStone(int playerIndex, int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = (playerIndex == 0) ? p1Color : p2Color;
		buttonImage.sprite = (newColor == Color.black) ? blackStoneSprite : whiteStoneSprite;
		newColor = (newColor == Color.black) ? Color.white : newColor;
		newColor.a = 1;
		buttonImage.color = newColor;
		buttonsMap[yCoord, xCoord].isEmpty = false;
		button.transform.GetChild(0).gameObject.SetActive(true);
	}

	[ClientRpc]
	public void RpcHighlightStone(int playerIndex, int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = (playerIndex == 0) ? p1Color : p2Color;
		buttonImage.sprite = (newColor == Color.black) ? blackStoneSprite : whiteStoneSprite;
		newColor = (newColor == Color.black) ? Color.white : newColor;
		newColor.a = 0.7f;
		buttonImage.color = newColor;
		button.transform.GetChild(0).gameObject.SetActive(true);
	}

	[ClientRpc]
	public void RpcPutDT(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = Color.white;
		newColor.a = 1;
		buttonImage.color = newColor;
		buttonImage.sprite = doubleThreeSprite;
		buttonsMap[yCoord, xCoord].isEmpty = false;
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	[ClientRpc]
	public void RpcPutNA(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.localScale = new Vector3(0.9f, 0.9f, 1);
		Image buttonImage = button.GetComponent<Image>();
		Color newColor = Color.white;
		newColor.a = 1;
		buttonImage.color = newColor;
		buttonImage.sprite = notAllowedSprite;
		buttonsMap[yCoord, xCoord].isEmpty = false;
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	[ClientRpc]
	public void RpcPutHandicap(int min, int max, int yCoord, int xCoord) {
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

	[ClientRpc]
	private void RpcClearMoveTracker(int yCoord, int xCoord) {
		GameObject button = buttonsMap[yCoord, xCoord].gameObject;
		button.transform.GetChild(0).gameObject.SetActive(false);
	}

	[ClientRpc]
	private void RpcDisplayWinner(int winnerIndex, bool byCapture) {
		if (winnerIndex == -1) {
			displayWinner.text = "Draw !";
		}
		else {
			string winner = (winnerIndex == 0) ? p1Name : p2Name;
			if (swappedColors)
				winner = (winnerIndex == 0) ? p2Name : p1Name;
			if (byCapture)
				displayWinner.text = winner + " won by capture !";
			else
				displayWinner.text = winner + " won by alignment!";
		}
		gameEndedPanel.SetActive(true);
	}

	[ClientRpc]
	private void RpcChangePlayerHiglight(int playerIndex) {
		if (swappedColors) {
			playersNames[1 - playerIndex].color = Color.cyan;
			playersNames[playerIndex].color = Color.white;
		}
		else {
			playersNames[playerIndex].color = Color.cyan;
			playersNames[1 - playerIndex].color = Color.white;
		}
	}

	[ClientRpc]
	private void RpcChangePlayerScore(int playerIndex, int score) {
		string pName;
		playerIndex = (swappedColors) ? 1 - playerIndex : playerIndex;
		pName = (playerIndex == 0) ? p1Name : p2Name;

		playersNames[playerIndex].text = pName + ": " + score;
	}

	[ClientRpc]
	private void RpcChangeAiTimer(float searchingTime, float maxSearchTime) {
		Debug.Log("Search time: " + gomoku.searchTime);
		AiTimer.text = "AI Timer: " + searchingTime.ToString();
		if (searchingTime >= maxSearchTime) {
			AiTimer.color = Color.red;
			Debug.LogWarning("Ai didnt find a move in time");
		}
		else
			AiTimer.color = Color.white;
	}

	[ClientRpc]
	private void RpcHasSwappedColors(bool hasSwapped) {
		swappedColors = hasSwapped;
		if (swappedColors) {
			playersStones[0].color = (p2Color == Color.black) ? Color.white : p2Color;
			playersStones[1].color = (p1Color == Color.black) ? Color.white : p1Color;
			playersStones[0].sprite = (p2Color == Color.black) ? blackStoneSprite : whiteStoneSprite;
			playersStones[1].sprite = (p1Color == Color.black) ? blackStoneSprite : whiteStoneSprite;
		}
		else {
			playersStones[0].color = (p1Color == Color.black) ? Color.white : p1Color;
			playersStones[1].color = (p2Color == Color.black) ? Color.white : p2Color;
			playersStones[0].sprite = (p1Color == Color.black) ? blackStoneSprite : whiteStoneSprite;
			playersStones[1].sprite = (p2Color == Color.black) ? blackStoneSprite : whiteStoneSprite;
		}
	}
#endregion

#region Client-to-Client dialogues
	private void HideAllPanels() {
		gameEndedPanel.SetActive(false);
		menuPanel.SetActive(false);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

    [TargetRpc]
	private void TargetHideDialoguePanel(NetworkConnection target) {
		dialoguePanel.SetActive(false);
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

    [TargetRpc]
    private void TargetWaitForResponse(NetworkConnection target, DialogueSubject subject)
    {
		HideAllPanels();
		dialoguePanel.SetActive(true);
		DialoguePanel panelScript = dialoguePanel.GetComponent<DialoguePanel>();
		panelScript.StartWaitForResponse(subject);
    }

    [TargetRpc]
    private void TargetShowDialogue(NetworkConnection target, DialogueSubject subject, string playerName)
    {
		HideAllPanels();
		dialoguePanel.SetActive(true);
		DialoguePanel panelScript = dialoguePanel.GetComponent<DialoguePanel>();
		panelScript.ShowOtherPlayerRequest(subject, playerName);
    }

	public void ShowSwapChoice() {
		string playerName = "";
		dialogAnswerId = gomoku.currentPlayerNetId;
		NetworkConnection target = NetworkServer.objects[dialogAnswerId].connectionToClient;
		NetworkConnection waiter = (dialogAnswerId == gomoku.p1NetId) ? NetworkServer.objects[gomoku.p2NetId].connectionToClient : NetworkServer.objects[gomoku.p1NetId].connectionToClient;

		ongoingSubject = DialogueSubject.DoSwap;
		TargetShowDialogue(target, ongoingSubject, playerName);
		TargetWaitForResponse(waiter, ongoingSubject);
	}

	public void ShowSwap2Choice() {
		string playerName = "";
		dialogAnswerId = gomoku.currentPlayerNetId;
		NetworkConnection target = NetworkServer.objects[dialogAnswerId].connectionToClient;
		NetworkConnection waiter = (dialogAnswerId == gomoku.p1NetId) ? NetworkServer.objects[gomoku.p2NetId].connectionToClient : NetworkServer.objects[gomoku.p1NetId].connectionToClient;

		ongoingSubject = DialogueSubject.DoSwap2;
		TargetShowDialogue(target, ongoingSubject, playerName);
		TargetWaitForResponse(waiter, ongoingSubject);
	}

	[Command]
	public void CmdStartDialogue(NetworkInstanceId playerNetId, DialogueSubject subject) {
		if (subject == DialogueSubject.Disconnection) { // Disconnection has priority over everything else
			if (ongoingSubject != subject) {
				ongoingSubject = subject;
				RpcBackToLobby();
			}
			return;
		}

		if (ongoingSubject != DialogueSubject.None || subject == DialogueSubject.None || gomoku.simulatingMove)
			return;

		string playerName = "";
		NetworkConnection target = null;
		// Only current player can ask AiHelp + only other player can ask UndoMove
		if (subject == DialogueSubject.UndoMove) {
			if (gomoku.backupStates.Count == 0 || playerNetId != gomoku.backupStates[0].currentNetId)
				return;
		}
		else if (subject == DialogueSubject.AiHelp) {
			if (gomoku.isGameEnded || playerNetId != gomoku.currentPlayerNetId)
				return;
		}

		if (playerNetId == gomoku.p1NetId) {
			playerName = "Player" + GomokuPlay.P1_VALUE;
			dialogAnswerId = gomoku.p2NetId;
		}
		else if (playerNetId == gomoku.p2NetId) {
			playerName = "Player" + GomokuPlay.P2_VALUE;
			dialogAnswerId = gomoku.p1NetId;
		}

		ongoingSubject = subject;
		target = NetworkServer.objects[dialogAnswerId].connectionToClient;
		TargetShowDialogue(target, ongoingSubject, playerName);
		TargetWaitForResponse(NetworkServer.objects[playerNetId].connectionToClient, ongoingSubject);
	}

    [Command]
	public void CmdExecuteResponse(NetworkInstanceId playerNetId, bool response) {
		if (ongoingSubject == DialogueSubject.None || playerNetId != dialogAnswerId)
			return;

		if (response) {
			if (ongoingSubject == DialogueSubject.Restart) {
				CmdStart(firstStart: false);
			}
			else if (ongoingSubject == DialogueSubject.UndoMove) {
				gomoku.GoBack();
			}
			else if (ongoingSubject == DialogueSubject.AiHelp) {
				gomoku.SimulateAiMove();
			}
			else if (ongoingSubject == DialogueSubject.DoSwap) {
				gomoku.DoSwap();
			}
			else if (ongoingSubject == DialogueSubject.DoSwap2) {
				gomoku.playedTwoMoreStones = true;
			}
		}
		else if (ongoingSubject == DialogueSubject.DoSwap2) {
			ShowSwapChoice();
			return;
		}
		else if (ongoingSubject == DialogueSubject.DoSwap && gomoku.playedTwoMoreStones) {
			gomoku.currentPlayerNetId = (gomoku.currentPlayerNetId == gomoku.p1NetId) ? gomoku.p2NetId : gomoku.p1NetId;
			RpcChangePlayerHiglight(gomoku.currentPlayerIndex);
		}

		TargetHideDialoguePanel(NetworkServer.objects[gomoku.p1NetId].connectionToClient);
		TargetHideDialoguePanel(NetworkServer.objects[gomoku.p2NetId].connectionToClient);
		dialogAnswerId = NetworkInstanceId.Invalid;
		ongoingSubject = DialogueSubject.None;
	}

#endregion

#region ExitLogic
	[ClientRpc]
	private void RpcBackToLobby() {
		HideAllPanels();
		dialoguePanel.SetActive(true);
		DialoguePanel panelScript = dialoguePanel.GetComponent<DialoguePanel>();
		panelScript.StartWaitForResponse(DialogueSubject.Disconnection);

		StartCoroutine(PrepareExit());
	}

	private IEnumerator PrepareExit() {
		yield return new WaitForSeconds(2.5f);
		Prototype.NetworkLobby.LobbyManager.s_Singleton.BackFromGame();
	}
#endregion
}

