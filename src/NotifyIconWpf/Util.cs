﻿// hardcodet.net NotifyIcon for WPF
// Copyright (c) 2009 - 2022 Philipp Sumi. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// Contact and Information: http://www.hardcodet.net

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Hardcodet.Wpf.TaskbarNotification.Interop;

namespace Hardcodet.Wpf.TaskbarNotification
{
    /// <summary>
    /// Util and extension methods.
    /// </summary>
    internal static class Util
    {
        public static readonly object SyncRoot = new object();

        #region IsDesignMode

        private static readonly bool isDesignMode;

        /// <summary>
        /// Checks whether the application is currently in design mode.
        /// </summary>
        public static bool IsDesignMode
        {
            get { return isDesignMode; }
        }

        #endregion

        #region construction

        static Util()
        {
            isDesignMode =
                (bool)
                DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty,
                        typeof(FrameworkElement))
                    .Metadata.DefaultValue;
        }

        #endregion

        #region CreateHelperWindow

        /// <summary>
        /// Creates an transparent window without dimension that
        /// can be used to temporarily obtain focus and/or
        /// be used as a window message sink.
        /// </summary>
        /// <returns>Empty window.</returns>
        public static Window CreateHelperWindow()
        {
            return new Window
            {
                Width = 0,
                Height = 0,
                ShowInTaskbar = false,
                WindowStyle = WindowStyle.None,
                AllowsTransparency = true,
                Opacity = 0
            };
        }

        #endregion

        #region WriteIconData

        /// <summary>
        /// Updates the taskbar icons with data provided by a given
        /// <see cref="NotifyIconData"/> instance.
        /// </summary>
        /// <param name="data">Configuration settings for the NotifyIcon.</param>
        /// <param name="command">Operation on the icon (e.g. delete the icon).</param>
        /// <returns>True if the data was successfully written.</returns>
        /// <remarks>See Shell_NotifyIcon documentation on MSDN for details.</remarks>
        public static bool WriteIconData(ref NotifyIconData data, NotifyCommand command)
        {
            return WriteIconData(ref data, command, data.ValidMembers);
        }


        /// <summary>
        /// Updates the taskbar icons with data provided by a given
        /// <see cref="NotifyIconData"/> instance.
        /// </summary>
        /// <param name="data">Configuration settings for the NotifyIcon.</param>
        /// <param name="command">Operation on the icon (e.g. delete the icon).</param>
        /// <param name="flags">Defines which members of the <paramref name="data"/>
        /// structure are set.</param>
        /// <returns>True if the data was successfully written.</returns>
        /// <remarks>See Shell_NotifyIcon documentation on MSDN for details.</remarks>
        public static bool WriteIconData(ref NotifyIconData data, NotifyCommand command, IconDataMembers flags)
        {
            //do nothing if in design mode
            if (IsDesignMode) return true;

            data.ValidMembers = flags;
            lock (SyncRoot)
            {
                return WinApi.Shell_NotifyIcon(command, ref data);
            }
        }

        #endregion

        #region GetBalloonFlag

        /// <summary>
        /// Gets a <see cref="BalloonFlags"/> enum value that
        /// matches a given <see cref="BalloonIcon"/>.
        /// </summary>
        public static BalloonFlags GetBalloonFlag(this BalloonIcon icon)
        {
            switch (icon)
            {
                case BalloonIcon.None:
                    return BalloonFlags.None;
                case BalloonIcon.Info:
                    return BalloonFlags.Info;
                case BalloonIcon.Warning:
                    return BalloonFlags.Warning;
                case BalloonIcon.Error:
                    return BalloonFlags.Error;
                default:
                    throw new ArgumentOutOfRangeException("icon");
            }
        }

        #endregion

        #region evaluate listings

