using UnityEngine;
using System.Collections;

public class shooting : MonoBehaviour {
public Rigidbody projectile;
public Transform spawnpoint;
 
public Transform cube;
public Vector3 SpawnAngle; 
public Vector3 currentAngle; 

public bool CanShoot = true;
public Rigidbody clone;
public int time = 1;
public bool CanShoot2 = true; 
public int Change = 0;

// Use this for initialization
void Start () {
	
}

// Update is called once per frame
void Update () 
{
	//speed up the training session
	if(Change ==0)
		Time.timeScale = 20; 
	else 
		Time.timeScale = 2;

	if(CanShoot == true && CanShoot2 == true)
 	{
		
 		//clone the arrow and shoot based on spawnpoint rotation and position
		clone = (Rigidbody)Instantiate(projectile, spawnpoint.position, spawnpoint.rotation);
		currentAngle = spawnpoint.rotation.eulerAngles;
		clone.velocity = spawnpoint.TransformDirection (Vector3.forward*40);
		CanShoot = false;
		
		//destroy the arrow
		StartCoroutine(waitfor(clone));
	} 

	
}

IEnumerator waitfor(Rigidbody clone)
{

		yield return new WaitForSeconds(1f);
		Destroy(clone.gameObject);
		CanShoot = true;
		
}

}