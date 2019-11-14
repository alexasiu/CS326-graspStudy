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
    public GameObject gaze;

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
    public float recRate = 0.05f;  //[s]
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

        duringTrialText.enabled = false;
        endTrialText.enabled = false;
        endStudyText.enabled = false;

        SetPegHoleActive(true);
        SetTargetActive(false);
        
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

                    // set screen text
                    duringTrialText.enabled = true;
                    endTrialText.enabled = false;
                    endStudyText.enabled = false;
                    trialNumText.text = "Trial num: " + currTrialNum + "/" + totalTrials;

                    // Remove start markers
                    SetPegHoleActive(false);

                    // Activate target
                    SetTargetActive(true);

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
                        dataLogger.RecordData(currTrialNum, Time.time, peg.transform, hole.transform, gaze.transform.position);
                        _startRecTime = recRate;
                    }
                }

                if (Input.GetKeyDown(KeyCode.RightArrow)) { // replace with trial end condition

                    // save end time for this trial
                    trialEndTime = Time.time;
                    // record the stats data
                    dataLogger.RecordStats(currTrialNum, trialStartTime, trialEndTime);

                    // advance trial number
                    currTrialNum++;
                    if (currTrialNum <= totalTrials) { // if we haven't reached max

                        duringTrialText.enabled = false;
                        endTrialText.enabled = true;
                        endStudyText.enabled = false;

                        // Set start positions and remove target
                        SetPegHoleActive(true);
                        SetTargetActive(false);

                        // Show screen that says press space to start next trial
                        if (DEBUG) Debug.Log("switch to pause");

                        _nextState = StudyState.Pause;
                        
                    } else { // End of study

                        duringTrialText.enabled = false;
                        endTrialText.enabled = false;
                        endStudyText.enabled = true;
                        
                        SetPegHoleActive(false);
                        SetTargetActive(false);

                        if (DEBUG) Debug.Log("switch to end");
                        
                        // Show screen that says end of study
                        _nextState = StudyState.End;
                    }

                }
            break;

            case StudyState.Pause:
                if (ReadyToStartTrial()) { 

                    if (DEBUG) Debug.Log("switch to trial");
                    
                    SetPegHoleActive(false);
                    SetTargetActive(true);

                    // start new file recordings and update start time
                    trialStartTime = Time.time;
                    dataLogger.NewDataFile(currTrialNum, userNum);

                    // set screen to start
                    duringTrialText.enabled = true;
                    endTrialText.enabled = false;
                    endStudyText.enabled = false;
                    trialNumText.text = "Trial num: " + currTrialNum + "/" + totalTrials;

                    _nextState = StudyState.Trial;
                }
            break;

            case StudyState.End:
                
            break;

        }// end switch

        _currentState = _nextState;
    }// end update


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
    
}
