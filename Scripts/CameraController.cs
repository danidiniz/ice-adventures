using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    public GridManager gridManager;

    [HideInInspector]
    public GameObject nodeDaPontaEsquerda;
    [HideInInspector]
    public GameObject nodeDaPontaDireita;


    public float panSpeed = 30f;
    public float panSpeedScroll = 60f;
    public float maxZoomIn = 10f;
    public float maxZoomOut = 200f;

    // Camera Movement mobile
    public float dragSpeed = 30f;
    private Vector3 dragOrigin;

    public bool cameraDragging = true;

    public float maxLeft = 4f;
    public float maxRight = 50f;
    public float maxUp = -50f;
    public float maxDown = 50f;

    void Start()
    {
        CentralizarCamera();
    }

    public GameObject canvasPanel;

    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            canvasPanel.SetActive(!canvasPanel.activeSelf);
        }


        if (Input.GetKey("w"))
        {
            transform.Translate(Vector3.forward * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("s"))
        {
            transform.Translate(Vector3.back * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("d"))
        {
            transform.Translate(Vector3.right * panSpeed * Time.deltaTime, Space.World);
        }
        if (Input.GetKey("a"))
        {
            transform.Translate(Vector3.left * panSpeed * Time.deltaTime, Space.World);
        }
        if(Input.GetAxis("Mouse ScrollWheel") > 0f && transform.position.y > maxZoomIn)
        {
            transform.Translate(Vector3.down * panSpeedScroll * Time.deltaTime, Space.World);
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0f && transform.position.y < maxZoomOut)
        {
            transform.Translate(Vector3.up * panSpeedScroll * Time.deltaTime, Space.World);
        }

        //CameraMovementMobile();
    }

    void CentralizarCamera()
    {
        int lineCenter = (gridManager.lines / 2 == 0) ? gridManager.lines / 2 : gridManager.lines / 2 + 1;
        int columnCenter = (gridManager.columns / 2 == 0) ? gridManager.columns / 2 : gridManager.columns / 2 + 1;

        transform.position = new Vector3(transform.position.x + lineCenter * gridManager.node.transform.localScale.x, transform.position.y, transform.position.z - columnCenter * gridManager.node.transform.localScale.z);
    }

    

    void CameraMovementMobile()
    {
        Vector2 mousePosition = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

        float left = Screen.width * 0.2f;
        float right = Screen.width - (Screen.width * 0.2f);

        if (mousePosition.x < left)
        {
            cameraDragging = true;
        }
        else if (mousePosition.x > right)
        {
            cameraDragging = true;
        }






        if (cameraDragging)
        {

            if (Input.GetMouseButtonDown(0))
            {
                dragOrigin = Input.mousePosition;
                return;
            }

            if (!Input.GetMouseButton(0)) return;

            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition - dragOrigin);
            Vector3 move = new Vector3(pos.x * dragSpeed, 0, pos.z * dragSpeed);

            if (move.x > 0f)
            {
                if (transform.position.x < maxLeft)
                {
                    transform.Translate(move, Space.World);
                }
            }
            if (move.x < 0f)
            {
                if (transform.position.x < maxRight)
                {
                    transform.Translate(move, Space.World);
                }
            }
            if (move.y > 0f)
            {
                if (transform.position.x < maxDown)
                {
                    transform.Translate(move, Space.World);
                }
            }
            if (move.x < 0f)
            {
                if (transform.position.x < maxUp)
                {
                    transform.Translate(move, Space.World);
                }
            }
        }
    }
}
