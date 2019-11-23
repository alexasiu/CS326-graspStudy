using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudyManager : MonoBehaviour
{

    public bool LOG_DATA = true;
    public bool DEBUG = false;

    public GameObject peg;
    public GameObject hole;
    public GazeManager gazeManager;

    public GameObject targetPos;
    public GameObject targetVisible;
    public GameObject pegStartPos;
    public GameObject holeStartPos;

    private float minTargetX = -0.741f;
    private float maxTargetX = -0.435f;
    private float minTargetY = 0.0645f;
    private float maxTargetY = 0.341f;
    
    private enum StudyState {Start, Trial, Pause, End};
    private StudyState _currentState = StudyState.Start;
    private StudyState _nextState = StudyState.Start;
    private DataManager dataLogger;

    private int currTrialNum = 0;
    public int totalTrials = 5;
    public int userNum = 0;

    private float trialStartTime;
    private float trialEndTime;

    #region DataRecording variables
    private float _startRecTime = 0.0f;
    private float _lastRecTime = 0.0f;  //[s]
    public float recRate = 0.02f;  //[s]
    #endregion DataRecording variables

    #region canvas instructions
    public Text duringTrialText;
    public Text endTrialText;
    public Text endStudyText; 
    public Text trialNumText; 
    public Text VRduringTrialText;
    public Text VRendTrialText;
    public Text VRendStudyText; 
    public Text VRtrialNumText; 
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        dataLogger = this.GetComponent<DataManager>();


        SetForInitialState();
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(_currentState) {

            case StudyState.Start:

                if (ReadyToStartTrial()) {
                    _nextState = StudyState.Trial;

                    if (DEBUG) Debug.Log("switch to trial");

                    currTrialNum++;

                    // set screen text + markers ready for new trial
                    SetForNewTrial();

                    // start recording file for this trial
                    trialStartTime = Time.time;
                    dataLogger.NewDataFile(currTrialNum, userNum);
                    dataLogger.NewStatsFile(userNum);
                }

            break;

            case StudyState.Trial:

                if (LOG_DATA) { // Record data if it's time
                    _startRecTime -= Time.deltaTime;
                    if ( _startRecTime <= 0 ) {
                        
                        GameObject obj = gazeManager.GetGazeGameObject();
                        string name = "";
                        if (obj != null) name = obj.name;

                        // TODO Call Gaze Manager and record gaze position data + focused object
                        // GetGaze Origin and GetGazeDirection
                        dataLogger.RecordData(currTrialNum, Time.time, 
                                                peg.transform, hole.transform, 
                                                targetPos.transform,
                                                gazeManager.GetGaze3DPoint(), name);
                        _startRecTime = recRate;
                    }
                }

                if (CheckTrialComplete()) { // check successful peg-hole

                    // save end time for this trial
                    trialEndTime = Time.time;
                    // record the stats data
                    dataLogger.RecordStats(currTrialNum, trialStartTime, trialEndTime, pegStartPos.transform, holeStartPos.transform);

                    // advance trial number
                    currTrialNum++;
                    if (currTrialNum <= totalTrials) { // if we haven't reached max

                        SetForEndTrial();

                        // Show screen that says press space to start next trial
                        if (DEBUG) Debug.Log("switch to pause");

                        _nextState = StudyState.Pause;
                        
                    } else { // End of study

                        SetForEndStudy();

                        if (DEBUG) Debug.Log("switch to end");
                        
                        // Show screen that says end of study
                        _nextState = StudyState.End;
                    }

                }
            break;

            case StudyState.Pause:
                if (ReadyToStartTrial()) { 

                    if (DEBUG) Debug.Log("switch to trial");
                    
                    SetForNewTrial();

                    // start new file recordings and update start time
                    trialStartTime = Time.time;
                    dataLogger.NewDataFile(currTrialNum, userNum);
                    
                    _nextState = StudyState.Trial;
                }
            break;

            case StudyState.End:
                // end of study
            break;

        }// end switch

        _currentState = _nextState;
    }// end update

    

    private void SetForInitialState() {
        duringTrialText.enabled = false;
        endTrialText.enabled = false;
        endStudyText.enabled = false;
        VRduringTrialText.enabled = false;
        VRendTrialText.enabled = false;
        VRendStudyText.enabled = false;
        
        SetPegHoleActive(true);
        SetTargetActive(false);
    }

    private void SetForNewTrial() {
        // set text
       duringTrialText.enabled = true;
       endTrialText.enabled = false;
       endStudyText.enabled = false;
       trialNumText.text = "Trial num: " + currTrialNum + "/" + totalTrials;
       VRduringTrialText.enabled = true;
       VRendTrialText.enabled = false;
       VRendStudyText.enabled = false;
       VRtrialNumText.text = "Trial num: " + currTrialNum + "/" + totalTrials;

       // Remove start markers
       SetPegHoleActive(false);
       // Activate target
       SetTargetActive(true);
    }

    private void SetForEndTrial() {
       duringTrialText.enabled = false;
       endTrialText.enabled = true;
       endStudyText.enabled = false;
       VRduringTrialText.enabled = false;
       VRendTrialText.enabled = true;
       VRendStudyText.enabled = false;

        // Set start positions and remove target
        SetPegHoleActive(true);
        SetTargetActive(false);
    }

    private void SetForEndStudy() {
        duringTrialText.enabled = false;
        endTrialText.enabled = false;
        endStudyText.enabled = true;
        VRduringTrialText.enabled = false;
        VRendTrialText.enabled = false;
        VRendStudyText.enabled = true;

        SetPegHoleActive(false);
        SetTargetActive(false);
    }

    private void SetPegHoleActive(bool active) {
        pegStartPos.SetActive(active);
        holeStartPos.SetActive(active);
    }

    private void SetTargetActive(bool active) {
        if (active) {
            // Activate target
            Vector2 newPos = GetRandomTargetPos();
            targetPos.transform.position = new Vector3(newPos.x, targetPos.transform.position.y, newPos.y);
            targetVisible.SetActive(true);
        } else {
            targetVisible.SetActive(false);
        }
    }
    
    private bool ReadyToStartTrial() {
        return CheckHoleInStart() && CheckPegInStart();
    }

    private bool CheckPegInStart() {
        TriggerDetector pegTrig = peg.GetComponent<TriggerDetector>();
            return (pegTrig.collided && pegTrig.objCollider.name == "VRStartPositionCollider");
    }

    private bool CheckHoleInStart() {
        TriggerDetector holeTrig = hole.GetComponent<TriggerDetector>();
            return (holeTrig.collided && holeTrig.objCollider.name == "ConStartPositionCollider");
    }

    private Vector2 GetRandomTargetPos() {
        return new Vector2( Random.Range(minTargetX, maxTargetX), 
                            Random.Range(minTargetY, maxTargetY));
    }

    private bool CheckPegInHole() {
        return this.GetComponent<PegInHole>().CheckPegInHole();
    }

    private bool CheckHoleInTarget() {
        // if (DEBUG) Debug.Log("Hole in target");
        TriggerDetector holeTrig = hole.GetComponent<TriggerDetector>();
        return (holeTrig.collided && holeTrig.objCollider.name == "TargetCollider");
    }

    private bool CheckTrialComplete() {
        return (CheckHoleInTarget() && CheckPegInHole());
    }
    
}
