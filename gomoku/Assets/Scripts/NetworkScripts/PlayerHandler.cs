using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerHandler : NetworkBehaviour {

	public int wins = 0;


	private MatchManager gameManager = null;

	// Register to server
	public override void OnStartLocalPlayer()
    {
		base.OnStartLocalPlayer();
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
		CmdTryPutStone(netId, y, x);
	}

	[Command]
	private void CmdTryPutStone(NetworkInstanceId playerNetId, int y, int x) {
		if (gameManager == null)
			return ;
		gameManager.CmdTrySavePlayerMove(playerNetId, y, x);
	}


	// Start Dialogue
	public void StartDialogue(DialogueSubject subject) {
		CmdStartDialogue(netId, subject);
	}

	[Command]
	private void CmdStartDialogue(NetworkInstanceId playerNetId, DialogueSubject subject) {
		if (gameManager == null)
			return ;
		gameManager.CmdStartDialogue(playerNetId, subject);
	}


	// Response to dialogue
	public void ConfirmDialogueChoice(bool choice) {
		CmdConfirmDialogueChoice(netId, choice);
	}

	[Command]
	private void CmdConfirmDialogueChoice(NetworkInstanceId playerNetId, bool choice) {
		if (gameManager == null)
			return ;
		gameManager.CmdExecuteResponse(playerNetId, choice);
	}
}
