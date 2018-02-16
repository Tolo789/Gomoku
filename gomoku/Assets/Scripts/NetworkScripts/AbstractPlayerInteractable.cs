using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


[RequireComponent(typeof(NetworkIdentity))]
public abstract class AbstractPlayerInteractable : NetworkBehaviour {
	protected PlayerHandler player;

	void Start() {
		Init();
	}

	protected virtual void Init() {
		PlayerHandler[] players = GameObject.FindObjectsOfType(typeof(PlayerHandler)) as PlayerHandler[];
		foreach (PlayerHandler p in players) {
			if (p.isLocalPlayer) {
				player = p;
				break;
			}
		}
	}
}
