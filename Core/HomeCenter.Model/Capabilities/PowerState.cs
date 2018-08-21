﻿using HomeCenter.ComponentModel.Capabilities.Constants;
using HomeCenter.ComponentModel.ValueTypes;
using HomeCenter.Model.Commands.Specialized;

namespace HomeCenter.ComponentModel.Capabilities
{
    public class PowerState : State
    {
        public static string StateName { get; } = nameof(PowerState);

        public PowerState(StringValue ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = new StringValue(nameof(PowerState));
            this[StateProperties.CapabilityName] = new StringValue(Constants.Capabilities.PowerController);
            this[StateProperties.Value] = new StringValue();
            this[StateProperties.ValueList] = new StringListValue(PowerStateValue.ON, PowerStateValue.OFF);
            this[StateProperties.SupportedCommands] = new StringListValue(nameof(TurnOnCommand), nameof(TurnOffCommand));
        }
    }
}