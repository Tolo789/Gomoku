using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {
    public ToggleGroup versus;
    public ToggleGroup firstPlayer;

    public GameObject settingsPanel;

    
    void Start() {
		if (PlayerPrefs.HasKey(CommonDefines.VERSUS_IA)) {
            Toggle[] listOfToggle = versus.GetComponentsInChildren<Toggle>(); 
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.VERSUS_IA) == 1) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.VERSUS_IA) == 1) ? false : true;
        }
        if (PlayerPrefs.HasKey(CommonDefines.VERSUS_IA)) {
            Toggle[] listOfToggle = firstPlayer.GetComponentsInChildren<Toggle>(); 
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.FIRST_PLAYER_PLAYING) == 0) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.FIRST_PLAYER_PLAYING) == 1) ? true : false;
            listOfToggle[2].isOn = (PlayerPrefs.GetInt(CommonDefines.FIRST_PLAYER_PLAYING) == 2) ? true : false;
        }
    }
    public void PlayClick(GameObject playSettings) {
        playSettings.SetActive(true);
    }
    public void ExitClick() {
        Application.Quit();
    }
    public void GoClick(GameObject playSettings) {
        GetSettingsGameInfo();
        playSettings.SetActive(false);
        SceneManager.LoadScene("Game");
    }
     public void GoBack(GameObject playSettings) {
       playSettings.SetActive(false);
    }


    //A CHANGER
     public void closeSettings() {
       settingsPanel.SetActive(false);
    }

    public void GetSettingsGameInfo() {
        string versusString = versus.ActiveToggles().FirstOrDefault().name;
        string firstPlayerString = firstPlayer.ActiveToggles().FirstOrDefault().name;
        if (versusString == "ToggleIA") {
            PlayerPrefs.SetInt(CommonDefines.VERSUS_IA, 1);
        }
        else {
            PlayerPrefs.SetInt(CommonDefines.VERSUS_IA, 0);
        }
        int playerStarting = (firstPlayerString == "TogglePlayer1") ? 0 : ((firstPlayerString == "TogglePlayer2") ? 1 : 2);
        PlayerPrefs.SetInt(CommonDefines.FIRST_PLAYER_PLAYING, playerStarting);
        PlayerPrefs.Save();
    }
}
