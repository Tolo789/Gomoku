using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerHandler : NetworkBehaviour {

	public const string Started = "PlayerController.Start";
	public const string StartedLocal = "PlayerController.StartedLocal";
	public const string Destroyed = "PlayerController.Destroyed";
	public const string RequestMove = "PlayerController.RequestMove";


	private MatchManager gameManager = null;
	public int wins = 0;

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

	void Start () {
		if (isLocalPlayer)
			Debug.Log("Player: " + netId);	
	}

	void Update() {
        if (!isLocalPlayer)
            return;
	}
}
