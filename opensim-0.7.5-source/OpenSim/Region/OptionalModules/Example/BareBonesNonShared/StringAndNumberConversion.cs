using System;

public class StringAndNumberConversion
{
    
    // Get only an integer which is in the string.
    // Split the string by a given splitValue
    // The limitation is that it will return the first number found in that string. 
    // -1 indicate that there is no integer in this string
    public static int getOnlyIntegerNumber(string stringAndNumber, char splitValue)
    {
        string[] stringAndNumberArray = stringAndNumber.Split(splitValue);
        foreach (string token in stringAndNumberArray)
        {
            int number;
            if (int.TryParse(token, out number))
                return number;
        }//foreach

        return -1;
    }//getOnlyIntegerNumber

    
}//class
