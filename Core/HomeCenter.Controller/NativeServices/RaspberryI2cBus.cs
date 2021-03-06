﻿using HomeCenter.Model.Native;
using System;
using System.Linq;
using Windows.Devices.Enumeration;
using Windows.Devices.I2c;

namespace HomeCenter.HomeController.NativeServices
{
    internal class RaspberryI2cBus : II2cBus
    {
        public II2cDevice CreateDevice(string deviceId, int slaveAddress)
        {
            var settings = new I2cConnectionSettings(slaveAddress)
            {
                BusSpeed = I2cBusSpeed.StandardMode,
                SharingMode = I2cSharingMode.Exclusive
            };

            var device = I2cDevice.FromIdAsync(deviceId, settings).GetAwaiter().GetResult();

            if (device == null) throw new InvalidOperationException($"Device {deviceId} was not found on I2C bus");

            return new RaspberryI2cDevice(device);
        }

        public string GetBusId()
        {
            var deviceSelector = I2cDevice.GetDeviceSelector();
            var deviceInformation = DeviceInformation.FindAllAsync(deviceSelector).GetAwaiter().GetResult();

            if (deviceInformation.Count == 0)
            {
                throw new InvalidOperationException("I2C bus not found.");
            }

            return deviceInformation.First().Id;
        }
    }
}