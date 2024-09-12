using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SliderManager : MonoBehaviour
{
    //private GameObject beamSynth;
    private BeamManager beamManager;

    // Text component references
    private TextMeshProUGUI valueText;
    private Slider slider;

    string csoundChannel;

    void Start()
    {
        // Find the child object with the name "Label"
        Transform labelTransform = transform.Find("Label");
        slider = GetComponent<Slider>();

        // Check if the "Label" child object is found
        if (labelTransform != null)
        {
            // Get the Text component from the "Label" child object
            // Check if the Text component is found
            if (labelTransform.TryGetComponent<TextMeshProUGUI>(out var labelText))
                // Set the text to the name of the parent object
                labelText.text = gameObject.name;
            else
                Debug.LogWarning("No Text component found on the 'Label' child object.");
        }
        else
            Debug.LogWarning("No child object named 'Label' found.");

        valueText = transform.Find("Value").GetComponent<TextMeshProUGUI>();


        // Get reference to Csound
        GameObject beamSynth = GameObject.FindWithTag("Csound");
        beamManager = beamSynth.GetComponent<BeamManager>();

        // Determine what csound channel to output to according to GameObject name and dictionary
        csoundChannel = beamManager.csoundChannelDict[gameObject.name];

        //Adds a listener to the main slider and invokes a method when the value changes.
        slider.onValueChanged.AddListener(delegate { ValueChanged(); });

        // Initialize values
        ValueChanged();
    }
    

    private void ValueChanged()
    {
        valueText.text = slider.value.ToString();
        beamManager.csound.SetChannel(csoundChannel, slider.value);
    }
}