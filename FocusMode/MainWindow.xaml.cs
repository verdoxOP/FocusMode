using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls; // For MenuItem and Menu
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using Newtonsoft.Json;
using Formatting = Newtonsoft.Json.Formatting;

namespace FocusModeLauncher
{
    public partial class MainWindow : Window
    {
        private FocusManager focusManager;
        private DispatcherTimer timer;
        private int remainingSeconds = 1500;
        private MediaPlayer mediaPlayer = new MediaPlayer();

        private readonly Dictionary<string, (string ImagePath, Brush UiColor, string SoundPath)> themes =
            new Dictionary<string, (string, Brush, string)>
            {
                {
                    "Roblo",
                    ("pack://application:,,,/Images/BG2.jpg", Brushes.Red, "pack://application:,,,/Sounds/Roblo.mp3")
                },
                {
                    "Alpha",
                    ("pack://application:,,,/Images/alpha.jpg", Brushes.Navy, "pack://application:,,,/Sounds/Alpha.mp3")
                },
                {
                    "SepCalling",
                    ("pack://application:,,,/Images/sepcalling.png", Brushes.LightBlue,
                        "pack://application:,,,/Sounds/SepCalling.mp3")
                },
                {
                    "ElectroWolf",
                    ("pack://application:,,,/Images/ElectroWolf.jpg", Brushes.DarkBlue,
                        "pack://application:,,,/Sounds/ElectroWolf.mp3")
                },
                {
                    "SepOG",
                    ("pack://application:,,,/Images/sepOGbg.jpg", Brushes.Orange,
                        "pack://application:,,,/Sounds/SepOG.mp3")
                },
                {
                    "Surprise",
                    ("pack://application:,,,/Images/zierickBG.png", Brushes.Purple,
                        "pack://application:,,,/Sounds/Surprise.mp3")
                }
            };

        public MainWindow()
        {
            InitializeComponent();
            focusManager = new FocusManager();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            PopulateThemesMenu();
            PopulateWhiteListMenu();
            PopulateBlackListMenu();
          
            
        }

        private void StartFocus_Click(object sender, RoutedEventArgs e)
        {
            focusManager.StartFocus();
            focusManager.StartFocusSession(1500);
            timer.Start();
        }

        private void StopFocus_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            focusManager.StopFocus();
            focusManager.EndFocusSession();
            TimerText.Text = "00:00";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            TimerText.Text = TimeSpan.FromSeconds(remainingSeconds).ToString(@"mm\:ss");

            if (remainingSeconds <= 0)
            {
                timer.Stop();
                focusManager.EndFocusSession();
                MessageBox.Show("Focus session complete!");
               
            }

            if (!string.IsNullOrEmpty(selectedBlacklistConfig) &&
                focusManager.BlacklistConfigs.TryGetValue(selectedBlacklistConfig, out var blacklist))
            {
                focusManager.SetBlacklist(blacklist);
            }
           

        }

