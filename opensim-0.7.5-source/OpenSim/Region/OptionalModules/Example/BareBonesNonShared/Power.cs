using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    class Power
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

    }
}
