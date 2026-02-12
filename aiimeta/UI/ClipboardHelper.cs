using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace aiimeta.UI
{
    /// <summary>Provides several helper methods for use with Clipboard operations.</summary>
    public static class ClipboardHelper
    {
        /// <summary>Reads a C WSTR data (UTF-16LE text terminated by a '\0') from a MemoryStream.</summary>
        /// <param name="obj"><see cref="MemoryStream"/> instance.</param>
        /// <exception cref="InvalidCastException"><paramref name="obj"/> is not a <see cref="MemoryStream"/>.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="obj"/> is null.</exception>
        /// <remarks>
        /// This method is intended for use like <c>e.Data.GetData("format")?.AsString()</c>
        /// in a <see cref="System.Windows.UIElement.Drop"/> event hander
        /// for a WSTR data format that the WPF doesn't know.
        /// As a side effect, this method invokes <see cref="IDisposable.Dispose"/> 
        /// for <paramref name="obj"/>.
        /// </remarks>
        public static string AsString(this object obj)
        {
            var m = (MemoryStream)obj ?? throw new ArgumentNullException(nameof(obj));
            using var r = new StreamReader(m, Encoding.Unicode);
            var s = r.ReadToEnd();
            var p = s.IndexOf('\0');
            if (p >= 0) s = s.Substring(0, p);
            return s;
        }
    }
}
