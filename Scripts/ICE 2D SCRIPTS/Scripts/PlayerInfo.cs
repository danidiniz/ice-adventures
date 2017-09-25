using UnityEngine;
using System.Collections;

public class PlayerInfo : MonoBehaviour
{

    public int line, column;

    public void info(int lineI, int columnJ)
    {
        line = lineI;
        column = columnJ;
    }

}
