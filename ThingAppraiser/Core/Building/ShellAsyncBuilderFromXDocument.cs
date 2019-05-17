﻿using System;
using System.Xml.Linq;
using ThingAppraiser.Logging;

namespace ThingAppraiser.Core.Building
{
    /// <summary>
    /// Builder class which provides the way of constructing <see cref="ShellAsync" /> instances 
    /// from <see cref="XDocument" /> config.
    /// </summary>
    /// <remarks>
    /// Structure of XML config must satisfy certain contracts, otherwise different exception could
    /// be thrown.
    /// </remarks>
    public sealed class ShellAsyncBuilderFromXDocument : ShellBuilderBase, IShellAsyncBuilder
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly LoggerAbstraction _logger =
            LoggerAbstraction.CreateLoggerInstanceFor<ShellAsyncBuilderFromXDocument>();

        /// <summary>
        /// Provides methods to create instances of service classes.
        /// </summary>
        private readonly ServiceAsyncBuilderForXmlConfig _serviceBuilder =
            new ServiceAsyncBuilderForXmlConfig();

        /// <summary>
        /// Helper class which contains several methods to parse XML configuration.
        /// </summary>
        private readonly XDocumentParser _documentParser;

        /// <summary>
        /// Default value of bounded capacity of service.
        /// </summary>
        private readonly int _defaultBoundedCapacity = 10;

        /// <summary>
        /// Variables which saves input manager instance during building process.
        /// </summary>
        private IO.Input.InputManagerAsync _inputManager;

        /// <summary>
        /// Variables which saves crawlers manager instance during building process.
        /// </summary>
        private Crawlers.CrawlersManagerAsync _crawlersManager;

        /// <summary>
        /// Variables which saves appraisers manager instance during building process.
        /// </summary>
        private Appraisers.AppraisersManagerAsync _appraisersManager;

        /// <summary>
        /// Variables which saves output manager instance during building process.
        /// </summary>
        private IO.Output.OutputManagerAsync _outputManager;


        /// <summary>
        /// Initializes builder instance and associates <see cref="XDocumentParser" /> which
        /// provides deferred parsing of XML configuration.
        /// </summary>
        /// <param name="configuration">
        /// XML configuration of <see cref="ShellAsync" /> class.
        /// </param>
        public ShellAsyncBuilderFromXDocument(XDocument configuration)
        {
            _documentParser = new XDocumentParser(
                new XDocument(configuration.Root.Element(_rootElementName))
            );
        }

        #region IShellAsyncBuilder Implementation

        /// <inheritdoc />
        public void Reset()
        {
            _inputManager = null;
            _crawlersManager = null;
            _appraisersManager = null;
            _outputManager = null;
        }

        /// <inheritdoc />
        public void BuildMessageHandler()
        {
            XElement messageHandlerElement = _documentParser.FindElement(
                _messageHandlerParameterName
            );

            Communication.GlobalMessageHandler.SetMessageHangler(
                _serviceBuilder.CreateMessageHandler(messageHandlerElement)
            );
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// XML configuration doesn't contain element for input manager with specified name.
        /// </exception>
        public void BuildInputManager()
        {
            XElement inputManagerElement = _documentParser.FindElement(_inputManagerParameterName);
            if (inputManagerElement is null)
            {
                var ex = new InvalidOperationException(
                    $"XML document hasn't value for {_inputManagerParameterName}."
                );
                _logger.Error(ex, "Cannot build InputManager.");
                throw ex;
            }

            string defaultStorageName = XDocumentParser.GetAttributeValue(
                inputManagerElement, _defaultInStorageNameParameterName
            );
            _inputManager = new IO.Input.InputManagerAsync(defaultStorageName);

            foreach (var element in inputManagerElement.Elements())
            {
                IO.Input.IInputterAsync inputter = _serviceBuilder.CreateInputter(element);
                _inputManager.Add(inputter);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// XML configuration doesn't contain element for crawlers manager with specified name.
        /// </exception>
        public void BuildCrawlersManager()
        {
            XElement crawlerManagerElement = _documentParser.FindElement(
                _crawlersManagerParameterName
            );
            if (crawlerManagerElement is null)
            {
                var ex = new InvalidOperationException(
                    $"XML document hasn't value for {_crawlersManagerParameterName}."
                );
                _logger.Error(ex, "Cannot build CrawlersManager.");
                throw ex;
            }

            var crawlersOutput = XDocumentParser.GetAttributeValue<bool>(
                crawlerManagerElement, _crawlersOutputParameterName
            );
            _crawlersManager = new Crawlers.CrawlersManagerAsync(crawlersOutput);

            foreach (var element in crawlerManagerElement.Elements())
            {
                Crawlers.CrawlerAsync crawler = _serviceBuilder.CreateCrawler(element);
                _crawlersManager.Add(crawler);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// XML configuration doesn't contain element for appraisers manager with specified name.
        /// </exception>
        public void BuildAppraisersManager()
        {
            XElement appraiserManagerElement = _documentParser.FindElement(
                _appraisersManagerParameterName
            );
            if (appraiserManagerElement is null)
            {
                var ex = new InvalidOperationException(
                    $"XML document hasn't value for {_appraisersManagerParameterName}."
                );
                _logger.Error(ex, "Cannot build AppraisersManager.");
                throw ex;
            }

            var appraisersOutput = XDocumentParser.GetAttributeValue<bool>(
                appraiserManagerElement, _appraisersOutputParameterName
            );
            _appraisersManager = new Appraisers.AppraisersManagerAsync(appraisersOutput);

            foreach (var element in appraiserManagerElement.Elements())
            {
                Appraisers.AppraiserAsync crawler = _serviceBuilder.CreateAppraiser(element);
                _appraisersManager.Add(crawler);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// XML configuration doesn't contain element for output manager with specified name.
        /// </exception>
        public void BuildOutputManager()
        {
            XElement outputManagerElement = _documentParser.FindElement(
                _outputManagerParameterName
            );
            if (outputManagerElement is null)
            {
                var ex = new InvalidOperationException(
                    $"XML document hasn't value for {_outputManagerParameterName}."
                );
                _logger.Error(ex, "Cannot build OutputManager.");
                throw ex;
            }

            string defaultStorageName = XDocumentParser.GetAttributeValue(
                outputManagerElement, _defaultOutStorageNameParameterName
            );
            _outputManager = new IO.Output.OutputManagerAsync(defaultStorageName);

            foreach (var element in outputManagerElement.Elements())
            {
                IO.Output.IOutputterAsync outputter = _serviceBuilder.CreateOutputter(element);
                _outputManager.Add(outputter);
            }
        }

        /// <inheritdoc />
        public ShellAsync GetResult()
        {
            _logger.Info("Created ShellAsync from user-defined XML config.");
            return new ShellAsync(_inputManager, _crawlersManager, _appraisersManager,
                                  _outputManager, _defaultBoundedCapacity);
        }

        #endregion
    }
}