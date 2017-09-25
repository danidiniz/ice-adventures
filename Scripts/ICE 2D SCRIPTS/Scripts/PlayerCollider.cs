using UnityEngine;
using System.Collections;

public class PlayerCollider : MonoBehaviour
{
    SaveLevelToFile save;

    void Start()
    {
        save = GameObject.Find("Save Level").GetComponent<SaveLevelToFile>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.name.Equals("End"))
        {
            // Salva a dificuldade do level
            SaveLevelToFile.levelDificulty();

            Application.LoadLevel(0);
        }
    }

}
