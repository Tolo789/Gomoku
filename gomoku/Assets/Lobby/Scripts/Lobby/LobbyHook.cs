using UnityEngine;
using UnityEngine.Networking;
using System.Collections;



namespace Prototype.NetworkLobby
{
    // Subclass this and redefine the function you want
    // then add it to the lobby prefab
    public abstract class LobbyHook : MonoBehaviour
    {
        public virtual void OnLobbyServerSceneLoadedForPlayer(NetworkManager manager, GameObject lobbyPlayer, GameObject gamePlayer) {
            LobbyPlayer lobbyScript = lobbyPlayer.GetComponent<LobbyPlayer>();
            PlayerHandler playerScript = gamePlayer.GetComponent<PlayerHandler>();

            playerScript.color = lobbyScript.playerColor;
            Debug.Log("Color: " + lobbyScript.playerColor);
        }
    }

}
