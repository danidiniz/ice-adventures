using UnityEngine;

public class NodeParent : MonoBehaviour {

    public static NodeParent instance;

    public enum tipo { START, END, STONE, STONE_GOLD, GOLD, TUNEL, PATH, PATH_UNDO, ICE, REMOVE };

    public struct parOrdenado { public int i; public int j; };

    public bool canClick;

    void Awake()
    {
        canClick = true;
        instance = this;
    }

    public void DisableClick()
    {
        canClick = false;
    }

    public void EnableClick()
    {
        canClick = true;
    }

}
