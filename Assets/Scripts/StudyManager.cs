﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StudyManager : MonoBehaviour
{

    public bool LOG_DATA = true;
    public bool DEBUG = false;

    public GameObject peg;
    public GameObject hole;
    
    private enum StudyState {Start, Trial, Pause, End};
    private StudyState _currentState = StudyState.Start;
    private StudyState _nextState = StudyState.Start;
    private DataManager dataLogger;

    private int currTrialNum = 0;
    public int totalTrials = 5;

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
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        dataLogger = this.GetComponent<DataManager>();

        duringTrialText.enabled = false;
        endTrialText.enabled = false;
        endStudyText.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        switch(_currentState) {

            case StudyState.Start:
                if (Input.GetKeyDown(KeyCode.Space)) {
                    _nextState = StudyState.Trial;

                    if (DEBUG) Debug.Log("switch to trial");

                    currTrialNum++;

                    // set screen text
                    duringTrialText.enabled = true;
                    endTrialText.enabled = false;
                    endStudyText.enabled = false;
                    trialNumText.text = "Trial num: " + currTrialNum + "/" + totalTrials;

                    // start recording file for this trial
                    trialStartTime = Time.time;
                    dataLogger.NewPegfile(currTrialNum);
                    dataLogger.NewHolefile(currTrialNum);
                }
            break;

            case StudyState.Trial:

                if (LOG_DATA) { // Record data if it's time
                    _startRecTime -= Time.deltaTime;
                    if ( _startRecTime <= 0 ) {
                        dataLogger.RecordPegPose(currTrialNum, Time.time, peg.transform);
                        dataLogger.RecordHolePose(currTrialNum, Time.time, hole.transform);
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

                        // Show screen that says press space to start next trial
                        if (DEBUG) Debug.Log("switch to pause");

                        _nextState = StudyState.Pause;
                        
                    } else { // End of study

                        duringTrialText.enabled = false;
                        endTrialText.enabled = false;
                        endStudyText.enabled = true;

                        if (DEBUG) Debug.Log("switch to end");
                        
                        // Show screen that says end of study
                        _nextState = StudyState.End;
                    }

                }
            break;

            case StudyState.Pause:
                if (Input.GetKeyDown(KeyCode.Space)) { 

                    if (DEBUG) Debug.Log("switch to trial");

                    // start new file recordings and update start time
                    trialStartTime = Time.time;
                    dataLogger.NewPegfile(currTrialNum);
                    dataLogger.NewHolefile(currTrialNum);
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
}