using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TPDIAProject.Models
{
    public class FuelTank : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// Raises property changed event.
        /// </summary>
        /// <param name="propertyName">Property name event arg.</param>
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        /// <summary>
        /// Handles setting property value and raising event.
        /// </summary>
        /// <typeparam name="T">Type of property</typeparam>
        /// <param name="field">Backing field reference.</param>
        /// <param name="value">Value to set.</param>
        /// <param name="callerName">Caller member name used when property changed event.</param>
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string callerName = "")
        {
            field = value;
            OnPropertyChanged(callerName);
        }
        #endregion


        #region Private fields
        private double _maxValue;
        private double _minValue;
        private double _currentValue;
        private double _warningValue;
        private double _criticalValue;
        #endregion

        #region Private properties
        private Timer UpdateTimer { get; set; }
        #endregion

        #region Public properties
        /// <summary>
        /// Max tank value.
        /// </summary>
        public double MaxValue
        {
            get { return _maxValue; }
            set { SetProperty(ref _maxValue, value); }
        }
        /// <summary>
        /// Min tank value.
        /// </summary>
        public double MinValue
        {
            get { return _minValue; }
            set { SetProperty(ref _minValue, value); }
        }
        /// <summary>
        /// Current tank level value.
        /// </summary>
        public double CurrentValue
        {
            get { return _currentValue; }
            set { SetProperty(ref _currentValue, value); }
        }
        /// <summary>
        /// Warning tank level value.
        /// </summary>
        public double WarningValue
        {
            get { return _warningValue; }
            set { SetProperty(ref _warningValue, value); }
        }
        /// <summary>
        /// Critical tank level value.
        /// </summary>
        public double CriticalValue
        {
            get { return _criticalValue; }
            set { SetProperty(ref _criticalValue, value); }
        }
        #endregion

        #region Constructors
        public FuelTank(double min, double max)
        {
            MinValue = min;
            MaxValue = max;

            UpdateTimer = new Timer(Update, null, new Random().Next(5000), 2000);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Updates values by random amount.
        /// </summary>
        /// <param name="state">null.</param>
        private void Update(object state)
        {
            double difference = MaxValue - MinValue;
            var newValueDifference = new Random((int)DateTime.Now.Ticks).NextDouble() * difference;
            CurrentValue = MinValue + newValueDifference;

            WarningValue = MinValue + (MaxValue / 2);
            WarningValue += new Random((int)DateTime.Now.Ticks).NextDouble() * (MaxValue / 3);

            CriticalValue = WarningValue / 2;
        }

        #endregion
    }
}
