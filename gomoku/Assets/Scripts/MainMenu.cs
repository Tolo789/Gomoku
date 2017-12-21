using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MainMenu : MonoBehaviour {
    public void TaskOnClick() {
        Debug.Log("titi");
        Application.LoadLevel("Main");
    }
}
