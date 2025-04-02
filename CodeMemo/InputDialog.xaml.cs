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
        public string LanguageName { get; set; }

        public InputDialog()
        {
            InitializeComponent();
        }

        // Automatically focus the TextBox when the dialog is shown
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            LanguageNameTextBox.Focus();  // Set focus to the TextBox
        }

        // Handle the OK button click to return the input
        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the entered language name and close the dialog
            LanguageName = LanguageNameTextBox.Text;
            this.DialogResult = true;
        }

        // Handle the Enter key press
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            // Check if the Enter key was pressed
            if (e.Key == Key.Enter)
            {
                LanguageName = LanguageNameTextBox.Text;  // Get the language name
                this.DialogResult = true;  // Close the dialog with the result
            }
        }
    }
}
