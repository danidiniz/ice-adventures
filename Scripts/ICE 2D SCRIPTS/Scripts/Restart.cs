using UnityEngine;
using System.Collections;

public class Restart : MonoBehaviour
{


    public void _RestartLevel()
    {
        if (GameObject.Find("Player").GetComponent<PlayerMovement>().canClick)
        {
            GameObject.Find("Player").transform.position = GameObject.Find("Start").transform.position;
            GameObject.Find("Player").GetComponent<SpriteRenderer>().sprite = GameObject.Find("Player").GetComponent<PlayerMovement>().down;
            GameObject.Find("Player").GetComponent<PlayerInfo>().info(GameObject.Find("Start").GetComponent<StartEndInfo>().line, GameObject.Find("Start").GetComponent<StartEndInfo>().column);
        }
    }

}
