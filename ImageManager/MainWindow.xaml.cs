using ImageManager.Annotations;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace ImageManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
    

        public ObservableCollection<FileInfo> FileInfos
        {
            get;
            set;
        }

        public bool ProgressEnable
        {
            get { return _progressEnable; }
            set
            {
                if (_progressEnable != value)
                {
                    _progressEnable = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool ButonsEnable
        {
            get
            {
                return _buttonsEnable;
            }
            set
            {
                if (_buttonsEnable != value)
                {
                    _buttonsEnable = value;
                    OnPropertyChanged();
                }
            }
        }
        private bool _buttonsEnable;
        private bool _progressEnable;
        public MainWindow()
        {
            FileInfos = new ObservableCollection<FileInfo>();
            InitializeComponent();
            ButonsEnable = true;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var openfileDialog = new OpenFileDialog();
            openfileDialog.Multiselect = true;
            if (openfileDialog.ShowDialog().GetValueOrDefault(false))
            {
                foreach (var fileName in openfileDialog.FileNames)
                {
                    FileInfos.Add(new FileInfo(fileName));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            ButonsEnable = false;

            var selectedPath = string.Empty;
            var folderBowserDialog = new FolderBrowserDialog();

            if (folderBowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                selectedPath = folderBowserDialog.SelectedPath;
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            int directionaryCounter = 0;

            await Task.Run(() =>
            {
                FileInfos.Select(x => new
                {
                    x.LastWriteTime.Year,
                    x.LastWriteTime.Month
                }).Distinct().OrderBy(x => x.Month).ToList().ForEach(y =>
                {
                    var filesFromSpecificTimeRange = FileInfos.Where(f => f.CreationTime.Year.Equals(y.Year) && f.CreationTime.Month.Equals(y.Month)).ToList();

                    var createdDirectionary = Directory.CreateDirectory(String.Format(@"{0}\{1:yyyy MMMM}", selectedPath, new DateTime(y.Year, y.Month, 1)));

                    directionaryCounter++;
                    filesFromSpecificTimeRange.ForEach(ftr =>
                    {
                        File.Copy(ftr.FullName, string.Format(@"{0}\{1}", createdDirectionary.FullName, ftr.Name));
                    });
                });
            }).ConfigureAwait(false);

            stopWatch.Stop();
            TimeSpan ts = stopWatch.Elapsed;
            string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10);

            System.Windows.MessageBox.Show(string.Format("Zakończono kopiowanie. Utworzono: {0} katalogów{1} Czas Twania to: {2}", directionaryCounter, Environment.NewLine, elapsedTime), "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);

            ButonsEnable = true;
            Process.Start("explorer.exe", selectedPath);
        }
    }
}