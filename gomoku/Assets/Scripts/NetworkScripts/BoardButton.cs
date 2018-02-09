using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BoardButton : NetworkBehaviour {

	[HideInInspector]
	public MatchManager gameManager;

	[HideInInspector]
	public bool isEmpty = true;

	public void TryToPutStone() {
		if (gameManager == null || !isEmpty || !gameManager.PlayerCanPutStone())
			return ;
		string[] coords = gameObject.name.Split('-');
		int yCoord = int.Parse(coords[0]);
		int xCoord = int.Parse(coords[1]);

		// gameManager.PutStone(yCoord, xCoord);
		gameManager.SavePlayerMove(yCoord, xCoord);
	}

}
