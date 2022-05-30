using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class OptionsScreen : MonoBehaviour
{
    public List<ResItem> resolutions = new List<ResItem>();
    private int selectedResolution;

    public TMP_Text resolutionLabel;
    public Toggle fullscreenTog;
    public Slider sensitivitySlider;
    public TMP_Text sensitivityLabel;

    private void Start() {
        fullscreenTog.isOn = Screen.fullScreen;
        sensitivityLabel.text = RoomManager.Instance.sensitivity.ToString("F1");
        selectedResolution = 0;
        UpdateResLabel();
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

    public void UpdateResLabel()
    {
        resolutionLabel.text = resolutions[selectedResolution].horizontal.ToString() + " X " + resolutions[selectedResolution].vertical.ToString();
    }

    public void ApplyGraphics() 
    {
        Screen.fullScreen = fullscreenTog.isOn;

        Screen.SetResolution(resolutions[selectedResolution].horizontal, resolutions[selectedResolution].vertical, fullscreenTog.isOn);
    }

    public void OnChangeSensitivity() 
    {
        sensitivityLabel.text = sensitivitySlider.value.ToString("F1");
        RoomManager.Instance.sensitivity = sensitivitySlider.value;
    }
}

[System.Serializable]
public class ResItem {
    public int horizontal, vertical;
}