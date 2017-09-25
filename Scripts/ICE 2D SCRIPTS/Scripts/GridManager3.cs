using UnityEngine;
using System.Collections.Generic;

public class GridManager3 : MonoBehaviour
{

    public int lines, columns;
    public int minLines;
    public int maxLines;

    public GameObject icePrefab, playerPrefab, stonePrefab, startPrefab, endPrefab, plantPrefab, stonePrefab2;

    [HideInInspector]
    public GameObject start, end, player;

    [HideInInspector]
    public IceInfo[,] grid;

    // Cima, Baixo, Esquerda, Direita
    private string currentDirection;
    // Horizontal e Vertical
    private string lastDirection;

    // Mínimo de 3 steps
    public int levelSteps;
    public int levelItems;

    private int stepsCount;

    private List<IceInfo> moveOptions = new List<IceInfo>();

    private List<IceInfo> corners = new List<IceInfo>();

    private float iceExtentsX, iceExtentsY;

    // Medidas da tela
    Vector3 upperCorner, target;

    // Gui text pro ice
    public GameObject iceTextPrefab;

    void Awake()
    {
        stepsCount = 0;

        // Apenas Game Grids ímpares, para poder centralizar perfeitamente.
        //int[] linesAllowed = { 5, 7, 9, 11 };
        //lines = linesAllowed[Random.Range(0, linesAllowed.Length)];
        lines = Random.Range(minLines, maxLines);
        columns = lines + 3;
        levelSteps = Random.Range(4, lines - 1);


        // Medidas do ice para o espaçamento entre eles (*2 porque o extents pega sempre a metade)
        iceExtentsX = icePrefab.GetComponent<SpriteRenderer>().bounds.extents.x * 2;
        iceExtentsY = icePrefab.GetComponent<SpriteRenderer>().bounds.extents.y * 2;

        // Incializa o array do grid
        grid = new IceInfo[lines, columns];

        // Preenche o grid com os ices
        fillGrid();

        // Centraliza o grid
        centerGrid();

        // Seta o level
        setLevel2();

        player.name = "Player";

        // Seta o Player, Start e End como children do Game Grid
        player.transform.SetParent(transform);
        start.transform.SetParent(transform);
        if(end != null)
        end.transform.SetParent(transform);

        // Centraliza o Game Grid
        //transform.position = new Vector3(0.0f, 0.0f, 0.0f);

       // setGuiTexts();
       //setGuisPassed();

        icesNotPassed();

        // Limpo o bool .passed de todos ices
        GameObject[] ices = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < ices.Length; i++)
        {
            ices[i].GetComponent<IceInfo>().passed = false;
        }


        // ADICIONO O ICE QUE O PLAYER ESTÁ A LISTA DE ICES QUE O PLAYER JA ESTEVE
        LevelInfo.addIce(grid[player.GetComponent<PlayerInfo>().line, player.GetComponent<PlayerInfo>().column]);
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

    private void fillGrid2()
    {
        float x = 0;
        float y = 0;
        int id = 0;

        int metadeColumns = columns / 2;
        int metadeLines = lines / 2;

        // Começa instanciando da esquerda de cima
        float startX = (- iceExtentsX * columns / 2) + (- target.x);
        float startY = (+ iceExtentsY * lines / 2)   + (+ target.y);



        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                grid[i, j] = (Instantiate(icePrefab, new Vector3(startX + x, transform.position.y - y, transform.position.z), Quaternion.identity) as GameObject).GetComponent<IceInfo>();
                grid[i, j].gameObject.name = string.Format("[{0}][{1}]", i, j);
                grid[i, j].info(i, j, id, false, false);

                // Seta como child do Game Grid
                grid[i, j].gameObject.transform.SetParent(transform);

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

    private void randomStartEndPosition()
    {
        ///// Start
        // Randomizando ice do Start
        IceInfo randomIceStart = grid[Random.Range(0, lines), Random.Range(0, columns)];
        // Instanciando Start
        start = Instantiate(startPrefab, randomIceStart.transform.position, Quaternion.identity) as GameObject;
        start.name = "Start";
        // Setando linha e coluna que o Start está
        start.GetComponent<StartEndInfo>().info(randomIceStart.line, randomIceStart.column);
        // Setando ices que ficaram na reta do Start
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesInScene.Length; i++)
        {
            icesInScene[i].GetComponent<IceInfo>().setIsRetaStartEnd();
        }
        // Desabilitando ice que está na posição do Start
        grid[start.GetComponent<StartEndInfo>().line, start.GetComponent<StartEndInfo>().column].GetComponent<SpriteRenderer>().enabled = false;
        grid[start.GetComponent<StartEndInfo>().line, start.GetComponent<StartEndInfo>().column].isRetaStart = false;

        ///// End
        // Preenchendo lista do moveOptions pra randomizar um ice que não esteja na reta do Start
        for (int i = 0; i < lines; i++)
            for (int j = 0; j < columns; j++)
                if (!grid[i, j].isRetaStart)
                    moveOptions.Add(grid[i, j]);
        // Randomizando um ice pro End
        IceInfo randomiceEnd = moveOptions[Random.Range(0, moveOptions.Count)];
        // Verificando se o End não foi instanciado em um dos extremos (se isso acontecer, pode ser que o player só precise de 2 movimentos pra terminar o level)
        while (randomiceEnd.line == 0 || randomiceEnd.line == lines - 1 ||
               randomiceEnd.column == 0 || randomiceEnd.column == columns - 1)
        {
            randomiceEnd = moveOptions[Random.Range(0, moveOptions.Count)];
        }
        // Instanciando End
        end = Instantiate(endPrefab, randomiceEnd.transform.position, Quaternion.identity) as GameObject;
        end.name = "End";
        // Setando linha e coluna que o End está
        end.GetComponent<StartEndInfo>().info(randomiceEnd.line, randomiceEnd.column);
        // Setando ices que ficaram na reta do End
        for (int i = 0; i < icesInScene.Length; i++)
        {
            icesInScene[i].GetComponent<IceInfo>().setIsRetaStartEnd();
        }
        // Desabilitando ice que está na posição do End
        grid[end.GetComponent<StartEndInfo>().line, end.GetComponent<StartEndInfo>().column].GetComponent<SpriteRenderer>().enabled = false;


        // Limpa o moveOptions
        moveOptions.Clear();
    }

