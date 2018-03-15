using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkLobbyHook : Prototype.NetworkLobby.LobbyHook {
	public override void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
		Prototype.NetworkLobby.LobbyPlayer lobbyScript = lobbyPlayer.GetComponent<Prototype.NetworkLobby.LobbyPlayer>();
		PlayerHandler playerScript = gamePlayer.GetComponent<PlayerHandler>();

		playerScript.playerColor = lobbyScript.playerColor;
		playerScript.playerName = lobbyScript.playerName;
		playerScript.LobbyInfoRetrieved();
	}
}
