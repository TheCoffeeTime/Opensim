using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    public class EssentialPaths
    {
        public static string getRTMainPath()
        {
            return "C:\\Users\\Thanakorn\\Documents\\wamp\\www\\www\\controlBoard\\";
        }

        public static string getModelListPath()
        {
            return getRTMainPath() + "RTmodels\\allModel.txt";
        }

        public static string getRTModelPath()
        {
            return getRTMainPath() + "RTmodels\\models\\";
        }

        public static string getProgressBarLogPath()
        {
            return getRTMainPath() + "RTLog\\progressLog.txt";
        }

        public static string getRayScriptPath()
        {
            return getRTMainPath() + "RTRayScript\\ray.txt";
        }

        public static string getConversionMainPath()
        {
            return "C:/Users/Thanakorn/Documents/GitHub/Opensim/opensim-0.7.5-source/bin/";
        }
    }
}
