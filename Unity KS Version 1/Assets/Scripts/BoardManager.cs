using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

	//Timer width
	public static float timerWidth =300;

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

	//66: WHY IF I CHANGE THE FOLLOWING vars to non-static i cant change them from a function?

	//If randomization of buttons:
	//1: No/Yes 0: Yes/No
	public static int randomYes;//=Random.Range(0,2);

	private String question;

	//Should the key be working?
	public static bool keysON = false;


	private static float minAreaBill = 1f;

	private static float minAreaWeight = 1f;

	private static int totalAreaBill = 7;

	private static int totalAreaWeight = 7;




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

		randomYes=GameManager.buttonRandomization[GameManager.trial-1];
			//Random.Range(0,2);
//		Debug.Log("RandomYesInterface");
//		Debug.Log(randomYes);

		if (randomYes == 1) {
			btnLeft.GetComponentInChildren<Text>().text = "No";
			btnRight.GetComponentInChildren<Text>().text = "Yes";
			//btnLeft.onClick.AddListener(()=>GameManager.changeToNextScene(0));
			//btnRight.onClick.AddListener(()=>GameManager.changeToNextScene(1));
		} else {
			btnLeft.GetComponentInChildren<Text>().text = "Yes";
			btnRight.GetComponentInChildren<Text>().text = "No";
			//btnLeft.onClick.AddListener(()=>GameManager.changeToNextScene(1));
			//btnRight.onClick.AddListener(()=>GameManager.changeToNextScene(0));
		}
	}

	//Initializes the instance for this trial:
	//1. Sets the question string using the instance (from the .txt files)
	//2. The weight and value vectors are uploaded
	//3. The instance prefab is uploaded
	void setKSInstance(){
		int randInstance = GameManager.instanceRandomization[GameManager.trial-1];

//		Text Quest = GameObject.Find("Question").GetComponent<Text>();
//
//		String question = "Can you obtain at least $" + GameManager.ksinstances[randInstance].profit + " with at most " + GameManager.ksinstances[randInstance].capacity +"kg?";
//
//		Quest.text = question;

		question = "Can you obtain at least $" + GameManager.ksinstances[randInstance].profit + " with at most " + GameManager.ksinstances[randInstance].capacity +"kg?";



		ws = GameManager.ksinstances [randInstance].weights;
		vs = GameManager.ksinstances [randInstance].values;

		//KSItemPrefab = (GameObject)Resources.Load ("KSItem");

		KSItemPrefab = (GameObject)Resources.Load ("KSItem3");


	}

	//Shows the question on the screen
	public void setQuestion(){
		Text Quest = GameObject.Find("Question").GetComponent<Text>();
		Quest.text = question;
	}


	//Intsantiates an Item
	void generateItem(int itemNumber ,Vector3 randomPosition){

		GameObject instance = Instantiate (KSItemPrefab, randomPosition, Quaternion.identity) as GameObject;

		canvas=GameObject.Find("Canvas");
		instance.transform.SetParent (canvas.GetComponent<Transform> (),false);

		//Setting the position in a separate line is importatant in order to set it according to global coordinates.
		instance.transform.position = randomPosition;

		//instance.GetComponentInChildren<Text>().text = ws[itemNumber]+ "Kg \n $" + vs[itemNumber];

		GameObject bill = instance.transform.Find("Bill").gameObject;
		bill.GetComponentInChildren<Text>().text = "$" + vs[itemNumber];

		GameObject weight = instance.transform.Find("Weight").gameObject;
		weight.GetComponentInChildren<Text>().text = ws[itemNumber]+ "Kg";


		// This calculates area accrding to approach 1
//		float areaItem1 = minAreaBill + (totalAreaBill - vs.Length * minAreaBill) * vs [itemNumber] / vs.Sum ();
//		float scale1 = Convert.ToSingle (Math.Sqrt (areaItem1) - 1);
//		bill.transform.localScale += new Vector3 (scale1, scale1, 0);
//
//		float areaItem2 = minAreaWeight + (totalAreaWeight - ws.Length * minAreaWeight) * ws [itemNumber] / ws.Sum ();
//		float scale2 = Convert.ToSingle (Math.Sqrt (areaItem2) - 1);
//		weight.transform.localScale += new Vector3 (scale2, scale2, 0);

		// This calculates area accrding to approach 2

		float adjustmentBill = (minAreaBill - totalAreaBill * vs.Min () / vs.Sum ()) / (1 - vs.Length * vs.Min () / vs.Sum ());

		float areaItem1 = adjustmentBill + (totalAreaBill - vs.Length * adjustmentBill) * vs [itemNumber] / vs.Sum ();
		float scale1 = Convert.ToSingle (Math.Sqrt (areaItem1) - 1);
		bill.transform.localScale += new Vector3 (scale1, scale1, 0);

		float adjustmentWeight = (minAreaWeight - totalAreaWeight * ws.Min () / ws.Sum ()) / (1 - ws.Length * ws.Min () / ws.Sum ());

		float areaItem2 = adjustmentWeight + (totalAreaWeight - ws.Length * adjustmentWeight) * ws [itemNumber] / ws.Sum ();
		float scale2 = Convert.ToSingle (Math.Sqrt (areaItem2) - 1);
		weight.transform.localScale += new Vector3 (scale2, scale2, 0);


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
			setQuestion ();
			RandomizeButtons ();
			keysON = true;
		}

	}

//	//Checks if positioning an item in the new position generates an overlap. Assuming the new item has a radius of KSITemRadius.
//	//Returns: TRUE if there is an overlap. FALSE Otherwise.
//	bool objectOverlapsQ(Vector3 pos)
//	{
//		//If physics could be started before update we could use the following easier function:
//		//bool overlap = Physics2D.IsTouchingLayers(newObject.GetComponent<Collider2D>());
//
//		bool overlap = Physics2D.OverlapCircle(pos,KSItemRadius);
//		return overlap;
//
//	}

	//Checks if positioning an item in the new position generates an overlap. Assuming the new item has a radius of KSITemRadius.
	//Returns: TRUE if there is an overlap. FALSE Otherwise.
	bool objectOverlapsQ(Vector3 pos)
	{
		//If physics could be started before update we could use the following easier function:
		//bool overlap = Physics2D.IsTouchingLayers(newObject.GetComponent<Collider2D>());

		bool overlap = Physics2D.OverlapCircle(pos,KSItemRadius);
		return overlap;

	}

	public void updateTimer(){
		RectTransform timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();

		timer.sizeDelta = new Vector2 (timerWidth * (GameManager.tiempo / GameManager.totalTime), timer.rect.height);
	}

	//Sets the triggers for pressing the corresponding keys
	//123: Perhaps a good practice thing to do would be to create a "close scene" funcction that takes as parameter the answer and closes everything (including keysON=false) and then forwards to 
	//changeToNextScene(answer) on game manager
	private void setKeyInput(){
		
		//1: No/Yes 0: Yes/No
		//Debug.Log("RandomYesKeys");
		//Debug.Log(randomYes);

		if(randomYes==1){
			if (Input.GetKeyDown (KeyCode.A)) {
				GameManager.changeToNextScene (0,randomYes);
			} else if (Input.GetKeyDown (KeyCode.G)) {
				GameManager.changeToNextScene (1,randomYes);
			}
		} else if (randomYes==0){
			if (Input.GetKeyDown (KeyCode.A)) {
				GameManager.changeToNextScene (1,randomYes);
			} else if (Input.GetKeyDown (KeyCode.G)) {
				GameManager.changeToNextScene (0,randomYes);
			}
		}


	}


	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		
		if (keysON) {
			setKeyInput ();
		}

	}



}