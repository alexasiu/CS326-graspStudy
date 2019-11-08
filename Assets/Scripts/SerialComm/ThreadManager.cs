/// <summary>
/// Handles serial threading for the canetroller
/// Sends commands through serial port.
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

public class ThreadManager : MonoBehaviour
{

    // Set to false for minimal print statements
    public bool debug = false;

    #region serial port settings
    public string comPort;
    public int baudRate = 115200;
    public int readTimeout = 10;
    public int writeTimeout = 1;
    #endregion serial port settings

    // Declare any threads to create here
    private SerialThreadLoop _CanetrollerSerialThread;

    // Data sending variables
    // Display automatic refresh rate
    public float refreshRate = 0.05f; //[s]

    // Enable sending data to hw at fixed rate
    private bool _BrakeRight = false;
    private bool _BrakeLeft = false;
    private bool _BrakeUp = false;
    private bool _BrakeDown = false;
    private bool _BrakeStab = false;

    #region msg commands
    private const int MSG_SIZE = 3;
    private const int BRAKE_CMD = 127;
    private const int RELEASE_CMD = 126;
    private const int RELEASE_ALL_CMD = 125;
    private const int UP_DIR  = 124;
    private const int DOWN_DIR = 123;
    private const int RIGHT_DIR = 122;
    private const int LEFT_DIR = 121;
    private const int STAB_DIR = 120;
    private int _BrakeUpVal = 0;
    private int _BrakeDownVal = 0;
    private int _BrakeRightVal = 255;
    private int _BrakeLeftVal = 255;
    private int _BrakeStabVal = 255;
    #endregion end msg commands

