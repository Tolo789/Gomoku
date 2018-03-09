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
		if (subject == DialogueSubject.DoSwap) {
			messageText.text = "Do you want to swap stones ?";

			affirmativeButton.GetComponentInChildren<Text>().text = "Yes";
			negativeButton.GetComponentInChildren<Text>().text = "No";
		}
		else if (subject == DialogueSubject.DoSwap2) {
			messageText.text = "Choose an option:";

			affirmativeButton.GetComponentInChildren<Text>().text = "Put two more stones";
			negativeButton.GetComponentInChildren<Text>().text = "Chose if swap";
		}
		else {
			if (subject == DialogueSubject.Restart)
				messageText.text = askerName + " asks to restart the game..";
			else if (subject == DialogueSubject.AiHelp)
				messageText.text = askerName + " asks for AI help..";
			else if (subject == DialogueSubject.UndoMove)
				messageText.text = askerName + " asks to undo his last move..";

			affirmativeButton.GetComponentInChildren<Text>().text = "Allow";
			negativeButton.GetComponentInChildren<Text>().text = "Deny";
		}


		affirmativeButton.interactable = true;
		negativeButton.interactable = true;
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
		else if (subject == DialogueSubject.DoSwap)
			messageText.text = "Waiting Swap choice..";
		else if (subject == DialogueSubject.DoSwap2)
			messageText.text = "Waiting Swap2 choice..";
		else if (subject == DialogueSubject.Disconnection)
			messageText.text = "Disconnecting..";

		affirmativeButton.interactable = false;
		affirmativeButton.GetComponentInChildren<Text>().text = "";
		negativeButton.interactable = false;
		negativeButton.GetComponentInChildren<Text>().text = "";
	}

}
