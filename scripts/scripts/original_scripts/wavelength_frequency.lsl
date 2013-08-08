default
{
    
    
    
    state_entry()
    {
    llSay(0, "Hello, Avatar!");    
    integer k =1;
     k=  llSetPrimMediaParams(2,
           [PRIM_MEDIA_CURRENT_URL, "http://trove.cs.man.ac.uk/trove/inworld/sinewave.html",
            PRIM_MEDIA_AUTO_ZOOM,TRUE,
            PRIM_MEDIA_CONTROLS,FALSE]);
    if ( llParcelMediaQuery([PARCEL_MEDIA_COMMAND_TEXTURE]) == [] )
            llSay(0, "Lacking permission to set/query parcel media. This object has to be owned by/deeded to the land owner.");
    llParcelMediaCommandList( [
            PARCEL_MEDIA_COMMAND_UNLOAD,
            PARCEL_MEDIA_COMMAND_TEXTURE, (key) llGetTexture(0) ] );
    llParcelMediaCommandList( [
            PARCEL_MEDIA_COMMAND_URL, "http://trove.cs.man.ac.uk/trove/inworld/sinewave.html",
            PARCEL_MEDIA_COMMAND_TYPE,"text/html",
            PARCEL_MEDIA_COMMAND_TEXTURE, (key) llGetTexture(0) ] );
    llSay(0,(string)k);
        integer handle  = llListen( 1000, "", NULL_KEY, "" );
    }
   listen(integer channel, string name, key id, string message)
    {
       llSay(0,"I heard:");
       llSay(0,message);

  
    llParcelMediaCommandList( [
            PARCEL_MEDIA_COMMAND_URL, "http://trove.cs.man.ac.uk/trove/inworld/sinewave.html?f="+message,
            PARCEL_MEDIA_COMMAND_TYPE,"text/html",
            PARCEL_MEDIA_COMMAND_TEXTURE, (key) llGetTexture(0) ] );
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
