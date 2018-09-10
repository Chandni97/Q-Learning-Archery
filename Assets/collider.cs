using System.Collections;
using System.Collections.Generic;
using UnityEngine;  
using System; 
using UnityEngine.UI;


public class collider : MonoBehaviour {

	struct Point
 	{
     	public double x, y;
 	} 

 	// public Button m_YourFirstButton;


 	//coordinates of the goal
	double goalX = 17.07; 
	double goalY = -0.66;  

	//check if input is received
	bool input = false;
	
	int iterator = 0;

	List<Vector3> QRow = new List<Vector3>(); // store the states
	List<float[]> Q = new List<float[]>();  //store the q values for all 4 directions
	List<int[]> allowedPaths = new List<int[]>(); // track the allowed paths
	shooting shoot; // refernce to the shooting script
	public GameObject arrowGameObject; 

    

	public Transform bow; 

	Vector3 InputAngles = new Vector3();
	Vector3 angles = new Vector3();
	Vector3 anglesBow = new Vector3();
	int previousStateIndex;
	int previousAction = -1;
	Vector3 PreviousAngles = new Vector3();
	float gamma = 0.8f;

	bool skipNext = false;
	public Rigidbody arrow;
	int trainingDone = 0; 

	bool test = false; 

	int inputTraining = 0;  
	bool input2 = false;
	bool buttoninc;

	

	void Start () {
		
		//refer to the shooting.cs
		shoot = arrowGameObject.GetComponent<shooting>();
		//get the initial angles
		InputAngles = shoot.spawnpoint.rotation.eulerAngles;

		print("arrow");
		

	}
	
	// Update is called once per frame
	void Update () {
		

		//get the keyboard input, change the angle of the spawn point accordingly and shoot the arrow
		if (Input.GetKeyUp("up"))
        {
            print("ups was pressed"); 
              
           	InputAngles.x -= 2.0f; 
           	shoot.CanShoot2 = false;
           	shoot.Change = 1;
            shoot.spawnpoint.rotation = Quaternion.Euler(InputAngles);
       		shoot.clone = (Rigidbody)Instantiate(shoot.projectile, shoot.spawnpoint.position, shoot.spawnpoint.rotation);
			shoot.clone.velocity = shoot.spawnpoint.TransformDirection (Vector3.forward*40);
			
			 //destroy the arrow after a while
			StartCoroutine(waitfor(shoot.clone));

        
        } 

       if(Input.GetKeyUp("down"))
        {
            print("down was pressed"); 
             
              InputAngles.x += 2.0f;  
               shoot.CanShoot2 = false; 
               shoot.Change = 1;
             shoot.spawnpoint.rotation = Quaternion.Euler(InputAngles);
       		shoot.clone = (Rigidbody)Instantiate(shoot.projectile, shoot.spawnpoint.position, shoot.spawnpoint.rotation);
			shoot.clone.velocity = shoot.spawnpoint.TransformDirection (Vector3.forward*40);
		
			StartCoroutine(waitfor(shoot.clone));  
            
        } 

        if (Input.GetKeyUp("right"))
        {
            print("right was pressed"); 
            InputAngles.y += 2.0f; 
             shoot.CanShoot2 = false; 
            shoot.Change = 1;
             shoot.spawnpoint.rotation = Quaternion.Euler(InputAngles);
       		shoot.clone = (Rigidbody)Instantiate(shoot.projectile, shoot.spawnpoint.position, shoot.spawnpoint.rotation);
			shoot.clone.velocity = shoot.spawnpoint.TransformDirection (Vector3.forward*40);
			
			
			StartCoroutine(waitfor(shoot.clone));
            
        } 
		if (Input.GetKeyUp("left"))
        {
            print("left was pressed");  
             InputAngles.y -= 2.0f; 
             shoot.CanShoot2 = false;  
            shoot.Change = 1;
            shoot.spawnpoint.rotation = Quaternion.Euler(InputAngles);
       		shoot.clone = (Rigidbody)Instantiate(shoot.projectile, shoot.spawnpoint.position, shoot.spawnpoint.rotation);
			shoot.clone.velocity = shoot.spawnpoint.TransformDirection (Vector3.forward*40);
			 
			StartCoroutine(waitfor(shoot.clone));
           

            
        }  

        //end of the input
        if(Input.GetKeyUp("space")) 
        {
        	input = true;
        	inputTraining = 1; 
        	input2 = true;
        	shoot.spawnpoint.rotation = Quaternion.Euler(InputAngles); 
            shoot.Change = 1;
        	shoot.CanShoot2 = true; 
        	shoot.CanShoot = true;
        }
        
	}

