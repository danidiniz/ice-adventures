using UnityEngine;

public class EventManager : MonoBehaviour {

    public delegate void ClickAction();
    public static event ClickAction OnClickedShow;
    public static event ClickAction OnClickedHide;

    public void ShowButtons()
    {
        if (OnClickedShow != null)
            OnClickedShow();
    }

    public void HideButtons()
    {
        if (OnClickedHide != null)
            OnClickedHide();
    }

}
