using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DialogueSubject {
	None = 0,
	Restart = 1,
	AiHelp = 2,
	UndoMove = 3,
	DoSwap = 4,
	DoSwap2 = 5,
	Disconnection = 6
}

public abstract class AbstractDialogue : AbstractPlayerInteractable {
}
