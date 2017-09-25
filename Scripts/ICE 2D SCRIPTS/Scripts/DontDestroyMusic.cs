using UnityEngine;
using System.Collections;

public class DontDestroyMusic : MonoBehaviour
{
	void Start ()
    {

        DontDestroyOnLoad(this);
	
	}

}
