using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.Json;

namespace CodeMemo
{



    public partial class MainWindow : Window
    {
        public class LanguageData
        {
            public Dictionary<string, List<string>> Languages { get; set; } = new Dictionary<string, List<string>>();
        }

        private string selectedLanguage = string.Empty;

        //Main Funcs
        private string vaultFilePath = "vault.json";
        private LanguageData languageData = new LanguageData();

        public MainWindow()
        {
            InitializeComponent();
            LoadLanguages();
        }
        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }





        //This is the section for modifying anything language related

        private void CreateLanguageBoxButton_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new InputDialog(languageData.Languages.Keys.ToList());
            bool? result = inputDialog.ShowDialog();

            if (result == true)
            {
                string languageName = inputDialog.newBoxName;
                CreateLanguageBox(languageName);
            }
        }

        private void CreateLanguageBox(string languageName)
        {
            if (!languageData.Languages.ContainsKey(languageName))
            {
                languageData.Languages[languageName] = new List<string>(); // Initialize with an empty list of functions
                SaveLanguages();
            }

            Image languageImage = new Image
            {
                Source = new BitmapImage(new Uri("Images/unselectedButton.PNG", UriKind.Relative)),
                Height = 34,
                Width = 124
            };

            Grid languageBoxContent = new Grid
            {
                Width = 124,
                Height = 34,
                Margin = new Thickness(0, 12, 0, 0)
            };

            TextBox languageLabel = new TextBox
            {
                Text = languageName,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 18,
                IsReadOnly = true,
                TextAlignment = TextAlignment.Center,
                Focusable = false,
                IsHitTestVisible = false,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Arrow
            };

            languageBoxContent.Children.Add(languageImage);
            languageBoxContent.Children.Add(languageLabel);

            Button optionsButton = new Button
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Width = 20,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Right,
                FocusVisualStyle = null,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            optionsButton.Style = (Style)Application.Current.Resources["CustomButtonStyle"];
            optionsButton.Click += (s, e) => ShowLanguageOptions(optionsButton, languageBoxContent);

            languageBoxContent.Children.Add(optionsButton);

            languageBoxContent.MouseLeftButtonDown += (s, e) =>
            {

                // Deselect all language boxes
                foreach (Grid languageBox in LanguagesStackPanel.Children.OfType<Grid>())
                {
                    Image image = languageBox.Children.OfType<Image>().FirstOrDefault();
                    if (image != null)
                    {
                        image.Source = new BitmapImage(new Uri("Images/unselectedButton.PNG", UriKind.Relative));
                    }
                }

                // Set the selected language box image
                Image selectedImage = languageBoxContent.Children.OfType<Image>().FirstOrDefault();
                if (selectedImage != null)
                {
                    selectedImage.Source = new BitmapImage(new Uri("Images/selectedButton.PNG", UriKind.Relative));
                }

                // Set the selected language variable
                selectedLanguage = ((TextBox)languageBoxContent.Children.OfType<TextBox>().FirstOrDefault()).Text;

                AddFunctionButton.Visibility = Visibility.Visible;

                // Load functions for the selected language
                LoadFunctionsForLanguage(selectedLanguage);

                // Make the AddFunctionButton visible once a language box is selected
                AddFunctionButton.Visibility = Visibility.Visible;
            };



            LanguagesStackPanel.Children.Insert(LanguagesStackPanel.Children.Count - 1, languageBoxContent);
            LanguageScrollViewer.ScrollToBottom();
        }

        private string GetSelectedLanguage()
        {
            return selectedLanguage;
        }


        private void SaveLanguages()
        {
            try
            {
                string json = JsonSerializer.Serialize(languageData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(vaultFilePath, json);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving languages: {ex.Message}");
            }
        }

        private void LoadLanguages()
        {
            if (File.Exists(vaultFilePath))
            {
                try
                {
                    string json = File.ReadAllText(vaultFilePath);
                    languageData = JsonSerializer.Deserialize<LanguageData>(json) ?? new LanguageData();

                    foreach (var language in languageData.Languages.Keys)
                    {
                        CreateLanguageBox(language);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error loading languages: {ex.Message}");
                }
            }
        }

        private void DeleteLanguage(Grid languageBoxContent)
        {
            TextBox languageLabel = languageBoxContent.Children.OfType<TextBox>().FirstOrDefault();
            StackPanel optionsMenu = languageBoxContent.Children.OfType<StackPanel>().LastOrDefault();
            if (languageLabel != null)
            {
                string languageName = languageLabel.Text;

                // Remove the language from the data list
                languageData.Languages.Remove(languageName);
                SaveLanguages(); // Save the updated list

                // Hide the options menu
                if (optionsMenu != null)
                {
                    optionsMenu.Visibility = Visibility.Collapsed;
                }

                // Remove the language from the UI
                LanguagesStackPanel.Children.Remove(languageBoxContent);
            }
        }

        private void ShowLanguageOptions(Button optionsButton, Grid languageBoxContent)
        {
            Popup optionsPopup = new Popup
            {
                PlacementTarget = optionsButton,
                Placement = PlacementMode.Bottom,
                StaysOpen = false, // Allow the popup to close when clicking outside
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            StackPanel optionsMenu = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = Brushes.LightGray,
                Width = 100,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button renameButton = new Button
            {
                Content = "Rename",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12,
                Width = 100,
                Height = 20,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            renameButton.Click += (s, e) =>
            {
                RenameLanguage(languageBoxContent);
                optionsPopup.IsOpen = false;
            };

            Button deleteButton = new Button
            {
                Content = "Delete",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12,
                Width = 100,
                Height = 20,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            deleteButton.Click += (s, e) =>
            {
                DeleteLanguage(languageBoxContent);
                optionsPopup.IsOpen = false;
            };

            optionsMenu.Children.Add(renameButton);
            optionsMenu.Children.Add(deleteButton);
            optionsPopup.Child = optionsMenu;

            optionsPopup.IsOpen = true;
        }

        private void RenameLanguage(Grid languageBoxContent)
        {
            // Close the menu
            StackPanel optionsMenu = languageBoxContent.Children.OfType<StackPanel>().LastOrDefault();
            if (optionsMenu != null)
            {
                optionsMenu.Visibility = Visibility.Collapsed;
            }

            // Open the rename dialog and update the label
            InputDialog inputDialog = new InputDialog(languageData.Languages.Keys.ToList());
            if (inputDialog.ShowDialog() == true)
            {
                string newLanguageName = inputDialog.newBoxName;

                // Find the current language name in the label
                TextBox languageLabel = languageBoxContent.Children.OfType<TextBox>().FirstOrDefault();
                if (languageLabel != null)
                {
                    // Update the label text in the UI
                    string oldLanguageName = languageLabel.Text;
                    languageLabel.Text = newLanguageName;

                    // Update the language data list
                    if (languageData.Languages.ContainsKey(oldLanguageName))
                    {
                        var functions = languageData.Languages[oldLanguageName];
                        languageData.Languages.Remove(oldLanguageName);
                        languageData.Languages[newLanguageName] = functions;
                        SaveLanguages(); // Save the updated list
                    }
                }
            }
        }

        private void CreateFunctionBoxButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = GetSelectedLanguage();
            if (string.IsNullOrEmpty(selectedLanguage)) return;

            // Prompt the user for a function name
            InputDialog inputDialog = new InputDialog(languageData.Languages[selectedLanguage]);
            bool? result = inputDialog.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(inputDialog.newBoxName))
            {
                string functionName = inputDialog.newBoxName;

                // Ensure the language exists in the dictionary
                if (!languageData.Languages.ContainsKey(selectedLanguage))
                {
                    languageData.Languages[selectedLanguage] = new List<string>();
                }

                // Add function and update UI
                languageData.Languages[selectedLanguage].Add(functionName);
                CreateFunctionBox(functionName);
                SaveLanguages();
            }
        }

        private void CreateFunctionBox(string functionName)
        {
            Image functionImage = new Image
            {
                Source = new BitmapImage(new Uri("Images/unselectedButton.PNG", UriKind.Relative)),
                Height = 34,
                Width = 124
            };

            TextBox functionLabel = new TextBox
            {
                Text = functionName,
                Background = Brushes.Transparent,
                Foreground = Brushes.Black,
                BorderBrush = Brushes.Transparent,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Left,
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 18,
                IsReadOnly = true,
                TextAlignment = TextAlignment.Center,
                Focusable = false,
                IsHitTestVisible = false,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Arrow
            };

            Grid functionBoxContent = new Grid
            {
                Width = 124,
                Height = 34,
                Margin = new Thickness(0, 12, 0, 0),
                Tag = "FunctionBox" // Tag to identify the function box
            };

            functionBoxContent.Children.Add(functionImage);
            functionBoxContent.Children.Add(functionLabel);

            // Add the options button for each function
            Button optionsButton = new Button
            {
                Background = Brushes.Transparent,
                BorderBrush = Brushes.Transparent,
                Width = 20,
                Height = 20,
                HorizontalAlignment = HorizontalAlignment.Right,
                FocusVisualStyle = null,
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Hand
            };

            optionsButton.Style = (Style)Application.Current.Resources["CustomButtonStyle"];
            optionsButton.Click += (s, e) => ShowFunctionOptions(optionsButton, functionBoxContent);

            functionBoxContent.Children.Add(optionsButton);

            // Insert the function box at the beginning of the stack panel
            FunctionsStackPanel.Children.Insert(FunctionsStackPanel.Children.Count - 1, functionBoxContent);

            // Scroll to the bottom of the list (optional)
            FunctionsScrollViewer.ScrollToBottom();
        }

        private void LoadFunctionsForLanguage(string selectedLanguage)
        {
            // Get the existing "Add Function" button to preserve it
            Button addFunctionButton = FunctionsStackPanel.Children.OfType<Button>().FirstOrDefault();

            // Clear only the function boxes, not the add button
            var functionBoxes = FunctionsStackPanel.Children.OfType<Grid>().ToList();
            foreach (var box in functionBoxes)
            {
                FunctionsStackPanel.Children.Remove(box);
            }

            // Check if the selected language has functions
            if (languageData.Languages.ContainsKey(selectedLanguage))
            {
                // Get the list of functions for the selected language
                var functions = languageData.Languages[selectedLanguage];

                // Loop through and add each function box
                foreach (var function in functions)
                {
                    CreateFunctionBox(function);
                }
            }
            else
            {
                // Handle case where no functions are available for the selected language
                Console.WriteLine("No functions available for the selected language.");
            }

            // If the "Add Function" button was found, re-add it at the bottom of the stack panel
            if (addFunctionButton != null)
            {
                // Make sure it stays visible
                addFunctionButton.Visibility = Visibility.Visible;

                // Insert the "Add Function" button at the bottom of the list
                if (!FunctionsStackPanel.Children.Contains(addFunctionButton))
                {
                    FunctionsStackPanel.Children.Add(addFunctionButton);
                }
            }

            // Optionally, scroll to the bottom of the ScrollViewer
            FunctionsScrollViewer.ScrollToBottom();
        }


        private void ShowFunctionOptions(Button optionsButton, Grid functionBoxContent)
        {
            Popup optionsPopup = new Popup
            {
                PlacementTarget = optionsButton,
                Placement = PlacementMode.Bottom,
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Fade
            };

            StackPanel optionsMenu = new StackPanel
            {
                Orientation = Orientation.Vertical,
                Background = Brushes.LightGray,
                Width = 100,
                Height = 40,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button renameButton = new Button
            {
                Content = "Rename",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12,
                Width = 100,
                Height = 20,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            renameButton.Click += (s, e) =>
            {
                RenameFunction(functionBoxContent);
                optionsPopup.IsOpen = false;
            };

            Button deleteButton = new Button
            {
                Content = "Delete",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12,
                Width = 100,
                Height = 20,
                Background = Brushes.Transparent,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            deleteButton.Click += (s, e) =>
            {
                DeleteFunction(functionBoxContent);
                optionsPopup.IsOpen = false;
            };

            optionsMenu.Children.Add(renameButton);
            optionsMenu.Children.Add(deleteButton);
            optionsPopup.Child = optionsMenu;

            optionsPopup.IsOpen = true;
        }

        // This method handles deleting a function from the UI and the language data
        private void DeleteFunction(Grid functionBoxContent)
        {
            TextBox functionLabel = functionBoxContent.Children.OfType<TextBox>().FirstOrDefault();
            if (functionLabel != null)
            {
                string functionName = functionLabel.Text;
                string selectedLanguage = GetSelectedLanguage();

                // Remove the function from the language's list of functions
                if (languageData.Languages.ContainsKey(selectedLanguage))
                {
                    languageData.Languages[selectedLanguage].Remove(functionName);
                    SaveLanguages(); // Save the updated list
                }

                // Remove the function box from the UI
                FunctionsStackPanel.Children.Remove(functionBoxContent);
            }
        }

        // This method handles renaming a function
        private void RenameFunction(Grid functionBoxContent)
        {
            TextBox functionLabel = functionBoxContent.Children.OfType<TextBox>().FirstOrDefault();
            if (functionLabel != null)
            {
                string oldFunctionName = functionLabel.Text;
                InputDialog inputDialog = new InputDialog(languageData.Languages.Keys.ToList()); // Create a new input dialog for the function name
                if (inputDialog.ShowDialog() == true)
                {
                    string newFunctionName = inputDialog.newBoxName;

                    // Update the function name in the UI
                    functionLabel.Text = newFunctionName;

                    // Update the function in the language data
                    string selectedLanguage = GetSelectedLanguage();
                    if (languageData.Languages.ContainsKey(selectedLanguage))
                    {
                        var functions = languageData.Languages[selectedLanguage];
                        functions[functions.IndexOf(oldFunctionName)] = newFunctionName;
                        SaveLanguages(); // Save the updated list
                    }
                }
            }
        }



    }
}