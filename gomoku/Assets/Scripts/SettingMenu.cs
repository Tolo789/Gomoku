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

	// Use this for initialization
	void Start () {
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
		// 
	}
}
