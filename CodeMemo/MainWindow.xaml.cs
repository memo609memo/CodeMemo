using System.Diagnostics;
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
            public Dictionary<string, Dictionary<string, Dictionary<string, string>>> Languages { get; set; } = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();

        }

        private string selectedLanguage = string.Empty;
        private string selectedFunction = string.Empty;


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
                languageData.Languages[languageName] = new Dictionary<string, Dictionary<string, string>>();
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
                FunctionTextScrollViewer.Visibility = Visibility.Hidden;

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

            // Add right-click event handler
            languageBoxContent.MouseRightButtonDown += (s, e) =>
            {
                ShowLanguageContextMenu(languageBoxContent);
            };

            LanguagesStackPanel.Children.Insert(LanguagesStackPanel.Children.Count - 1, languageBoxContent);
            LanguageScrollViewer.ScrollToBottom();
        }

        private string GetSelectedLanguage()
        {
            return selectedLanguage;
        }

        private string GetSelectedFunction()
        {
            return selectedFunction;
        }

        private void SaveLanguages()
        {
            try
            {
                string json = JsonSerializer.Serialize(languageData, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(vaultFilePath, json); // Save the full structure
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

                    // Load all languages and their functions
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

        private void ShowLanguageContextMenu(Grid languageBoxContent)
        {
            ContextMenu contextMenu = new ContextMenu
            {
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12
            };

            MenuItem renameMenuItem = new MenuItem
            {
                Header = "Rename",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12
            };
            renameMenuItem.Click += (s, e) => RenameLanguage(languageBoxContent);

            MenuItem deleteMenuItem = new MenuItem
            {
                Header = "Delete",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12
            };
            deleteMenuItem.Click += (s, e) => DeleteLanguage(languageBoxContent);

            contextMenu.Items.Add(renameMenuItem);
            contextMenu.Items.Add(deleteMenuItem);

            contextMenu.IsOpen = true;
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
                        languageData.Languages[newLanguageName] = functions; // Keep functions
                        SaveLanguages();
                    }
                }
            }
        }

        private void CreateFunctionBoxButton_Click(object sender, RoutedEventArgs e)
        {
            string selectedLanguage = GetSelectedLanguage();
            if (string.IsNullOrEmpty(selectedLanguage)) return;

            InputDialog inputDialog = new InputDialog(languageData.Languages[selectedLanguage].Keys.ToList());
            bool? result = inputDialog.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(inputDialog.newBoxName))
            {
                string functionName = inputDialog.newBoxName;

                // Ensure the language exists in the dictionary
                if (!languageData.Languages.ContainsKey(selectedLanguage))
                {
                    languageData.Languages[selectedLanguage] = new Dictionary<string, Dictionary<string, string>>();
                }

                // Add function with empty dictionary for "Block" and "keybind"
                languageData.Languages[selectedLanguage][functionName] = new Dictionary<string, string>
                {
                    { "Block", "" },
                    { "keybind", "" }
                };

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

            functionBoxContent.MouseLeftButtonDown += (s, e) =>
            {
                foreach (Grid functionBox in FunctionsStackPanel.Children.OfType<Grid>())
                {
                    Image image = functionBox.Children.OfType<Image>().FirstOrDefault();
                    if (image != null)
                    {
                        image.Source = new BitmapImage(new Uri("Images/unselectedButton.PNG", UriKind.Relative));
                    }
                }

                // Set the selected language box image
                Image selectedImage = functionBoxContent.Children.OfType<Image>().FirstOrDefault();
                if (selectedImage != null)
                {
                    selectedImage.Source = new BitmapImage(new Uri("Images/selectedButton.PNG", UriKind.Relative));
                }

                // Show the ScrollViewer with the TextBox
                FunctionTextScrollViewer.Visibility = Visibility.Visible;
            };

            functionBoxContent.MouseLeftButtonDown += FunctionBox_Click;

            // Add right-click event handler
            functionBoxContent.MouseRightButtonDown += (s, e) =>
            {
                ShowFunctionContextMenu(functionBoxContent);
            };

            // Insert the function box at the beginning of the stack panel
            FunctionsStackPanel.Children.Insert(FunctionsStackPanel.Children.Count - 1, functionBoxContent);

            // Scroll to the bottom of the list (optional)
            FunctionsScrollViewer.ScrollToBottom();
        }

        private void ShowFunctionContextMenu(Grid functionBoxContent)
        {
            ContextMenu contextMenu = new ContextMenu
            {
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12
            };

            MenuItem renameMenuItem = new MenuItem
            {
                Header = "Rename",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12
            };
            renameMenuItem.Click += (s, e) => RenameFunction(functionBoxContent);

            MenuItem deleteMenuItem = new MenuItem
            {
                Header = "Delete",
                FontFamily = (FontFamily)Application.Current.Resources["KalamFont"],
                FontSize = 12
            };
            deleteMenuItem.Click += (s, e) => DeleteFunction(functionBoxContent);

            contextMenu.Items.Add(renameMenuItem);
            contextMenu.Items.Add(deleteMenuItem);

            contextMenu.IsOpen = true;
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

                    // Check if the new function name already exists under the selected language
                    string selectedLanguage = GetSelectedLanguage();
                    if (languageData.Languages.ContainsKey(selectedLanguage) && languageData.Languages[selectedLanguage].ContainsKey(newFunctionName))
                    {
                        MessageBox.Show($"Function '{newFunctionName}' already exists under language '{selectedLanguage}'. Please choose a different name.", "Rename Function", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Update the function name in the UI
                    functionLabel.Text = newFunctionName;

                    // Update the function in the language data
                    if (languageData.Languages.ContainsKey(selectedLanguage))
                    {
                        var functions = languageData.Languages[selectedLanguage];
                        if (functions.ContainsKey(oldFunctionName))
                        {
                            var functionData = functions[oldFunctionName]; // Preserve the data
                            functions.Remove(oldFunctionName); // Remove old function
                            functions[newFunctionName] = functionData; // Add new function with new name
                            SaveLanguages(); // Save changes
                        }
                    }
                }
            }
        }


        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private void FunctionBox_Click(object sender, MouseButtonEventArgs e)
        {
            // Save current function text before switching
            if (!string.IsNullOrEmpty(selectedFunction))
            {
                string[] parts = selectedFunction.Split('.');
                if (parts.Length == 2)
                {
                    string lang = parts[0];
                    string func = parts[1];

                    if (languageData.Languages.ContainsKey(lang))
                    {
                        // Ensure function is stored as a Dictionary<string, string>
                        if (languageData.Languages[lang].ContainsKey(func))
                        {
                            var functionData = languageData.Languages[lang][func] as Dictionary<string, string>;

                            if (functionData != null)
                            {
                                // Update the "Block" key in the dictionary with the new value
                                functionData["Block"] = FunctionTextBlock.Text;
                            }
                            else
                            {
                                // If it's not a dictionary, initialize it as one
                                languageData.Languages[lang][func] = new Dictionary<string, string>
                        {
                            { "Block", FunctionTextBlock.Text },
                            { "keybind", "" }
                        };
                            }

                            SaveLanguages();
                        }
                    }
                }
            }

            // Identify the clicked function
            Grid functionBoxContent = (Grid)sender;
            TextBox functionLabel = functionBoxContent.Children.OfType<TextBox>().FirstOrDefault();

            if (functionLabel != null)
            {
                string functionName = functionLabel.Text;
                string newSelectedLanguage = GetSelectedLanguage();

                if (!string.IsNullOrEmpty(newSelectedLanguage) && languageData.Languages.ContainsKey(newSelectedLanguage))
                {
                    selectedFunction = $"{newSelectedLanguage}.{functionName}";

                    if (languageData.Languages[newSelectedLanguage].ContainsKey(functionName))
                    {
                        var functionData = languageData.Languages[newSelectedLanguage][functionName] as Dictionary<string, string>;

                        if (functionData != null)
                        {
                            // Update the "Block" key in the dictionary with the new value
                            FunctionTextBlock.Text = functionData["Block"];
                        }
                        else
                        {
                            // If it's not a dictionary, initialize it as one
                            languageData.Languages[newSelectedLanguage][functionName] = new Dictionary<string, string>
                    {
                        { "Block", "" },
                        { "keybind", "" }
                    };

                            FunctionTextBlock.Text = "";
                        }
                    }
                    else
                    {
                        FunctionTextBlock.Text = "";
                    }
                }
            }

            FunctionTextScrollViewer.Visibility = Visibility.Visible;
        }

        private void FunctionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string selectedLanguage = GetSelectedLanguage();
            string selectedFunction = GetSelectedFunction();

            Trace.WriteLine($"TextChanged event triggered. Selected Language: {selectedLanguage}, Selected Function: {selectedFunction}");

            if (!string.IsNullOrEmpty(selectedLanguage) && !string.IsNullOrEmpty(selectedFunction))
            {
                Trace.WriteLine("Selected language and function are not empty.");

                if (languageData.Languages.ContainsKey(selectedLanguage))
                {
                    Trace.WriteLine($"Language '{selectedLanguage}' exists in languageData.");

                    // Extract the function name from selectedFunction
                    string functionName = selectedFunction.Split('.').Last();

                    if (languageData.Languages[selectedLanguage].ContainsKey(functionName))
                    {
                        Trace.WriteLine($"Function '{functionName}' exists in language '{selectedLanguage}'.");

                        // Ensure functionData is a Dictionary<string, string>
                        var functionData = languageData.Languages[selectedLanguage][functionName] as Dictionary<string, string>;

                        if (functionData != null)
                        {
                            // Update the "Block" key in the dictionary with the new value
                            functionData["Block"] = FunctionTextBlock.Text;
                            Trace.WriteLine($"Updated Block text: {functionData["Block"]}");
                        }
                        else
                        {
                            // If it's not a dictionary, initialize it as one
                            languageData.Languages[selectedLanguage][functionName] = new Dictionary<string, string>
                    {
                        { "Block", FunctionTextBlock.Text },
                        { "keybind", "" }
                    };
                            Trace.WriteLine($"Initialized new function data with Block text: {FunctionTextBlock.Text}");
                        }

                        // Save the updated languages data
                        SaveLanguages();
                        Trace.WriteLine("Languages data saved.");
                    }
                    else
                    {
                        Trace.WriteLine($"Function '{functionName}' does not exist in language '{selectedLanguage}'.");
                    }
                }
                else
                {
                    Trace.WriteLine($"Language '{selectedLanguage}' does not exist in languageData.");
                }
            }
            else
            {
                Trace.WriteLine("Selected language or function is empty.");
            }
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
                foreach (var functionName in functions.Keys)
                {
                    CreateFunctionBox(functionName);
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


        



    }
}