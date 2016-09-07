using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
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

        #region Private fields
        private List<FuelTank> _fuelTanks;
        #endregion

        #region Public properties
        public List<FuelTank> FuelTanks
        {
            get { return _fuelTanks; }
            set { SetProperty(ref _fuelTanks, value); }
        }
        #endregion

        #region Constructors
        public MainWindowViewModel()
        {
            FuelTanks = new List<FuelTank>()
            {
                new FuelTank(0, 100),
                new FuelTank(0, 500),
                new FuelTank(0, 1300),
                new FuelTank(0, 1582),
                new FuelTank(0, 5000),
                new FuelTank(0, 18200)
            };
        }
        #endregion

        #region Methods

        #endregion
    }
}
