using AudioEndPointControllerWrapper;

using BananaStand.Properties;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace BananaStand.ViewModels
{
    public class DevicesViewModel
    {
        public List<DeviceViewModel> Devices { get; }

        public DevicesViewModel()
        {
                var devicesLister = new AudioDeviceLister(DeviceState.All);
                Devices = devicesLister.GetPlaybackDevices()
                    .Where(i => i.DeviceState != DeviceState.NotPresent && i.DeviceState != DeviceState.Unplugged)
                    .OrderBy(i => i.DeviceState)
                    .Select(i => new DeviceViewModel(i))
                    .ToList();

                foreach (var d in Devices)
                {
                    d.IsSpeakers = Settings.Default.SpeakerId == d.Device.Id;
                    d.IsHeadphones = Settings.Default.HeadphoneId == d.Device.Id;
                }
        }

        public void SetSpeaker(DeviceViewModel speaker)
        {
            foreach (var d in Devices)
            {
                d.IsSpeakers = d == speaker;
            }
            Settings.Default.SpeakerId = speaker.Device.Id;
            Settings.Default.Save();
        }

        public void SetHeadphone(DeviceViewModel headphone)
        {
            foreach (var d in Devices)
            {
                d.IsHeadphones = d == headphone;
            }
            Settings.Default.HeadphoneId = headphone.Device.Id;
            Settings.Default.Save();
        }

        public DeviceViewModel SelectedSpeakers => Devices.FirstOrDefault(i => i.IsSpeakers);

        public DeviceViewModel SelectedHeadphones => Devices.FirstOrDefault(i => i.IsHeadphones);
    }
}
