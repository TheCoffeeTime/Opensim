using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenMetaverse;
using OpenSim.Region.OptionalModules.Example.BareBonesNonShared;
using OpenSim.Region.Framework.Scenes;
using log4net;
using System.Reflection;
using OpenSim.Region.Framework.Interfaces;

namespace OpenSim.Region.OptionalModules.Example.BareBonesNonShared
{
    public class RayTraceRequest
    {
        public UUID scriptID;
        public string command;
        public string input1;
        public string input2;

        public RayTraceRequest(UUID _scriptID, string _command, string _input1, string _input2)
        {
            scriptID = _scriptID;
            command = _command;
            input1 = _input1;
            input2 = _input2;
        }//constructure
    }//Request class

    public class RayTracerRequestHandler
    {
        //variables for debugging purposes
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IScriptModuleComms m_commsMod;
        Scene m_scene;


        static bool isBusy;                            //The lock to prevent other request using the resource.
        RayTracer currentModel;                        //Current ray tracer model

        public RayTracerRequestHandler(IScriptModuleComms _m_commsMod, Scene _m_scene)
        {
            isBusy = false;
            currentModel = new RayTracer();

            m_commsMod = _m_commsMod;
            m_scene = _m_scene;
        }//constructure

        /// <summary>
        /// Handle all requests for ray tracing including generating new ray trace model, drawing rays from a model, or
        /// delete ray. 
        /// </summary>
        /// <param name="newRequest">All the parameter send from modSendCommand()</param>
        public void handleARequest(RayTraceRequest newRequest)
        {
            //If the command is to generating a new ray tracer model
            if (newRequest.command == "RayTrace0To1" || newRequest.command == "RayTrace0ToMax")
            {
                currentModel.deleteRays();
                currentModel = new RayTracer();

                if (newRequest.command == "RayTrace0To1")
                {
                    currentModel.RayTrace(true, m_scene, newRequest.input1, newRequest.scriptID);
                }
                else
                {
                    currentModel.RayTrace(false, m_scene, newRequest.input1, newRequest.scriptID);
                }
            }//if

            //If the command is to select from the existed model (reading a model from a file)
            //Input1 in the newRequest must contains file name. 
            else if(newRequest.command == "RayTraceFromModel")
            {
                string model = System.IO.File.ReadAllText(@EssentialPaths.getRTModelPath() + newRequest.input1 + ".txt");
                currentModel.deleteRays();
                currentModel = new RayTracer();
                currentModel.RayTraceFromModel(model, m_scene, newRequest.scriptID);
            }
            
            //If the command is to get all ray tracer model name
            else if(newRequest.command == "GetAllModelsName")
            {
                string allModels = System.IO.File.ReadAllText(@EssentialPaths.getModelListPath());
                m_commsMod.DispatchReply(newRequest.scriptID, 1, "AllModelsName//" + allModels, "");
            }

            //Else, it is other ray trace commands such as drawing, deleting, calculating power. 
            else
            {
                switch (newRequest.command)
                {
                    case "DeleteRayTrace":
                        currentModel.deleteRays();
                        break;

                    case "IDtype0": 
                        currentModel.deleteRays();
                        currentModel.drawRayPath(0, 0);
                        break;

                    case "IDtype1": 
                        currentModel.deleteRays();
                        currentModel.drawRayPath(1, 0);
                        break;

                    case "IDtype2": 
                        currentModel.deleteRays();
                        currentModel.drawRayPath(2, 0);
                        break;

                    case "IDtype3": 
                        currentModel.deleteRays();
                        currentModel.drawRayPath(3, 0);
                        break;

                    case "IDtype4": 
                        currentModel.deleteRays();
                        currentModel.drawRayPath(4, 0);
                        break;

                    case "IDtype5": 
                        currentModel.deleteRays();
                        currentModel.drawRayPath(5, 0);
                        break;

                    case "DrawNextPath":
                    case "DrawPrevPath":
                        currentModel.deleteRays();
                        //Get number of reflection and element index
                        string[] tokens = newRequest.input1.Split('_');
                        currentModel.drawRayPath(int.Parse(tokens[0]), int.Parse(tokens[1]));
                        break;



                    //This method can only be called after the ray tracer model has been initialised and post initialised
                    //Input1 contains no of reflection, pathID, and rayID (keys), Input2 contains the position of where 
                    //the ray was pressed;
                    case "getStrengthAt": 
                        double signalStrength = currentModel.getStrengthAt(newRequest.input1, newRequest.input2);
                        m_log.DebugFormat("[strength = ]" + signalStrength.ToString());
                        m_commsMod.DispatchReply(newRequest.scriptID, 1, "Strength: " + signalStrength.ToString(), "");
                        break;
                    default: break;
                }//switch
                
            }//else
        }
    }//RayTracerRequestHandler class
}
