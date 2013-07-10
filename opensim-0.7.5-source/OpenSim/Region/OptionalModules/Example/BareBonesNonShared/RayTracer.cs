using System;
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
/// Author: Mihail Kovachev
/// Authir: Mona Demaidi
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

    class RayTracer
    {

       
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        Scene m_scene=null;
        EntityBase[] m_prims;
        IScriptModuleComms m_commsMod;
        SceneObjectGroup transmitter;
        SceneObjectGroup receiver;
        int maxReflections = 5;
        float maxDistance = 30;
        List<OneRayReflections>[] hits;

        /**
         * Sets the transmitter to the prim with the name Tx and the 
         * receiver to the prim called Rx.
         * 
         */
        static string runningFreqRT="2.4_ghz";
        static double runningPowerRT=44;
        public void Initialise(Scene scene, IConfigSource source, string runningFreq)
        {
            runningFreqRT = runningFreq;
            m_scene = scene;
           
            hits = new List<OneRayReflections>[maxReflections + 1];
            
            int i;
            for (i = 0; i <= maxReflections; i++)
            {

                
                hits[i] = new List<OneRayReflections>();
                hits[i].Clear();
            }
                
                m_log.DebugFormat("[BARE BONES NON SHARED]: INITIALIZED MODULE");

        }
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
                rayTraceFromTransmitterBrutefoce();
            }

        }
        bool findTransmitterAndReceiver()
        {
			try{
            EntityBase[] prims = m_scene.GetEntities();
			
			
            m_log.DebugFormat("[BARE BONES NON SHARED] Found {0} Entities!", prims.Length);
				
			

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

        // shoot rays in all directions and follow the reflections

        //Delete all rays in scene
        public void deleteRays()
        {
			try{
            EntityBase[] R_prims = m_scene.GetEntities();
            m_log.DebugFormat("[BARE BONES NON SHARED] Found {0} Entities!", R_prims.Length);

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

        void rayTraceFromTransmitterBrutefoce()
        {

            m_log.DebugFormat("[BARE BONES NON SHARED] Started Raytracing!");

            float lat, lon;
            List<OneRayReflections> rays = new List<OneRayReflections>();

            for (lon = 0; lon < 360; lon += 20)
            {
                m_log.DebugFormat ("[BARE BONES NON SHARED] Raytracing longtitude {0}!", lon);

                for (lat = 0; lat < 360; lat += 20)
                {
                    Vector3 dir = new Vector3();
                    double DEG_TO_RAD = Math.PI / 180.0;
                    double lonr = lon * DEG_TO_RAD;
                    double latr = lat * DEG_TO_RAD;

                    dir.X = (float)(Math.Cos(latr) * Math.Sin(lonr));
                    dir.Y = (float)(Math.Cos(latr) * Math.Cos(lonr));
                    dir.Z = (float)(Math.Sin(latr));
                    dir.Normalize();
                   
                    OneRayReflections reflections = new OneRayReflections(this, transmitter.AbsolutePosition, dir);
                    rays.Add(reflections);

                    Thread worker = new Thread(reflections.followRayReflections);
                    worker.Start();
                    //while (worker.IsAlive);


                }

                // wait for all threads to finish
                foreach (OneRayReflections reflection in rays)
                {
                    //wait untill the current thread has finished
                    while (!reflection.ready)
                        Thread.Sleep(0);

                    if (reflection.reachesReceiver)
                    {
                        //m_log.DebugFormat ("[BARE BONES NON SHARED] Found a hitting ray {0}, reflections {1}!", reflection.direction, reflection.path.Count - 2);

                        hits[reflection.path.Count - 2].Add(reflection);
                        reflection.reachesReceiver = false;

                    }

                }
            }
            int i;
            for (i = 0; i < maxReflections; i++)
                m_log.DebugFormat("[BARE BONES NON SHARED] {0} reflections has #{1} hits!", i, hits[i].Count);

            //Mihail Code//
            // you can get an array of 2-reflections 
            // by calling hits[2].toArray(). 
            //int y=0;
            //for (y = 0; y < hits.Length; y++)
            //{

            //    foreach (OneRayReflections r in hits[y])
            //    {
            //         r.drawPath();
            //        //break;
            //    }
            //}

            m_log.DebugFormat("[BARE BONES NON SHARED] Finnished Raytracing!");

        }
        //Added By Mona///////
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


        /**
         * Get the angle of incidence cosine when the incidence vector given and the plane normal.
         * 
         */
        public static float getAngleOfIncidenceCos(Vector3 ray, Vector3 normal)
        {
            ray.Normalize();
            normal.Normalize();

            return Vector3.Dot(ray, normal);
        }
        
        /**
         * Given the vector of the ray of incidence and 
         * the normal of the plane hit finds the reflection vector.
         * 
         */

        public static Vector3 getReflectedRay(Vector3 ray, Vector3 normal)
        {

            Vector3 reflectedRay = new Vector3();

            // get the angle of incidence
            Vector3 backwardRay = -ray;

            float cosAlpha = getAngleOfIncidenceCos(backwardRay, normal);
            if (cosAlpha < 0)
            {
                m_log.DebugFormat("----------->This should not happen!!!!");
                normal = -normal;
                cosAlpha = getAngleOfIncidenceCos(-ray, normal);
                if (cosAlpha < 0)
                    m_log.DebugFormat("----------->This IS IMPOSSIBLE!!!!");

            }
            // resize the falling ray to make a right triangle with the normal
            float rayResized = normal.Length() / cosAlpha;
            float coefToResizeRay = ray.Length() / rayResized;
            ray = ray * coefToResizeRay;

            // get the angle vector
            Vector3 angleVector = ray + normal;

            // apply the angle vector from the normal and we should get the reflected ray
            reflectedRay = normal + angleVector;
            reflectedRay.Normalize();

            return reflectedRay;
        }

        // this class is easy threaded
        public class OneRayReflections
        {
            public Vector3 start;
            public Vector3 direction;
            public List<EntityIntersection> path;
            public bool reachesReceiver;
            RayTracer m_parent;
            public bool ready;
            private RayTracer rayTracer;
            private Vector3 vector3;
            private Vector3 dir;
            //Add the caculations done
            private double angleOfIncidence = 0;
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
               // doClaculations(angle,fromPoint,toPoint);

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
                   

                    //doClaculations(angle, fromPoint, toPoint,part);
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
                    
                    
                }


            }

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
            double calculatePathloss(double distance, string frequency)
            {
                double path_loss = 0;
                // Any input frequency is converted to Mhz using the the converter ontology!!
                //the input frequency is in the following format ex 1000_hz
                //The ontolgy converts the frequency value to mhz
                string frequencyValue = frequency.Split('_')[0];
                string frequencyUnit = frequency.Split('_')[1];
                Conversion.Initialize();
                string from = Conversion.Get_Unit_Class1(frequencyValue, frequencyUnit);
                string to = Conversion.Get_Unit_Class1("1", "mhz");
                double result = Double.Parse(Conversion.Conversion_Between_units(from, to, frequencyValue).Split('^')[0]);
                path_loss = 20 * Math.Log10(distance) + 20 * Math.Log10(result) - 27.55;
                return path_loss;
            }
            /// <summary>
            /// /////////////////////////////////////////////////////////////////////////////////////////////////////////
            double CalculatePathlossRays(double distance, string frequency,double refcoeff,double pos)
            {
                double powercurr = Math.Pow(10, (runningPowerRT + 30) / 10);
                double path_loss = 0;
                string frequencyValue = frequency.Split('_')[0];
                string frequencyUnit = frequency.Split('_')[1];
                
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
            void doClaculations(double angle1,EntityIntersection fromPoint, EntityIntersection toPoint,SceneObjectPart part)
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
                      pathLoss = CalculatePathlossRays(distance, RayTracer.runningFreqRT, reflecCoeff, 0);
                      
                      m_log.DebugFormat("[pathLoss]" + pathLoss.ToString());
                  }
                  else
                  {

                      pathLoss = CalculatePathlossRays(distance+TotalDistance, RayTracer.runningFreqRT, reflecCoeff, 0);
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
                    if (m_parent.m_prims[i] is SceneObjectGroup)
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
                                if (intersect.HitTF == true && getAngleOfIncidenceCos(-ray.Direction,intersect.normal) >= 0 && (closest.HitTF == false || closest.distance >intersect.distance))
                                {
                                   // m_log.DebugFormat("[BARE BONES NON SHARED] We HIT {0}>>>>>{1}>>>>{2}!", intersect.ipoint.ToString(), part.AbsolutePosition.Z.ToString(), part.Scale.Z.ToString());
                                   // m_log.DebugFormat("[BARE BONES NON SHARED] We HIT {0}", intersect.obj.Name);
                                    if ((intersect.ipoint.Z <= (part.AbsolutePosition.Z + (part.Scale.Z / 2.0))) && (intersect.ipoint.Z >= (part.AbsolutePosition.Z - (part.Scale.Z / 2.0))))
                                        //(intersect.ipoint.X <= (part.AbsolutePosition.X + (part.Scale.X / 2.0)) + 0.1) && (intersect.ipoint.X >= (part.AbsolutePosition.X - (part.Scale.X / 2.0) + 0.1)) ||
                                        //(intersect.ipoint.Y <= (part.AbsolutePosition.Y + (part.Scale.Y / 2.0)) + 0.1) && (intersect.ipoint.Y >= (part.AbsolutePosition.Y - (part.Scale.Y / 2.0) + 0.1)))
                                    {
                                        m_log.DebugFormat("[BARE BONES NON SHARED] We HIT {0}>>>>>{1}>>>>{2}!", intersect.ipoint.ToString(), part.AbsolutePosition.Z.ToString(), part.Scale.Z.ToString());
                                        closest = intersect;
                                        closest.obj = part;
                                    }
                                       // if (closest.obj.Name.CompareTo("SlideDoor") == 0 )
                                      // 

                                   
                                }
                            }
                        });

                    }
                }
                return closest;
            }

            public void followRayReflections()
            {
                //m_log.DebugFormat ("[BARE BONES NON SHARED] At least the thread starts!");
                //m_log.DebugFormat ("[BARE BONES NON SHARED] At least the thread {0}!", m_parent.m_prims.Length);

                path = new List<EntityIntersection>();

                EntityIntersection currentPos = new EntityIntersection(start, new Vector3(0, 0, 0), true);
                currentPos.obj = m_parent.transmitter.RootPart;
                Vector3 dir = direction;
                path.Add(currentPos);
                float distance = 0;
                reachesReceiver = false;
                int hops = 0;
               
                while (distance < m_parent.maxDistance && hops < m_parent.maxReflections)
                {
                    hops++;

                    Ray ray = new Ray(currentPos.ipoint, dir);
                    EntityIntersection closest = findNextHit(ray, m_parent.transmitter.RootPart);

                    if (closest.HitTF != false)
                    {
                       // m_log.DebugFormat ("[BARE BONES NON SHARED] We HIT {0}{1}{2}!", closest.obj.Name,closest.ipoint,hops.ToString());
                       // distance += GetDistanceTo(closest.ipoint , currentPos.ipoint);
                       //
                        distance += (closest.ipoint - currentPos.ipoint).Length();
                        path.Add(closest);
                        dir = getReflectedRay(dir, closest.normal);

                        if (closest.obj.ParentGroup.Equals(m_parent.receiver))
                        {
                            reachesReceiver = true;
                            break;
                        }
                        currentPos = closest;
                       
                    }
                    else break;
                }

                //	if(reachesReceiver)
                //		m_log.DebugFormat ("[BARE BONES NON SHARED] We HIT the receiver (party)(party)(party)!");

                ready = true;
            }
            public double GetDistanceTo(Vector3 a, Vector3 b)
            {
                float dx = a.X - b.X;
                float dy = a.Y - b.Y;
                float dz = a.Z - b.Z;
                return Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }

        };

    }
}
