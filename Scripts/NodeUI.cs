using UnityEngine;
using UnityEngine.UI;

public class NodeUI : NodeParent {

    public static NodeUI ins;

    public static tipo selectedType;

    public GameObject canvasMessages;

    public GameObject panelButtons;

    void OnEnable()
    {
        /* Events */
        EventManager.OnClickedShow += ShowButtons;
        EventManager.OnClickedHide += HideButtons;
    }

    void OnDisable()
    {
        /* Events */
        EventManager.OnClickedShow -= ShowButtons;
        EventManager.OnClickedHide -= HideButtons;
    }

    void Awake()
    {
        ins = this;

        selectedType = tipo.ICE;
    }

    public void ChangeSelectedType(tipo t)
    {
        selectedType = t;
    }

    public void SelectStart()
    {
        selectedType = tipo.START;
        SetMessage("Set where player starts");
    }

    public void SelectEnd()
    {
        selectedType = tipo.END;
        SetMessage("Set where map finishes");
    }

    public void SelectStone()
    {
        selectedType = tipo.STONE;
        SetMessage("Select an ice to have a stone");
    }

    public void SelectStoneGold()
    {
        selectedType = tipo.STONE_GOLD;
        SetMessage("Select a stone to have a gold");
    }

    public void SelectGold()
    {
        selectedType = tipo.GOLD;
        SetMessage("Select an ice to have a gold");
    }

    public void SelectTunel()
    {
        selectedType = tipo.TUNEL;
        SetMessage("Set the tunels");
    }

    public void SetPath()
    {
        selectedType = tipo.PATH;
        SetMessage("Set a path");
    }

    public void PathUndo()
    {
        if (GridManager.instance.pathOfPointNodes.Count > 0)
        {
            selectedType = tipo.PATH_UNDO;
            GridManager.instance.PathUndo();
        }
    }

    public void Remove()
    {
        selectedType = tipo.REMOVE;
        SetMessage("Remove");
    }

    public void SetMessage(string m)
    {
        ShowMessage(true);
        canvasMessages.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = m;
        HideButtons();
    }

    public void ShowMessage(bool b)
    {
        canvasMessages.SetActive(b);
    }

    public void ShowButtons()
    {
        panelButtons.SetActive(true);
    }

    public void HideButtons()
    {
        panelButtons.SetActive(false);
    }
}
