using System.Collections.Generic;
using System.Windows.Input;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;

namespace HatDesktop.Views
{
    public class CustomKeyboardCommandProvider : DefaultKeyboardCommandProvider
    {
        private readonly GridViewDataControl _dataControl;

        public CustomKeyboardCommandProvider(GridViewDataControl dataControl)
            : base(dataControl)
        {
            _dataControl = dataControl;
        }

        public override IEnumerable<ICommand> ProvideCommandsForKey(Key key)
        {
            if (key != Key.Return)
            {
                return base.ProvideCommandsForKey(key);
            }

            var commandsToExecute = new List<ICommand>();

            if (_dataControl.CurrentCell == null)
            {
                return commandsToExecute;
            }

            commandsToExecute.Add(_dataControl.CurrentCell.IsInEditMode
                ? RadGridViewCommands.CommitEdit
                : RadGridViewCommands.ActivateRow);

            return commandsToExecute;
        }
    }
}
