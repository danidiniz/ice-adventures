using UnityEngine;
using System.Collections;

public class Background : MonoBehaviour
{

    Vector3 upperCorner, target;

    public GameObject background;

	void Awake ()
    {

        // Medidas da tela
        upperCorner = new Vector3(Screen.width, Screen.height, 0.0f);
        target = Camera.main.ScreenToWorldPoint(upperCorner);

        Debug.Log("-target x " + -target.x);
        Debug.Log("-target y " + -target.y);

        

        float backgroundExtentsX, backgroundExtentsY;
        backgroundExtentsX = background.GetComponent<SpriteRenderer>().bounds.extents.x;
        backgroundExtentsY = background.GetComponent<SpriteRenderer>().bounds.extents.y;


        transform.position = new Vector3(-target.x + backgroundExtentsX, target.y - backgroundExtentsY, 0.0f);

        float x = 0;
        float y = 0;

        for (float j = transform.position.y; j > -target.y * 2; j -= backgroundExtentsY)
        {
            for (float i = transform.position.x; i < target.x * 2; i += backgroundExtentsX)
            {
                Instantiate(background, new Vector3(transform.position.x + x, transform.position.y - y, 0.0f), Quaternion.identity);

                x += backgroundExtentsX;
            }

            x = 0;
            y += backgroundExtentsY;
        }

    }


}
