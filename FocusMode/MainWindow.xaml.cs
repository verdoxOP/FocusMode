using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls; // For MenuItem and Menu
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FocusModeLauncher
{
    public partial class MainWindow : Window
    {
        private FocusManager focusManager;
        private DispatcherTimer timer;
        private int remainingSeconds = 1500; // 25 minutes
        private MediaPlayer mediaPlayer = new MediaPlayer(); // Class-level MediaPlayer instance

        private readonly Dictionary<string, (string ImagePath, Brush UiColor, string SoundPath)> themes = new Dictionary<string, (string, Brush, string)>
        {
            { "Roblo", ("pack://application:,,,/Images/BG2.jpg", Brushes.Red, "pack://application:,,,/Sounds/Roblo.mp3") },
            { "Alpha", ("pack://application:,,,/Images/alpha.jpg", Brushes.Navy, "pack://application:,,,/Sounds/Alpha.mp3") },
            { "SepCalling", ("pack://application:,,,/Images/sepcalling.png", Brushes.LightBlue, "pack://application:,,,/Sounds/SepCalling.mp3") },
            { "ElectroWolf", ("pack://application:,,,/Images/ElectroWolf.jpg", Brushes.DarkBlue, "pack://application:,,,/Sounds/ElectroWolf.mp3") },
            { "SepOG", ("pack://application:,,,/Images/sepOGbg.jpg", Brushes.Orange, "pack://application:,,,/Sounds/SepOG.mp3") },
            { "Surprise", ("pack://application:,,,/Images/zierickBG.png", Brushes.Purple, "pack://application:,,,/Sounds/Surprise.mp3") }
        };

        public MainWindow()
        {
            InitializeComponent();
            focusManager = new FocusManager();

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += Timer_Tick;

            PopulateThemesMenu();
        }

        private void StartFocus_Click(object sender, RoutedEventArgs e)
        {
            focusManager.StartFocus();
            remainingSeconds = 1500;
            timer.Start();
        }

        private void StopFocus_Click(object sender, RoutedEventArgs e)
        {
            timer.Stop();
            focusManager.StopFocus();
            TimerText.Text = "00:00";
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            remainingSeconds--;
            TimerText.Text = TimeSpan.FromSeconds(remainingSeconds).ToString(@"mm\:ss");

            if (remainingSeconds <= 0)
            {
                timer.Stop();
                MessageBox.Show("Focus session complete!");
                focusManager.StopFocus();
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

                    // Apply the UI color to specific elements
                    ApplyColorToUIElements(this, uiColor);

                    // Extract the sound file to a local path and play it
                    string soundPath = theme.SoundPath; // e.g., "pack://application:,,,/Sounds/SepCalling.mp3"
                    var uri = new Uri(soundPath, UriKind.RelativeOrAbsolute);
                    var streamResourceInfo = Application.GetResourceStream(uri);

                    if (streamResourceInfo != null)
                    {
                        // Create a temporary file with the correct extension
                        string tempFile = System.IO.Path.Combine(System.IO.Path.GetTempPath(), System.IO.Path.GetFileName(soundPath));
                        using (var fileStream = System.IO.File.Create(tempFile))
                        {
                            streamResourceInfo.Stream.CopyTo(fileStream);
                        }

                        // Play the sound from the temporary file
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

                // Recursively apply to child elements
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
    }
}