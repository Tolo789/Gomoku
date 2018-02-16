using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialoguePanel : AbstractDialogue {

	public Text messageText;
	public Button affirmativeButton;
	public Button negativeButton;

	public void ShowOtherPlayerRequest(DialogueSubject subject, string askerName) {
		if (subject == DialogueSubject.None)
			return;

		// Adapt message to subject
		if (subject == DialogueSubject.Restart)
			messageText.text = askerName + " asks to restart the game..";
		else if (subject == DialogueSubject.AiHelp)
			messageText.text = askerName + " asks for AI help..";
		else if (subject == DialogueSubject.UndoMove)
			messageText.text = askerName + " asks to undo his last move..";


		affirmativeButton.interactable = true;
		affirmativeButton.GetComponentInChildren<Text>().text = "Allow";
		negativeButton.interactable = true;
		negativeButton.GetComponentInChildren<Text>().text = "Deny";
	}

	public void ConfirmChoice(bool choice) {
		if (player == null)
			return;
		player.DialogueResponse(choice);
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);

	}

	public void StartWaitForResponse(DialogueSubject subject) {
		// adapt message to subject
		messageText.text = "";
		if (subject == DialogueSubject.Restart)
			messageText.text = "Asking to restart game..";
		else if (subject == DialogueSubject.AiHelp)
			messageText.text = "Asking for AI help..";
		else if (subject == DialogueSubject.UndoMove)
			messageText.text = "Asking to undo your last move..";

		affirmativeButton.interactable = false;
		affirmativeButton.GetComponentInChildren<Text>().text = "";
		negativeButton.interactable = false;
		negativeButton.GetComponentInChildren<Text>().text = "";
	}

}
