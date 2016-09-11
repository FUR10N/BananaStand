using System.Collections.Generic;
using AudioEndPointControllerWrapper;

namespace BananaStand.ViewModels
{
    public interface IAudioDeviceLister
    {
        /// <summary>
        ///     Get the playback device in the set state
        /// </summary>
        /// <returns></returns>
        ICollection<IAudioDevice> GetPlaybackDevices();

        /// <summary>
        ///     Get the recording device in the set state
        /// </summary>
        /// <returns></returns>
        ICollection<IAudioDevice> GetRecordingDevices();
    }
}
