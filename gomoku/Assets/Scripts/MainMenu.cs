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
    public ToggleGroup openingRules;

    public GameObject settingsPanel;
    public GameObject errorMessage;

    void Start() {
        Screen.SetResolution(1024, 768, false);
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
        if (PlayerPrefs.HasKey(CommonDefines.OPENING_RULE)) {
            Toggle[] listOfToggle = openingRules.GetComponentsInChildren<Toggle>();
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.OPENING_RULE) == 1) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.OPENING_RULE) == 2) ? true : false;
            listOfToggle[2].isOn = (PlayerPrefs.GetInt(CommonDefines.OPENING_RULE) == 3) ? true : false;
            listOfToggle[3].isOn = (PlayerPrefs.GetInt(CommonDefines.OPENING_RULE) == 4) ? true : false;
            listOfToggle[4].isOn = (PlayerPrefs.GetInt(CommonDefines.OPENING_RULE) == 5) ? true : false;
        }
    }
    public void PlayClick(GameObject playSettings) {
        playSettings.SetActive(true);
    }
    public void PlayOnlineClick(GameObject playSettings) {
        SceneManager.LoadScene("NetworkLobby");
    }
    public void ExitClick() {
        Application.Quit();
    }
    public void GoClick(GameObject playSettings) {
        string isP1IA_versus = is_P1_IA.ActiveToggles().FirstOrDefault().name;
        string isP2IA_versus = is_P2_IA.ActiveToggles().FirstOrDefault().name;
        string checkOpeningRules = openingRules.ActiveToggles().FirstOrDefault().name;

        if ((isP1IA_versus == "ToggleIA" || isP2IA_versus == "ToggleIA") && (checkOpeningRules == "Swap" || checkOpeningRules == "Swap2")) {
            errorMessage.SetActive(true);
            return ;
        }

        GetSettingsGameInfo();
        SceneManager.LoadScene("Game");
    }
     public void GoBack(GameObject playSettings) {
        playSettings.SetActive(false);
    }


    //A CHANGER
     public void closeSettings() {
       settingsPanel.SetActive(false);
    }

    public int GetOpeningRules(string handicap) {
        if (handicap == "Standard")
            return 1;
        else if (handicap == "LongPro")
            return 2;
        else if (handicap == "Pro")
            return 3;
        else if (handicap == "Swap")
            return 4;
        else if (handicap == "Swap2")
            return 5;
        return -1;
    }

    public void GetSettingsGameInfo() {
        string isP1IA_versus = is_P1_IA.ActiveToggles().FirstOrDefault().name;
        string isP2IA_versus = is_P2_IA.ActiveToggles().FirstOrDefault().name;
        string firstPlayerString = firstPlayer.ActiveToggles().FirstOrDefault().name;
        string handicap = openingRules.ActiveToggles().FirstOrDefault().name;
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
        PlayerPrefs.SetInt(CommonDefines.OPENING_RULE, GetOpeningRules(handicap));
        PlayerPrefs.Save();
    }
}
