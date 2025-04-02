using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CodeMemo
{
    public partial class InputDialog : Window
    {
        private List<string> existingLanguages;  // List to hold existing language names
        public string newBoxName { get; set; }
        // Constructor now accepts a list of existing languages from the JSON
        public InputDialog(List<string> existingLanguages)
        {
            InitializeComponent();
            this.existingLanguages = existingLanguages;
        }

        // Automatically focus the TextBox when the dialog is shown
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            NameTextBox.Focus();  // Set focus to the TextBox
        }

        // Handle the OK button click to return the input
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            ValidateAndCloseDialog();
        }

        // Handle the Enter key press
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.Key == Key.Enter)
            {
                ValidateAndCloseDialog();
            }
        }

        // Validate the input and close the dialog if valid
        private void ValidateAndCloseDialog()
        {
            string inputName = NameTextBox.Text.Trim();

            // Check if the input is null or empty
            if (string.IsNullOrEmpty(inputName))
            {
                MessageBox.Show("Name cannot be empty.", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Check if the name already exists in the existing languages from the JSON
            if (existingLanguages.Contains(inputName))
            {
                MessageBox.Show("This name is already in use. Please choose a different name.", "Name Taken", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // If validation passes, set the newBoxName and close the dialog
            newBoxName = inputName;
            this.DialogResult = true;
        }
    }
}