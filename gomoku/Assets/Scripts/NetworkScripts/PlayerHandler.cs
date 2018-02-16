using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerHandler : NetworkBehaviour {

	[HideInInspector] public int wins = 0;



	private GameObject menuPanel;
	private MatchManager gameManager = null; // Only used by Server

	// Register to server
	public override void OnStartLocalPlayer()
    {
		base.OnStartLocalPlayer();

		// Retrieve Client-handled UI
		menuPanel = GameObject.Find("/Canvas/MenuPanel");
		menuPanel.SetActive(false);

		// Tell server that Client player is ready
		CmdRegisterSelf(netId);
    }

	[Command]
	private void CmdRegisterSelf(NetworkInstanceId playerNetId) {
		if (gameManager == null)
			gameManager = GameObject.Find("MatchManager").GetComponent<MatchManager>();
		gameManager.CmdRegisterPlayer(playerNetId);
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
		if (menuPanel.activeSelf && subject != DialogueSubject.Restart) // If players opened the Menu, then he can only ask for a rematch
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
