﻿using System;
using System.Collections.Generic;
using ThingAppraiser.DataPipeline;
using ThingAppraiser.Extensions;
using ThingAppraiser.Logging;

namespace ThingAppraiser.Appraisers
{
    public sealed class AppraisersManagerAsync : IManager<IAppraiserAsync>
    {
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor<AppraisersManagerAsync>();

        private readonly Dictionary<Type, IList<IAppraiserAsync>> _appraisersAsync =
            new Dictionary<Type, IList<IAppraiserAsync>>();

        private readonly bool _outputResults;


        public AppraisersManagerAsync(bool outputResults)
        {
            _outputResults = outputResults;
        }

        #region IManager<AppraiserAsync> Implementation

        public void Add(IAppraiserAsync item)
        {
            item.ThrowIfNull(nameof(item));

            if (_appraisersAsync.TryGetValue(item.TypeId, out IList<IAppraiserAsync> list))
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }
            else
            {
                _appraisersAsync.Add(item.TypeId, new List<IAppraiserAsync> { item });
            }
        }

        public bool Remove(IAppraiserAsync item)
        {
            item.ThrowIfNull(nameof(item));
            return _appraisersAsync.Remove(item.TypeId);
        }

        #endregion

        public AppraisersFlow CreateFlow()
        {
            var appraisersFunc = new List<Funcotype>();
            foreach ((Type type, IList<IAppraiserAsync> appraisersAsync) in _appraisersAsync)
            {
                foreach (var appraiserAsync in appraisersAsync)
                {
                    var funcotype = new Funcotype(
                        entityInfo => appraiserAsync.GetRatings(entityInfo, _outputResults),
                        type
                    );
                    appraisersFunc.Add(funcotype);
                }
            }

            var appraisersFlow = new AppraisersFlow(appraisersFunc);

            _logger.Info("Constructed appraisers pipeline.");
            return appraisersFlow;
        }
    }
}
