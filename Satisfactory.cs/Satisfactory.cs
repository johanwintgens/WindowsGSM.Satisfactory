﻿using System;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using WindowsGSM.Functions;
using WindowsGSM.GameServer.Query;
using WindowsGSM.GameServer.Engine;

namespace WindowsGSM.Plugins
{
    public class Satisfactory : SteamCMDAgent
    {
        // - Plugin Details
        public Plugin Plugin = new Plugin
        {
            name = "WindowsGSM.Satisfactory",
            author = "johanwintgens",
            description = "WindowsGSM plugin for supporting Satisfactory Dedicated Server",
            version = "1.0",
            url = "https://github.com/johanwintgens/WindowsGSM.Satisfactory",
            color = "#ffffff" // Color Hex
        };

        // - Settings properties for SteamCMD installer
        public override bool loginAnonymous => true;
        public override string AppId => "1690800"; // Game server appId

        // - Standard Constructor and properties
        public Satisfactory(ServerConfig serverData) : base(serverData) => base.serverData = _serverData = serverData;
        private readonly ServerConfig _serverData;


        // - Game server Fixed variables
        public override string StartPath => @"FactoryServer.exe"; // Game server start path
        public string FullName = "Satisfactory Dedicated Server"; // Game server FullName
        public bool AllowsEmbedConsole = true;
        public int PortIncrements = 1; // This tells WindowsGSM how many ports should skip after installation
        public object QueryMethod = new A2S(); // Query method should be use on current server type. Accepted value: null or new A2S() or new FIVEM() or new UT3()


        // - Game server default values
        public string Port = "7777"; // Default port
        public string QueryPort = "157777"; // Default query port
        public string Defaultmap = "Dedicated"; // Default map name
        public string Maxplayers = "10"; // Default maxplayers
        public string Additional = "-log -unattended"; // Additional server start parameter


        // - Create a default cfg for the game server after installation
        public async void CreateServerCFG()
        {
            // Satisfy log
            await Task.CompletedTask;
        }

        // - Start server function, return its Process to WindowsGSM
        public async Task<Process> Start()
        {
            // Satisfy log
            await Task.CompletedTask;

            string fileName = Functions.ServerPath.GetServersServerFiles(_serverData.ServerID, StartPath);

            if (!File.Exists(fileName))
            {
                Error = $"{Path.GetFileName(fileName)} not found ({fileName})";
                return null;
            }

            // Prepare Process
            var p = new Process
            {
                EnableRaisingEvents = true,
                StartInfo =
                {
                    WorkingDirectory = ServerPath.GetServersServerFiles(_serverData.ServerID),
                    FileName = fileName,
                    Arguments = Additional,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            // Set up Redirect Input and Output to WindowsGSM Console if EmbedConsole is on
            if (AllowsEmbedConsole)
            {
                p.StartInfo.CreateNoWindow = true;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;

                var serverConsole = new ServerConsole(_serverData.ServerID);

                p.OutputDataReceived += serverConsole.AddOutput;
                p.ErrorDataReceived += serverConsole.AddOutput;
            }

            // Start Process
            try
            {
                p.Start();
            }
            catch (Exception e)
            {
                // Return null upon crashing
                Error = e.Message;
                return null;
            }

            if (AllowsEmbedConsole)
            {
                p.BeginOutputReadLine();
                p.BeginErrorReadLine();
            }

            return p;
        }


        // - Stop server function
        public async Task Stop(Process p)
        {
            await Task.Run(() =>
            {
                Functions.ServerConsole.SetMainWindow(p.MainWindowHandle);
                Functions.ServerConsole.SendWaitToMainWindow("^c");
            });

            await Task.Delay(2000);
        }
    }
}