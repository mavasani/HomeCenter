﻿using HomeCenter.Adapters.Denon.Messages;
using HomeCenter.CodeGeneration;
using HomeCenter.Model.Adapters;
using HomeCenter.Model.Capabilities;
using HomeCenter.Model.Exceptions;
using HomeCenter.Model.Messages.Commands;
using HomeCenter.Model.Messages.Commands.Device;
using HomeCenter.Model.Messages.Queries.Device;
using HomeCenter.Utils.Extensions;
using Microsoft.Extensions.Logging;
using Proto;
using System;
using System.Threading.Tasks;

namespace HomeCenter.Adapters.Denon
{
    [ProxyCodeGenerator]
    public class DenonAdapter : Adapter
    {
        public const int DEFAULT_POOL_INTERVAL = 2000;
        public const int DEFAULT_VOLUME_CHANGE_FACTOR = 10;

        private bool _powerState;
        private double? _volume;
        private bool _mute;
        private string _input;
        private string _surround;
        private DenonDeviceInfo _fullState;
        private string _description;

        private string _hostName;
        private int _zone;
        private TimeSpan _poolInterval;

        protected override async Task OnStarted(IContext context)
        {
            await base.OnStarted(context).ConfigureAwait(false);

            _hostName = AsString(AdapterProperties.Hostname);
            _poolInterval = AsIntTime(AdapterProperties.PoolInterval, DEFAULT_POOL_INTERVAL);
            _zone = AsInt(AdapterProperties.Zone);

            await DelayDeviceRefresh<RefreshStateJob>(TimeSpan.FromSeconds(1)).ConfigureAwait(false);
            await ScheduleDeviceRefresh<RefreshLightStateJob>(_poolInterval).ConfigureAwait(false);
        }

        protected DiscoveryResponse Discover(DiscoverQuery message)
        {
            return new DiscoveryResponse(RequierdProperties(), new PowerState(),
                                                               new VolumeState(),
                                                               new MuteState(),
                                                               new InputSourceState(),
                                                               new SurroundSoundState(),
                                                               new DescriptionState()
                                          );
        }

        protected async Task Handle(RefreshCommand message)
        {
            _fullState = await MessageBroker.QueryService<DenonStatusQuery, DenonDeviceInfo>(new DenonStatusQuery { Address = _hostName }).ConfigureAwait(false);
            var mapping = await MessageBroker.QueryService<DenonMappingQuery, DenonDeviceInfo>(new DenonMappingQuery { Address = _hostName }).ConfigureAwait(false);
            _fullState.FriendlyName = mapping.FriendlyName;
            _fullState.InputMap = mapping.InputMap;

            _surround = await UpdateState(SurroundSoundState.StateName, _surround, _fullState.Surround).ConfigureAwait(false);
            _description = await UpdateState(DescriptionState.StateName, _description, GetDescription()).ConfigureAwait(false);
        }

        private string GetDescription() => $"{_fullState.FriendlyName} [Model: {_fullState.Model}, Zone: {_zone}, Address: {_hostName}]";

        protected async Task Handle(RefreshLightCommand message)
        {
            var statusQuery = new DenonStatusLightQuery
            {
                Address = _hostName,
                Zone = _zone.ToString()
            };

            var state = await MessageBroker.QueryService<DenonStatusLightQuery, DenonStatus>(statusQuery).ConfigureAwait(false);

            _input = await UpdateState(InputSourceState.StateName, _input, state.ActiveInput).ConfigureAwait(false);
            _mute = await UpdateState(MuteState.StateName, _mute, state.Mute).ConfigureAwait(false);
            _powerState = await UpdateState(PowerState.StateName, _powerState, state.PowerStatus).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, state.MasterVolume).ConfigureAwait(false);            
        }

