using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerMovement : MonoBehaviour
{

    public bool canMove;

    public bool canClick;

    public float movementSpeed;

    private float xDirection, yDirection;

    GridManager4 gridManagerInfo;

    PlayerInfo playerInfo;

    GameObject iceToMove;

    public Sprite left, right, up, down;

    ArrowClick arrow;

    public List<IceInfo> icesCanClick = new List<IceInfo>();

    void Start()
    {
        canClick = true;

        GameObject[] ices = GameObject.FindGameObjectsWithTag("Ice");
        playerInfo = this.GetComponent<PlayerInfo>();
        gridManagerInfo = GameObject.Find("Game Grid").GetComponent<GridManager4>();

        arrow = GameObject.Find("ArrowClick").GetComponent<ArrowClick>();

        icesThatCanClick();
    }

    void Update()
    {
        if (canMove)
        {
            //transform.position = Vector3.Lerp(transform.position, iceToMove.transform.position, movementSpeed * Time.deltaTime);
            transform.position = Vector3.MoveTowards(transform.position, iceToMove.transform.position, movementSpeed * Time.deltaTime);
        }

        if (iceToMove != null)
        {
            if (transform.position == iceToMove.transform.position)
            {
                icesThatCanClick();
                canClick = true;
                canMove = false;
            }
        }

        if (canClick)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                setMovement("left");
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                setMovement("right");
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                setMovement("down");
            }
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                setMovement("up");
            }
        }

        if (canClick)
        {
            if (arrow.direction.Equals("left"))
            {
                setMovement("left");
            }
            else if (arrow.direction.Equals("right"))
            {
                setMovement("right");
            }
            else if (arrow.direction.Equals("down"))
            {
                setMovement("down");

            }
            else if (arrow.direction.Equals("up"))
            {
                setMovement("up");
            }
        }
    }

    public void icesThatCanClick()
    {
        // Ices que pode clicar
        for (int i = 0; i < gridManagerInfo.lines; i++)
        {
            icesCanClick.Add(gridManagerInfo.grid[i, playerInfo.column]);
        }
        for (int j = 0; j < gridManagerInfo.columns; j++)
        {
            icesCanClick.Add(gridManagerInfo.grid[playerInfo.line, j]);
        }
    }

    public void setMovement(string direction)
    {
        if (direction.Equals("left"))
        {
            this.GetComponent<SpriteRenderer>().sprite = left;

            for (int j = playerInfo.column - 1; j >= 0; j--)
            {
                // Se o ice à esquerda do player for uma stone, eu movo até o ice antes da stone
                if (gridManagerInfo.grid[playerInfo.line, j].isStone)
                {
                    iceToMove = gridManagerInfo.grid[playerInfo.line, j + 1].gameObject;
                    break;
                }
                else
                {
                    // Verifico se é o Start ou End
                    if (gridManagerInfo.grid[playerInfo.line, j].isStart || gridManagerInfo.grid[playerInfo.line, j].isEnd)
                    {
                        iceToMove = gridManagerInfo.grid[playerInfo.line, j].gameObject;
                        break;
                    }
                    // Se não eu continuo seguindo até o limite
                    else
                    {
                        iceToMove = gridManagerInfo.grid[playerInfo.line, j].gameObject;
                    }
                }
            }
        }
        else if (direction.Equals("right"))
        {
            this.GetComponent<SpriteRenderer>().sprite = right;

            for (int j = playerInfo.column + 1; j <= gridManagerInfo.columns - 1; j++)
            {
                // Se o ice à esquerda do player for uma stone, eu movo até o ice antes da stone
                if (gridManagerInfo.grid[playerInfo.line, j].isStone)
                {
                    iceToMove = gridManagerInfo.grid[playerInfo.line, j - 1].gameObject;
                    break;
                }
                else
                {
                    // Verifico se é o Start ou End
                    if (gridManagerInfo.grid[playerInfo.line, j].isStart || gridManagerInfo.grid[playerInfo.line, j].isEnd)
                    {
                        iceToMove = gridManagerInfo.grid[playerInfo.line, j].gameObject;
                        break;
                    }
                    // Se não eu continuo seguindo até o limite
                    else
                    {
                        iceToMove = gridManagerInfo.grid[playerInfo.line, j].gameObject;
                    }
                }
            }
        }
        else if (direction.Equals("down"))
        {
            this.GetComponent<SpriteRenderer>().sprite = down;

            for (int i = playerInfo.line + 1; i <= gridManagerInfo.lines - 1; i++)
            {
                // Se o ice à esquerda do player for uma stone, eu movo até o ice antes da stone
                if (gridManagerInfo.grid[i, playerInfo.column].isStone)
                {
                    iceToMove = gridManagerInfo.grid[i - 1, playerInfo.column].gameObject;
                    break;
                }
                else
                {
                    // Verifico se é o Start ou End
                    if (gridManagerInfo.grid[i, playerInfo.column].isStart || gridManagerInfo.grid[i, playerInfo.column].isEnd)
                    {
                        iceToMove = gridManagerInfo.grid[i, playerInfo.column].gameObject;
                        break;
                    }
                    // Se não eu continuo seguindo até o limite
                    else
                    {
                        iceToMove = gridManagerInfo.grid[i, playerInfo.column].gameObject;
                    }
                }
            }
        }
        else if (direction.Equals("up"))
        {
            this.GetComponent<SpriteRenderer>().sprite = up;

            for (int i = playerInfo.line - 1; i >= 0; i--)
            {
                // Se o ice à esquerda do player for uma stone, eu movo até o ice antes da stone
                if (gridManagerInfo.grid[i, playerInfo.column].isStone)
                {
                    iceToMove = gridManagerInfo.grid[i + 1, playerInfo.column].gameObject;
                    break;
                }
                else
                {
                    // Verifico se é o Start ou End
                    if (gridManagerInfo.grid[i, playerInfo.column].isStart || gridManagerInfo.grid[i, playerInfo.column].isEnd)
                    {
                        iceToMove = gridManagerInfo.grid[i, playerInfo.column].gameObject;
                        break;
                    }
                    // Se não eu continuo seguindo até o limite
                    else
                    {
                        iceToMove = gridManagerInfo.grid[i, playerInfo.column].gameObject;
                    }
                }
            }
        }

        playerInfo.info(iceToMove.GetComponent<IceInfo>().line, iceToMove.GetComponent<IceInfo>().column);

        Debug.Log("Player parou [" + playerInfo.line + "][" + playerInfo.column + "]");

        // Adiciono o ice que o player parou à lista de ices que ele percorreu
        // (para estatísticas e saber a dificuldade do level)
        LevelInfo.addIce(iceToMove.GetComponent<IceInfo>());

        arrow.direction = "";
        canMove = true;
        canClick = false;

        LevelInfo.stepsCount++;

        icesCanClick.Clear();
    }

}
