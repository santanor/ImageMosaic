using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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
            dlg.Filter = "All Files(*.*)|*.*";
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
            var source = SourceImageTextBox.Text;
            var dst = DestinyDirectoryTextBox.Text;
            var tiles = int.Parse(TileSizeTextBox.Text);
            Task.Run(() => {
                var generator = new MosaicGeneratorParalell(source, dst, 10);
                generator.OnTilePlaced += _UpdateProgress;
                generator.OnTilePlacedStream += _UpdateImageProgress;
                generator.OnGeneratorStarted += OnStartProcess;
                generator.OnGeneratorStop += OnStopProcess;
                generator.GenerateImageMosaic(tiles, 1);
            });            
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            var textBox = sender as TextBox;
            e.Handled = Regex.IsMatch(textBox.Text, "[^0-9]+");
        }
    
        private void _UpdateProgress(int count, int total)
        {
            var currentProgress = (count * 100) / total;
            this.Dispatcher.Invoke(() =>
            {
                ProgressBar.Value = currentProgress;
            });
            
        }

        private void _UpdateImageProgress (System.Drawing.Image image)
        {
            this.Dispatcher.Invoke(() =>
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.StreamSource = new MemoryStream();
                image.Save(bi.StreamSource, ImageFormat.Bmp);
                bi.EndInit();
                OutputImage.Source = bi;
                image.Dispose();
            });            
        }

        private void OnStartProcess()
        {
            this.Dispatcher.Invoke(() =>
            {
                StartProcessButton.IsEnabled = false;
            });            
        }

        private void OnStopProcess()
        {
            this.Dispatcher.Invoke(() =>
            {
                StartProcessButton.IsEnabled = true;
            });            
        }
    }
}
