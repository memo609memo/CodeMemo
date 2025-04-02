using System.Text;
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

namespace CodeMemo
{
    public partial class MainWindow : Window
    {
        //Main Funcs


        public MainWindow()
        {
            InitializeComponent();
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
            InputDialog inputDialog = new InputDialog();
            bool? result = inputDialog.ShowDialog(); 

            if (result == true) 
            {
                string languageName = inputDialog.newBoxName; 
                CreateLanguageBox(languageName);
            }
        }

        private void CreateLanguageBox(string languageName)
        {
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
                Margin = new Thickness(0, 5, 0, 0)
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

            // Create the options button and attach the click event
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
            optionsButton.Click += (s, e) => ShowLanguageOptions(optionsButton, languageBoxContent); // Pass the button and the container

            languageBoxContent.Children.Add(optionsButton);

            // Insert the language box content into the StackPanel
            LanguagesStackPanel.Children.Insert(LanguagesStackPanel.Children.Count - 1, languageBoxContent);

            LanguageScrollViewer.ScrollToBottom();
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

            // Store the event handler in a variable
            MouseButtonEventHandler clickOutsideHandler = null;

            clickOutsideHandler = (s, e) =>
            {
                if (!optionsMenu.IsMouseOver) // If click is outside the menu
                {
                    optionsPopup.IsOpen = false;
                }
            };

            // Attach event listener
            this.MouseDown += clickOutsideHandler;

            // Ensure the event is removed when popup closes
            optionsPopup.Closed += (s, e) =>
            {
                this.MouseDown -= clickOutsideHandler;
            };

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
            InputDialog inputDialog = new InputDialog();
            if (inputDialog.ShowDialog() == true)
            {
                string newLanguageName = inputDialog.newBoxName;

                TextBox languageLabel = languageBoxContent.Children.OfType<TextBox>().FirstOrDefault();
                if (languageLabel != null)
                {
                    languageLabel.Text = newLanguageName;
                }
            }
        }

        private void DeleteLanguage(Grid languageBoxContent)
        {

            // Close the menu
            StackPanel optionsMenu = languageBoxContent.Children.OfType<StackPanel>().LastOrDefault();
            if (optionsMenu != null)
            {
                optionsMenu.Visibility = Visibility.Collapsed;
            }

            // Remove the language box content
            LanguagesStackPanel.Children.Remove(languageBoxContent);
        }










        //This is the section for modifying anything Function related

        private void CreateFunctionBoxButton_Click(object sender, RoutedEventArgs e)
        {
            InputDialog inputDialog = new InputDialog();
            bool? result = inputDialog.ShowDialog();

            if (result == true)
            {
                string functionName = inputDialog.newBoxName;
                CreateFunctionBox(functionName);
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

            Grid functionBoxContent = new Grid
            {
                Width = 124,
                Height = 34,
                Margin = new Thickness(0, 5, 0, 0)
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

            functionBoxContent.Children.Add(functionImage);
            functionBoxContent.Children.Add(functionLabel);

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

            FunctionsStackPanel.Children.Insert(FunctionsStackPanel.Children.Count - 1, functionBoxContent);

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

            MouseButtonEventHandler clickOutsideHandler = null;

            clickOutsideHandler = (s, e) =>
            {
                if (!optionsMenu.IsMouseOver) 
                {
                    optionsPopup.IsOpen = false;
                }
            };

            this.MouseDown += clickOutsideHandler;

            optionsPopup.Closed += (s, e) =>
            {
                this.MouseDown -= clickOutsideHandler;
            };

            optionsPopup.IsOpen = true;
        }

        private void RenameFunction(Grid functionBoxContent)
        {

            StackPanel optionsMenu = functionBoxContent.Children.OfType<StackPanel>().LastOrDefault();
            if (optionsMenu != null)
            {
                optionsMenu.Visibility = Visibility.Collapsed;
            }

            InputDialog inputDialog = new InputDialog();
            if (inputDialog.ShowDialog() == true)
            {
                string newFunctionName = inputDialog.newBoxName;

                TextBox functionLabel = functionBoxContent.Children.OfType<TextBox>().FirstOrDefault();
                if (functionLabel != null)
                {
                    functionLabel.Text = newFunctionName;
                }
            }
        }

        private void DeleteFunction(Grid functionBoxContent)
        {

            StackPanel optionsMenu = functionBoxContent.Children.OfType<StackPanel>().LastOrDefault();
            if (optionsMenu != null)
            {
                optionsMenu.Visibility = Visibility.Collapsed;
            }

            FunctionsStackPanel.Children.Remove(functionBoxContent);
        }
    }
}