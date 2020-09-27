using Hardcodet.Wpf.TaskbarNotification;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WpfCommon;

namespace TaskbarIcon04.ViewModels
{
    public class NotifyIconViewModel
    {
        public ICommand TestCommand
        {
get
            {
                Action<object> action = parameter =>
                {
                    MessageBox.Show("Test","Caption",MessageBoxButton.OK,MessageBoxImage.Information,MessageBoxResult.OK,MessageBoxOptions.ServiceNotification);
                    GetTaskbarWindow(parameter).Close();
                    CommandManager.InvalidateRequerySuggested();

                };
                Predicate<object> predicate = _=> true;
                return new RelayCommand(action,predicate);
            }
        }

        /// <summary>
        /// Resolves the window that owns the TaskbarIcon class.
        /// </summary>
        /// <param name="commandParameter"></param>
        /// <returns>Window</returns>
        protected Window GetTaskbarWindow(object commandParameter)
        {
            if (IsDesignMode) return null;

            // get the showcase window off the taskbar icon
            var tb = commandParameter as TaskbarIcon;
            return tb == null ? null : TryFindParent<Window>(tb);
        }
        public static bool IsDesignMode
        {
            get
            {
                return (bool)
                    DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                        typeof(FrameworkElement))
                        .Metadata.DefaultValue;
            }
        }
        /// <summary>
        /// Finds a parent of a given item on the visual tree.
        /// </summary>
        /// <typeparam name="TParent">The type of the queried item.</typeparam>
        /// <param name="child">A direct or indirect child of the
        /// queried item.</param>
        /// <returns>The first parent item that matches the submitted
        /// type parameter. If not matching item can be found, a null
        /// reference is being returned.</returns>
        public static TParent TryFindParent<TParent>(DependencyObject child) where TParent : DependencyObject
        {
            //get parent item
            DependencyObject parentObject = GetParentObject(child);

            //we've reached the end of the tree
            if (parentObject == null) return null;

            //check if the parent matches the type we're looking for
            if (parentObject is TParent parent)
            {
                return parent;
            }

            //use recursion to proceed with next level
            return TryFindParent<TParent>(parentObject);
        }
        /// <summary>
        /// This method is an alternative to WPF's
        /// <see cref="VisualTreeHelper.GetParent"/> method, which also
        /// supports content elements. Keep in mind that for content element,
        /// this method falls back to the logical tree of the element!
        /// </summary>
        /// <param name="child">The item to be processed.</param>
        /// <returns>The submitted item's parent, if available. Otherwise
        /// null.</returns>
        public static DependencyObject GetParentObject(DependencyObject child)
        {
            if (child == null) return null;

            if (child is ContentElement contentElement)
            {
                DependencyObject parent = ContentOperations.GetParent(contentElement);
                if (parent != null) return parent;

                FrameworkContentElement fce = contentElement as FrameworkContentElement;
                return fce?.Parent;
            }

            //if it's not a ContentElement, rely on VisualTreeHelper
            return VisualTreeHelper.GetParent(child);
        }
    }
}
