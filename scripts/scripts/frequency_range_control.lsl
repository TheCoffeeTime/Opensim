// This code needs tidying - makes too many assumptions


// Touch counter. Use as a condition to set the on/off flag (using modulus)
integer touch_num=1;
// On/Off flag. Indicating whether the whole "app" is on or off. 
integer flag=0;
integer WavelengthCh;
integer HANDLE;
float value=0;


// NOTE: This function is not used in the script, at the moment.
// Returns a new string with spaces removed from the beginning of source.
string StringLeftTrim(string source)
{
    //The current position of a pointer to a specific char in a given string (source)
    integer x = 0;
    string str = llGetSubString(source, x, x);
    //While char = space
    while (str == " ")
    {
        str = llGetSubString(source, x, x);
        ++x;    
    }
    return llGetSubString(source, x, llStringLength(source));
}

// NOTE: This function is not used in the script, at the moment.
// Returns a new string with spaces removed from the end of source.
string StringRightTrim(string source)
{
    //The current position of a pointer to a specific char in a given string (source)
    integer x = -1;    
    string str = llGetSubString(source, x, x);
    //While char = space
    while (str == " ")
    {
        --x;        
        str = llGetSubString(source, x, x);
    }
    return llGetSubString(source, 0, llStringLength(source) + x);
}

//This function returns TRUE or FALSE. It checks if a given char is a number 0-9 or not
integer isNumber(string char)
{
  if(char == "0" || char == "1" || char == "2" || char == "3" || char == "4" ||
     char == "5" || char == "6" || char == "7" || char == "8" || char == "9")  
    return TRUE;
  else
    return FALSE;
}//IsNumber


// This function returns TRUE or FALSE depending on if the input string consists entirely of numbers.
// It will return TRUE if the entire string is an integer number (whole number).
// It will return FALSE if the entire string is not an integer. Floating points, letters, symbols, and spaces will make it return FALSE.
integer StringIsNum(string input) {
    integer length = llStringLength(input);  integer i;  integer verify;
 
    for(i = 0; i < length; i++) {
        string char = llGetSubString(input, i, i);
 
        //Compare each character in the string. For long strings, this may take time, and consume memory.
        if(isNumber(char) == TRUE || char == ".")
          //Do something. You can do whatever you like here, the variable is just a placeholder to keep the compiler happy.
          verify = TRUE;
 
        //Exit with FALSE at the first sign of a non numerical character
        else return FALSE;
    }
 
    //Exit with TRUE if all characters are a number
    return TRUE;
} // end StringIsNum

// This function checks if the given unit is Hz. It will return TRUE if the given string is in hz and FALSE otherwise.
integer UnitIsValid(string unit)
{
    // This part can be changed in order to check for any other unit.
    if(unit == "hz" || unit == "khz" || unit == "mhz" || unit == "ghz" || unit == "ehz" || unit == "phz")
        return TRUE;
    else
        return FALSE;
} // end UnitIsValid

// Convert the appended unit input into a useful one.
//*********FIX THIS_------------------------------------------------------------------------------------------------------------------------
integer InputHasAnAppendedHz(string input)
{
    // Length of the given string.
    integer len = llStringLength(input);
    // Boolean to check the status of 3rd char from th end.
    integer thirdIsANum = FALSE;
    // String to store the numerical value of the given input.
    string integerValue;
    // Copy of the input for modification
    string inputcopy = llToLower(input);
    
    //Last item = length -1
    integer lastChar = len - 1;
    integer secondLastChar = len - 2;
    integer thirdLastChar = len - 3;
    
    string thirdChar = (string) llGetSubString(input, thirdLastChar, thirdLastChar);
    if(llGetSubString(inputcopy, lastChar, lastChar) != "z")
        return FALSE;
    if(llGetSubString(inputcopy, secondLastChar, secondLastChar) != "h")
        return FALSE;
    // Check if its 3rd char is an SI prefix.
    if(thirdChar != "k" && thirdChar != "m" && thirdChar != "g"
                        && thirdChar != "t" && thirdChar != "p" 
                        && thirdChar != "e")
    {
        // If its not, check if its a valid integer.
        if(StringIsNum(thirdChar) == FALSE)
            return FALSE;
        else
            thirdIsANum = TRUE;
    } // end if
    
    // Construct a numerical value to check in order to check if its an integer.
    if(thirdIsANum == TRUE)
        integerValue = llGetSubString(input, -len, -3);
    else  
        integerValue = llGetSubString(input, -len, -4);
    
    if(StringIsNum(integerValue) == FALSE)
        return FALSE;
    else
        return TRUE;    
} // end InputHasAnAppendedHz

