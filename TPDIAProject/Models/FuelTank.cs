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

        /// <summary>
        /// Max value backing field.
        /// </summary>
        private double _maxValue;

        /// <summary>
        /// Min value backing field.
        /// </summary>
        private double _minValue;

        /// <summary>
        /// Current value backing field.
        /// </summary>
        private double _currentValue;

        /// <summary>
        /// Warning value backing field.
        /// </summary>
        private double _warningValue;

        /// <summary>
        /// Critical value backing field.
        /// </summary>
        private double _criticalValue;

        /// <summary>
        /// Name backing field.
        /// </summary>
        private string _name;

        /// <summary>
        /// Warning level reached flag backing field.
        /// </summary>
        private bool _warningLevelReached;

        /// <summary>
        /// Critical level reached flag backing field.
        /// </summary>
        private bool _criticalLevelReached;
        #endregion

        #region Private properties
        /// <summary>
        /// Timer responsible for updating levels with randomly generated values (for testing purposes)
        /// <para>--Not used anymore, left for reference--</para>
        /// </summary>
        private Timer UpdateTimer { get; set; }

        /// <summary>
        /// Flag indicating whether warning level has been reached.
        /// <para>Raises WarningReached on first "true" value (locally).</para>
        /// </summary>
        private bool WarningLevelReached
        {
            get { return _warningLevelReached; }
            set
            {
                var previousValue = _warningLevelReached;
                _warningLevelReached = value;

                if (!previousValue && _warningLevelReached)
                {
                    RaiseWarningReached();
                }
            }
        }

        /// <summary>
        /// Flag indicating whether critical level has been reached.
        /// <para>Raises CriticalReached on first "true" value (locally)</para>
        /// </summary>
        private bool CriticalLevelReached
        {
            get { return _criticalLevelReached; }
            set
            {
                var previousValue = _criticalLevelReached;
                _criticalLevelReached = value;
                if (!previousValue && _criticalLevelReached)
                {
                    RaiseCriticalReached();
                }
            }
        }
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
            set
            {
                SetProperty(ref _currentValue, value);
                WarningLevelReached = CurrentValue <= WarningValue;
                CriticalLevelReached = CurrentValue <= CriticalValue;
            }
        }

        /// <summary>
        /// Name of the FuelTank, modifiable in runtime (although no current implementation supports it)
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
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

        #region Events

        /// <summary>
        /// Event raised when warning level has been reached.
        /// </summary>
        public event Action<FuelTank> WarningReached;

        /// <summary>
        /// Event raised when critical level has been reached.
        /// </summary>
        public event Action<FuelTank> CriticalReached;

        /// <summary>
        /// Raises the WarningReached event.
        /// </summary>
        protected void RaiseWarningReached()
        {
            WarningReached?.Invoke(this);
        }

        /// <summary>
        /// Raises the CriticalReached event.
        /// </summary>
        protected void RaiseCriticalReached()
        {
            CriticalReached?.Invoke(this);
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Base constructor initializing name, max value and min value.
        /// </summary>
        /// <param name="name">Name of the fuel tank.</param>
        /// <param name="min">Minimal value of the fuel tank.</param>
        /// <param name="max">Maximum value of the fuel tank.</param>
        public FuelTank(string name, double min, double max)
        {
            Name = name;
            MinValue = min;
            MaxValue = max;

            //UpdateTimer = new Timer(Update, null, new Random().Next(5000), 2000);
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

        /// <summary>
        /// Sets current value of the fuel tank (testing if there is any difference between this method call and calling setter)
        /// <para>Seems like there's no difference. Left for reference.</para>
        /// </summary>
        /// <param name="value">New value.</param>
        internal void SetCurrentValue(double value)
        {
            CurrentValue = value;
        }

        #endregion
    }
}
