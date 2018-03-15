using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerHandler : NetworkBehaviour {

	[SyncVar]
	public Color playerColor = Color.black;
	[SyncVar]
	public string playerName = "";
	[SyncVar]
	private bool lobbyInfoRetrieved = false; // tells if has infos from lobby has been retrieved
	[SyncVar]
	private bool hasRegistered = false; // tells if has been registered to server

	[HideInInspector] public GameObject menuPanel;

	private MatchManager gameManager = null; // Only used by Server

	// Register to server
	public override void OnStartLocalPlayer()
    {
		base.OnStartLocalPlayer();

		// Tell server that Client player is ready
		StartCoroutine(RegisterToMatchManager());
    }

	private IEnumerator RegisterToMatchManager() {
		yield return new WaitUntil(() => lobbyInfoRetrieved);
		while (!hasRegistered) {
			CmdRegisterSelf(netId, playerName, playerColor, isServer);
			yield return new WaitForSeconds(0.2f);
		}

	}

	[Command]
	private void CmdRegisterSelf(NetworkInstanceId playerNetId, string pName, Color pColor, bool serverPlayer) {
		if (gameManager == null) {
			GameObject go = GameObject.Find("MatchManager");
			if (go == null)
				return;
			gameManager = go.GetComponent<MatchManager>();
		}
		gameManager.CmdRegisterPlayer(playerNetId, pName, pColor, serverPlayer);
	}

	public void LobbyInfoRetrieved() {
		lobbyInfoRetrieved = true;
	}

	public void HasBeenRegistered() {
		hasRegistered = true;
	}

	// Try put stone
	public void TryPutStone(int y, int x) {
		if (menuPanel.activeSelf)
			return;
		CmdTryPutStone(netId, y, x);
	}

	[Command]
	private void CmdTryPutStone(NetworkInstanceId playerNetId, int y, int x) {
		if (gameManager == null)
			return ;
		gameManager.CmdTrySavePlayerMove(playerNetId, y, x);
	}

#region Dialogues logic
	public void StartDialogue(DialogueSubject subject) {
		if (menuPanel.activeSelf && !(subject == DialogueSubject.Restart || subject == DialogueSubject.Disconnection)) // If players opened the Menu, then he can only ask for a rematch or quit game
			return;
		CmdStartDialogue(netId, subject);
	}

	[Command]
	private void CmdStartDialogue(NetworkInstanceId playerNetId, DialogueSubject subject) {
		if (gameManager == null)
			return ;
		gameManager.CmdStartDialogue(playerNetId, subject);
	}

	public void DialogueResponse(bool choice) {
		CmdDialogueResponse(netId, choice);
	}

	[Command]
	private void CmdDialogueResponse(NetworkInstanceId playerNetId, bool choice) {
		if (gameManager == null)
			return ;
		gameManager.CmdExecuteResponse(playerNetId, choice);
	}

#endregion

#region Client-only UI
	public void OpenMenuPanel() {
        menuPanel.SetActive(true);
    }

	public void CloseMenuPanel() {
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
		menuPanel.SetActive(false);
    }
#endregion
}
