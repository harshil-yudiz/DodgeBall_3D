using UnityEngine;


public class UIBase : MonoBehaviour
{
    #region PUBLIC_VARS
    public Canvas canvas;
    #endregion

    #region PRIVATE_VARS
    #endregion

    #region UNITY_CALLBACKS
    public virtual void OnAwake()
    {

    }

    public void Awake()
    {
        canvas = GetComponent<Canvas>();
        OnAwake();
    }
    #endregion

    #region STATIC_FUNCTIONS
    #endregion

    #region PUBLIC_FUNCTIONS
    public virtual void ShowScreen()
    {
        canvas.enabled = true;
    }

    public virtual void HideScreen()
    {
        canvas.enabled = false;
    }
    #endregion

    #region PRIVATE_FUNCTIONS
    #endregion

    #region CO-ROUTINES
    #endregion

    #region EVENT_HANDLERS
    #endregion

    #region UI_CALLBACKS
    #endregion
}
