using UnityEngine;
using System.Collections.Generic;

public class LevelInfo : MonoBehaviour
{

    public static int stepsCount;

    // Toda vez que o player volta à um ice que ele já esteve, significa que ele não terminou o level "de primeira"
    public static int backToSameIce;

    public static List<IceInfo> icesThatPlayerWent = new List<IceInfo>();

    void Start()
    {
        stepsCount = 0;
        backToSameIce = 0;
    }

    public static void addIce(IceInfo ice)
    {
        // Se ele nunca foi até esse ice, adiciono à lista
        if (!icesThatPlayerWent.Contains(ice))
        {
            icesThatPlayerWent.Add(ice);
        }
        // Se não, significa que ele ja esteve nesse ice, então soma na dificuldade
        else
        {
            backToSameIce++;
        }
    }

}
