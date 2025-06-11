using UnityEngine;
using UnityEngine.UI;

public class HomeScreen : UIBase
{
   [Header("Button")]
    public Button btn_StartGame;
    [SerializeField] private NetworkManagerController networkController;

    public override void OnAwake()
    {
        base.OnAwake();
    }
    
    public override void ShowScreen()
    {
        base.ShowScreen();
    }

    public override void HideScreen()
    {
        base.HideScreen();
    }

    private void Start()
    {
        btn_StartGame.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        networkController.OnPlayButtonClick();
        btn_StartGame.interactable = false;
    }
}
