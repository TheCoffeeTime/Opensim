integer freqInfoBoardChannel = 2000;
default
{
    state_entry()
    {

    }
	
	//trigger when this box is touched/clicked. 
	touch_start(integer num_detected)
	{
	  llSay(freqInfoBoardChannel, "IN");
	}//touch_start
}