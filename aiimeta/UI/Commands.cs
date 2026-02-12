using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace aiimeta.UI
{
    /// <summary>Provides several app-specific <see cref="RoutedUICommand"/> instances.</summary>
    public static class Commands
    {
        /// <summary>Copy command. This is an alias of <see cref="ApplicationCommands.Copy"/>.</summary>
        public static RoutedUICommand Copy { get; } = ApplicationCommands.Copy;

        /// <summary>Copy Row command.</summary>
        public static RoutedUICommand CopyRow { get; } =
            new RoutedUICommand("Copy _Row", "CopyRow", typeof(Commands),
                new InputGestureCollection(new[]
                {
                    new KeyGesture(Key.C, ModifierKeys.Control | ModifierKeys.Shift),
                    new KeyGesture(Key.Insert, ModifierKeys.Control | ModifierKeys.Shift),
                }));
    }
}
