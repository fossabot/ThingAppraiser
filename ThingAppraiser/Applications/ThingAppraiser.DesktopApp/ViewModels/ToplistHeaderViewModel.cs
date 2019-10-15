﻿using System.Windows.Input;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using ThingAppraiser.DesktopApp.Domain;
using ThingAppraiser.DesktopApp.Domain.Messages;
using ThingAppraiser.Extensions;
using ThingAppraiser.Logging;

namespace ThingAppraiser.DesktopApp.ViewModels
{
    internal sealed class ToplistHeaderViewModel : BindableBase
    {
        private static readonly ILogger _logger =
            LoggerFactory.CreateLoggerFor<ToplistHeaderViewModel>();

        private readonly IEventAggregator _eventAggregator;

        public object DialogIdentifier { get; }

        public ICommand CreateToplistDialogCommand { get; }

        public ICommand OpenToplistFileDialogCommand { get; }

        public ICommand OpenToplistFromDriveDialogCommand { get; }


        public ToplistHeaderViewModel(IEventAggregator eventAggregator)
        {
            DialogIdentifier = MainDialogIdentifier.DialogIdentifier;
            _eventAggregator = eventAggregator.ThrowIfNull(nameof(eventAggregator));

            CreateToplistDialogCommand = new DelegateCommand<ToplistHeaderViewModel>(
                ExecutableDialogs.ExecuteCreateToplistDialog
            );
            OpenToplistFileDialogCommand = new DelegateCommand(SendLoadToplistFileMessage);
            OpenToplistFromDriveDialogCommand = new DelegateCommand(OpenToplistFromDrive);
        }

        private void SendLoadToplistFileMessage()
        {
            string? filename = ExecutableDialogs.ExecuteOpenToplistFileDialog();
            if (string.IsNullOrWhiteSpace(filename))
            {
                _logger.Info("Skipping openning toplist because got an empty filename value.");
                return;
            }

            _eventAggregator.GetEvent<LoadToplistFileMessage>().Publish(filename);
        }

        private void OpenToplistFromDrive()
        {
            MessageBoxProvider.ShowInfo("Work in progress.");
            // TODO: implement logic to load toplist file from Google Drive and parse it.
        }
    }
}