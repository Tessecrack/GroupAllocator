using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GroupAllocator.Desktop.Commands
{
    internal class LambdaCommand : Command
    {
        private readonly Func<object, bool> _canExeute;
        private readonly Action<object> _execute;

        public LambdaCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            _execute = execute;
            _canExeute = canExecute;
        }

        public override bool CanExecute(object? parameter) => _canExeute?.Invoke(arg: parameter) ?? true;

        public override void Execute(object? parameter) => _execute?.Invoke(parameter);
    }
}
