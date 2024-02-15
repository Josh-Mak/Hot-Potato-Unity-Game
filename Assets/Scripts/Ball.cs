using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Ball : MonoBehaviour
{

    public Rigidbody2D rb;
    public LayerMask Terrain;
    public GameObject Goal;

    public Vector2 startingPosition = new Vector2(27f, 0f);

    public bool hitFloor = false;
    public bool hitGoal = false;

    private float breakableWallSpeedThreshold = 40f;

    public void ResetBall()
    {
        this.transform.position = startingPosition;  // whatever are our starting coordinates for the current level.
        this.rb.velocity = Vector2.zero;
        this.rb.isKinematic = true;
        hitFloor = false;
        hitGoal = false;
    }

    public void ActivateBall()
    {
        this.rb.isKinematic = false;
    }

    // Start is called before the first frame update
    void Start()
    {
        this.transform.position = startingPosition;
        this.rb.velocity = Vector2.zero;
        this.rb.isKinematic = true;
        hitGoal = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & Terrain) != 0)  // a bitwise comparison, complicated >:(
        {
            Vector2 normal = collision.contacts[0].normal;
            float angle = Vector2.Angle(normal, Vector2.up);

            if (angle < 45 && !hitGoal)
            {
                hitFloor = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == Goal)
        {
            hitGoal = true;   
        }
        if (collision.tag == "Breakable Wall")
        {
            float ballSpeed = rb.velocity.magnitude;
            if (ballSpeed >= breakableWallSpeedThreshold)
            {
                collision.gameObject.SetActive(false);
                // play some animation
            }
        }
    }
}
