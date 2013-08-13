
//**** Permission is granted to use this script any way you like ****

vector dpos;

integer doorSte = 0;
default
{
    state_entry()
    {
        llOwnerSay("Sliding door script is active");
    }
 
    touch_start(integer total_number)
    {

        if (doorSte == 0)
        {

            dpos = llGetPos();

            dpos = dpos + <2,0,0>; //calculate door position + 2m in X
            llSetPos(dpos);         //moves the door to new position
           
            doorSte = 1;
        }
        else
        {
         
            dpos = llGetPos();
            dpos = dpos - <2,0,0>;//calculate door position - 2m in X
            llSetPos(dpos);         //moves the door to new position
           
            doorSte = 0;
        }
    }
}
