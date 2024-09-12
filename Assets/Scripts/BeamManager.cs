using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using TMPro;
using UnityEngine.UI;
using Unity.VisualScripting;
using System.Linq;
using UnityEngine.SceneManagement;

public class BeamManager : MonoBehaviour
{
    public CsoundUnity csound;
    public Autohand.Hand hand;

    //bool noteOn = false;

    // Reference to flashlights
    [SerializeField] GameObject objR;
    [SerializeField] GameObject objL;
    [SerializeField] GameObject crystal;

    // Options
    [SerializeField] float maxPitchDistance = 4.0f;
    [SerializeField] float maxModDistance = 2f;

    // Midi Note Display
    [SerializeField] TextMeshProUGUI text;

    // Tuner
    [SerializeField] int midiNote;
    [SerializeField] RectTransform tuner;
    [SerializeField] Image tunerImage;
    [SerializeField] float freq;

    // Mod Wheel
    [SerializeField] RectTransform modWheel;
    [SerializeField] Image modWheelImage;

    // Rotational Mod wheel
    [SerializeField] RectTransform dot;

    // Csound Channel Manager
    readonly List<string> csoundChannels = new()
    {
        "vibrato",
        "pbUp",
        "pbDown",
        "filter",
        "rvbAmt",
        "delAmt",
        "delTime",
        "delFeedback",
        "distortion",
        "portTime"
    };

    // Csound Channel Dictionary
    public readonly Dictionary<string, string> csoundChannelDict = new()
    {
        {"Vibrato","vibrato" },
        {"Pitchbend Up", "pbUp" },
        {"Pitchbend Down", "pbDown" },
        {"Filter Cutoff", "filter" },
        {"Reverb Amount", "rvbAmt" },
        {"Delay Amount", "delAmt" },
        {"Delay Time", "delTime" },
        {"Delay Feedback", "delFeedback" },
        {"Distortion Amount", "distortion" },
        {"Portamento Time", "portTime" }
    };

    // Csound channels for parameters the xy pad will change
    public int[] channelIndexes = new int[5];

    [SerializeField] TMP_Text tensionLabel;

    // Orb indicators
    [SerializeField] OrbIndicator orbIndicator;

    

    // Start is called before the first frame update
    void Start()
    {
        csound = GetComponent<CsoundUnity>();

        //find reference to hand
        GameObject[] hands = GameObject.FindGameObjectsWithTag("Right Hand");
        hand = hands[0].GetComponent<Autohand.Hand>();

        // Disable cursor for showcase reasons
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        hand.OnSqueezed += OnSqueezed;
        hand.OnUnsqueezed += OnUnsqueezed;
    }

    private void OnDisable()
    {
        hand.OnSqueezed -= OnSqueezed;
        hand.OnUnsqueezed -= OnUnsqueezed;
    }

