using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSim.Region.OptionalModules.Example.BareBonesNonShared;
using log4net;
using System.Reflection;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    [Serializable]
    class RadioPower
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Convert a given unit of watts into dBW
        /// </summary>
        /// <param name="value">Value in watts, kwatts, mwatts, or gwatts</param>
        /// <param name="unit">unit of the value. watts/kwatts/mwatts/gwatts</param>
        /// <returns>power in dBW</returns>
        public static double ConvertWTodBW(double value, string unit)
        {
            unit = unit.ToLower();
            double valueInWatt = value;
            switch (unit)
            {
                case "watts":
                    valueInWatt = value;
                    break;
                case "kwatts":
                    valueInWatt = value * 1000;
                    break;
                case "mwatts":
                    valueInWatt = value * 1000000;
                    break;
                case "gwatts":
                    valueInWatt = value * 1000000000;
                    break;
                default: break;
            }//switch

            return (10 * Math.Log10(valueInWatt));
        }
        /// <summary>
        /// Convert a given power in dBW into watts or equivalent
        /// </summary>
        /// <param name="value">power value in dBW</param>
        /// <param name="unit">return unit. watts/mwatts/kwatts/gwatts</param>
        /// <returns>power in watts or specified unit</returns>
        public static double ConvertdBWToW(double value, string unit)
        {
            unit = unit.ToLower();
            double valueInWatt = Math.Pow(10, (value / 10));
            switch (unit)
            {
                case "watts":
                    break;
                case "kwatts":
                    valueInWatt = valueInWatt / 1000;
                    break;
                case "mwatts":
                    valueInWatt = valueInWatt / 1000000;
                    break;
                case "gwatts":
                    valueInWatt = valueInWatt / 1000000000;
                    break;
                default: break;
            }//switch

            return valueInWatt;
        }
        /// <summary>
        /// Compute reflection loss in dB. 
        /// </summary>
        /// <param name="refIndex1">Refractive index for source medium</param>
        /// <param name="refIndex2">Refractive index for next medium</param>
        /// <param name="incidentAngle"></param>
        /// <param name="transmitAngle"></param>
        /// <author> Mona </author>
        /// <returns></returns>
        public static double ComputeRefloss(double refIndex1, double refIndex2, double incidentAngle, double transmitAngle)
        {
            //Compute reflection coefficient

            double parallelRefCoef = 0;
            double perpendicularRefCoef = 0;
            parallelRefCoef = ((refIndex2 * Math.Cos(incidentAngle) - refIndex1 * Math.Cos(transmitAngle)) / (refIndex2 * Math.Cos(incidentAngle) + refIndex1 * Math.Cos(transmitAngle)));
            perpendicularRefCoef = ((refIndex1 * Math.Cos(incidentAngle) - refIndex2 * Math.Cos(transmitAngle)) / (refIndex1 * Math.Cos(incidentAngle) + refIndex2 * Math.Cos(transmitAngle)));
            double refCoef = Math.Sqrt(parallelRefCoef * parallelRefCoef + perpendicularRefCoef * perpendicularRefCoef);

            //Compute reflection loss using reflection coefficient 

            double reflectionLoss = 0;
            reflectionLoss = -20 * Math.Log10(refCoef);
            return reflectionLoss;
        }
        /// <summary>
        /// The path loss is calculated in dB where d is in meters and f in megahertz
        /// This is a free space propagation model. 
        /// </summary>
        /// <auhor>Mona</auhor>
        public static double CalculatePathloss(double distance, string runningFreqRT, string runningFreqRTUnit)
        {

            double path_loss = 0;
            double frequencyInMHz = convertHzUnit(Double.Parse(runningFreqRT), runningFreqRTUnit, "mhz");
            path_loss = 20 * Math.Log10(distance) + 20 * Math.Log10(frequencyInMHz) - 27.55;
            return path_loss;
        }
        /// <summary>
        /// Convert any unit into any unit.
        /// Current supported unit are Hz, KHz, MHz, and GHz
        /// </summary>
        /// <param name="value">Current value for current unit</param>
        /// <param name="from">Current Unit</param>
        /// <param name="To">To Unit</param>
        /// <returns></returns>
        public static double convertHzUnit(double value, string from, string to)
        {
            
            double hzValue = value;
            from = from.ToLower();
            to = to.ToLower();

            double requiredValue = 0;

            //First convert a given unit into Hz
            switch (from)
            {
                case "hz":
                    hzValue = value;
                    break;
                case "khz":
                    hzValue = value * 1000;
                    break;
                case "mhz":
                    hzValue = value * 1000000;
                    break;
                case "ghz":
                    hzValue = value * 1000000000;
                    break;
            }//switch

            //Now convert the hz into the required unit
            switch (to)
            {
                case "hz":
                    requiredValue = hzValue;
                    break;
                case "khz":
                    requiredValue = hzValue / 1000;
                    break;
                case "mhz":
                    requiredValue = hzValue / 1000000;
                    break;
                case "ghz":
                    requiredValue = hzValue / 1000000000;
                    break;
            }

            return requiredValue;
        }
    }
}
