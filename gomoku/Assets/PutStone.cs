using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutStone : MonoBehaviour {
	[HideInInspector]
	public GameManager gameManager;

	public void TryToPutStone() {
		if (gameManager == null)
			return ;

		gameManager.TryPutStone(gameObject);
	}
}
