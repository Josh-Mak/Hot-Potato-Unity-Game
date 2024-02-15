using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneCamera : MonoBehaviour
{

    public float followSpeed = 8f;
    public Transform target;
    public float maxLeft = 0f;
    public float maxRight = 200f;
    public float maxUp = 200f;
    public float maxDown = 0f;

    public float yOffset = 5f;
    public float xOffset = 0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 newPos = new Vector3(target.position.x + xOffset, target.position.y + yOffset, -10f);
        if (newPos.x > maxRight)
        {
            newPos.x = maxRight;
        }
        else if (newPos.x < maxLeft)
        {
            newPos.x = maxLeft;
        }
        if (newPos.y > maxUp)
        {
            newPos.y = maxUp;
        }
        else if (newPos.y < maxDown)
        {
            newPos.y = maxDown;
        }
        
        transform.position = Vector3.Slerp(transform.position, newPos, followSpeed * Time.deltaTime);
    }
}
