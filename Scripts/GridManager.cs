using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class GridManager : MonoBehaviour
{

    public struct StartEnd
    {
        public int line;
        public int col;
        public bool isActive;
    }

    public StartEnd startInf;
    public StartEnd endInf;

    // Prefabs
    public GameObject stone;
    public GameObject start;
    public GameObject end;
    public GameObject gold;

    public float stoneSizeMultiplier;

    public static GridManager instance;

    // Lista com os pontos chaves que o player ira criar
    public List<Node> pathOfPointNodes;
    // Lista com TODOS os nodes do caminho
    public List<Node> pathOfAllNodes;

    // Array contendo todos nodes
    public Node[,] gridOfNodes;

    public CameraController camera;

    public GameObject node;

    public int lines;
    public int columns;

    public float spaceBetweenNode;

    NodeParent.parOrdenado firstTunel;
    NodeParent.parOrdenado secondTunel;
    public Color tunelColor;

    void Awake()
    {
        startInf.line = -1;
        startInf.col = -1;
        startInf.isActive = false;

        endInf.line = -1;
        endInf.col = -1;
        endInf.isActive = false;

        instance = this;
    }

    void Start()
    {
        firstTunel.i = -1;
        firstTunel.j = -1;
        secondTunel.i = -1;
        secondTunel.j = -1;

        gridOfNodes = new Node[lines, columns];
        pathOfPointNodes = new List<Node>();
        CreateGrid();
    }

    void CreateGrid()
    {
        int lineCenter = lines / 2;
        for (int i = 0; i < lines; i++)
        {
            for (int j = 0; j < columns; j++)
            {
                Vector3 pos = new Vector3(transform.position.x + j * (node.transform.localScale.x + spaceBetweenNode), transform.position.y, transform.position.z - i * (node.transform.localScale.z + spaceBetweenNode));
                GameObject n = Instantiate(node, pos, Quaternion.identity) as GameObject;
                n.transform.parent = GameObject.Find("Nodes").transform;
                n.name = "Node [" + i + "][" + j + "]";

                n.GetComponent<Node>().Line = i;
                n.GetComponent<Node>().Column = j;

                gridOfNodes[i, j] = n.GetComponent<Node>();

                // Salvando os das pontas para centralizar a Camera
                if ((j == 0 || j == columns - 1) && i == lineCenter)
                {
                    if (camera.nodeDaPontaEsquerda == null)
                        camera.nodeDaPontaEsquerda = n;
                    else
                        camera.nodeDaPontaDireita = n;
                }
            }
        }
    }

   
    // Criar tunel
    public void SetTunel(Node n)
    {
        // Se eu clicar no mesmo tunel ou onde já tem um tunel ou qualquer outra coisa
        if (n.NodeType != NodeParent.tipo.ICE)
        {
            Debug.Log("You can't set a tunel here!");
            return;
        }

        // Se ainda não setei o primeiro tunel
        if(firstTunel.i == -1 && firstTunel.j == -1)
        {
            firstTunel.i = n.Line;
            firstTunel.j = n.Column;
            n.rend.material.color = tunelColor;
            n.colorCurrent = tunelColor;
        }
        // Se ainda não setei o segundo tunel
        else if (secondTunel.i == -1 && secondTunel.j == -1)
        {
            secondTunel.i = n.Line;
            secondTunel.j = n.Column;
            n.rend.material.color = tunelColor;
            n.colorCurrent = tunelColor;

            // Finalizando os tuneis
            gridOfNodes[firstTunel.i, firstTunel.j].tunelGoToLine = secondTunel.i;
            gridOfNodes[firstTunel.i, firstTunel.j].tunelGoToCol = secondTunel.j;

            gridOfNodes[secondTunel.i, secondTunel.j].tunelGoToLine = firstTunel.i;
            gridOfNodes[secondTunel.i, secondTunel.j].tunelGoToCol = firstTunel.j;

            // Resetando variaveis
            firstTunel.i = -1;
            firstTunel.j = -1;
            secondTunel.i = -1;
            secondTunel.j = -1;
        }
    }

    // Método para criar caminho
    public void PathMaker(Node node)
    {
        Color c = new Color();
        c = node.colorPath;

        int count = instance.pathOfPointNodes.Count;
        if (count > 0)
        {
            int startLine = pathOfPointNodes[count - 1].Line;
            int startColumn = pathOfPointNodes[count - 1].Column;

            int endLine = node.Line;
            int endColumn = node.Column;

            if ((startLine > endLine || startLine < endLine) && (startColumn > endColumn || startColumn < endColumn))
                return;

            if (!pathOfAllNodes.Contains(gridOfNodes[node.Line, node.Column]))
            {
                if (startLine < endLine || startColumn < endColumn)
                {
                    for (int i = startLine; i < endLine; i++)
                    {
                        gridOfNodes[i, endColumn].rend.material.color = c;
                        gridOfNodes[i, endColumn].colorCurrent = c;
                        node.dirStone = Node.directionOfStone.SOUTH;
                        pathOfAllNodes.Add(gridOfNodes[i, endColumn]);
                    }
                    for (int j = startColumn; j < endColumn; j++)
                    {
                        gridOfNodes[endLine, j].rend.material.color = c;
                        gridOfNodes[endLine, j].colorCurrent = c;
                        node.dirStone = Node.directionOfStone.EAST;
                        pathOfAllNodes.Add(gridOfNodes[endLine, j]);
                    }
                }
                else if (startLine > endLine || startColumn > endColumn)
                {
                    for (int i = startLine; i > endLine; i--)
                    {
                        gridOfNodes[i, endColumn].rend.material.color = c;
                        gridOfNodes[i, endColumn].colorCurrent = c;
                        node.dirStone = Node.directionOfStone.NORTH;
                        pathOfAllNodes.Add(gridOfNodes[i, endColumn]);
                    }
                    for (int j = startColumn; j > endColumn; j--)
                    {
                        gridOfNodes[endLine, j].rend.material.color = c;
                        gridOfNodes[endLine, j].colorCurrent = c;
                        node.dirStone = Node.directionOfStone.WEST;
                        pathOfAllNodes.Add(gridOfNodes[endLine, j]);
                    }
                }
            }
            else
            {
                Debug.Log("Ja clicou nesse ice");
                return;
            }
        }
        else
        {
            // Se for o primeiro node do path, eu seto o start nesse node
            // Se o player ja tiver setado um start, eu destruo e seto o novo start nesse node
            if (startInf.isActive)
            {
                ResetNode(gridOfNodes[startInf.line, startInf.col]);
                gridOfNodes[startInf.line, startInf.col].transform.GetChild(0).transform.position = node.transform.position;
                gridOfNodes[startInf.line, startInf.col].transform.GetChild(0).transform.parent = node.transform;
                startInf.line = node.Line;
                startInf.col  = node.Column;
            }
            gridOfNodes[node.Line, node.Column].rend.material.color = c;
        }

        if (node.dirStone != Node.directionOfStone.NONE)
        {
//            Debug.Log("Node " + node.name + " Dir " + node.dirStone);
            InstantiateStone(node);
        }

        pathOfPointNodes.Add(node);
        pathOfAllNodes.Add(node);
        node.colorCurrent = c;
    }

    // Método para desfazer caminho
    public void PathUndo()
    {
        // Último da lista
        Node node = pathOfPointNodes[pathOfPointNodes.Count - 1].GetComponent<Node>();

        Color c = new Color();
        c = pathOfPointNodes[pathOfPointNodes.Count - 1].colorStartMaterial;

        int count = pathOfPointNodes.Count;
        if (count > 0)
        {
            int startLine = node.Line;
            int startColumn = node.Column;

            // Caso tenha apenas 1 node no path
            if (count == 1)
            {
                pathOfPointNodes.Remove(pathOfPointNodes[pathOfPointNodes.Count - 1]);
                pathOfAllNodes.Remove(pathOfAllNodes[pathOfAllNodes.Count - 1]);
                ResetNode(node);   
                return;
            }

            // Penúltimo da lista
            int endLine = pathOfPointNodes[count - 2].Line;
            int endColumn = pathOfPointNodes[count - 2].Column;

            RemoveStoneEmVolta(node);

            if (startLine < endLine || startColumn < endColumn)
            {
                for (int i = startLine; i < endLine; i++)
                {
                    gridOfNodes[i, endColumn].rend.material.color = c;
                    gridOfNodes[i, endColumn].colorCurrent = c;
                    if (node.dirStone == Node.directionOfStone.NONE) node.dirStone = Node.directionOfStone.SOUTH;
                }
                for (int j = startColumn; j < endColumn; j++)
                {
                    gridOfNodes[endLine, j].rend.material.color = c;
                    gridOfNodes[endLine, j].colorCurrent = c;
                    if (node.dirStone == Node.directionOfStone.NONE) node.dirStone = Node.directionOfStone.EAST;
                }
            }
            else if (startLine > endLine || startColumn > endColumn)
            {
                for (int i = startLine; i > endLine; i--)
                {
                    gridOfNodes[i, endColumn].rend.material.color = c;
                    gridOfNodes[i, endColumn].colorCurrent = c;
                    if (node.dirStone == Node.directionOfStone.NONE) node.dirStone = Node.directionOfStone.NORTH;
                }
                for (int j = startColumn; j > endColumn; j--)
                {
                    gridOfNodes[endLine, j].rend.material.color = c;
                    gridOfNodes[endLine, j].colorCurrent = c;
                    if (node.dirStone == Node.directionOfStone.NONE) node.dirStone = Node.directionOfStone.WEST;
                }
            }

            //gridOfNodes[startLine, startColumn].NodeType = NodeParent.tipo.ICE;
            pathOfPointNodes.Remove(pathOfPointNodes[pathOfPointNodes.Count - 1]);
        }
    }

    public void PathByStep()
    {
        // Removendo os repetidos (gambiarra pq ainda n pensei em como ajeitar isso)
        for (int i = 0; i < pathOfAllNodes.Count-1; i++)
        {
            if(pathOfAllNodes[i] == pathOfAllNodes[i + 1])
            {
                pathOfAllNodes.Remove(pathOfAllNodes[i]);
                i--;
            }
        }
        StartCoroutine(StepByStep());
    }

    private IEnumerator StepByStep()
    {
        for (int i = 0; i < pathOfAllNodes.Count; i++)
        {
            pathOfAllNodes[i].rend.material.color = pathOfAllNodes[i].colorStartMaterial;
            Debug.Log(pathOfAllNodes[i].name);
        }
        yield return new WaitForSeconds(1.0f);
        for (int i = 0; i < pathOfAllNodes.Count; i++)
        {
            pathOfAllNodes[i].rend.material.color = Color.cyan;
            yield return new WaitForSeconds(0.1f);
            pathOfAllNodes[i].rend.material.color = pathOfAllNodes[i].colorPath;
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void InstantiateStone(Node node)
    {
        GameObject pedregulho = null;
        switch (node.dirStone)
        {
            case Node.directionOfStone.NONE:
                // Pos da stone em relação a este node
                node.posOfStone.i = node.Line;
                node.posOfStone.j = node.Column;

                gridOfNodes[node.Line, node.Column].NodeType = NodeParent.tipo.STONE;
                // Caso já tenha uma pedra, não instancia outras
                if (gridOfNodes[node.Line, node.Column].transform.childCount > 0)
                    break;

                pedregulho = Instantiate(stone, gridOfNodes[node.Line, node.Column].transform.position, Quaternion.identity) as GameObject;
                pedregulho.transform.parent = gridOfNodes[node.Line, node.Column].transform;
                break;
            case Node.directionOfStone.NORTH:
                if (CheckIfInsideGrid(node.Line - 1, node.Column))
                {
                    // Pos da stone em relação a este node
                    node.posOfStone.i = node.Line - 1;
                    node.posOfStone.j = node.Column;

                    gridOfNodes[node.Line - 1, node.Column].NodeType = NodeParent.tipo.STONE;
                    // Caso já tenha uma pedra, não instancia outras
                    if (gridOfNodes[node.Line - 1, node.Column].transform.childCount > 0)
                        break;

                    pedregulho = Instantiate(stone, gridOfNodes[node.Line - 1, node.Column].transform.position, Quaternion.identity) as GameObject;
                    pedregulho.transform.parent = gridOfNodes[node.Line - 1, node.Column].transform;
                }
                break;
            case Node.directionOfStone.SOUTH:
                if (CheckIfInsideGrid(node.Line + 1, node.Column))
                {
                    // Pos da stone em relação a este node
                    node.posOfStone.i = node.Line + 1;
                    node.posOfStone.j = node.Column;

                    gridOfNodes[node.Line + 1, node.Column].NodeType = NodeParent.tipo.STONE;
                    // Caso já tenha uma pedra, não instancia outras
                    if (gridOfNodes[node.Line + 1, node.Column].transform.childCount > 0)
                        break;

                    pedregulho = Instantiate(stone, gridOfNodes[node.Line + 1, node.Column].transform.position, Quaternion.identity) as GameObject;
                    pedregulho.transform.parent = gridOfNodes[node.Line + 1, node.Column].transform;
                }
                break;
            case Node.directionOfStone.EAST:
                if (CheckIfInsideGrid(node.Line, node.Column + 1))
                {
                    // Pos da stone em relação a este node
                    node.posOfStone.i = node.Line;
                    node.posOfStone.j = node.Column + 1;

                    gridOfNodes[node.Line, node.Column + 1].NodeType = NodeParent.tipo.STONE;
                    // Caso já tenha uma pedra, não instancia outras
                    if (gridOfNodes[node.Line, node.Column + 1].transform.childCount > 0)
                        break;
                    pedregulho = 
                        Instantiate(stone, gridOfNodes[node.Line, node.Column + 1].transform.position, Quaternion.identity) as GameObject;
                    pedregulho.transform.parent = gridOfNodes[node.Line, node.Column + 1].transform;
                }
                break;
            case Node.directionOfStone.WEST:
                if (CheckIfInsideGrid(node.Line, node.Column - 1))
                {
                    // Pos da stone em relação a este node
                    node.posOfStone.i = node.Line;
                    node.posOfStone.j = node.Column - 1;

                    gridOfNodes[node.Line, node.Column - 1].NodeType = NodeParent.tipo.STONE;
                    // Caso já tenha uma pedra, não instancia outras
                    if (gridOfNodes[node.Line, node.Column - 1].transform.childCount > 0)
                        break;

                    pedregulho = Instantiate(stone, gridOfNodes[node.Line, node.Column - 1].transform.position, Quaternion.identity) as GameObject;
                    pedregulho.transform.parent = gridOfNodes[node.Line, node.Column - 1].transform;
                }
                break;
        }
        if (pedregulho != null)
        {
            pedregulho.name = "Stone";
            Vector3 size = pedregulho.transform.localScale;
            pedregulho.transform.localScale = new Vector3(size.x * stoneSizeMultiplier, size.y * stoneSizeMultiplier, size.z * stoneSizeMultiplier);
        }
    }

    public void RemoveStoneEmVolta(Node node)
    {
        if(CheckIfInsideGrid(node.posOfStone.i, node.posOfStone.j))
        {
            GameObject pedregulho = gridOfNodes[node.posOfStone.i, node.posOfStone.j].gameObject;
            for (int i = 0; i < pedregulho.transform.childCount; i++)
            {
                if (pedregulho.transform.GetChild(i).name.Equals("Stone"))
                {
                    Destroy(pedregulho.transform.GetChild(i).gameObject);
                }
            }
        }
    }

    public bool CheckIfInsideGrid(int line, int col)
    {
        if ((line >= 0 && line < lines) && (col >= 0 && col < columns))
            return true;
        return false;
    }

    public void ResetNode(Node node)
    {
        // Se o node for um tunel, reseto o outro tunel ligado à esse
        if(node.NodeType == NodeParent.tipo.TUNEL)
        {
            // Altero as cores pq ele n estava resetando a cor
            Debug.Log("Node atual: " + node.name + " | proximo node: " + gridOfNodes[node.tunelGoToLine, node.tunelGoToCol].name);
            // Se eu não resetar o outro tunel antes de chamar o método, ele entraria em loop
            gridOfNodes[node.tunelGoToLine, node.tunelGoToCol].NodeType = NodeParent.tipo.ICE;
            gridOfNodes[node.tunelGoToLine, node.tunelGoToCol].tunelGoToCol = -1;
            gridOfNodes[node.tunelGoToLine, node.tunelGoToCol].tunelGoToLine = -1;
            // Agora sim, chamo o método e reseto o tunel
            ResetNode(gridOfNodes[node.tunelGoToLine, node.tunelGoToCol]);
        }
        for (int i = 0; i < node.gameObject.transform.childCount; i++)
        {
            Destroy(node.transform.GetChild(i).gameObject);
        }
        node.gameObject.SetActive(false);
        node.gameObject.SetActive(true);
    }
}
