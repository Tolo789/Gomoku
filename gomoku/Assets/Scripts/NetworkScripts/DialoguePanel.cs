using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public enum DialogueSubject {
	None = 0,
	Restart = 1,
	AiHelp = 2,
	UndoMove = 3
}

public class DialoguePanel : NetworkBehaviour {

	public Text messageText;
	public Button affirmativeButton;
	public Button negativeButton;

	private PlayerHandler player;
	
	void Start() {
		PlayerHandler[] players = GameObject.FindObjectsOfType(typeof(PlayerHandler)) as PlayerHandler[];
		foreach (PlayerHandler p in players) {
			if (p.isLocalPlayer) {
				player = p;
				break;
			}
		}
	}

	public void ShowOtherPlayerRequest(DialogueSubject subject, string askerName) {
		if (subject == DialogueSubject.None)
			return;

		// Adapt message to subject
		if (subject == DialogueSubject.Restart)
			messageText.text = askerName + " would like to restart the game..";
		else if (subject == DialogueSubject.AiHelp)
			messageText.text = askerName + " would like to ask help to AI..";
		else if (subject == DialogueSubject.UndoMove)
			messageText.text = askerName + " would like to undo his move..";


		affirmativeButton.interactable = true;
		affirmativeButton.GetComponentInChildren<Text>().text = "Allow";
		negativeButton.interactable = true;
		negativeButton.GetComponentInChildren<Text>().text = "Deny";
	}

	public void ConfirmChoice(bool choice) {
		if (player == null)
			return;
		player.ConfirmDialogueChoice(choice);
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

	}

	public void StartWaitForResponse(DialogueSubject subject) {

		// adapt message to subject
		if (subject == DialogueSubject.Restart)
			messageText.text = "Asking to restart game..";
		else if (subject == DialogueSubject.AiHelp)
			messageText.text = "Asking to use AI help..";
		else if (subject == DialogueSubject.UndoMove)
			messageText.text = "Asking to undo last move..";
		affirmativeButton.interactable = false;
		affirmativeButton.GetComponentInChildren<Text>().text = "";
		negativeButton.interactable = false;
		negativeButton.GetComponentInChildren<Text>().text = "";
	}

}
