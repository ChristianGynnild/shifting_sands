using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Rigidbody2D rb;
    public Rigidbody2D Prb;
    public Transform PlayerT;
    public Transform CameraT;
    public Vector3 PreferedPos;
    public float OffsetX;
    public float OffsetY;
    public float CameraSpeedX;
    public float CameraSpeedY;
    public float MaxDistanceX;
    public float MaxDistanceY;
    public float MaxDistanceSpeedInc;
    

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (CameraT.position.x + 1 < PreferedPos.x)
        {
            rb.velocity = new Vector2(CameraSpeedX, rb.velocity.y);
        }
        else if (CameraT.position.x - 1 > PreferedPos.x)
        {
            rb.velocity = new Vector2(-CameraSpeedX, rb.velocity.y);
        }
        else
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
        }

        if (CameraT.position.y - 1 > PreferedPos.y)
        {
            rb.velocity = new Vector2(rb.velocity.x, -CameraSpeedY);
        }
        else if (CameraT.position.y + 1 < PreferedPos.y)
        {
            rb.velocity = new Vector2(rb.velocity.x, CameraSpeedY);
        }
        else
        {
            rb.velocity = new Vector2(rb.velocity.x, 0);
        }

        /*
        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            PreferedPos.x = PlayerT.position.x - OffsetX;
        }
        else if (!Input.GetKey(KeyCode.A) && Input.GetKey(KeyCode.D))
        {
            PreferedPos.x = PlayerT.position.x + OffsetX;
        }
        else PreferedPos.x = PlayerT.position.x;

        if (Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S))
        {
            PreferedPos.y = PlayerT.position.y + OffsetY;
        }
        else if (!Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.S))
        {
            PreferedPos.y = PlayerT.position.y - OffsetY;
        }
        else PreferedPos.y = PlayerT.position.y;
        */


        //Looking very harsh
        //PreferedPos.x = PlayerT.position.x + Player.PositionChange.x * 10; //Prb.velocity.x / 1.4f;
        //PreferedPos.y = PlayerT.position.y + Player.PositionChange.y * 10; //Prb.velocity.y / 1.4f;


        //if(Mathf.Abs(PreferedPos.x - PlayerT.position.x) < MaxDistanceX)
        {
            PreferedPos.x = PlayerT.position.x + Prb.velocity.x / 2f;
        }
        //else PreferedPos.x = PlayerT.position.x + Prb.velocity.x / 1.4f;

        //if (Mathf.Abs(PreferedPos.y - PlayerT.position.y) < MaxDistanceY)
        {
            PreferedPos.y = PlayerT.position.y + Prb.velocity.y / 2f;
        }
        //else PreferedPos.y = PlayerT.position.y + Prb.velocity.y / 1.4f;




        if (Mathf.Abs(PreferedPos.x - CameraT.position.x) > MaxDistanceX)
        {
            CameraSpeedX = 2 * Mathf.Abs(PreferedPos.x - CameraT.position.x + MaxDistanceSpeedInc);
        }
        else CameraSpeedX = 2 * Mathf.Abs(PreferedPos.x - CameraT.position.x);

        if (Mathf.Abs(PreferedPos.y - CameraT.position.y) > MaxDistanceY)
        {
            CameraSpeedY = 2 * Mathf.Abs(PreferedPos.y - CameraT.position.y) + MaxDistanceSpeedInc;
        }
        else CameraSpeedY = 2 * Mathf.Abs(PreferedPos.y - CameraT.position.y);
    }
}
