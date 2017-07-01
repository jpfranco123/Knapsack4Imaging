﻿
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Linq;
using Random = UnityEngine.Random;
//using System.Diagnostics;

public class GameManager : MonoBehaviour {

	//Game Manager: It is a singleton (i.e. it is always one and the same it is nor destroyed nor duplicated)
	public static GameManager instance=null;

	//The reference to the script managing the board (interface/canvas).
	private BoardManager boardScript;

	//Current Scene
	public static int escena;

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

	//Time given for each trial (The total time the items are shown -With and without the question-)
	public static float timeTrial=10;

	//Time for seeing the KS items without the question
	public static float timeOnlyItems=1;

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

	//This is the string that will be used as the file name where the data is stored. DeCurrently the date-time is used.
	public static string participantID = "Empty";

	public static string dateID = @System.DateTime.Now.ToString("dd MMMM, yyyy, HH-mm");

	private static string identifierName;

	//Is the question shown on scene scene 1?
	private static int questionOn;

	//Input and Outout Folders with respect to the Application.dataPath;
	private static string inputFolder = "/DATAinf/Input/";
	private static string inputFolderKSInstances = "/DATAinf/Input/KPInstances/";
	private static string outputFolder = "/DATAinf/Output/";

	// Stopwatch to calculate time of events.
	private static System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
	// Time at which the stopwatch started. Time of each event is calculated according to this moment.
	private static string initialTimeStamp;

	private static bool soundON =false;


	//A structure that contains the parameters of each instance
	public struct KSInstance
	{
		public int capacity;
		public int profit;

		public int[] weights;
		public int[] values;

		public string id;
		public string type;

		public int solution;
	}

	//An array of all the instances to be uploaded form .txt files.
	public static KSInstance[] ksinstances;// = new KSInstance[numberOfInstances];

	// Use this for initialization
	void Awake () {

		//Makes the Gama manager a Singleton
		if (instance == null) {
			instance = this;
		}
		else if (instance != this)
			Destroy (gameObject);

		DontDestroyOnLoad (gameObject);

		//Initializes the game
		boardScript = instance.GetComponent<BoardManager> ();

		InitGame();
		if (escena != 0) {
			saveTimeStamp(escena);
		}

	}


	//Initializes the scene. One scene is setup, other is trial, other is Break....
	void InitGame(){

		/*
		Scene Order: escena
		0=setup
		1=trial game
		2=trial game answer
		3= intertrial rest
		4= interblock rest
		5= end
		*/
		Scene scene = SceneManager.GetActiveScene();
		escena = scene.buildIndex;
		Debug.Log ("escena" + escena);
		if (escena == 0) {
			//Only uploads parameters and instances once.
			block++;
			loadParameters ();
			loadKPInstance ();

			//RandomizeKSInstances ();
			randomizeButtons ();
			boardScript.setupInitialScreen ();
			//SceneManager.LoadScene (1);

		} else if (escena == 1) {
			trial++;
			showTimer = true;
			boardScript.SetupScene (1);

			tiempo = timeOnlyItems;
			totalTime = timeOnlyItems;

			questionOn = 0;

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
			//Debug.Log ("TiempoRest=" + tiempo);

			randomizeButtons ();
			//SceneManager.LoadScene (1);
		}

	}

	// Update is called once per frame
	void Update () {

		if (escena != 0) {
			startTimer ();
			pauseManager ();
		}
	}

	//To pause press alt+p
	//Pauses/Unpauses the game. Unpausing take syou directly to next trial
	//Warning! When Unpausing the following happens:
	//If paused/unpaused in scene 1 or 2 (while items are shown or during answer time) then saves the trialInfo with an error: "pause" without information on the items selected.
	//If paused/unpaused on ITI or IBI then it generates a new row in trial Info with an error ("pause"). i.e. there are now 2 rows for the trial.
	private void pauseManager(){
		if (( Input.GetKey (KeyCode.LeftAlt) || Input.GetKey (KeyCode.RightAlt)) && Input.GetKeyDown (KeyCode.P) ){
			Time.timeScale = (Time.timeScale == 1) ? 0 : 1;
			if(Time.timeScale==1){
				errorInScene("Pause");
			}
		}
	}

