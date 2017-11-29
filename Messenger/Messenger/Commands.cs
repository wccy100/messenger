﻿using Messenger.Models;
using Messenger.Modules;
using Mikodev.Logger;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;

namespace Messenger
{
    internal static class Commands
    {
        public static RoutedUICommand CopyText { get; } = new RoutedUICommand() { Text = "复制消息内容" };

        public static RoutedUICommand Remove { get; } = new RoutedUICommand() { Text = "移除这条消息" };

        public static RoutedUICommand ViewImage { get; } = new RoutedUICommand() { Text = "在图片查看器中查看" };

        static Commands()
        {
            var cpy = new CommandBinding { Command = CopyText };
            cpy.CanExecute += _CopyCanExecute;
            cpy.Executed += _CopyExecuted;

            var rmv = new CommandBinding { Command = Remove };
            rmv.CanExecute += _RemoveCanExecute;
            rmv.Executed += _RemoveExecuted;

            var vie = new CommandBinding { Command = ViewImage };
            vie.CanExecute += _LargeImageCanExecute;
            vie.Executed += _LargeImageExecuted;

            Application.Current.MainWindow.CommandBindings.Add(cpy);
            Application.Current.MainWindow.CommandBindings.Add(rmv);
            Application.Current.MainWindow.CommandBindings.Add(vie);
        }

        private static void _LargeImageCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var msg = (e.OriginalSource as FrameworkElement)?.DataContext as Packet;
            if (msg is null || msg.Path != "image")
                e.CanExecute = false;
            else
                e.CanExecute = true;
            e.Handled = true;
        }

        private static void _LargeImageExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var msg = (e.OriginalSource as FrameworkElement)?.DataContext as Packet;
            if (msg is null)
                return;
            var str = msg.Value as string;
            if (str == null)
                return;
            try
            {
                var flp = CacheModule.GetPath(str);
                Process.Start(flp);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        private static void _RemoveCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var msg = (e.OriginalSource as FrameworkElement)?.DataContext as Packet;
            if (msg is null)
                e.CanExecute = false;
            else
                e.CanExecute = true;
            e.Handled = true;
        }

        private static void _RemoveExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var msg = (e.OriginalSource as FrameworkElement)?.DataContext as Packet;
            if (msg is null)
                return;
            HistoryModule.Remove(msg);
            e.Handled = true;
        }

        private static void _CopyCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var val = (e.OriginalSource as FrameworkElement)?.DataContext as Packet;
            if (val == null || val.Path != "text")
                e.CanExecute = false;
            else
                e.CanExecute = true;
            e.Handled = true;
        }

        private static void _CopyExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            var msg = (e.OriginalSource as FrameworkElement)?.DataContext as Packet;
            if (msg?.MessageText is null)
                return;
            try
            {
                Clipboard.SetText(msg.MessageText);
            }
            catch (Exception ex)
            {
                Log.Error(ex);
                Entrance.ShowError("复制消息出错", ex);
            }
            e.Handled = true;
        }
    }
}
