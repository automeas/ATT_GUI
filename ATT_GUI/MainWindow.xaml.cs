using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using PrimS.Telnet;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Globalization;
using System.ComponentModel;


namespace ATT_GUI
{

    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Brush _dialbackground1 = new BrushConverter().ConvertFromString("DarkRed") as SolidColorBrush;
        public Brush DialBackground1
        {
            get
            {
                return _dialbackground1;
            }

            set
            {
                _dialbackground1 = value;
                NotifyPropertyChanged("DialBackground1");
            }
        }
        private string _dialbackground2 = "DarkRed";
        public string DialBackground2
        {
            get
            {
                return _dialbackground2;
            }

            set
            {
                _dialbackground2 = value;
                NotifyPropertyChanged("DialBackground2");
            }
        }

        private string _addr1 = "10.100.102.44";
        public string addr1
        {
            get
            {
                return _addr1;
            }
            set
            {
                _addr1 = value;
                NotifyPropertyChanged("addr1");
            }
        }

        private string _addr2 = "10.100.102.44";
        public string addr2
        {
            get
            {
                return _addr2;
            }
            set
            {
                _addr2 = value;
                NotifyPropertyChanged("addr2");
            }
        }

        private double _step = 1;
        public double step
        {
            get
            {
                return _step;
            }
            set
            {
                _step = value;
                NotifyPropertyChanged("step");
            }
        }
        private double _step1 = 4;
        public double step1
        {
            get
            {
                return _step1;
            }
            set
            {
                _step1 = value;
                NotifyPropertyChanged("step1");
            }
        }

        public bool _IsConnected1 = false;
        public bool IsConnected1
        {
            get { return _IsConnected1; }
            set
            {
                _IsConnected1 = value;
                NotifyPropertyChanged("IsConnected1");
            }
        }

        public bool _IsConnected2 = false;
        public bool IsConnected2
        {
            get { return _IsConnected2; }
            set
            {
                _IsConnected2 = value;
                NotifyPropertyChanged("IsConnected2");
            }
        }
        
        public bool _IsSync = false;
        public bool IsSync
        {
            get { return _IsSync; }
            set
            {
                _IsSync = value;
                NotifyPropertyChanged("IsSync");
            }
        }

        public bool _IsFine = false;
        public bool IsFine
        {
            get { return _IsFine; }
            set
            {
                _IsFine = value;
                NotifyPropertyChanged("IsFine");
            }
        }

        private double _att1 = 25;
        public double att1
        {
            get
            {
                return _att1;
            }
            set
            {
                _att1 = ((int)(value / step)) * step;
                NotifyPropertyChanged("att1");
            }
        }

        private double _att2 = 10;
        public double att2
        {
            get
            {
                return _att2;
            }
            set
            {
                _att2 = ((int)(value / step)) * step;
                NotifyPropertyChanged("att2");
            }
        }

        public bool _linked = true;
        public bool linked
        {
            get { return _linked; }
            set
            {
                _linked = value;
                NotifyPropertyChanged("linked");
            }
        }


        private int _numValue1 = 4010;

        public int NumValue1
        {
            get { return _numValue1; }
            set
            {
                _numValue1 = value;
                port1.Text = value.ToString();
            }
        }

        private void port1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (port1 == null)
            {
                return;
            }

