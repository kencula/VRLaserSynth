using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class DropdownsManager : MonoBehaviour
{

    // BeamManager reference
    [SerializeField] BeamManager beamManager;

    // UI References
    [SerializeField] TMP_Dropdown[] dropdowns;
    [SerializeField] TMP_Text[] labels;

    private readonly List<string> dropdownOptions = new()
    {"Vibrato", 
    "Pitchbend Up",
    "Pitchbend Down",
    "Filter Cutoff",
    "Reverb Amount",
    "Delay Amount",
    "Delay Time",
    "Delay Feedback",
    "Distortion Amount",
    "Portamento Time"};

    // Start is called before the first frame update
    void Start()
    {
        // Index init
        int i = 0;
        foreach (TMP_Dropdown dropdown in dropdowns)
        {
            // Add parameter options to Unity
            dropdown.ClearOptions();
            dropdown.AddOptions(dropdownOptions);

            // Set dropdowns to a initial preset
            switch (i)
            {
                case 0:
                    dropdown.value = 0;
                    break;
                case 1:
                    dropdown.value = 8; break;
                case 2:
                    dropdown.value = 4; break;
                case 3:
                    dropdown.value = 6; break;
            }

            // Subscribe to the onValueChanged event
            dropdown.onValueChanged.AddListener(delegate { OnDropdownValueChanged(dropdown); });

            // Initialize dropdowns
            OnDropdownValueChanged(dropdown);

            // Increment index
            i++;
        }
    }


    void OnDropdownValueChanged(TMP_Dropdown dropdown)
    {
        // Determine which axis dropdown is changed
        var x = dropdown.gameObject.tag switch
        {
            "posX" => 0,
            "posY" => 1,
            "negX" => 2,
            "negY" => 3,
            _ => 0,
        };

        // Send parameter selected to array in beamManager script in correct index for the axis selected
        beamManager.channelIndexes[x] = dropdown.value;

        // Set main UI xy coord labels to matching parameter
        labels[x].text = dropdownOptions[dropdown.value];
    }
}
