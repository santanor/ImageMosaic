using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using WinForms = System.Windows.Forms;

namespace ImageMosaic.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_ClickSourceImage(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.DefaultExt = ".png";
            dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif| All Files(*.*)|*.*";
            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filename = dlg.FileName;
                SourceImageTextBox.Text = filename;
            }
        }

        private void Button_ClickDestination(object sender, RoutedEventArgs e)
        {
            var dlg = new WinForms.FolderBrowserDialog();
            // Display OpenFileDialog by calling ShowDialog method 
            var result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox 
            if (result == WinForms.DialogResult.OK)
            {
                // Open document 
                string filename = dlg.SelectedPath;
                DestinyDirectoryTextBox.Text = filename;
            }
        }

        private void Button_ClickDoTheThing(object sender, RoutedEventArgs e)
        {
            var generator = new MosaicGeneratorParalell(SourceImageTextBox.Text, DestinyDirectoryTextBox.Text, 10);
            generator.OnTilePlaced += _UpdateProgress;
            generator.GenerateImageMosaic(int.Parse(TileSizeTextBox.Text), 1);
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(textBox.Text, "[^0-9]+");
        }
    
        private void _UpdateProgress(int count, int total)
        {
            var currentProgress = (count * 100) / total;
            ProgressBar.Value = currentProgress;
        }
    }
}
