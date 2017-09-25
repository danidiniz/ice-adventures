using UnityEngine;
using System.Collections;

public class IceClick : MonoBehaviour
{

    PlayerMovement playerMov;
    PlayerInfo playerInfo;
    IceInfo iceInfo;

    void Start()
    {
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
            playerMov = player.GetComponent<PlayerMovement>();
            playerInfo = player.GetComponent<PlayerInfo>();
        }

        iceInfo = this.GetComponent<IceInfo>();
    }

    void OnMouseDown()
    {
        if (playerMov != null)
        {
            if (playerMov.canClick && playerMov.icesCanClick.Contains(iceInfo))
            {
                if (iceInfo.line < playerInfo.line)
                {
                    playerMov.setMovement("up");
                }
                else if (iceInfo.line > playerInfo.line)
                {
                    playerMov.setMovement("down");
                }
                else if (iceInfo.column < playerInfo.column)
                {
                    playerMov.setMovement("left");
                }
                else if (iceInfo.column > playerInfo.column)
                {
                    playerMov.setMovement("right");
                }
            }
        }
    }

}
