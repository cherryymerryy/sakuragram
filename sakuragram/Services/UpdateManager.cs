using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using Microsoft.UI.Xaml;

namespace sakuragram.Services;

public class UpdateManager
{
    public string _newVersion;
    public AsyncCompletedEventHandler _asyncCompletedEventHandler;
    
    [Obsolete("Obsolete")]
    public bool CheckForUpdates()
    {
        var newVersion = ThisAssembly.Git.BaseTag;
        var currentVersion = Config.AppVersion;

        newVersion = newVersion.Replace(".", "");
        currentVersion = currentVersion.Replace(".", "");

        if (Convert.ToInt32(newVersion) > Convert.ToInt32(currentVersion))
        {
            _newVersion = newVersion;
            return true;
        }
        else
        {
            _newVersion = currentVersion;
            return false;
        }
    }

    public void InitScript()
    {
        string path = AppContext.BaseDirectory + @"\installUpdate.bat";
        
        Process process = new Process();
        process.StartInfo.FileName = path;
        process.StartInfo.Arguments = "";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.CreateNoWindow = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.Verb = "runas";
        process.Start();
        Environment.Exit(1);
    }

    [Obsolete("Obsolete")]
    public void DownloadUpdate()
    {
        WebClient client = new WebClient();
        string path = AppContext.BaseDirectory + @"\sakuragram_Release_x64.msi"; 
        client.DownloadFileCompleted += _asyncCompletedEventHandler;
        client.DownloadFileAsync(new Uri(Config.LinkForUpdate), path);
    }

    public void Update()
    {
        InitScript();
    }
}