//The side which decide where to place the screen on the object. 
integer screen_side = 2;

default
{  
    state_entry()
    {
      llSay(0, "Hello, Avatar!");    
      integer k =1;
      
      //Setting the screen properties. 
      k=      llSetPrimMediaParams(screen_side,
               //To start once everything is loaded. 
              [PRIM_MEDIA_AUTO_PLAY,TRUE,
               //Full screen
               PRIM_MEDIA_AUTO_SCALE,TRUE,
               //The URL of the radio simulator site. 
               PRIM_MEDIA_CURRENT_URL, "http://trove.cs.man.ac.uk/trove/inworld/sinewave.html",
               PRIM_MEDIA_AUTO_ZOOM,TRUE,
               //Standard web navigation controls.
               PRIM_MEDIA_CONTROLS,1]);  
               
      //Echo error message if the object doesn't belong to the land owner
      if ( llParcelMediaQuery([PARCEL_MEDIA_COMMAND_TEXTURE]) == [] )
              llSay(0, "Lacking permission to set/query parcel media. This object has to be owned by/deeded to the land owner.");
      
      llSay(0,(string)k);
      integer handle  = llListen( 2000, "", NULL_KEY, "" );
    }//state_entry
    
    listen(integer channel, string name, key id, string message)
    {
      llSay(0,"I heard:");
      llSay(0,message);
      //Used to listen to user input e.g. pressing button on the web etc. 
      llSetPrimMediaParams(screen_side,
               //To start once everything is loaded. 
              [PRIM_MEDIA_AUTO_PLAY,TRUE,
               //The URL of the radio simulator site. 
               PRIM_MEDIA_CURRENT_URL, "http://localhost/www/radioSpectrum.php?f="+message,
               PRIM_MEDIA_AUTO_ZOOM,TRUE,
               //Standard web navigation controls.
               PRIM_MEDIA_CONTROLS,1]); 
    }
   
    
}

state off // a second state besides "default"
{
    state_entry() // this is run as soon as the state is entered
    {
        llSay(0, "turning off!");
        llSetColor(<0.0, 0.0, 0.0>, ALL_SIDES); // sets all sides as dark as possible
       
    }
     
    touch_start(integer total_number)
    {
        state default;
    }
}
