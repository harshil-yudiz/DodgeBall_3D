using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;



public enum ScreenNames
{
    HomeScreen,
    GamePlayScreen,
}

public enum PopUpNames
{

}

[System.Serializable]
public class ScreenType
{
    public ScreenNames screenName;
    public UIBase screenBase;
}

[System.Serializable]
public class PopUpType
{
    public PopUpNames popUpName;
    public UIBase popUpBase;
}

public class UIManager : Singleton<UIManager>
{
    #region PUBLIC_VARS
    [Header("Screen Canvas")]
    public List<ScreenType> screenTypes;
    public ScreenNames currentScreen;
    public ScreenNames previousScreen;
    public ScreenNames InitScreen;

    [Header("PopUp Canvas")]
    public List<PopUpType> popUpTypes;
    public PopUpNames currentPopUpScreen;
    public bool isPopUpOn;




    #endregion

    #region UNITY_CALLBACKS
    public override void Awake()
    {
        // Check if an instance already exists and destroy the duplicate
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();
        // DontDestroyOnLoad(gameObject); // Make the UIManager persistent across scenes
        InitializeScreen();
    }

    private void Start()
    {
        //  InitializeScreen();
    }
    #endregion

    #region PUBLIC_FUNCTIONS


    public void ShowNextScreen(ScreenNames screen)
    {
        screenTypes.Find(a => a.screenName == currentScreen).screenBase.HideScreen();
        previousScreen = currentScreen;
        currentScreen = screen;
        screenTypes.Find(a => a.screenName == currentScreen).screenBase.ShowScreen();
    }

    public void ShowPopUp(PopUpNames popUpScreen)
    {
        currentPopUpScreen = popUpScreen;
        popUpTypes.Find(a => a.popUpName == popUpScreen).popUpBase.ShowScreen();
        isPopUpOn = true;
    }

    public void HidePopUp()
    {
        if (isPopUpOn)
        {
            // Debug.Log("HidePopUp");
            popUpTypes.Find(a => a.popUpName == currentPopUpScreen).popUpBase.HideScreen();
            isPopUpOn = false;
        }
    }

    public T GetScreen<T>() where T : UIBase
    {
        ScreenType requestedScreenType = screenTypes.Find(st => st.screenBase.GetType().Name == typeof(T).Name);
        return requestedScreenType?.screenBase as T;
    }

    public T GetPopUp<T>() where T : UIBase
    {
        PopUpType requestedPopUpType = popUpTypes.Find(pt => pt.popUpBase.GetType().Name == typeof(T).Name);
        return requestedPopUpType?.popUpBase as T;
    }

    // public void ShowLoadingScreen(ScreenNames screenAfterLoading)
    // {
    //     // Get the LoadingScreen instance from your ScreenTypes
    //     LoadingScreen loadingScreen = GetScreen<LoadingScreen>();

    //     // Start the loading process and specify which screen to show after loading
    //     loadingScreen.StartLoading(screenAfterLoading);
    // }


    public void PopUpAnimation(RectTransform UiObject, float animationDuration)
    {
        UiObject.localScale = Vector3.zero;
        UiObject.DOScale(Vector3.one, animationDuration).SetEase(Ease.OutBack).SetDelay(0.1f);
    }
    #endregion

    #region PRIVATE_FUNCTIONS
    private void InitializeScreen()
    {
        foreach (var item in screenTypes)
        {
            if (item.screenName == InitScreen)
            {
                item.screenBase.ShowScreen();
                currentScreen = item.screenName;
            }
            else
            {
                item.screenBase.HideScreen();
            }
        }
    }

    #endregion
}
