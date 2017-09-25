using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class GridManager4 : MonoBehaviour
{
    enum Direction { AndouParaCima, AndouParaEsquerda, AndouParaBaixo, AndouParaDireita, Parado };
    Direction currDir;

    enum Reta { Vertical, Horizontal, Parado };
    Reta currReta;

    public int lines, columns;
    public int minLines;
    public int maxLines;

    // Mínimo de 3 steps
    public int levelSteps;

    private int levelStepsCount;

    public GameObject icePrefab, playerPrefab, stonePrefab, stonePrefab2, startPrefab, endPrefab;

    // Game object contendo a instancia com component etc
    GameObject end;

    [HideInInspector]
    public IceInfo[,] grid;

    private List<IceInfo> corners = new List<IceInfo>();

    private float iceExtentsX, iceExtentsY;

    private GameObject player;

    PlayerInfo playerCopy;

    private List<IceInfo> moveOptions = new List<IceInfo>();

    private IceInfo startIce;
    private IceInfo endIce;

    // Lista contendo Ices do caminho do level
    List<IceInfo> caminhoIces = new List<IceInfo>();

    // Boolean dizendo se o player está na reta do Start. 
    // Se estiver, não pode randomizar um ice que também esteja na reta (Horizontal ou Vertical)
    private bool estouNaRetaDoStart;

    string debugIcesPassed = "";
    string debugIcesRandomizados = "Ices randomizados: \n";

    bool removerExtremos;

    void Update()
    {
        Text levestep = GameObject.Find("leve step deletar dps").GetComponent<Text>();
        levestep.text = "Level Step: " + levelSteps + "\n" + "Step Count: " + levelStepsCount + "\n" + "Map reloaded times: " + PlayerPrefs.GetInt("Map Reload");
    }

    void Awake()
    {
        estouNaRetaDoStart = false;

        levelStepsCount = 0;

        currDir = Direction.Parado;
        currReta = Reta.Parado;

        lines = Random.Range(minLines, maxLines);
        columns = lines + 3;
        //levelSteps = Random.Range(4, lines - 1);
        levelSteps = Random.Range(minLines, columns);


        // Medidas do ice para o espaçamento entre eles (*2 porque o extents pega sempre a metade)
        iceExtentsX = icePrefab.GetComponent<SpriteRenderer>().bounds.extents.x * 2;
        iceExtentsY = icePrefab.GetComponent<SpriteRenderer>().bounds.extents.y * 2;

        // Incializa o array do grid
        grid = new IceInfo[lines, columns];

        // Preenche o grid com os ices
        fillGrid();

        // Centraliza o grid
        centerGrid();

        // Randomizando posição do Start e End
        //randomStartEnd();

        //nLevels(startIce, grid, 0, Direction.Parado, Reta.Parado);

        // pega a matriz que contém o caminho e atualiza o grid com esse caminho, stones, etc
        //atualizaGridComMatrizDoLevel();


        /*

        try
        {
            setLevel();
        }
        catch (System.ArgumentOutOfRangeException e)
        {
            Debug.LogError("------------------" + e + "------------------");

            PlayerPrefs.SetInt("Map Reload", PlayerPrefs.GetInt("Map Reload") + 1);

            Application.LoadLevel(Application.loadedLevel);

            //// Instanciando End no último ice que o player esteve
            //end = Instantiate(endPrefab, grid[playerCopy.line, playerCopy.column].gameObject.transform.position, Quaternion.identity) as GameObject;
            //end.name = "End";
            //// Setando ice que está o End
            //grid[playerCopy.line, playerCopy.column].isEnd = true;
            //// Setando linha e coluna que o End está
            //end.GetComponent<StartEndInfo>().info(playerCopy.line, playerCopy.column);

            //// Setando segundo caminho
            ////setSegundoCaminho(startIce, playerCopy);

            //// Destruindo a cópia
            //Destroy(playerCopy.gameObject);


            //// Mudando a cor dos ices que não passei para saber onde colocar as coins, etc
            //// deletar dps
            //GameObject[] icesnot = GameObject.FindGameObjectsWithTag("Ice");
            //for (int i = 0; i < icesnot.Length; i++)
            //{
            //    if (!icesnot[i].GetComponent<IceInfo>().passed)
            //        icesnot[i].GetComponent<SpriteRenderer>().color = Color.gray;
            //}


            //icesNotPassed2();
        }

        // Apenas certificando que o Ice do Start não seja uma Stone
        startIce.isStone = false;



        //Debug.Log("------------------Direções até o final------------------");
        //for (int i = 0; i < seqDirec.Count; i++)
        //{
        //    Debug.Log(seqDirec[i]);
        //} */
    } 

    void setLevel()
    {
        // Ice que vai ser o Start
        startIce = grid[Random.Range(0, lines), Random.Range(0, columns)];

        // Setando que esse Ice é o Start
        startIce.isStart = true;

        // Instanciando o Start nesse Ice
        GameObject start = (Instantiate(startPrefab, startIce.transform.position, Quaternion.identity)) as GameObject;
        start.transform.SetParent(transform);
        start.name = "Start";
        start.GetComponent<StartEndInfo>().info(startIce.line, startIce.column);

        // Setando os Ices que estão na reta do Start
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesInScene.Length; i++)
        {
            if (icesInScene[i].GetComponent<IceInfo>().line == startIce.line || icesInScene[i].GetComponent<IceInfo>().column == startIce.column)
            {
                icesInScene[i].GetComponent<IceInfo>().isRetaStart = true;
            }
        }

        // Instanciando o player
        PlayerInfo player = ((Instantiate(playerPrefab, startIce.transform.position, Quaternion.identity)) as GameObject).GetComponent<PlayerInfo>();
        player.gameObject.name = "Player";
        player.transform.SetParent(transform);

        player.transform.SetParent(transform);
        // Setando posição do player no Start
        player.info(startIce.line, startIce.column);

        // Criando uma cópia do Player
        // Assim, posso criar o Level e depois destruir essa cópia sem alterar a linha e coluna original do Player
        playerCopy = (Instantiate(player.gameObject, player.transform.position, Quaternion.identity) as GameObject).GetComponent<PlayerInfo>();

        Debug.Log("O Player começou na posição [" + player.line + "][" + player.column + "]");

        // Steps
        for (int i = 0; i < levelSteps; i++)
        {
            Debug.Log("------------------Step " + i + " E Step Count: " + levelStepsCount + "------------------");

            // Definindo ices que o player pode se mover
            setMoveOptions(playerCopy);

            if (moveOptions.Count <= 0)
            {
                throw new System.ArgumentOutOfRangeException();
            }

            // Randomizando um ice para se mover
            IceInfo iceTarget = moveOptions[Random.Range(0, moveOptions.Count)];

            // Lista do caminho
            caminhoIces.Add(iceTarget);

            // Setando direção
            setDirection(playerCopy, iceTarget);

            ////////////// MUDANDO O ICE DE COR APENAS PARA SABER O CAMINHO
//            iceTarget.GetComponent<SpriteRenderer>().color = Color.red;

            //debugIcesRandomizados += i + ": [" + iceTarget.line + "][" + iceTarget.column + "]" + "\n";

            // Setando os ices que o player passou
            setIcesPassed(playerCopy, iceTarget);

            // Setando a stone ao lado do ice
            setStone(playerCopy, iceTarget);

            // Atualiza posição do player
            playerCopy.info(iceTarget.line, iceTarget.column);

            Debug.Log("O Player se moveu até: " + playerCopy.line + " " + playerCopy.column);

            moveOptions.Clear();

            levelStepsCount++;
        }

        // Instanciando End
        end = Instantiate(endPrefab, grid[playerCopy.line, playerCopy.column].gameObject.transform.position, Quaternion.identity) as GameObject;
        end.name = "End";
        // Setando ice que está o End
        grid[playerCopy.line, playerCopy.column].isEnd = true;
        // Setando linha e coluna que o End está
        end.GetComponent<StartEndInfo>().info(playerCopy.line, playerCopy.column);

        // Setando segundo caminho
        //setSegundoCaminho(startIce, playerCopy);

        // Destruindo a cópia
        Destroy(playerCopy.gameObject);


        // Mudando a cor dos ices que não passei para saber onde colocar as coins, etc
        // deletar dps
        GameObject[] icesnot = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesnot.Length; i++)
        {
            if (!icesnot[i].GetComponent<IceInfo>().passed)
                icesnot[i].GetComponent<SpriteRenderer>().color = Color.gray;
        }

        icesNotPassed2();

        //Debug.Log(debugIcesRandomizados);

    }

    // Segundo caminho (caminho das coins ou sei la)
    void setSegundoCaminho(IceInfo iceStart, PlayerInfo playerCopy)
    {
        // Seta o player na posição start
        playerCopy.info(iceStart.line, iceStart.column);

        Debug.Log("-------------------------------------------------------------");
        Debug.Log("-------------------------------------------------------------");
        Debug.Log("------------------SEGUNDO CAMINHO COMEÇANDO------------------");
        Debug.Log("-------------------------------------------------------------");
        Debug.Log("-------------------------------------------------------------");


        // Steps
        // 1 a menos que o caminho normal pq o último precisa levar a algum ice que o player passou,
        // assim ele poderá chegar ao final do level
        for (int i = 0; i < levelSteps - 1; i++)
        {
            Debug.Log("------------------Step " + i + " E Step Count: " + levelStepsCount + "------------------");

            // Definindo ices que o player pode se mover
            setMoveOptions(playerCopy);

            // Randomizando um ice para se mover
            IceInfo iceTarget = moveOptions[Random.Range(0, moveOptions.Count)];

            ////////////// MUDANDO O ICE DE COR APENAS PARA SABER O CAMINHO
            iceTarget.GetComponent<SpriteRenderer>().color = Color.blue;

            //debugIcesRandomizados += i + ": [" + iceTarget.line + "][" + iceTarget.column + "]" + "\n";

            // Setando os ices que o player passou
            setIcesPassed(playerCopy, iceTarget);

            // Setando a stone ao lado do ice
            setStone(playerCopy, iceTarget);

            // Atualiza posição do player
            playerCopy.info(iceTarget.line, iceTarget.column);

            Debug.Log("O Player se moveu até: " + playerCopy.line + " " + playerCopy.column);

            moveOptions.Clear();
            
        }

    }

    // Ices na reta do player
    void icesNaRetaDoPlayer(PlayerInfo playerPosition)
    {
        // Adicionando todos ices na coluna do Player
        for (int i = 0; i < lines; i++)
        {
            // Se não for a posição que o player está, adiciona
            if (i != playerPosition.line)
            {
                moveOptions.Add(grid[i, playerPosition.column]);
            }

        }
        // Adicionando todos ices na linha do Player
        for (int j = 0; j < columns; j++)
        {
            // Se não for a posição que o player está, adiciona
            if (j != playerPosition.column)
            {
                moveOptions.Add(grid[playerPosition.line, j]);
            }
        }
    }

    // Recursividade
    void icesNaReta(IceInfo currIcePos, IceInfo[,] matriz)
    {
        // Adicionando todos ices na coluna do Player
        for (int i = 0; i < lines; i++)
        {
            // Se não for a posição que o player está, adiciona
            if (i != currIcePos.line)
            {
                moveOptions.Add(matriz[i, currIcePos.column]);
            }

        }
        // Adicionando todos ices na linha do Player
        for (int j = 0; j < columns; j++)
        {
            // Se não for a posição que o player está, adiciona
            if (j != currIcePos.column)
            {
                moveOptions.Add(matriz[currIcePos.line, j]);
            }
        }
    }

    // MoveOptions
    void setMoveOptions(PlayerInfo playerPos)
    {
        // Preenche a lista com os ices na reta do player
        icesNaRetaDoPlayer(playerPos);

        Debug.Log("------------------Setou os ices na reta do player------------------");

        Debug.Log("------------------Quantidade de Ices na Lista: " + moveOptions.Count + "------------------");

        // Se estiver no último step, retiro os Extremos e os que estão na Reta do Start
        if (levelStepsCount == (levelSteps - 1))
        {
            Debug.Log("------------------Último step. Removendo extremos e Reta do Start------------------");
            for (int i = 0; i < moveOptions.Count; i++)
            {
                if (moveOptions[i].isCorner || moveOptions[i].isRetaStart)
                {
                    Debug.Log("Removeu extremo ou reta do start: [" + moveOptions[i].line + "][" + moveOptions[i].column + "]");
                    moveOptions.Remove(moveOptions[i]);
                    i--;
                }
            }
        }

        //// Removendo os Ices que já passou, são Stones e o Start
        int contador = 0;
        for (int i = 0; i < moveOptions.Count; i++)
        {
            Debug.Log("Verificando Ice " + contador + ": [" + moveOptions[i].line + "][" + moveOptions[i].column + "] / Passed: " + moveOptions[i].passed + " / Stone: " + moveOptions[i].isStone + " / Start: " + moveOptions[i].isStart);
            if (moveOptions[i].passed || moveOptions[i].isStone || moveOptions[i].isStart)
            {
                Debug.Log("Removeu    Ice " + contador + ": [" + moveOptions[i].line + "][" + moveOptions[i].column + "]");
                moveOptions.Remove(moveOptions[i]);
                i--;
            }
            contador++;
        }

        Debug.Log("------------------Quantidade de Ices após remoção: " + moveOptions.Count + "------------------");

        Debug.Log("------------------Removeu ices Passed, Stones e Start------------------");

        // Se o player parar em um ice na reta do start, dependendo da direção que ele está vou precisar
        // retirar os ices que estão à cima, à baixo ou ao lado do Start e do Player
        // Enfim, vou deixar apenas os Ices ENTRE o Player e o Start, retirando o que está ao lado do Start (caso contrario eu teria que colocar uma Stone onde está o Start), por isso o + 1 no if
        if (moveOptions != null && (startIce.line == playerPos.line || startIce.column == playerPos.column))
        {
            Debug.Log("------------------Deixando na lista Ices entre Start e Player------------------");

            for (int k = 0; k < moveOptions.Count; k++)
            {
                Debug.Log("Ice " + k + " [" + moveOptions[k].line + "][" + moveOptions[k].column + "]");

                // Se o Start estiver à cima do ice que o player está
                if (startIce.line < playerPos.line)
                {
                    //Debug.Log("Start [" + startIce.line + "][" + startIce.column + "] à cima do Player [" + playerPos.line + "][" + playerPos.column + "]");
                    // Retirando todos ices que não estão entre o Ice e o Player
                    for (int i = 0; i < moveOptions.Count; i++)
                    {
                        // Vou verificar apenas os Ices na reta do StartIce/Player
                        if (moveOptions[i].column == startIce.column)
                        {
                            //Debug.Log("Ice " + i + " [" + moveOptions[i].line + "][" + moveOptions[i].column + "] ESTÁ NA MESMA COLUNA");

                            if (moveOptions[i].line <= startIce.line || moveOptions[i].line >= playerPos.line)
                            {
                                Debug.Log("Ice " + i + " [" + moveOptions[i].line + "][" + moveOptions[i].column + "] REMOVIDO");
                                moveOptions.Remove(moveOptions[i]);
                            }
                        }
                    }
                }
                // Se o Start estiver à baixo do ice que o player está
                else if (startIce.line > playerPos.line)
                {
                    //Debug.Log("Start [" + startIce.line + "][" + startIce.column + "] à baixo do Player [" + playerPos.line + "][" + playerPos.column + "]");
                    // Retirando todos ices que não estão entre o Ice e o Player
                    for (int i = 0; i < moveOptions.Count; i++)
                    {
                        // Vou verificar apenas os Ices na reta do StartIce/Player
                        if (moveOptions[i].column == startIce.column)
                        {
                            //Debug.Log("Ice " + i + " [" + moveOptions[i].line + "][" + moveOptions[i].column + "] ESTÁ NA MESMA COLUNA");

                            if (moveOptions[i].line >= startIce.line || moveOptions[i].line <= playerPos.line)
                            {
                                Debug.Log("Ice " + i + " [" + moveOptions[i].line + "][" + moveOptions[i].column + "] REMOVIDO");
                                moveOptions.Remove(moveOptions[i]);
                            }
                        }
                    }
                }
                // Se o Start estiver à direita do ice que o player está
                if (startIce.column > playerPos.column)
                {
                    //Debug.Log("Start [" + startIce.line + "][" + startIce.column + "] à direita do Player [" + playerPos.line + "][" + playerPos.column + "]");
                    // Retirando todos ices que não estão entre o Ice e o Player
                    for (int j = 0; j < moveOptions.Count; j++)
                    {
                        // Vou verificar apenas os Ices na reta do StartIce/Player
                        if (moveOptions[j].line == startIce.line)
                        {
                            //Debug.Log("Ice " + j + " [" + moveOptions[j].line + "][" + moveOptions[j].column + "] ESTÁ NA MESMA LINHA");

                            if (moveOptions[j].column >= startIce.column || moveOptions[j].column <= playerPos.column)
                            {
                                Debug.Log("Ice " + j + " [" + moveOptions[j].line + "][" + moveOptions[j].column + "] REMOVIDO");
                                moveOptions.Remove(moveOptions[j]);
                            }
                        }
                    }
                }
                // Se o Start estiver à esquerda do ice que o player está
                if (startIce.column < playerPos.column)
                {
                    //Debug.Log("Start [" + startIce.line + "][" + startIce.column + "] à esquerda do Player [" + playerPos.line + "][" + playerPos.column + "]");
                    // Retirando todos ices que não estão entre o Ice e o Player
                    for (int j = 0; j < moveOptions.Count; j++)
                    {
                        // Vou verificar apenas os Ices na reta do StartIce/Player
                        if (moveOptions[j].line == startIce.line)
                        {
                            //Debug.Log("Ice " + j + " [" + moveOptions[j].line + "][" + moveOptions[j].column + "] ESTÁ NA MESMA LINHA");

                            if (moveOptions[j].column <= startIce.column || moveOptions[j].column >= playerPos.column)
                            {
                                Debug.Log("Ice " + j + " [" + moveOptions[j].line + "][" + moveOptions[j].column + "] REMOVIDO");
                                moveOptions.Remove(moveOptions[j]);
                            }
                        }
                    }
                }
            }
        }

        if (moveOptions.Count > 0)
        {
            Debug.Log("------------------Removendo Ices no sentido contrário do player------------------");

            if (currDir == Direction.AndouParaCima)
            {
                // Retiro os que estão em baixo
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].line > playerPos.line && moveOptions[i].column == playerPos.column)
                    {
                        Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + currDir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }
            else if (currDir == Direction.AndouParaBaixo)
            {
                // Retiro os que estão em cima
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].line < playerPos.line && moveOptions[i].column == playerPos.column)
                    {
                        Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + currDir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }
            else if (currDir == Direction.AndouParaDireita)
            {
                // Retiro os que estão em esquerda
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].column < playerPos.column && moveOptions[i].line == playerPos.line)
                    {
                        Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + currDir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }
            else if (currDir == Direction.AndouParaEsquerda)
            {
                // Retiro os que estão em direita
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].column > playerPos.column && moveOptions[i].line == playerPos.line)
                    {
                        Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + currDir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }

        }

        // Dobrando as chances dele mudar a direção da 'Reta' (Vertical e Horizontal)
        // Se ele estiver na Vertical, tem maior chance de randomizar um ice na Horizontal. Vice versa
        List<IceInfo> temporary = new List<IceInfo>();
        if (moveOptions.Count > 0)
        {
            Debug.Log("------------------Dobrando as chances de mudar de Reta------------------");
            if (currReta == Reta.Vertical)
            {
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    // Adiciono os que estão na Horizontal
                    if (moveOptions[i].line == playerPos.line)
                    {
                        Debug.Log("Dobrou as chances do ice Horizontal [" + moveOptions[i].line + "][" + moveOptions[i].column + "] ");
                        temporary.Add(moveOptions[i]);
                    }
                }
            }
            else if (currReta == Reta.Horizontal)
            {
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    // Adiciono os que estão na Vertical
                    if (moveOptions[i].column == playerPos.column)
                    {
                        Debug.Log("Dobrou as chances do ice Vertical [" + moveOptions[i].line + "][" + moveOptions[i].column + "] ");
                        temporary.Add(moveOptions[i]);
                    }
                }
            }

            // Adiciono esses ices que separei à lista de moveOptions.
            // Fiz isso pq se eu adicionasse direto à lista de moveOptions dentro do for, ia dar um loop
            for (int i = 0; i < temporary.Count; i++)
            {
                moveOptions.Add(temporary[i]);
            }
        }
        Debug.Log("------------------------------------");
        Debug.Log("------------------------------------");
        Debug.Log("------------------Mostrando o Move Options FINAL------------------");
        Debug.Log("------------------------------------");
        Debug.Log("------------------------------------");
        for (int i = 0; i < moveOptions.Count; i++)
        {
            Debug.Log("Ice " + i +  ": [" + moveOptions[i].line + "][" + moveOptions[i].column + "]");
        }

        Debug.Log("------------------Finalizou------------------");
    }

    // Recursividade
    void setMoveOptions(IceInfo ice, Direction dir, Reta reta, IceInfo[,] matriz)
    {
        // Preenche a lista com os ices na reta do player
        icesNaReta(ice, matriz);

        //Debug.Log("------------------Setou os ices na reta do player------------------");

        //Debug.Log("------------------Quantidade de Ices na Lista: " + moveOptions.Count + "------------------");

        //// Removendo os Ices que já passou, são Stones e o Start
        int contador = 0;
        for (int i = 0; i < moveOptions.Count; i++)
        {
            //Debug.Log("Verificando Ice " + contador + ": [" + moveOptions[i].line + "][" + moveOptions[i].column + "] / Passed: " + moveOptions[i].passed + " / Stone: " + moveOptions[i].isStone + " / Start: " + moveOptions[i].isStart);
            if (moveOptions[i].Equals("4") || moveOptions[i].Equals("1") || moveOptions[i].Equals("2"))
            {
                //Debug.Log("Removeu    Ice " + contador + ": [" + moveOptions[i].line + "][" + moveOptions[i].column + "]");
                moveOptions.Remove(moveOptions[i]);
                i--;
            }
            contador++;
        }

        //Debug.Log("------------------Quantidade de Ices após remoção: " + moveOptions.Count + "------------------");

        //Debug.Log("------------------Removeu ices Passed, Stones e Start------------------");

        if (moveOptions.Count > 0)
        {
            //Debug.Log("------------------Removendo Ices no sentido contrário do player------------------");

            if (dir == Direction.AndouParaCima)
            {
                // Retiro os que estão em baixo
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].line > ice.line && moveOptions[i].column == ice.column)
                    {
                        //Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + dir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }
            else if (dir == Direction.AndouParaBaixo)
            {
                // Retiro os que estão em cima
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].line < ice.line && moveOptions[i].column == ice.column)
                    {
                        //Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + dir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }
            else if (dir == Direction.AndouParaDireita)
            {
                // Retiro os que estão em esquerda
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].column < ice.column && moveOptions[i].line == ice.line)
                    {
                        //Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + dir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }
            else if (dir == Direction.AndouParaEsquerda)
            {
                // Retiro os que estão em direita
                for (int i = 0; i < moveOptions.Count; i++)
                {
                    if (moveOptions[i].column > ice.column && moveOptions[i].line == ice.line)
                    {
                        //Debug.Log("Removeu o ice [" + moveOptions[i].line + "][" + moveOptions[i].column + "] " + dir);
                        moveOptions.Remove(moveOptions[i]);
                        i--;
                    }
                }
            }

        }

        // Dobrando as chances dele mudar a direção da 'Reta' (Vertical e Horizontal)
        // Se ele estiver na Vertical, tem maior chance de randomizar um ice na Horizontal. Vice versa
        //List<IceInfo> temporary = new List<IceInfo>();
        //if (moveOptionsEmString.Count > 0)
        //{
        //    Debug.Log("------------------Dobrando as chances de mudar de Reta------------------");
        //    if (currReta == Reta.Vertical)
        //    {
        //        for (int i = 0; i < moveOptionsEmString.Count; i++)
        //        {
        //            // Adiciono os que estão na Horizontal
        //            if (moveOptionsEmString[i].line == ice.line)
        //            {
        //                Debug.Log("Dobrou as chances do ice Horizontal [" + moveOptionsEmString[i].line + "][" + moveOptionsEmString[i].column + "] ");
        //                temporary.Add(moveOptionsEmString[i]);
        //            }
        //        }
        //    }
        //    else if (currReta == Reta.Horizontal)
        //    {
        //        for (int i = 0; i < moveOptionsEmString.Count; i++)
        //        {
        //            // Adiciono os que estão na Vertical
        //            if (moveOptionsEmString[i].column == ice.column)
        //            {
        //                Debug.Log("Dobrou as chances do ice Vertical [" + moveOptionsEmString[i].line + "][" + moveOptionsEmString[i].column + "] ");
        //                temporary.Add(moveOptionsEmString[i]);
        //            }
        //        }
        //    }

        //    // Adiciono esses ices que separei à lista de moveOptions.
        //    // Fiz isso pq se eu adicionasse direto à lista de moveOptions dentro do for, ia dar um loop
        //    for (int i = 0; i < temporary.Count; i++)
        //    {
        //        moveOptionsEmString.Add(temporary[i]);
        //    }
        //}

        //Debug.Log("------------------Mostrando o Move Options FINAL------------------");

        //for (int i = 0; i < moveOptions.Count; i++)
        //{
        //    Debug.Log("Ice " + i + ": [" + moveOptions[i].line + "][" + moveOptions[i].column + "]");
        //}

        //Debug.Log("------------------Finalizou------------------");
    }

    void setIcesPassed(PlayerInfo playerPos, IceInfo iceTargetPos)
    {
        // Setando os de cima
        if (iceTargetPos.line < playerPos.line)
        {
            for (int i = iceTargetPos.line; i < playerPos.line; i++)
            {
                grid[i, playerPos.column].passed = true;
            }
        }

        // Setando os de baixo
        if (iceTargetPos.line > playerPos.line)
        {
            for (int i = iceTargetPos.line; i > playerPos.line; i--)
            {
                grid[i, playerPos.column].passed = true;
            }
        }

        // Setando os da esquerda
        if (iceTargetPos.column < playerPos.column)
        {
            for (int j = iceTargetPos.column; j < playerPos.column; j++)
            {
                grid[playerPos.line, j].passed = true;
            }
        }

        // Setando os da direita
        if (iceTargetPos.column > playerPos.column)
        {
            for (int j = iceTargetPos.column; j > playerPos.column; j--)
            {
                grid[playerPos.line, j].passed = true;
            }
        }
    }

    // Recursividade
    void setIcesPassedRecursividade(IceInfo currIcePos, IceInfo nextIcePos, IceInfo[,] matriz)
    {
        // Setando os de cima
        if (nextIcePos.line < currIcePos.line)
        {
            for (int i = nextIcePos.line; i < currIcePos.line; i++)
            {
                matriz[i, currIcePos.column].passed = true;
            }
        }

        // Setando os de baixo
        if (nextIcePos.line > currIcePos.line)
        {
            for (int i = nextIcePos.line; i > currIcePos.line; i--)
            {
                matriz[i, currIcePos.column].passed = true;
            }
        }

        // Setando os da esquerda
        if (nextIcePos.column < currIcePos.column)
        {
            for (int j = nextIcePos.column; j < currIcePos.column; j++)
            {
                matriz[currIcePos.line, j].passed = true;
            }
        }

        // Setando os da direita
        if (nextIcePos.column > currIcePos.column)
        {
            for (int j = nextIcePos.column; j > currIcePos.column; j--)
            {
                matriz[currIcePos.line, j].passed = true;
            }
        }
    }

    // Esse método coloca stones apenas nos ices que não passei e estão na reta do end ou start
    void icesNotPassed()
    {
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesInScene.Length; i++)
        {
            if (!icesInScene[i].GetComponent<IceInfo>().passed)
            {
                if (Random.Range(0, 3) == 0)
                {
                    if (icesInScene[i].GetComponent<IceInfo>().isRetaEnd || icesInScene[i].GetComponent<IceInfo>().isRetaStart)
                    {
                        GameObject stone = Instantiate(stonePrefab2, icesInScene[i].transform.position, Quaternion.identity) as GameObject;

                        grid[icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column].isStone = true;

                        stone.transform.SetParent(transform);

                        stone.GetComponent<StoneInfo>().info(icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column);
                    }

                    //Instantiate(stonePrefab, icesInScene[i].transform.position, Quaternion.identity);

                    //grid[icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column].isStone = true;
                }
            }
        }
    }

    // Esse coloca em qualquer lugar, menos na linha ou coluna a cima ou a baixo do End
    void icesNotPassed2()
    {
        Debug.Log("------------------Mostrando ices que não podem ter stones do tipo 2------------------");

        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesInScene.Length; i++)
        {
            if (!icesInScene[i].GetComponent<IceInfo>().passed)
            {
                if ((icesInScene[i].GetComponent<IceInfo>().line == (end.GetComponent<StartEndInfo>().line - 1) || icesInScene[i].GetComponent<IceInfo>().line == (end.GetComponent<StartEndInfo>().line + 1) )&&(
                    icesInScene[i].GetComponent<IceInfo>().column != end.GetComponent<StartEndInfo>().column))
                {
                    Debug.Log("Não instanciou stone 2 no ice: [" + icesInScene[i].GetComponent<IceInfo>().line + "][" + icesInScene[i].GetComponent<IceInfo>().column + "] / Passed: " + icesInScene[i].GetComponent<IceInfo>().passed);
                    // Mudando a cor apenas para facilitar de ver (deletar dps)
//                    icesInScene[i].GetComponent<SpriteRenderer>().color = Color.green;
                    // Não faz nada
                }
                else if ((icesInScene[i].GetComponent<IceInfo>().column == (end.GetComponent<StartEndInfo>().column - 1) || icesInScene[i].GetComponent<IceInfo>().column == (end.GetComponent<StartEndInfo>().column + 1) )&&(
                    icesInScene[i].GetComponent<IceInfo>().line != end.GetComponent<StartEndInfo>().line))
                {
                    Debug.Log("Não instanciou stone 2 no ice: [" + icesInScene[i].GetComponent<IceInfo>().line + "][" + icesInScene[i].GetComponent<IceInfo>().column + "] / Passed: " + icesInScene[i].GetComponent<IceInfo>().passed);
                    // Mudando a cor apenas para facilitar de ver (deletar dps)
//                   icesInScene[i].GetComponent<SpriteRenderer>().color = Color.green;
                    // Não faz nada
                }
                else
                {
                    if (Random.Range(0, 3) == 0)
                    {
                        GameObject stone = Instantiate(stonePrefab2, icesInScene[i].transform.position, Quaternion.identity) as GameObject;

                        grid[icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column].isStone = true;

                        stone.transform.SetParent(transform);

                        stone.GetComponent<StoneInfo>().info(icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column);
                    }
                }

                //Instantiate(stonePrefab, icesInScene[i].transform.position, Quaternion.identity);

                //grid[icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column].isStone = true;

            }
        }
    }

    void setStone(PlayerInfo playerPos, IceInfo iceTargetPos)
    {
        // Posição do ice em relação ao player

        // O ICE QUE IREI COLOCAR A STONE > Não pode ser uma Stone, nem estar no Extremo

        // Cima (linha do ice < linha do player)
        if (iceTargetPos.line < playerPos.line)
        {
            // Se não estiver no extremo de cima
            if (iceTargetPos.line != 0)
            {
                // Coloca uma stone em cima do ice
                ((Instantiate(stonePrefab, grid[iceTargetPos.line - 1, iceTargetPos.column].transform.position, Quaternion.identity)) as GameObject).GetComponent<StoneInfo>().info(iceTargetPos.line - 1, iceTargetPos.column);
                grid[iceTargetPos.line - 1, iceTargetPos.column].isStone = true;
            }
        }

        // Baixo (linha do ice > linha do player)
        else if (iceTargetPos.line > playerPos.line)
        {
            // Se não estiver no extremo de baixo
            if (iceTargetPos.line != (lines - 1))
            {
                ((Instantiate(stonePrefab, grid[iceTargetPos.line + 1, iceTargetPos.column].transform.position, Quaternion.identity)) as GameObject).GetComponent<StoneInfo>().info(iceTargetPos.line + 1, iceTargetPos.column); ;
                grid[iceTargetPos.line + 1, iceTargetPos.column].isStone = true;
            }
        }

        // Direita (coluna do ice > coluna do player)
        else if (iceTargetPos.column > playerPos.column)
        {
            // Se não estiver no extremo da direita
            if (iceTargetPos.column != (columns - 1))
            {
                ((Instantiate(stonePrefab, grid[iceTargetPos.line, iceTargetPos.column + 1].transform.position, Quaternion.identity)) as GameObject).GetComponent<StoneInfo>().info(iceTargetPos.line, iceTargetPos.column + 1); ;
                grid[iceTargetPos.line, iceTargetPos.column + 1].isStone = true;
            }
        }

        // Esquerda (coluna do ice < coluna do player)
        else if (iceTargetPos.column < playerPos.column)
        {
            // Se não estiver no extremo da esquerda
            if (iceTargetPos.column != 0)
            {
                ((Instantiate(stonePrefab, grid[iceTargetPos.line, iceTargetPos.column - 1].transform.position, Quaternion.identity)) as GameObject).GetComponent<StoneInfo>().info(iceTargetPos.line, iceTargetPos.column - 1); ;
                grid[iceTargetPos.line, iceTargetPos.column - 1].isStone = true;
            }
        }
    }
    
    // Recursividade
    void setStone(IceInfo currIcePos, IceInfo nextIcePos, IceInfo[,] matriz)
    {
        // Posição do ice em relação ao player

        // O ICE QUE IREI COLOCAR A STONE > Não pode ser uma Stone, nem estar no Extremo

        // Cima (linha do ice < linha do player)
        if (nextIcePos.line < currIcePos.line)
        {
            // Se não estiver no extremo de cima
            if (nextIcePos.line != 0)
            {
                // Coloca uma stone em cima do ice
                matriz[nextIcePos.line - 1, nextIcePos.column].isStone = true;
            }
        }

        // Baixo (linha do ice > linha do player)
        else if (nextIcePos.line > currIcePos.line)
        {
            // Se não estiver no extremo de baixo
            if (nextIcePos.line != (lines - 1))
            {
                matriz[nextIcePos.line + 1, nextIcePos.column].isStone = true;
            }
        }

        // Direita (coluna do ice > coluna do player)
        else if (nextIcePos.column > currIcePos.column)
        {
            // Se não estiver no extremo da direita
            if (nextIcePos.column != (columns - 1))
            {
                matriz[nextIcePos.line, nextIcePos.column + 1].isStone = true;
            }
        }

        // Esquerda (coluna do ice < coluna do player)
        else if (nextIcePos.column < currIcePos.column)
        {
            // Se não estiver no extremo da esquerda
            if (nextIcePos.column != 0)
            {
                matriz[nextIcePos.line, nextIcePos.column - 1].isStone = true;
            }
        }
    }

    private void fillGrid()
    {
        float x = 0;
        float y = 0;
        int id = 0;

        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                grid[i, j] = (Instantiate(icePrefab, new Vector3(transform.position.x + x, transform.position.y - y, transform.position.z), Quaternion.identity) as GameObject).GetComponent<IceInfo>();
                grid[i, j].gameObject.name = string.Format("[{0}][{1}]", i, j);
                grid[i, j].info(i, j, id, false, false);

                if (i == 0 || j == 0 || i == lines - 1 || j == columns - 1)
                {
                    grid[i, j].isCorner = true;
                    corners.Add(grid[i, j]);
                    grid[i, j].gameObject.name += " corner";
                }

                x += iceExtentsX;
                id++;
            }
            y += iceExtentsY;
            x = 0;
        }
    }

    private void centerGrid()
    {
        // Coloca o Game Grid na posição do ice central
        // Se o número de linhas for par (colunas ímpar)
        if (lines % 2 == 0)
        {
            this.transform.position = new Vector3(grid[(lines / 2), ((columns + 1) / 2) - 1].gameObject.transform.position.x, grid[(lines / 2), ((columns + 1) / 2) - 1].gameObject.transform.position.y - iceExtentsY / 2, 0.0f);
            // Debug.Log("Posição do ice central antes de setar parent " + transform.position);
            // Após centralizar o Game Grid, seta o parent (Game Grid) dos ices
            setIcesParent();
            // Debug.Log("Posição do ice central após setar o parent " + grid[(lines / 2), ((columns + 1) / 2) - 1].transform.position);
            // Não sei por que, mas precisei fazer isso quando o número de linhas fosse par
            // Resumindo, ele não centralizava corretamente, ficava com um intervalo de 0.4f. Por isso fiz a diferença entre a posição do ice central antes de setar como parent e depois de setar como parent
            float yPos = Mathf.Abs(transform.position.y - grid[(lines / 2), ((columns + 1) / 2) - 1].transform.position.y - iceExtentsY / 2);
            // Após saber quanto deu a diferença, eu seto a posição do Game Grid novamente, porém com o Y sendo essa diferença
            this.transform.position = new Vector3(0.0f, -yPos, 0.0f);
        }
        // Se for ímpar (colunas par)
        else
        {
            this.transform.position = new Vector3(grid[((lines + 1) / 2) - 1, columns / 2].gameObject.transform.position.x - iceExtentsX / 2, grid[((lines + 1) / 2) - 1, columns / 2].gameObject.transform.position.y, 0.0f);
            // Após centralizar o Game Grid, seta o parent (Game Grid) dos ices
            setIcesParent();

            this.transform.position = new Vector3(0.0f, 0.0f, 0.0f);
        }
    }

    private void setIcesParent()
    {
        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Seta como child do Game Grid
                grid[i, j].gameObject.transform.SetParent(transform);
            }
        }
    }


    // Lista contendo a sequencia de direções 
    // deletar dps
    List<Direction> seqDirec = new List<Direction>();
    private void setDirection(PlayerInfo playerCopy, IceInfo iceTarget)
    {
        // Andou na vertical
        if((playerCopy.line - iceTarget.line) != 0)
        {
            // Andou pra baixo
            if (playerCopy.line < iceTarget.line)
                currDir = Direction.AndouParaBaixo;
            // Andou para cima
            else
                currDir = Direction.AndouParaCima;

            currReta = Reta.Vertical;
        }
        // Andou na horizontal
        else
        {
            // Andou pra direita
            if (playerCopy.column < iceTarget.column)
                currDir = Direction.AndouParaDireita;
            // Andou para esquerda
            else
                currDir = Direction.AndouParaEsquerda;

            currReta = Reta.Horizontal;
        }

        seqDirec.Add(currDir);
    }

    // Recursividade
    private void setDirection(IceInfo currIcePos, IceInfo nextIcePos)
    {
        // Andou na vertical
        if ((currIcePos.line - nextIcePos.line) != 0)
        {
            // Andou pra baixo
            if (currIcePos.line < nextIcePos.line)
                currDir = Direction.AndouParaBaixo;
            // Andou para cima
            else
                currDir = Direction.AndouParaCima;

            currReta = Reta.Vertical;
        }
        // Andou na horizontal
        else
        {
            // Andou pra direita
            if (currIcePos.column < nextIcePos.column)
                currDir = Direction.AndouParaDireita;
            // Andou para esquerda
            else
                currDir = Direction.AndouParaEsquerda;

            currReta = Reta.Horizontal;
        }
    }

    // Recursividade
    private Direction setDir(IceInfo currIcePos, IceInfo nextIcePos)
    {
        Direction d;
        // Andou na vertical
        if ((currIcePos.line - nextIcePos.line) != 0)
        {
            // Andou pra baixo
            if (currIcePos.line < nextIcePos.line)
                d = Direction.AndouParaBaixo;
            // Andou para cima
            else
                d = Direction.AndouParaCima;
        }
        // Andou na horizontal
        else
        {
            // Andou pra direita
            if (currIcePos.column < nextIcePos.column)
                d = Direction.AndouParaDireita;
            // Andou para esquerda
            else
                d = Direction.AndouParaEsquerda;
        }
        return d;
    }

    // Recursividade
    private Reta setReta(IceInfo currIcePos, IceInfo nextIcePos)
    {
        Reta r;

        // Andou na vertical
        if ((currIcePos.line - nextIcePos.line) != 0)
            r = Reta.Vertical;

        // Andou na horizontal
        else
            r = Reta.Horizontal;

        return r;
    }

    // Recursividade

    // 0 : ice
    // 1 : stone
    // 2 : start
    // 3 : end
    // 4 : ices do caminho

    int maiorNumeroDeSteps = 0;

    IceInfo[,] matrizDoLevel;

    List<string> moveOptionsEmString = new List<string>();

    

    void nLevels(IceInfo currIcePos, IceInfo[,] currGrid, int currStepCount, Direction dir, Reta reta)
    {
        IceInfo[,] matriz = currGrid;
        int stepCount = currStepCount;
        Direction rcurrDir = dir;
        Reta rcurrReta = reta;

        setMoveOptions(currIcePos, dir, reta, matriz);

        for (int i = 0; i < moveOptionsEmString.Count; i++)
        {
            IceInfo nextIce = moveOptions[i];
            rcurrDir = setDir(currIcePos, nextIce);
            rcurrReta = setReta(currIcePos, nextIce);
            setStone(currIcePos, nextIce, matriz);
            setIcesPassedRecursividade(currIcePos, nextIce, matriz);
            stepCount++;
            moveOptions.Clear();
            if (nextIce.isEnd)
            {
                if (stepCount > maiorNumeroDeSteps)
                {
                    setMatriz(matriz);
                    Debug.Log("Setou a matriz");
                }
            }
            else
                nLevels(nextIce, matriz, stepCount, rcurrDir, rcurrReta);
        }
    }

    void setMatriz(IceInfo[,] matriz)
    {
        for (int i = 0; i < lines; i++)
            for (int j = 0; j < columns; j++)
            {
                matrizDoLevel[i, j] = matriz[i, j];
            }
    }

    void atualizaGridComMatrizDoLevel()
    {
        for (int i = 0; i < lines; i++)
            for (int j = 0; j < columns; j++)
            {
                if (matrizDoLevel[i, j].isStone)
                    Instantiate(stonePrefab, matrizDoLevel[i, j].transform.position, Quaternion.identity);
            }
    }

    void randomStartEnd()
    {
        // Ice que vai ser o Start
        startIce = grid[Random.Range(0, lines), Random.Range(0, columns)];

        // Setando que esse Ice é o Start
        startIce.isStart = true;

        // Instanciando o Start nesse Ice
        GameObject start = (Instantiate(startPrefab, startIce.transform.position, Quaternion.identity)) as GameObject;
        start.transform.SetParent(transform);
        start.name = "Start";
        start.GetComponent<StartEndInfo>().info(startIce.line, startIce.column);

        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");

        // Reta do start
        for (int i = 0; i < icesInScene.Length; i++)
        {
            if (icesInScene[i].GetComponent<IceInfo>().line == startIce.line || icesInScene[i].GetComponent<IceInfo>().column == startIce.column)
            {
                icesInScene[i].GetComponent<IceInfo>().isRetaStart = true;
            }
        }

        // Pegando todos ices menos o ice que está o Start para instanciar o End
        List<IceInfo> temp = new List<IceInfo>();
        for (int i = 0; i < icesInScene.Length; i++)
        {
            if (!icesInScene[i].GetComponent<IceInfo>().isStart && !icesInScene[i].GetComponent<IceInfo>().isRetaStart)
            {
                temp.Add(icesInScene[i].GetComponent<IceInfo>());
            }
        }

        IceInfo endRandom = temp[Random.Range(0, temp.Count)];

        // Instanciando End
        end = Instantiate(endPrefab, endRandom.gameObject.transform.position, Quaternion.identity) as GameObject;
        end.name = "End";
        // Setando ice que está o End
        grid[endRandom.line, endRandom.column].isEnd = true;
        // Setando linha e coluna que o End está
        end.GetComponent<StartEndInfo>().info(endRandom.line, endRandom.column);

        Debug.Log("Start: [" + startIce.line + "][" + startIce.column + "]");
        Debug.Log("End: [" + endRandom.line + "][" + endRandom.column + "]");
    }










    // Toggles jump e break
    // deletar dps que fizer uma maneira melhor
    public GameObject jumpTog, breakTog;
    public void inverte()
    {
        if (jumpTog.GetComponent<Toggle>().isOn)
        {
            breakTog.GetComponent<Toggle>().isOn = false;
        }
        else
        {
            breakTog.GetComponent<Toggle>().isOn = true;
        }
    }
    public void inverte2()
    {
        if (breakTog.GetComponent<Toggle>().isOn)
        {
            jumpTog.GetComponent<Toggle>().isOn = false;
        }
        else
        {
            jumpTog.GetComponent<Toggle>().isOn = true;
        }
    }

}
