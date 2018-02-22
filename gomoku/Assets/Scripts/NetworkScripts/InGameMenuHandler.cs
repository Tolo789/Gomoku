using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenuHandler : AbstractPlayerInteractable {

	public void OpenMenuPanel() {
		player.OpenMenuPanel();
	}

	public void CloseMenuPanel() {
		player.CloseMenuPanel();
	}
}
