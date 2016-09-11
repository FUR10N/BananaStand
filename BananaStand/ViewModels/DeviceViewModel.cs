using System;
using System.ComponentModel;
using System.Windows.Media;
using AudioEndPointControllerWrapper;

namespace BananaStand.ViewModels
{
    public class DeviceViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public readonly IAudioDevice Device;

        public DeviceViewModel(IAudioDevice i)
        {
            Device = i;
            Name = i.FriendlyName;

            Icon = AudioDeviceIconExtractor.ExtractIconFromAudioDevice(i, true);
        }

        public string Name { get; set; }

        private bool isSpeakers;

        public bool IsSpeakers
        {
            get
            {
                return isSpeakers;
            }
            set
            {
                if (isSpeakers != value)
                {
                    isSpeakers = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSpeakers)));
                }
            }
        }

        private bool isHeadphones;

        public bool IsHeadphones
        {
            get
            {
                return isHeadphones;
            }
            set
            {
                if (isHeadphones != value)
                {
                    isHeadphones = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsHeadphones)));
                }
            }
        }

        private ImageSource icon;

        public ImageSource Icon
        {
            get
            {
                return icon;
            }
            set
            {
                if (icon != value)
                {
                    icon = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Icon)));
                }
            }
        }
    }
}
