// Grafitti board 0.0.2 for OpenSim
// By Justin Clark-Casey (justincc)
// http://justincc.wordpress.com
// This script is available under the BSD License

//Copy and edit by Thanakorn Tuanwachat 21/06/13


string text = "";

integer LISTENING_CHANNEL = 43;

// XXX Only putting this here as well to get around OpenSim's int -> string casting oddness
string LISTENING_CHANNEL_STRING = "43";

// FIXME: Should be dynamic!
integer CHARS_WIDTH = 42;

// Add some additional graffiti
addGraffiti(string message)
{
    while (llStringLength(message) > CHARS_WIDTH)
    {
        text += "\n\n" + llGetSubString(message, 0, CHARS_WIDTH - 1);
        message = llDeleteSubString(message, 0, CHARS_WIDTH - 1);
    }
    
    text += "\n\n" + message;
}

// Clear the existing graffiti and initialise stuff
clearGraffiti()
{
    text = "";
    
}

// Actually fires the graffiti out to the dynamic texture module
draw()
{
    //llSay(0, text);
    string drawList = "PenColour BLACK; MoveTo 40,220; FontSize 200; Text " + text + ";";
    drawList = osDrawFilledRectangle( CommandList, 200, 100 )

    osSetDynamicTextureData("", "vector", drawList, "1024", 0);
}

default
{
    state_entry()
    {      
        llListen(LISTENING_CHANNEL, "", NULL_KEY, "");      
        addGraffiti("Status message");
        draw();        
    }
    
    listen(integer channel, string name, key id, string message)
    {
        if (message == "!clear")
        {
            clearGraffiti();
        }
        else
        {
            addGraffiti(message);
        }
        
        draw();
    }
}
