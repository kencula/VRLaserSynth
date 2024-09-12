using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OrbIndicator : MonoBehaviour

{
    public Orb orbR;
    public Orb orbL;
    public float duration = 2f; // The duration over which the color will shift
    public Color initialColor = new(204, 76, 51);
    public Color targetColor = new(255, 0, 165);

    float initSize;

    public float oscillationPeriod = 2;
    public float sizeScaling = 1.4f;
    public float oscilSizeScaling = 1.4f;

    bool isPlaying = false;

    private Coroutine currentCoroutine;

    private void Start()
    {
        initSize = orbR.size;
    }

    // Start the color shift to red
    public void StartColorShift()
    {
        // If a coroutine is already running, stop it
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        // Start a new coroutine to shift to red
        currentCoroutine = StartCoroutine(ShiftToColor(targetColor, duration));

        isPlaying = true;
    }

    // Start the color shift back to the initial color
    public void StartColorShiftBack()
    {
        // If a coroutine is already running, stop it
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        //Color currentColor = orbR.colour;

        // Start a new coroutine to shift back to the initial color
        currentCoroutine = StartCoroutine(ShiftToInit(initialColor, duration));

        isPlaying = false;
    }

    // Coroutine to gradually shift the color
    private IEnumerator ShiftToColor(Color targetColor, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = orbR.colour;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = elapsedTime / duration;

            Color colorOutput = Color.Lerp(startColor, targetColor, lerpValue);
            float sizeOutput = Mathf.Lerp(orbR.size, initSize * sizeScaling, lerpValue);
            orbR.colour = colorOutput;
            orbL.colour = colorOutput;
            orbR.size = sizeOutput;
            orbL.size = sizeOutput;
            orbR.UpdateShader();
            orbL.UpdateShader();

            yield return null;
        }

        // Ensure the final color is the target color
        orbR.colour = targetColor;
        orbL.colour = targetColor;
        orbR.UpdateShader();
        orbL.UpdateShader();

        if(isPlaying) currentCoroutine = StartCoroutine(Oscillate());
    }

    private IEnumerator ShiftToInit(Color targetColor, float duration)
    {
        float elapsedTime = 0f;
        Color startColor = orbR.colour;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float lerpValue = elapsedTime / duration;

            Color colorOutput = Color.Lerp(startColor, targetColor, lerpValue);
            float sizeOutput = Mathf.Lerp(orbR.size, initSize, lerpValue);
            orbR.colour = colorOutput;
            orbL.colour = colorOutput;
            orbR.size = sizeOutput;
            orbL.size = sizeOutput;
            orbR.UpdateShader();
            orbL.UpdateShader();

            yield return null;
        }

        // Ensure the final color is the target color
        orbR.colour = targetColor;
        orbL.colour = targetColor;
        orbR.UpdateShader();
        orbL.UpdateShader();

        if (isPlaying) currentCoroutine = StartCoroutine(Oscillate());
    }

    private IEnumerator Oscillate()
    {
        //float startSize = orbR.size;
        //float endSize = startSize * oscilSizeScaling;
        //float sizeOutput;

        float elapsedTime = 0f;
        Color startColor = orbR.colour;
        Color white = Color.white;
        Color colorOutput;
        float halfPeriod = oscillationPeriod / 2;
        while (true)
        {
            
            while (elapsedTime < oscillationPeriod)
            {
                //Debug.Log("Oscillating");
                if (elapsedTime < halfPeriod)
                {
                    colorOutput = Color.Lerp(startColor, white, elapsedTime / halfPeriod);
                    //sizeOutput = Mathf.Lerp(startSize, endSize, elapsedTime / halfPeriod);
                }
                else
                {
                    colorOutput = Color.Lerp(white, startColor, (elapsedTime - halfPeriod) / halfPeriod);
                    //sizeOutput = Mathf.Lerp(startSize, endSize, (elapsedTime - halfPeriod) / halfPeriod);
                }
                orbR.colour = colorOutput;
                orbL.colour = colorOutput;
                //orbR.size = sizeOutput;
                //orbL.size = sizeOutput;
                orbR.UpdateShader();
                orbL.UpdateShader();
                yield return null;
                elapsedTime += Time.deltaTime;
            }
            elapsedTime -= oscillationPeriod;
            //yield return null;
        }
    }
}
