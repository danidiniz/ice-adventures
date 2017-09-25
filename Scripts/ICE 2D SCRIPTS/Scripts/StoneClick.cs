using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class StoneClick : MonoBehaviour
{

    PlayerInfo player;

    StoneInfo stone;

    GridManager4 grid;

    private bool canClick;

    enum Position { Cima, Baixo, Esquerda, Direita, Longe };
    Position pos;

    public bool isBreakable;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerInfo>();
        stone = this.GetComponent<StoneInfo>();

        grid = GameObject.Find("Game Grid").GetComponent<GridManager4>();
    }

    void OnMouseDown()
    {
        Debug.Log("Clicou na Stone [" + stone.line + "][" + stone.column + "]");

        canClick = player.GetComponent<PlayerMovement>().canClick;

        if (canClick)
        {
            // Pula a stone
            if (GameObject.Find("Jump (deletar dps)").GetComponent<Toggle>().isOn)
                jumpStone();
            else // Quebra a stone
            {
                if (isBreakable)
                {
                    // Seta como false o isStone do ice que a stone está
                    grid.grid[stone.line, stone.column].isStone = false;
                    Destroy(this.gameObject);
                }
            }
        }
    }

    void playerPosRelacaoStone()
    {
        Debug.Log("Posição do player [" + player.line + "][" + player.column + "]");
        Debug.Log("Posição da stone  [" + stone.line + "][" + stone.column + "]");

        // Verifica se o Player está ao redor da Stone clicada

        // Vertical
        if (player.column == stone.column)
        {
            if (player.line == (stone.line - 1) && player.column == stone.column)
            {
                pos = Position.Cima;
                Debug.Log("Player em Cima");
            }
            else if (player.line == (stone.line + 1) && player.column == stone.column)
            {
                pos = Position.Baixo;
                Debug.Log("Player em Baixo");
            }
            else
            {
                pos = Position.Longe;
                Debug.Log("Player longe da stone");
            }
        }
        // Horizontal
        else
        {
            if (player.column == (stone.column - 1) && player.line == stone.line)
            {
                pos = Position.Esquerda;
                Debug.Log("Player na Esquerda");
            }
            else if (player.column == (stone.column + 1) && player.line == stone.line)
            {
                pos = Position.Direita;
                Debug.Log("Player na Direita");
            }
            else
            {
                pos = Position.Longe;
                Debug.Log("Player longe da stone");
            }
        }
    }

    void jumpStone()
    {
        // Posição do Player em relação à Stone
        playerPosRelacaoStone();

        if (pos != Position.Longe)
        {
            switch (pos)
            {
                // Se o player estiver à cima da stone e essa stone não estiver em um extremo,
                // pulo para o ice de baixo.
                // Mesma coisa para os demais.
                case Position.Cima:
                    // Se não for um extremo, não for uma Stone , não for o Start e não for o End
                    //if ((stone.line != (grid.lines - 1)) && !grid.grid[stone.line + 1, stone.column].isStone && !grid.grid[stone.line + 1, stone.column].isStart && !grid.grid[stone.line + 1, stone.column].isEnd)
                    if ((stone.line != (grid.lines - 1)) && !grid.grid[stone.line + 1, stone.column].isStone)
                    {
                        // Coloca o player na posição da Stone a baixo
                        player.transform.position = grid.grid[stone.line + 1, stone.column].transform.position;
                        // Atualiza a linha e coluna do player
                        player.info(stone.line + 1, stone.column);
                    }
                    break;
                case Position.Baixo:
                    // Se não for um extremo, não for uma Stone e não for o Start
                    //if ((stone.line != 0) && !grid.grid[stone.line - 1, stone.column].isStone && !grid.grid[stone.line - 1, stone.column].isStart && !grid.grid[stone.line - 1, stone.column].isEnd)
                    if ((stone.line != 0) && !grid.grid[stone.line - 1, stone.column].isStone)
                    {
                        // Coloca o player na posição da Stone a cima
                        player.transform.position = grid.grid[stone.line - 1, stone.column].transform.position;
                        // Atualiza a linha e coluna do player
                        player.info(stone.line - 1, stone.column);
                    }
                    break;
                case Position.Esquerda:
                    // Se não for um extremo, não for uma Stone e não for o Start
                    //if ((stone.column != (grid.columns - 1)) && !grid.grid[stone.line, stone.column + 1].isStone && !grid.grid[stone.line, stone.column + 1].isStart && !grid.grid[stone.line, stone.column + 1].isEnd)
                    if ((stone.column != (grid.columns - 1)) && !grid.grid[stone.line, stone.column + 1].isStone)
                    {
                        // Coloca o player na posição da Stone a esquerda
                        player.transform.position = grid.grid[stone.line, stone.column + 1].transform.position;
                        // Atualiza a linha e coluna do player
                        player.info(stone.line, stone.column + 1);
                    }
                    break;
                case Position.Direita:
                    // Se não for um extremo, não for uma Stone e não for o Start
                    //if ((stone.column != 0) && !grid.grid[stone.line, stone.column - 1].isStone && !grid.grid[stone.line, stone.column - 1].isStart && !grid.grid[stone.line, stone.column - 1].isEnd)
                    if ((stone.column != 0) && !grid.grid[stone.line, stone.column - 1].isStone)
                    {
                        // Coloca o player na posição da Stone a direita
                        player.transform.position = grid.grid[stone.line, stone.column - 1].transform.position;
                        // Atualiza a linha e coluna do player
                        player.info(stone.line, stone.column - 1);
                    }
                    break;
            }
        }
    }

}
