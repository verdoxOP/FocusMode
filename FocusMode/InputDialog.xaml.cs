using System.Windows;

namespace FocusModeLauncher
{
    public partial class InputDialog : Window
    {
        public string ResponseText { get; private set; }

        public InputDialog(string prompt)
        {
            InitializeComponent(); // Initializes the XAML components
            PromptText.Text = prompt; // Set the prompt text
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = InputTextBox.Text; // Retrieve user input
            DialogResult = true; // Close the dialog with a positive result
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false; // Close the dialog with a negative result
        }
    }
}