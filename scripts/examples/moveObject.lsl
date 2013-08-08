default
{
//  when the script has been saved (only in default) or when re-entering this state
    state_entry()
    {
        // Some scripters prefer to use a 'Mnemonic Constant' for channel zero
        // However, Mnemonic constants are much more useful and are recommended for less obvious values like CHANGED_OWNER instead of remembering the value 128
        // Here is an example. 'PUBLIC_CHANNEL' has the value 0. 
        llSay(PUBLIC_CHANNEL, "Hello, Avatar!");
    }
 
//  when someone starts touching the prim
    touch_start(integer num_detected)
    {
        vector oldPosition = llGetPos();
 
        llSetPos(oldPosition + <0.0, 0.0, 1.0>);
    }
}