using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Orb))]
public class OrbEditor : Editor
{
    [MenuItem("GameObject/3D Object/Orb")]
    public static void CreateOrb() {
        var gameObject = new GameObject("Orb");
        var orb = gameObject.AddComponent<Orb>();
        orb.GenerateSphere();
        orb.GenerateNewMaterial();
        orb.UpdateShader();
    }

    public override void OnInspectorGUI() {
        Orb myOrb = (Orb)target;

        // Original properties
        var ogSize = myOrb.size;
        var ogDetail = myOrb.detail;
        var ogEffect = myOrb.effect;
        var ogBrightness = myOrb.brightness;
        var ogDefinition = myOrb.definition;
        var ogVary = myOrb.vary;
        var ogColour = myOrb.colour;
        // Size
        myOrb.size = EditorGUILayout.Slider("Size", myOrb.size, 1f, 10f);
        myOrb.detail = EditorGUILayout.IntSlider("Detail", myOrb.detail, 1, 50);
        myOrb.effect = EditorGUILayout.Slider("Effect", myOrb.effect, 0, 1);
        myOrb.brightness = EditorGUILayout.Slider("Brightness", myOrb.brightness, 0, 1);
        myOrb.definition = EditorGUILayout.Slider("Definition", myOrb.definition, 0, 1);
        myOrb.colour = EditorGUILayout.ColorField("Colour", myOrb.colour);
        myOrb.vary = EditorGUILayout.Toggle("Vary", myOrb.vary);
        myOrb.varyOffset = EditorGUILayout.IntSlider("Vary Offset", myOrb.varyOffset, 0, 10000);

        // Initial generation
        if (!myOrb.IsGenerated) {
            myOrb.GenerateSphere();
            myOrb.GenerateNewMaterial();
            myOrb.UpdateShader();
        }

        // Automatically update orb
        if (ogEffect != myOrb.effect ||
                ogBrightness != myOrb.brightness ||
                ogDefinition != myOrb.definition ||
                ogColour != myOrb.colour ||
                ogVary != myOrb.vary ||
                ogSize != myOrb.size) {
            myOrb.UpdateShader();
        }
        if (ogSize != myOrb.size || ogDetail != myOrb.detail)
            myOrb.GenerateSphere();
    }
}