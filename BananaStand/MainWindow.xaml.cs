using System;
using System.IO;
using System.Reflection;
using System.Windows;

using BananaStand.ViewModels;
using System.Windows.Input;

namespace BananaStand
{
    public partial class MainWindow : Window
    {
        private DevicesViewModel devices;

        public MainWindow()
        {
            Left = 860;
            Top = 170;
            InitializeComponent();
            Assembly assembly = Assembly.GetExecutingAssembly();
            Title += " - " + AssemblyName.GetAssemblyName(assembly.Location).Version;

            DataContext = devices = new DevicesViewModel();
            ControlPanel.DataContext = new ArduinoViewModel(devices);

            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            using (
                var iconStream =
                    Application.GetResourceStream(new Uri("pack://application:,,,/BananaStand;component/Main.ico"))
                        .Stream)
            {
                ni.Icon = new System.Drawing.Icon(iconStream);
            }
            ni.Visible = true;
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {
                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

        protected void SpeakerList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            devices.SetSpeaker(SpeakerList.SelectedItem as DeviceViewModel);
        }

        protected void HeadphoneList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            devices.SetHeadphone(HeadphoneList.SelectedItem as DeviceViewModel);
        }
    }
}
