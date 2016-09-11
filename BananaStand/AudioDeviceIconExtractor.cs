using AudioEndPointControllerWrapper;
using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace BananaStand
{
    internal class AudioDeviceIconExtractor
    {
        private static readonly Dictionary<string, ImageSource> IconCache = new Dictionary<string, ImageSource>();

        /// <summary>
        ///     Extract the Icon out of an AudioDevice
        /// </summary>
        /// <param name="audioDevice"></param>
        /// <param name="largeIcon"></param>
        /// <returns></returns>
        public static ImageSource ExtractIconFromAudioDevice(IAudioDevice audioDevice, bool largeIcon)
        {
            ImageSource ico;
            if (IconCache.TryGetValue(audioDevice.DeviceClassIconPath, out ico))
            {
                return ico;
            }
            try
            {
                if (audioDevice.DeviceClassIconPath.EndsWith(".ico"))
                {
                    ico = System.Drawing.Icon.ExtractAssociatedIcon(audioDevice.DeviceClassIconPath).ToImageSource();
                }
                else
                {
                    var iconInfo = audioDevice.DeviceClassIconPath.Split(',');
                    var dllPath = iconInfo[0];
                    var iconIndex = int.Parse(iconInfo[1]);
                    ico = IconExtractor.Extract(dllPath, iconIndex, largeIcon);
                }
            }
            catch (Exception e)
            {
                switch (audioDevice.Type)
                {
                    case AudioDeviceType.Playback:
                        //ico = Resources.defaultSpeakers;
                        break;
                    case AudioDeviceType.Recording:
                        //ico = Resources.defaultMicrophone;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            IconCache.Add(audioDevice.DeviceClassIconPath, ico);
            return ico;
        }
    }
}