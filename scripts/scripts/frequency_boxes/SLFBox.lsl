integer freqInfoBoardChannel = 2000;
default
{
    state_entry()
    {	
		llListen( 0, "", NULL_KEY, "" );
    }
	
	//trigger when this box is touched/clicked. 
	touch_start(integer num_detected)
	{
	  llSay(freqInfoBoardChannel, "SLF");
	}//touch_start
}