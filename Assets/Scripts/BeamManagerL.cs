using Autohand;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;

public class BeamManagerL : MonoBehaviour
{
    public Autohand.Hand hand;

    // Screen Fade
    [SerializeField] FadeToBlack screenFader;

    // Start is called before the first frame update
    void Start()
    {
        //find reference to hand
        GameObject[] hands = GameObject.FindGameObjectsWithTag("Left Hand");
        hand = hands[0].GetComponent<Autohand.Hand>();
    }

    private void OnEnable()
    {
        hand.OnSqueezed += OnSqueezed;
    }

    private void OnDisable()
    {
        hand.OnSqueezed -= OnSqueezed;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            screenFader.FadeAndReloadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    void OnSqueezed(Autohand.Hand hand, Grabbable grab)
    {
        //Called when the "Squeeze" event is called, this event is tied to a secondary controller input through the HandControllerLink component on the hand
        screenFader.FadeAndReloadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