        protected async Task Handle(TurnOnCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "PowerOn",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "ON").ConfigureAwait(false))
            {
                _powerState = await UpdateState(PowerState.StateName, _powerState, true).ConfigureAwait(false);
            }
        }

        protected async Task Handle(TurnOffCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "PowerStandby",
                Api = "formiPhoneAppPower",
                ReturnNode = "Power",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "OFF").ConfigureAwait(false))
            {
                _powerState = await UpdateState(PowerState.StateName, _powerState, false).ConfigureAwait(false);
            }
        }

        protected async Task Handle(VolumeUpCommand command)
        {
            if (_volume.HasValue)
            {
                var changeFactor = command.AsDouble(CommandProperties.ChangeFactor, DEFAULT_VOLUME_CHANGE_FACTOR);
                var volume = _volume + changeFactor;

                var normalized = NormalizeVolume(volume.Value);

                var control = new DenonControlQuery
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = _hostName,
                    Zone = _zone.ToString()
                };

                // Results are unpredictable so we ignore them
                await MessageBroker.QueryService<DenonControlQuery, string>(control).ConfigureAwait(false);
                _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
            }
        }

        protected async Task Handle(VolumeDownCommand command)
        {
            if (_volume.HasValue)
            {
                var changeFactor = command.AsDouble(CommandProperties.ChangeFactor, DEFAULT_VOLUME_CHANGE_FACTOR);
                var volume = _volume - changeFactor;
                var normalized = NormalizeVolume(volume.Value);

                var control = new DenonControlQuery
                {
                    Command = normalized,
                    Api = "formiPhoneAppVolume",
                    ReturnNode = "MasterVolume",
                    Address = _hostName,
                    Zone = _zone.ToString()
                };

                // Results are unpredictable so we ignore them
                await MessageBroker.QueryService<DenonControlQuery, string>(control).ConfigureAwait(false);
                _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
            }
        }

        protected async Task Handle(VolumeSetCommand command)
        {
            var volume = command.AsDouble(CommandProperties.Value);
            var normalized = NormalizeVolume(volume);

            var control = new DenonControlQuery
            {
                Command = normalized,
                Api = "formiPhoneAppVolume",
                ReturnNode = "MasterVolume",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            await MessageBroker.QueryService<DenonControlQuery, string>(control).ConfigureAwait(false);
            _volume = await UpdateState(VolumeState.StateName, _volume, volume).ConfigureAwait(false);
        }

        private string NormalizeVolume(double volume)
        {
            if (volume < 0) volume = 0;
            if (volume > 100) volume = 100;

            return (volume - 80).ToFloatString();
        }

        protected async Task Handle(MuteCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "MuteOn",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "on").ConfigureAwait(false))
            {
                _mute = await UpdateState(MuteState.StateName, _mute, true).ConfigureAwait(false);
            }
        }

        protected async Task Handle(UnmuteCommand message)
        {
            var control = new DenonControlQuery
            {
                Command = "MuteOff",
                Api = "formiPhoneAppMute",
                ReturnNode = "Mute",
                Address = _hostName,
                Zone = _zone.ToString()
            };

            if (await MessageBroker.QueryServiceWithVerify<DenonControlQuery, string, object>(control, "off").ConfigureAwait(false))
            {
                _mute = await UpdateState(MuteState.StateName, _mute, false).ConfigureAwait(false);
            }
        }

        protected async Task SetInput(InputSetCommand message)
        {
            if (_fullState == null) throw new UnsupportedStateException("Cannot change input source on Denon device because device info was not downloaded from device");
            var inputName = message.AsString(CommandProperties.InputSource);
            var input = _fullState.TranslateInputName(inputName, _zone.ToString());
            if (input?.Length == 0) throw new UnsupportedPropertyStateException($"Input {inputName} was not found on available device input sources");

            var control = new DenonControlQuery
            {
                Command = input,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            };

            await MessageBroker.QueryService<DenonControlQuery, string>(control).ConfigureAwait(false);
            //TODO Check if this value is ok - confront with pooled state
            _input = await UpdateState(InputSourceState.StateName, _input, inputName).ConfigureAwait(false);
        }

        protected async Task Handle(ModeSetCommand message)
        {
            //Surround support only in main zone
            if (_zone != 1) return;
            var surroundMode = message.AsString(CommandProperties.SurroundMode);
            var mode = DenonSurroundModes.MapApiCommand(surroundMode);
            if (mode?.Length == 0) throw new UnsupportedPropertyStateException($"Surroundmode {mode} was not found on available surround modes");

            var control = new DenonControlQuery
            {
                Command = mode,
                Api = "formiPhoneAppDirect",
                ReturnNode = "",
                Zone = "",
                Address = _hostName
            };

            await MessageBroker.QueryService<DenonControlQuery, string>(control).ConfigureAwait(false);
            //TODO Check if this value is ok - confront with pooled state
            _surround = await UpdateState(SurroundSoundState.StateName, _surround, surroundMode).ConfigureAwait(false);
        }
    }
}