        private void Themes_OnClick(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem && menuItem.Header is string themeName)
            {
                ApplyTheme(themeName);
            }
        }

        public void ApplyTheme(string themeName)
        {
            if (themes.TryGetValue(themeName, out var theme))
            {
                try
                {
                    // Extract the image path and UI color from the theme
                    string imagePath = theme.ImagePath;
                    Brush uiColor = theme.UiColor;

                    // Set the background image
                    ImageBrush backgroundBrush = new ImageBrush
                    {
                        ImageSource = new BitmapImage(new Uri(imagePath, UriKind.RelativeOrAbsolute)),
                        Stretch = Stretch.UniformToFill
                    };
                    this.Background = backgroundBrush;


                    ApplyColorToUIElements(this, uiColor);


                    string soundPath = theme.SoundPath;
                    var uri = new Uri(soundPath, UriKind.RelativeOrAbsolute);
                    var streamResourceInfo = Application.GetResourceStream(uri);

                    if (streamResourceInfo != null)
                    {

                        string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(),
                            System.IO.Path.GetFileName(soundPath));
                        using (var fileStream = System.IO.File.Create(tempFile))
                        {
                            streamResourceInfo.Stream.CopyTo(fileStream);
                        }


                        mediaPlayer.Open(new Uri(tempFile, UriKind.Absolute));
                        mediaPlayer.Play();
                    }
                    else
                    {
                        MessageBox.Show("Failed to load sound resource.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to apply theme '{themeName}': {ex.Message}");
                }
            }
            else
            {
                MessageBox.Show($"Theme '{themeName}' not found.");
            }
        }

        private void ApplyColorToUIElements(DependencyObject parent, Brush color)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);

                if (child is Button button)
                {
                    button.Foreground = color;
                    button.BorderBrush = color;
                }
                else if (child is Menu menu)
                {
                    menu.Foreground = color;
                }
                else if (child is TextBlock textBlock)
                {
                    textBlock.Foreground = color;
                }
                else if (child is Border border)
                {
                    border.BorderBrush = color;
                }


                ApplyColorToUIElements(child, color);
            }
        }

        private void PopulateThemesMenu()
        {
            var themesMenu = new MenuItem { Header = "Themes" };

            foreach (var theme in themes)
            {
                var themeItem = new MenuItem { Header = theme.Key };
                themeItem.Click += (sender, e) => ApplyTheme(theme.Key);
                themesMenu.Items.Add(themeItem);
            }

            var menu = (Menu)LogicalTreeHelper.FindLogicalNode(this, "MainMenu");
            if (menu != null)
            {
                menu.Items.Add(themesMenu);
            }
            else
            {
                MessageBox.Show("MainMenu not found in the visual tree.");
            }
        }

        private void PopulateWhiteListMenu()
        {
            var whiteListMenu = new MenuItem { Header = "Whitelists" };

            foreach (var config in focusManager.WhitelistConfigs)
            {
                var configItem = new MenuItem { Header = config.Key };
                configItem.Click += (sender, e) => SelectWhitelistConfig(config.Key);
                whiteListMenu.Items.Add(configItem);
            }

            var menu = (Menu)LogicalTreeHelper.FindLogicalNode(this, "MainMenu");
            if (menu != null)
            {
                menu.Items.Add(whiteListMenu);
            }
            else
            {
                MessageBox.Show("MainMenu not found in the visual tree.");
            }
        }



        private string selectedWhitelistConfig;

        private void SelectWhitelistConfig(string configName)
        {
            if (focusManager.WhitelistConfigs.TryGetValue(configName, out var whitelist))
            {
                selectedWhitelistConfig = configName;
                focusManager.SetWhitelist(whitelist); // Apply the selected whitelist
                MessageBox.Show($"Whitelist '{configName}' selected and applied.", "Whitelist Selection");
            }
            else
            {
                MessageBox.Show($"Whitelist configuration '{configName}' not found.");
            }
        }


        private string selectedBlacklistConfig;

        private void PopulateBlackListMenu()
        {
            var blackListMenu = new MenuItem { Header = "Blacklists" };

            foreach (var config in focusManager.BlacklistConfigs)
            {
                var configItem = new MenuItem { Header = config.Key };
                configItem.Click += (sender, e) => SelectBlacklistConfig(config.Key);
                blackListMenu.Items.Add(configItem);
            }

            var menu = (Menu)LogicalTreeHelper.FindLogicalNode(this, "MainMenu");
            if (menu != null)
            {
                menu.Items.Add(blackListMenu);
            }
            else
            {
                MessageBox.Show("MainMenu not found in the visual tree.");
            }
        }

        private void SelectBlacklistConfig(string configName)
        {
            if (focusManager.BlacklistConfigs.ContainsKey(configName))
            {
                selectedBlacklistConfig = configName;
                MessageBox.Show($"Blacklist '{configName}' selected.", "Blacklist Selection");
            }
            else
            {
                MessageBox.Show($"Blacklist configuration '{configName}' not found.");
            }
        }

        private void CreateBlacklist_Click(object sender, RoutedEventArgs e)
        {
            var inputDialog = new InputDialog("Enter a name for the new blacklist:");
            if (inputDialog.ShowDialog() == true)
            {
                string blacklistName = inputDialog.ResponseText;
                if (!string.IsNullOrEmpty(blacklistName))
                {
                    var appInputDialog = new InputDialog("Enter applications to blacklist (comma-separated):");
                    if (appInputDialog.ShowDialog() == true)
                    {
                        string apps = appInputDialog.ResponseText;
                        var appList = apps.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                        if (!focusManager.BlacklistConfigs.ContainsKey(blacklistName))
                        {
                            focusManager.BlacklistConfigs[blacklistName] = new List<string>(appList);
                            MessageBox.Show($"Blacklist '{blacklistName}' created successfully.");

                          
                            PopulateBlackListMenu();
                        }
                        else
                        {
                            MessageBox.Show("A blacklist with this name already exists.");
                        }
                    }
                }
            }
        }

        public void SaveBlacklistsToFile(string filePath)
        {
            var json = JsonConvert.SerializeObject(focusManager.BlacklistConfigs, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public void LoadBlacklistsFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                focusManager.BlacklistConfigs = JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(json)
                                                ?? new Dictionary<string, List<string>>();
            }
        }

        public class GamificationManager
        {
            public int DailyStreak { get; private set; }
            public int HourlyReward { get; set; } = 10; // Points per hour
            public int TotalPoints { get; private set; }
            private DateTime lastUsedDate;

            public void UpdateDailyStreak()
            {
                var today = DateTime.Today;
                if (lastUsedDate.Date == today.AddDays(-1))
                {
                    DailyStreak++;
                }
                else if (lastUsedDate.Date != today)
                {
                    DailyStreak = 1; // Reset streak if not consecutive
                }
                lastUsedDate = today;
            }

            public void AddHourlyReward(int hours)
            {
                TotalPoints += hours * HourlyReward;
            }

            public void SaveProgress(string filePath)
            {
                var data = new
                {
                    DailyStreak,
                    TotalPoints,
                    LastUsedDate = lastUsedDate
                };
                File.WriteAllText(filePath, JsonConvert.SerializeObject(data, Formatting.Indented));
            }

            public void LoadProgress(string filePath)
            {
                if (File.Exists(filePath))
                {
                    var data = JsonConvert.DeserializeObject<dynamic>(File.ReadAllText(filePath));
                    DailyStreak = (int)data.DailyStreak;
                    TotalPoints = (int)data.TotalPoints;
                    lastUsedDate = (DateTime)data.LastUsedDate;
                }
            }
        }
        }
        }
    
        
        
        
    


