using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class PlayerHandler : NetworkBehaviour {

	public int wins = 0;


	private MatchManager gameManager = null;

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


	public void TryPutStone(int y, int x) {
		CmdTryPutStone(netId, y, x);
	}


	[Command]
	public void CmdTryPutStone(NetworkInstanceId playerNetId, int y, int x) {
		if (gameManager == null)
			return ;
		gameManager.CmdTrySavePlayerMove(playerNetId, y, x);
	}
}
