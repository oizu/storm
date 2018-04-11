﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.

using System;
using System.IO;
using System.Reflection;
using CommandLine;
using Dotnet.Storm.Adapter.Channels;
using Dotnet.Storm.Adapter.Components;
using Dotnet.Storm.Adapter.Logging;
using Dotnet.Storm.Adapter.Serializers;
using log4net;
using log4net.Config;
using log4net.Core;

namespace Dotnet.Storm.Adapter
{
    class Program
    {
        private readonly static ILog Logger = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {
            // Parse command line arguments
            var parser = new Parser(with => with.EnableDashDash = true).ParseArguments<Options>(args);

            string className = null;
            string assemblyName = null;
            string arguments = null;
            LogLevel level = LogLevel.INFO;
            string serializer;
            string channel;

            parser.WithParsed(options =>
            {
                className = options.Class;
                assemblyName = options.Assembly;
                arguments = options.Arguments;
                serializer = options.Serializer;
                channel = options.Channel;

                // by default TryParse will return TRACE level in case of error
                Enum.TryParse(options.LogLevel.ToUpper(), out level);

                // if user didn't set TRACE level but we TryParse returned TRACE = parsing exception
                if (!options.LogLevel.ToLower().Equals("trace") && level == LogLevel.TRACE)
                {
                    // reset default log level to INFO
                    level = LogLevel.INFO;
                }
            });

            // Configure logging
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            if (File.Exists("log4net.config"))
            {
                XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            }
            else
            {
                XmlConfigurator.Configure(repository);
            }

            // setting up log level
            Level newLevel = StormAppender.GetLogLevel(level);

            // now wlet's add StormApender and remove all console appenders
            // initially StormAppender i in desblem mode, we'llenable it later
            StormAppender.CreateAppender(newLevel);

            Logger.Debug($"Current working directory: {Environment.CurrentDirectory}.");

            // there is an idea to use shared memory channel
            Channel.Instance = new StandardChannel
            {
                // there is an idea to use ProtoBuffer serialization 
                Serializer = new JsonSerializer()
            };

            // Instantiate component
            Type type = null;

            // className is required option so we don't need to check it for NULL
            if(!string.IsNullOrEmpty(assemblyName))
            {
                assemblyName = assemblyName.EndsWith(".dll") ? assemblyName : assemblyName + ".dll";
                string path = Path.Combine(Environment.CurrentDirectory, assemblyName);

                Logger.Debug($"Loading assembly: {assemblyName}.");
                Assembly assembly = Assembly.Load(File.ReadAllBytes(path));
                type = assembly.GetType(className, true);
            }

            Logger.Debug($"Trying to create instance of {type.Name}.");
            Component component = (Component)Activator.CreateInstance(type);

            Logger.Debug($"Setting up the arguments {arguments}.");
            component.SetArguments(arguments);

            Logger.Debug($"Executing handshake protocol.");
            component.Connect();

            Logger.Debug($"Enabling storm logging mechanism.");
            StormAppender.Enable();

            Logger.Debug($"Let's start the process.");
            component.Start();
        }

        private static string getName(string assemblyName)
        {
            string name = assemblyName.Replace(".dll", "");
            string[] parts = name.Split(new char[] { '/', '\\' });
            return parts[parts.Length - 1];

        }
    }
}