    private void setLevel()
    {
        // Começa como vazio
        lastDirection = "";

        GameObject playerCopy = Instantiate(player, player.transform.position, Quaternion.identity) as GameObject;
        PlayerInfo playerCopyInfo = playerCopy.GetComponent<PlayerInfo>();

        Debug.Log("Start: O Player começou na posição [" + playerCopyInfo.line + "][" + playerCopyInfo.column + "]");

        // Percorre até o antepenúltimo step
        for (int i = 0; i < levelSteps - 2; i++)
        {
            // Preencho a lista dos ices que pode se mover
            setMoveOptionsWithoutItens(playerCopyInfo, lastDirection);

            // Randomizo um ice da lista
            IceInfo randomIceTarget = moveOptions[Random.Range(0, moveOptions.Count)];

            // Verifico se esse ice não está na reta do End ou Start e se já não passei por ele
            while (randomIceTarget.isRetaEnd)
            {
                randomIceTarget = moveOptions[Random.Range(0, moveOptions.Count)];
            }

            // Seto a variável "passed" dos ices que passei
            setIcesPassed(playerCopyInfo, randomIceTarget);

            // Seto a direção
            if (randomIceTarget.line < playerCopyInfo.line)
            {
                currentDirection = "Cima";
                lastDirection = "Vertical";
            }
            else if (randomIceTarget.line > playerCopyInfo.line)
            {
                currentDirection = "Baixo";
                lastDirection = "Vertical";
            }
            else if (randomIceTarget.column < playerCopyInfo.column)
            {
                currentDirection = "Esquerda";
                lastDirection = "Horizontal";
            }
            else if (randomIceTarget.column > playerCopyInfo.column)
            {
                currentDirection = "Direita";
                lastDirection = "Horizontal";
            }

            Debug.Log("Current direction: " + currentDirection + " Target: " + randomIceTarget.name);

            // Crio a pedra ao lado do ice dependendo da direção
            // Primeiro verifico se o player não está em um ice extremo (das pontas)
            // OBS.: PRECISO VERIFICAR A DIREÇÃO !!
            if ((currentDirection.Equals("Cima") && randomIceTarget.line != 0) ||
               (currentDirection.Equals("Baixo") && randomIceTarget.line != lines - 1) ||
               (currentDirection.Equals("Esquerda") && randomIceTarget.column != 0) ||
               (currentDirection.Equals("Direita") && randomIceTarget.column != columns - 1))
            {
                // Verifico a direção que ele estava e coloco uma pedra no ice ao lado ou acima/abaixo
                if (currentDirection.Equals("Cima"))
                {
                    // Verifico se já passei pelo local onde vou colocar a pedra
                    if (grid[randomIceTarget.line - 1, randomIceTarget.column].passed)
                    {
                        // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                        levelItems++;
                    }

                    // Verifico se já tem uma pedra no local, se não tiver eu coloco
                    if (!grid[randomIceTarget.line - 1, randomIceTarget.column].isStone)
                    {
                        // Posição da pedra
                        Vector3 stonePosition = grid[randomIceTarget.line - 1, randomIceTarget.column].gameObject.transform.position;
                        // Instancio a pedra
                        GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                        // Seto a linha e coluna que a stona foi colocada
                        stone.GetComponent<StoneInfo>().info(randomIceTarget.line - 1, randomIceTarget.column);
                        // Seto o ice como uma pedra
                        grid[randomIceTarget.line - 1, randomIceTarget.column].isStone = true;

                        Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Cimado ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
                    }

                }
                else if (currentDirection.Equals("Baixo"))
                {
                    // Verifico se já passei pelo local onde vou colocar a pedra
                    if (grid[randomIceTarget.line + 1, randomIceTarget.column].passed)
                    {
                        // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                        levelItems++;
                    }

                    // Verifico se já tem uma pedra no local, se não tiver eu coloco
                    if (!grid[randomIceTarget.line + 1, randomIceTarget.column].isStone)
                    {
                        // Posição da pedra
                        Vector3 stonePosition = grid[randomIceTarget.line + 1, randomIceTarget.column].gameObject.transform.position;
                        // Instancio a pedra
                        GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                        // Seto a linha e coluna que a stona foi colocada
                        stone.GetComponent<StoneInfo>().info(randomIceTarget.line + 1, randomIceTarget.column);
                        // Seto o ice como uma pedra
                        grid[randomIceTarget.line + 1, randomIceTarget.column].isStone = true;

                        Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Baixo do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
                    }
                }
                else if (currentDirection.Equals("Esquerda"))
                {
                    // Verifico se já passei pelo local onde vou colocar a pedra
                    if (grid[randomIceTarget.line, randomIceTarget.column - 1].passed)
                    {
                        // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                        levelItems++;
                    }

                    // Verifico se já tem uma pedra no local, se não tiver eu coloco
                    if (!grid[randomIceTarget.line, randomIceTarget.column - 1].isStone)
                    {
                        // Posição da pedra
                        Vector3 stonePosition = grid[randomIceTarget.line, randomIceTarget.column - 1].gameObject.transform.position;
                        // Instancio a pedra
                        GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                        // Seto a linha e coluna que a stona foi colocada
                        stone.GetComponent<StoneInfo>().info(randomIceTarget.line, randomIceTarget.column - 1);
                        // Seto o ice como uma pedra
                        grid[randomIceTarget.line, randomIceTarget.column - 1].isStone = true;

                        Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Esquerda do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
                    }
                }
                else if (currentDirection.Equals("Direita"))
                {
                    // Verifico se já passei pelo local onde vou colocar a pedra
                    if (grid[randomIceTarget.line, randomIceTarget.column + 1].passed)
                    {
                        // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                        levelItems++;
                    }

                    // Verifico se já tem uma pedra no local, se não tiver eu coloco
                    if (!grid[randomIceTarget.line, randomIceTarget.column + 1].isStone)
                    {
                        // Posição da pedra
                        Vector3 stonePosition = grid[randomIceTarget.line, randomIceTarget.column + 1].gameObject.transform.position;
                        // Instancio a pedra
                        GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                        // Seto a linha e coluna que a stona foi colocada
                        stone.GetComponent<StoneInfo>().info(randomIceTarget.line, randomIceTarget.column + 1);
                        // Seto o ice como uma pedra
                        grid[randomIceTarget.line, randomIceTarget.column + 1].isStone = true;

                        Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Direita do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
                    }
                }
            }

            // Atualizo a linha e coluna que o playerCopy está
            playerCopyInfo.info(randomIceTarget.line, randomIceTarget.column);

            // Limpo a lista de move options
            moveOptions.Clear();
        }



        // Penúltimo step é pra ir até a reta do End 
        // Preencho a lista dos ices que pode se mover
        setMoveOptionsWithoutItens(playerCopyInfo, lastDirection);
        // Procuro o ice que esteja na reta do End
        IceInfo iceNaRetaDoEnd = moveOptions[0];
        while (!iceNaRetaDoEnd.isRetaEnd)
        {
            iceNaRetaDoEnd = moveOptions[Random.Range(0, moveOptions.Count)];
        }

        // Seto a variável "passed" dos ices que passei
        setIcesPassed(playerCopyInfo, iceNaRetaDoEnd);

        // Seto a direção
        if (iceNaRetaDoEnd.line < playerCopyInfo.line)
        {
            currentDirection = "Cima";
            lastDirection = "Vertical";
        }
        else if (iceNaRetaDoEnd.line > playerCopyInfo.line)
        {
            currentDirection = "Baixo";
            lastDirection = "Vertical";
        }
        else if (iceNaRetaDoEnd.column < playerCopyInfo.column)
        {
            currentDirection = "Esquerda";
            lastDirection = "Vertical";
        }
        else if (iceNaRetaDoEnd.column > playerCopyInfo.column)
        {
            currentDirection = "Direita";
            lastDirection = "Horizontal";
        }

        Debug.Log("Current direction: " + currentDirection + " Target: " + iceNaRetaDoEnd.name);

        // Crio a pedra ao lado do ice dependendo da direção
        // Primeiro verifico se o player não está em um ice extremo (das pontas)
        // OBS.: PRECISO VERIFICAR A DIREÇÃO !!
        if ((currentDirection.Equals("Cima") && iceNaRetaDoEnd.line != 0) ||
           (currentDirection.Equals("Baixo") && iceNaRetaDoEnd.line != lines - 1) ||
           (currentDirection.Equals("Esquerda") && iceNaRetaDoEnd.column != 0) ||
           (currentDirection.Equals("Direita") && iceNaRetaDoEnd.column != columns - 1))
        {
            // Verifico a direção que ele estava e coloco uma pedra no ice ao lado ou acima/abaixo
            if (currentDirection.Equals("Cima"))
            {
                // Verifico se já passei pelo local onde vou colocar a pedra
                if (grid[iceNaRetaDoEnd.line - 1, iceNaRetaDoEnd.column].passed)
                {
                    // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                    levelItems++;
                }

                // Verifico se já tem uma pedra no local, se não tiver eu coloco
                if (!grid[iceNaRetaDoEnd.line - 1, iceNaRetaDoEnd.column].isStone)
                {
                    // Posição da pedra
                    Vector3 stonePosition = grid[iceNaRetaDoEnd.line - 1, iceNaRetaDoEnd.column].gameObject.transform.position;
                    // Instancio a pedra
                    GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                    // Seto a linha e coluna que a stona foi colocada
                    stone.GetComponent<StoneInfo>().info(iceNaRetaDoEnd.line - 1, iceNaRetaDoEnd.column);
                    // Seto o ice como uma pedra
                    grid[iceNaRetaDoEnd.line - 1, iceNaRetaDoEnd.column].isStone = true;

                    Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Cima do ice: " + grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column].name);
                }
            }
            else if (currentDirection.Equals("Baixo"))
            {
                // Verifico se já passei pelo local onde vou colocar a pedra
                if (grid[iceNaRetaDoEnd.line + 1, iceNaRetaDoEnd.column].passed)
                {
                    // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                    levelItems++;
                }

                // Verifico se já tem uma pedra no local, se não tiver eu coloco
                if (!grid[iceNaRetaDoEnd.line + 1, iceNaRetaDoEnd.column].isStone)
                {
                    // Posição da pedra
                    Vector3 stonePosition = grid[iceNaRetaDoEnd.line + 1, iceNaRetaDoEnd.column].gameObject.transform.position;
                    // Instancio a pedra
                    GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                    // Seto a linha e coluna que a stona foi colocada
                    stone.GetComponent<StoneInfo>().info(iceNaRetaDoEnd.line + 1, iceNaRetaDoEnd.column);
                    // Seto o ice como uma pedra
                    grid[iceNaRetaDoEnd.line + 1, iceNaRetaDoEnd.column].isStone = true;

                    Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Baixo do ice: " + grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column].name);
                }
            }
            else if (currentDirection.Equals("Esquerda"))
            {
                // Verifico se já passei pelo local onde vou colocar a pedra
                if (grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column - 1].passed)
                {
                    // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                    levelItems++;
                }

                // Verifico se já tem uma pedra no local, se não tiver eu coloco
                if (!grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column - 1].isStone)
                {
                    // Posição da pedra
                    Vector3 stonePosition = grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column - 1].gameObject.transform.position;
                    // Instancio a pedra
                    GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                    // Seto a linha e coluna que a stona foi colocada
                    stone.GetComponent<StoneInfo>().info(iceNaRetaDoEnd.line, iceNaRetaDoEnd.column - 1);
                    // Seto o ice como uma pedra
                    grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column - 1].isStone = true;

                    Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Esquerda do ice: " + grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column].name);
                }
            }
            else if (currentDirection.Equals("Direita"))
            {
                // Verifico se já passei pelo local onde vou colocar a pedra
                if (grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column + 1].passed)
                {
                    // Se vou colocar um pedra em um local que passei, precisarei do item pra destruir
                    levelItems++;
                }

                // Verifico se já tem uma pedra no local, se não tiver eu coloco
                if (!grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column + 1].isStone)
                {
                    // Posição da pedra
                    Vector3 stonePosition = grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column + 1].gameObject.transform.position;
                    // Instancio a pedra
                    GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                    // Seto a linha e coluna que a stona foi colocada
                    stone.GetComponent<StoneInfo>().info(iceNaRetaDoEnd.line, iceNaRetaDoEnd.column + 1);
                    // Seto o ice como uma pedra
                    grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column + 1].isStone = true;

                    Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Direita do ice: " + grid[iceNaRetaDoEnd.line, iceNaRetaDoEnd.column].name);
                }
            }
        }

        // Atualizo a linha e coluna que o playerCopy está
        playerCopyInfo.info(iceNaRetaDoEnd.line, iceNaRetaDoEnd.column);

        // Limpo a lista de move options
        moveOptions.Clear();

        playerCopy.transform.position = end.transform.position;

        // Último step é pra ir até o End
        // Preencho a lista dos ices que pode se mover
        //setMoveOptions(playerCopy);


    }

    // Correto
    private void setLevel2()
    {
        ///// Start
        // Randomizando ice do Start
        IceInfo randomIceStart = grid[Random.Range(0, lines), Random.Range(0, columns)];
        // Setando ice que está o Start
        randomIceStart.isStart = true;
        // Instanciando Start
        start = Instantiate(startPrefab, randomIceStart.transform.position, Quaternion.identity) as GameObject;
        start.name = "Start";
        // Setando linha e coluna que o Start está
        start.GetComponent<StartEndInfo>().info(randomIceStart.line, randomIceStart.column);
        // Setando ices que ficaram na reta do Start
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesInScene.Length; i++)
        {
            icesInScene[i].GetComponent<IceInfo>().setIsRetaStartEnd();
        }

        // Instancia Player na posição do Start
        player = Instantiate(playerPrefab, start.transform.position, Quaternion.identity) as GameObject;
        // Seta linha e coluna que o player está
        player.GetComponent<PlayerInfo>().info(start.GetComponent<StartEndInfo>().line, start.GetComponent<StartEndInfo>().column);

        // Começa como vazio
        lastDirection = "";

        GameObject playerCopy = Instantiate(player, player.transform.position, Quaternion.identity) as GameObject;
        PlayerInfo playerCopyInfo = playerCopy.GetComponent<PlayerInfo>();

        Debug.Log("Start: O Player começou na posição [" + playerCopyInfo.line + "][" + playerCopyInfo.column + "]");

        // Steps
        for (int i = 0; i < levelSteps - 1; i++)
        {
            // Preencho a lista dos ices que pode se mover
            setMoveOptionsWithoutItens(playerCopyInfo, lastDirection);

            if (moveOptions.Count == 0)
            {
                Debug.Log("Move options ZERO !");

                Application.LoadLevel(Application.loadedLevel);
            }
            else
            {
                // Randomizo um ice da lista
                IceInfo randomIceTarget = moveOptions[Random.Range(0, moveOptions.Count)];

                // TESTEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEEE
                randomIceTarget.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                // Seto a direção
                setDirection(randomIceTarget, playerCopyInfo);

                // Seto a variável "passed" dos ices que passei
                setIcesPassed(playerCopyInfo, randomIceTarget);

                // Seto a stone
                setStones(randomIceTarget);

                // Atualizo a linha e coluna que o playerCopy está
                playerCopyInfo.info(randomIceTarget.line, randomIceTarget.column);

                // Limpo a lista de move options
                moveOptions.Clear();
            }
        }

        // Preencho a lista dos ices que pode se mover
        setMoveOptionsWithoutItens(playerCopyInfo, lastDirection);

        // Último step:
        // Preciso remover os ices que estão na reta do Start e nos Extremos
        // Neste Step que seto a posição do End
        for (int i = 0; i < moveOptions.Count; i++)
        {
            if (moveOptions[i].isRetaStart)
            {
                Debug.Log("Removeu o da reta start: " + moveOptions[i].name);
                moveOptions.Remove(moveOptions[i]);
            }
        }
        for (int i = 0; i < moveOptions.Count; i++)
        {
            if (moveOptions[i].line == 0 || moveOptions[i].line == (lines - 1) ||
                moveOptions[i].column == 0 || moveOptions[i].column == (columns - 1))
            {
                Debug.Log("Removeu o extremo: " + moveOptions[i].name);
                moveOptions.Remove(moveOptions[i]);
            }
        }


        // Se depois de remover os ices, não sobrar nenhum, coloca o End na última posição que o player estava
        if (moveOptions.Count == 0)
        {
            Debug.Log("Move options ZERO !! {2}. Count do moveOptions: " + moveOptions.Count);

            Application.LoadLevel(Application.loadedLevel);
        }
        // Se não eu randomizo um ice da lista de opções
        else
        {
            // Randomizo um ice da lista
            IceInfo randomIceTarget = moveOptions[Random.Range(0, moveOptions.Count)];

            // Seto a direção
            setDirection(randomIceTarget, playerCopyInfo);

            // Seto a variável "passed" dos ices que passei
            setIcesPassed(playerCopyInfo, randomIceTarget);

            // Seto a stone
            setStones(randomIceTarget);

            // Atualizo a linha e coluna que o playerCopy está
            playerCopyInfo.info(randomIceTarget.line, randomIceTarget.column);

            // Limpo a lista de move options
            moveOptions.Clear();

            // Instanciando End
            end = Instantiate(endPrefab, grid[playerCopyInfo.line, playerCopyInfo.column].gameObject.transform.position, Quaternion.identity) as GameObject;
            end.name = "End";
            // Setando ice que está o End
            grid[playerCopyInfo.line, playerCopyInfo.column].isEnd = true;
            // Setando linha e coluna que o End está
            end.GetComponent<StartEndInfo>().info(playerCopyInfo.line, playerCopyInfo.column);
            // Setando ices que ficaram na reta do End
            for (int i = 0; i < icesInScene.Length; i++)
            {
                icesInScene[i].GetComponent<IceInfo>().setIsRetaStartEnd();
            }
        }

        Destroy(playerCopy.gameObject);
    }

    // Tentando simplificar o código e tirar alguns bugs
    private void setLevel3()
    {
        // Start position
        setStartPos();

        // Setando ices que ficaram na reta do Start
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");
        for (int i = 0; i < icesInScene.Length; i++)
        {
            icesInScene[i].GetComponent<IceInfo>().setIsRetaStartEnd();
        }

        // Player position
        setPlayerPos();



        // Começa como vazio
        lastDirection = "";




        // Randomizo um start aleatorio

        // Crio uma lista contendo os todos ices DISPONIVEIS e uma lista contendo os todos ices PERMITIDOS (que estão na reta do player)

        // Preencho a lista com ices DISPONIVEIS e PERMITIDOS para se mover

        // Randomizo o um ice da lista dos PERMITIDOS

        // Retiro da lista dos DISPONIVEIS os ices em volta dos que o player passou
        // Obs.: aqui posso fazer uma variação e criar o item de destruir stones. Sendo assim, eu permitiria a criação de stones nos ices
        //       que o player já passou



        // Ideia:
        // criação de 3 itens
        // 1) destruir a stone
        // 2) pular a stone
        // 3) empurrar a stone

        // Explicando como seria o item 3)
        // player, ice, ice, stone,  ice que o player precisa chegar, ice, stone
        // movo o player até a stone que quero empurrar: ice, ice, player, stone,  ice que o player precisa chegar, ice, stone
        // empurro a stone: ice, ice, player, ice,  ice que o player precisa chegar, stone, stone
        // movo o player ao ice que precisa chegar

        // Para decidir onde entrarão as stones que vão precisar ser empurradas:
        // Quando eu for instanciar uma stone, instancio 1 casa a mais e instancio a stone que vai ser empurrada em um dos ices que o player vai passar


    }

    // Level com itens
    private void setLevelWithItens()
    {

    }

    // Método pro setLevel3 tentando simplificar
    void setStartPos()
    {
        // Randomizando ice do Start
        IceInfo randomIceStart = grid[Random.Range(0, lines), Random.Range(0, columns)];
        // Setando ice que está o Start
        randomIceStart.isStart = true;
        // Instanciando Start
        start = Instantiate(startPrefab, randomIceStart.transform.position, Quaternion.identity) as GameObject;
        start.name = "Start";
        // Setando linha e coluna que o Start está
        start.GetComponent<StartEndInfo>().info(randomIceStart.line, randomIceStart.column);
    }

    // Método pro setLevel3 tentando simplificar
    void setPlayerPos()
    {
        // Instancia Player na posição do Start
        player = Instantiate(playerPrefab, start.transform.position, Quaternion.identity) as GameObject;
        // Seta linha e coluna que o player está
        player.GetComponent<PlayerInfo>().info(start.GetComponent<StartEndInfo>().line, start.GetComponent<StartEndInfo>().column);
    }

    void setStones(IceInfo randomIceTarget)
    {
        // Crio a pedra ao lado do ice dependendo da direção
        // Primeiro verifico se o player não está em um ice extremo (das pontas)
        // OBS.: PRECISO VERIFICAR A DIREÇÃO !!
        if ((currentDirection.Equals("Cima") && randomIceTarget.line != 0) ||
           (currentDirection.Equals("Baixo") && randomIceTarget.line != lines - 1) ||
           (currentDirection.Equals("Esquerda") && randomIceTarget.column != 0) ||
           (currentDirection.Equals("Direita") && randomIceTarget.column != columns - 1))
        {
            // Verifico a direção que ele estava e coloco uma pedra no ice ao lado ou acima/abaixo
            if (currentDirection.Equals("Cima"))
            {
                // Posição da pedra
                Vector3 stonePosition = grid[randomIceTarget.line - 1, randomIceTarget.column].gameObject.transform.position;
                // Instancio a pedra
                GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                // Seto a linha e coluna que a stona foi colocada
                stone.GetComponent<StoneInfo>().info(randomIceTarget.line - 1, randomIceTarget.column);
                // Seto o ice como uma pedra
                grid[randomIceTarget.line - 1, randomIceTarget.column].isStone = true;
                // Seta como Stone
                grid[randomIceTarget.line - 1, randomIceTarget.column].isStone = true;

                // Seta como child
                stone.transform.SetParent(transform);

//                Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Cima do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
            }
            else if (currentDirection.Equals("Baixo"))
            {
                // Posição da pedra
                Vector3 stonePosition = grid[randomIceTarget.line + 1, randomIceTarget.column].gameObject.transform.position;
                // Instancio a pedra
                GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                // Seto a linha e coluna que a stona foi colocada
                stone.GetComponent<StoneInfo>().info(randomIceTarget.line + 1, randomIceTarget.column);
                // Seto o ice como uma pedra
                grid[randomIceTarget.line + 1, randomIceTarget.column].isStone = true;
                // Seta como Stone
                grid[randomIceTarget.line + 1, randomIceTarget.column].isStone = true;

                stone.transform.SetParent(transform);

                //                Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Baixo do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
            }
            else if (currentDirection.Equals("Esquerda"))
            {
                // Posição da pedra
                Vector3 stonePosition = grid[randomIceTarget.line, randomIceTarget.column - 1].gameObject.transform.position;
                // Instancio a pedra
                GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                // Seto a linha e coluna que a stona foi colocada
                stone.GetComponent<StoneInfo>().info(randomIceTarget.line, randomIceTarget.column - 1);
                // Seto o ice como uma pedra
                grid[randomIceTarget.line, randomIceTarget.column - 1].isStone = true;
                // Seta como Stone
                grid[randomIceTarget.line, randomIceTarget.column - 1].isStone = true;

                stone.transform.SetParent(transform);

                //                Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Esquerda do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
            }
            else if (currentDirection.Equals("Direita"))
            {
                // Posição da pedra
                Vector3 stonePosition = grid[randomIceTarget.line, randomIceTarget.column + 1].gameObject.transform.position;
                // Instancio a pedra
                GameObject stone = Instantiate(stonePrefab, stonePosition, Quaternion.identity) as GameObject;
                // Seto a linha e coluna que a stona foi colocada
                stone.GetComponent<StoneInfo>().info(randomIceTarget.line, randomIceTarget.column + 1);
                // Seto o ice como uma pedra
                grid[randomIceTarget.line, randomIceTarget.column + 1].isStone = true;
                // Seta como Stone
                grid[randomIceTarget.line, randomIceTarget.column + 1].isStone = true;

                stone.transform.SetParent(transform);

                //                Debug.Log("Pedra colocada em: [" + stone.GetComponent<StoneInfo>().line + "][" + stone.GetComponent<StoneInfo>().column + "], à Direita do ice: " + grid[randomIceTarget.line, randomIceTarget.column].name);
            }
        }
    }

    // Passa o ice randomizado e a posição atual do player, então define a direção
    void setDirection(IceInfo iceTarget, PlayerInfo playerCopyInfo)
    {
        // Seto a direção
        if (iceTarget.line < playerCopyInfo.line)
        {
            currentDirection = "Cima";
            lastDirection = "Vertical";
        }
        else if (iceTarget.line > playerCopyInfo.line)
        {
            currentDirection = "Baixo";
            lastDirection = "Vertical";
        }
        else if (iceTarget.column < playerCopyInfo.column)
        {
            currentDirection = "Esquerda";
            lastDirection = "Horizontal";
        }
        else if (iceTarget.column > playerCopyInfo.column)
        {
            currentDirection = "Direita";
            lastDirection = "Horizontal";
        }

        Debug.Log("Current direction: " + currentDirection + ". Target: " + iceTarget.name);
    }

    void setIcesPassed(PlayerInfo playerCopyInfo, IceInfo iceTargetInfo)
    {
        if (playerCopyInfo.line > iceTargetInfo.line)
        {
            for (int i = playerCopyInfo.line; i >= iceTargetInfo.line; i--)
            {
                grid[i, playerCopyInfo.column].passed = true;
                grid[i, playerCopyInfo.column].stepOrder = stepsCount;
                stepsCount++;
            }
        }
        if (playerCopyInfo.line < iceTargetInfo.line)
        {
            for (int i = playerCopyInfo.line; i <= iceTargetInfo.line; i++)
            {
                grid[i, playerCopyInfo.column].passed = true;
                grid[i, playerCopyInfo.column].stepOrder = stepsCount;
                stepsCount++;
            }
        }

        if (playerCopyInfo.column > iceTargetInfo.column)
        {
            for (int i = playerCopyInfo.column; i >= iceTargetInfo.column; i--)
            {
                grid[playerCopyInfo.line, i].passed = true;
                grid[playerCopyInfo.line, i].stepOrder = stepsCount;
                stepsCount++;
            }
        }
        if (playerCopyInfo.column < iceTargetInfo.column)
        {
            for (int i = playerCopyInfo.column; i <= iceTargetInfo.column; i++)
            {
                grid[playerCopyInfo.line, i].passed = true;
                grid[playerCopyInfo.line, i].stepOrder = stepsCount;
                stepsCount++;
            }
        }
    }

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
                        GameObject stone = Instantiate(stonePrefab, icesInScene[i].transform.position, Quaternion.identity) as GameObject;

                        grid[icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column].isStone = true;

                        stone.transform.SetParent(transform);
                    }

                    //Instantiate(stonePrefab, icesInScene[i].transform.position, Quaternion.identity);

                    //grid[icesInScene[i].GetComponent<IceInfo>().line, icesInScene[i].GetComponent<IceInfo>().column].isStone = true;
                }
            }
        }
    }

    // MoveOptions para Level sem usar itens
    void setMoveOptionsWithoutItens(PlayerInfo playerCopyInfo, string lastDirection)
    {
        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Se o último movimento tiver sido na Horizontal, o próximo movimento deverá ser na Vertical. Logo, seleciono os ices da vertical
                if (lastDirection.Equals("Horizontal"))
                {
                    // Pego os que estão na mesma coluna
                    if (grid[i, j].column == playerCopyInfo.column)
                    {
                        moveOptions.Add(grid[i, j]);
                    }
                }
                // Se o último movimento tiver sido na Vertical, o próximo movimento deverá ser na Horizontal. Logo, seleciono os ices da horizontal
                else if (lastDirection.Equals("Vertical"))
                {
                    // Pego os que estão na mesma linha
                    if (grid[i, j].line == playerCopyInfo.line)
                    {
                        moveOptions.Add(grid[i, j]);
                    }
                }
                // Se for o primeiro passo, tanto faz. Pego os da linha e coluna
                else
                {
                    if (grid[i, j].line == playerCopyInfo.line || grid[i, j].column == playerCopyInfo.column)
                    {
                        moveOptions.Add(grid[i, j]);
                    }
                }
            }
        }

        StartEndInfo startInfo = start.GetComponent<StartEndInfo>();
        // Remove da lista os 4 ices em volta do Start.
        // Esquerda do Start
        if (startInfo.column != 0)
        {
            if (moveOptions.Contains(grid[startInfo.line, startInfo.column - 1]))
                moveOptions.Remove(grid[startInfo.line, startInfo.column - 1]);
        }
        // Direita do Start
        if (startInfo.column != (columns - 1))
        {
            if (moveOptions.Contains(grid[startInfo.line, startInfo.column + 1]))
                moveOptions.Remove(grid[startInfo.line, startInfo.column + 1]);
        }
        // Cima do Start
        if (startInfo.line != 0)
        {
            if (moveOptions.Contains(grid[startInfo.line - 1, startInfo.column]))
                moveOptions.Remove(grid[startInfo.line - 1, startInfo.column]);
        }
        // Baixo do Start
        if (startInfo.line != (lines - 1))
        {
            if (moveOptions.Contains(grid[startInfo.line + 1, startInfo.column]))
                moveOptions.Remove(grid[startInfo.line + 1, startInfo.column]);
        }


        // Para não acontecer de colocar pedra em ices que o player precisa passar:
        // Se a lastDirection for Horizontal, eu preciso remover todos os ices que o player passou na vertical do ice que o player está.
        // Se a lastDirection for Vertical, eu preciso remover todos os ices que o player passou na horizontal do ice que o player está.
        // Por que disso ? O player sempre se move em sequencia horizontal > vertical ou vice versa

        // Removendo caso o lastDirection for Horizontal
        // Nesse caso, o próximo movimento será na Vertical. Logo, irei remover os ices que já passei nessa coluna
        // Coluna fixa, move na linha
        if (lastDirection.Equals("Horizontal"))
        {
            for (int i = 0; i <= lines - 1; i++)
            {
                // Verifico se não é a posição do player
//                if (i != playerCopyInfo.line)
//                {
                    // Verifico se já passou nesse ice
                    if (grid[i, playerCopyInfo.column].passed)
                    {
                        // Se não for um extremo
                        if (grid[i, playerCopyInfo.column].line != 0)
                        {
                            if (moveOptions.Contains(grid[i - 1, playerCopyInfo.column]))
                            {
                                Debug.Log("Removeu .passed " + grid[i - 1, playerCopyInfo.column].name);
                                moveOptions.Remove(grid[i - 1, playerCopyInfo.column]);
                            }
                        }
                        // Se não for um extremo
                        if (grid[i, playerCopyInfo.column].line != (lines - 1))
                        {
                            if (moveOptions.Contains(grid[i + 1, playerCopyInfo.column]))
                            {
                                Debug.Log("Removeu .passed " + grid[i + 1, playerCopyInfo.column].name);
                                moveOptions.Remove(grid[i + 1, playerCopyInfo.column]);
                            }
                        }
//                    }
                }
            }
        }
        // Removendo caso o lastDirection for Vertical
        // Nesse caso, o próximo movimento será na Horizontal. Logo, irei remover os ices que já passei nessa linha
        // Linha fixa, move na coluna
        if (lastDirection.Equals("Vertical"))
        {
            for (int j = 0; j <= columns - 1; j++)
            {
                // Verifico se não é a posição do player
//                if (j != playerCopyInfo.column)
//                {
                    // Verifico se já passou nesse ice
                    if (grid[playerCopyInfo.line, j].passed)
                    {
                        // Se não for um extremo
                        if (grid[playerCopyInfo.line, j].column != 0)
                        {
                            if (moveOptions.Contains(grid[playerCopyInfo.line, j - 1]))
                            {
                                Debug.Log("Removeu .passed " + grid[playerCopyInfo.line, j - 1].name);
                                moveOptions.Remove(grid[playerCopyInfo.line, j - 1]);
                            }
                        }
                        // Se não for um extremo
                        if (grid[playerCopyInfo.line, j].column != (columns - 1))
                        {
                            if (moveOptions.Contains(grid[playerCopyInfo.line, j + 1]))
                            {
                                Debug.Log("Removeu .passed " + grid[playerCopyInfo.line, j + 1].name);
                                moveOptions.Remove(grid[playerCopyInfo.line, j + 1]);
                            }
                        }
                    }
//                }
            }
        }

        // Mesma lógica acima
        // Removendo ices que estão próximos à stones e removendo ices que são stones
        //if (lastDirection.Equals("Horizontal"))
        //{
        //    for (int i = 0; i < lines - 1; i++)
        //   {
                // Verifico se não é a posição do player
        //        if (i != playerCopyInfo.line)
        //        {
                    // Verifico se é stone
        //            if (grid[i, playerCopyInfo.column].isStone)
          //          {
            //            // Se não for um extremo
              //          if (grid[i, playerCopyInfo.column].line != 0)
                //        {
                  //          if (moveOptions.Contains(grid[i - 1, playerCopyInfo.column]))
                    //            moveOptions.Remove(grid[i - 1, playerCopyInfo.column]);
                     //   }
                        // Se não for um extremo
                     //   if (grid[i, playerCopyInfo.column].line != (lines - 1))
                      //  {
                       //     if (moveOptions.Contains(grid[i + 1, playerCopyInfo.column]))
                       //         moveOptions.Remove(grid[i + 1, playerCopyInfo.column]);
                        //}
                        // Removo a stone
                       // moveOptions.Remove(grid[i, playerCopyInfo.column]);
                    //}
               // }
            //}
        //}
        // Removendo caso o lastDirection for Vertical
        // Nesse caso, o próximo movimento será na Horizontal. Logo, irei remover os ices que já passei nessa linha
        // Linha fixa, move na coluna
        //if (lastDirection.Equals("Vertical"))
        //{
         //   for (int j = 0; j < columns - 1; j++)
          //  {
           //     // Verifico se não é a posição do player
            //    if (j != playerCopyInfo.column)
             //   {
              //      // Verifico se já passou nesse ice
               //     if (grid[playerCopyInfo.line, j].isStone)
                //    {
                 //       // Se não for um extremo
                  //      if (grid[playerCopyInfo.line, j].column != 0)
                   //     {
                    //        if (moveOptions.Contains(grid[playerCopyInfo.line, j - 1]))
                     //           moveOptions.Remove(grid[playerCopyInfo.line, j - 1]);
                      //  }
                       // // Se não for um extremo
                        //if (grid[playerCopyInfo.line, j].column != (columns - 1))
                        //{
                         //   if (moveOptions.Contains(grid[playerCopyInfo.line, j + 1]))
                          //      moveOptions.Remove(grid[playerCopyInfo.line, j + 1]);
                        //}
                        // Removo a stone
                        //moveOptions.Remove(grid[playerCopyInfo.line, j]);
                    //}
                //}
            //}
        //}

        // Removendo ices que estão após uma stone
        // Exemplo:
        // ice 0, ice 1, player, ice 2, stone ,ice 3, ice 4
        // Irei remover da lista de opções os ices 3 e 4
        if (lastDirection.Equals("Horizontal"))
        {
            // Procurando por stone em cima do player
            for (int i = playerCopyInfo.line - 1; i > 0; i--)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[i, playerCopyInfo.column].isStone)
                {
                    for (int k = i; k >= 0; k--)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[k, playerCopyInfo.column]);
                        moveOptions.Remove(grid[k, playerCopyInfo.column]);
                    }
                }
            }
            // Procurando por stone embaixo do player
            for (int i = playerCopyInfo.line + 1; i < lines - 1; i++)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[i, playerCopyInfo.column].isStone)
                {
                    for (int k = i; k <= lines - 1; k++)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[k, playerCopyInfo.column]);
                        moveOptions.Remove(grid[k, playerCopyInfo.column]);
                    }
                }
            }
        }
        if (lastDirection.Equals("Vertical"))
        {
            // Procurando por stone a direita do player
            for (int j = playerCopyInfo.column + 1; j < columns - 1; j++)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[playerCopyInfo.line, j].isStone)
                {
                    for (int k = j; k <= columns - 1; k++)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[playerCopyInfo.line, k]);
                        moveOptions.Remove(grid[playerCopyInfo.line, k]);
                    }
                }
            }
            // Procurando por stone a esquerda do player
            for (int j = playerCopyInfo.line - 1; j > 0; j--)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[playerCopyInfo.line, j].isStone)
                {
                    for (int k = j; k >= 0; k--)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[playerCopyInfo.line, k]);
                        moveOptions.Remove(grid[playerCopyInfo.line, k]);
                    }
                }
            }
        }

        // Remove a posição que o playerCopy está (pra não randomizar a mesma posição)
        moveOptions.Remove(grid[playerCopyInfo.line, playerCopyInfo.column]);
    }

    // MoveOptions para Level sem usar itens
    void setMoveOptionsWithItens(PlayerInfo playerCopyInfo, string lastDirection, string currDir)
    {
        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Se o último movimento tiver sido na Horizontal, o próximo movimento deverá ser na Vertical. Logo, seleciono os ices da vertical
                if (lastDirection.Equals("Horizontal"))
                {
                    // Pego os que estão na mesma coluna
                    if (grid[i, j].column == playerCopyInfo.column)
                    {
                        moveOptions.Add(grid[i, j]);
                    }
                }
                // Se o último movimento tiver sido na Vertical, o próximo movimento deverá ser na Horizontal. Logo, seleciono os ices da horizontal
                else if (lastDirection.Equals("Vertical"))
                {
                    // Pego os que estão na mesma linha
                    if (grid[i, j].line == playerCopyInfo.line)
                    {
                        moveOptions.Add(grid[i, j]);
                    }
                }
                // Se for o primeiro passo, tanto faz. Pego os da linha e coluna
                else
                {
                    if (grid[i, j].line == playerCopyInfo.line || grid[i, j].column == playerCopyInfo.column)
                    {
                        moveOptions.Add(grid[i, j]);
                    }
                }
            }
        }

        StartEndInfo startInfo = start.GetComponent<StartEndInfo>();
        // Remove da lista os 4 ices em volta do Start.
        // Esquerda do Start
        if (startInfo.column != 0)
        {
            if (moveOptions.Contains(grid[startInfo.line, startInfo.column - 1]))
                moveOptions.Remove(grid[startInfo.line, startInfo.column - 1]);
        }
        // Direita do Start
        if (startInfo.column != (columns - 1))
        {
            if (moveOptions.Contains(grid[startInfo.line, startInfo.column + 1]))
                moveOptions.Remove(grid[startInfo.line, startInfo.column + 1]);
        }
        // Cima do Start
        if (startInfo.line != 0)
        {
            if (moveOptions.Contains(grid[startInfo.line - 1, startInfo.column]))
                moveOptions.Remove(grid[startInfo.line - 1, startInfo.column]);
        }
        // Baixo do Start
        if (startInfo.line != (lines - 1))
        {
            if (moveOptions.Contains(grid[startInfo.line + 1, startInfo.column]))
                moveOptions.Remove(grid[startInfo.line + 1, startInfo.column]);
        }


        // Para não acontecer de colocar pedra em ices que o player precisa passar:
        // Se a lastDirection for Horizontal, eu preciso remover todos os ices que o player passou na vertical do ice que o player está.
        // Se a lastDirection for Vertical, eu preciso remover todos os ices que o player passou na horizontal do ice que o player está.
        // Por que disso ? O player sempre se move em sequencia horizontal > vertical ou vice versa

        // Removendo caso o lastDirection for Horizontal
        // Nesse caso, o próximo movimento será na Vertical. Logo, irei remover os ices que já passei nessa coluna
        // Coluna fixa, move na linha
        if (lastDirection.Equals("Horizontal"))
        {
            for (int i = 0; i <= lines - 1; i++)
            {
                // Verifico se não é a posição do player
                //                if (i != playerCopyInfo.line)
                //                {
                // Verifico se já passou nesse ice
                if (grid[i, playerCopyInfo.column].passed)
                {
                    // Se não for um extremo
                    if (grid[i, playerCopyInfo.column].line != 0)
                    {
                        if (moveOptions.Contains(grid[i - 1, playerCopyInfo.column]))
                        {
                            Debug.Log("Removeu .passed " + grid[i - 1, playerCopyInfo.column].name);
                            moveOptions.Remove(grid[i - 1, playerCopyInfo.column]);
                        }
                    }
                    // Se não for um extremo
                    if (grid[i, playerCopyInfo.column].line != (lines - 1))
                    {
                        if (moveOptions.Contains(grid[i + 1, playerCopyInfo.column]))
                        {
                            Debug.Log("Removeu .passed " + grid[i + 1, playerCopyInfo.column].name);
                            moveOptions.Remove(grid[i + 1, playerCopyInfo.column]);
                        }
                    }
                    //                    }
                }
            }
        }
        // Removendo caso o lastDirection for Vertical
        // Nesse caso, o próximo movimento será na Horizontal. Logo, irei remover os ices que já passei nessa linha
        // Linha fixa, move na coluna
        if (lastDirection.Equals("Vertical"))
        {
            for (int j = 0; j <= columns - 1; j++)
            {
                // Verifico se não é a posição do player
                //                if (j != playerCopyInfo.column)
                //                {
                // Verifico se já passou nesse ice
                if (grid[playerCopyInfo.line, j].passed)
                {
                    // Se não for um extremo
                    if (grid[playerCopyInfo.line, j].column != 0)
                    {
                        if (moveOptions.Contains(grid[playerCopyInfo.line, j - 1]))
                        {
                            Debug.Log("Removeu .passed " + grid[playerCopyInfo.line, j - 1].name);
                            moveOptions.Remove(grid[playerCopyInfo.line, j - 1]);
                        }
                    }
                    // Se não for um extremo
                    if (grid[playerCopyInfo.line, j].column != (columns - 1))
                    {
                        if (moveOptions.Contains(grid[playerCopyInfo.line, j + 1]))
                        {
                            Debug.Log("Removeu .passed " + grid[playerCopyInfo.line, j + 1].name);
                            moveOptions.Remove(grid[playerCopyInfo.line, j + 1]);
                        }
                    }
                }
                //                }
            }
        }

        // Mesma lógica acima
        // Removendo ices que estão próximos à stones e removendo ices que são stones
        //if (lastDirection.Equals("Horizontal"))
        //{
        //    for (int i = 0; i < lines - 1; i++)
        //   {
        // Verifico se não é a posição do player
        //        if (i != playerCopyInfo.line)
        //        {
        // Verifico se é stone
        //            if (grid[i, playerCopyInfo.column].isStone)
        //          {
        //            // Se não for um extremo
        //          if (grid[i, playerCopyInfo.column].line != 0)
        //        {
        //          if (moveOptions.Contains(grid[i - 1, playerCopyInfo.column]))
        //            moveOptions.Remove(grid[i - 1, playerCopyInfo.column]);
        //   }
        // Se não for um extremo
        //   if (grid[i, playerCopyInfo.column].line != (lines - 1))
        //  {
        //     if (moveOptions.Contains(grid[i + 1, playerCopyInfo.column]))
        //         moveOptions.Remove(grid[i + 1, playerCopyInfo.column]);
        //}
        // Removo a stone
        // moveOptions.Remove(grid[i, playerCopyInfo.column]);
        //}
        // }
        //}
        //}
        // Removendo caso o lastDirection for Vertical
        // Nesse caso, o próximo movimento será na Horizontal. Logo, irei remover os ices que já passei nessa linha
        // Linha fixa, move na coluna
        //if (lastDirection.Equals("Vertical"))
        //{
        //   for (int j = 0; j < columns - 1; j++)
        //  {
        //     // Verifico se não é a posição do player
        //    if (j != playerCopyInfo.column)
        //   {
        //      // Verifico se já passou nesse ice
        //     if (grid[playerCopyInfo.line, j].isStone)
        //    {
        //       // Se não for um extremo
        //      if (grid[playerCopyInfo.line, j].column != 0)
        //     {
        //        if (moveOptions.Contains(grid[playerCopyInfo.line, j - 1]))
        //           moveOptions.Remove(grid[playerCopyInfo.line, j - 1]);
        //  }
        // // Se não for um extremo
        //if (grid[playerCopyInfo.line, j].column != (columns - 1))
        //{
        //   if (moveOptions.Contains(grid[playerCopyInfo.line, j + 1]))
        //      moveOptions.Remove(grid[playerCopyInfo.line, j + 1]);
        //}
        // Removo a stone
        //moveOptions.Remove(grid[playerCopyInfo.line, j]);
        //}
        //}
        //}
        //}

        // Removendo ices que estão após uma stone
        // Exemplo:
        // ice 0, ice 1, player, ice 2, stone ,ice 3, ice 4
        // Irei remover da lista de opções os ices 3 e 4
        if (lastDirection.Equals("Horizontal"))
        {
            // Procurando por stone em cima do player
            for (int i = playerCopyInfo.line - 1; i > 0; i--)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[i, playerCopyInfo.column].isStone)
                {
                    for (int k = i; k >= 0; k--)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[k, playerCopyInfo.column]);
                        moveOptions.Remove(grid[k, playerCopyInfo.column]);
                    }
                }
            }
            // Procurando por stone embaixo do player
            for (int i = playerCopyInfo.line + 1; i < lines - 1; i++)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[i, playerCopyInfo.column].isStone)
                {
                    for (int k = i; k <= lines - 1; k++)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[k, playerCopyInfo.column]);
                        moveOptions.Remove(grid[k, playerCopyInfo.column]);
                    }
                }
            }
        }
        if (lastDirection.Equals("Vertical"))
        {
            // Procurando por stone a direita do player
            for (int j = playerCopyInfo.column + 1; j < columns - 1; j++)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[playerCopyInfo.line, j].isStone)
                {
                    for (int k = j; k <= columns - 1; k++)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[playerCopyInfo.line, k]);
                        moveOptions.Remove(grid[playerCopyInfo.line, k]);
                    }
                }
            }
            // Procurando por stone a esquerda do player
            for (int j = playerCopyInfo.line - 1; j > 0; j--)
            {
                // Verifico se é stone
                // Se for, remove das opções todos os ices a cima
                if (grid[playerCopyInfo.line, j].isStone)
                {
                    for (int k = j; k >= 0; k--)
                    {
                        // Removo a(s) stone(s)
                        Debug.Log("Removeu .stones " + grid[playerCopyInfo.line, k]);
                        moveOptions.Remove(grid[playerCopyInfo.line, k]);
                    }
                }
            }
        }

        // Remove a posição que o playerCopy está (pra não randomizar a mesma posição)
        moveOptions.Remove(grid[playerCopyInfo.line, playerCopyInfo.column]);
    }


    void setIcesPlayerCanMove()
    {
        PlayerInfo playerInfo = player.GetComponent<PlayerInfo>();

        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                // Se o ice estiver na mesma linha ou coluna do player, seta esse ice como permitido para o player clicar
                if (grid[i, j].line == playerInfo.line || grid[i, j].column == playerInfo.column)
                {
                    moveOptions.Add(grid[i, j]);
                    grid[i, j].canMove = true;
                }
            }
        }
    }


    void setGuisPassed()
    {
        GameObject[] ices = GameObject.FindGameObjectsWithTag("Ice");

        for (int i = 0; i < ices.Length; i++)
        {
            if (ices[i].GetComponent<IceInfo>().passed)
            {
                GUIText guiById = GameObject.Find(ices[i].GetComponent<IceInfo>().id.ToString()).GetComponent<GUIText>();

                guiById.text = ices[i].GetComponent<IceInfo>().stepOrder.ToString();
            }
        }
    }

    void setGuiTexts()
    {
        // Medidas da tela
        upperCorner = new Vector3(Screen.width, Screen.height, 0.0f);
        target = Camera.main.ScreenToWorldPoint(upperCorner);

        // Posicionando gui texts nos ices
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");

        for (int i = 0; i < icesInScene.Length; i++)
        {
            // Pegando o ice por id
            int k = 0;
            while (icesInScene[k].GetComponent<IceInfo>().id != i)
            {
                k++;
            }
            GameObject iceCorrectId = icesInScene[k];

            // Instanciando o gui text
            GameObject iceText = Instantiate(iceTextPrefab, transform.position, Quaternion.identity) as GameObject;

            // Posição x e y do gui text
            float x, y;

            // Pegando a posição correta do ice pra fazer a regra de 3
            float icePosX, icePosY;
            // Se a posição for positiva, significa que ele está do lado direito, logo tenho SOMAR A PARTE NEGATIVA DA TELA (a tela vai de -target.x até +target.x)
            if (iceCorrectId.transform.position.x > 0)
            {
                icePosX = iceCorrectId.transform.position.x + target.x;
            }
            else
            {
                icePosX = target.x - Mathf.Abs(iceCorrectId.transform.position.x);
            }
            if (iceCorrectId.transform.position.y > 0)
            {
                icePosY = iceCorrectId.transform.position.y + target.y;
            }
            else
            {
                icePosY = target.y - Mathf.Abs(iceCorrectId.transform.position.y);
            }

            // Regra de 3
            x = icePosX / (target.x * 2);
            y = icePosY / (target.y * 2);

            // Posicionando o ice
            iceText.transform.localPosition = new Vector3(x, y, iceText.transform.localPosition.z);
            iceText.GetComponent<GUIText>().text = "";
            iceText.name = i.ToString();

            // Seta como child
            //iceText.transform.SetParent(GameObject.Find("gui texts").transform);
        }
    }
}
