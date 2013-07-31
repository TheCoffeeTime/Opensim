using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSim.Region.OptionalModules.Example.BareBonesNonShared;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    class RadioPower
    {
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

            //Any input frequency is converted to Mhz using the the converter ontology!!
            //the input frequency is in the following format ex 1000_hz
            //The ontolgy converts the frequency value to mhz

            Conversion.Initialize();
            string from = Conversion.Get_Unit_Class1(runningFreqRT, runningFreqRTUnit);
            string to = Conversion.Get_Unit_Class1("1", "mhz");
            double result = Double.Parse(Conversion.Conversion_Between_units(from, to, runningFreqRT).Split('^')[0]);
            path_loss = 20 * Math.Log10(distance) + 20 * Math.Log10(result) - 27.55;
            return path_loss;
        }
    }
}
