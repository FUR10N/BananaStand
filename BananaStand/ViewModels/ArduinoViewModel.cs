using AudioEndPointControllerWrapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;

namespace BananaStand.ViewModels
{
    public class ArduinoViewModel : ViewModelBase
    {
        private readonly DevicesViewModel devices;

        private readonly ArduinoReader arduinoReader = new ArduinoReader();

        private bool usingHeadphones;

        public ArduinoViewModel(DevicesViewModel devices)
        {
            StartCommand = new RelayCommand(ExecuteStartCommand);
            StopCommand = new RelayCommand(ExecuteStopCommand);

            this.devices = devices;
            arduinoReader.LightChanged += ArduinoReader_LightChanged;

            StartCommand.Execute(null);
        }

        private void ArduinoReader_LightChanged(object sender, LightChangedEventArgs e)
        {
            HeadphoneLight = e.Headphone;
            AmbientLight = e.Ambient;
            if (devices.SelectedSpeakers == null || devices.SelectedHeadphones == null)
            {
                return;
            }
            if (e.UseHeadphones)
            {
                if (usingHeadphones)
                {
                    return;
                }
                usingHeadphones = true;
                devices.SelectedHeadphones.Device.SetAsDefault(Role.Console);
                devices.SelectedHeadphones.Device.SetAsDefault(Role.Multimedia);
            }
            else
            {
                if (!usingHeadphones)
                {
                    return;
                }
                usingHeadphones = false;
                devices.SelectedSpeakers.Device.SetAsDefault(Role.Console);
                devices.SelectedSpeakers.Device.SetAsDefault(Role.Multimedia);
            }
        }

        public RelayCommand StartCommand { get; }

        private void ExecuteStartCommand()
        {
            arduinoReader.Start();
        }

        public RelayCommand StopCommand { get; }

        private void ExecuteStopCommand()
        {
            arduinoReader.Stop();
        }

        private int ambientLight;

        public int AmbientLight
        {
            get
            {
                return ambientLight;
            }
            private set
            {
                if (Set(ref ambientLight, value))
                {
                    RaisePropertyChanged(nameof(Status));
                }
            }
        }

        private int headphoneLight;

        public int HeadphoneLight
        {
            get
            {
                return headphoneLight;
            }
            private set
            {
                if (Set(ref headphoneLight, value))
                {
                    RaisePropertyChanged(nameof(Status));
                }
            }
        }

        public int Threshold
        {
            get
            {
                return arduinoReader.Threshold;
            }
            set
            {
                arduinoReader.Threshold = value;
            }
        }

        public string Status => $"Light: {HeadphoneLight}, Ambient: {AmbientLight}";
    }
}
