using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenSim.Region.OptionalModules.Example.BareBonesNonShared;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    class Class1
    {
        static void Main(string[] args)
        {

            string freq = "2.4";
            System.Console.WriteLine(RadioPower.convertHzUnit(Double.Parse(freq), "ghz","hz"));
        }
    }
}