// Function to change the format of input from e.g. 1000hz to 1000_hz
string MakeAppenededHzInputPropper(string input)
{
    integer len = llStringLength(input);
    // Value to be returned.
    integer value;
    // Unit to be returned.
    string unit;
    string thirdChar = (string) llGetSubString(input, -3, -3);
    
    //Third char is either an SI prefix or an integer.
    if(StringIsNum(thirdChar) == FALSE)
    {
        unit = llGetSubString(input, -3, -1);
        value = (integer) llGetSubString(input, -len, -4);
    } // end if
    else
    {
        unit = llGetSubString(input, -2, -1);
        value = (integer) llGetSubString(input, -len, -3);
    } // end else 
    
    return value + "_" + unit;
} // end MakeAppenededHzInputPropper

// Checks to see if the value can fit in base64.
integer CheckValueForOverflow(string input)
{
    integer len = llStringLength(input);
    if(len > 19)
        return TRUE;
    else 
        return FALSE;
} // end CheckValueForOverflow

default
{
    state_entry()
    {
        flag=0;
        value=0;
        llSetText("Touch me and insert frequency in chat box",<1,1,1>,1.0);
        llListen(0,"",llDetectedKey(0),"");
        llSetColor( <1,1,1>, ALL_SIDES );
    } // end state_entry
    
    touch_start(integer num)
    {
        touch_num++;
        //Checking if the function is currently on or off. 
        //Even number of touch = off. Turn the box color to its original color
        if(touch_num%2!=0)
        {
             // Set prim to white, r g b
             llSetColor( <1,1,1>, ALL_SIDES );
             flag=0;
            
        } // end if
        else
        {
            // Set prim to yellow. r g b
            llSetColor( <1,1,0>, ALL_SIDES );
            flag=1;
            // Inform the user on how to use this prim and what is the allowed input.
            //integer ch1 = (integer)(llFrand(-1000000000.0) - 1000000000.0);
            //string msg1 = "In order to find the wavelength, insert value and unit in the chat box in format \"value unit\" (without quotes) e.g. 10000 Hz";
            //llDialog( llGetOwner(), msg1, ["OK"], ch1);
             
        } // end else    
    } // end touch_start
    
    listen(integer chan, string name, key id, string msg) {
        //If the "apps" is on, this check the input message and act according to the message
        if(flag==1)
        {
            // Put all to lower case letters.
            // IMPORTANT - needed for different conversions.
            msg = llToLower(msg);
            
            // Make a list of given values in order to append undescore.
            // This will eliminate any excess spaces. List of separators can be changed in order to
            // allow/disallow different inputs.
            list tokens = llParseString2List(msg, [" ", ",", "'", "\n", "\"", "_", "\\"], []);
            
            //To check what are the results after using llParseString2List
            /*
            integer i; 
            integer input_length = llGetListLength(tokens);
            for(i = 0; i < input_length; i++)
            {
              llSay(0, "Output = " + llList2String(tokens, i));
            }//for
            */
            
            
            
            // Get the value given in order to check if it is an integer.
            string numValue = llList2String(tokens, 0);
            // Get the unit given in order to see if it is the correct unit.
            string unitGiven = llList2String(tokens, 1);
            
            // Check if the number given is too big in terms of numbers given.
//***FIX THIS---------------------------------------------------------------------------------------------------------------------
            if(CheckValueForOverflow(numValue) == TRUE)
            {
                integer ch4 = (integer)(llFrand(-1000000000.0) - 1000000000.0);
                string msg4 = "The value given has too many numbers. Please try some other unit.";
                llDialog( llGetOwner(), msg4, ["OK"], ch4);
                return;
            }//if
            
            // Code to check for the propper format of given values,
            // and send the values to the ontology. It can be normal or with appended unit.
            if(InputHasAnAppendedHz(numValue) == TRUE ||
               (StringIsNum(numValue) == TRUE) && (UnitIsValid(unitGiven) == TRUE) || //Correct number fotmat with unit
               (StringIsNum(numValue) == TRUE && unitGiven == ""))     //Correct number format without unit
            {
                string msgToSend;
                if(InputHasAnAppendedHz(msg) == TRUE)
                {
                    msgToSend = MakeAppenededHzInputPropper(msg);
                } // end if
                //If there is no unit given then give it a Hz unit
                else if(unitGiven == "")
                { 
                    // Append the underscore.
                    msgToSend = numValue + "_hz";
                } // end else
                else if(UnitIsValid(unitGiven) == TRUE)
                {
                  msgToSend = numValue + "_" + unitGiven;
                }//else
                else
                {
                  llSay(0,"WTF am I doing here?");
                }//else
                 
                //string hz = llGetSubString(msg, llStringLength(msg) - 2, -1);
                llSay(0,"Msg to send is: " + msgToSend);

                // Send the message to the ontology for conversion.
                modSendCommand("MYMOD", msgToSend, "");
            } // end if
            else
            {   //Error message
                string msg2 = "";
                integer ch2 = (integer)(llFrand(-1000000000.0) - 1000000000.0);
                //If there is no unit given
                if(unitGiven == "" && InputHasAnAppendedHz(msg) == FALSE)
                {
                  msg2 = "format error: \"" + msg + "\" isn't a regconised format\n";
                  msg2 += "the format is [frequency] optional[unit] e.g 100 or 100hz";
                }//if
                else
                {
                  //If String isn't a number or decimal 
                  if (StringIsNum(numValue) == FALSE)
                  {
                    msg2 = "format error: \"" + numValue + "\" isn't regconised as a number\n";
                  }//if
                  //If the unit format is incorrect. 
                  if (UnitIsValid(unitGiven) == FALSE)
                  {
                    msg2 += "format error: Unit format \"" + unitGiven + "\" isn't regconised\n";
                  }//if 
                }//else
                llDialog( llGetOwner(), msg2, ["OK"], ch2);
            } // end else
            
            
            //value=llList2Float(tokens,1);
            //value=300000000/value;
            // value needs to be formatted?            
            
            //llSay(0,(string)value+" m");
            //integer ch2 = (integer)(llFrand(-1000000000.0) - 1000000000.0);
            //string msg1="WaveLength=C/f which equals 300000000 / "+msg+"="+(string)value+" m";
            //llDialog( llGetOwner(), msg1, ["OK"], ch2);
            //flag=0;
       
        } // end if
    } // end listen
    
    link_message(integer sender_num, integer num, string message, key id)
    {
       if (flag == 1)
       {
          //integer ch2 = (integer)(llFrand(-1000000000.0) - 1000000000.0);
          //string msg1="WaveLength=C/f which equals 300 MHz / "+message +"="+(string)value+" m";
          //llDialog( llGetOwner(), msg1, ["OK"], ch2);
          //flag=0;
         
          //The result is returned from the ontology side. Underscore is removed.
          list formatted_tokens = llParseString2List(message, [" ", "_"], []);
         
          // Check if the value given was too big and inform the user.
          if(((integer)llList2String(formatted_tokens, 0)) == -1)
          {
              integer ch3 = (integer)(llFrand(-1000000000.0) - 1000000000.0);
              string msg3 = "The value given is too big.";
              llDialog( llGetOwner(), msg3, ["OK"], ch3);
          } // end if
          else
          {
             // Make a final string from the list.        
             string final = llList2String(formatted_tokens, 0) + " " + llList2String(formatted_tokens, 1);
             llSay(0, "Conversion is: " + final);
             // Channel 139 dedicated to communication between this object and electromagnetic spectrum.
             //Disable the box. 
             touch_num++;
             llSay(139, message);
             llResetScript();
          } // end else
       } // end if
    } // end link_message
} // end state default
state Get_Value
{
      state_entry()
    {
          
          //llDialog(llDetectedKey(0), "Please choose one of the below options:",["Yes", "No", "0", "1"], channel);
    }
    
}


