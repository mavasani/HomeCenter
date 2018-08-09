﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HomeCenter.Core.Interface.Native;
using Microsoft.Extensions.Logging;

namespace HomeCenter.Core.Services
{
    public class SerialMessagingService : ISerialMessagingService
    {
        private IBinaryReader _dataReader;

        private readonly ILogger<SerialMessagingService> _logService;
        private readonly ISerialDevice _serialDevice;
        private readonly List<Func<byte, byte, IBinaryReader, Task<bool>>> _messageHandlers = new List<Func<byte, byte, IBinaryReader, Task<bool>>>();
        private readonly DisposeContainer _disposeContainer = new DisposeContainer();
        private readonly ILogger<SerialMessagingService> _logger;

        public SerialMessagingService(ISerialDevice serialDevice, ILogger<SerialMessagingService> logger)
        {
            _serialDevice = serialDevice ?? throw new ArgumentNullException(nameof(serialDevice));
            _logger = logger;
        }

        public async Task Initialize()
        {
            await _serialDevice.Init().ConfigureAwait(false);
            _dataReader = _serialDevice.GetBinaryReader();

            _disposeContainer.Add(_dataReader);
            _disposeContainer.Add(_serialDevice);

            var task = Task.Run(async () => await Listen().ConfigureAwait(false), _disposeContainer.Token);
        }

        public void RegisterMessageHandler(Func<byte, byte, IBinaryReader, Task<bool>> handler) => _messageHandlers.Add(handler);
        public void Dispose() => _disposeContainer.Dispose();

        private async Task Listen()
        {
            try
            {
                while (true)
                {
                    await ReadAsync(_disposeContainer.Token).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                _logService.LogError(ex.ToString());
            }
        }

        private async Task ReadAsync(CancellationToken cancellationToken)
        {
            const uint messageHeaderSize = 2;
            cancellationToken.ThrowIfCancellationRequested();

            using (var childCancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken))
            {
                var headerBytesRead = await _dataReader.LoadAsync(messageHeaderSize, childCancellationTokenSource.Token).ConfigureAwait(false);
                if (headerBytesRead > 0)
                {
                    var messageBodySize = _dataReader.ReadByte();
                    var messageType = _dataReader.ReadByte();

                    var bodyBytesReaded = await _dataReader.LoadAsync(messageBodySize, childCancellationTokenSource.Token).ConfigureAwait(false);
                    if (bodyBytesReaded > 0)
                    {
                        foreach (var handler in _messageHandlers)
                        {
                            if(await handler(messageType, messageBodySize, _dataReader).ConfigureAwait(false))
                            {
                                break;
                            }
                        }
                    }
                }
            }
        }
    }
}