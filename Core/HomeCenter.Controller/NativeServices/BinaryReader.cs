﻿using HomeCenter.Model.Native;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage.Streams;

namespace HomeCenter.HomeController.NativeServices
{
    internal class BinaryReader : IBinaryReader
    {
        private readonly DataReader _dataReader;

        public BinaryReader(IInputStream stream)
        {
            _dataReader = new DataReader(stream)
            {
                ByteOrder = ByteOrder.LittleEndian,
                InputStreamOptions = InputStreamOptions.Partial
            };
        }

        public void Dispose()
        {
            _dataReader.DetachStream();
            _dataReader.Dispose();
        }

        public byte ReadByte() => _dataReader.ReadByte();

        public float ReadSingle() => _dataReader.ReadSingle();

        public string ReadString(byte size) => _dataReader.ReadString(size);

        public uint ReadUInt32() => _dataReader.ReadUInt32();

        public Task<uint> LoadAsync(uint count, CancellationToken cancellationToken) => _dataReader.LoadAsync(count).AsTask(cancellationToken);
    }
}