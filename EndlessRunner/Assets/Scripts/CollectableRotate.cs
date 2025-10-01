using UnityEngine;

public class CollectableRotate : MonoBehaviour
{
    // Script to make the collectables rotate 
    [SerializeField] float rotateSpeed = 1;
    void Update()
    {
        transform.Rotate(0, rotateSpeed, 0, Space.World);
    }
}
