using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

namespace Prototype.NetworkLobby
{
    //Main menu, mainly only a bunch of callback called by the UI (setup throught the Inspector)
    public class LobbyMainMenu : MonoBehaviour 
    {
        public LobbyManager lobbyManager;

        public RectTransform createGamePanel;
        public RectTransform lobbyServerList;
        public RectTransform lobbyPanel;
        public Button createMatchButton;

        // public InputField ipInput;
        public InputField matchNameInput;

        public LobbyRoomSettings lobbyRoomSettings;

        public void OnEnable()
        {
            lobbyRoomSettings.PrefillSettings();

            lobbyManager.topPanel.ToggleVisibility(true);

            // ipInput.onEndEdit.RemoveAllListeners();
            // ipInput.onEndEdit.AddListener(onEndEditIP);

            matchNameInput.onEndEdit.RemoveAllListeners();
            matchNameInput.onEndEdit.AddListener(onEndEditGameName);

            createMatchButton.interactable = false; // Force match to have a name
        }

        public void OnClickHost()
        {
            lobbyManager.StartHost();
        }

        public void OnClickJoin()
        {
            lobbyManager.ChangeTo(lobbyPanel);

            // lobbyManager.networkAddress = ipInput.text;
            lobbyManager.StartClient();

            lobbyManager.backDelegate = lobbyManager.StopClientClbk;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Connecting...", lobbyManager.networkAddress);
        }

        public void OnClickDedicated()
        {
            lobbyManager.ChangeTo(null);
            lobbyManager.StartServer();

            lobbyManager.backDelegate = lobbyManager.StopServerClbk;

            lobbyManager.SetServerInfo("Dedicated Server", lobbyManager.networkAddress);
        }

        public void OnClickCreateMatchmakingGame()
        {
            lobbyRoomSettings.SaveGameSettings();

            lobbyManager.StartMatchMaker();
            lobbyManager.matchMaker.CreateMatch(
                matchNameInput.text,
                (uint)lobbyManager.maxPlayers,
                true,
				"", "", "", 0, 0,
				lobbyManager.OnMatchCreate);

            lobbyManager.backDelegate = lobbyManager.StopHost;
            lobbyManager._isMatchmaking = true;
            lobbyManager.DisplayIsConnecting();

            lobbyManager.SetServerInfo("Matchmaker Host", lobbyManager.matchHost);
        }

        public void OnClickOpenServerList()
        {
            lobbyManager.StartMatchMaker();
            // lobbyManager.backDelegate = lobbyManager.SimpleBackClbk;
            lobbyManager.backDelegate = lobbyManager.BackToMainMenu;
            lobbyManager.ChangeTo(lobbyServerList);
        }

        public void OnClickStartCreateMatch()
        {
            lobbyManager.backDelegate = lobbyManager.BackToMainMenu;
            lobbyManager.ChangeTo(createGamePanel);
        }

        void onEndEditIP(string text)
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                OnClickJoin();
            }
        }

        void onEndEditGameName(string text)
        {
            if (text == "")
                createMatchButton.interactable = false;
            else {
                createMatchButton.interactable = true;
                if (Input.GetKeyDown(KeyCode.Return))
                {
                    OnClickCreateMatchmakingGame();
                }
            }
        }

    }
}
