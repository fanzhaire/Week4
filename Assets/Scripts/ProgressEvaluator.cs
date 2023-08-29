#define Pass60
#define HD90

using System;
using System.Collections;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;

public class ProgressEvaluator : MonoBehaviour
{
    private enum GradeBand { Deactivate, Pass50, Pass60, Credit70, Distinction80, HD90, HD100 };
    private Action[] evalMethods;
    [SerializeField] private uint studentNumber = 0;
    [SerializeField] private GradeBand bandReached = GradeBand.Deactivate;
    //[SerializeField] private bool showSuccessMessages = false;

    private string lastLog;
    private string currentLog;
    private GradeBand loadedBand = GradeBand.Deactivate;

    // Start is called before the first frame update
    void Start()
    {
        evalMethods = new Action[] { () => Pass50Band(), () => Pass60Band(), () => Credit70Band(), () => Distinction80Band(), () => HighDist90Band(), () => HighDist100Band() };
        loadedBand = EvalReader();
        //Debug.LogWarning("LOADED BAND == " + loadedBand);

        int i = 0;
        try
        {
            while (i < (int)bandReached)
            {
                if (i == 0)
                    CheckStudentNum();
                GradeBand currentBand = (GradeBand)i + 1;
                evalMethods[i]();
                if (currentBand <= GradeBand.Pass60)
                {
                    EvalPassMessage(currentBand.ToString());
                    if (currentBand > loadedBand)
                        EvalOutput(currentBand);
                }
                else
                    EvalInProgressMessage(currentBand.ToString());
                i++;
            }
        }
        catch (EvalFailedException e)
        {
            EvalFailMessage(((GradeBand)i + 1).ToString(), e.Message);
        }
        catch (Exception e)
        {
            EvalFailMessage(((GradeBand)i + 1).ToString(), "An unknown error occured during the progress evaluation. It is likely " +
                "that you have made a mistake that the ProgressEvaluator system has not been setup to handle. Go back over the " +
                "steps for this grade band and see if you can spot anything wrong or ask your tutor. The full error is below for reference: ");
            Debug.LogError(e.GetType() + ": " + e.Message);
            Debug.LogError(e.GetType() + ": Full Stack Trace: " + e.StackTrace);
        }
    }


    private void Pass50Band()
    {
        if (bandReached == GradeBand.Pass50)
        {
            if (!Directory.Exists(".git"))
                throw new EvalFailedException("No .git folder found. Do you have a Git repository setup at the same folder level as the Assets folder, Library folder, etc?");
            if (!File.Exists(".gitignore"))
                throw new EvalFailedException("No .gitignore file found. Do you have a Git repository setup at the same folder level as the Assets folder, Library folder, etc?");
        }
    }

    private void Pass60Band()
    {
        // Test: RedPrefab and material
        string[] prefabFiles = Directory.GetFiles("./", "RedPrefab.prefab", SearchOption.AllDirectories);
        if (prefabFiles.Length == 0)
            throw new EvalFailedException("No red prefab found in the Assets folder.");
        string path = prefabFiles[0]; ;
        GameObject prefabEdit = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Material mat = prefabEdit.GetComponent<Renderer>().material;
            if (mat.color.r < mat.color.g || mat.color.r < mat.color.b)
                throw new EvalFailedException("The red material on RedPrefab doesn't actually seem to be red");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabEdit);
        }


        // Test: BluePrefab and material
        prefabFiles = Directory.GetFiles("./", "BluePrefab.prefab", SearchOption.AllDirectories);
        if (prefabFiles.Length == 0)
            throw new EvalFailedException("No blue prefab found in the Assets folder.");
        path = prefabFiles[0];
        prefabEdit = PrefabUtility.LoadPrefabContents(path);
        try
        {
            Material mat = prefabEdit.GetComponent<Renderer>().material;
            if (mat.color.b < mat.color.g || mat.color.b < mat.color.r)
                throw new EvalFailedException("The blue material on BluePrefab doesn't actually seem to be blue");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabEdit);
        }

        //Test: LoadManager GameObject
        GameObject loadObj = GameObject.Find("LoadManager");
        if (loadObj == null)
            throw new EvalFailedException("No LoadManager GameObject found in the scene");
