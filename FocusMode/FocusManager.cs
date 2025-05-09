using System.Diagnostics;

namespace FocusModeLauncher
{
    public class FocusManager
    {
        private readonly string[] blacklist = { "discord", "steam", "chrome" };
        private readonly string[] whitelist = { @"C:\Users\mattn\AppData\Local\JetBrains\Installations\Rider243\bin\rider64.exe", "https://shorturl.at/ZYrJs", "https://chatgpt.com/", @"C:\Program Files (x86)\Cisco\Cisco AnyConnect Secure Mobility Client\vpnui.exe", @"C:\Program Files\Microsoft Teams\current\Teams.exe"} ;
        private readonly string[] BlacklistSC = { "discord.exe", "steam.exe", "Brave.exe", "chrome.exe", "firefox.exe", "Microsoft Teams.exe", "MicrosoftTeams.exe", "Spotify.exe", "Zoom.exe", "slack.exe", "Skype.exe", "EpicGamesLauncher", "EpicGamesLauncher.exe", "Battle.net.exe", "Origin.exe", "Uplay.exe", "GOG Galaxy.exe", "Twitch.exe", "OBS Studio.exe", "NVIDIA GeForce Experience.exe", "Razer Synapse.exe", "Logitech Gaming Software.exe", "Corsair iCUE.exe", "MSI Afterburner.exe", "TeamViewer.exe", "AnyDesk.exe", "ZoomLauncher.exe", "wallpaper engine", "wallpaperengine.exe" };

        public void StartFocus()
        {
            KillBlacklistedApps();
            LaunchWhitelistedApps();
        }

        public void StopFocus()
        {
            // Optional: Clean-up or restart apps that were closed
        }

        private void KillBlacklistedApps()
        {
            foreach (var proc in Process.GetProcesses())
            {
                if (blacklist.Contains(proc.ProcessName.ToLower()))
                {
                    try { proc.Kill(); } catch { }
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
                
                
            }
        }
        
        
    
