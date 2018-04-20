﻿/**
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
using Dotnet.Storm.Adapter.Channels;
using Dotnet.Storm.Adapter.Messaging;
using log4net;
using log4net.Appender;
using log4net.Core;

namespace Dotnet.Storm.Adapter.Logging
{
    public class StormAppender : AppenderSkeleton
    {
        internal bool Enabled { get; set; }

        protected override void Append(LoggingEvent loggingEvent)
        {
            if (Enabled)
            {
                string message = RenderLoggingEvent(loggingEvent);
                LogLevel level = GetStormLevel(loggingEvent.Level);

                Channel.Instance.Send(new LogMessage(message, level));
            }
        }

        public static LogLevel GetStormLevel(Level level)
        {
            switch (level.Name)
            {
                case "FINE":
                case "TRACE":
                case "FINER":
                case "VERBOSE":
                case "FINEST":
                case "ALL":
                    return LogLevel.TRACE;
                case "log4net:DEBUG":
                case "DEBUG":
                    return LogLevel.DEBUG;
                case "OFF":
                case "INFO":
                    return LogLevel.INFO;
                case "WARN":
                case "NOTICE":
                    return LogLevel.WARN;
                case "EMERGENCY":
                case "FATAL":
                case "ALERT":
                case "CRITICAL":
                case "SEVERE":
                case "ERROR":
                    return LogLevel.ERROR;
                default:
                    return LogLevel.INFO;
            }
        }

        public static Level GetLogLevel(LogLevel level)
        {
            switch (level)
            {
                case LogLevel.TRACE:
                    return Level.Trace;
                case LogLevel.DEBUG:
                    return Level.Debug;
                case LogLevel.INFO:
                    return Level.Info;
                case LogLevel.WARN:
                    return Level.Warn;
                case LogLevel.ERROR:
                    return Level.Error;
                default:
                    return Level.Info;
            }
        }
    }

    public static class ILogEx
    {
        public static void Metrics(this ILog log, string name, object value)
        {
            Channel.Instance.Send(new MetricMessage(name, value));
        }
    }
}
