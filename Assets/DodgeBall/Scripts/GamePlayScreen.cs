using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayScreen : MonoBehaviour
{
    [Header("Button")]
    public Button btn_PickUpball;

    [Header("GameObject")]
    public GameObject btn_Throwball;

    [Header("Main Reference")]
    public PlayerController playerController;
    public Ball ball;

    [Header("Slider")]
    public Slider slider_HandPower;

    // --------------- Coroutines -------------------
    private Coroutine sliderCoroutine;

    void OnEnable()
    {
        btn_PickUpball.onClick.AddListener(playerController.PickupBall);
    }

    void OnDisable()
    {
        btn_PickUpball.onClick.RemoveListener(playerController.PickupBall);
    }

    void Start()
    {
        slider_HandPower.gameObject.SetActive(false);
        ManageThrowBallButton(false);
    }

    public void OnPressThrowBallButton()
    {
        slider_HandPower.gameObject.SetActive(true);
        AnimateSliderValue(true);
        playerController.StartAiming();
    }

    public void OnReleaseThrowBallButton()
    {
        float currentPower = slider_HandPower.value;
        playerController.ThrowBall(currentPower);

        AnimateSliderValue(false);
        slider_HandPower.gameObject.SetActive(false);

        ManageThrowBallButton(false);
    }

    public void ManageThrowBallButton(bool value)
    {
        btn_Throwball.gameObject.SetActive(value);
    }

    // ------------------ Slider Animation Method ------------------
    public void AnimateSliderValue(bool shouldStart)
    {
        if (shouldStart)
        {
            if (sliderCoroutine == null)
                sliderCoroutine = StartCoroutine(SliderLoop());
        }
        else
        {
            if (sliderCoroutine != null)
            {
                StopCoroutine(sliderCoroutine);
                sliderCoroutine = null;
                slider_HandPower.value = 0;
            }
        }
    }

    IEnumerator SliderLoop()
    {
        float duration = 1f;
        while (true)
        {
            // 0 to 1
            yield return StartCoroutine(LerpSliderValue(0f, 1f, duration));
            // 1 to 0
            yield return StartCoroutine(LerpSliderValue(1f, 0f, duration));
        }
    }

    IEnumerator LerpSliderValue(float from, float to, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            slider_HandPower.value = Mathf.Lerp(from, to, elapsed / duration);

            // Update trajectory with the current power - very important!
            playerController.DrawTrajectory(slider_HandPower.value);

            elapsed += Time.deltaTime;
            yield return null;
        }
        slider_HandPower.value = to;
    }
}