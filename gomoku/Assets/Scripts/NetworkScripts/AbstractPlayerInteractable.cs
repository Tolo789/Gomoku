using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AbstractPlayerInteractable : MonoBehaviour {
	public bool activeAfterInit = true;

	protected PlayerHandler player;

	public void Init(PlayerHandler playerScript) {
		player = playerScript;
		gameObject.SetActive(activeAfterInit);
	}
}