            if (!int.TryParse(port1.Text, out _numValue1))
                port1.Text = _numValue1.ToString();
        }

        private int _numValue2 = 4011;

        public int NumValue2
        {
            get { return _numValue2; }
            set
            {
                _numValue2 = value;
                port2.Text = value.ToString();
            }
        }

        private void port2_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (port2 == null)
            {
                return;
            }

            if (!int.TryParse(port2.Text, out _numValue2))
                port2.Text = _numValue2.ToString();
        }



        #region INotifyPropertyChanged

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion INotifyPropertyChanged


        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        }
        
        private void att1_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (e.Delta > 0)
            {
                atten1.Value = ((int)(atten1.Value/step) + 1) * step;
                if (IsSync) atten2.Value = ((int)(atten2.Value / step) + 1) * step;
            }
            else
            {
                atten1.Value = ((int)(atten1.Value / step) - 1) * step;
                if (IsSync) atten2.Value = ((int)(atten2.Value / step) - 1) * step;
            }
        }

        private void att2_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (e.Delta > 0)
            {
                if (IsSync) atten1.Value = ((int)(atten1.Value / step) + 1) * step;
                atten2.Value = ((int)(atten2.Value / step) + 1) * step;
            }
            else
            {
                if (IsSync) atten1.Value = ((int)(atten1.Value / step) - 1) * step;
                atten2.Value = ((int)(atten2.Value / step) - 1) * step;
            }
        }



        public void Telnet1()
        {
            string linefeed = "\r";
            
            float fvalue = -1 ;
            System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowTrailingWhite;
            var ci = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            try
            {
                var client = new Client(_addr1, _numValue1, new System.Threading.CancellationToken());

                if (client.IsConnected == true) Trace.WriteLine("1:IsConnected == true");
                else
                {
                    IsConnected1 = false;
                    return;
                }
                string s = "";
                bool connected = false;
                for (int i = 0; i < 10; i++)
                {
                    client.WriteLine("A?", linefeed);
                    Task<string> task1 = client.TerminatedReadAsync("\r", TimeSpan.FromMilliseconds(1000));
                    s = task1.Result;
                    s = s.ReplaceLineEndings(string.Empty);
                    Trace.WriteLine("1:ASC:" + s + "| hex:" + stoh(s));
                    if (Single.TryParse(s, style, ci, out fvalue))
                    {
                        connected = true;
                        Dispatcher.Invoke((Action)(() => atten1.OuterDialFill = new SolidColorBrush(Color.FromRgb(0, 50, 0))));
                        att1 = fvalue;
                        break;
                    }
                    else Trace.Write(".");
                }
                if (connected == true)
                    Trace.WriteLine("\n1:Connected. Read value_1:" + fvalue.ToString());
                else IsConnected1 = false;

                for (; ; )
                {

                    if (!IsConnected1) break;
                    if (client.IsConnected == false) break;
                    client.WriteLine("A?", linefeed);
                    Task<string> task2 = client.TerminatedReadAsync("\r", TimeSpan.FromMilliseconds(1000));
                    s = task2.Result;
                    s = s.ReplaceLineEndings(string.Empty);
                    if (Single.TryParse(s, style, ci, out fvalue))
                    {
                        connected = true;
                        Trace.WriteLine("1:Set value:" + fvalue.ToString());
                        if ((int)(fvalue / step) == (int)(att1 / step)) Dispatcher.Invoke((Action)(() => atten1.OuterDialFill = new SolidColorBrush(Color.FromRgb(0, 50, 0))));
                        else
                        {
                            Dispatcher.Invoke((Action)(() => atten1.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                            client.WriteLine("B" + att1.ToString() + "E", linefeed).Wait();
                            Task<string> task3 = client.TerminatedReadAsync("K", TimeSpan.FromMilliseconds(1000));
                            s = task3.Result;
                            Trace.WriteLine("1:hex:" + stoh(s) + "|ASC:" + s);
                        }
                    }
                    else Dispatcher.Invoke((Action)(() => atten1.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                    for (int j = 0; j < 100; j++)
                    {
                        if (client.IsConnected == false) break;
                        if ((int)(fvalue / step) == (int)(att1 / step) && IsConnected1) Task.Delay(100).Wait();
                        else break;
                    }


                }
                client.Dispose();
                Dispatcher.Invoke((Action)(() => atten1.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                IsConnected1 = false;
            }
            catch (Exception e)
            {
                Dispatcher.Invoke((Action)(() => atten1.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                IsConnected1 = false;
            }






        }

        private void Connect_Checked(object sender, RoutedEventArgs e)
        {
            Task.Run(()=>Telnet1());
        }





        public void Telnet2()
        {
            string linefeed = "\r";

            float fvalue = -1;
            System.Globalization.NumberStyles style = System.Globalization.NumberStyles.Float | System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowTrailingWhite;
            var ci = System.Globalization.CultureInfo.CreateSpecificCulture("en-US");
            try
            {
                var client = new Client(_addr2, _numValue2, new System.Threading.CancellationToken());

                if (client.IsConnected == true) Trace.WriteLine("2:IsConnected == true");
                else
                {
                    IsConnected2 = false;
                    return;
                }
                string s = "";
                bool connected = false;
                for (int i = 0; i < 10; i++)
                {
                    client.WriteLine("A?", linefeed);
                    Task<string> task1 = client.TerminatedReadAsync("\r", TimeSpan.FromMilliseconds(1000));
                    s = task1.Result;
                    s = s.ReplaceLineEndings(string.Empty);
                    Trace.WriteLine("2:ASC:" + s + "| hex:" + stoh(s));
                    if (Single.TryParse(s, style, ci, out fvalue))
                    {
                        connected = true;
                        Dispatcher.Invoke((Action)(() => atten2.OuterDialFill = new SolidColorBrush(Color.FromRgb(0, 50, 0))));
                        att2 = fvalue;
                        break;

                    }
                    else Trace.Write(".");
                }
                if (connected == true) 
                    Trace.WriteLine("\n2:Connected. Read value:" + fvalue.ToString());
                else IsConnected2 = false;

                for (; ; )
                {

                    if (!IsConnected2) break;
                    if (client.IsConnected == false) break;
                    client.WriteLine("A?", linefeed);
                    Task<string> task2 = client.TerminatedReadAsync("\r", TimeSpan.FromMilliseconds(1000));
                    s = task2.Result;
                    s = s.ReplaceLineEndings(string.Empty);
                    if (Single.TryParse(s, style, ci, out fvalue))
                    {
                        connected = true;
                        Trace.WriteLine("2:Set value:" + fvalue.ToString());
                        if ((int)(fvalue / step) == (int)(att2 / step)) Dispatcher.Invoke((Action)(() => atten2.OuterDialFill = new SolidColorBrush(Color.FromRgb(0, 50, 0))));
                        else
                        {
                            Dispatcher.Invoke((Action)(() => atten2.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                            client.WriteLine("B" + att2.ToString() + "E", linefeed).Wait();
                            Task<string> task3 = client.TerminatedReadAsync("K", TimeSpan.FromMilliseconds(1000));
                            s = task3.Result;
                            Trace.WriteLine("2:hex:" + stoh(s)+ "|ASC:" + s);
                        }
                    }
                    else Dispatcher.Invoke((Action)(() => atten2.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                    for (int j = 0; j < 100; j++)
                    {
                        if (client.IsConnected == false) break;
                        if ((int)(fvalue / step) == (int)(att2 / step) && IsConnected2) Task.Delay(100).Wait();
                        else break;
                    }
                }
                client.Dispose();
                Dispatcher.Invoke((Action)(() => atten2.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                IsConnected2 = false;
            }
            catch (Exception e)
            {
                Dispatcher.Invoke((Action)(() => atten2.OuterDialFill = new SolidColorBrush(Color.FromRgb(120, 0, 0))));
                IsConnected2 = false;
            }






        }








        public string stoh (string sample)
        {
            byte[] ba = Encoding.Default.GetBytes(sample);
            var hexString = BitConverter.ToString(ba);
            //hexString = hexString.Replace("-", "");
            return hexString;
        }

        private void Connect_Checked2(object sender, RoutedEventArgs e)
        {
            Task.Run(() => Telnet2());

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            paramload();
        }

        private void paramupd()
        {
            Properties.Settings.Default.addr1 = addr1;
            Properties.Settings.Default.addr2 = addr2;
            Properties.Settings.Default.port1 = NumValue1;
            Properties.Settings.Default.port2 = NumValue2;
            Properties.Settings.Default.fine = IsFine;
            Properties.Settings.Default.sync = IsSync;
            Properties.Settings.Default.Save();


        }
        private void paramload()
        {

            /*      if (Properties.Settings.Default.imsilist.Count > 0)
                  {
                      string[] items = new string[Properties.Settings.Default.imsilist.Count];
                      Properties.Settings.Default.imsilist.CopyTo(items, 0);
                      listBoxUniqueIMSI.Items.Add(items);
                  }*/

            addr1 = Properties.Settings.Default.addr1 ;
            addr2 = Properties.Settings.Default.addr2 ;
            NumValue1 = Properties.Settings.Default.port1;
            NumValue2 = Properties.Settings.Default.port2;
            IsFine = Properties.Settings.Default.fine;
            IsSync = Properties.Settings.Default.sync;


        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            paramupd();
        }

        private void Fine_Checked(object sender, RoutedEventArgs e)
        {
            step = 0.25;
            step1 = 19;
        }

        private void Fine_Unchecked(object sender, RoutedEventArgs e)
        {
            step = 1;
            step1 = 4;
        }

        private void Sync_Checked(object sender, RoutedEventArgs e)
        {
            atten2.Value = atten1.Value;
        }
    }







}
