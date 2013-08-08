integer freqInfoBoardChannel = 2000;
integer boxGlowChannel       = 2001;
default
{
    state_entry()
    {

    }
    
    //trigger when this box is touched/clicked. 
    touch_start(integer num_detected)
    {
        //Make the object glow  
        llSetPrimitiveParams( [ PRIM_GLOW, ALL_SIDES, 0.5]);
        
        //This message is for the display screen
        llSay(freqInfoBoardChannel, "ELF");
        
        //Send message to other boxes to stop glowing
        llSay(boxGlowChannel, llGetKey());
        
        //Listen to other boxes message
        llListen( boxGlowChannel, "", NULL_KEY, "" );
      
    }//touch_start
    
    listen( integer channel, string name, key id, string message )
    {
        //If it comes from box glow channel
        if (channel = boxGlowChannel)
        {
            //If the message doesn't come from itself
            if(id != llGetKey())
            {
               llSetPrimitiveParams( [ PRIM_GLOW, ALL_SIDES, 0]); 
            }//if
        }//if
    }//listen
}//default