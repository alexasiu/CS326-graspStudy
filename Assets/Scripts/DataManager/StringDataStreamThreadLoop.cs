/// <summary>
/// Handles string data streaming threads. Create StringDataStreamThreadLoop objects 
/// for each thread that requires any data communication.
/// 
/// Author: A. Siu
/// June 30, 2019
/// </summary>
/// 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using UnityEngine;

namespace thrThreadLoop
{
    public class StringDataStreamThreadLoop : ThreadLoop
    {

        private string _Name;

        private Queue<String> DataToWrite = new Queue<String>();

        private double _MaximumLoopSpan;
        private bool _CloseWheneverPossible = false;

        public StringDataStreamThreadLoop(string strThreadName, Boolean bolIsBackground, System.Threading.ThreadPriority ePriority, Double numTimerInterval, Double numMaximumLoopSpan)
            : base(strThreadName, bolIsBackground, ePriority, numTimerInterval)
        {
            this._Name = strThreadName;
            this._MaximumLoopSpan = numMaximumLoopSpan;
        }

        /// <summary>
        /// Call to kill thread immediately.
        /// </summary>
        public void CloseAtConvenience()
        {
            this._CloseWheneverPossible = true;
        }

        protected override void DoMainProcess()
        {
            if (this._CloseWheneverPossible)
            {
                //Debug.Log("closing data stream");
                this.CloseStream();
                this.Close();
                return;
            }

            bool bolLoopActivity = false;
            bool bolDidOne = true;
            string string2Write;
            while (bolDidOne && this.LoopSpan().TotalMilliseconds < this._MaximumLoopSpan)
            {
                bolDidOne = false;
                string2Write = this.GetStringToWrite();
                if (string2Write != null)
                {
                    bolDidOne = true;
                    this.WriteDataStream(string2Write);
                }

                if (this._CloseWheneverPossible)
                {
                    this.CloseStream();
                    this.Close();
                    return;
                }

                if (this.ReadDataStream())
                {
                    bolDidOne = true;
                }

                if (this._CloseWheneverPossible)
                {
                    this.CloseStream();
                    this.Close();
                    return;
                }

                if (bolDidOne) bolLoopActivity = true;
            }
            if (bolLoopActivity)
            {
                this.UpdateLastLoopActivity();
            }
            else
            {
                this.NoLoopActivityProcess();
            }
            this.ResetProcess();
        }

        private void ResetProcess()
        {
            if (this.BugResetCondition())
            {
                //the low level RS code has a bug that need to be reset every once in a while
                //because otherwise it consumes the CPU and the program gets stuck
                this.ReportResetBug();
                this.SetNextBugResetDateTime();
                this.CloseStream();
            }
            else if (this.NoDataTimeOutCondition())
            {
                //if it's been a while without receiving a data, close it and the try to open next
                this.ReportNoDataTimeOut();
                this.SetNextNoDataTimeOutDateTime();
                this.CloseStream();
            }
            if (!this.IsOpenStream())
            {
                //try to re-open the conection
                this.OpenStream();
            }
        }

        /// <summary>
        /// Enqueues the string to write.
        /// </summary>
        /// <param name="s">A string.</param>
        public void EnqueueStringToWrite(string s)
        {
            this.DataToWrite.Enqueue(s);
        }

        /// <summary>
        /// Get the next data from the queue to write.
        /// </summary>
        private string GetStringToWrite()
        {
            if (this.DataToWrite.Count == 0) return null;
            return this.DataToWrite.Dequeue();
        }
        public virtual void OpenStream()
        {
        }
        public virtual bool IsOpenStream()
        {
            return true;
        }
        public virtual void CloseStream()
        {
        }
        public virtual void WriteDataStream(string s)
        {
        }
        public virtual bool ReadDataStream()
        {
            return true;
        }
    }
}