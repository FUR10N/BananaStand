/********************************************************************
* Copyright (C) 2015 Antoine Aflalo
* 
* This program is free software; you can redistribute it and/or
* modify it under the terms of the GNU General Public License
* as published by the Free Software Foundation; either version 2
* of the License, or (at your option) any later version.
* 
* This program is distributed in the hope that it will be useful,
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
* GNU General Public License for more details.
********************************************************************/

using System;
using System.Collections.Generic;
using System.Threading;
using AudioEndPoint;
using AudioEndPointControllerWrapper;

namespace BananaStand.ViewModels
{

    public class AudioDeviceLister : IAudioDeviceLister
    {
        private readonly DeviceState _state;
        private readonly HashSet<IAudioDevice> _recording = new HashSet<IAudioDevice>();
        private readonly HashSet<IAudioDevice> _playback = new HashSet<IAudioDevice>();
        private volatile bool _needUpdate = true;
        private readonly ReaderWriterLockSlim _cacheLock = new ReaderWriterLockSlim();

        public AudioDeviceLister(DeviceState state)
        {
            _state = state;
            AudioController.DeviceAdded += AudioControllerOnDeviceAdded;
            AudioController.DeviceRemoved += AudioControllerOnDeviceRemoved;
            AudioController.DeviceStateChanged += AudioControllerOnDeviceStateChanged;
        }

        ~AudioDeviceLister()
        {
            AudioController.DeviceAdded -= AudioControllerOnDeviceAdded;
            AudioController.DeviceRemoved -= AudioControllerOnDeviceRemoved;
            AudioController.DeviceStateChanged -= AudioControllerOnDeviceStateChanged;
        }

        private void AudioControllerOnDeviceStateChanged(object sender, DeviceStateChangedEvent deviceStateChangedEvent)
        {
            _cacheLock.EnterWriteLock();
            try
            {
                if ((deviceStateChangedEvent.newState | _state) == _state)
                {
                    if (deviceStateChangedEvent.device.Type == AudioDeviceType.Playback)
                    {
                        _playback.Add(deviceStateChangedEvent.device);
                    }
                    else
                    {
                        _recording.Add(deviceStateChangedEvent.device);
                    }
                }
                else if ((deviceStateChangedEvent.previousState | _state) == _state)
                {
                    if (deviceStateChangedEvent.device.Type == AudioDeviceType.Playback)
                    {
                        _playback.Remove(deviceStateChangedEvent.device);
                    }
                    else
                    {
                        _recording.Remove(deviceStateChangedEvent.device);
                    }
                }
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        private void AudioControllerOnDeviceRemoved(object sender, DeviceRemovedEvent deviceRemovedEvent)
        {
            _needUpdate = true;
        }

        private void AudioControllerOnDeviceAdded(object sender, DeviceAddedEvent deviceAddedEvent)
        {
            _needUpdate = true;
        }


        /// <summary>
        ///     Get the playback device in the set state
        /// </summary>
        /// <returns></returns>
        public ICollection<IAudioDevice> GetPlaybackDevices()
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (_needUpdate)
                {
                    Refresh();
                }
                return _playback;
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }


        }

        private void Refresh()
        {
            _cacheLock.EnterWriteLock();
            try
            {
                _playback.Clear();

                try
                {
                    _playback.UnionWith(AudioController.GetPlaybackDevices(_state));
                }
                catch (DefSoundException e)
                {
                }


                _recording.Clear();
                try
                {
                    _recording.UnionWith(AudioController.GetRecordingDevices(_state));
                }
                catch (DefSoundException e)
                {
                }
                _needUpdate = false;
            }
            finally
            {
                _cacheLock.ExitWriteLock();
            }
        }

        /// <summary>
        ///     Get the recording device in the set state
        /// </summary>
        /// <returns></returns>
        public ICollection<IAudioDevice> GetRecordingDevices()
        {
            _cacheLock.EnterUpgradeableReadLock();
            try
            {
                if (_needUpdate)
                {
                    Refresh();
                }
                return _recording;
            }
            finally
            {
                _cacheLock.ExitUpgradeableReadLock();
            }
        }
    }
}
