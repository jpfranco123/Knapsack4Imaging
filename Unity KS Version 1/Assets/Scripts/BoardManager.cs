using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;


// This Script (a component of Game Manager) Initializes the Borad (i.e. screen).
public class BoardManager : MonoBehaviour {

	//66. Delete Count Class if is not used.
	[Serializable]
	public class Count
	{
		public int minimum;
		public int maximum;

		public Count(int min, int max)
		{
			minimum=min;
			maximum=max;
		}
	}

	//Resoultion width and Height
	public int resolutionWidth = 800;
	public int resolutionHeight = 600;

	//Number of Columns and rows of the grid (the possible positions of the items).
	public int columns = 8;
	public int rows = 6;

	//Prefab of the item interface configuration
	public static GameObject KSItemPrefab;

	//A canvas where all the board is going to be placed
	private GameObject canvas;

	//The item radius. This is used to avoid superposition of items.
	public float KSItemRadius = 1.5f;

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
			btnLeft.onClick.AddListener(()=>GameManager.changeToNextTrial(2,0));
			btnRight.onClick.AddListener(()=>GameManager.changeToNextTrial(2,1));
		} else {
			btnLeft.GetComponentInChildren<Text>().text = "Yes";
			btnRight.GetComponentInChildren<Text>().text = "No";
			btnLeft.onClick.AddListener(()=>GameManager.changeToNextTrial(2,1));
			btnRight.onClick.AddListener(()=>GameManager.changeToNextTrial(2,0));
		}
	}

	//Initializes the instance for this trial:
	//1. Sets the question string using the instance (from the .txt files)
	//2. The weight and value vectors are uploaded
	//3. The instance prefab is uploaded
	void setKSInstance(){
		int randInstance = GameManager.instanceRandomization[GameManager.trial];

		Text Quest = GameObject.Find("Question").GetComponent<Text>();

		String question = "Can you get at least $" + GameManager.ksinstances[randInstance].profit + " using at most " + GameManager.ksinstances[randInstance].capacity +"kg?";

		Quest.text = question;

		//123 Currently saving the new items on KSItems so they are randomized as before
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

					//66: SImplify expression
					if (objectOverlapsQ (randomPosition)) {
					
					} else {
						generateItem (i, randomPosition);
						objectPositioned = 1;
					}
				}
				else{
					//66. If not all are able to be located try several times and if still not: Show an error
					Debug.Log ("Not enough space to place all items");
					return false;
				}
			}

		}
		return true;
	}

	//Macro function that initializes the Board
	public void SetupScene()
	{
		InitialiseList();
		RandomizeButtons ();
		setKSInstance ();

		//66 Generate: if the bool return by the following function is false, then retry again.
		LayoutObjectAtRandom();

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