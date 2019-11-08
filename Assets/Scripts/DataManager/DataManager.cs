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

    #region Unity updates
    // Use this for initialization
    void Start()
    {
        // TODO use Path.Combine instead for cross platform compatibility
        filePath = Directory.GetCurrentDirectory() + "/Assets/Data";
        string fileName = fileNameBase +  "_stats.csv";

        _statsLoggerThread = new FileLoggerThreadLoop(
            "statsLoggerThread", true, System.Threading.ThreadPriority.Lowest, 5, 4,
            fileName, filePath, writeTimeout);

        // Start the thread
        _statsLoggerThread.Start();
    }

    public void RecordStats(int trialNum, float start, float end) {
        if (_statsLoggerThread == null) return;
        _statsLoggerThread.EnqueueStringToWrite(trialNum + "," + start + "," + end);
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

    public void NewPegfile(int trialNum) {
        if (_pegLoggerThread != null)  {
            _pegLoggerThread.CloseAtConvenience();
        }

        string fileName = fileNameBase + "_" + trialNum + "_peg.csv";
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
        string fileName = fileNameBase + "_" + trialNum + "_hole.csv";
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