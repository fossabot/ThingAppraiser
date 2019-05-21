﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using ThingAppraiser.Communication;
using ThingAppraiser.Logging;

namespace ThingAppraiser.IO.Input
{
    public sealed class InputManagerAsync : IManager<IInputterAsync>
    {
        private static readonly LoggerAbstraction _logger =
            LoggerAbstraction.CreateLoggerInstanceFor<InputManagerAsync>();

        private readonly string _defaultStorageName;

        private readonly List<IInputterAsync> _inputtersAsync = new List<IInputterAsync>();


        public InputManagerAsync(string defaultStorageName)
        {
            _defaultStorageName = defaultStorageName.ThrowIfNullOrWhiteSpace(
                nameof(defaultStorageName)
            );
        }

        #region IManager<IInputterAsync> Implementation

        public void Add(IInputterAsync item)
        {
            item.ThrowIfNull(nameof(item));
            if (!_inputtersAsync.Contains(item))
            {
                _inputtersAsync.Add(item);
            }
        }

        public bool Remove(IInputterAsync item)
        {
            item.ThrowIfNull(nameof(item));
            return _inputtersAsync.Remove(item);
        }

        #endregion

        public async Task<bool> GetNames(BufferBlock<string> queueToWrite, string storageName)
        {
            if (string.IsNullOrWhiteSpace(storageName))
            {
                storageName = _defaultStorageName;

                string message = "Storage name is empty, using the default value.";
                _logger.Info(message);
                GlobalMessageHandler.OutputMessage(message);
            }

            List<Task<bool>> producers = _inputtersAsync.Select(
                inputterAsync => TryReadThingNames(inputterAsync, queueToWrite, storageName)
            ).ToList();

            bool[] statuses = await Task.WhenAll(producers);
            queueToWrite.Complete();

            if (!statuses.IsNullOrEmpty() && statuses.All(r => r))
            {
                _logger.Info($"{statuses.Length} Thing names queues were read.");
                return true;
            }

            _logger.Info($"No Things were found in \"{storageName}\".");
            return false;
        }

        private static async Task<bool> TryReadThingNames(IInputterAsync inputterAsync,
            BufferBlock<string> queueToWrite, string storageName)
        {
            try
            {
                await inputterAsync.ReadThingNames(queueToWrite, storageName);
            }
            catch (Exception ex)
            {
                _logger.Warn(ex, "Couldn't get access to the storage.");
                return false;
            }
            return true;
        }
    }
}