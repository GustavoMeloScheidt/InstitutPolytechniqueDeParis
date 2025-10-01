using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float playerSpeed = 5; //public so we can view it from the inspector panel 
    public float horizontalSpeed = 5;
    public float rightLimit = 5.5f;
    public float leftLimit = -5.5f;
    [SerializeField] bool isRunning;

    
    void Update()
    {
        if (isRunning == false)
        {
            isRunning = true;
            StartCoroutine(AddDistance());
        }


        //only do forward * playerSpeed won't work,
        // as the speed needs to be relative to the world around it and the game speed
        // we can think of delta as the game speed, but we also need to make it relative to the world around it, so "Space.World"
        transform.Translate(Vector3.forward * Time.deltaTime * playerSpeed, Space.World);
        //here we add horizontal movement
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) //adding both "A" key and "Arrow key"
        {
            //going to use nesting to define limits/boundaries in the map
            if (this.gameObject.transform.position.x > leftLimit)
            {
                transform.Translate(Vector3.left * Time.deltaTime * horizontalSpeed);
            }
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            if (this.gameObject.transform.position.x < rightLimit)
            {
                transform.Translate(Vector3.right * Time.deltaTime * horizontalSpeed);
            }
        }

    }

    IEnumerator AddDistance()
    {
        yield return new WaitForSeconds(0.35f);
        MasterLevelInfo.distanceRun += 1;
        isRunning = false;
    }    
    
}
