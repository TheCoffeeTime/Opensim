integer dieChannel=898989;
integer TelChannel=8;
integer GrChannel=16;
integer SPlChannel=17;
integer delay=0;

init(string scriptUrl)
{
    
    
   string url = "http://trove.cs.man.ac.uk/trove/inworld/radiobuttons.html?url=" + scriptUrl;
   llSay(0,url);

    integer face = 4;
    llClearPrimMedia(face);
    llSetPrimMediaParams(face, [PRIM_MEDIA_CURRENT_URL, url, 
                                PRIM_MEDIA_AUTO_SCALE, TRUE, 
                                PRIM_MEDIA_AUTO_PLAY, TRUE, 
                                PRIM_MEDIA_PERMS_CONTROL, 
                                PRIM_MEDIA_PERM_NONE, 
                                PRIM_MEDIA_WIDTH_PIXELS, 512,
                                PRIM_MEDIA_AUTO_ZOOM,TRUE, 
                                PRIM_MEDIA_HEIGHT_PIXELS, 512, 
                                PRIM_MEDIA_FIRST_CLICK_INTERACT, 1]);
    integer face2 = 2;
    llClearPrimMedia(face2);
    llSetPrimMediaParams(face2, [PRIM_MEDIA_CURRENT_URL, url, 
                                 PRIM_MEDIA_AUTO_SCALE, TRUE, 
                                 PRIM_MEDIA_AUTO_PLAY, TRUE, 
                                 PRIM_MEDIA_PERMS_CONTROL, 
                                 PRIM_MEDIA_PERM_NONE, 
                                 PRIM_MEDIA_WIDTH_PIXELS, 512,
                                 PRIM_MEDIA_AUTO_ZOOM,TRUE, 
                                 PRIM_MEDIA_HEIGHT_PIXELS, 512, 
                                 PRIM_MEDIA_FIRST_CLICK_INTERACT, 1]);
    
}
    
key gRequestURL;
string gURL;
 
default
{
    state_entry()
    {
        llListen(dieChannel,"",NULL_KEY,"");
        gRequestURL = llRequestURL();    
    }
 
    //Triggered when this object receives an HTTP request
    http_request(key id, string method, string body) 
    {
      llSay(0, "Body: " + body);
	  llSay(0, "Method: " + method);
	  
	  //If it is what has just been requested (done in the state entry) 
      if (id == gRequestURL) 
      {
		//Convert into URL format, e.g. space become %20
		string arg = llEscapeURL(body);
		init(arg);
      }//if
      else 
      {
        // it's a message from the Shared Media!
        
        string msgType = llGetSubString(body, 0, 0);
        string data = llGetSubString(body, 1, -1);
        string data1=llGetSubString(body, 1, -1);
		
        if (msgType == "1") 
        {
          // it's a chat message!
          if(data=="telout")
          {    
            llSay(TelChannel,"");
            llSetTimerEvent(1);
            // llDie();
          } 
          else if(data=="telgr") 
          {
              llSay(GrChannel,"");
              llSetTimerEvent(1);
          }
          
          else if(data=="telsph")
          {
            llSay(SPlChannel,"");
            llSetTimerEvent(1);      
          }
          modSendCommand(data, "Greet|World", "");
          //llSay(data, data1);
          llHTTPResponse(id,200,"");
        }//if 
        else if (msgType == "2") 
        {
            // it's a time of data request
            float tod = llGetTimeOfDay( );
            string reply = "Time since last region restart or SL midnight (based on SL 4 hour day):";
            integer hours = ((integer)tod / 3600) ;
            integer minutes = ((integer)tod / 60) - (hours * 60);
            reply += (string) tod + " seconds which is "+(string) hours+"h "+(string) minutes+"m"; 
            llHTTPResponse(id,200, reply);
        }//else               
      }//else
    }//http_request
    
    listen(integer channel, string name, key id, string message)
    {
      if (message == "DIE")
      {
         llDie();
      }//if
    }//listen
    
    timer() 
    {
      delay++;
      //if(delay==4)
      //llDie();     
    }//timer
    
    // reset script when the object is rezzed
    on_rez(integer start_param)
    {
        llResetScript();
    }//on_rez
 
    changed(integer change)
    {
        // reset script when the owner or the inventory changed
        if (change & (CHANGED_OWNER | CHANGED_INVENTORY))
            llResetScript();
    }//changed
   
    
}
