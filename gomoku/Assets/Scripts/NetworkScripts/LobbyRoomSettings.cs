using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public struct RoomInfo {
	public string roomName;
	public string aiDifficulty;
	public int aiDepth;
	public int aiMovesNbr;
	public float aiMaxTime;
	public int doubleTree;
	public int selfCapture;
	public int firstPlaying;
	public int openingRule;
}

public class LobbyRoomSettings : MonoBehaviour {
	[Header("Setting inputs")]
	public Text matchNameText;

	public Slider aiDepthSlider;
	public Text aiDephText;

	public Slider nbrOfMovesSlider;
	public Text nbrOfMovesText;
	
	public Slider maxAIRepSlider;
	public Text maxAIRepText;

    public ToggleGroup activateDoubleThree;
    public ToggleGroup activateSelfCapture;

	public ToggleGroup aiDifficulty; 

    public ToggleGroup firstPlayer;

    public ToggleGroup openingRules;


	[Header("Room infos")]
	public Text roomName;
	public Text aiGeneral;
	public Text aiSpeed;
    public Text doubleThree;
    public Text selfCapture;
    public Text firstToPlay;
    public Text usingRules;


	public void PrefillSettings () {
		// Texts
		if (PlayerPrefs.HasKey(CommonDefines.ROOM_NAME)) {
			matchNameText.text = PlayerPrefs.GetString(CommonDefines.ROOM_NAME);
		}
		else
			matchNameText.text = "";

		// Sliders
		if (PlayerPrefs.HasKey(CommonDefines.AI_DEPTH_SETTING)) {
			aiDepthSlider.value = PlayerPrefs.GetInt(CommonDefines.AI_DEPTH_SETTING);
		}
		else {
			aiDepthSlider.value = 3;
		}
		

		if (PlayerPrefs.HasKey(CommonDefines.AI_MOVES_NB_SETTING)) {
			nbrOfMovesSlider.value = PlayerPrefs.GetInt(CommonDefines.AI_MOVES_NB_SETTING);
		}
		else {
			nbrOfMovesSlider.value = 30;
		}

		if (PlayerPrefs.HasKey(CommonDefines.AI_TIME_SETTING)) {
			maxAIRepSlider.value = PlayerPrefs.GetFloat(CommonDefines.AI_TIME_SETTING);
		}
		else {
			maxAIRepSlider.value = 0.5f;
		}
		

		aiDephText.text = aiDepthSlider.value.ToString();
		nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
		maxAIRepText.text = maxAIRepSlider.value.ToString() + "s";

		// Toggles
		if (PlayerPrefs.HasKey(CommonDefines.DOUBLE_THREE_SETTING)) {
            Toggle[] listOfToggle = activateDoubleThree.GetComponentsInChildren<Toggle>();
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.DOUBLE_THREE_SETTING) == 1) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.DOUBLE_THREE_SETTING) == 1) ? false : true;
        }
		if (PlayerPrefs.HasKey(CommonDefines.SELF_CAPTURE_SETTING)) {
            Toggle[] listOfToggle = activateSelfCapture.GetComponentsInChildren<Toggle>();
            listOfToggle[0].isOn = (PlayerPrefs.GetInt(CommonDefines.SELF_CAPTURE_SETTING) == 1) ? true : false;
            listOfToggle[1].isOn = (PlayerPrefs.GetInt(CommonDefines.SELF_CAPTURE_SETTING) == 1) ? false : true;
        }
		if (PlayerPrefs.HasKey(CommonDefines.DIFFICULTY_SETTING)) {
			Toggle[] listOfToggle = aiDifficulty.GetComponentsInChildren<Toggle>();
			foreach (Toggle toggle in listOfToggle) {
				if (toggle.name == PlayerPrefs.GetString(CommonDefines.DIFFICULTY_SETTING)) {
					toggle.isOn = true;
				}
				else {
					toggle.isOn = false;
				}
			}
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

	public void ChangeSliderDephTextValue() {
		aiDephText.text = aiDepthSlider.value.ToString();
	}

	public void changeDifficultySliders(float aiDepth, float nbrOfMoves) {
		aiDepthSlider.interactable = false;
		nbrOfMovesSlider.interactable = false;
		aiDepthSlider.value = aiDepth;
		nbrOfMovesSlider.value = nbrOfMoves;
		aiDephText.text = aiDepthSlider.value.ToString();
		nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
	}
	
	public void ChangeDifficulty(Toggle toggle) {
		if (toggle.isOn) {
			if (toggle.name == "Easy") {
				changeDifficultySliders(1.0f, 30f);
			}
			else if (toggle.name == "Medium") {
				changeDifficultySliders(2.0f, 30f);
			}
			else if (toggle.name == "Hard") {
				changeDifficultySliders(3.0f, 30f);
			}
			else if (toggle.name == "Custom") {
				aiDepthSlider.interactable = true;
				nbrOfMovesSlider.interactable = true;
			}
		}
	}

	public void ChangeNbrOfMovesTextValue() {
		nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
	}
	public void ChangeMaxAIRepTextValue() {
		float nbr = Mathf.RoundToInt(maxAIRepSlider.value * 100) / 100f;
		maxAIRepSlider.value = nbr;
		maxAIRepText.text = maxAIRepSlider.value.ToString() + "s";
	}

	public void DecreaseMaxAIRepValue() {
		float nbr = Mathf.RoundToInt(maxAIRepSlider.value * 10) / 10f;
		maxAIRepSlider.value = nbr - 0.1f;
		maxAIRepText.text = maxAIRepSlider.value.ToString() + "s";
	}

	public void IncreaseMaxAIRepValue() {
		float nbr = Mathf.RoundToInt(maxAIRepSlider.value * 10) / 10f;
		maxAIRepSlider.value = nbr + 0.1f;
		maxAIRepText.text = maxAIRepSlider.value.ToString() + "s";
	}

	public void DecreaseAIDepth() {
		if (aiDepthSlider.interactable) {
			float nbr = Mathf.RoundToInt(aiDepthSlider.value * 10) / 10f;
			aiDepthSlider.value = nbr - 1f;
			aiDephText.text = aiDepthSlider.value.ToString();
		}
	}

	public void IncreaseAIDepth() {
		if (aiDepthSlider.interactable) {
			float nbr = Mathf.RoundToInt(aiDepthSlider.value * 10) / 10f;
			aiDepthSlider.value = nbr + 1f;
			aiDephText.text = aiDepthSlider.value.ToString();
		}
	}

	public void DecreaseNbrOfMove() {
		if (nbrOfMovesSlider.interactable) {
			float nbr = Mathf.RoundToInt(nbrOfMovesSlider.value * 10) / 10f;
			nbrOfMovesSlider.value = nbr - 1f;
			nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
		}
	}

	public void IncreaseNbrOfMove() {
		if (nbrOfMovesSlider.interactable) {
			float nbr = Mathf.RoundToInt(nbrOfMovesSlider.value * 10) / 10f;
			nbrOfMovesSlider.value = nbr + 1f;
			nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
		}
	}

	public void SaveGameSettings() {
		// Texts
		PlayerPrefs.SetString(CommonDefines.ROOM_NAME, matchNameText.text);

		// Sliders
		PlayerPrefs.SetInt(CommonDefines.AI_DEPTH_SETTING, Mathf.RoundToInt(aiDepthSlider.value));
		PlayerPrefs.SetInt(CommonDefines.AI_MOVES_NB_SETTING, Mathf.RoundToInt(nbrOfMovesSlider.value));
		PlayerPrefs.SetFloat(CommonDefines.AI_TIME_SETTING, maxAIRepSlider.value);

		// Toggles
        string toggleChoice = activateDoubleThree.ActiveToggles().FirstOrDefault().name;
		if (toggleChoice == "YesToggle") {
			PlayerPrefs.SetInt(CommonDefines.DOUBLE_THREE_SETTING, 1);
		}
		else {
			PlayerPrefs.SetInt(CommonDefines.DOUBLE_THREE_SETTING, 0);
		}
        toggleChoice = activateSelfCapture.ActiveToggles().FirstOrDefault().name;
		if (toggleChoice == "YesToggle") {
			PlayerPrefs.SetInt(CommonDefines.SELF_CAPTURE_SETTING, 1);
		}
		else {
			PlayerPrefs.SetInt(CommonDefines.SELF_CAPTURE_SETTING, 0);
		}
        toggleChoice = aiDifficulty.ActiveToggles().FirstOrDefault().name;
		PlayerPrefs.SetString(CommonDefines.DIFFICULTY_SETTING, toggleChoice);
        string firstPlayerString = firstPlayer.ActiveToggles().FirstOrDefault().name;
        string handicap = openingRules.ActiveToggles().FirstOrDefault().name;
        int playerStarting = (firstPlayerString == "TogglePlayer1") ? 0 : ((firstPlayerString == "TogglePlayer2") ? 1 : 2);
        PlayerPrefs.SetInt(CommonDefines.FIRST_PLAYER_PLAYING, playerStarting);
        PlayerPrefs.SetInt(CommonDefines.OPENING_RULE, GetOpeningRules(handicap));

		// Save and exit
        PlayerPrefs.Save();
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

    public string GetOpeningRuleName(int handicap) {
        if (handicap == 1)
            return "Standard";
        else if (handicap == 2)
            return "LongPro";
        else if (handicap == 3)
            return "Pro";
        else if (handicap == 4)
            return "Swap";
        else if (handicap == 5)
            return "Swap2";
        return "Unkown";
    }

	public void SetRoomInfo(RoomInfo roomInfo) {
		string str;
		roomName.text = "Game \"" + roomInfo.roomName + "\"";

		aiGeneral.text = "AI difficulty: " + roomInfo.aiDifficulty + " (depth: " + roomInfo.aiDepth + ", nbr. of moves: " + roomInfo.aiMovesNbr + ")";
		aiSpeed.text = "AI max response time: " + roomInfo.aiMaxTime;

		str = (roomInfo.doubleTree == 1) ? "Has" : "No";
    	doubleThree.text = str + " double three";
		str = (roomInfo.selfCapture == 1) ? "Has" : "No";
    	selfCapture.text = str + " self-capture";
		str = (roomInfo.firstPlaying == 0) ? "Host" : (roomInfo.firstPlaying == 1) ? "Client" : "Random";
    	firstToPlay.text = "First player playing: " + str;
    	usingRules.text = "Opening rule: " + GetOpeningRuleName(roomInfo.openingRule);
	}

	public RoomInfo GetRoomInfo() {
		RoomInfo infos = new RoomInfo();
		infos.roomName = PlayerPrefs.GetString(CommonDefines.ROOM_NAME); 
		infos.aiDifficulty = PlayerPrefs.GetString(CommonDefines.DIFFICULTY_SETTING);
		infos.aiDepth = PlayerPrefs.GetInt(CommonDefines.AI_DEPTH_SETTING);
		infos.aiMovesNbr = PlayerPrefs.GetInt(CommonDefines.AI_MOVES_NB_SETTING);
		infos.aiMaxTime = PlayerPrefs.GetFloat(CommonDefines.AI_TIME_SETTING);
		infos.doubleTree = PlayerPrefs.GetInt(CommonDefines.DOUBLE_THREE_SETTING);
		infos.selfCapture = PlayerPrefs.GetInt(CommonDefines.SELF_CAPTURE_SETTING);
		infos.firstPlaying = PlayerPrefs.GetInt(CommonDefines.FIRST_PLAYER_PLAYING);
		infos.openingRule = PlayerPrefs.GetInt(CommonDefines.OPENING_RULE);

		return infos;
	}
}
