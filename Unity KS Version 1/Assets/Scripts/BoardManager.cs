using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour {

	//Resoultion width and Height
	public static int resolutionWidth = 800;
	public static int resolutionHeight = 600;

	//Number of Columns and rows of the grid (the possible positions of the items).
	public static int columns = 8;
	public static int rows = 6;

	//The item radius. This is used to avoid superposition of items.
	public static float KSItemRadius = 1.5f;

	//Prefab of the item interface configuration
	public static GameObject KSItemPrefab;

	//A canvas where all the board is going to be placed
	private GameObject canvas;

	//The possible positions of the items;
	private List <Vector3> gridPositions = new List<Vector3> ();

	//Weights and value vectors for this trial. CURRENTLY SET UP TO ALLOW ONLY INTEGERS.
	//ws and vs must be of the same length
	private int[] ws;
	private int[] vs;


	//This Initializes the GridPositions
	void InitialiseList()
	{
		gridPositions.Clear ();

		for(int x=1;x<columns+1;x++)
		{
			for ( int y =1; y<rows+1;y++)
			{	
				float xUnit =(float) (resolutionWidth / 100)/columns;
				float yUnit =(float) (resolutionHeight / 100)/rows;
				gridPositions.Add(new Vector3(x*xUnit,y*yUnit,0f));

			}
		}
	}

	//Randomizes YES/NO button positions (left or right) and allocates corresponding script to save the correspondent answer.
	void RandomizeButtons(){
		Button btnLeft = GameObject.Find("LEFTbutton").GetComponent<Button>();
		Button btnRight = GameObject.Find("RIGHTbutton").GetComponent<Button>();

		int randomYes=Random.Range(0,2);

		if (randomYes == 1) {
			btnLeft.GetComponentInChildren<Text>().text = "No";
			btnRight.GetComponentInChildren<Text>().text = "Yes";
			btnLeft.onClick.AddListener(()=>GameManager.changeToNextScene(0));
			btnRight.onClick.AddListener(()=>GameManager.changeToNextScene(1));
		} else {
			btnLeft.GetComponentInChildren<Text>().text = "Yes";
			btnRight.GetComponentInChildren<Text>().text = "No";
			btnLeft.onClick.AddListener(()=>GameManager.changeToNextScene(1));
			btnRight.onClick.AddListener(()=>GameManager.changeToNextScene(0));
		}
	}

	//Initializes the instance for this trial:
	//1. Sets the question string using the instance (from the .txt files)
	//2. The weight and value vectors are uploaded
	//3. The instance prefab is uploaded
	void setKSInstance(){
		int randInstance = GameManager.instanceRandomization[GameManager.trial-1];

		Text Quest = GameObject.Find("Question").GetComponent<Text>();

		String question = "Can you get at least $" + GameManager.ksinstances[randInstance].profit + " using at most " + GameManager.ksinstances[randInstance].capacity +"kg?";

		Quest.text = question;

		//66 Carefull: Make sure that KSItems is erased and started in each trial. Static?

		ws = GameManager.ksinstances [randInstance].weights;
		vs = GameManager.ksinstances [randInstance].values;

		KSItemPrefab = (GameObject)Resources.Load ("KSItem");

	}

	//Intsantiates an Item
	void generateItem(int itemNumber ,Vector3 randomPosition){

		GameObject instance = Instantiate (KSItemPrefab, randomPosition, Quaternion.identity) as GameObject;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		instance.transform.position = randomPosition;

		instance.GetComponentInChildren<Text>().text = ws[itemNumber]+ "Kg \n $" + vs[itemNumber];

	}


	//Retunrs a random position from the grid and removes the item from the list.
	Vector3 RandomPosition()
	{
		int randomIndex=Random.Range(0,gridPositions.Count);
		Vector3 randomPosition = gridPositions[randomIndex];
		gridPositions.RemoveAt(randomIndex);
		return randomPosition;
	}

	//Places all the objects from the instance (ws,vs) on the canvas. 
	// Returns TRUE if all items where positioned, FALSE otherwise.
	bool LayoutObjectAtRandom()
	{
		int objectCount =ws.Length;
		for(int i=0; i < objectCount;i++)
		{
			int objectPositioned = 0;
			while (objectPositioned == 0) 
			{
				if (gridPositions.Count > 0) {
					Vector3 randomPosition = RandomPosition ();

					if (!objectOverlapsQ (randomPosition)) {
						generateItem (i, randomPosition);
						objectPositioned = 1;
					} 
//					else {
//						generateItem (i, randomPosition);
//						objectPositioned = 1;
//					}
				}
				else{
					//Debug.Log ("Not enough space to place all items");
					return false;
				}
			}

		}
		return true;
	}

	//Macro function that initializes the Board
	public void SetupScene(int sceneToSetup)
	{
		if (sceneToSetup == 1) {
			//InitialiseList();
			setKSInstance ();

			//If the bool returned by LayoutObjectAtRandom() is false, then retry again:
			//Destroy all items. Initialize list again and try to place them once more.
			int nt=100;
			bool itemsPlaced = false;
			while (nt >= 1 && !itemsPlaced) {

				GameObject[] items = GameObject.FindGameObjectsWithTag("Item");
				foreach (GameObject item in items)
				{
					Destroy(item);
				}

				InitialiseList ();
				itemsPlaced = LayoutObjectAtRandom ();
				nt--;
				Debug.Log (nt);
			}
			if (itemsPlaced == false) {
				Debug.Log ("Not enough space to place all items");
			}

		} else if(sceneToSetup ==2){
			setKSInstance ();
			RandomizeButtons ();
		}

	}

	//Checks if positioning an item in the new position generates an overlap. Assuming the new item has a radius of KSITemRadius.
	//Returns: TRUE if there is an overlap. FALSE Otherwise.
	bool objectOverlapsQ(Vector3 pos)
	{
		//If physics could be started before update we could use the following easier function:
		//bool overlap = Physics2D.IsTouchingLayers(newObject.GetComponent<Collider2D>());

		bool overlap = Physics2D.OverlapCircle(pos,KSItemRadius);
		return overlap;

	}


	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}
}