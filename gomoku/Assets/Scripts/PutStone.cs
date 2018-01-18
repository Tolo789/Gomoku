using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutStone : MonoBehaviour {
	[HideInInspector]
	public GameManager gameManager;

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
