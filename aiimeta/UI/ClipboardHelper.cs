using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace aiimeta.UI
{
    /// <summary>Provides several helper methods for use with Clipboard operations.</summary>
    public static class ClipboardHelper
    {
        /// <summary>Reads a C WSTR data from a MemoryStream.</summary>
        /// <param name="obj"><see cref="string"/> or <see cref="MemoryStream"/> instance.</param>
        /// <exception cref="InvalidCastException"><paramref name="obj"/> is neither <see cref="string"/> nor <see cref="MemoryStream"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null.</exception>
        /// <remarks>
        /// This method is intended for use like <c>e.Data.GetData("format")?.AsString()</c>
        /// in a <see cref="System.Windows.UIElement.Drop"/> event hander
        /// for a C WSTR data format that the WPF doesn't automatically convert.
        /// When WPF auto-convert the data to string, this method returns it.
        /// (C WSTR data is a UTF-16LE data stream with a terminating '\0' (U+0000).)
        /// </remarks>
        /// <remarks>
        /// If <paramref name="obj"/> is <see cref="MemoryStream"/>, 
        /// this method invokes its <see cref="IDisposable.Dispose"/> 
        /// as a side effect.
        /// </remarks>
        public static string AsString(this object obj)
        {
            if (obj is string str) return str;
            var m = (MemoryStream)obj ?? throw new ArgumentNullException(nameof(obj));
            using var r = new StreamReader(m, Encoding.Unicode);
            var s = r.ReadToEnd();
            var p = s.IndexOf('\0');
            if (p >= 0) s = s.Substring(0, p);
            return s;
        }
    }
}