	//Saves the data of a trial to a .txt file with the participants ID as filename using StreamWriter.
	//If the file doesn't exist it creates it. Otherwise it adds on lines to the existing file.
	//Each line in the File has the following structure: "trial;answer;timeSpent".
	public static void save(int answer, float timeSpent, int randomYes, string error) {

		//string xyCoordinates = instance.boardScript.getItemCoordinates ();//BoardManager.getItemCoordinates ();
		string xyCoordinates = BoardManager.getItemCoordinates ();

		//Get the instance n umber for this trial and add 1 because the instanceRandomization is linked to array numbering in C#, which starts at 0;
		int instanceNum = instanceRandomization [trial + (block - 1) * numberOfTrials - 1] + 1;

		int solutionQ = ksinstances [instanceNum - 1].solution;
		int correct = (solutionQ == answer) ? 1 : 0;

		if (correct != 1) {
			instance.playSound ();
		}

		string dataTrialText = block + ";" + trial + ";" + answer + ";" + correct + ";" + timeSpent + ";" + randomYes +";" + instanceNum + ";" + xyCoordinates + ";" + error;

		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;

		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName +"TrialInfo.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

		//Options of streamwriter include: Write, WriteLine, WriteAsync, WriteLineAsync
	}

	private void playSound(){
		if (soundON) {
			//int samplerate = 44100;
			//AudioClip myClip = AudioClip.Create("MySinusoid", samplerate * 2, 1, samplerate, true);
			AudioSource aud = GetComponent<AudioSource> ();
			//aud.clip = myClip;
			aud.Play ();
		}
	}

	/// <summary>
	/// Saves the time stamp for a particular event type to the "TimeStamps" File
	/// </summary>
	/// Event type: 1=ItemsNoQuestion;11=ItemsWithQuestion;2=AnswerScreen;21=ParticipantsAnswer;3=InterTrialScreen;4=InterBlockScreen;5=EndScreen
	public static void saveTimeStamp(int eventType) {

		string dataTrialText = block + ";" + trial + ";" + eventType + ";" + timeStamp();

		string[] lines = {dataTrialText};
		string folderPathSave = Application.dataPath + outputFolder;

		//This location can be used by unity to save a file if u open the game in any platform/computer:      Application.persistentDataPath;
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}
	}

	/// <summary>
	/// Saves the headers for both files (Trial Info and Time Stamps)
	/// In the trial file it saves:  1. The participant ID. 2. Instance details.
	/// In the TimeStamp file it saves: 1. The participant ID. 2.The time onset of the stopwatch from which the time stamps are measured. 3. the event types description.
	/// </summary>
	private static void saveHeaders(){

		identifierName = participantID + "_" + dateID + "_" + "Dec" + "_";
		string folderPathSave = Application.dataPath + outputFolder;

		string[] lines3 = new string[numberOfInstances+2];
		lines3[0]="PartcipantID:" + participantID;
		lines3 [1] = "Instance" + ";c"  + ";p" + ";w" + ";v" + ";id" + ";type" + ";sol";
		int l = 2;
		int ksn = 1;
		foreach (KSInstance ks in ksinstances) {
			//Without instance type and problem ID:
			//lines [l] = "Instance:" + ksn + ";c=" + ks.capacity + ";p=" + ks.profit + ";w=" + string.Join (",", ks.weights.Select (p => p.ToString ()).ToArray ()) + ";v=" + string.Join (",", ks.values.Select (p => p.ToString ()).ToArray ());
			//With instance type and problem ID
			lines3 [l] = ksn + ";" + ks.capacity + ";" + ks.profit + ";" + string.Join (",", ks.weights.Select (p => p.ToString ()).ToArray ()) + ";" + string.Join (",", ks.values.Select (p => p.ToString ()).ToArray ())
				+ ";" + ks.id + ";" + ks.type + ";" + ks.solution;
			l++;
			ksn++;
		}
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "InstancesInfo.txt",true)) {
			foreach (string line in lines3)
				outputFile.WriteLine(line);
		}


		// Trial Info file headers
		string[] lines = new string[2];
		lines[0]="PartcipantID:" + participantID;
		lines [1] = "block;trial;answer;correct;timeSpent;randomYes(1=Left:No/Right:Yes);instanceNumber;xyCoordinates;error";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TrialInfo.txt",true)) {
			foreach (string line in lines)
				outputFile.WriteLine(line);
		}

		// Time Stamps file headers
		string[] lines1 = new string[4];
		lines1[0]="PartcipantID:" + participantID;
		lines1[1] = "InitialTimeStamp:" + initialTimeStamp;
		lines1[2]="1:ItemsNoQuestion;11:ItemsWithQuestion;2:AnswerScreen;21:ParticipantsAnswer;3:InterTrialScreen;4:InterBlockScreen;5:EndScreen";
		lines1[3]="block;trial;eventType;elapsedTime";
		using (StreamWriter outputFile = new StreamWriter(folderPathSave + identifierName + "TimeStamps.txt",true)) {
			foreach (string line in lines1)
				outputFile.WriteLine(line);
		}



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
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/KPInstances/";
		string folderPathLoad = Application.dataPath + inputFolderKSInstances;
