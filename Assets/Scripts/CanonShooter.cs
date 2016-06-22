using UnityEngine;
using System.Collections;

public class CanonShooter : MonoBehaviour {

    public GameObject canonBallPrefab;
    
    public GameObject canon;
    public GameObject mouth;
    
    public float canonBallSpeed = 50f;

	// Use this for initialization
	void Start () {
	   //InvokeRepeating("shoot", 0f, 1F);
	}
	
	// Update is called once per frame
	void Update () {
	   if (Input.GetKey("space"))
       {
           shoot();
       }
	}
    
    void shoot()
    {
        GameObject canonBall = Instantiate(canonBallPrefab, mouth.transform.position, Quaternion.identity) as GameObject;
        canonBall.SetActive(true);
        
        canonBall.transform.localScale = new Vector3(1f,1f,1f);
        
        Vector3 canonBallDirection = mouth.transform.position - canon.transform.position;
        canonBallDirection.Normalize();
        canonBall.GetComponent<Rigidbody>().velocity = canonBallDirection*canonBallSpeed;
    }
}