#if Pass60        
        LoadAssets la = loadObj.GetComponent<LoadAssets>();
        if (la == null)
            throw new EvalFailedException("No LoadAssets component on the LoadManager GameObject");
        if (la.redObj == null)
            throw new EvalFailedException("The redObj variable has not been set on the LoadAssets component attached to the LoadManager GameObject");
        if (!la.redObj.name.Equals("RedPrefab"))
            throw new EvalFailedException("The redObj variable of the LoadAssets component on the LoadManager is not currently set to the RedPrefab");
        Debug.LogWarning("PROGRESS EVALUATOR: Git repository not being automatically checked from Pass50 onwards. " +
            "Make sure you are adding files, committing changes, and pushing commits on your respository.");
#elif (!Pass60)
        throw new EvalFailedException("Open the ProgressEvaluator.cs file and uncomment (i.e. remove the // symbol) the line at the top that says #define Pass60");
#endif        
    }

    private void Credit70Band()
    {
        // Test: blueObj variable
        string[] laContents = Directory.GetFiles("./", "LoadAssets.cs", SearchOption.AllDirectories);
        if (laContents.Length == 0)
            throw new EvalFailedException("Can't find the LoadAssets.cs script in your Assets folder. " +
                "You should not be seeing this error, let William Raffe know.");
        string laContent = File.ReadAllText(laContents[0]);
        Regex rx = new Regex(@"\[SerializeField\]\s+private");
        string result = rx.Match(laContent).ToString();
        if (result.Equals(""))
            throw new EvalFailedException("Your blueObj needs to be private but exposed in the Inspector window (lookup SerializeField)");

        // Test: Instantiate
        rx = new Regex(@"Instantiate");
        result = rx.Match(laContent).ToString();
        if (result.Equals(""))
            throw new EvalFailedException("You are not using Instantiate in LoadAssets");

        StartCoroutine(Cred70Coroutine());
        Debug.LogWarning("PROGRESS EVALUATOR: Git usage is part of Assessment 3, so make sure you are understanding it now.");

    }

    private IEnumerator Cred70Coroutine()
    {
        // Test: Check RedPrefab instantiation
        yield return null;
        GameObject redObj = GameObject.Find("RedPrefab(Clone)");
        if (redObj == null)
            throw new EvalFailedException("No instance of the RedPrefab found in your scene");
        if (!VectApprox(redObj.transform.position, new Vector3(2, 0, 0)))
            throw new EvalFailedException("The RedPrefab instance has not been instatiated to the correct location");
        if (redObj.transform.rotation != Quaternion.identity)
            throw new EvalFailedException("The RedPrefab instance has not been instantiate with the correct rotation");

        // Test: Check BluePrefab instantiation
        GameObject blueObj = GameObject.Find("BluePrefab(Clone)");
        if (blueObj == null)
            throw new EvalFailedException("No instance of the BluePrefab found in your scene");
        if (!VectApprox(blueObj.transform.position, new Vector3(-2, 0, 0)))
            throw new EvalFailedException("The BluePrefab instance has not been instatiated to the correct location");
        if (blueObj.transform.rotation != Quaternion.identity)
            throw new EvalFailedException("The BluePrefab instance has not been instantiate with the correct rotation");

        // Eval Finished
        EvalPassMessage(GradeBand.Credit70.ToString());
        if (GradeBand.Credit70 > loadedBand)
            EvalOutput(GradeBand.Credit70);
    }

    private void Distinction80Band()
    {
        Debug.LogWarning("PROGRESS EVALUATOR: The automated checking for this band will be done after the next band. Continue development.");
        Debug.LogWarning("PROGRESS EVALUATOR: You should have multiple branches (master, dev, and feature-print) in your repository by now. Does your Git tree (e.g. gitk command in GitBash) show this?");
    }


    private void HighDist90Band()
    {
#if (HD90)
        // Test: Check PrintAndHide and Tag of RedPrefab
        string path = Directory.GetFiles("./", "RedPrefab.prefab", SearchOption.AllDirectories)[0];
        GameObject prefabEdit = PrefabUtility.LoadPrefabContents(path);
        PrintAndHide pAndH;
        try
        {
            pAndH = prefabEdit.GetComponent<PrintAndHide>();
            if (pAndH == null)
                throw new EvalFailedException("No PrintAndHide component found on the RedPrefab");
            if (pAndH.rend == null)
                throw new EvalFailedException("The rend variable of the PrintAndHide component on the RedPrefab has not been set.");
            if (!prefabEdit.CompareTag("Red"))
                throw new EvalFailedException("The RedPrefab does not have the tag 'Red'");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabEdit);
        }

        // Test: Check PrintAndHide and Tag of BluePrefab
        path = Directory.GetFiles("./", "BluePrefab.prefab", SearchOption.AllDirectories)[0];
        prefabEdit = PrefabUtility.LoadPrefabContents(path);
        pAndH = null;
        try
        {
            pAndH = prefabEdit.GetComponent<PrintAndHide>();
            if (pAndH == null)
                throw new EvalFailedException("No PrintAndHide component found on the BluePrefab");
            if (pAndH.rend == null)
                throw new EvalFailedException("The rend variable of the PrintAndHide component on the BluePrefab has not been set.");
            if (!prefabEdit.CompareTag("Blue"))
                throw new EvalFailedException("The BluePrefab does not have the tag 'Blue'");
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(prefabEdit);
        }

        Debug.LogWarning("PROGRESS EVALUATOR: Seriously, do manual checks on your Git repository, does it look correct? A lot of your subjects at UTS will use it for group projects. " +
            "You don't want to be 'that person' who stuffs up the entire group's repository, do you?");

        StartCoroutine(HD90Coroutine());
