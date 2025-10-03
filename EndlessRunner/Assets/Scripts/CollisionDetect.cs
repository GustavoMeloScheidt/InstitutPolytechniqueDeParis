using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CollisionDetect : MonoBehaviour
{
    // This script is going to detect collisions and play the animation
    [SerializeField] GameObject thePlayer;
    [SerializeField] GameObject playerAnimation;
    [SerializeField] AudioSource collisionFX;
    [SerializeField] GameObject mainCamera;
    [SerializeField] GameObject fadeOut;
    [SerializeField] AudioSource failSound;



    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(CollisionEnd());
    }

    IEnumerator CollisionEnd()
    {
        collisionFX.Play();
        failSound.Play();
        thePlayer.GetComponent<PlayerMovement>().enabled = false; //to stop running
        playerAnimation.GetComponent<Animator>().Play("Stumble Backwards");
        mainCamera.GetComponent<Animator>().Play("CollisionCamera");
        yield return new WaitForSeconds(0);
        fadeOut.SetActive(true);
        yield return new WaitForSeconds(4.5f);
        SceneManager.LoadScene(5); //go to the game over scene when we die

    }
}
