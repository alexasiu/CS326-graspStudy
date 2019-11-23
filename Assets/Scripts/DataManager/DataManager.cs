/// <summary>
/// Handles serial threading for recording study data
/// 
/// Author: A. Siu
/// July 27, 2019
/// </summary>
/// 
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;
using thrThreadLoop;

public class DataManager : MonoBehaviour
{

    // Set to false for minimal print statements
     public bool debug = false;

    #region file logging settings
    public string fileNameBase = "user";
    public string filePath;
    public int writeTimeout = 1;
    #endregion file logging settings

    // Declare any threads to create here
    private FileLoggerThreadLoop _pegLoggerThread;
    private FileLoggerThreadLoop _holeLoggerThread;
    private FileLoggerThreadLoop _statsLoggerThread;
    private FileLoggerThreadLoop _dataLoggerThread;

    #region Unity updates
    // Use this for initialization
    void Start()
    {
        // TODO use Path.Combine instead for cross platform compatibility
        filePath = Directory.GetCurrentDirectory() + "/Assets/Data";

    }

    public void RecordStats(int trialNum, float start, float end, Transform pegStart, Transform holeStart) {
        if (_statsLoggerThread == null) return;
        _statsLoggerThread.EnqueueStringToWrite(trialNum + "," + start + "," + end + "," + 
                    pegStart.position.x + "," + pegStart.position.y + "," + pegStart.position.z + "," + 
                    pegStart.rotation.x + "," + pegStart.rotation.y + "," + pegStart.rotation.z + "," +
                    holeStart.position.x + "," + holeStart.position.y + "," + holeStart.position.z + "," + 
                    holeStart.rotation.x + "," + holeStart.rotation.y + "," + holeStart.rotation.z);
    }


    // TODO Update this function to take in gaze data for recording
    public void RecordData(int trialNum, float time, Transform peg, Transform hole, Transform target, Vector3 gazePoint, string focusObject) {
        if (_dataLoggerThread == null) return;
        string data = trialNum + "," + time + "," + 
                    peg.position.x + "," + peg.position.y + "," + peg.position.z + "," + 
                    peg.rotation.x + "," + peg.rotation.y + "," + peg.rotation.z + "," +
                    hole.position.x + "," + hole.position.y + "," + hole.position.z + "," + 
                    hole.rotation.x + "," + hole.rotation.y + "," + hole.rotation.z + "," +
                    target.position.x + "," + target.position.y + "," + target.position.z + "," + 
                    target.rotation.x + "," + target.rotation.y + "," + target.rotation.z + "," +
                    gazePoint.x + "," + gazePoint.y + "," + gazePoint.z + "," +
                    focusObject;
        _dataLoggerThread.EnqueueStringToWrite(data);
    }

    public void RecordPegPose(int trialNum, float time, Transform trans) {
        if (_pegLoggerThread == null) return;
        string data = trialNum + "," + time + "," + 
                    trans.position.x + "," + trans.position.y + "," + trans.position.z + "," + 
                    trans.rotation.x + "," + trans.rotation.y + "," + trans.rotation.z;
        _pegLoggerThread.EnqueueStringToWrite(data);
    }

    public void RecordHolePose(int trialNum, float time, Transform trans) {
        if (_holeLoggerThread == null) return;
        string data = trialNum + "," + time + "," + 
                    trans.position.x + "," + trans.position.y + "," + trans.position.z + "," + 
                    trans.rotation.x + "," + trans.rotation.y + "," + trans.rotation.z;
        _holeLoggerThread.EnqueueStringToWrite(data);
    }

    public void NewStatsFile(int userNum) {
        string fileName = fileNameBase + "-" + userNum + "_stats";

        _statsLoggerThread = new FileLoggerThreadLoop(
            "statsLoggerThread", true, System.Threading.ThreadPriority.Lowest, 5, 4,
            fileName, filePath, writeTimeout);

        // Start the thread
        _statsLoggerThread.Start();
    }

    public void NewDataFile(int trialNum, int userNum) {
        if (_dataLoggerThread != null)  {
            _dataLoggerThread.CloseAtConvenience();
        }

        string fileName = fileNameBase + "-" + userNum + "_data" + "_trial-" + trialNum;
        _dataLoggerThread = new FileLoggerThreadLoop(
            "dataLoggerThread"+trialNum, true, System.Threading.ThreadPriority.Lowest, 5, 4,
            fileName, filePath, writeTimeout);
        // Start the thread
        _dataLoggerThread.Start();
    }

    public void NewPegfile(int trialNum) {
        if (_pegLoggerThread != null)  {
            _pegLoggerThread.CloseAtConvenience();
        }

        string fileName = fileNameBase + "_" + trialNum + "_peg";
        _pegLoggerThread = new FileLoggerThreadLoop(
            "pegLoggerThread"+trialNum, true, System.Threading.ThreadPriority.Lowest, 5, 4,
            fileName, filePath, writeTimeout);
        // Start the thread
        _pegLoggerThread.Start();
    }

    public void NewHolefile(int trialNum) {
        if (_holeLoggerThread != null)  {
            _holeLoggerThread.CloseAtConvenience();
        }
        string fileName = fileNameBase + "_" + trialNum + "_hole";
        _holeLoggerThread = new FileLoggerThreadLoop(
            "holeLoggerThread"+trialNum, true, System.Threading.ThreadPriority.Lowest, 5, 4,
            fileName, filePath, writeTimeout);
        // Start the thread
        _holeLoggerThread.Start();
    }

    /// <summary>
    /// Clean up the thread and close the port on application close event.
    /// </summary>
    void OnApplicationQuit()
    {
        if (_holeLoggerThread != null) _holeLoggerThread.CloseAtConvenience();
        if (_pegLoggerThread != null) _pegLoggerThread.CloseAtConvenience();
        if (_statsLoggerThread != null) _statsLoggerThread.CloseAtConvenience();
        if (_dataLoggerThread != null) _dataLoggerThread.CloseAtConvenience();

        if (debug)
        {
            Debug.Log("closing data logger thread");
        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// OnDestroy will only be called on game objects that have previously
    /// been active.
    /// </summary>
    void OnDestroy()
    {
        if (_holeLoggerThread != null) _holeLoggerThread.CloseAtConvenience();
        if (_pegLoggerThread != null) _pegLoggerThread.CloseAtConvenience();
        if (_statsLoggerThread != null) _statsLoggerThread.CloseAtConvenience();
        if (_dataLoggerThread != null) _dataLoggerThread.CloseAtConvenience();

        if (debug)
        {
            Debug.Log("closing data logger thread");
        }
    }
    #endregion Unity updates

	public bool IsOpenStream() {
        if (_holeLoggerThread == null) return false;

		return _holeLoggerThread.IsOpenStream ();
	}

}