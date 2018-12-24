using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Path = System.IO.Path;

namespace Txt2Csv
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _sign;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            _sign = tbxSign.Text;
            Task task = new Task(Run);
            task.Start();
        }

        private void UpdateText(Label lbl, string text)
        {
            lbl.Content = text;
        }

        private void Run()
        {
            List<string> fileList = new List<string>();
            string path = AppDomain.CurrentDomain.BaseDirectory;
            Action<Label, String> updateAction = new Action<Label, string>(UpdateText);

            foreach (var file in Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
            {
                fileList.Add(file);
            }
            
            lblNums.Dispatcher.BeginInvoke(updateAction, lblNums, $"文件总数:     {fileList.Count}");

            int index = 1;
            foreach (var file in fileList)
            {
                lblNote.Dispatcher.BeginInvoke(updateAction, lblNote, Path.GetFileName(file));
                lblNums.Dispatcher.BeginInvoke(updateAction, lblNums, $"文件总数:     {index++}/{fileList.Count}");

                string fileCSV = file + ".csv";
                if (File.Exists(fileCSV))
                {
                    File.Delete(fileCSV);
                }
                StringBuilder sb = new StringBuilder();
                foreach (var line in File.ReadAllLines(file))
                {
                    sb.Append("\"" + line.Replace(_sign, "\",\"") + "\"\r\n");
                }
                
                File.WriteAllText(fileCSV, sb.ToString(), Encoding.UTF8);
            }

            Process.Start(path);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var count = Directory.GetFiles(AppDomain.CurrentDomain.BaseDirectory, "*.txt", SearchOption.AllDirectories).Length;
            lblNums.Content = $"文件总数:     {count}";
        }
    }
}
