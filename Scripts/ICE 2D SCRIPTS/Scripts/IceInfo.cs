using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class IceInfo : MonoBehaviour
{
    // Criar variaveis de Direção, igual no GridManager
    // AndouParaCima, etc
    // Criar uma classe e fazer estes herdarem


    // Variável pra saber se passou pelo ice
    public bool passed;

    // Está na ponta?
    public bool isCorner;

    // Está na reta do start ou end?
    public bool isRetaStart, isRetaEnd;

    // Tem uma pedra nesse ice?
    public bool isStone;

    public bool isStart, isEnd;

    public int stepOrder;

    public int line, column;

    public bool canMove;

    public int id;

    public void info(int lineI, int columnJ, int idd, bool pass, bool corner)
    {
        line = lineI;
        column = columnJ;
        id = idd;
        passed = pass;
        isCorner = corner;
    }

    public IceInfo(int lineI, int columnJ, int idd, bool pass, bool corner)
    {
        line = lineI;
        column = columnJ;
        id = idd;
        passed = pass;
        isCorner = corner;
    }


    public void setIsRetaStartEnd()
    {
        GridManager3 gridManager = GameObject.Find("Game Grid").GetComponent<GridManager3>();
        if (gridManager.start != null)
        {
            StartEndInfo startComp = gridManager.start.GetComponent<StartEndInfo>();
            // Mesma linha ou coluna do start
            if (line == startComp.line || column == startComp.column)
            {
                isRetaStart = true;
            }
        }

        if (gridManager.end != null)
        {
            StartEndInfo endComp = gridManager.end.GetComponent<StartEndInfo>();
            // Mesma linha ou coluna do end
            if (line == endComp.line || column == endComp.column)
            {
                isRetaEnd = true;
            }
        }
    }

}
