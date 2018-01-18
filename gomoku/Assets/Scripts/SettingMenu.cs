using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingMenu : MonoBehaviour {

	public Slider aiDepthSlider;
	public Text aiDephText;

	public Slider aiDifficultySlider;
	public Text aiDifficultyText;

	public Slider nbrOfMovesSlider;
	public Text nbrOfMovesText;
	
	public Slider maxAIRepSlider;
	public Text maxAIRepText;

	public MainMenu mainMenu;

	// Use this for initialization
	void Start () {
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
		PlayerPrefs.SetInt(CommonDefines.AI_DEPTH_SETTING, Mathf.RoundToInt(aiDepthSlider.value));
		PlayerPrefs.SetInt(CommonDefines.AI_MOVES_NB_SETTING, Mathf.RoundToInt(nbrOfMovesSlider.value));
		PlayerPrefs.SetFloat(CommonDefines.AI_TIME_SETTING, maxAIRepSlider.value);
        PlayerPrefs.Save();
		mainMenu.closeSettings();
	}
}
