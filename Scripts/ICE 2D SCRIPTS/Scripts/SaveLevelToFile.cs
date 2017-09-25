using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.UI;

public class SaveLevelToFile : MonoBehaviour
{

    static GridManager4 grid;

    public Text teste;

    void Start()
    {
        //grid = GameObject.Find("Game Grid").GetComponent<GridManager4>();

        teste.text = Application.persistentDataPath;
    }

    public static void Save(string dificuldade)
    {
        // 0 : ice
        // 1 : stone
        // 2 : start
        // 3 : end

        string matriz = string.Format("Matriz [{0}]x[{1}]", grid.lines, grid.columns);

        // Seta a pasta
        string directory = "/sdcard/Ice Adventure Levels/" + matriz + "/" + dificuldade;
        //string directory = @"C:/TESTEEEEE/" + matriz + "/" + dificuldade;

        // Se a pasta não existir, cria
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Cria o txt do level
        directory += "/" + dificuldade + PlayerPrefs.GetInt(dificuldade + matriz + "count").ToString() + ".txt";
        
        StreamWriter writer = new StreamWriter(directory);

        // Salvando level
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");

        // Organiza o array
        organizeArray(icesInScene);

        // Escrevo o level no txt
        writeLevelTxt(writer, icesInScene);

        // Soma 1 pra salvar o próximo level em outro txt
        PlayerPrefs.SetInt(dificuldade + "count", PlayerPrefs.GetInt(dificuldade + matriz + "count") + 1);

        writer.Close();
    }


    void Save3(string dificuldade)
    {
        // Seta a pasta
        string directory = @"C:/TESTE/IceAdventureLevels/" + dificuldade;

        // Se a pasta não existir, cria
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Cria o txt do level
        directory += "/" + dificuldade + PlayerPrefs.GetInt(dificuldade + "count").ToString() + ".txt";

        StreamWriter writer = new StreamWriter(directory);

        // Salvando level
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");

        // Organiza o array
        organizeArray(icesInScene);

        // Escrevo o level no txt
        writeLevelTxt(writer, icesInScene);

        // Soma 1 pra salvar o próximo level em outro txt
        PlayerPrefs.SetInt(dificuldade + "count", PlayerPrefs.GetInt(dificuldade + "count") + 1);

        writer.Close();
    }

    public void Save2()
    {
        StreamWriter writer = new StreamWriter(@"C:/TESTE\testando.txt");

        // Salvando level
        GameObject[] icesInScene = GameObject.FindGameObjectsWithTag("Ice");

        // Organiza o array
        organizeArray(icesInScene);

        // Escrevo o level no txt
        writeLevelTxt(writer, icesInScene);

        writer.Close();
    }

    static void writeLevelTxt(StreamWriter writer, GameObject[] ices)
    {
        string kkk = "";
        for (int i = 0; i < (int) ((grid.columns-5)/2); i++)
        {
            kkk += "-";
        }
        string matriz = kkk + " " + grid.lines.ToString() + "x" + grid.columns.ToString() + " " + kkk;
        writer.WriteLine(matriz);

        string output = "";

        int k = 0;

        for (int i = 0; i < grid.lines; i++)
        {
            for (int j = 0; j < grid.columns; j++)
            {
                // Stone
                if (ices[k].GetComponent<IceInfo>().isStone)
                {
                    output += "1";
                }

                // Start
                else if (ices[k].GetComponent<IceInfo>().isStart)
                {
                    output += "2";
                }

                // End
                else if (ices[k].GetComponent<IceInfo>().isEnd)
                {
                    output += "3";
                }

                // Ice
                else
                {
                    output += "0";

                }

                k++;
            }
            // Terminou de ver todos ices da linha
            // Escrevo o output da linha
            writer.WriteLine(output);

            // Limpo o output pra pular pra próxima linha
            output = "";
        }
    }

    static void organizeArray(GameObject[] ices)
    {
        for (int i = 0; i < ices.Length; i++)
        {
            GameObject curr = ices[i];
            GameObject aux = ices[curr.GetComponent<IceInfo>().id];
            ices[curr.GetComponent<IceInfo>().id] = curr;
            ices[i] = aux;
        }
    }

    public static void levelDificulty()
    {
        // Fez o level de primeira
        if (LevelInfo.backToSameIce == 0)
            Save("Muito fácil");
        // Voltou à um ice que já esteve algumas vezes
        else if (LevelInfo.backToSameIce == 1 || LevelInfo.backToSameIce == 2)
            Save("Fácil");
        else if (LevelInfo.backToSameIce == 3)
            Save("Médio");
        else if (LevelInfo.backToSameIce == 4)
            Save("Difícil");
        else
        {
            Save("Muito difícil");
        }
    }

}
