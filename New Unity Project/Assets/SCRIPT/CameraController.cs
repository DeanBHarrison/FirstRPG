using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public Transform target;

    public Tilemap theMap;
    private Vector3 bottomLeftLimit;
    private Vector3 topRightLimit;

    private float halfHeight;
    private float halfWidth;

    public int musicToPlay;
    private bool musicStarted;

    // Start is called before the first frame update

    void Start()

    {
        //this works in combination with "transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);" in update, if i remove this it keeps the camera from following
        // my character only when loaded into a new scene

        // target = PlayerController.instance.transform;
        //target = FindObjectOfType<PlayerController>().transform;

        //this gives and object reference error when it's ran from here, even though it works for the teacher, i've moved it down to update and now it works 
        //PlayerController.instance.Setbounds(theMap.localBounds.min, theMap.localBounds.max); 

        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;

        bottomLeftLimit = theMap.localBounds.min + new Vector3(halfWidth, halfHeight, 0f);
        topRightLimit = theMap.localBounds.max + new Vector3(-halfWidth, -halfHeight, 0f);

        
    }

    // Update is called once per frame
    void Update()
    {
        target = FindObjectOfType<PlayerController>().transform;
        // had to use this here see above comment
        FindObjectOfType<PlayerController>().Setbounds(theMap.localBounds.min, theMap.localBounds.max);

        transform.position = new Vector3(target.position.x, target.position.y, transform.position.z);

        
        //keep the camera inside the bounds
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, bottomLeftLimit.x, topRightLimit.x), Mathf.Clamp(transform.position.y, bottomLeftLimit.y, topRightLimit.y), transform.position.z);

        if(!musicStarted)
        {
            musicStarted = true;
            AudioManager.instance.PlayBGM(musicToPlay);
        }
    }
}
