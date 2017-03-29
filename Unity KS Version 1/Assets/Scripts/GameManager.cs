
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

	//Time spent so far on this scene 
	public static float tiempo;

	//Total time for these scene
	public static float totalTime;

	//Current trial initialization
	public static int trial = 0;

	//Current block initialization
	public static int block = 0;

	private static bool showTimer;


	//Modifiable Variables:
	//Minimum and maximum for randomized interperiod Time 
	public static float timeRest1min=5;
	public static float timeRest1max=9;

	//InterBlock rest time
	public static float timeRest2=10;

	//public static float timeRest1;

	//Time given for each trial
	public static float timeTrial=10;

	//Time given for answering
	public static float timeAnswer=3;

	//Total number of trials in each block
	private static int numberOfTrials = 30;

	//Total number of blocks
	private static int numberOfBlocks = 3;

	//Number of instance file to be considered. From i1.txt to i_.txt..
	public static int numberOfInstances = 3;

	//The order of the instances to be presented
	public static int[] instanceRandomization;

	//The order of the left/right No/Yes randomization
	public static int[] buttonRandomization;



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

		InitGame();

	}


	//Initializes the scene. One scene is setup, other is trial, other is Break....
	//escena = 		0->Setup	1->Trial	2->RestPeriod
	void InitGame(){

		/*
		Scene Order:
		0=setup
		1=trial game
		2=trial game answer
		3= intertrial rest
		4= interblock rest
		5= end
		*/
		Scene scene = SceneManager.GetActiveScene();
		escena = scene.buildIndex;

		if (escena == 0) {
			//Only uploads parameters and instances once.
			block++;
			loadParameters ();
			loadKPInstance ();

			//RandomizeKSInstances ();
			randomizeButtons ();
			SceneManager.LoadScene (1);
		} else if (escena == 1) {
			trial++;
			showTimer = true;
			boardScript.SetupScene (1);
			tiempo = timeTrial;
			totalTime = timeTrial;

		} else if (escena == 2) {
			showTimer = true;
			boardScript.SetupScene (2);
			tiempo = timeAnswer;
			totalTime = timeAnswer;
		} else if (escena == 3) {
			showTimer = false;
			tiempo = Random.Range (timeRest1min, timeRest1max);
			totalTime = tiempo;
		} else if (escena == 4) {
			trial = 0;
			block++;
			showTimer = true;
			tiempo = timeRest2;
			totalTime = tiempo;
			Debug.Log ("TiempoRest=" + tiempo);

			randomizeButtons ();
			//SceneManager.LoadScene (1);
		}
	}

	// Update is called once per frame
	void Update () {
		startTimer ();
	}
		
	//Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	//If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	//Each line in the File has the following structure: "trial;answer;timeSpent".
	public static void save(int answer, float timeSpent, int randomYes) {

		string dataTrialText = block + ";" + trial + ";" + answer + ";" + timeSpent + ";" + randomYes; 

		string[] lines = {dataTrialText};
		//string folderPathSave = @"/Users/jfranco1/Desktop/Unity Projects/knapsack4Imaging/DATA/";

		string folderPathSave = Application.dataPath.Replace("Assets","") + "DATA/Output/";

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
		//string folderPathLoad = @"/Users/jfranco1/Desktop/Unity Projects/knapsack4Imaging/KPInstances/";
		string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/KPInstances/";
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

	//Loads the parameters form the text files: param.txt and layoutParam.txt
	void loadParameters(){
		string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/";
		var dict = new Dictionary<string, string>();

		try {   // Open the text file using a stream reader.
			using (StreamReader sr = new StreamReader (folderPathLoad + "layoutParam.txt")) {

				// (This loop reads every line until EOF or the first blank line.)
				string line;
				while (!string.IsNullOrEmpty((line = sr.ReadLine())))
				{
					// Split each line around ':'
					string[] tmp = line.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}
				

			using (StreamReader sr1 = new StreamReader (folderPathLoad + "param.txt")) {

				// (This loop reads every line until EOF or the first blank line.)
				string line1;
				while (!string.IsNullOrEmpty((line1 = sr1.ReadLine())))
 				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line1.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
					// Add the key-value pair to the dictionary:
					dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
				}
			}
		} catch (Exception e) {
			Debug.Log ("The file could not be read:");
			Debug.Log (e.Message);
		}

		assignVariables(dict);
				
	}

	//Assigns the parameters in the dictionary to variables
	void assignVariables(Dictionary<string,string> dictionary){

		//Assigns Parameters
		string timeRest1minS;
		string timeRest1maxS;
		string timeRest2S;
		string timeTrialS;
		string timeAnswerS;
		string numberOfTrialsS;
		string numberOfBlocksS;
		string numberOfInstancesS;
		string instanceRandomizationS;

		dictionary.TryGetValue ("timeRest1min", out timeRest1minS);
		dictionary.TryGetValue ("timeRest1max", out timeRest1maxS);
		dictionary.TryGetValue ("timeRest2", out timeRest2S);

		dictionary.TryGetValue ("timeTrial", out timeTrialS);

		dictionary.TryGetValue ("timeAnswer", out timeAnswerS);

		dictionary.TryGetValue ("numberOfTrials", out numberOfTrialsS);

		dictionary.TryGetValue ("numberOfBlocks", out numberOfBlocksS);

		dictionary.TryGetValue ("numberOfInstances", out numberOfInstancesS);

		timeRest1min=Convert.ToSingle (timeRest1minS);
		timeRest1max=Convert.ToSingle (timeRest1maxS);
		timeRest2=Convert.ToSingle (timeRest2S);
		timeTrial=Int32.Parse(timeTrialS);
		timeAnswer=Int32.Parse(timeAnswerS);
		numberOfTrials=Int32.Parse(numberOfTrialsS);
		numberOfBlocks=Int32.Parse(numberOfBlocksS);
		numberOfInstances=Int32.Parse(numberOfInstancesS);
			
		dictionary.TryGetValue ("instanceRandomization", out instanceRandomizationS);

		//If instanceRandomization is not included in the parameters file. It generates a randomization.
		if (!dictionary.ContainsKey("instanceRandomization")){
			RandomizeKSInstances();
		} else{
			int[] instanceRandomizationNo0 = Array.ConvertAll(instanceRandomizationS.Substring (1, instanceRandomizationS.Length - 2).Split (','), int.Parse);
			instanceRandomization = new int[instanceRandomizationNo0.Length];
			foreach (int i in instanceRandomizationNo0) {
				instanceRandomization[i] = instanceRandomizationNo0 [i] - 1;
			}
		}


		////Assigns LayoutParameters
		string timerWidthS;
		string resolutionWidthS;
		string resolutionHeightS;
		string columnsS;
		string rowsS;
		string KSItemRadiusS;

		dictionary.TryGetValue ("timerWidth", out timerWidthS);

		dictionary.TryGetValue ("resolutionWidth", out resolutionWidthS);

		dictionary.TryGetValue ("resolutionHeight", out resolutionHeightS);

		dictionary.TryGetValue ("columns", out columnsS);

		dictionary.TryGetValue ("rows", out rowsS);

		dictionary.TryGetValue ("KSItemRadius", out KSItemRadiusS);

		//66
		BoardManager.timerWidth = Convert.ToSingle (timerWidthS);

		//123 / 66 : Understand why if radius is not static it wont change the actual overlapping radius. i.e. understand static vars and see where layout parameters should be
		BoardManager.resolutionWidth=Int32.Parse(resolutionWidthS);
		BoardManager.resolutionHeight=Int32.Parse(resolutionHeightS);
		BoardManager.columns=Int32.Parse(columnsS);
		BoardManager.rows=Int32.Parse(rowsS);
		BoardManager.KSItemRadius=Convert.ToSingle(KSItemRadiusS);//Int32.Parse(KSItemRadiusS);
	}


	//Randomizes the sequence of Instances to be shown to the participant adn stores it in: instanceRandomization
	void RandomizeKSInstances(){
		instanceRandomization = new int[numberOfTrials*numberOfBlocks];
		for (int i = 0; i < numberOfTrials*numberOfBlocks; i++) {
				instanceRandomization[i] = Random.Range(0,numberOfInstances);
		}

	}

	//Randomizes The Location of the Yes/No button for a whole block.
	void randomizeButtons(){

		buttonRandomization = new int[numberOfTrials];

		List<int> buttonRandomizationTemp = new List<int> ();

		for (int i = 0; i < numberOfTrials; i++){
			if (i % 2 == 0) {
				buttonRandomizationTemp.Add(0);
			} else {
				buttonRandomizationTemp.Add(1);
			}
		}

		for (int i = 0; i < numberOfTrials; i++) {
			int randomIndex = Random.Range (0, buttonRandomizationTemp.Count);
			buttonRandomization [i] = buttonRandomizationTemp [randomIndex];
			buttonRandomizationTemp.RemoveAt (randomIndex);
		}

	}

//	//Saves and Changes to the next trial
//	public static void changeToNextTrial(int newKPInstance, int answer){
//		save (answer, timeTrial-tiempo);
//		SceneManager.LoadScene(newKPInstance);
//	}


	//Takes care of changing the Scene to the next one (Except for when in the setup scene)
	public static void changeToNextScene(int answer, int randomYes){
		BoardManager.keysON = false;
		if (escena == 1) {
			SceneManager.LoadScene (2);
		} else if (escena == 2) {
			if (answer == 2) {
				save (answer, timeTrial, randomYes);
			} else {
				save (answer, timeAnswer - tiempo, randomYes);
			}
			SceneManager.LoadScene (3);
		} else if (escena == 3) {

			//Checks if trials are over
			if (trial < numberOfTrials) {
				SceneManager.LoadScene (1);
			} else if (block < numberOfBlocks) {
				SceneManager.LoadScene (4);
	
			} else {
				SceneManager.LoadScene (5);
			}
		} else if (escena == 4) {
			SceneManager.LoadScene (1);
		}
			
	}

	//Updates the timer (including the graphical representation)
	//If time runs out in the trial or the break scene. It switches to the next scene. 
	void startTimer(){
		tiempo -= Time.deltaTime;
		//Debug.Log (tiempo);
		if (showTimer) {
			//66 Pasar esto a boardManager

			boardScript.updateTimer();
//			RectTransform timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
//			timer.sizeDelta = new Vector2 (timerWidth * (tiempo / timeTrial), timer.rect.height);
		}

		if(tiempo < 0)
		{	
			changeToNextScene(2,BoardManager.randomYes);
//			if (escena == 1) {
//				save (2, timeTrial);
//			}
//
//			int nuevaEscena = (escena == 1) ? 2 : 1;
//			SceneManager.LoadScene(nuevaEscena);
		}
	}


}