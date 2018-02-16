using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DialogueButton : NetworkBehaviour {
	public DialogueSubject subject = DialogueSubject.None;

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

	public void StartDialogue() {
		if (player != null)
			player.StartDialogue(subject);
	}
}
