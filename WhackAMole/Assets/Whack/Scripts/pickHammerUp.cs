using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickHammerUp : MonoBehaviour
{
    GameObject startTrans;
    Transform controller;
    bool pickedUp = false;
    char hand;


    public Quaternion RotationOffset;
    public Vector3 tempVec; 
    // Start is called before the first frame update
    void Start()
    {
        startTrans = new GameObject();

        startTrans.transform.position = transform.position;
        startTrans.transform.rotation = transform.rotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (hand == 'R')
        {
            if (OVRInput.Get(OVRInput.Button.Two) && GameManager.RightHandInUse)
            {
                GameManager.RightHandInUse = false;
                pickedUp = false;
                hand = ' ';
            }

        }
        if (hand == 'L')
        {
            if (OVRInput.Get(OVRInput.Button.Four) && GameManager.LeftHandInUse)
            {
                GameManager.LeftHandInUse = false;
                pickedUp = false;
                hand = ' ';
            }
        }



        if (pickedUp == true)
        {
            //transform = controller;

            transform.position = controller.position;

            RotationOffset.eulerAngles = tempVec;

            transform.rotation = controller.rotation *  RotationOffset;


        }
        else //remove/move!?
        {
            transform.position = startTrans.transform.position;
            transform.rotation = startTrans.transform.rotation;
        }


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "RightHand" && GameManager.RightHandInUse == false)
        {
            pickedUp = true;
            this.controller = other.gameObject.transform;
            GameManager.RightHandInUse = true;
            hand = 'R';
        }

        if (other.gameObject.tag == "LeftHand" && GameManager.LeftHandInUse == false)
        {
            pickedUp = true;
            this.controller = other.gameObject.transform;
            GameManager.LeftHandInUse = true;
            hand = 'L';
        }

    }
}