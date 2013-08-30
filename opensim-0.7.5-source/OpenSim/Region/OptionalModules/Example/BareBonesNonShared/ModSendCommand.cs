/*
  Original Source http://opensimulator.org/wiki/OSSL_Script_Library/ModSendCommand
  Description below is written by Thanakorn Tuanwachat. 09/07/2013, oak.thanakorn@gmail.com
  This is a class used for passing data between a script and a region module with modSendCommand()
  
  If you want to edit some commands go directly to ProcessScriptCommand() method below. 
  I think that's why you are here in the first place ;-)
  
  Summary: 1. A script send message to the server using modSendCommand, 
           2. The server received the message here in the method ProcessScriptCommand() below
           3. If you want, you can send a message back to the script using  m_commsMod.DispatchReply()
           4. The script will receive that message in the link_message(...) method.
 * For more details, keep reading..

  1. To send a message from the world to the server (a region module), use modSendCommand. 
  To use: modSendCommand([command] [string1] [string2]) ---> place this in a script in the world NOT in here. 
  e.g.    modSendCommand("MYMOD", "This is a message from a script to the server", "this is also a message")
  [command] is to specify what command that script wants to do. You can add more command below, just follow the pattern. 
  [string1] and [string2] are just additional messages for you to use. You don't have to use them if you don't want to. 
  e.g. leaving them blank would be modSendCommand("command1", "", "") etc.
  The message sent from script will arrive in ProcessScriptCommand method below. 

  3. To send message from this class back to the script, use "m_commsMod.DispatchReply()" in the "ProcessScriptCommand" method
  To use: m_commsMod.DispatchReply(scriptID, 1, [your message], "")
  To be honest, I don't actually understand the whole interface but it works. Instead of [your message], just give whatever 
  string you want to pass to the client and leave other parameters as they are (or you can play around with them if you want)
  Note: This can only be done when a script in the world has sent a message to here. 

  4. After a message is sent from here, it will arrive in the script in the link message
 
  link_message(integer sender_num, integer num, string message, key id)
  {
       your code goes here... whatever you want to do with the server message.
  }
  
 */

using System;
using System.Reflection;
using Mono.Addins;
using Nini.Config;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;
using OpenMetaverse; //using OpenSim.Region.Framework.Scenes;
using log4net;
using System.Collections.Generic;
using OpenSim.Region.OptionalModules.Example.BareBonesNonShared;


//I comment this line out becasue it's a duplicate(a bit different but gives compile error) to  AssembleInfo.cs in the Properties
//[assembly: Addin("MyRegionModule", "0.1")]
[assembly: AddinDependency("OpenSim", "0.5")]

namespace ModSendCommandExample
{

    [Extension(Path = "/OpenSim/RegionModules", NodeName = "RegionModule")]
    public class MyRegionModule : INonSharedRegionModule
    {
        //For debugging purpose. Variables used for sneding message and debugging
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        IScriptModuleComms m_commsMod;

        //Other variables which need to be initialised here. 
        Scene m_scene;
        IConfigSource Source;

        RayTracerRequestHandler rayTracerRequestHandler;
       
        /**
         * Sets the transmitter to the prim with the name Tx and the 
         * receiver to the prim called Rx.
         * 
         */
        public string Name
        {
            get
            {
                return "MyRegionModule";
            }
        }

        public Type ReplaceableInterface
        {
            get
            {
                return null;
            }
        }

        public void Close() { }

        public void AddRegion(Scene scene)
        {
        }

        public void RemoveRegion(Scene scene) { }

        public void RegionLoaded(Scene scene)
        {
            m_scene = scene;
            m_commsMod = m_scene.RequestModuleInterface<IScriptModuleComms>();
            m_commsMod.OnScriptCommand += ProcessScriptCommand;
            rayTracerRequestHandler = new RayTracerRequestHandler(m_commsMod, m_scene);
        }

        public void Initialise(IConfigSource source) 
        {
            Source = source;
        }

        void ProcessScriptCommand(UUID scriptID, string reqId, string command, string input1, string input2)
        {
            // Example of how to send message back to the script as an acknowledgement
            //m_commsMod.DispatchReply(scriptID, 1, "Command received: " + module, "");
            //m_commsMod.DispatchReply(scriptID, 1, "String 1: " + input1, "");
            //m_commsMod.DispatchReply(scriptID, 1, "String 2: " + input2, "");

            //If command = mymod
            if (command == "MYMOD")
            {
                string[] tokens = input1.Split(new char[] { '_' }, StringSplitOptions.None);
                string value = tokens[0];
                string unit = tokens[1];
                Conversion.Initialize();
                string result = Conversion.workOnUnit(value, unit);
                result = result + "|";
                m_commsMod.DispatchReply(scriptID, 1, result.Split('|')[0], "");
            }
            else //Ray Tracer Command
            {
                RayTraceRequest request = new RayTraceRequest(scriptID, command, input1, input2);
                rayTracerRequestHandler.handleARequest(request);
            }

        }//ProcessScriptCommand
    }//class
}//name space