using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TPDIAProject.Models
{
    /// <summary>
    /// Class used for extracting data from files and providing next step values.
    /// </summary>
    public class Data
    {

        #region Constants

        /// <summary>
        /// Modifier used to scale temperature by month value extracted from datasets.
        /// </summary>
        private const double TEMPERATURE_BY_MONTHS_MODIFIER = 0.05;

        /// <summary>
        /// Modifier used to scale result to % range divided into 3 regions of priority.
        /// </summary>
        private const double SCALE_MODIFIER = 3.0;

        /// <summary>
        /// Minimum warning level modifier in order to keep calculated data in desired range.
        /// Data is rounded to this value if result is lower.
        /// </summary>
        private const double MINIMUM_WARNING_MODIFIER = 0.1;

        /// <summary>
        /// Maximum warning level modifier in order to prevent warning levels from exceeding desired range.
        /// Data is rounded to this value if result is higher.
        /// </summary>
        private const double MAXIMUM_WARNING_MODIFIER = 0.3;

        #endregion

        /// <summary>
        /// Deserialized Temperature by months data from file.
        /// </summary>
        public List<double> TemperatureByMonths { get; set; }

        /// <summary>
        /// Deserialized Fuel usage by hours data from file.
        /// </summary>
        public List<double> FuelUsageByHours { get; set; }

        /// <summary>
        /// Fuel tank measurements stream reference for data reading.
        /// </summary>
        public StreamReader FuelTankMeasurementsStreamReader { get; set; }

        /// <summary>
        /// Indicates whether FuelTankMeasurementsStreamReader is open or not.
        /// </summary>
        private bool IsStreamOpen { get; set; }
        
        /// <summary>
        /// Regex used to extract fuel tank id and measurement value from single entry. 
        /// <para>Returns data in a KeyValuePair object with Key being tank id.</para>
        /// </summary>
        private Regex FuelTankCSVRegex
        {
            get
            {
                return new Regex(@"(\d+),""?(\d+,?\d*)""?");
            }
        }

        /// <summary>
        /// Index of value currently extracted from data lists.
        /// </summary>
        private int CurrentValueIndex { get; set; }

        /// <summary>
        /// Previously extraced value.
        /// </summary>
        public double LastValue { get; set; }

        /// <summary>
        /// Base constructor, calls all data loading methods and sets CurrentValueIndex to 0.
        /// </summary>
        public Data()
        {
            LoadTemperature();
            LoadFuelUsage();
            OpenFuelTankMeasurementsStream();

            CurrentValueIndex = 0;
        }

        /// <summary>
        /// Attempts to open FuelTankMeasurements stream.
        /// <para>On success sets IsStreamOpen to "true"</para>
        /// </summary>
        private void OpenFuelTankMeasurementsStream()
        {
            var streamInfo = App.GetResourceStream(new Uri("Data/tankMeasures.csv", UriKind.Relative));
            if (streamInfo != null)
            {
                FuelTankMeasurementsStreamReader = new StreamReader(streamInfo.Stream);
                IsStreamOpen = true;
            }
        }

        /// <summary>
        /// Closes FuelTanksMeasurements stream and sets IsStreamOpen to "false".
        /// </summary>
        private void CloseFuelTanksMeasurementsStream()
        {
            if (IsStreamOpen)
            {
                FuelTankMeasurementsStreamReader.Close();
                FuelTankMeasurementsStreamReader.Dispose();
                IsStreamOpen = false;
            }
        }

        /// <summary>
        /// Loads temperature data from file. Stored data is available under TemperatureByMonths list.
        /// </summary>
        private void LoadTemperature()
        {
            TemperatureByMonths = new List<double>();
            var stream = TPDIAProject.App.GetResourceStream(new Uri("Data/temperatura_miesiacami.txt", UriKind.Relative));
            using (StreamReader sr = new StreamReader(stream.Stream))
            {
                while (!sr.EndOfStream)
                {
                    double val = 0.0;
                    if (double.TryParse(sr.ReadLine(), out val))
                    {
                        TemperatureByMonths.Add(val);
                    }
                }
            }
        }

        /// <summary>
        /// Loads Fuel usage data from file. Stored data is available under FuelUsageByHours list.
        /// </summary>
        private void LoadFuelUsage()
        {
            FuelUsageByHours = new List<double>();
            var stream = TPDIAProject.App.GetResourceStream(new Uri("Data/zuzycie_paliwa_godzinowo.txt", UriKind.Relative));
            using (StreamReader sr = new StreamReader(stream.Stream))
            {
                while (!sr.EndOfStream)
                {
                    double val = 0.0;
                    if (double.TryParse(sr.ReadLine(), out val))
                    {
                        FuelUsageByHours.Add(val);
                    }
                }
            }

            var min = FuelUsageByHours.Min();
            var max = FuelUsageByHours.Max();

            for (int i = 0; i < FuelUsageByHours.Count; i++)
            {
                FuelUsageByHours[i] = FuelUsageByHours[i] / max;
            }
        }

        /// <summary>
        /// Provides next value from datasets. Calculates next value in order for it to return 
        /// a value ranging from 0 to 30% (Used for warning level for fuel tanks).
        /// </summary>
        /// <returns>Calculated value.</returns>
        public double GetNextValue()
        {
            if (FuelUsageByHours == null || TemperatureByMonths == null)
            {
                return 0;
            }

            var result = FuelUsageByHours[CurrentValueIndex % FuelUsageByHours.Count] * (TEMPERATURE_BY_MONTHS_MODIFIER * TemperatureByMonths[CurrentValueIndex]);
            result *= SCALE_MODIFIER;

            if (result < MINIMUM_WARNING_MODIFIER)
            {
                result += MINIMUM_WARNING_MODIFIER;
            }
            else if (result > MAXIMUM_WARNING_MODIFIER)
            {
                result = MAXIMUM_WARNING_MODIFIER;
            }
            CurrentValueIndex = ++CurrentValueIndex >= TemperatureByMonths.Count ? 0 : CurrentValueIndex;

            LastValue = result;

            return result;
        }

        /// <summary>
        /// Finalizer used to close the file stream.
        /// </summary>
        ~Data()
        {
            CloseFuelTanksMeasurementsStream();
        }

        /// <summary>
        /// Method used to retrieve next measurement value from dataset.
        /// </summary>
        /// <returns>KeyValuePair with key being tank id and value being measurement value.</returns>
        /// <exception cref="InvalidDataException">Thrown when read lind from dataset cannot be parsed by regex,
        /// basically meaning that this particular line is incorrectly saved into file.</exception>
        public KeyValuePair<int, double> GetNextMeasurementValue()
        {
            if (!IsStreamOpen)
            {
                OpenFuelTankMeasurementsStream();
            }
            if (FuelTankMeasurementsStreamReader.EndOfStream)
            {
                CloseFuelTanksMeasurementsStream();
                OpenFuelTankMeasurementsStream();
            }
            var dataLine = FuelTankMeasurementsStreamReader.ReadLine();
            var values = FuelTankCSVRegex.Match(dataLine);
            if (values.Success)
            {
                int fuelTankId = int.Parse(values.Groups[1].Value);
                double fuelTankMeasurement = double.Parse(values.Groups[2].Value.Replace(',', '.'));
                return new KeyValuePair<int, double>(fuelTankId, fuelTankMeasurement);
            }

            throw new InvalidDataException("Data in csv file did not meet requiriments: REGEX - @\"(\\d+),\"?(\\d+,?\\d*)\"?\")");
        }
    }
}
