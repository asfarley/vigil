using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Drawing;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Reflection;

namespace vigil
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        FileSystemWatcher watcher;

        public string imagePath = "";

        public string rootPath;

        public List<string> imagePaths = new List<string>();

        public int index = 0;

        public MainWindow()
        {
            InitializeComponent();

            if(((App) App.Current).imagePath != "")
            {
                imagePath = ((App)App.Current).imagePath;
                try
                {
                    MainImage.Source = LoadBitmapImage(imagePath);
                }
                catch (Exception ex)
                {

                }

                CreateFileWatcher(imagePath);
            }
        }

        private void MainImage_Drop(object sender, DragEventArgs e)
        {
            // If the DataObject contains string data, extract it.
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] droppedFiles = (string[])e.Data.GetData(DataFormats.FileDrop);


                imagePath = droppedFiles[0];

                try
                {
                    MainImage.Source = LoadBitmapImage(imagePath);
                    rootPath = System.IO.Path.GetDirectoryName(imagePath);
                    //imagePaths = Directory.GetFiles(rootPath, "*.bmp|*.png|*.jpg|*.jpeg").ToList();
                    //imagePaths = Directory.GetFiles(rootPath, "*.png").ToList();

                    string supportedExtensions = "*.jpg,*.gif,*.png,*.bmp,*.jpe,*.jpeg,*.wmf,*.emf,*.xbm,*.ico,*.eps,*.tif,*.tiff,*.g01,*.g02,*.g03,*.g04,*.g05,*.g06,*.g07,*.g08";
                    imagePaths = Directory.GetFiles(rootPath, "*.*", SearchOption.AllDirectories).Where(s => supportedExtensions.Contains(System.IO.Path.GetExtension(s).ToLower())).ToList();

                    index = imagePaths.IndexOf(imagePath);
                }
                catch (Exception ex)
                {

                }

                CreateFileWatcher(imagePath);
            }
        }

        public void CreateFileWatcher(string path)
        {
            var dir = System.IO.Path.GetDirectoryName(imagePath);
            var file = System.IO.Path.GetFileName(imagePath);

            // Create a new FileSystemWatcher and set its properties.
            watcher = new FileSystemWatcher();
            watcher.Path = dir;
            /* Watch for changes in LastAccess and LastWrite times. */
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite;
            // Only watch text files.
            //watcher.Filter = file;
            watcher.Filter = file;

            // Add event handlers.
            watcher.Changed += new FileSystemEventHandler(OnChanged);

            // Begin watching.
            watcher.EnableRaisingEvents = true;
        }

        // Define the event handlers.
        private void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);

            this.Dispatcher.Invoke(() =>
            {
                try
                {
                    MainImage.Source = LoadBitmapImage(imagePath);
                }
                catch (Exception ex)
                {

                }
            });
        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            Console.WriteLine("File: {0} renamed to {1}", e.OldFullPath, e.FullPath);
        }

        public BitmapImage LoadBitmapImage(string fileName)
        {
            using (var stream = WaitForFile(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var fs = new FileShare();

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.StreamSource = stream;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
                return bitmapImage;
            }
        }

        FileStream WaitForFile(string fullPath, FileMode mode, FileAccess access, FileShare share)
        {
            for (int numTries = 0; numTries < 10; numTries++)
            {
                FileStream fs = null;
                try
                {
                    fs = new FileStream(fullPath, mode, access, share);
                    return fs;
                }
                catch (IOException)
                {
                    if (fs != null)
                    {
                        fs.Dispose();
                    }
                    System.Threading.Thread.Sleep(50);
                }
            }

            return null;
        }

        private void OnLeft(object sender, ExecutedRoutedEventArgs e)
        {
            if(index > 0)
            {
                index--;
            }
            else
            {
                index = imagePaths.Count - 1;
            }
            
            imagePath = imagePaths[index];
            MainImage.Source = LoadBitmapImage(imagePath);
        }

        private void OnRight(object sender, ExecutedRoutedEventArgs e)
        {
            if(index < imagePaths.Count - 1)
            {
                index++;
            }
            else
            {
                index = 0;
            }
            
            imagePath = imagePaths[index];
            MainImage.Source = LoadBitmapImage(imagePath);
        }
    }
    public partial class App : Application
    {
        public string imagePath = "";
        protected override void OnStartup(StartupEventArgs e)
        {
            foreach (string arg in e.Args)
            {
                imagePath = arg;
            }
            base.OnStartup(e);
        }
    }
}