#elif (!HD90)
        throw new EvalFailedException("Open the ProgressEvaluator.cs file and uncomment (i.e. remove the // symbol) the line at the top that says #define HD90");
#endif        
    }


    private IEnumerator HD90Coroutine()
    {
        yield return null;
        GameObject redObj = GameObject.FindGameObjectWithTag("Red");
        if (redObj == null)
            throw new EvalFailedException("The RedPrefab has the correct tag but the red object instantiated in the scene doesn't. " +
                "This is likely because you have deactivated your redObj in the first frame. The redObject should be deactivated on the 100th frame instead.");

        Application.logMessageReceived += HandleLog;
        for (int i = 6; i < 101; i++)
        {
            yield return null;
            if (!CompareLogs(i))
                throw new EvalFailedException("There is a mistake on frame " + (i - 3) + " of your Debug.Log statements for the RedPrefab OR BluePrefab. " +
                    "The output should be 'RedPrefab(Clone):" + i + "' and BluePrefab(Clone):" + i + "'. " +
                    "Instead your output was '" + lastLog + "' and '" + currentLog + "'");
        }

        // Eval Finished
        EvalPassMessage(GradeBand.HD90.ToString());
        if (GradeBand.HD90 > loadedBand)
            EvalOutput(GradeBand.HD90);

        // Start HD100 checks if needed
        if (bandReached == GradeBand.HD100)
            StartCoroutine(HD100Coroutine(redObj));
    }

    private bool CompareLogs(int i)
    {
        if (currentLog.Contains("PROGRESS EVALUATOR") || lastLog.Contains("PROGRESS EVALUATOR"))
            return true;
        if (!currentLog.Equals("RedPrefab(Clone):" + i) && !currentLog.Equals("BluePrefab(Clone):" + i))
            return false;
        if (!lastLog.Equals("RedPrefab(Clone):" + i) && !lastLog.Equals("BluePrefab(Clone):" + i))
            return false;

        return true;
    }

    private void HighDist100Band()
    {
        string[] laContents = Directory.GetFiles("./", "PrintAndHide.cs", SearchOption.AllDirectories);
        if (laContents.Length == 0)
            throw new EvalFailedException("Can't find the PrintAndHides.cs script in your Assets folder. " +
                "You should not be seeing this error, let William Raffe know.");
        string laContent = File.ReadAllText(laContents[0]);
        Regex rx = new Regex("Range.*250");
        Regex rx2 = new Regex("Range.*//.*250");
        //string result = rx.Match(laContent).ToString();

        if (rx.Match(laContent).Success && !rx2.Match(laContent).Success)
            throw new EvalFailedException("It looks like your randomization of the Blue object disabling is not inclusive of 250. " +
                "Read the Unity manual for Random.Range(...) carefully.");
        rx = new Regex("Range.*200");
        rx2 = new Regex("Range.*150");
        if (!rx.Match(laContent).Success && !rx2.Match(laContent).Success)
            throw new EvalFailedException("It looks like your randomization of the Blue object disabling is not inclusive of 200. " +
                "Read the Unity manual for Random.Range(...) carefully.");

        Debug.LogWarning("PROGRESS EVALUATOR: Git git git git git git git git git git git. Check your local and remote repositories one last time.");
    }

    private IEnumerator HD100Coroutine(GameObject redObj)
    {
        // Test: Ongoing debug logs for blue object
        for (int i = 101; i < 260; i++)
        {
            yield return null;
            if (!CompareBlueLog(i))
                throw new EvalFailedException("There is a mistake on frame " + (i - 3) + " of your Debug.Log statements for the RedPrefab OR BluePrefab. " +
                    "The output should be 'BluePrefab(Clone):" + i + " and no output for the RedPrefab" +
                    "Instead your last two outputs were '" + lastLog + "' and '" + currentLog + "'. " +
                    "This is likely because you have have not deactivated the red object properly or you have incorrectly deactivated the blue object");
        }

        // Test: Check for proper disabling / deactivation
        if (redObj.activeSelf)
            throw new EvalFailedException("The red object is still active. It should be deactivated by this time");
        GameObject blueObj = GameObject.FindGameObjectWithTag("Blue");
        if (!blueObj.activeSelf)
            throw new EvalFailedException("The blue object has been deactivate. It should still be active");
        if (blueObj.GetComponent<Renderer>().enabled)
            throw new EvalFailedException("The blue object's renderer is enabled, it shuld be disabled by this time");

        // Eval Finished
        EvalPassMessage(GradeBand.HD100.ToString());
        if (GradeBand.HD100 > loadedBand)
            EvalOutput(GradeBand.HD100);
    }

    private bool CompareBlueLog(int i)
    {
        if (currentLog.Contains("PROGRESS EVALUATOR") || lastLog.Contains("PROGRESS EVALUATOR"))
            return true;
        if (!currentLog.Equals("BluePrefab(Clone):" + i))
            return false;

        return true;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        lastLog = currentLog;
        currentLog = logString;
    }

    private void CheckStudentNum()
    {
        if (studentNumber < 10000000)
            throw new EvalFailedException("Student Number Check: Invalid student number. This must be an 8 digit student ID.");
    }

    private static void EvalFailMessage(String band, String message)
    {
        Debug.LogError("PROGRESS EVALUATOR: " + band + ": " + message);
    }

    private static void EvalPassMessage(String band)
    {
        Debug.LogWarning("PROGRESS EVALUATOR: " + band + " Band: No common mistakes found. -------------");
    }

    private static void EvalInProgressMessage(String band)
    {
        Debug.LogWarning("PROGRESS EVALUATOR: " + band + " Band: This band is doing checks over multiple frames. Press keys identified in this band (if any) in the PDF instructions -------------");
    }


    private void EvalOutput(GradeBand evalGrade)
    {
        Debug.LogWarning("PROGRESS EVALUATOR: Remember to also check your progress against the Status-xPercent.png files and " +
            "Status-100Percent-Windows or Status-100Percent-Mac.app executables.");
        using BinaryWriter writer = new BinaryWriter(File.Open("ProjectSettings\\ProgEv", FileMode.Create));
        writer.Write(evalGrade.ToString());
        writer.Write(studentNumber);
        writer.Write(DateTime.Now.ToString("MM/dd/yyyy"));
        //writer.Write(DateTime.Now.Year.ToString());
    }

    private GradeBand EvalReader()
    {
        try
        {
            using BinaryReader reader = new BinaryReader(File.Open("ProgEv", FileMode.Open));
            return (GradeBand)Enum.Parse(typeof(GradeBand), reader.ReadString());
        }
        catch (Exception e)
        {
            lastLog = e.Message;
            return GradeBand.Deactivate;
        }
    }

    private bool VectApprox(Vector3 a, Vector3 b)
    {
        if (Vector3.Distance(a, b) < 0.01f)
            return true;
        else
            return false;
    }
}

public class EvalFailedException : Exception
{
    public EvalFailedException(string message) : base(message) { }
}