    // Update is called once per frame
    void Update()
    {
        

        if (!csound.IsInitialized) return;

        //------------------------------------------------------------------------------------
        // TENSION MOD

        // Get distance between orbs
        float distance = Vector3.Distance(objR.transform.position, objL.transform.position);
        float convertedDistance = Mathf.Clamp(ConvertRange(0f, maxModDistance, 0f, 1f, distance), 0, 1);

        // Disable tension mod if xy pad is modifying filter cutoff
        if (!channelIndexes.Contains(3))
        {
            csound.SetChannel("filter", convertedDistance);
            tensionLabel.text = "Filter Cutoff";
        }
        else
        {
            tensionLabel.text = "Disabled! XY pad is controlling filter cutoff";
        }

        // Set modWheel Visual
        modWheel.anchoredPosition = new Vector2(modWheel.anchoredPosition.x, ConvertRange(0f, 1f, -100f, 0, convertedDistance));
        modWheel.sizeDelta = new Vector2(modWheel.sizeDelta.x, convertedDistance * 200);
        modWheelImage.color = Color.Lerp(Color.green, Color.red, convertedDistance);

        //------------------------------------------------------------------------------------
        // PITCH MOD

        // get distance between right orb and crystal
        float distance2 = Vector3.Distance(objR.transform.position, crystal.transform.position);

        // convert distance into midi note (4 octaves)
        float midiNoteF = (48 * ConvertRange(0f, maxPitchDistance, 0f, 1f, distance2)) + 36;
        midiNote = Mathf.RoundToInt(midiNoteF);
        float offTuneAmt = midiNoteF - midiNote;
        text.text = MidiToNoteString(midiNote);

        //------------------------------------------------------------------------------------
        // TUNER

        // setting visual tuner
        float yPos = ConvertRange(-0.5f, 0.5f, -50f, 0f, offTuneAmt);
        tuner.anchoredPosition = new Vector2(tuner.anchoredPosition.x, yPos);
        tuner.sizeDelta = new Vector2(tuner.sizeDelta.x, (offTuneAmt + 0.5f) * 100);

        // Tuner Color
        tunerImage.color = Mathf.Abs(offTuneAmt) < 0.2 ? Color.green : Color.red;

        // convert midi note to frequency and send to csound
        freq = MidiToFrequency(midiNote);
        csound.SetChannel("freq", freq);

        //------------------------------------------------------------------------------------
        // XY PAD
        // Rotation based CC
        // Atan2 taken from: https://starmanta.gitbooks.io/unitytipsredux/content/second-question.html
        Vector3 up = objL.transform.up;
        Vector3 right = objL.transform.right;
        float pitchAngle = Mathf.Abs(Mathf.Atan2(up.y, -up.x)) * Mathf.Rad2Deg; // Outputs car turn right to left rotation, from 0 (straight left) to 180 (straight right)
        float rollAngle = Mathf.Abs(Mathf.Atan2(up.y, up.z)) * Mathf.Rad2Deg; // Outputs barrel roll right to left rotation, from 0 (laying on the left side) to 180 (laying on the right side)

        // Convert range to -1 - 1
        pitchAngle = ConvertRange(0, 180, -1, 1, pitchAngle);
        rollAngle = ConvertRange(0, 180, -1, 1, rollAngle);

        // Set visual xy pad
        dot.anchoredPosition = new Vector2(dot.anchoredPosition.x, pitchAngle * 150);
        dot.anchoredPosition = new Vector2(rollAngle * 150, dot.anchoredPosition.y);

        // Send to csound
        if (pitchAngle <= 0)
        {
            // NegY
            csound.SetChannel(csoundChannels[channelIndexes[3]], Mathf.Abs(pitchAngle));
            csound.SetChannel(csoundChannels[channelIndexes[1]], 0);
        }
        else
        {
            // PosY
            csound.SetChannel(csoundChannels[channelIndexes[1]], Mathf.Abs(pitchAngle));
            csound.SetChannel(csoundChannels[channelIndexes[3]], 0);
        }
        if (rollAngle >= 0)
        {
            // PosX
            csound.SetChannel(csoundChannels[channelIndexes[0]], rollAngle);
            csound.SetChannel(csoundChannels[channelIndexes[2]], 0);
        }
        else {
            // NegX
            csound.SetChannel(csoundChannels[channelIndexes[2]], Mathf.Abs(rollAngle));
            csound.SetChannel(csoundChannels[channelIndexes[0]], 0);
        }
    }

    // Method to convert MIDI number to note string
    public string MidiToNoteString(int midiNumber)
    {
        string[] noteNames = { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

        int noteIndex = midiNumber % 12;
        int octave = midiNumber / 12 - 1;

        string noteName = noteNames[noteIndex];
        return noteName + octave.ToString();
    }

    float ConvertRange(
    float originalStart, float originalEnd, // original range
    float newStart, float newEnd, // desired range
    float value) // value to convert
    {
        float scale = (newEnd - newStart) / (originalEnd - originalStart);
        return (float)(newStart + ((value - originalStart) * scale));
    }

    float MidiToFrequency(int midiNote)
    {
        return Mathf.Pow(2f, (midiNote - 69) / 12f) * 440f;
    }

    // Autohand Events...
    void OnSqueezed(Autohand.Hand hand, Grabbable grab)
    {
        //Called when the "Squeeze" event is called, this event is tied to a secondary controller input through the HandControllerLink component on the hand
        csound.SetChannel("noteon", 1);
        //noteOn = true;
        orbIndicator.StartColorShift();
    }
    void OnUnsqueezed(Autohand.Hand hand, Grabbable grab)
    {
        //Called when the "Unsqueeze" event is called, this event is tied to a secondary controller input through the HandControllerLink component on the hand
        csound.SetChannel("noteon", 0);
        //noteOn = false;
        orbIndicator.StartColorShiftBack();
    }
}
