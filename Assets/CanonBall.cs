using UnityEngine;
using System.Collections;

public class CanonBall : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	   if (transform.position.y < -80f)
       {
           Destroy(this.gameObject);
       }
	}
}
