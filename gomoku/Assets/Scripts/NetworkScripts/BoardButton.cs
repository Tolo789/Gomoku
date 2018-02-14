using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class BoardButton : NetworkBehaviour {

	[HideInInspector]
	public PlayerHandler player;

	[HideInInspector]
	public bool isEmpty = true;

	public void TryToPutStone() {
		if (player == null || !isEmpty)
			return ;
		string[] coords = gameObject.name.Split('-');
		int yCoord = int.Parse(coords[0]);
		int xCoord = int.Parse(coords[1]);

		player.TryPutStone(yCoord, xCoord);
	}

}