    #region Unity updates
    // Use this for initialization
    void Start()
    {
        // Initialize thread
        _CanetrollerSerialThread = new SerialThreadLoop(
            "canetrollerThread", true, System.Threading.ThreadPriority.Lowest, 5, 4,
            comPort, baudRate, readTimeout, writeTimeout);

        // Start the thread
        _CanetrollerSerialThread.Start();

        // Register for a notification of the SerialDataReceivedEvent
        SerialThreadLoop.SerialReceivedDataEvent +=
           new SerialThreadLoop.SerialReceivedDataEventHandler(SerialReceivedEvent);
        
        comPort = GetComPortName();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            _BrakeUp = true;
            SendCanetrollerData();
        } else if (Input.GetKeyUp(KeyCode.UpArrow))
        {
            _BrakeUp = false;;
            SendCanetrollerData();
        } 
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            _BrakeDown = true;
            SendCanetrollerData();
        } else if (Input.GetKeyUp(KeyCode.DownArrow))
        {
            _BrakeDown = false;
            SendCanetrollerData();
        } 
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            _BrakeRight = true;
            SendCanetrollerData();
        } else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            _BrakeRight = false;
            SendCanetrollerData();
        } 
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            _BrakeLeft = true;
            SendCanetrollerData();
        } else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            _BrakeLeft = false;
            SendCanetrollerData();
        } 
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            _BrakeStab = true;
            SendCanetrollerData();
        } else if (Input.GetKeyUp(KeyCode.Alpha0))
        {
            _BrakeStab = false;
            SendCanetrollerData();
        } 
        
    }

    /// <summary>
    /// Clean up the thread and close the port on application close event.
    /// </summary>
    void OnApplicationQuit()
    {
        _CanetrollerSerialThread.CloseAtConvenience();

        // Remove event notifiation registration
        SerialThreadLoop.SerialReceivedDataEvent -= SerialReceivedEvent;

        if ( debug )
        {
            Debug.Log("closing canetroller thread");
        }
    }

    /// <summary>
    /// This function is called when the MonoBehaviour will be destroyed.
    /// OnDestroy will only be called on game objects that have previously
    /// been active.
    /// </summary>
    void OnDestroy()
    {
        _CanetrollerSerialThread.CloseAtConvenience();

        // Remove event notifiation registration
        SerialThreadLoop.SerialReceivedDataEvent -= SerialReceivedEvent;

        if (debug)
        {
            Debug.Log("closing canetroller thread");
        }
    }
    #endregion Unity updates

    #region Public Functions for Communicating with Teensy
    public void BrakeRight() {
        _BrakeRight = true;
        SendCanetrollerData();
    }
    public void ReleaseRight() {
        _BrakeRight = false;
        SendCanetrollerData();
    }
    public void BrakeLeft() {
        _BrakeLeft = true;
        SendCanetrollerData();
    }
    public void ReleaseLeft() {
        _BrakeLeft = false;
        SendCanetrollerData();
    }
    public void BrakeStab() {
        _BrakeStab = true;
        SendCanetrollerData();
    }
    public void ReleaseStab() {
        _BrakeStab = false;
        SendCanetrollerData();
    }
    public void BrakeUp() {
        _BrakeUp = true;
        SendCanetrollerData();
    }
    public void ReleaseUp() {
        _BrakeUp = false;
        SendCanetrollerData();
    }
    public void BrakeDown() {
        _BrakeDown = true;
        SendCanetrollerData();
    }
    public void ReleaseDown() {
        _BrakeDown = false;
        SendCanetrollerData();
    }
    public void ReleaseAll() {
        ReleaseUp();
        ReleaseDown();
        ReleaseLeft();
        ReleaseRight();
        ReleaseStab();
    }
    #endregion

    #region Notification Events
    /// <summary>
    /// Data parsed serialport notification event
    /// </summary>
    /// <param name="Data">string</param>
    /// <param name="RawData">string[]</param>
    void SerialReceivedEvent(string[] Data, string RawData)
    {
        if (debug)
        {
            Debug.Log("Data recieved from port: " + RawData);
        }
    }
    #endregion Notification Events


    #region Data parse functions
    /// <summary>
    /// Forwards the display data to the hardware
    /// and resets thread-safe flags. 
    /// </summary>
    private void SendCanetrollerData()
    {
       if (_CanetrollerSerialThread.IsOpenStream())
        {
            if (_BrakeUp)
            {
                SendMsg(BRAKE_CMD, UP_DIR, _BrakeUpVal);
            } else {
                SendMsg(RELEASE_CMD, UP_DIR, 0);
            }
            if (_BrakeDown)
            {
                SendMsg(BRAKE_CMD, DOWN_DIR, _BrakeDownVal);
            } else {
                SendMsg(RELEASE_CMD, DOWN_DIR, 0);
            }
            if (_BrakeRight)
            {
                SendMsg(BRAKE_CMD, RIGHT_DIR, _BrakeRightVal);
            } else {
                SendMsg(RELEASE_CMD, RIGHT_DIR, 0);
            }
            if (_BrakeLeft)
            {
                SendMsg(BRAKE_CMD, LEFT_DIR, _BrakeLeftVal);
            } else {
                SendMsg(RELEASE_CMD, LEFT_DIR, 0);
            }
            if (_BrakeStab)
            {
                SendMsg(BRAKE_CMD, STAB_DIR, _BrakeStabVal);
            } else {
                SendMsg(RELEASE_CMD, STAB_DIR, 0);
            }
        }
    }

    // Send all the zMap data to the master shape display
    private void SendMsg(int cmd, int dir, int pwm)
    {
        byte[] msg = new byte[MSG_SIZE];
        msg[0] = (byte)cmd;
        msg[1] = (byte)dir;
        msg[2] = (byte)pwm;
        _CanetrollerSerialThread.EnqueueBytesToWrite(msg);
    }

	public bool IsOpenStream() {
		return _CanetrollerSerialThread.IsOpenStream ();
	}

	public string GetComPortName() {
		if (IsOpenStream ()) {
			return _CanetrollerSerialThread.GetComPortName ();
		} 
		return "";
	}
    #endregion Data parse functions

}