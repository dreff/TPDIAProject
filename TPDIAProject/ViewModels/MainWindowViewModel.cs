using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using TPDIAProject.Models;

namespace TPDIAProject.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
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

        #region Constants

        /// <summary>
        /// Value for maximum animation speed.
        /// </summary>
        public const int MAX_SPEED = 300;

        /// <summary>
        /// Value for minimum animation speed.
        /// </summary>
        public const int MIN_SPEED = 1;

        /// <summary>
        /// Animation idle time, in milliseconds. After this period animation timers start.
        /// </summary>
        public const int ANIMATION_DUE_TIME = 1000;

        /// <summary>
        /// Animation interval offset. It is multiplied by speed variable.
        /// </summary>
        public const int ANIMATION_INTERVAL_OFFSET = 10;

        /// <summary>
        /// Animation counter starting value, used for initialization of fuel tanks levels.
        /// </summary>
        public const int ANIMATION_COUNTER_STARTING_VALUE = UPDATE_LEVELS_HIT_COUNT - 5;

        /// <summary>
        /// Fuel tanks levels update count. Each animation tick raises a counter 
        /// and once it's reached this hit count fuel tanks levels are updated.
        /// </summary>
        public const int UPDATE_LEVELS_HIT_COUNT = 100;

        /// <summary>
        /// Critical level treshold used to evaluate critical level difference ratio.
        /// </summary>
        public const double CRITICAL_LEVEL_THRESHOLD = 0.15;

        /// <summary>
        /// One tenth fraction.
        /// </summary>
        public const double ONE_TENTH = 1.0 / 10.0;

        /// <summary>
        /// Three tenths fraction.
        /// </summary>
        public const double THREE_TENTHS = 3.0 / 10.0;
        #endregion

        #region Private fields

        /// <summary>
        /// Collection of Fuel Tanks.
        /// </summary>
        private List<FuelTank> _fuelTanks;
        
        /// <summary>
        /// Data extraction manager class.
        /// </summary>
        private Data _data;

        /// <summary>
        /// Current speed variable.
        /// </summary>
        private int _speed;
        #endregion

        #region Private properties

        /// <summary>
        /// Animation timer responsible for reading data and updating the visuals.
        /// </summary>
        private Timer AnimationTimer { get; set; }

        /// <summary>
        /// Animation counter used to determine when to update fuel tanks' levels values.
        /// </summary>
        private int AnimationCounter { get; set; }

        /// <summary>
        /// Dispatcher from view used to update ObservableCollections.
        /// </summary>
        private Dispatcher UIDispatcher { get; set; }
        #endregion

        #region Public properties

        /// <summary>
        /// Collection of Fuel Tanks property.
        /// </summary>
        public List<FuelTank> FuelTanks
        {
            get { return _fuelTanks; }
            set { SetProperty(ref _fuelTanks, value); }
        }

        /// <summary>
        /// Collection of log entries, shows all activity since application launch.
        /// </summary>
        public ObservableCollection<LogEntry> LogActivity { get; set; }

        /// <summary>
        /// Data extraction manager property.
        /// </summary>
        public Data Data
        {
            get { return _data; }
            set { SetProperty(ref _data, value); }
        }

        /// <summary>
        /// Maximum speed property (bindable)
        /// </summary>
        public int MaxSpeed
        {
            get { return MAX_SPEED; }
        }

        /// <summary>
        /// Minimum speed property (bindable)
        /// </summary>
        public int MinSpeed
        {
            get { return MIN_SPEED; }
        }

        /// <summary>
        /// Speed property, updates timer on each change.
        /// </summary>
        public int Speed
        {
            get { return _speed; }
            set
            {
                SetProperty(ref _speed, value);
                if (AnimationTimer == null)
                {
                    AnimationTimer = new Timer(Animate, null, ANIMATION_DUE_TIME, (MaxSpeed - 20 - _speed + MinSpeed) * ANIMATION_INTERVAL_OFFSET);
                }
                else
                {
                    AnimationTimer.Change(0, (MaxSpeed - _speed + MinSpeed) * ANIMATION_INTERVAL_OFFSET);
                }
            }
        }
        #endregion

        #region Constructors

        /// <summary>
        /// Base constructor.
        /// </summary>
        /// <param name="uiDispatcher">Dispatcher reference from view to enable ObservableCollection editing.</param>
        public MainWindowViewModel(Dispatcher uiDispatcher)
        {
            FuelTanks = new List<FuelTank>()
            {
                new FuelTank("1", 0, 10000),
                new FuelTank("2", 0, 20000),
                new FuelTank("3", 0, 30000),
                new FuelTank("4", 0, 40000),
            };

            UIDispatcher = uiDispatcher;

            foreach (var tank in FuelTanks)
            {
                tank.CriticalValue = tank.MaxValue * ONE_TENTH;
                tank.WarningValue = tank.MaxValue * THREE_TENTHS;

                tank.WarningReached += HandleWarningReached;
                tank.CriticalReached += HandleCriticalReached;
            }

            Data = new Data();
            Speed = 200;
            AnimationCounter = ANIMATION_COUNTER_STARTING_VALUE;
            LogActivity = new ObservableCollection<LogEntry>();


        }
        #endregion

        #region Methods

        /// <summary>
        /// Animation timer callback method, extracts data and animates it.
        /// </summary>
        /// <param name="state">null.</param>
        private void Animate(object state)
        {
            ++AnimationCounter;
            if (AnimationCounter == UPDATE_LEVELS_HIT_COUNT)
            {
                AnimationCounter = 0;
                int sign;
                if (Data.LastValue >= CRITICAL_LEVEL_THRESHOLD)
                {
                    sign = 1;
                }
                else
                {
                    sign = -1;
                }
                var value = Data.GetNextValue();
                foreach (var tank in FuelTanks)
                {
                    tank.CriticalValue = tank.CriticalValue + (sign * value);
                    tank.WarningValue = tank.CriticalValue + (value * tank.MaxValue);
                }
            }

            var measurementValue = Data.GetNextMeasurementValue();
            if (measurementValue.Key - 1 < FuelTanks.Count)
            {
                FuelTanks[measurementValue.Key - 1].SetCurrentValue(measurementValue.Value);
            }
        }

        /// <summary>
        /// Handles WarningReached event from FuelTanks. Adds a log entry to LogActivity.
        /// </summary>
        /// <param name="sender">Sender.</param>
        private void HandleWarningReached(FuelTank sender)
        {
            UIDispatcher.Invoke(() =>
            {
                LogActivity.Add(new LogEntry(Colors.Yellow, $"{DateTime.Now.ToLocalTime()}: Poziom w zbiorniku {sender.Name}. spadł do poziomu zagrożenia, potrzebna dostawa."));
            });
        }

        /// <summary>
        /// Handles CriticalReached event from FuelTanks. Adds a log entry to LogActivity.
        /// </summary>
        /// <param name="sender"></param>
        private void HandleCriticalReached(FuelTank sender)
        {
            UIDispatcher.Invoke(() =>
            {
                LogActivity.Add(new LogEntry(Colors.Red, $"{DateTime.Now.ToLocalTime()}: Poziom w zbiorniku {sender.Name} spadł do poziomu krytycznego, potrzebna pilna dostawa."));
            });
        }
        #endregion
    }
}
