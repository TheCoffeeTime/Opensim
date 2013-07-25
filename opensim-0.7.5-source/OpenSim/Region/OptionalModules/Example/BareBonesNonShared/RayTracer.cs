﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using OpenSim.Region.Framework.Scenes;
using OpenSim.Region.Framework.Interfaces;
using System.Reflection;
using OpenMetaverse;
using System.Threading;
using Nini.Config;
using OpenSim.Framework;
using OpenMetaverse.Assets;
using System.Drawing;
using OpenSim.Region.OptionalModules.Example.BareBonesNonShared;

/// Author: Mihail Kovachev
/// Author: Mona Demaidi
/// Author: Thanakorn Tuanwachat
namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    /// <summary>
    /// Added By Mona
    /// Determine the refractive index of different PrimMaterials
    /// </summary>
    static class PrimMaterial
    {
        public const double AIR = 1;
        public const double WOOD = 4;
        public const double CEMENT = 1.8;
        public const double IRON = 14;
        
    }
    static class PrimType
    {
        public const int Box = 1;
        public const int Sphere = 2;
        public const int Cylinder = 3;
    }//PrimType
    public class RayTracer
    {
        //m_log: used for outputting message on standard output. 
        //m_prims of objects/entities in the world.
        //hits: Array of List reflections from 0 to MAX_REFLECTIONS. 
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Scene m_scene=null; 
        EntityBase[] m_prims;
        IScriptModuleComms m_commsMod;
        SceneObjectGroup transmitter; 
        SceneObjectGroup receiver;
        List<OneRayReflections>[] hits; 

        //Default class variables. These variables will be reset then the "restart ray stacer" button is pressed
        int MAX_REFLECTIONS = 5;
        float MAX_DISTANCE = 30;
        int ANGLE_BETWEEN_RAY = 1;
        static string runningFreqRT = "2.4";
        static string runningFreqRTUnit = "ghz";
        static double runningPowerRT = 44;
        static string runningPowerRTUnit = "dBw";
        static string RTModel;
        static string ANModel;
        double DEG_TO_RAD = Math.PI / 180.0;
        UUID scriptID;

        /**
         * Sets the transmitter to the prim with the name Tx and the 
         * receiver to the prim called Rx.
         * setOfVariables is a string that can be 'split'/broken down into many sub strings which will be used here
         * to initialse variables
         * 
         */
        public void Initialise(Scene scene, IConfigSource source, string setOfvariables, UUID _scriptID)
        {
            m_scene = scene;
            //This variable can be used to send message back to the script calling this function
            m_commsMod = m_scene.RequestModuleInterface<IScriptModuleComms>();
            //This script is used as a parameter to send a message back to the right object in the world. 
            scriptID = _scriptID;

            //Total number of initialise variables
            int noOfInitVar = initialiseVariables(setOfvariables);
            m_log.DebugFormat("[Ray Tracer Init]: total number of initialised variables = " + noOfInitVar.ToString());
           
            hits = new List<OneRayReflections>[MAX_REFLECTIONS + 1];
            
            int i;
            for (i = 0; i <= MAX_REFLECTIONS; i++)
            {
                hits[i] = new List<OneRayReflections>();
                hits[i].Clear();
            }
                
                m_log.DebugFormat("[BARE BONES NON SHARED]: INITIALIZED MODULE");

        }
        public void testPoint()
        {
            for (int i = 0; i < m_prims.Length; i++)
            {
                if (m_prims[i] is SceneObjectGroup && checkToken("reflectable", m_prims[i]))
                {
                    SceneObjectGroup sog = (SceneObjectGroup)m_prims[i];
                    if (sog.Parts.Length > 1)
                    {
                        foreach (SceneObjectPart sop in sog.Parts)
                        {
                            string name = sop.Name;
                            Vector3 midPoint = sop.AbsolutePosition;
                            Vector3 scale = sop.Scale;
                            m_log.DebugFormat("[name]:" + name);
                            m_log.DebugFormat("[midPoit]" + midPoint);
                            m_log.DebugFormat("[scale]:" + scale);
                        }
                    }//if
                    else
                    {
                    }//else
                    
                }//for
            }//for
        }//
        /// <summary>
        /// set RayTracer class varaibles according to the given set of variables. 
        /// This set of variables come from the in-world script from the remote control, when "restart raytracer" button 
        /// is pressed. 
        /// </summary>
        /// <param name="setOfVariables"></param>
        public int initialiseVariables(string setOfVariables)
        {
            //Each element will have a format of e.g. Frequency=10, FrequencyUnit=MHz etc.
            string[] arrayOfVariables = setOfVariables.Split('_');
            
            //Check if all the variable has been initialise by counting. 
            int countVariable = 0;
            //Foreach of those element the the variables
            foreach (string variableNameAndValue in arrayOfVariables)
            {
                string[] variableNameAndValueToken = variableNameAndValue.Split('=');
                //if variable has value attatch to it.
                if (variableNameAndValueToken.Length == 2)
                {
                    string variableName = variableNameAndValueToken[0];
                    string variableValue = variableNameAndValueToken[1];
                    switch (variableName)
                    {

                        case "Frequency":
                            runningFreqRT = variableValue;
                            countVariable++;
                            break;

                        case "FrequencyUnit":
                            runningFreqRTUnit = variableValue;
                            countVariable++;
                            break;

                        case "TransmitterPower":
                            runningPowerRT = Convert.ToDouble(variableValue);
                            countVariable++;
                            break;
                        case "TransmitterPowerUnit":
                            runningPowerRTUnit = variableValue;
                            countVariable++;
                            break;
                        case "Model":
                            RTModel = variableValue;
                            countVariable++;
                            break;
                        case "AngleBetweenRays":
                            ANGLE_BETWEEN_RAY = StringAndNumberConversion.getOnlyIntegerNumber(variableValue, ' ');
                            countVariable++; 
                            break;
                        case "AntennaType":
                            ANModel = variableValue;
                            countVariable++;
                            break;
                        case "MaxReflection":
                            MAX_REFLECTIONS = StringAndNumberConversion.getOnlyIntegerNumber(variableValue, ' ');
                            countVariable++;
                            break;
                        case "MaxDistance":
                            MAX_DISTANCE = StringAndNumberConversion.getOnlyIntegerNumber(variableValue, ' ');
                            countVariable++;
                            break;
                        default:
                            //This default is for debugging purpose. If this variable name did not match any of the existing variables name. 
                            string errorMsg = "Error: " + variableName + "did not match one of the variables in ray tracer";
                            m_commsMod.DispatchReply(scriptID, 1, "Error: " + errorMsg, "");
                            m_log.DebugFormat("[RAY TRACER INIT] " + variableName + " did not match one of the variables in ray tracer");
                            break;
                    }//switch
                }//if

                //This else is purely for the purpose of debugging purpose. It only happens when the string doesn't 
                //contain both variable and its value, ie, not in this formar -> var=10, frequency=20
                else

                {
                    string errorMsg = "Error: " + variableNameAndValueToken[0] + "has length != 2";
                    m_commsMod.DispatchReply(scriptID, 1, "Error: " + errorMsg, "");
                }//else
            }//foreach

            return countVariable;
        }//initialiseVariables
        //To start ray tracing from the transimitter to the receiver
        //To initialise m_prims variable. (entities in the world)
        public void PostInitialise()
        {
           // m_commsMod
           //   = m_scene.RequestModuleInterface<IScriptModuleComms>();
           // m_commsMod.OnScriptCommand += ProcessScriptCommand;
			try{
            m_prims = m_scene.GetEntities(); 
			}
			catch(Exception ex){
			}
            if (!findTransmitterAndReceiver())
                m_log.DebugFormat("[BARE BONES NON SHARED] Transmitter and/or receiver not Found!");
            else
            {
                m_log.DebugFormat("[BARE BONES NON SHARED] {0}!", receiver.RootPart.Scale);
                //rayTraceFromTransmitterBrutefoce();
                getRaysPath();
                //testPoint();
            }

        }
        /// <summary>
        /// Find if transmitter and receiver exist by checking all the objects in the world name. 
        /// Transmitter = Tx, Receiver = Ry.
        /// </summary>
        /// <returns></returns>
        bool findTransmitterAndReceiver()
        {
			try{
            EntityBase[] prims = m_scene.GetEntities();
			
            m_log.DebugFormat("[BARE BONES NON SHARED] Found {0} Entities!", prims.Length);
				
			//For each entities in the world, check if they are a transimitter Tx or a receiver Ry. 
            //Return true if both are true. 
            foreach (EntityBase prim in prims)
            {
                if (prim.Name.CompareTo("Tx") == 0)
                {
                    m_log.DebugFormat("[BARE BONES NON SHARED] Found the transmitter!");
                    transmitter = (SceneObjectGroup)prim;
                }
                if (prim.Name.CompareTo("Ry") == 0)
                {
                    m_log.DebugFormat("[BARE BONES NON SHARED] Found the Receiver!");
                    receiver = (SceneObjectGroup)prim;
                   
                }

            }
		}
			catch(Exception ex)
			{
				
			}
            return transmitter != null && receiver != null;
        }
        /// <summary>
        /// Delete all the object in the world whose name = Ray
        /// </summary>
        public void deleteRays()
        {
			try{
            EntityBase[] R_prims = m_scene.GetEntities();
            m_log.DebugFormat("[BARE BONES NON SHARED] Found {0} Entities!", R_prims.Length);

            //For each prim in the world, if their name is Rays, then delete them. 
            foreach (EntityBase prim in R_prims)
            {
                if (prim.Name.CompareTo("Rays") == 0)
                {
                    m_scene.DeleteSceneObject((SceneObjectGroup)prim, false);
                }
              
            }
            m_log.DebugFormat("[BARE BONES NON SHARED] Delete all Rays in scene");
			}
			catch(Exception ex)
			{
			}

        }
        public bool checkToken(string token, EntityBase prim)
        {

            //Get all tokens from the name
            string[] tokens = prim.Name.Split('_');
            //For each of the token if it matches then return true
            foreach (string t in tokens)
            {   //If matches, then return true
                if (t.CompareTo(token) == 0)
                    return true;
            }//foreach
            return false;
        }
        /// <summary>
        /// For every ray generated by the transmitter, trace it and check if it will be received by the transmitter 
        /// or not. For each ray, the number of maximum reflection is = MAX_REFLECTIONS. 
        /// </summary>
        void rayTraceFromTransmitterBrutefoce()
        {

            m_log.DebugFormat("[BARE BONES NON SHARED] Started Raytracing!");

            float lat, lon;
            List<OneRayReflections> rays = new List<OneRayReflections>();

            //For every lon and lat increase by ANGLE_BETWEEN_RAY
            for (lon = 0; lon < 360; lon += ANGLE_BETWEEN_RAY)
            {
                //m_log.DebugFormat ("[BARE BONES NON SHARED] Raytracing longtitude {0}!", lon);
                m_log.DebugFormat("[Ray Tracer] Raytracing {0}/360", lon);

                for (lat = 0; lat < 360; lat += ANGLE_BETWEEN_RAY)
                {
                    Vector3 dir = new Vector3();
                    double lonr = lon * DEG_TO_RAD;
                    double latr = lat * DEG_TO_RAD;

                    dir.X = (float)(Math.Cos(latr) * Math.Sin(lonr));
                    dir.Y = (float)(Math.Cos(latr) * Math.Cos(lonr));
                    dir.Z = (float)(Math.Sin(latr));
                    dir.Normalize();
                   
                    OneRayReflections reflections = new OneRayReflections(this, transmitter.AbsolutePosition, dir);
                    rays.Add(reflections);
                    //Thread worker = new Thread(reflections.followRayReflections);
                    //worker.Start();

                    reflections.followRayReflections();
                    //while (worker.IsAlive);
                }
            }//for
            //For all the rays those we have computed their reflections, if they hit the receiver, then add them to hits. 
            foreach (OneRayReflections reflection in rays)
            {
                if (reflection.reachesReceiver)
                {
                    //m_log.DebugFormat ("[BARE BONES NON SHARED] Found a hitting ray {0}, reflections {1}!", reflection.direction, reflection.path.Count - 2);

                    hits[reflection.path.Count - 2].Add(reflection);
                }//if

            }//foreach

            m_log.DebugFormat("[BARE BONES NON SHARED] Finnished Raytracing!");

        }
        public void drawRaysWithDifferentNumberOfReflections(int numRef)
        {
            foreach (OneRayReflections r in hits[numRef])
            {
                r.drawPath(numRef);
                //break;
            }
        }
        public void drawAllRays()
        {
            int y=0;
            for (y = 0; y < hits.Length; y++)
            {

                foreach (OneRayReflections r in hits[y])
                {
                    r.drawPath(y);
                    //break;
                }
            }
        }
        /// <summary>
        /// Get the angle of incidence cosine when the incidence vector given and the plane normal.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="normal"></param>
        /// <returns></returns>
        public static float getAngleOfIncidenceCos(Vector3 ray, Vector3 normal)
        {
            ray.Normalize();
            normal.Normalize();

            return Vector3.Dot(ray, normal);
        }
        /// <summary>
        /// Get a reflected ray from a given ray and the normal of the surface. 
        /// First calculate an angle different between normal and the surface. 
        /// Second, create a new ray deriving from rotate the normal using the angle found in part 1
        /// Third, negate that new ray as it is pointing to the opposite direction
        /// </summary>
        /// <param name="ray">in-coming ray</param>
        /// <param name="normal">normal of the surface</param>
        /// <returns></returns>
        public  Vector3 getReflectedRay(Vector3 ray, Vector3 normal)
        {
            Quaternion rotation = Vector3.RotationBetween(ray, normal);
            return Vector3.Negate(Vector3.Transform(normal, Matrix4.CreateFromQuaternion(rotation)));
        }
        /// <summary>
        /// Get 'all' possible path from the transmitter to the receiver
        /// </summary>
        public void getRaysPath()
        {
            EntityIntersection intersectPoint = null;
            Vector3 reflectedVector = new Vector3();
            for(int i = 0; i < m_prims.Length; i++)
            {
                if (m_prims[i] is SceneObjectGroup && checkToken("reflectable", m_prims[i]))
                {
                    SceneObjectGroup t = (SceneObjectGroup)m_prims[i];
                    int found = 0;
                       
                    // look at each part that makes the group
                    t.ForEachPart(delegate(SceneObjectPart part)
                    {
                        if(found == 5)
                        {
                            Vector3 position = part.AbsolutePosition;
                            Vector3 directionVector = position - transmitter.AbsolutePosition;
                            directionVector.Normalize();

                            Ray ray = new Ray(transmitter.AbsolutePosition, directionVector);
                            intersectPoint = part.TestIntersectionOBB(ray, part.ParentGroup.GroupRotation,true, false);
                            drawPlaneRay(transmitter.AbsolutePosition, intersectPoint.ipoint);
                            reflectedVector = getReflectedRay(directionVector, intersectPoint.normal);
                            found++;
                        }
                        else found++;
                    });//forEachPart
                }//for
            }//for

            Ray ray1 = new Ray(intersectPoint.ipoint, reflectedVector);
            for (int i = 0; i < m_prims.Length; i++)
            {
                if (m_prims[i] is SceneObjectGroup && checkToken("reflectable", m_prims[i]))
                {
                    SceneObjectGroup t = (SceneObjectGroup)m_prims[i];
                    // look at each part that makes the group
                    t.ForEachPart(delegate(SceneObjectPart part)
                    {

                        if (part.TestIntersectionOBB(ray1, part.ParentGroup.GroupRotation, true, false).HitTF)
                        {
                            Vector3 destination = part.TestIntersectionOBB(ray1, part.ParentGroup.GroupRotation, true, false).ipoint;
                            if (checkPointIntersectPrim(destination, part))
                              drawPlaneRay(intersectPoint.ipoint, destination);
                        }//if
                    });//forEachPart
                }//for
            }//for
        }//getRayPath
        /// <summary>
        /// Draw a ray (which has a shape of rectangle but with thin y and z value) from 2 given points. 
        /// The ray is drawn on the mid point between the 2 given points then rotate around y-axis and z-axis
        /// to make the ray connect between 2 points. For more information, please have a look at online TROVE Developer Documentations.
        /// By Thanakorn Tuanwachat 07/2013
        /// </summary>
        /// <param name="startPoint">The point where the ray start</param>
        /// <param name="endPoint">The point where the ray end</param>
        public void drawPlaneRay(Vector3 startPoint, Vector3 endPoint)
        {
            //Set up parameters for drawing an object in the world. Width = 0.2 unit and depth = 0.01 unit.
            //Length of the ray is the distance from the two given points

            float rayWidth = 0.05f; 
            float rayDepth = 0.05f;

            //Distance and Mid-point between 2 points. vectorFromStartToEnd can be used to represent a distance 
            //from x.end to x.start, y.end to y.start, z.end to z.start

            double rayLength = (startPoint - endPoint).Length();
            Vector3 midPoint = (startPoint + endPoint) / 2;
            Vector3 vectorFromStartToEnd = endPoint - startPoint;
            Vector3 xAxis = new Vector3(1, 0, 0);
            Vector3 yAxis = new Vector3(0, 1, 0);
            Vector3 zAxis = new Vector3(0, 0, 1);

            //Angles for rotating the objects (dervied from x, y, and z distances)
            //See developer documentations for more details

            Quaternion rotation = Vector3.RotationBetween(xAxis, vectorFromStartToEnd);

            //Start drawing the ray

            Vector3 dimension = new Vector3((float)rayLength, (float)rayWidth, (float)rayDepth);
            addObjectToTheWorld(null, midPoint, dimension, rotation, PrimType.Cylinder, startPoint);


        }
        /// <summary>
        /// Add a given object to the world. If you prefer argument to use default value, when pass it as null. 
        /// *There are 1 compulsory fields which is position of the object. The rest are optionals. 
        /// By Thanakorn Tuanwachat 07/2013
        /// </summary>
        /// <param name="name">Name of this object. Use default value "primative" if null is given</param>
        /// <param name="position">"Vector(x y z) where x, y, and z represent the coordinates in the world"</param>
        /// <param name="dimension">"Vector(x, y, z) where x, y, and z represent the size of the object"</param>
        /// <param name="rotations">double(x, y, z) where x, y, and z represent angle of rotation from its axis in radian</param>
        /// <param name="primType">There are only 3 types which are Box, </param>
        public void addObjectToTheWorld(string name, Vector3 position, Vector3 dimension, Quaternion rotation, int primType,
                                        Vector3 startPoint)
        {
            //Generate a new UUID for this ray

            UUID rayUUID = UUID.Random();

            //Place an object into the world. It can either be Box, Sphere, or Cylinder. according to the given PrimType

            SceneObjectGroup sog;
            switch (primType)
            {
                case PrimType.Box:
                    sog = new SceneObjectGroup(new UUID(rayUUID), position, OpenSim.Framework.PrimitiveBaseShape.CreateBox());
                    break;
                case PrimType.Sphere:
                    sog = new SceneObjectGroup(new UUID(rayUUID), position, OpenSim.Framework.PrimitiveBaseShape.CreateSphere());
                    break;
                case PrimType.Cylinder:
                    sog = new SceneObjectGroup(new UUID(rayUUID), position, OpenSim.Framework.PrimitiveBaseShape.CreateCylinder());
                    break;
                default:
                    sog = new SceneObjectGroup(new UUID(rayUUID), position, OpenSim.Framework.PrimitiveBaseShape.CreateBox());
                    break;
            }//switch

            //Check if it is possible to set thses parameters. If not then use default value

            if(dimension != null)
            {
              sog.RootPart.Scale = dimension;
            }
            if(rotation != null)
            {
                sog.RootPart.UpdateRotation(rotation);
            }//if
            if(name != null)
            {
                sog.Name = name;
            }//if
            else
            {
                sog.Name = "Primative";
            }//else
            //Add this object to the scene.
            m_scene.AddNewSceneObject(sog, false);
            sog.ScheduleGroupForFullUpdate();
            sog.HasGroupChanged = true;
        }
        /// <summary>
        /// Check if a given point is within the rectangle boundary of a prim. 
        /// Note: This method works best with retangle object. 
        /// </summary>
        /// <param name="point">A point to be checked</param>
        /// <param name="prim">An object in the world</param>
        /// <returns></returns>
        public bool checkPointIntersectPrim(Vector3 point, SceneObjectPart prim)
        {
            //Offset is used to determined boundary. Boundary = mid-point +- offset.
            //Precision = 0 means the point is absolutely within the boundary. 
            //Increase in precision = allowing more error for 
            //the point to intersect the prim further. i.e. don't make it too big. 

            float precision = 0.2f;
            Vector3 offset = (prim.Scale / 2) + new Vector3(precision, precision, precision);
            Vector3 primPosition = prim.AbsolutePosition;
            Vector3 maxBoundary = primPosition + offset;
            Vector3 minBoundary = primPosition - offset;

            //At the moment, if the prim has been rotated, the point of intersection will not hit the boundary,
            //so we need to rotate the point to map it to the original prim where no rotation has been done
            //Do this by create a vector from the prim position to the intersection point, rotate that vector, then 
            //recalculate the intersection point. 

            Quaternion rotation = prim.RotationOffset * -1;
            Vector3 rotationVector = point - primPosition;
            rotationVector = Vector3.Transform(rotationVector, Matrix4.CreateFromQuaternion(rotation));
            rotationVector += primPosition;

            //Check if the intersection is within the boundary.

            if ((rotationVector.X >= minBoundary.X && rotationVector.X <= maxBoundary.X) &&
                (rotationVector.Y >= minBoundary.Y && rotationVector.Y <= maxBoundary.Y) &&
                (rotationVector.Z >= minBoundary.Z && rotationVector.Z <= maxBoundary.Z))
            {
                return true;
            }//if
            else
                return false;
        }
        public class OneRayReflections
        {
            public Vector3 start;
            public Vector3 direction;
            public List<EntityIntersection> path;
            public bool reachesReceiver;
            RayTracer m_parent;
            public bool ready;
            private RayTracer rayTracer;
            //Add the caculations done
            double reflecCoeff=0;
            double refloss11=0;
            double transcoeff=0;   
            double refractionAngle = 0;  
            float distance = 0;
            double pathLoss = 0;
            double recievedPower = 0;
            double angle = 0;
            /// ///////////////////
            /// </summary>
            /// <param name="parent"></param>
            /// <param name="_start"></param>
            /// <param name="_direction"></param>

            public OneRayReflections(RayTracer parent, Vector3 _start, Vector3 _direction)
            {
                rayTracer = new RayTracer();
                m_parent = parent;
                direction = _direction;
                start = _start;
                ready = false;
            }

            public void drawLine(EntityIntersection fromPoint, EntityIntersection toPoint, int numRef)
            {
                Vector3 dir = toPoint.ipoint - fromPoint.ipoint;
                //Added By mona
               // angle =RadianToDegree( Math.Acos((double)(getAngleOfIncidenceCos(-dir, toPoint.normal))));
               // m_log.Info("[ANGLE]"+angle.ToString());
               // doCalculations(angle,fromPoint,toPoint);

                /////////////////
                // 5.0 is the speres used for a unit length
                // increase them to make them more dense
                int k = (int)Math.Ceiling(dir.Length() * 5.0);

                if (k < 2)
                    k = 2;

                // the distance between two spheres on the path
                Vector3 delta = dir / (k - 1);
                Vector3 pos = new Vector3(fromPoint.ipoint);
                int i;
                //Added by Mona//////
                //Building uuid for each object//
                string uuid="";
                int uuidpart1 = 11111111;
                int uuidpart2 = 1111;
                Int64 uuidpart3 = 111111111111;
                ////////////////////////////////
      
                UUID ncAssetUuid = UUID.Random();
                UUID ncItemUuid = UUID.Random();
                for (i = 0; i < k - 1; i++)
                {
                    uuid=uuidpart1.ToString()+"-"+uuidpart2.ToString()+"-"+uuidpart2.ToString()+"-"+uuidpart2.ToString()+"-"+uuidpart3.ToString();
                    pos = pos + delta;

                    SceneObjectGroup sog = new SceneObjectGroup(new UUID(uuid), pos, OpenSim.Framework.PrimitiveBaseShape.CreateSphere());
                  
                    sog.RootPart.Scale = new Vector3(0.1f, 0.1f, 0.1f);

                    SceneObjectPart part = new SceneObjectPart();

                    part=sog.RootPart;

                    sog.Name = "Rays";
                    //sog.SetRootPart(part);
                    part.Color = Color.FromArgb(255, 255, 0, 0);
                    part.Material = (byte)Material.Metal;


                    //doCalculations(angle, fromPoint, toPoint, part);
                    //Important to add scripts to prims on the client side//
                    //m_log.Info("Add touch script");
                    //Added by Mona///
                    //Call method to add touchScripts//
                    //addTouchScript(sog, part);
                    //m_log.Info("Touch script added.");
                    // tHIS SHOULD BE REMOVED !!!
                   
                    m_parent.m_scene.AddNewSceneObject(sog, false);
                    sog.ScheduleGroupForFullUpdate();
                    sog.HasGroupChanged = true;

                   // part.Inventory.CreateScriptInstance(taskItem, 0, false, m_parent.m_scene.DefaultScriptEngine, 0);
                    ////Added by Mona//////
                    uuidpart1++;
                    uuidpart2++;
                    uuidpart3++;
                    //////////////////////
                    
                }//for
            }//drawLine

            /////Added By Mona// This function add a touch script to each rezzed prim
            public void addTouchScript(SceneObjectGroup sog,SceneObjectPart part)
            {
                AssetBase asset = new AssetBase();
                asset.Name = "Default touch script";
                asset.Description = "Default touch script";
                asset.Type = 10; // 10 is the asset type for scripts.
                asset.FullID = UUID.Random();
                string data = angle + "_" + pathLoss;
                string partScript = "default\n{\n touch_start(integer i) \n {\n        llSay(0, \"Hiii\");\n    }\n}";
                //string partScript = "string data="+data+";\n" +"default\n{\n touch_start(integer i) \n {\n        llSay(0,data);\n    }\n}";
                asset.Data = Encoding.ASCII.GetBytes(partScript);
                m_parent.m_scene.AssetService.Store(asset);
               
                TaskInventoryItem taskItem = new TaskInventoryItem();

                taskItem.ResetIDs(part.UUID);
                taskItem.ParentID = part.UUID;
                taskItem.CreationDate = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
                taskItem.Name = "Default touch script";
                taskItem.Description = "Default touch script";
                taskItem.Type = 10;
                taskItem.InvType = 10;
                taskItem.OwnerID = UUID.Zero;
                taskItem.CreatorID = UUID.Zero;
                taskItem.BasePermissions = 2147483647; //0; // Not sure, let's try zeros.
                taskItem.CurrentPermissions = 2147483647; //0;
                taskItem.EveryonePermissions = 0;
                taskItem.GroupPermissions = 0;
                taskItem.NextPermissions = 532480; //0;
                taskItem.GroupID = UUID.Zero;
                taskItem.Flags = 0;
                taskItem.PermsGranter = UUID.Zero;
                taskItem.PermsMask = 0;
                taskItem.AssetID = asset.FullID;
                sog.ScheduleGroupForTerseUpdate();
                part.Inventory.AddInventoryItem(taskItem, false);
                m_parent.m_scene.AddNewSceneObject(sog, false);
                part.Inventory.CreateScriptInstance(taskItem, 0, false, m_parent.m_scene.DefaultScriptEngine, 0);
                
            }
            /// <summary>
            /// Added By Mona (Five functions) to compute the reflection and refraction coeff,the reflected power and the path loss
            /// The calculations carried Out depends on the PrimMaterial texture-As opensim don't gove any description of the texture attached to 
            /// a prim the prim PrimMaterial is determined in the prim name
            /// </summary>
            /// 

            ///////////////////////////////Reflected Power////////////////////////////////////
            double computeRefloss(double reflectionCoefficient)
            {
                double reflectionLoss = 0;
                reflectionLoss = -20 * Math.Log10(reflectionCoefficient);
                return reflectionLoss;
            }
            ///////////////////////////////Reflection Coefficient////////////////////////////////////
            double computeRefCoef(double refIndex1, double refIndex2, double incidentAngle, double transmitAngle)
            {
                double parallelRefCoef = 0;
                double perpendicularRefCoef = 0;
                parallelRefCoef = ((refIndex2 * Math.Cos(DegreeToRadian(incidentAngle)) - refIndex1 * Math.Cos(DegreeToRadian(transmitAngle))) / (refIndex2 * Math.Cos(DegreeToRadian(incidentAngle)) + refIndex1 * Math.Cos(DegreeToRadian(transmitAngle))));
                perpendicularRefCoef = ((refIndex1 * Math.Cos(DegreeToRadian(incidentAngle)) - refIndex2 * Math.Cos(DegreeToRadian(transmitAngle))) / (refIndex1 * Math.Cos(DegreeToRadian(incidentAngle)) + refIndex2 * Math.Cos(DegreeToRadian(transmitAngle))));
                double refCoef = Math.Sqrt(parallelRefCoef * parallelRefCoef + perpendicularRefCoef * perpendicularRefCoef);
                return refCoef;
            }
            ///////////////////////////////Transmission Coefficient////////////////////////////////////
            double computeTransCoef(double refIndex1, double refIndex2, double incidentAngle, double transmitAngle)
            {
                double parallelRefCoef = 0;
                double perpendicularRefCoef = 0;
                parallelRefCoef = 2 * (refIndex1 * Math.Cos(DegreeToRadian(incidentAngle)) / (refIndex2 * Math.Cos(DegreeToRadian(incidentAngle)) + refIndex1 * Math.Cos(DegreeToRadian(transmitAngle))));
                perpendicularRefCoef = 2 * (refIndex1 * Math.Cos(DegreeToRadian(incidentAngle)) / (refIndex1 * Math.Cos(DegreeToRadian(incidentAngle)) + refIndex2 * Math.Cos(DegreeToRadian(transmitAngle))));
                double refCoef = Math.Sqrt(parallelRefCoef * parallelRefCoef + perpendicularRefCoef * perpendicularRefCoef);
                return refCoef;
            }
            ////////////////////////////////////////////Degrees to Radians converter and Radian to degree converter/////////////////////////////////////////////////////////////////
            private double DegreeToRadian(double angle)
            {
                return Math.PI * angle / 180.0;
            }
            private double RadianToDegree(double angle)
            {
                return angle * (180.0 / Math.PI);
            }
            /// <summary>
            /// ////////////////////////////////////////////CalculatePathLoss////////////////////////////////////////////////////////////
            /// </summary>
            /// The path loss is calculated in dB where d is in meters and f in megahertz
            double calculatePathloss(double distance)
            {
                double path_loss = 0;
                // Any input frequency is converted to Mhz using the the converter ontology!!
                //the input frequency is in the following format ex 1000_hz
                //The ontolgy converts the frequency value to mhz
                string frequencyValue = runningFreqRT.Split('_')[0];
                string frequencyUnit = runningFreqRT.Split('_')[1];
                Conversion.Initialize();
                string from = Conversion.Get_Unit_Class1(frequencyValue, frequencyUnit);
                string to = Conversion.Get_Unit_Class1("1", "mhz");
                double result = Double.Parse(Conversion.Conversion_Between_units(from, to, frequencyValue).Split('^')[0]);
                path_loss = 20 * Math.Log10(distance) + 20 * Math.Log10(result) - 27.55;
                return path_loss;
            }
            /// <summary>
            /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
            double CalculatePathlossRays(double distance, double refcoeff,double pos)
            {
                double powercurr = Math.Pow(10, (runningPowerRT + 30) / 10);
                double path_loss = 0;
                string frequencyValue = runningFreqRT.Split('_')[0];
                string frequencyUnit = runningFreqRT.Split('_')[1];
                
                Conversion.Initialize();
                string from = Conversion.Get_Unit_Class1(frequencyValue, frequencyUnit);
                string to = Conversion.Get_Unit_Class1("1", "hz");
                double result = Double.Parse(Conversion.Conversion_Between_units(from, to, frequencyValue).Split('^')[0]);
                double wavelength = 3 * Math.Pow(10, 8) / result;
                path_loss = 10 * Math.Log10((powercurr * Math.Pow(refcoeff,2) *Math.Pow(wavelength,2)) / (16 *Math.Pow(Math.PI,2) * ( distance) * (distance)));
                return path_loss;



            }

            ////////////////////////////////CalculateRecievedPower//////////////////////////
            double Calculate_Recieved_Power()
            {

                double path_loss2 = Math.Pow(10, (pathLoss / 10));
                double power2 = Math.Pow(10, RayTracer.runningPowerRT / 10);
                recievedPower = 10 * Math.Log10(-path_loss2 + power2);
                return recievedPower;

            }
            /// ///////////////////////////////SnellsLaw////////////////////////////////////
            double snellsLaw(double incidentAngle, double refIndex1, double refIndex2)
            {
                double transmitAngle = 0;
                transmitAngle =RadianToDegree( Math.Asin(refIndex1 * Math.Sin(DegreeToRadian( incidentAngle)) / refIndex2) );
                return transmitAngle;
            }
            /// </summary>
            float TotalDistance = 0;
            public void drawPath(int numRef)
            {
                m_log.DebugFormat("[BARE BONES NON SHARED] drawing path!");

                int i = 0;
                EntityIntersection last = new EntityIntersection();
                
                foreach (EntityIntersection next in path)
                {
                    
                    m_log.DebugFormat("[BARE BONES NON SHARED]{0}->{1}, {2}!", i, next.obj.Name, next.obj.GroupID);
                    TotalDistance = 0;
                    if (i > 0)
                    {
                        drawLine(last, next, numRef);
                        // To figure out how the path loss of a ray can be calcuted after it hits a wall !!!
                        TotalDistance += Vector3.Distance(next.obj.AbsolutePosition, last.obj.AbsolutePosition);
                        
                    }
                    i++;
                    last = next;
                }
                m_log.DebugFormat("[BARE BONES NON SHARED] drawing path!");

            }
            //Added By Mona
            //Perform the calculations
            void doCalculations(double angle1,EntityIntersection fromPoint, EntityIntersection toPoint,SceneObjectPart part)
            {
                  //m_log.DebugFormat(fromPoint.obj.PrimMaterial.ToString());

                  reflecCoeff=computeRefCoef(PrimMaterial.AIR,PrimMaterial.WOOD,angle1,snellsLaw(angle1,PrimMaterial.AIR,PrimMaterial.WOOD)) ;
                  refloss11=computeRefloss( reflecCoeff);
                  transcoeff = computeTransCoef(PrimMaterial.AIR, PrimMaterial.WOOD, angle1, snellsLaw(angle1, PrimMaterial.AIR, PrimMaterial.WOOD));
                  m_log.DebugFormat("[reflecCoeff]"+reflecCoeff.ToString());
                  m_log.DebugFormat("[refloss11]" + refloss11.ToString());
                  m_log.DebugFormat("[transcoeff]" + transcoeff.ToString());
                  refractionAngle = snellsLaw(angle1, PrimMaterial.AIR, PrimMaterial.WOOD);
                  m_log.DebugFormat("[refractionAngle]" + refractionAngle.ToString());
                  //Check if the it is the first ray from  TX  
                  // If yes do the calculations
                  distance = Vector3.Distance(part.AbsolutePosition, fromPoint.obj.AbsolutePosition);
                  m_log.DebugFormat("[distance]" + distance.ToString());
                  if (fromPoint.obj.Name == "Tx")
                  {
                      pathLoss = CalculatePathlossRays(distance, reflecCoeff, 0);
                      
                      m_log.DebugFormat("[pathLoss]" + pathLoss.ToString());
                  }
                  else
                  {

                      pathLoss = CalculatePathlossRays(distance+TotalDistance, reflecCoeff, 0);
                      m_log.DebugFormat("[pathLoss]" + pathLoss.ToString());

                  }
                  m_log.DebugFormat("[RecivedPower]" + Calculate_Recieved_Power().ToString());
                  //CalculatePathlossRay

                  
            }

            //
            // Finds the intersection with the first prim the ray hits
            EntityIntersection findNextHit(Ray ray, SceneObjectPart last)
            {
                EntityIntersection closest = new EntityIntersection();
                closest.HitTF = false;
                ray.Direction.Normalize();
                //advance a bit to prevent rounding errors
                ray.Origin = ray.Origin + ray.Direction * 0.1f;

                int i;
                for (i = 0; i < m_parent.m_prims.Length; i++)
                {
                    //Check if the prim can reflect a ray 
                    //For any prim to reflect a ray, please include a "_reflectable" tag in it's name e.g. primName_reflectable. 
                    //Do this in the world object (in its property). 
                    if (m_parent.m_prims[i] is SceneObjectGroup && m_parent.checkToken("reflectable", m_parent.m_prims[i]))
                    {
                        SceneObjectGroup t = (SceneObjectGroup)m_parent.m_prims[i];
                       
                        // look at each part that makes the group
                        t.ForEachPart(delegate(SceneObjectPart part)
                        {
                            // it can not hit the same prim after reflection
                            if (!part.Equals(last))
                            {

                                EntityIntersection intersect = new EntityIntersection();
                                intersect.HitTF = false;

                                // decide to use a bounding sphere or bounding box
                                if (part.GetPrimType() == OpenSim.Region.Framework.Scenes.PrimType.SPHERE)
                                    intersect = part.TestIntersection(ray, part.ParentGroup.GroupRotation);
                             
                                else
                                {
                                    intersect = part.TestIntersectionOBB(ray, part.ParentGroup.GroupRotation,true, false);
                                }

                               // if (intersect.HitTF == true && (closest.HitTF == false || closest.distance > intersect.distance))
                                //If the ray intersect something and the angle of incidence is > 0 and its distance is better than all other intersection then...
                                if (intersect.HitTF == true && getAngleOfIncidenceCos(-ray.Direction,intersect.normal) >= 0 && (closest.HitTF == false || closest.distance >intersect.distance))
                                {
                                   // m_log.DebugFormat("[BARE BONES NON SHARED] We HIT {0}>>>>>{1}>>>>{2}!", intersect.ipoint.ToString(), part.AbsolutePosition.Z.ToString(), part.Scale.Z.ToString());
                                   // m_log.DebugFormat("[BARE BONES NON SHARED] We HIT {0}", intersect.obj.Name);
                                    if ((intersect.ipoint.Z <= (part.AbsolutePosition.Z + (part.Scale.Z / 2.0))) && (intersect.ipoint.Z >= (part.AbsolutePosition.Z - (part.Scale.Z / 2.0))))
                                        //(intersect.ipoint.X <= (part.AbsolutePosition.X + (part.Scale.X / 2.0)) + 0.1) && (intersect.ipoint.X >= (part.AbsolutePosition.X - (part.Scale.X / 2.0) + 0.1)) ||
                                        //(intersect.ipoint.Y <= (part.AbsolutePosition.Y + (part.Scale.Y / 2.0)) + 0.1) && (intersect.ipoint.Y >= (part.AbsolutePosition.Y - (part.Scale.Y / 2.0) + 0.1)))
                                    {
                                        //m_log.DebugFormat("[BARE BONES NON SHARED] We HIT {0}>>>>>{1}>>>>{2}!", intersect.ipoint.ToString(), part.AbsolutePosition.Z.ToString(), part.Scale.Z.ToString());
                                        closest = intersect;
                                        closest.obj = part;
                                    }
                                       // if (closest.obj.Name.CompareTo("SlideDoor") == 0 )
                                }//if
                            }//if
                        });

                    }//if
                }
                return closest;
            }//findNextHit

            /// <summary>
            /// Track this ray upto the MAX_REFLECTIONS, and keep the path (coordinates) of where this ray intersect.
            /// </summary>
            public void followRayReflections()
            {

                //path: An array of path. To keep track of where intecsection occurs for each reflection. 
                //distance: Total distance travel from transimitter to the receiver.
                //currentPos: The direction of where this ray is "heading". Note: The currentDirection will change then there is a reflection
                //hope: total number of reflection so far. 

                path = new List<EntityIntersection>();
                EntityIntersection currentPos = new EntityIntersection(start, new Vector3(0, 0, 0), true);
                currentPos.obj = m_parent.transmitter.RootPart;
                Vector3 currentDirection = direction;
                path.Add(currentPos); 
                double distance = 0;
                reachesReceiver = false;
                int hops = 0;
               
                while (distance < m_parent.MAX_DISTANCE && hops < m_parent.MAX_REFLECTIONS)
                {
                    hops++;

                    //A ray has origin and direction
                    Ray ray = new Ray(currentPos.ipoint, currentDirection);
                    
                    //Find an exact point where this ray will hitf a prim. Closest var stores attributes of where this intersection occurs. 
                    //EntityIntersection closest = findNextHit(ray, m_parent.transmitter.RootPart);
                    //It doesn't make sense to use m_parent.transmitter.RootPart as the 
                    
                    EntityIntersection closest = findNextHit(ray, currentPos.obj);

                    //If the ray hit something (a prim)
                    if (closest.HitTF)
                    {
                        //If it hit something, then add the intersection as the Ray's path. 
                        path.Add(closest);
                        distance += (closest.ipoint - currentPos.ipoint).Length();

                        //if the ray hits the receiver
                        if (closest.obj.ParentGroup.Equals(m_parent.receiver))
                        {
                            reachesReceiver = true;
                            break;
                        }//if

                        //Add this intersection to path
                        //Use a specific model to calculate a relfection
                        currentDirection = m_parent.getReflectedRay(currentDirection, closest.normal);
                        currentPos = closest;
                       
                    }//if it didn't hit something, then there is no point keep tracking it...
                    else break;
                }
                ready = true;
            }

            //Author: Thanakorn Tuanwachat
            /// <summary>
            /// Check if a given token matches one of the tokens in a given prim name
            /// </summary>

        }
    }//RayTracer
}//namespace
