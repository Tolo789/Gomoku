using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class SettingMenu : MonoBehaviour {

	public Slider aiDepthSlider;
	public Text aiDephText;

	public Slider aiDifficultySlider;
	public Text aiDifficultyText;

	public Slider nbrOfMovesSlider;
	public Text nbrOfMovesText;
	
	public Slider maxAIRepSlider;
	public Text maxAIRepText;

    public ToggleGroup activateDoubleThree;
    public ToggleGroup activateSelfCapture;

	public MainMenu mainMenu;

	// Use this for initialization
	void Start () {
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
			nbrOfMovesSlider.value = 20;
		}

		if (PlayerPrefs.HasKey(CommonDefines.AI_TIME_SETTING)) {
			maxAIRepSlider.value = PlayerPrefs.GetFloat(CommonDefines.AI_TIME_SETTING);
		}
		else {
			maxAIRepSlider.value = 0.5f;
		}

		aiDephText.text = aiDepthSlider.value.ToString();
		aiDifficultyText.text = aiDifficultySlider.value.ToString();
		nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
		maxAIRepText.text = maxAIRepSlider.value.ToString();

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
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeSliderDephTextValue() {
		aiDephText.text = aiDepthSlider.value.ToString();
	}

	public void ChangeSliderDifficultyTextValue() {
		aiDifficultyText.text = aiDifficultySlider.value.ToString();
	}

	public void ChangeNbrOfMovesTextValue() {
		nbrOfMovesText.text = nbrOfMovesSlider.value.ToString();
	}
	public void ChangeMaxAIRepTextValue() {
		maxAIRepText.text = maxAIRepSlider.value.ToString();
	}

	public void saveChanges() {
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

		// Save and exit
        PlayerPrefs.Save();
		mainMenu.closeSettings();
	}
}
