using System.Diagnostics;

namespace FocusModeLauncher
{
    public class FocusManager
    {
        
        public void SetWhitelist(List<string> whitelist)
        {
            this.whitelist = whitelist.ToArray(); // Update the active whitelist

            // Logic to apply the whitelist
            Debug.WriteLine("Whitelist applied: " + string.Join(", ", whitelist));

            // Optionally, launch whitelisted apps immediately
        
        }

        public List<string> Whitelist { get; set; } = new List<string>();
        private string[] blacklist = { "discord", "steam", "chrome" };

        private string[] whitelist =
        {
            @"C:\Users\mattn\AppData\Local\JetBrains\Installations\Rider243\bin\rider64.exe",
            "https://shorturl.at/ZYrJs", "https://chatgpt.com/",
            @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpnui.exe",
            @"C:\Program Files\Microsoft Teams\current\Teams.exe"
        };

        private readonly string[] BlacklistSC =
        {
            "discord.exe", "steam.exe", "Brave.exe", "chrome.exe", "firefox.exe", "Microsoft Teams.exe",
            "MicrosoftTeams.exe", "Spotify.exe", "Zoom.exe", "slack.exe", "Skype.exe", "EpicGamesLauncher",
            "EpicGamesLauncher.exe", "Battle.net.exe", "Origin.exe", "Uplay.exe", "GOG Galaxy.exe", "Twitch.exe",
            "OBS Studio.exe", "NVIDIA GeForce Experience.exe", "Razer Synapse.exe", "Logitech Gaming Software.exe",
            "Corsair iCUE.exe", "MSI Afterburner.exe", "TeamViewer.exe", "AnyDesk.exe", "ZoomLauncher.exe",
            "wallpaper engine", "wallpaper32.exe"
        };

        public void StartFocus()
        {
            KillBlacklistedApps();
            LaunchWhitelistedApps();
        }

        public void StopFocus()
        {
      
        }

        private void KillBlacklistedApps()
        {
            foreach (var proc in Process.GetProcesses())
            {
                try
                {
   
                    string processName = proc.ProcessName.ToLower();

                    if (blacklist.Any(b => processName.Contains(b.ToLower())))
                    {
                        proc.Kill();
                        Debug.WriteLine($"Killed process: {proc.ProcessName}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to kill process {proc.ProcessName}: {ex.Message}");
                }
            }
        }

        private void LaunchWhitelistedApps()
        {
            foreach (var path in whitelist)
            {
                try
                {
                    if (Uri.IsWellFormedUriString(path, UriKind.Absolute))
                    {
                        // Open the URL in the default web browser
                        Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
                    }
                    else
                    {
                        // Check if the application is already running
                        string processName = System.IO.Path.GetFileNameWithoutExtension(path);
                        var existingProcess = Process.GetProcessesByName(processName).FirstOrDefault();

                        if (existingProcess != null)
                        {
                            // Bring the existing process to the foreground
                            IntPtr handle = existingProcess.MainWindowHandle;
                            if (handle != IntPtr.Zero)
                            {
                                SetForegroundWindow(handle);
                            }
                        }
                        else
                        {
                            // Launch the application with a working directory
                            var startInfo = new ProcessStartInfo
                            {
                                FileName = path,
                                UseShellExecute = true,
                                WorkingDirectory = System.IO.Path.GetDirectoryName(path) ?? string.Empty
                            };
                            Process.Start(startInfo);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to launch: {path}. Error: {ex}");
                }
            }
        }

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        public string[] GetWhitelist()
        {
            return new string[]
            {
                @"C:\Users\mattn\AppData\Local\JetBrains\Installations\Rider243\bin\rider64.exe",
                "https://shorturl.at/ZYrJs",
                "https://chatgpt.com/",
                @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpnui.exe",
                @"C:\Program Files\Microsoft Teams\current\Teams.exe"
            };
        }



        public Dictionary<string, List<string>> WhitelistConfigs { get; set; } = new Dictionary<string, List<string>>
        {
            {
                "Whitelist1", new List<string>
                {
                    @"C:\Users\mattn\AppData\Local\JetBrains\Installations\Rider243\bin\rider64.exe",
                    "https://shorturl.at/ZYrJs",
                    "https://chatgpt.com/",
                    @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpnui.exe",
                    @"C:\Program Files\Microsoft Teams\current\Teams.exe"
                }
            },
            
            {
                "NoWhitelist", new List<string>
                {
                    "",
                    @""
                }
            }
        };
            
            public Dictionary<string, List<string>> BlacklistConfigs { get; set; } = new Dictionary<string, List<string>>
{
    {
        "BlacklistSC", new List<string>
        {
            "discord", "steam.exe", "Brave", "chrome", "firefox", "Microsoft Teams",
            "MicrosoftTeams.exe", "Spotify", "Zoom.exe", "slack.exe", "Skype.exe", "EpicGamesLauncher",
            "EpicGamesLauncher", "Battle.net.exe", "Origin.exe", "Uplay.exe", "GOG Galaxy.exe", "Twitch.exe",
            "OBS Studio.exe", "NVIDIA GeForce Experience.exe", "Razer Synapse.exe", "Logitech Gaming Software.exe",
            "Corsair iCUE.exe", "MSI Afterburner.exe", "TeamViewer.exe", "AnyDesk.exe", "ZoomLauncher.exe",
            "wallpaper engine", "wallpaperengine.exe"
        }
    },
    {
        "Blacklist2", new List<string>
        {
            "spotify.exe",
            "slack.exe",
            "zoom.exe"
        }
    }
};
            
public void SetBlacklist(List<string> blacklist)
{
    this.blacklist = blacklist.ToArray();
}
        };
        }

    

        
        
    
