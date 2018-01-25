using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.SceneManagement;


public class MainMenu : MonoBehaviour {

    public ToggleGroup is_P1_IA;
    public ToggleGroup is_P2_IA;
    public ToggleGroup firstPlayer;

    public GameObject settingsPanel;

    
    void Start() {
		if (PlayerPrefs.HasKey(CommonDefines.IS_P1_IA)) {
            Toggle[] listOfToggle = is_P1_IA.GetComponentsInChildren<Toggle>(); 
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.IS_P1_IA) == 1) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.IS_P1_IA) == 1) ? false : true;
        }
		if (PlayerPrefs.HasKey(CommonDefines.IS_P2_IA)) {
            Toggle[] listOfToggle = is_P2_IA.GetComponentsInChildren<Toggle>(); 
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.IS_P2_IA) == 1) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.IS_P2_IA) == 1) ? false : true;
        }
        if (PlayerPrefs.HasKey(CommonDefines.FIRST_PLAYER_PLAYING)) {
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
        string isP1IA_versus = is_P1_IA.ActiveToggles().FirstOrDefault().name;
        string isP2IA_versus = is_P2_IA.ActiveToggles().FirstOrDefault().name;
        string firstPlayerString = firstPlayer.ActiveToggles().FirstOrDefault().name;
        if (isP1IA_versus == "ToggleIA") {
            PlayerPrefs.SetInt(CommonDefines.IS_P1_IA, 1);
        }
        else {
            PlayerPrefs.SetInt(CommonDefines.IS_P1_IA, 0);
        }
        if (isP2IA_versus == "ToggleIA") {
            PlayerPrefs.SetInt(CommonDefines.IS_P2_IA, 1);
        }
        else {
            PlayerPrefs.SetInt(CommonDefines.IS_P2_IA, 0);
        }
        int playerStarting = (firstPlayerString == "TogglePlayer1") ? 0 : ((firstPlayerString == "TogglePlayer2") ? 1 : 2);
        PlayerPrefs.SetInt(CommonDefines.FIRST_PLAYER_PLAYING, playerStarting);

        PlayerPrefs.Save();
    }
}