	void OnCollisionEnter(Collision col)
	{

	  
		test = true;

		shoot.Change = 0;

		//check if the training has ended
		if(iterator == 2000 )
		{
			print("training IS done"); 
			goToGoal(col);
		} 

		
		else if(col.gameObject.name.Contains("arrow") )
		{

			shoot = arrowGameObject.GetComponent<shooting>();
			shoot.clone.isKinematic = true; //prevent the arrow from falling
			

			//get the x and y coordinates of the collision point and calcuate its distance from the goal
			double collideX = col.contacts [0].point.x; 
			double collideY = col.contacts [0].point.y; 

			double a = (double)(goalX - collideX);
			double b = (double)(goalY - collideY); 
			double distance = System.Math.Sqrt (a * a + b * b);

			print("X : " + collideX + "Y: " + collideY);

			//prevent thh arrow from crossing the boundaries of the wall
			if(collideX > 23.0 || collideX < 10 || collideY > 3.0  || collideY < -5)
			{
				if(previousAction != -1)
				{
					print("ON THE BORDER");

					angles = PreviousAngles;
					allowedPaths[previousStateIndex][previousAction] = 0;
					shoot.spawnpoint.rotation = Quaternion.Euler(angles);
					skipNext = true;
				}

			}
			 else
			 {

			 	//store the current angle of the spawn point
			 	angles = shoot.spawnpoint.rotation.eulerAngles;
				PreviousAngles = angles;

				//get the reward for the action
			 	int reward = getReward(distance);
			 	if(reward >= 90)
			 	{
			 		print("GOALS IS CLOSE");
			 		print(angles);
			 	}

				//check if this angles exist in the QRow else add it
				int index = checkForAngle(angles);
				 if( index == -1)
				{
				 	QRow.Add(angles);
				 	float[] rewards = new float[4];
				 	int[] allowed = {1,1,1,1};
				 	allowedPaths.Add(allowed);
				 	Q.Add(rewards);
				 	index = QRow.Count - 1;
				 }

				 //check for the next valid action to be taken randomly
			 	int valid = 0;
				int angle = -1;
				while(valid == 0)
				{
					angle = UnityEngine.Random.Range(1, 5); 
					valid = allowedPaths[index][angle-1];
				}
				print("angle : " + angle);

				//change the angle of the spawn point according to the random number genrated
				
				 if(angle == 1)
				 {
				 	angles.y += 2.0f;
				 }
				 if(angle == 2)
				 {
				 	angles.y -= 2.0f;
				 }
				 if(angle == 3)
				 {
				 	angles.x += 2.0f;
				 }
				 else
				 {
				 	angles.x -= 2.0f;
				 }

				 shoot.spawnpoint.rotation = Quaternion.Euler(angles);
				
				 //caluclate the q value
			 	if(previousAction != -1 && skipNext == false)
			 	{
			 		Q[previousStateIndex][previousAction] = reward + ( gamma * maxQ(index) );

			 	}
			 	else
			 	{
			 		skipNext = false;
			 	}

			 	//store the current state and action as previous state and action to be used in the next iteraiton
				previousStateIndex = index;
				previousAction = angle - 1;
				
				//print the q matrix
				print("X collision : " + collideX + " y collision : " + collideY + " action : " + (angle - 1));
				printQ();
				print(iterator + "-----------------------------------------------");

				iterator ++;
			}

			


		} 
	} 

	//destroy the arrow
	IEnumerator waitfor(Rigidbody clone)
	{
		yield return new WaitForSeconds(1f);
		Destroy(clone.gameObject);
		shoot.CanShoot = false; 
	}

