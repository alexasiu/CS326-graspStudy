/// <summary>
/// Handles threaded file logging. Create FileLogThreadLoop objects 
/// for threads that handles asynchronous write to file.
/// 
/// Author: A. Siu, M. Lin
/// August 21, 2019
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;

using System.Diagnostics; // for the stopwatch

using UnityEngine;

namespace thrThreadLoop
{
    public class FileLoggerThreadLoop : StringDataStreamThreadLoop
    {
        // File stream
        FileStream fs;
        
        #region serial port settings
        // Member variables
        private string _FileName;
        private string _FilePath;
        private int _WriteTimeout = 10;
        #endregion serial port settings

        private bool _isFileStreamOpen = false;

        // The values separator used to create the ParsedData
        private char ValuesSeparator = '\n';

        public FileLoggerThreadLoop(string strThreadName, Boolean bolIsBackground,
            System.Threading.ThreadPriority ePriority, Double numTimerInterval, Double numMaximumLoopSpan,
            string fileName, string filePath, int writeTimeout)
            : base(strThreadName, bolIsBackground, ePriority, numTimerInterval, numMaximumLoopSpan)
        {
            _FileName = fileName;
            _FilePath = filePath;
            _WriteTimeout = writeTimeout;

            // open stream to file
            this.OpenStream();
        }

        /// <summary>
        /// Opens the stream to the filepath here.
        /// </summary>
        public override void OpenStream()
        {
            string[] paths2Concat = new string[2];
            paths2Concat[0] = _FilePath; paths2Concat[1] = _FileName; 
            // Use this concatenation method so it handles Windows/Unix path system
            string fullFilePath = Path.Combine(paths2Concat);
            int k = 0;  // copy number of file
            string temp = fullFilePath + k;
            // Find a file name that works
            while (File.Exists(temp)) {
                k ++;
                temp = fullFilePath + k;
            }
            fullFilePath = temp;
            // Open file stream
            fs = File.Create(fullFilePath);
            _isFileStreamOpen = true;
        }

        public override bool IsOpenStream()
        {
            return _isFileStreamOpen;
        }

        public override void CloseStream()
        {
            try {
                fs.Dispose();
            } catch (Exception ex) {
                UnityEngine.Debug.Log("Tried to close already closed file stream. " + ex.ToString());
            }
        }

        public override void WriteDataStream(string s)
        {
            // If stream is not open yet, open it.
            if (this.IsOpenStream() == false)
            {
                this.OpenStream();
            }
            byte[] info = new UTF8Encoding(true).GetBytes(s + ValuesSeparator);
            fs.Write(info, 0, info.Length);
        }

    }
}