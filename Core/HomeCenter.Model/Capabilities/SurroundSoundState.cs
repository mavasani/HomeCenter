﻿using HomeCenter.Model.Capabilities.Constants;
using HomeCenter.Model.Messages.Commands.Device;

namespace HomeCenter.Model.Capabilities
{
    public class SurroundSoundState : StateBase
    {
        public static string StateName { get; } = nameof(SurroundSoundState);

        public SurroundSoundState(string ReadWriteMode = default) : base(ReadWriteMode)
        {
            this[StateProperties.StateName] = nameof(SurroundSoundState);
            this[StateProperties.CapabilityName] = Constants.Capabilities.SpeakerController;
            this.SetPropertyList(StateProperties.SupportedCommands, nameof(ModeSetCommand));
        }
    }
}