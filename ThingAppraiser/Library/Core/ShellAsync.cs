﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using System.Xml.Linq;
using ThingAppraiser.Communication;
using ThingAppraiser.Data;
using ThingAppraiser.Logging;

namespace ThingAppraiser.Core
{
    public sealed class ShellAsync
    {
        private static readonly LoggerAbstraction _logger =
            LoggerAbstraction.CreateLoggerInstanceFor<ShellAsync>();

        private readonly int _boundedCapacity;

        private readonly DataflowBlockOptions _dataFlowOptions;

        public IO.Input.InputManagerAsync InputManagerAsync { get; }

        public Crawlers.CrawlersManagerAsync CrawlersManagerAsync { get; }

        public Appraisers.AppraisersManagerAsync AppraisersManagerAsync { get; }

        public IO.Output.OutputManagerAsync OutputManagerAsync { get; }


        public ShellAsync(
            IO.Input.InputManagerAsync inputManagerAsync,
            Crawlers.CrawlersManagerAsync crawlersManagerAsync,
            Appraisers.AppraisersManagerAsync appraisersManagerAsync,
            IO.Output.OutputManagerAsync outputManagerAsync,
            int boundedCapacity)
        {
            InputManagerAsync = inputManagerAsync.ThrowIfNull(nameof(inputManagerAsync));
            CrawlersManagerAsync = crawlersManagerAsync.ThrowIfNull(nameof(crawlersManagerAsync));
            AppraisersManagerAsync = 
                appraisersManagerAsync.ThrowIfNull(nameof(appraisersManagerAsync));
            OutputManagerAsync = outputManagerAsync.ThrowIfNull(nameof(outputManagerAsync));

            _boundedCapacity = boundedCapacity;
            _dataFlowOptions = new DataflowBlockOptions { BoundedCapacity = _boundedCapacity };
        }

        public static Building.ShellAsyncBuilderDirector CreateBuilderDirector(
            XDocument configuration)
        {
            return new Building.ShellAsyncBuilderDirector(
                new Building.ShellAsyncBuilderFromXDocument(configuration)
            );
        }

        private async Task<ServiceStatus> GetThingNames(BufferBlock<string> queue, string storageName)
        {
            try
            {
                bool status = await InputManagerAsync.GetNames(queue, storageName);
                if (status)
                {
                    GlobalMessageHandler.OutputMessage("Things were successfully gotten.");
                    return ServiceStatus.Ok;
                }

                GlobalMessageHandler.OutputMessage($"No Things were found in \"{storageName}\".");
                return ServiceStatus.Nothing;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured during input work.");
                return ServiceStatus.InputError;
            }
        }

        private async Task<ServiceStatus> RequestData(BufferBlock<string> entitiesQueue,
            IDictionary<Type, BufferBlock<BasicInfo>> responsesQueues)
        {
            try
            {
                bool status = await CrawlersManagerAsync.CollectAllResponses(
                    entitiesQueue, responsesQueues, _dataFlowOptions
                );
                if (status)
                {
                    GlobalMessageHandler.OutputMessage(
                        "Crawlers have received responses from services."
                    );
                    return ServiceStatus.Ok;
                }

                GlobalMessageHandler.OutputMessage(
                    "Crawlers have not received responses from services. Result is empty."
                );
                return ServiceStatus.Nothing;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured during collecting data.");
                return ServiceStatus.RequestError;
            }
        }

        private async Task<ServiceStatus> AppraiseThings(
            IDictionary<Type, BufferBlock<BasicInfo>> entitiesInfoQueues,
            IDictionary<Type, BufferBlock<RatingDataContainer>> entitiesRatingQueues)
        {
            try
            {
                bool status = await AppraisersManagerAsync.GetAllRatings(
                    entitiesInfoQueues, entitiesRatingQueues, _dataFlowOptions
                );
                if (status)
                {
                    GlobalMessageHandler.OutputMessage(
                        "Appraisers have calculated ratings successfully."
                    );
                    return ServiceStatus.Ok;
                }

                GlobalMessageHandler.OutputMessage(
                    "Appraisers have not calculated ratings. Result is empty."
                );
                return ServiceStatus.Nothing;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured during appraising work.");
                return ServiceStatus.AppraiseError;
            }
        }

        private async Task<ServiceStatus> SaveResults(
            IDictionary<Type, BufferBlock<RatingDataContainer>> ratings)
        {
            try
            {
                bool status = await OutputManagerAsync.SaveResults(ratings, string.Empty);
                if (status)
                {
                    GlobalMessageHandler.OutputMessage("Ratings was saved successfully.");
                    return ServiceStatus.Ok;
                }

                GlobalMessageHandler.OutputMessage("Ratings wasn't saved.");
                return ServiceStatus.OutputUnsaved;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Exception occured during output work.");
                return ServiceStatus.OutputError;
            }
        }

        public async Task<ServiceStatus> Run(string storageName)
        {
            GlobalMessageHandler.OutputMessage("Shell started work.");
            _logger.Info("Shell started work.");

            var inputQueue = new BufferBlock<string>(_dataFlowOptions);

            var responsesQueues = new Dictionary<Type, BufferBlock<BasicInfo>>();

            var ratingsQueues = new Dictionary<Type, BufferBlock<RatingDataContainer>>();

            // Input component work.
            Task<ServiceStatus> inputStatus = GetThingNames(inputQueue, storageName);

            // Crawlers component work.
            Task<ServiceStatus> crawlersStatus = RequestData(inputQueue, responsesQueues);

            // Appraisers component work.
            Task<ServiceStatus> appraisersStatus = AppraiseThings(responsesQueues, ratingsQueues);

            // Output component work.
            Task<ServiceStatus> outputStatus = SaveResults(ratingsQueues);

            Task<ServiceStatus[]> statusesTask = Task.WhenAll(inputStatus, crawlersStatus,
                                                              appraisersStatus, outputStatus);

            Task responsesQueuesTasks = Task.WhenAll(
                responsesQueues.Values.Select(bufferBlock => bufferBlock.Completion)
            );
            Task ratingsQueuesTasks = Task.WhenAll(
                ratingsQueues.Values.Select(bufferBlock => bufferBlock.Completion)
            );

            await Task.WhenAll(statusesTask, inputQueue.Completion, responsesQueuesTasks,
                               ratingsQueuesTasks);

            // FIX ME: if there are error statuses need to create aggregate status which contains
            // more details then simple EStatus.Error value.
            ServiceStatus[] statuses = await statusesTask;
            if (statuses.Any(status => status != ServiceStatus.Ok))
            {
                GlobalMessageHandler.OutputMessage(
                    "Shell got error status during data processing."
                );
                _logger.Info("Shell got error status during data processing.");
                return ServiceStatus.Error;
            }

            _logger.Info("Shell finished work successfully.");
            GlobalMessageHandler.OutputMessage("Shell finished work successfully.");
            return ServiceStatus.Ok;
        }
    }
}