﻿using System;
using System.Xml.Linq;
using ThingAppraiser.Logging;

namespace ThingAppraiser.Core.Building
{
    /// <summary>
    /// Builder class which provides the way of constructing <see cref="Shell" /> instances from
    /// <see cref="XDocument" /> config.
    /// </summary>
    /// <remarks>
    /// Structure of XML config must satisfy certain contracts, otherwise different exception could
    /// be thrown.
    /// </remarks>
    public sealed class CShellBuilderFromXDocument : ShellBuilderBase, IShellBuilder
    {
        /// <summary>
        /// Logger instance for current class.
        /// </summary>
        private static readonly LoggerAbstraction _logger =
            LoggerAbstraction.CreateLoggerInstanceFor<CShellBuilderFromXDocument>();

        /// <summary>
        /// Provides methods to create instances of service classes.
        /// </summary>
        private readonly ServiceBuilderForXmlConfig _serviceBuilder =
            new ServiceBuilderForXmlConfig();

        /// <summary>
        /// Helper class which contains several methods to parse XML configuration.
        /// </summary>
        private readonly XDocumentParser _documentParser;

        /// <summary>
        /// Variables which saves input manager instance during building process.
        /// </summary>
        private IO.Input.InputManager _inputManager;

        /// <summary>
        /// Variables which saves crawlers manager instance during building process.
        /// </summary>
        private Crawlers.CrawlersManager _crawlersManager;

        /// <summary>
        /// Variables which saves appraisers manager instance during building process.
        /// </summary>
        private Appraisers.AppraisersManager _appraisersManager;

        /// <summary>
        /// Variables which saves output manager instance during building process.
        /// </summary>
        private IO.Output.OutputManager _outputManager;

        /// <summary>
        /// Variables which saves data base manager instance during building process.
        /// </summary>
        private DAL.DataBaseManager _dataBaseManager;


        /// <summary>
        /// Initializes builder instance and associates <see cref="XDocumentParser" /> which
        /// provides deferred parsing of XML configuration.
        /// </summary>
        /// <param name="configuration">XML configuration of <see cref="Shell" /> class.</param>
        public CShellBuilderFromXDocument(XDocument configuration)
        {
            _documentParser = new XDocumentParser(
                new XDocument(configuration.Root.Element(_rootElementName))
            );
        }

        #region IShellBuilder Implementation

        /// <inheritdoc />
        public void Reset()
        {
            _inputManager = null;
            _crawlersManager = null;
            _appraisersManager = null;
            _outputManager = null;
            _dataBaseManager = null;
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

            String defaultStorageName = XDocumentParser.GetAttributeValue(
                inputManagerElement, _defaultInStorageNameParameterName
            );
            _inputManager = new IO.Input.InputManager(defaultStorageName);

            foreach (var element in inputManagerElement.Elements())
            {
                IO.Input.IInputter inputter = _serviceBuilder.CreateInputter(element);
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

            var crawlersOutput = XDocumentParser.GetAttributeValue<Boolean>(
                crawlerManagerElement, _crawlersOutputParameterName
            );
            _crawlersManager = new Crawlers.CrawlersManager(crawlersOutput);

            foreach (var element in crawlerManagerElement.Elements())
            {
                Crawlers.Crawler crawler = _serviceBuilder.CreateCrawler(element);
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

            var appraisersOutput = XDocumentParser.GetAttributeValue<Boolean>(
                appraiserManagerElement, _appraisersOutputParameterName
            );
            _appraisersManager = new Appraisers.AppraisersManager(appraisersOutput);

            foreach (var element in appraiserManagerElement.Elements())
            {
                Appraisers.Appraiser crawler = _serviceBuilder.CreateAppraiser(element);
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

            String defaultStorageName = XDocumentParser.GetAttributeValue(
                outputManagerElement, _defaultOutStorageNameParameterName
            );
            _outputManager = new IO.Output.OutputManager(defaultStorageName);

            foreach (var element in outputManagerElement.Elements())
            {
                IO.Output.IOutputter outputter = _serviceBuilder.CreateOutputter(element);
                _outputManager.Add(outputter);
            }
        }

        /// <inheritdoc />
        /// <exception cref="InvalidOperationException">
        /// XML configuration doesn't contain element for data base manager with specified name.
        /// </exception>
        public void BuildDataBaseManager()
        {
            XElement dataBaseManagerElement = _documentParser.FindElement(
                _dataBaseManagerParameterName
            );
            if (dataBaseManagerElement is null)
            {
                var ex = new InvalidOperationException(
                    $"XML document hasn't value for {_dataBaseManagerParameterName}."
                );
                _logger.Error(ex, "Cannot build DataBaseManager.");
                throw ex;
            }

            string connectionString = XDocumentParser.GetAttributeValue(
                dataBaseManagerElement, _connectionStringParameterName
            );
            var dataBaseSettings = new DAL.DataStorageSettings(connectionString);
            _dataBaseManager = new DAL.DataBaseManager(
                new DAL.Repositories.ResultInfoRepository(dataBaseSettings),
                new DAL.Repositories.RatingRepository(dataBaseSettings)
            );

            foreach (var element in dataBaseManagerElement.Elements())
            {
                DAL.Repositories.IDataRepository repository = _serviceBuilder.CreateRepository(
                    element, dataBaseSettings
                );
                _dataBaseManager.DataRepositoriesManager.Add(repository);
            }
        }

        /// <inheritdoc />
        public Shell GetResult()
        {
            _logger.Info("Created Shell from user-defined XML config.");
            return new Shell(_inputManager, _crawlersManager, _appraisersManager, _outputManager,
                             _dataBaseManager);
        }

        #endregion
    }
}