﻿using System.Threading.Tasks;
using System.Windows.Input;

namespace ThingAppraiser.DesktopApp.Domain.Commands
{
    internal interface IAsyncCommand : ICommand
    {
        Task ExecuteAsync();

        bool CanExecute();
    }
}
