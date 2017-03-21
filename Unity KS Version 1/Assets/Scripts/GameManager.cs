
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour {

	//Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)
	public static GameManager instance=null;

	//The reference to the script managing the board (interface/canvas).
	public BoardManager boardScript;

	//Current Scene
	private static int escena;

	//Time spent on this scene
	public static float tiempo;

	//Current trial initialization
	public static int trial = 0;


	//Modifiable Variables:
	//Interperiod Time
	public static float timeRest1=5;

	//Time given for each trial
	public static float timeTrial=10;

	//Total number of trials
	private static int numberOfTrials = 30;

	//Number of instance file to be considered. From i1.txt to i_.txt..
	public static int numberOfInstances = 3;

	//The order of the instances to be presented
	public static int[] instanceRandomization;

	//Timer width
	private static float timerWidth =300;

	//This is the string that will be used as the file name where the data is stored. Currently the date-time is used.
	private static string participantID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");





	//A structure that contains the parameters of each instance
	public struct KSInstance
	{
		public int capacity;
		public int profit;

		public int[] weights;
		public int[] values;
	}

	//An array of all the instances to be uploaded form .txt files.
	public static KSInstance[] ksinstances = new KSInstance[numberOfInstances];




	// Use this for initialization
	void Awake () {

		//Makes the Gama manager a Singleton
		if (instance == null)
			instance = this;
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);


		//Initializes the game
		boardScript = GetComponent<BoardManager> ();

		//66: Relocate to to InitGame
		Scene scene = SceneManager.GetActiveScene();
		escena = scene.buildIndex;
		InitGame();

	}


	//Initializes the scene. One scene is setup, other is trial, other is Break....
	//escena = 		0->Setup	1->Trial	2->RestPeriod
	void InitGame(){

		if (escena == 0) {
			loadKPInstance ();
			RandomizeKSInstances ();
			SceneManager.LoadScene(1);
		}
		else if ( escena == 1) {
			boardScript.SetupScene ();
			tiempo = timeTrial;
			trial++;
		}
		else if (escena == 2) {
			tiempo = timeRest1; 
		}
	}

	// Update is called once per frame
	void Update () {
		startTimer ();
	}
		
	//Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	//If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	//Each line in the File has the following structure: "trial;answer;timeSpent".
	public static void save(int answer, float timeSpent) {

		string dataTrialText = trial + ";" + answer + ";" + timeSpent; 

		string[] lines = {dataTrialText};
		string folderPathSave = @"/Users/jfranco1/Desktop/Unity Projects/knapsack4Imaging/DATA/";

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;

		using (StreamWriter outputFile = new StreamWriter(folderPathSave + participantID,true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		} 

		//Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
	}

	/*
	 * Loads all of the instances to be uploaded form .txt files. Example of input file:
	 * Name of the file: i3.txt 
	 * Structure of each file is the following:
	 * weights:[2,5,8,10,11,12]
	 * values:[10,8,3,9,1,4]
	 * capacity:15
	 * profit:16
	 * 
	 * The instances are stored as ksinstances structures in the array of structures: ksinstances
	 */
	public static void loadKPInstance(){
		string folderPathLoad = @"/Users/jfranco1/Desktop/Unity Projects/knapsack4Imaging/KPInstances/";
		int linesInEachKPInstance = 4;

		for (int k = 1; k <= numberOfInstances; k++) {
			
			string[] KPInstanceText = new string[linesInEachKPInstance];
			try {   // Open the text file using a stream reader.
				using (StreamReader sr = new StreamReader (folderPathLoad + "i"+ k +".txt")) {
					for (int i = 0; i < linesInEachKPInstance; i++) {
						string line = sr.ReadLine ();
						string[] dataInLine = line.Split (':');
						KPInstanceText [i] = dataInLine [1];
					}
					// Read the stream to a string, and write the string to the console.
					//String line = sr.ReadToEnd();
				}
			} catch (Exception e) {
				Debug.Log ("The file could not be read:");
				Debug.Log (e.Message);
			}

			int textLineN = 0;
			ksinstances[k-1].weights = Array.ConvertAll (KPInstanceText [textLineN].Substring (1, KPInstanceText [textLineN].Length - 2).Split (','), int.Parse);

			textLineN = 1;
			ksinstances[k-1].values = Array.ConvertAll (KPInstanceText [textLineN].Substring (1, KPInstanceText [textLineN].Length - 2).Split (','), int.Parse);

			textLineN = 2;
			ksinstances[k-1].capacity = int.Parse (KPInstanceText [textLineN]);

			textLineN = 3;
			ksinstances[k-1].profit = int.Parse (KPInstanceText [textLineN]);

		}
			
	}


	//Randomizes the sequence of Instances to be shown to the participant adn stores it in: instanceRandomization
	void RandomizeKSInstances(){
		instanceRandomization = new int[numberOfTrials];
		for (int i = 0; i < numberOfTrials; i++) {
				instanceRandomization[i] = Random.Range(0,numberOfInstances);
		}

	}

	//Saves and Changes to the next trial
	public static void changeToNextTrial(int newKPInstance, int answer){
		save (answer, timeTrial-tiempo);
		SceneManager.LoadScene(newKPInstance);
	}

	//Updates the timer (including the graphical representation)
	//If time runs out in the trial or the break scene. It switches to the next scene. 
	void startTimer(){
		tiempo -= Time.deltaTime;
		//Debug.Log (tiempo);

		RectTransform timer = GameObject.Find ("Timer").GetComponent<RectTransform>();

		timer.sizeDelta = new Vector2(timerWidth*(tiempo/timeTrial), timer.rect.height);

		if(tiempo < 0)
		{	
			if (escena == 1) {
				save (2, timeTrial);
			}

			int nuevaEscena = (escena == 1) ? 2 : 1;
			SceneManager.LoadScene(nuevaEscena);
		}
	}


}