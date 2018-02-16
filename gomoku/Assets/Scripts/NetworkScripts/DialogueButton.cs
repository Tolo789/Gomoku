using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueButton : AbstractDialogue {
	public DialogueSubject subject = DialogueSubject.None;

	public void StartDialogue() {
		if (player != null)
			player.StartDialogue(subject);
	}
}