        /// <summary>
        /// Checks a list of candidates for equality to a given
        /// reference value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="value">The evaluated value.</param>
        /// <param name="candidates">A list of possible values that are
        /// regarded valid.</param>
        /// <returns>True if one of the submitted <paramref name="candidates"/>
        /// matches the evaluated value. If the <paramref name="candidates"/>
        /// parameter itself is null, too, the method returns false as well,
        /// which allows to check with null values, too.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="candidates"/>
        /// is a null reference.</exception>
        public static bool Is<T>(this T value, params T[] candidates)
        {
            if (candidates == null) return false;

            foreach (var t in candidates)
            {
                if (value.Equals(t)) return true;
            }

            return false;
        }

        #endregion

        #region match MouseEvent to PopupActivation

        /// <summary>
        /// Checks if a given <see cref="PopupActivationMode"/> is a match for
        /// an effectively pressed mouse button.
        /// </summary>
        public static bool IsMatch(this MouseEvent me, PopupActivationMode activationMode)
        {
            switch (activationMode)
            {
                case PopupActivationMode.LeftClick:
                    return me == MouseEvent.IconLeftMouseUp;
                case PopupActivationMode.RightClick:
                    return me == MouseEvent.IconRightMouseUp;
                case PopupActivationMode.LeftOrRightClick:
                    return me.Is(MouseEvent.IconLeftMouseUp, MouseEvent.IconRightMouseUp);
                case PopupActivationMode.LeftOrDoubleClick:
                    return me.Is(MouseEvent.IconLeftMouseUp, MouseEvent.IconDoubleClick);
                case PopupActivationMode.DoubleClick:
                    return me.Is(MouseEvent.IconDoubleClick);
                case PopupActivationMode.MiddleClick:
                    return me == MouseEvent.IconMiddleMouseUp;
                case PopupActivationMode.All:
                    //return true for everything except mouse movements
                    return me != MouseEvent.MouseMove;
                case PopupActivationMode.None:
                    return false;
                default:
                    throw new ArgumentOutOfRangeException("activationMode");
            }
        }

        #endregion

        #region execute command

        /// <summary>
        /// Executes a given command if its <see cref="ICommand.CanExecute"/> method
        /// indicates it can run.
        /// </summary>
        /// <param name="command">The command to be executed, or a null reference.</param>
        /// <param name="commandParameter">An optional parameter that is associated with
        /// the command.</param>
        /// <param name="target">The target element on which to raise the command.</param>
        public static void ExecuteIfEnabled(this ICommand command, object commandParameter, IInputElement target)
        {
            if (command == null) return;

            RoutedCommand rc = command as RoutedCommand;
            if (rc != null)
            {
                //routed commands work on a target
                if (rc.CanExecute(commandParameter, target)) rc.Execute(commandParameter, target);
            }
            else if (command.CanExecute(commandParameter))
            {
                command.Execute(commandParameter);
            }
        }

        #endregion

        /// <summary>
        /// Returns a dispatcher for multi-threaded scenarios
        /// </summary>
        /// <returns>Dispatcher</returns>
        internal static Dispatcher GetDispatcher(this DispatcherObject source)
        {
            //use the application's dispatcher by default
            if (Application.Current != null) return Application.Current.Dispatcher;

            //fallback for WinForms environments
            if (source.Dispatcher != null) return source.Dispatcher;

            // ultimately use the thread's dispatcher
            return Dispatcher.CurrentDispatcher;
        }


        /// <summary>
        /// Checks whether the <see cref="FrameworkElement.DataContextProperty"/>
        ///  is bound or not.
        /// </summary>
        /// <param name="element">The element to be checked.</param>
        /// <returns>True if the data context property is being managed by a
        /// binding expression.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="element"/>
        /// is a null reference.</exception>
        public static bool IsDataContextDataBound(this FrameworkElement element)
        {
            if (element == null) throw new ArgumentNullException("element");
            return element.GetBindingExpression(FrameworkElement.DataContextProperty) != null;
        }
    }
}