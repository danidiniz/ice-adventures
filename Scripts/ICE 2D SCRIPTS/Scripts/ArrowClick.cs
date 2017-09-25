using UnityEngine;
using System.Collections;

public class ArrowClick : MonoBehaviour
{

    public string direction;

    public void _clickedArrow(string arrow)
    {
        direction = arrow;
    }

}
