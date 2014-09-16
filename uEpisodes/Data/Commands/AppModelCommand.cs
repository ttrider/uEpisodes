using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace TTRider.uEpisodes.Data.Commands
{
    abstract class AppModelCommand : ICommand
    {
        private bool canCommandExecute;
        protected AppModel Model { get { return AppModel.Current; } }

        public AppModelCommand()
        {
            this.CanCommandExecute = true;
        }

        protected bool CanCommandExecute
        {
            get { return this.canCommandExecute; }
            set
            {
                if (this.canCommandExecute != value)
                {
                    this.canCommandExecute = value;
                    OnCanExecuteChanged();
                }
            }
        }

        public virtual bool CanExecute(object parameter)
        {
            return this.CanCommandExecute;
        }

        public event EventHandler CanExecuteChanged;

        public abstract void Execute(object parameter);

        protected virtual void OnCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
