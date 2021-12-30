using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class Player : MonoBehaviour
{
    public float speed;
    public float jumpHeight;

    public Collider2D groundCheck;

    public float maxDistance;
    float distance;
    public LayerMask hookLayer;
    public LayerMask groundLayer;
    Camera cam;

    public Transform playerT;
    Rigidbody2D rb;

    LineRenderer line;

    float isPulling = 0;

    public float pullSlower;
    public float pullDuration;

    float weight = 10;
    Transform hookT;
    Rigidbody2D hookRb;
    Vector3 hookOffset;
    Entity hookEntity;

    bool hookEnabled;
    float holdingE = 0;

    RaycastHit2D hit = new RaycastHit2D();


    // Start is called before the first frame update
    void Start()
    {
        playerT = GetComponent<Transform>();
        rb = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        line = GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        //enable / disable hook
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!hookEnabled)
            {

                hit = Physics2D.Raycast(transform.position, cam.ScreenToWorldPoint(Input.mousePosition) - transform.position, maxDistance, hookLayer);
                if (hit.collider != null)
                {
                    hookRb = hit.collider.GetComponent<Rigidbody2D>();
                    hookT = hit.collider.GetComponent<Transform>();
                    hookOffset = hit.point - (new Vector2(hookT.position.x, hookT.position.y));
                    hookEntity = hit.collider.GetComponent<Entity>();

                    distance = Vector2.Distance(playerT.position, hookT.position + hookOffset);
                    hookEnabled = true;
                    //wait with enabling the line untill the positions have been updated
                    //to avoid after image, for htis make a bool to check if its on instead.
                    line.enabled = true;
                }
            }
            else
            {
                holdingE = 0;
                isPulling = 1;
                //hookEnabled = !hookEnabled;
                //line.enabled = false;
            }
        }

        //jump
        if (Input.GetKeyDown(KeyCode.Space) && groundCheck.IsTouchingLayers(groundLayer))
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpHeight);
        }
    }



    void FixedUpdate()
    {
        //perhaps gjør sånn at det tar lengre tid når du først enable den så det ikke blir irriterende?
        //check om man fortsatt holder inn E
        //muligens ha all input i update?
        if (hookEnabled && isPulling == 0)
        {
            if (Input.GetKey(KeyCode.E))
            {
                holdingE += 1;
                if (holdingE > 10)
                {
                    isPulling = 1;
                    holdingE = 0;
                }
            }
        }

        //Constrain
        if (hookEnabled && distance < Vector2.Distance(playerT.position, hookT.position + hookOffset))
        {
            Vector2 hookDirection = (hookT.position + hookOffset - playerT.position).dir();
            float hookDistance = (Vector2.Distance(playerT.position, hookT.position + hookOffset) - distance);
            Vector3 force = (hookDirection * hookDistance);
                            
            if (hookEntity == null || hookRb == null)
            {
                RaycastHit2D playerRay = Physics2D.Raycast(transform.position, hookDirection, hookDistance, hookLayer);
                if(playerRay.collider == null)
                {
                    playerT.position += force;
                    rb.velocity += force.toVector2() * 10;
                }
                else
                {
                    playerT.position += (Vector3)(playerRay.point - playerT.position.toVector2());
                    rb.velocity += (playerRay.point - playerT.position.toVector2()) * 10;
                }
            }
            else
            {
                float totalWeight = weight + hookEntity.weight;

                RaycastHit2D playerRay = Physics2D.Raycast(transform.position, hookDirection, hookDistance / totalWeight * hookEntity.weight, hookLayer);
                RaycastHit2D hookRay = Physics2D.Raycast(transform.position, -hookDirection, hookDistance / totalWeight * weight, hookLayer);

                if (playerRay.collider == null)
                {
                    playerT.position += force / totalWeight * hookEntity.weight;
                    rb.velocity += force.toVector2() * 10 / totalWeight * hookEntity.weight;
                }
                else
                {
                    playerT.position += (Vector3)(playerRay.point-playerT.position.toVector2());
                    rb.velocity += (playerRay.point - playerT.position.toVector2()) * 10;
                }
                if (hookRay.collider == null)
                {
                    hookT.position -= force / totalWeight * weight;
                    hookRb.velocity -= force.toVector2() * 10 / totalWeight * weight;
                }
                else
                {
                    hookT.position -= (Vector3)(hookRay.point - hookT.position.toVector2());
                    hookRb.velocity -= (hookRay.point - hookT.position.toVector2()) * 10;

                }
            }
        }

        //pull
        if(isPulling != 0)
        {
            isPulling += 1;
            distance -= isPulling/pullSlower;

            if(distance < 0.3 || isPulling > pullDuration || !(Input.GetKey(KeyCode.E)))
            {
                isPulling = 0;
                if (holdingE < 4 || distance > 55)
                {
                    hookEnabled = false;
                    line.enabled = false;
                }
                else holdingE = 0;
            }
        }

        //render line
        if (hookEnabled)
        {
            line.SetPosition(0, playerT.position);
            line.SetPosition(1, hookT.position + hookOffset);
        }

        /*

               * * * * * * * * 
          * *                  * *
        *                          *

        */
        float hookTipheight()
        {
            float length = 1;
            float upForce = 1;
            float gravity = 1;
            float height = 1;
            float startPos = 1;

            return (length * 2 / upForce - Mathf.Pow(length, 2) / gravity) / height + startPos;

        }

        //movement
        if (Input.GetKey(KeyCode.D) && groundCheck.IsTouchingLayers(groundLayer))
        {
            rb.velocity = new Vector2(speed, rb.velocity.y);
        }
        if (Input.GetKey(KeyCode.A) && groundCheck.IsTouchingLayers(groundLayer))
        {
            rb.velocity = new Vector2(-speed, rb.velocity.y);
        }

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(playerT.position, maxDistance);
    }

    

    /*private void FixedUpdate()
    {
        //GUIUtility.ExitGUI();
    }*/
}

static class extenstions
{
    /// <summary>
    /// Makes a Vector2 to a direction.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    public static Vector3 dir(this Vector2 vector)
    {
        float a = Mathf.Abs(vector.x) + Mathf.Abs(vector.y);
        return new Vector3(vector.x / a, vector.y / a);
    }
    public static Vector3 dir(this Vector3 vector)
    {
        float a = Mathf.Abs(vector.x) + Mathf.Abs(vector.y);
        return new Vector3(vector.x / a, vector.y / a);
    }
    public static Vector2 toVector2(this Vector3 vector3){
        return new Vector2(vector3.x, vector3.y);
    }
}