//		int linesInEachKPInstance = 4;
		ksinstances = new KSInstance[numberOfInstances];

		for (int k = 1; k <= numberOfInstances; k++) {

			var dict = new Dictionary<string, string>();
//			string[] KPInstanceText = new string[linesInEachKPInstance];
			try {   // Open the text file using a stream reader.
				using (StreamReader sr = new StreamReader (folderPathLoad + "i"+ k +".txt")) {
//					for (int i = 0; i < linesInEachKPInstance; i++) {
//						string line = sr.ReadLine ();
//						string[] dataInLine = line.Split (':');
//						KPInstanceText [i] = dataInLine [1];
//					}
					string line;
					while (!string.IsNullOrEmpty((line = sr.ReadLine())))
					{
						string[] tmp = line.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
						// Add the key-value pair to the dictionary:
						dict.Add(tmp[0], tmp[1]);//int.Parse(dict[tmp[1]]);
					}
					// Read the stream to a string, and write the string to the console.
					//String line = sr.ReadToEnd();
				}
			} catch (Exception e) {
				Debug.Log ("The file could not be read:");
				Debug.Log (e.Message);
			}

			string weightsS;
			string valuesS;
			string capacityS;
			string profitS;
			string solutionS;

			dict.TryGetValue ("weights", out weightsS);
			dict.TryGetValue ("values", out valuesS);
			dict.TryGetValue ("capacity", out capacityS);
			dict.TryGetValue ("profit", out profitS);
			dict.TryGetValue ("solution", out solutionS);

			ksinstances[k-1].weights = Array.ConvertAll (weightsS.Substring (1, weightsS.Length - 2).Split (','), int.Parse);

			ksinstances[k-1].values = Array.ConvertAll (valuesS.Substring (1, valuesS.Length - 2).Split (','), int.Parse);

			ksinstances[k-1].capacity = int.Parse (capacityS);

			ksinstances[k-1].profit = int.Parse (profitS);

			ksinstances[k-1].solution = int.Parse (solutionS);

			dict.TryGetValue ("problemID", out ksinstances[k-1].id);
			dict.TryGetValue ("instanceType", out ksinstances[k-1].type);

		}

	}

	//Loads the parameters form the text files: param.txt and layoutParam.txt
	void loadParameters(){
		//string folderPathLoad = Application.dataPath.Replace("Assets","") + "DATA/Input/";
		string folderPathLoad = Application.dataPath + inputFolder;
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

			using (StreamReader sr2 = new StreamReader (folderPathLoad + "param2.txt")) {

				// (This loop reads every line until EOF or the first blank line.)
				string line2;
				while (!string.IsNullOrEmpty((line2 = sr2.ReadLine())))
				{
					//Debug.Log (1);
					// Split each line around ':'
					string[] tmp = line2.Split(new char[] {':'}, StringSplitOptions.RemoveEmptyEntries);
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
		string timeOnlyItemsS;

		dictionary.TryGetValue ("timeRest1min", out timeRest1minS);
		dictionary.TryGetValue ("timeRest1max", out timeRest1maxS);
		dictionary.TryGetValue ("timeRest2", out timeRest2S);

		dictionary.TryGetValue ("timeTrial", out timeTrialS);

		dictionary.TryGetValue ("timeAnswer", out timeAnswerS);

		dictionary.TryGetValue ("numberOfTrials", out numberOfTrialsS);

		dictionary.TryGetValue ("numberOfBlocks", out numberOfBlocksS);

		dictionary.TryGetValue ("numberOfInstances", out numberOfInstancesS);

		dictionary.TryGetValue ("timeOnlyItems", out timeOnlyItemsS);

		timeRest1min=Convert.ToSingle (timeRest1minS);
		timeRest1max=Convert.ToSingle (timeRest1maxS);
		timeRest2=Convert.ToSingle (timeRest2S);
		timeTrial=Int32.Parse(timeTrialS);
		timeAnswer=Int32.Parse(timeAnswerS);
		numberOfTrials=Int32.Parse(numberOfTrialsS);
		numberOfBlocks=Int32.Parse(numberOfBlocksS);
		numberOfInstances=Int32.Parse(numberOfInstancesS);
		timeOnlyItems=Convert.ToSingle (timeOnlyItemsS);

		dictionary.TryGetValue ("instanceRandomization", out instanceRandomizationS);
		//If instanceRandomization is not included in the parameters file. It generates a randomization.
//		if (!dictionary.ContainsKey("instanceRandomization")){
//			RandomizeKSInstances();
//		} else{
		int[] instanceRandomizationNo0 = Array.ConvertAll(instanceRandomizationS.Substring (1, instanceRandomizationS.Length - 2).Split (','), int.Parse);
		instanceRandomization = new int[instanceRandomizationNo0.Length];
		//foreach (int i in instanceRandomizationNo0)
		for (int i = 0; i < instanceRandomizationNo0.Length; i++){
			instanceRandomization[i] = instanceRandomizationNo0 [i] - 1;
		}
//		}


		////Assigns LayoutParameters
		string randomPlacementTypeS;
		//string resolutionWidthS;
		//string resolutionHeightS;
		string columnsS;
		string rowsS;
		//string KSItemRadiusS;
		string totalAreaBillS;
		string totalAreaWeightS;

		dictionary.TryGetValue ("randomPlacementType", out randomPlacementTypeS);
		//dictionary.TryGetValue ("resolutionWidth", out resolutionWidthS);
		//dictionary.TryGetValue ("resolutionHeight", out resolutionHeightS);
		dictionary.TryGetValue ("columns", out columnsS);
		dictionary.TryGetValue ("rows", out rowsS);
		dictionary.TryGetValue ("totalAreaBill", out totalAreaBillS);
		dictionary.TryGetValue ("totalAreaWeight", out totalAreaWeightS);

		//dictionary.TryGetValue ("KSItemRadius", out KSItemRadiusS);

		BoardManager.randomPlacementType = Int32.Parse(randomPlacementTypeS);

		//BoardManager.resolutionWidth=Int32.Parse(resolutionWidthS);
		//BoardManager.resolutionHeight=Int32.Parse(resolutionHeightS);
		BoardManager.columns=Int32.Parse(columnsS);
		BoardManager.rows=Int32.Parse(rowsS);
		BoardManager.totalAreaBill=Int32.Parse(totalAreaBillS);
		BoardManager.totalAreaWeight=Int32.Parse(totalAreaWeightS);
		//BoardManager.KSItemRadius=Convert.ToSingle(KSItemRadiusS);//Int32.Parse(KSItemRadiusS);
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
		if (escena == 0) {
			saveHeaders ();
			SceneManager.LoadScene (1);
		}
		else if (escena == 1) {
			SceneManager.LoadScene (2);
		} else if (escena == 2) {
			if (answer == 2) {
				save (answer, timeTrial, randomYes, "");
			} else {
				save (answer, timeAnswer - tiempo, randomYes, "");
				saveTimeStamp (21);
			}
			SceneManager.LoadScene (3);
		} else if (escena == 3) {
			changeToNextTrial ();
		} else if (escena == 4) {
			SceneManager.LoadScene (1);
		}

	}

	//Redirects to the next scene depending if the trials or blocks are over.
	private static void changeToNextTrial(){
		//Checks if trials are over
		if (trial < numberOfTrials) {
			SceneManager.LoadScene (1);
		} else if (block < numberOfBlocks) {
			SceneManager.LoadScene (4);
		} else {
			SceneManager.LoadScene (5);
		}
	}

	/// <summary>
	/// In case of an error: Skip trial and go to next one.
	/// Example of error: Not enough space to place all items
	/// </summary>
	/// Receives as input a string with the errorDetails which is saved in the output file.
	public static void errorInScene(string errorDetails){
		Debug.Log (errorDetails);

		BoardManager.keysON = false;
		int answer = 3;
		int randomYes = -1;
		save (answer, timeTrial, randomYes, errorDetails);
		changeToNextTrial ();
	}


	/// <summary>
	/// Starts the stopwatch. Time of each event is calculated according to this moment.
	/// Sets "initialTimeStamp" to the time at which the stopwatch started.
	/// </summary>
	public static void setTimeStamp(){
		initialTimeStamp=@System.DateTime.Now.ToString("HH-mm-ss-fff");
		stopWatch.Start ();
		Debug.Log (initialTimeStamp);
	}

	/// <summary>
	/// Calculates time elapsed
	/// </summary>
	/// <returns>The time elapsed in milliseconds since the "setTimeStamp()".</returns>
	private static string timeStamp(){
//		TimeSpan ts = stopWatch.Elapsed;
//		string stamp = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",
//			ts.Hours, ts.Minutes, ts.Seconds,
//			ts.Milliseconds / 10);
		long milliSec = stopWatch.ElapsedMilliseconds;
		string stamp = milliSec.ToString();
		return stamp;
	}


	//Updates the timer (including the graphical representation)
	//If time runs out in the trial or the break scene. It switches to the next scene.
	void startTimer(){
		tiempo -= Time.deltaTime;
		//Debug.Log (tiempo);
		if (showTimer) {
			boardScript.updateTimer();
//			RectTransform timer = GameObject.Find ("Timer").GetComponent<RectTransform> ();
//			timer.sizeDelta = new Vector2 (timerWidth * (tiempo / timeTrial), timer.rect.height);
		}

		//When the time runs out:
		if(tiempo < 0)
		{
			if (escena == 1 && questionOn == 0) {
				//After showing only the items do not change to next scene. Just show the question.
				totalTime = timeTrial - timeOnlyItems;
				tiempo = totalTime;
				boardScript.setQuestion ();
				saveTimeStamp(11);
				questionOn = 1;
			} else {
				//changeToNextScene(2,BoardManager.randomYes);
				changeToNextScene(BoardManager.answer,BoardManager.randomYes);
			}

		}
	}


}
