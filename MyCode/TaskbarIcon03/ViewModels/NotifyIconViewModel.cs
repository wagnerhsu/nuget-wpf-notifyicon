using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Input;
using WpfCommon;

namespace TaskbarIcon03.ViewModels
{
    public class NotifyIconViewModel
    {
        public ICommand TestCommand
        {
get
            {
                Action<object> action = _ =>
                {
                    MessageBox.Show("Test");

                };
                Predicate<object> predicate = _=> true;
                return new RelayCommand(action,predicate);
            }
        }
    }
}
