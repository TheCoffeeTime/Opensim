using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{

    public class RayWithPower
    {
        Ray ray;
        double maxPower;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="_ray"> A ray from starting point and its direction</param>
        /// <param name="_maxPower">The maximum transmittion power of the stating point</param>
        public RayWithPower(Ray _ray, double _maxPower) 
        {
            ray = _ray;
            maxPower = _maxPower;
        }//constructor

        public double getStrengthAt(Vector3 point) 
        {
            return 0; 
        }//getStrengthAt
    }//class

}//namespace
