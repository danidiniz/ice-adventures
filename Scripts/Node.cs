using UnityEngine;

public class Node : NodeParent
{
    // Direção que a stone estará em relação ao node
    public enum directionOfStone { NORTH, SOUTH, EAST, WEST, NONE}
    public directionOfStone dirStone;
    // Par ordenado contendo a posição da stone que está em volta DESSE node
    // (é utilizada no método InstantiateStone() no GridManager)
    public parOrdenado posOfStone;

    private int line;
    public int Line
    {
        get { return line; }
        set { line = value; }
    }

    private int column;
    public int Column
    {
        get { return column; }
        set { column = value; }
    }

    public tipo nodeType;
    public tipo NodeType
    {
        get { return nodeType; }
        set { nodeType = value; }
    }

    public int gold;

    public Color colorHighlighted;
    public Color colorPath;
    public Color colorStartMaterial;
    [HideInInspector]
    public Color colorCurrent;
    [HideInInspector]
    public Renderer rend;

    private NodeUI nodeUI;

    public int tunelGoToLine;
    public int tunelGoToCol;

    void OnEnable()
    {
        posOfStone.i = -1;
        posOfStone.j = -1;

        tunelGoToLine = -1;
        tunelGoToCol = -1;

        NodeType = tipo.ICE;

        dirStone = directionOfStone.NONE;

        rend = GetComponent<Renderer>();
        
        rend.material.color = colorStartMaterial;
        colorCurrent = colorStartMaterial;

        /* Events */
        EventManager.OnClickedShow += DisableClick;
        EventManager.OnClickedHide += EnableClick;
    }

    void OnDisable()
    {
        /* Events */
        EventManager.OnClickedShow -= DisableClick;
        EventManager.OnClickedHide -= EnableClick;
    }

    public void ChangeNodeType(tipo t)
    {
        NodeType = t;
    }

    void OnMouseEnter()
    {
        rend.material.color = colorHighlighted;
    }

    void OnMouseExit()
    {
        rend.material.color = colorCurrent;
    }

    void OnMouseDown()
    {
        if(!canClick)
            return;

        if (NodeUI.selectedType == tipo.START)
        {
            if(!GridManager.instance.startInf.isActive)
            {

                GridManager.instance.startInf.line = Line;
                GridManager.instance.startInf.col = Column;
                GridManager.instance.startInf.isActive = true;

                // Destroi qualquer coisa que tiver nesse node
                GridManager.instance.ResetNode(this);

                NodeType = tipo.START;
                GameObject g = Instantiate(GridManager.instance.start, transform.position, Quaternion.identity) as GameObject;
                g.transform.parent = transform;
                g.name = "Start";
                NodeUI.ins.ShowMessage(false);
                return;
            }
            else
            {
                GridManager.instance.startInf.isActive = false;

                int l = GridManager.instance.startInf.line;
                int c = GridManager.instance.startInf.col;
                GridManager.instance.ResetNode(GridManager.instance.gridOfNodes[l, c]);

                OnMouseDown();
                return;
//              Debug.Log("Já possui um start no [" + GridManager.instance.startInf.line + "][" + GridManager.instance.startInf.col + "]");
            }
        }

        if (NodeUI.selectedType == tipo.END)
        {
            if (!GridManager.instance.endInf.isActive)
            {

                GridManager.instance.endInf.line = Line;
                GridManager.instance.endInf.col = Column;
                GridManager.instance.endInf.isActive = true;

                // Destroi qualquer coisa que tiver nesse node
                GridManager.instance.ResetNode(this);

                NodeType = tipo.END;
                GameObject g = Instantiate(GridManager.instance.end, transform.position, Quaternion.identity) as GameObject;
                g.transform.parent = transform;
                g.name = "End";

                NodeUI.ins.ShowMessage(false);
                return;
            }
            else
            {
                GridManager.instance.endInf.isActive = false;

                int l = GridManager.instance.endInf.line;
                int c = GridManager.instance.endInf.col;
                GridManager.instance.ResetNode(GridManager.instance.gridOfNodes[l, c]);

                // Chamo o método de novo para colocar o end no novo node
                OnMouseDown();
                return;
            }
        }

        if (NodeUI.selectedType == tipo.STONE)
        {
            GridManager.instance.InstantiateStone(this);
            NodeUI.ins.HideButtons();
            return;
        }
        if (NodeUI.selectedType == tipo.STONE_GOLD)
        {
            return;
        }
        if (NodeUI.selectedType == tipo.GOLD)
        {
            return;
        }
        if (NodeUI.selectedType == tipo.TUNEL)
        {
            // Freezar o canvas até q o player coloque os dois tunels
            // Mostrar canvas text dizendo "Coloque o primeiro tunel" e "Coloque o segundo tunel"
            GridManager.instance.SetTunel(this);
            NodeType = tipo.TUNEL;
            return;
        }
        if (NodeUI.selectedType == tipo.PATH)
        {
            GridManager.instance.PathMaker(this);
            NodeUI.ins.HideButtons();
            return;
        }
        if (NodeUI.selectedType == tipo.REMOVE)
        {
            // Se for um tunel, removo o outro tunel ligado à ele
            if(NodeType == tipo.TUNEL && (tunelGoToLine != -1 && tunelGoToCol != -1))
                GridManager.instance.ResetNode(GridManager.instance.gridOfNodes[tunelGoToLine, tunelGoToCol]);

            GridManager.instance.ResetNode(this);
            return;
        }
        ChangeNodeType(NodeUI.selectedType);
        //Debug.Log("Type: " + NodeType + "\n[" + Line + "][" + Column + "]");
    }

}
