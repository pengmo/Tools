using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Configuration;
using System.Windows.Input;
using Newtonsoft.Json.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;

namespace ParseAddress
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private bool _findOne;                                  // 判断点击的按钮
        private bool _openPath;                                 // 控制窗口打开路径（上次选择文件的路径）
        private string _serverUrl = "";                         // 服务器地址
        private readonly string _fileType = ".txt";             // 支持待标色文件类型
        private List<DataSource> _dataSourceList = new List<DataSource>();   // 控件绑定

        public event PropertyChangedEventHandler PropertyChanged;

        private int _dataCount;
        // 摘要:
        //     返回序列中的元素数。
        public int DataCount
        {
            get { return _dataCount; }

            set
            {
                _dataCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("DataCount"));
            }
        }
        private int _validCount;
        // 有效次数
        public int ValidCount
        {
            get { return _validCount; }

            set
            {
                _validCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ValidCount"));
            }
        }

        private int _invalidCount;
        // 无效次数
        public int InvalidCount
        {
            get { return _invalidCount; }

            set
            {
                _invalidCount = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InvalidCount"));
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            // 读取配置文件获取服务器地址
            AccessAppSettings();
        }

        /// <summary>
        /// 文本框回车事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputGroupButton_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                InputGroupButton_Click(sender, e);
            }
        }

        /// <summary>
        /// 文本框获取焦点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputGroupButton_GotFocus(object sender, RoutedEventArgs e)
        {
            if (InputGroupButton.Text == "请输入处理地址")
            {
                InputGroupButton.Text = "";
            }
        }

        /// <summary>
        /// 文本框失去焦点事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputGroupButton_LostFocus(object sender, RoutedEventArgs e)
        {
            if (InputGroupButton.Text == "")
            {
                InputGroupButton.Text = "请输入处理地址";
            }
        }


        /// <summary>
        /// 导入地址文件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnImportFile_Click(object sender, RoutedEventArgs e)
        {
            // 打开文件浏览窗口，选择待处理文件
            var ofd = new System.Windows.Forms.OpenFileDialog
            {
                Title = @"选择待处理文件",
                Filter = @"文本文档(*.txt)|*.txt",
                Multiselect = false,
                CheckFileExists = true,
                RestoreDirectory = false
            };

            if (!_openPath)
            {
                // 打开文件对话框的初始目录
                ofd.InitialDirectory = Environment.CurrentDirectory;
                _openPath = true;
            }

            // 判断窗口是否打开
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            // 清空
            _dataSourceList.Clear();

            var file = ofd.FileNames[0];

            // 判断选中文件的后缀是否符合给定的文件后缀，注意？是三元表达式的缩写
            if (_fileType.Contains(Path.GetExtension(file)?.ToLower()))
            {
                // 读取文本文件
                ReadTxt(file);
            }
            else
            {
                MessageBox.Show("不支持当前文件类型，请重新选择！", "Koala Team", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }

        /// <summary>
        /// 读取文本文件
        /// </summary>
        /// <param name="file"></param>
        private void ReadTxt(string file)
        {
            // 捕获术语文件格式错误或者解析错误
            try
            {
                var content = File.ReadAllLines(file, Encoding.UTF8);
                var index = 1;
                foreach (var item in content)
                {
                    _dataSourceList.Add(new DataSource
                    {
                        Index = index++,
                        Address = item.Replace("\t", " ")
                    });
                }

                DataCount = _dataSourceList.Count;
                RefreshDataGrid(false);
            }
            catch (Exception)
            {
                MessageBox.Show("导入文本文件格式有误，请重新导入！", "Koala Team", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 导出地址文件事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnExportFile_Click(object sender, RoutedEventArgs e)
        {
            FileStream fs = new FileStream(Environment.CurrentDirectory + "\\result.txt", FileMode.Create);
            var info = new StringBuilder();
            foreach (var item in _dataSourceList)
            {
                var line = $"{item.Index}\t{item.Address}\t{item.Province}\t{item.City}\t{item.District}\t{item.AreaCode}\t{item.Tel}";
                info.AppendLine(line);
            }
            //获得字节数组
            byte[] data = Encoding.Default.GetBytes(info.ToString());
            //开始写入
            fs.Write(data, 0, data.Length);
            //清空缓冲区、关闭流
            fs.Flush();
            fs.Close();

            System.Diagnostics.Process.Start("notepad.exe", Environment.CurrentDirectory + "\\result.txt");
        }

        /// <summary>
        /// 刷新DataGrid
        /// </summary>
        private void RefreshDataGrid(bool valid = true)
        {
            DG.ItemsSource = null;
            DG.ItemsSource = _dataSourceList;
            DG.Items.Refresh();
            if (!valid)
            {
                ValidCount = 0;
            }
            //DataSum.Content = "总行数：" + Convert.ToString(_dataSum);
            //ValidDataSum.Content = "有效分析数：" + Convert.ToString(_validDataSum);
            //InvalidDataSum.Content = "无效分析数：" + Convert.ToString(_invalidDataSum);
        }

        /// <summary>
        /// 初始化窗体加载事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //DataSum.Content = "总行数：" + Convert.ToString(_dataSum);
            //ValidDataSum.Content = "有效分析数：" + Convert.ToString(_validDataSum);
            //InvalidDataSum.Content = "无效分析数：" + Convert.ToString(_invalidDataSum);
        }

        /// <summary>
        /// 右键菜单复制事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CopyLine_Click(object sender, RoutedEventArgs e)
        {
            var ds = DG.SelectedItem as DataSource;
            var line = $"{ds.Index}\t{ds.Address}\t{ds.Province}\t{ds.City}\t{ds.District}\t{ds.AreaCode}\t{ds.Tel}";
            Clipboard.SetDataObject(line);
        }

        /// <summary>
        /// 查询按钮事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (InputGroupButton.Text == "")
            {
                return;
            }
            DataCount = 1;
            ValidCount = 0;
            InvalidCount = 0;
            _dataSourceList.Clear();
            _findOne = true;
            var address = InputGroupButton.Text;

            Thread t = new Thread(new ParameterizedThreadStart(HttpPostConnectToServer));
            t.Start(address);
        }

        /// <summary>
        /// 通过GET方式向服务器发送数据并保存返回结果
        /// </summary>
        /// <param name="serverUrl"></param>
        /// <param name="postData"></param>
        /// <returns></returns>
        private void HttpGetConnectToServer(object postData)
        {
            var postdata = postData as string;
            var pd = ToBase64String(postdata);
            HttpWebRequest request = null;
            try
            {
                //创建请求
                request = (HttpWebRequest)HttpWebRequest.Create("http://" + _serverUrl + "/get_address?data=" + pd);
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    NetworkProPop.IsOpen = true;
                    HintInfo.Text = ex.Message;
                }));
            }

            request.Method = "GET";
            //设置上传服务的数据格式
            request.ContentType = "application/x-www-form-urlencoded";
            //请求的身份验证信息为默认
            request.Credentials = CredentialCache.DefaultCredentials;
            //请求超时时间
            request.Timeout = 60000;
            //读取返回消息
            var json = "";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                json = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    NetworkProPop.IsOpen = true;
                    HintInfo.Text = ex.Message;
                }));
            }
            ParseJson(json);
        }

        /// <summary>
        /// 通过POST方式向服务器发送数据并保存返回结果
        /// </summary>
        /// <param name="postData"></param>
        /// <returns></returns>
        private void HttpPostConnectToServer(object postData)
        {
            var postdata = postData as string;
            var pd = ToBase64String(postdata);
            var dataArray = Encoding.UTF8.GetBytes("data=" + pd);

            string str = Encoding.Default.GetString(dataArray);
            //创建请求
            var request = (HttpWebRequest)HttpWebRequest.Create("http://" + _serverUrl + "/get_address");
            request.Method = "POST";
            request.ContentLength = dataArray.Length;
            //设置上传服务的数据格式
            request.ContentType = "application/x-www-form-urlencoded";
            //请求的身份验证信息为默认
            request.Credentials = CredentialCache.DefaultCredentials;
            //请求超时时间
            request.Timeout = 300000;
            //创建输入流
            Stream dataStream;
            //using (var dataStream = request.GetRequestStream())
            //{
            //    dataStream.Write(dataArray, 0, dataArray.Length);
            //    dataStream.Close();
            //}
            try
            {
                dataStream = request.GetRequestStream();
            }
            catch (Exception)
            {
                return;//连接服务器失败
            }
            //发送请求
            dataStream.Write(dataArray, 0, dataArray.Length);
            dataStream.Close();
            //读取返回消息
            var json = "";
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                json = reader.ReadToEnd();
                reader.Close();
            }
            catch (Exception ex)
            {
                Dispatcher.BeginInvoke(new Action(delegate
                {
                    NetworkProPop.IsOpen = true;
                    HintInfo.Text = ex.Message;
                }));
            }
            ParseJson(json);
        }

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string ToBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string UnBase64String(string value)
        {
            if (value == null || value == "")
            {
                return "";
            }
            byte[] bytes = Convert.FromBase64String(value);
            return Encoding.UTF8.GetString(bytes);
        }


        /// <summary>
        /// 解析json 保存在类里
        /// </summary>
        /// <param name="json"></param>
        private void ParseJson(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;

            var jsonObj = JObject.Parse(json);
            var jList = JArray.Parse(jsonObj["list"].ToString());
            
            for (int i = 0; i < jList.Count; i++)
            {
                var item = JObject.Parse(jList[i].ToString());
                if (_findOne)
                {
                    _dataSourceList.Add(new DataSource
                    {
                        Address = item["address"].ToString(),
                        Province = item["province"].ToString(),
                        City = item["city"].ToString(),
                        District = item["area"].ToString(),
                        AreaCode = item["areacode"].ToString(),
                        Tel = item["tel"].ToString()
                    });
                    
                }
                else
                {
                    var ds = _dataSourceList.FindAll(s => s.Address.Equals(item["address"].ToString()));
                    if (ds.Count > 0)
                    {
                        foreach (var info in ds)
                        {
                            info.Province = item["province"].ToString();
                            info.City = item["city"].ToString();
                            info.District = item["area"].ToString();
                            info.AreaCode = item["areacode"].ToString();
                            info.Tel = item["tel"].ToString();
                        }

                        //if (string.IsNullOrEmpty(ds.AreaCode) && string.IsNullOrEmpty(ds.Tel))
                        //{
                        //    _invalidDataSum += 1;
                        //}
                        //else
                        //{
                        //    _validDataSum += 1;
                        //}
                    }
                }
            }
            Dispatcher.BeginInvoke(new Action(delegate
            {
                var finds = _dataSourceList.FindAll(s => !string.IsNullOrEmpty(s.Province));
                ValidCount = finds.Count;
                RefreshDataGrid();
                DG.SelectedIndex = _dataSourceList.Count - 1;
            }));
        }

        /// <summary>
        /// 解析地址事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnParseFile_Click(object sender, RoutedEventArgs e)
        {
            if (_dataSourceList.Count == 0)
            {
                return;
            }
            else
            {
                ValidCount = 0;
                InvalidCount = 0;
                _findOne = false;
                FuncBtnShow(false);
                Title = "地址分析--数据分析中";
                Task.Run(() => Analyse());
                //tasks.Add(new Task<object>(() => SegmentHandling(_dataSourceList.GetRange(start, count)));
            }
        }

        private void Analyse()
        {
            var start = 0;
            var count = _dataSourceList.Count / 500 + 1;

            var taskParams = new List<object>();
            for (int i = 1; i <= count; i++)
            {
                if (start >= _dataSourceList.Count)
                    break;

                if (i == count)
                {
                    taskParams.Add(_dataSourceList.GetRange(start, _dataSourceList.Count - start));
                }
                else
                {
                    taskParams.Add(_dataSourceList.GetRange(start, 500));
                    start += 500;
                }
            }

            var tasks = taskParams.Select(param => new Task<object>(() => SegmentHandling(param))).Cast<Task>().ToList();
            const int startThreadCount = 1;
            var sw = new Stopwatch();
            sw.Start();
            var runTasks = new List<Task>();
            for (int i = 0; i < tasks.Count; i++)
            {
                tasks[i].Start();
                runTasks.Add(tasks[i]);
                if (i >= startThreadCount)
                {
                    var overTask = Task.WhenAny(runTasks.ToArray()).Result;
                    runTasks.Remove(overTask);
                }
            }
            Task.WaitAll(tasks.ToArray());
            sw.Start();
            
            Dispatcher.BeginInvoke(new Action(delegate
            {
                this.Title = "地址分析";
                InvalidCount = DataCount - ValidCount;
                Console.WriteLine(InvalidCount);
                FuncBtnShow(true);
                //MessageBox.Show("数据处理完毕！", "Koala Team", MessageBoxButton.OK, MessageBoxImage.Information);
            }));
            MessageBox.Show($"分析完毕，耗时 {sw.ElapsedMilliseconds / 1000} 秒", "提示");
        }

        /// <summary>
        /// 分段向服务器请求数据
        /// </summary>
        private object SegmentHandling(object obj)
        {
            StringBuilder sb = new StringBuilder();

            var list = (List<DataSource>)obj;
            for (int i = 1; i <= list.Count; i++)
            {
                var line = list[i - 1].Address;
                sb.AppendLine(line);
            }

            HttpPostConnectToServer(sb.ToString());
            sb.Clear();
            
            //MessageBox.Show(ts2.Seconds.ToString());
            //]Console.WriteLine("DateTime总共花费{0}m.", ts.Minutes);//TimeSpan的TotalMilliseconds方法，返回TimeSpan值表示的毫秒数
            

            return "";
        }

        /// <summary>
        /// 根据配置文件 获取服务器地址
        /// </summary>
        private void AccessAppSettings()
        {
            try
            {
                //获取Configuration对象
                Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                //根据Key读取<add>元素的Value
                _serverUrl = config.AppSettings.Settings["ServerUrl"].Value;
                if (string.IsNullOrEmpty(_serverUrl))
                {
                    MessageBox.Show("服务器地址解析错误，请核对配置文件后重启！", "Koala Team", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("服务器地址解析错误，请核对配置文件后重启！", "Koala Team", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 右键菜单 删除事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteLine_Click(object sender, RoutedEventArgs e)
        {
            var index = DG.SelectedIndex;
            var ds = DG.SelectedItem as DataSource;
            if (string.IsNullOrEmpty(ds.AreaCode) && string.IsNullOrEmpty(ds.City))
            {
                InvalidCount -= 1;
            }
            else
            {
                ValidCount -= 1;
            }
            _dataSourceList.RemoveAt(index);
            RefreshDataGrid();
            DG.SelectedIndex = index;
        }

        /// <summary>
        /// 右键菜单 清空事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLines_Click(object sender, RoutedEventArgs e)
        {
            _dataSourceList.Clear();
            ValidCount = 0;
            InvalidCount = 0;
            RefreshDataGrid();
        }

        /// <summary>
        /// 程序运行时控制按钮是否可以继续点击状态
        /// </summary>
        /// <param name="state"></param>
        private void FuncBtnShow(bool state)
        {
            BtnImportFile.IsEnabled = state;
            BtnParseFile.IsEnabled = state;
            BtnExportFile.IsEnabled = state;
        }
    }
}
