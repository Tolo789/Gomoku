using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PutStone : MonoBehaviour {
	[HideInInspector]
	public GameManager gameManager;

	[HideInInspector]
	public bool isEmpty = true;

	public void TryToPutStone() {
		if (gameManager == null || !isEmpty)
			return ;

		gameManager.TryPutStone(gameObject);
	}
}
