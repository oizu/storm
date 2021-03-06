﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Dotnet.Storm.Adapter.Channels;
using Dotnet.Storm.Adapter.Messaging;
using log4net;
using Newtonsoft.Json;

namespace Dotnet.Storm.Adapter.Components
{
    public abstract class Component
    {
        #region Component interface
        protected void Sync()
        {
            Channel.Send(new SyncMessage());
        }

        protected void Error(string message)
        {
            Channel.Send(new ErrorMessage(message));
        }

        protected void Metrics(string name, object value)
        {
            Channel.Send(new MetricMessage(name, value));
        }

        protected VerificationResult VerifyInput(string component, string stream, List<object> tuple)
        {
            if(stream == "__heartbeat" || stream == "__tick")
            {
                return new VerificationResult(false, "Input: OK");
            }
            if (string.IsNullOrEmpty(component))
            {
                return new VerificationResult(true, "Input: component is null");
            }
            if (string.IsNullOrEmpty(stream))
            {
                return new VerificationResult(true, "Input: stream is null");
            }
            if (tuple == null)
            {
                return new VerificationResult(true, "Input: tuple is null");
            }
            if (!Context.SourceToStreamToFields.ContainsKey(component))
            {
                return new VerificationResult(true, $"Input: component '{component}' is not defined as an input");
            }
            if (!Context.SourceToStreamToFields[component].ContainsKey(stream))
            { 
                return new VerificationResult(true, $"Input: component '{component}' doesn't contain '{stream}' stream");
            }
            int count = Context.SourceToStreamToFields[component][stream].Count;
            if (count != tuple.Count)
            {
                return new VerificationResult(true, $"Input: tuple contains [{tuple.Count}] fields but the {stream} stream can process only [{count}]");
            }
            return new VerificationResult(false, "Input: OK");
        }

        protected VerificationResult VerifyOutput(string stream, List<object> tuple)
        {
            if (string.IsNullOrEmpty(stream))
            {
                return new VerificationResult(true, "Output: stream is null");
            }
            if (tuple == null)
            {
                return new VerificationResult(true, "Output: tuple is null");
            }
            if (!Context.StreamToOputputFields.ContainsKey(stream))
            {
                return new VerificationResult(true, $"Output: component doesn't contain {stream} stream");
            }
            int count = Context.StreamToOputputFields[stream].Count;
            if (count != tuple.Count)
            {
                return new VerificationResult(true, $"Output: tuple contains [{tuple.Count}] fields but the {stream} stream can process only [{count}]");
            }
            return new VerificationResult(false, "Output: OK");
        }

        protected readonly static ILog Logger = LogManager.GetLogger(typeof(Component));

        protected string[] Arguments { get; private set; }

        public IDictionary<string, object> Configuration { get; internal set; }

        public StormContext Context { get; internal set; }

        internal Channel Channel { get; set; }

        protected bool IsGuaranteed
        {
            get
            {
                if (!Configuration.ContainsKey("topology.acker.executors"))
                    return false;

                object number = Configuration["topology.acker.executors"];

                if (number == null)
                {
                    return true;
                }

                if (int.TryParse(number.ToString(), out int result))
                {
                    return result != 0;
                }

                return false;
            }
        }

        protected int MessageTimeout
        {
            get
            {
                if (!Configuration.ContainsKey("topology.message.timeout.secs"))
                    return 30;

                object number = Configuration["topology.message.timeout.secs"];
                if (number == null)
                {
                    return 30;
                }
                if (int.TryParse(number.ToString(), out int result))
                {
                    return result;
                }
                return 30;
            }
        }
        #endregion

        internal void SetArguments(string line)
        {
            if(!string.IsNullOrEmpty(line))
            {
                Arguments = line.Split(new char[] { ' ' });
            }
            else
            {
                Arguments = new string[0];
            }
        }

        internal void Connect()
        {
            // waiting for storm to send connect message
            Logger.Debug("Waiting for connect message.");
            ConnectMessage message = (ConnectMessage)Channel.Receive<ConnectMessage>();

            int pid = Process.GetCurrentProcess().Id;

            // storm requires to create empty file named with PID
            if (!string.IsNullOrEmpty(message.PidDir) && Directory.Exists(message.PidDir))
            {
                Logger.Debug($"Creating pid file. PidDir: {message.PidDir}; PID: {pid}");
                string path = Path.Combine(message.PidDir, pid.ToString());
                File.WriteAllText(path, "");
            }

            Logger.Debug($"Current context: {JsonConvert.SerializeObject(message.Context)}.");
            Context = message.Context;

            Logger.Debug($"Current config: {JsonConvert.SerializeObject(message.Configuration)}.");
            Configuration = message.Configuration;

            // send PID back to storm
            Channel.Send(new PidMessage(pid));
        }

        internal abstract void Start();
    }
}
