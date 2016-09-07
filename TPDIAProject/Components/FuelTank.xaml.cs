using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TPDIAProject.Components
{
    public partial class FuelTank : UserControl, INotifyPropertyChanged
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

        #region Dependency properties



        public double CurrentValue
        {
            get { return (double)GetValue(CurrentValueProperty); }
            set { SetValue(CurrentValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentValueProperty =
            DependencyProperty.Register("CurrentValue", typeof(double), typeof(FuelTank), new PropertyMetadata(default(double), OnCurrentValueChanged));

        private async static void OnCurrentValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FuelTank tank;
            if (e.NewValue == DependencyProperty.UnsetValue)
            {
                return;
            }
            else if ((tank = d as FuelTank) != null)
            {
                tank.OldCurrentValueHeight = tank.CurrentValueHeight;
                tank.OldCurrentValueBlankHeight = tank.CurrentValueBlankHeight;

                var CurrentValueHeight = new GridLength((tank.CurrentValue / tank.MaxValue), GridUnitType.Star);
                var CurrentValueBlankHeight = new GridLength(1.0 - tank.CurrentValueHeight.Value, GridUnitType.Star);

                var minValue = tank.MinValue;
                var maxValue = tank.MaxValue;

                await Task.Run(() =>
                {
                    // UBER ANIMATION CODE.

                    var fps = 60;
                    int delay = 1000 / fps;

                    var diff = CurrentValueHeight.Value - (tank.OldCurrentValueHeight.IsAuto ? 0 : tank.OldCurrentValueHeight.Value);
                    diff /= 30.0;

                    var blankDiff = CurrentValueBlankHeight.Value - (tank.OldCurrentValueBlankHeight.IsAuto ? 0 : tank.OldCurrentValueBlankHeight.Value);
                    blankDiff /= 30.0;

                    bool direction = diff > 0;
                    bool blankDirection = blankDiff > 0;

                    for (int i = 0; i < 30; i++)
                    {
                        if ((direction && tank.CurrentValueHeight.Value < CurrentValueHeight.Value)
                        || (!direction && tank.CurrentValueHeight.Value > CurrentValueHeight.Value))
                        {
                            tank.CurrentValueHeight = new GridLength((tank.CurrentValueHeight.Value + diff), GridUnitType.Star);
                            if (tank.CurrentValueHeight.Value < minValue || tank.CurrentValueHeight.Value < 0)
                            {
                                tank.CurrentValueHeight = new GridLength(minValue, GridUnitType.Star);
                            }
                            else if (tank.CurrentValueHeight.Value > maxValue)
                            {
                                tank.CurrentValueHeight = new GridLength(maxValue, GridUnitType.Star);
                            }
                        }
                        if ((blankDirection && tank.CurrentValueBlankHeight.Value < CurrentValueBlankHeight.Value)
                        || (!blankDirection && tank.CurrentValueBlankHeight.Value > CurrentValueBlankHeight.Value))
                        {
                            tank.CurrentValueBlankHeight = new GridLength((tank.CurrentValueBlankHeight.Value + blankDiff), GridUnitType.Star);
                            if (tank.CurrentValueBlankHeight.Value < minValue || tank.CurrentValueBlankHeight.Value < 0)
                            {
                                tank.CurrentValueBlankHeight = new GridLength(minValue, GridUnitType.Star);
                            }
                            else if (tank.CurrentValueBlankHeight.Value > maxValue)
                            {
                                tank.CurrentValueBlankHeight = new GridLength(maxValue, GridUnitType.Star);
                            }
                        }
                        Thread.Sleep(delay);
                    }

                    tank.CurrentValueHeight = CurrentValueHeight;
                    tank.CurrentValueBlankHeight = CurrentValueBlankHeight;
                });
            }
        }

        public double MinValue
        {
            get { return (double)GetValue(MinValueProperty); }
            set { SetValue(MinValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinValueProperty =
            DependencyProperty.Register("MinValue", typeof(double), typeof(FuelTank), new PropertyMetadata(default(double)));




        public double MaxValue
        {
            get { return (double)GetValue(MaxValueProperty); }
            set { SetValue(MaxValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxValueProperty =
            DependencyProperty.Register("MaxValue", typeof(double), typeof(FuelTank), new PropertyMetadata(100.0));



        public double WarningLevel
        {
            get { return (double)GetValue(WarningLevelProperty); }
            set { SetValue(WarningLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for WarningLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WarningLevelProperty =
            DependencyProperty.Register("WarningLevel", typeof(double), typeof(FuelTank), new PropertyMetadata(default(double), UpdateGrid));

        public double CriticalLevel
        {
            get { return (double)GetValue(CriticalLevelProperty); }
            set { SetValue(CriticalLevelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CriticalLevel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CriticalLevelProperty =
            DependencyProperty.Register("CriticalLevel", typeof(double), typeof(FuelTank), new PropertyMetadata(default(double), UpdateGrid));

        private async static void UpdateGrid(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FuelTank tank;
            if (e.NewValue == DependencyProperty.UnsetValue)
            {
                return;
            }
            else if ((tank = d as FuelTank) != null)
            {
                tank.OldCriticalHeight = tank.CriticalHeight;
                tank.OldWarningHeight = tank.WarningHeight;
                tank.OldBlankHeight = tank.BlankHeight;


                var CriticalHeight = new GridLength((tank.CriticalLevel / tank.MaxValue), GridUnitType.Star);
                var WarningHeight = new GridLength((tank.WarningLevel / tank.MaxValue) - (tank.CriticalHeight.IsAuto ? 0 : tank.CriticalHeight.Value), GridUnitType.Star);
                var BlankHeight = new GridLength((1.0 - tank.CriticalHeight.Value - (tank.WarningHeight.IsAuto ? 0 : tank.WarningHeight.Value)), GridUnitType.Star);

                var minValue = tank.MinValue;
                var maxValue = tank.MaxValue;

                await Task.Run(() =>
                {
                    // UBER ANIMATION CODE.

                    if (tank.IsAnimating)
                    {
                        return;
                    }
                    tank.IsAnimating = true;
                    var fps = 60;
                    int delay = 1000 / fps;

                    var criticalDiff = CriticalHeight.Value - (tank.OldCriticalHeight.IsAuto ? 0 : tank.OldCriticalHeight.Value);
                    criticalDiff /= 30.0;

                    var warningDiff = WarningHeight.Value - (tank.OldWarningHeight.IsAuto ? 0 : tank.OldWarningHeight.Value);
                    warningDiff /= 30.0;

                    var blankDiff = BlankHeight.Value - (tank.OldBlankHeight.IsAuto ? 0 : tank.OldBlankHeight.Value);
                    blankDiff /= 30.0;

                    bool criticalDirection = criticalDiff > 0;
                    bool warningDirection = warningDiff > 0;
                    bool blankDirection = blankDiff > 0;

                    for (int i = 0; i < 30; i++)
                    {
                        if ((criticalDirection && tank.CriticalHeight.Value < CriticalHeight.Value)
                        || (!criticalDirection && tank.CriticalHeight.Value > CriticalHeight.Value))
                        {
                            tank.CriticalHeight = new GridLength((tank.CriticalHeight.Value + criticalDiff), GridUnitType.Star);
                            if (tank.CriticalHeight.Value < minValue || tank.CriticalHeight.Value < 0)
                            {
                                tank.CriticalHeight = new GridLength(minValue, GridUnitType.Star);
                            }
                            else if (tank.CriticalHeight.Value > maxValue)
                            {
                                tank.CriticalHeight = new GridLength(maxValue, GridUnitType.Star);
                            }
                        }
                        if ((warningDirection && tank.WarningHeight.Value < WarningHeight.Value)
                        || (!warningDirection && tank.WarningHeight.Value > WarningHeight.Value))
                        {
                            tank.WarningHeight = new GridLength((tank.WarningHeight.Value + warningDiff), GridUnitType.Star);
                            if (tank.WarningHeight.Value < minValue || tank.WarningHeight.Value < 0)
                            {
                                tank.WarningHeight = new GridLength(minValue, GridUnitType.Star);
                            }
                            else if (tank.WarningHeight.Value > maxValue)
                            {
                                tank.WarningHeight = new GridLength(maxValue, GridUnitType.Star);
                            }
                        }
                        if ((blankDirection && tank.BlankHeight.Value < BlankHeight.Value)
                        || (!blankDirection && tank.BlankHeight.Value > BlankHeight.Value))
                        {
                            tank.BlankHeight = new GridLength((tank.BlankHeight.Value + blankDiff), GridUnitType.Star);
                            if (tank.BlankHeight.Value < minValue || tank.BlankHeight.Value < 0)
                            {
                                tank.BlankHeight = new GridLength(minValue, GridUnitType.Star);
                            }
                            else if (tank.BlankHeight.Value > maxValue)
                            {
                                tank.BlankHeight = new GridLength(maxValue, GridUnitType.Star);
                            }
                        }
                        Thread.Sleep(delay);
                    }

                    tank.CriticalHeight = CriticalHeight;
                    tank.WarningHeight = WarningHeight;
                    tank.BlankHeight = BlankHeight;

                    tank.IsAnimating = false;
                });

            }
        }

        #endregion

        #region Private fields

        private GridLength _blankHeight;
        private GridLength _criticalHeight;
        private GridLength _warningHeight;
        private GridLength _currentValueHeight;
        private GridLength _currentValueBlankHeight;


        private GridLength _oldBlankHeight;
        private GridLength _oldCriticalHeight;
        private GridLength _oldWarningHeight;
        private GridLength _oldCurrentValueHeight;
        private GridLength _oldCurrentValueBlankHeight;
        #endregion

        #region Public properties

        public bool IsAnimating
        {
            get;set;
        }

        /// <summary>
        /// Blank section of the tank.
        /// </summary>
        public GridLength BlankHeight
        {
            get { return _blankHeight; }
            set { SetProperty(ref _blankHeight, value); }
        }

        /// <summary>
        /// Critical Height value.
        /// </summary>
        public GridLength CriticalHeight
        {
            get { return _criticalHeight; }
            set { SetProperty(ref _criticalHeight, value); }
        }

        /// <summary>
        /// Warning height value.
        /// </summary>
        public GridLength WarningHeight
        {
            get { return _warningHeight; }
            set { SetProperty(ref _warningHeight, value); }
        }

        /// <summary>
        /// Current value height.
        /// </summary>
        public GridLength CurrentValueHeight
        {
            get { return _currentValueHeight; }
            set { SetProperty(ref _currentValueHeight, value); }
        }

        /// <summary>
        /// Current value blank height.
        /// </summary>
        public GridLength CurrentValueBlankHeight
        {
            get { return _currentValueBlankHeight; }
            set { SetProperty(ref _currentValueBlankHeight, value); }
        }


        /// <summary>
        /// Blank section of the tank.
        /// </summary>
        public GridLength OldBlankHeight
        {
            get { return _oldBlankHeight; }
            set { SetProperty(ref _oldBlankHeight, value); }
        }

        /// <summary>
        /// Critical Height value.
        /// </summary>
        public GridLength OldCriticalHeight
        {
            get { return _oldCriticalHeight; }
            set { SetProperty(ref _oldCriticalHeight, value); }
        }

        /// <summary>
        /// Warning height value.
        /// </summary>
        public GridLength OldWarningHeight
        {
            get { return _oldWarningHeight; }
            set { SetProperty(ref _oldWarningHeight, value); }
        }

        /// <summary>
        /// Current value height.
        /// </summary>
        public GridLength OldCurrentValueHeight
        {
            get { return _oldCurrentValueHeight; }
            set { SetProperty(ref _oldCurrentValueHeight, value); }
        }

        /// <summary>
        /// Current value blank height.
        /// </summary>
        public GridLength OldCurrentValueBlankHeight
        {
            get { return _oldCurrentValueBlankHeight; }
            set { SetProperty(ref _oldCurrentValueBlankHeight, value); }
        }

        #endregion
        /// <summary>
        /// Interaction logic for FuelTank.xaml
        /// </summary>
        public FuelTank()
        {
            InitializeComponent();

            InnerControl.DataContext = this;

            OldBlankHeight = OldCriticalHeight = OldCurrentValueBlankHeight = OldCurrentValueHeight = OldWarningHeight = new GridLength(0.5, GridUnitType.Star);
        }
    }
}
