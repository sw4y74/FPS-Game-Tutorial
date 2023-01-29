using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour
{
    public List<ResItem> resolutions = new List<ResItem>();
    public List<DisplayModeItem> displayModes = new List<DisplayModeItem>();
    private int selectedResolution;
    private int selectedDisplayMode;
    public TMP_Text resolutionLabel;
    public TMP_Text displayModeLabel;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityLabel;

    private void Start() {
        sensitivityLabel.text = RoomManager.Instance.sensitivity.ToString("F1");
        selectedResolution = PlayerPrefs.HasKey("resolution") ? PlayerPrefs.GetInt("resolution") : 1;
        selectedDisplayMode = PlayerPrefs.HasKey("displaymode") ? PlayerPrefs.GetInt("displaymode") : 0;
        Debug.Log(selectedResolution);
        Debug.Log(selectedDisplayMode);
        UpdateResLabel();
        UpdateDisplayModeLabel();
        if (PlayerPrefs.HasKey("sensitivity"))
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("sensitivity");
            OnChangeSensitivity();
        }
        ApplyGraphics();
    }

    public void ResLeft() 
    {
        selectedResolution--;
        if(selectedResolution < 0) {
            selectedResolution = 0;
        }
        UpdateResLabel();
    }

    public void ResRight() 
    {
        selectedResolution++;
        if (selectedResolution > resolutions.Count - 1) {
            selectedResolution = resolutions.Count - 1;
        }
        UpdateResLabel();
    }

    public void DisplayModeLeft() 
    {
        selectedDisplayMode--;
        if(selectedDisplayMode < 0) {
            selectedDisplayMode = 0;
        }
        UpdateDisplayModeLabel();
    }

    public void DisplayModeRight() 
    {
        selectedDisplayMode++;
        if (selectedDisplayMode > displayModes.Count - 1) {
            selectedDisplayMode = displayModes.Count - 1;
        }
        UpdateDisplayModeLabel();
    }

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + " X " + resolutions[selectedResolution].vertical.ToString();
    }

    public void UpdateDisplayModeLabel()
    {
        displayModeLabel.text = displayModes[selectedDisplayMode].displayModeName;
    }

    public void ApplyGraphics() 
    {
        Screen.fullScreenMode = displayModes[selectedDisplayMode].displayMode;
        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, displayModes[selectedDisplayMode].displayMode);
        PlayerPrefs.SetInt("displaymode", selectedDisplayMode);
        PlayerPrefs.SetInt("resolution", selectedResolution);
        PlayerPrefs.Save();
    }

    public void OnChangeSensitivity() 
    {
        sensitivityLabel.text = sensitivitySlider.value.ToString("F1");
        RoomManager.Instance.sensitivity = sensitivitySlider.value;
        if (GameObject.FindGameObjectWithTag("LocalPlayer"))
        {
            PlayerController pc = GameObject.FindGameObjectWithTag("LocalPlayer").GetComponent<PlayerController>();
            pc.ChangeSensitivity(sensitivitySlider.value);
        }
        PlayerPrefs.SetFloat("sensitivity", sensitivitySlider.value);
    }
}

[System.Serializable]
public class ResItem {
    public int horizontal, vertical;
}
[System.Serializable]
public class DisplayModeItem {
    public FullScreenMode displayMode;
    public string displayModeName;
}