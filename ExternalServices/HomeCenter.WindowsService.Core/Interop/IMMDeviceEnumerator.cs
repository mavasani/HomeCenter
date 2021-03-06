﻿using HomeCenter.WindowsService.Core.Interop.Enum;
using System;
using System.Runtime.InteropServices;

namespace HomeCenter.WindowsService.Core.Interop
{
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMMDeviceEnumerator
    {
        [PreserveSig]
        int EnumAudioEndpoints(AudioDeviceKind dataFlow, AudioDeviceState stateMask,
            out IMMDeviceCollection devices);

        [PreserveSig]
        int GetDefaultAudioEndpoint(AudioDeviceKind dataFlow, AudioDeviceRole role, out IMMDevice endpoint);

        [PreserveSig]
        int GetDevice(string id, out IMMDevice deviceName);

        [PreserveSig]
        int RegisterEndpointNotificationCallback(IMMNotificationClient client);

        [PreserveSig]
        int UnregisterEndpointNotificationCallback(IMMNotificationClient client);
    }
}