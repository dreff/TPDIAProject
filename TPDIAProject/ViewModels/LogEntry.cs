using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace TPDIAProject.ViewModels
{
    /// <summary>
    /// Class representing single log entry, it consists of a background color 
    /// and text to be displayed.
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Background of the log entry.
        /// </summary>
        public SolidColorBrush Background { get; set; }

        /// <summary>
        /// Text message of the log entry.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Constructor initializing background color and text properties.
        /// </summary>
        /// <param name="color">Bacground color.</param>
        /// <param name="text">Text message.</param>
        public LogEntry(Color color, string text)
        {
            Background = new SolidColorBrush(color);
            Text = text;
        }
    }
}
