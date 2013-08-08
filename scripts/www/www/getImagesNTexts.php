<?php 
function getImagesNTexts()
{
	//Written by Oak Thanakorn Tuanwachat 06/2013. 
	//This program only works on a specific format which is defined in 
	//the frequency_data.txt. Its purpose it to read use frequency type from HTML_GET to display
	//The correct images and text on the photo slide show. 

	//Read the frequency data file
	$fileContent = file_get_contents("frequency_data.txt", true);
	//Remove all comments
	$fileContent = preg_replace('/\/\/(.*)/', '', $fileContent);
	//Create an array of frequency type. Different element contain different type e.g. ELF or EHF or both etc. 
	$arrayOfType = explode('>>>', $fileContent);
	
	//This variable will be used to check whether a given argument is defined. 
	$matchFound = FALSE;
	foreach($arrayOfType as $token)
	{
	  //Get the type of the frequency. Token 0 will contain frequency type e.g. ELF. 
	  //Token 1 will contain images link and descriptions
	  $token = explode('<<<', $token);
	  //Pattern = frequency type parameter from HTML get
	  $pattern = $_GET["f"];
	  //If it is the right pattern, then create an array of images & descriptions;
	  if($pattern == trim($token[0]))
	  {	
		//token[1] is all the text after <<<. We then separate each images and its description into an array
	    $ArrayOfImagesNDescriptions = explode('&&', $token[1]);
		$images = array();
		$descriptions  = array();
		foreach(($ArrayOfImagesNDescriptions) as $slide)
		{
		   $imageNDescription = explode("[[", $slide);
		   $imageNDescription[1] = preg_replace('/]]/', '', $imageNDescription[1]);
		   array_push($images, $imageNDescription[0]);
		   array_push($descriptions, $imageNDescription[1]);
		}//foreach
		$matchFound = TRUE;
		return array($images, $descriptions);
	  }//if
	}//foreach
	//If it hasn't been set true in the loop above, therefore the given
	//frequency type argument from HTML_GET isn't 'correct'
	if (!$matchFound)
	  throw new Exception("Given frequency type is not defined");
}//getImagesNTexts()
?>