	//try and go to the goal
	void goToGoal(Collision col)
	{
		//calculate the distance between collision point and goal
		double collideX = col.contacts [0].point.x; 
		double collideY = col.contacts [0].point.y; 

		double a = (double)(goalX - collideX);
		double b = (double)(goalY - collideY); 
		double distance = System.Math.Sqrt (a * a + b * b);

		//make sure that the arrow is able to reach the goal within 20 steps - it should not take longer because of the size of the wall 
		//keep training if thats the case
		if(trainingDone == 20)
		{
			iterator -= 500;
			trainingDone = 0;  

			if(inputTraining ==1) 
				inputTraining = 2;
		}

		//check if input has received from the user
		if(input2 ==true && inputTraining == 2) 
		{ 
			angles = InputAngles; 
			inputTraining = 1;
			input = true;

		}

		if(distance > 1)
		{
			if(!input) 
			{ 
				//reach the goal from current spawn point position
				angles = shoot.spawnpoint.rotation.eulerAngles;
			} 

			else 
			{ 
				//reach the goal from input angles
				angles = InputAngles; 
				input = false; 
				input2 = true;
			}
			
			//check if the current state exist in the q matrix
			int index = checkForAngle(angles);
			if(index == -1)
			{
				iterator -= 500;
				trainingDone = 0;
				if(inputTraining ==1) 
					inputTraining = 2;
				return;

			}

			//find the index/action with the maxQ value for the current state
			int indexMaxQ = maxQIndex(index);

			//change angle accordingly
			if(indexMaxQ == 0)
			{
				angles.y +=2.0f;
				
			}
			if(indexMaxQ == 1)
			{
				angles.y -=2.0f;
				
			}
			if(indexMaxQ == 2)
			{
				angles.x +=2.0f;
				
			}
			if(indexMaxQ == 3)
			{
				angles.x -=2.0f;
				
			}

			trainingDone += 1;
			shoot.spawnpoint.rotation = Quaternion.Euler(angles);
			
		}
		else
		{
			//if the goal is reached ask for user input
			print("the goal is : ");
			print(angles);
			print("Enter Input");
			

		}
		
	} 


	//check if angle exist in the q matrix
	int checkForAngle(Vector3 angle)
	{
		for(int i=0; i<QRow.Count; i++)
		{
	 		float dis = Vector3.Distance(angle, QRow[i]); // Calculating Distance
	 		if(dis < 0.5) // checking if distance is less than required distance.
	 		{
	 			return i;
	 		}
 		}

 		return -1;
	}


	//get reward for the distance
	int getReward(double distance)
	{
		int reward = -1; 
		if (distance >= 8)
			reward = -5; 
		else if (distance < 8 && distance >= 6.5)
			reward = 10;  
		else if (distance < 6.5 && distance >= 6.0)
			reward = 15; 
		else if (distance < 6.0 && distance >= 5.0)
			reward = 20; 
		else if (distance < 5.0 && distance >= 4.5)
			reward = 25; 
		else if (distance < 4.5 && distance >= 4.0)
			reward = 30; 
		else if (distance < 4.0 && distance >= 3.5)
			reward = 35; 
		else if (distance < 3.5 && distance >= 3.0)
			reward = 45; 
		else if (distance < 3.0 && distance >= 2.5)
			reward = 55; 
		else if (distance < 2.5 && distance >= 2.0)
			reward = 65; 
		else if (distance < 2.0 && distance >= 1.5)
			reward = 75; 
		else if (distance < 1.5 && distance >= 1.0)
			reward = 80; 
		else if (distance < 1.0 && distance >= 0.5)
			reward = 90; 
		else if (distance < 0.5 && distance >= 0)
			reward = 100; 

		return reward;
	}

	//calculate the max q value for given state
	float maxQ(int index)
	{
		float max = Q[index][0];
		for(int i=1; i<4; i++)
		{
			if(Q[index][i] > max)
				max = Q[index][i];
		}

		return max;
	}

	//calculate index of the max q value for the given state
	int maxQIndex(int index)
	{
		float max = Q[index][0];
		int maxIndex = 0;
		for(int i=1; i<4; i++)
		{
			if(Q[index][i] > max)
			{
				max = Q[index][i];
				maxIndex = i;
			}
		}

		return maxIndex;
	}

	//print q value
	void printQ()
	{
		for(int i=0; i<Q.Count; i++)
		{
			print(QRow[i] + ":" + Q[i][0] + " " + Q[i][1] + " " + Q[i][2] + " " + Q[i][3]);
		}
	}
		
}
