using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FadeToBlack : MonoBehaviour
{
    public Image fadeImage; // The UI Image that will be used to fade
    public float fadeDuration = 0.7f; // Duration of the fade
    public Coroutine currentCoroutine;

    private void Start()
    {
        if (fadeImage == null)
        {
            Debug.LogError("Fade Image is not assigned!");
        }
        currentCoroutine = StartCoroutine(FadeIn());
    }

    public void FadeAndReloadScene(int sceneName)
    {
        fadeImage.enabled = true;
        currentCoroutine = StartCoroutine(FadeOutAndReload(sceneName));
    }

    private IEnumerator FadeOutAndReload(int sceneName)
    {
        float elapsedTime = 0f;
        Color initialColor = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            fadeImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }

        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color initialColor = fadeImage.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(1 - elapsedTime / fadeDuration);
            fadeImage.color = new Color(initialColor.r, initialColor.g, initialColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(0, 0, 0, 0);
        fadeImage.enabled = false;
    }
}
