<?php include 'header.html'; ?>

<!-- To edit information on the slide show, go edit it in frequency_data.txt -->

<div class="frequencyRange">
	<!-- Display different information for different frequency -->
	<h1>
		<?php
			switch($_GET["f"])
			{
				case "ELF": echo 'Extremely Low Frequency (ELF)';
							break;
				case "SLF": echo 'Super Low Frequency (SLF)';
							break;
				case "ULF": echo 'Ultra Low Frequency (ULF)';
							break;
				case "VLF": echo 'Very Low Frequency (VLF)';
							break;
				case "LF":	echo 'Low Frequency (LF)';
							break;
				case "MF":	echo 'Medium Frequency (MF)';
							break;
				case "HF":  echo 'High Frequency (HF)';
							break;
				case "VHF": echo 'Very High Frequency (VHF)';
							break;
				case "UHF": echo 'Ultro High Frequency (UHF)';
							break;
				case "SHF": echo 'Super High Frequency (SHF)';
							break;
				case "EHF": echo 'Extremely High Frequency (EHF)';
							break;
				case "THF": echo 'Tremendously High Frequency (THF)';
							break;
				case "IN": 	echo 'Infrared';
							break;
				case "VL":  echo 'Visible Light';
							break;
				case "UV": 	echo 'Ultra Violet';
							break;
				case "XR":	echo 'X-Ray';
							break;
				default: 	echo 'UNKNOWN frequency';
							break;
			}//switch
				
		?>
	</h1>
</div>
<!-- Information about different type of frequency.-->
<div class="frequencyRangeInfo">
	<h2>
		<?php
			switch($_GET["f"])
			{
				case "ELF": echo 'Frequency range: 3 Hz - 300 Hz. Wavelength: 100,000 km - 10,000 km';
							break;
				case "SLF": echo 'Frequency range: 30 Hz - 300 Hz. Wavelength: 10,000 km - 100,000 km';
							break;
				case "ULF": echo 'Frequency range: 300 Hz - 3000 Hz. Wavelength: 1,000 km - 100 km';
							break;
				case "VLF": echo 'Frequency range: 3 kHz - 30 kHz. Wavelength: 100 km - 10 km';
							break;
				case "LF":	echo 'Frequency range: 30 kHz - 300 kHz. Wavelength: 10 km - 1 km';
							break;
				case "MF":	echo 'Frequency range: 300 kHz - 3000 kHz. Wavelength: 1 km - 100 m';
							break;
				case "HF":  echo 'Frequency range: 3 MHz - 30 MHz. Wavelength: 100 m - 10 m';
							break;
				case "VHF": echo 'Frequency range: 30 MHz - 300 MHz. Wavelength: 10 m - 1 m';
							break;
				case "UHF": echo 'Frequency range: 300 MHz - 3000 MHz. Wavelength: 1 m - 100 mm';
							break;
				case "SHF": echo 'Frequency range: 3 GHz - 30 GHz. Wavelength: 100 mm - 10 mm';
							break;
				case "EHF": echo 'Frequency range: 30 GHz - 300 GHz. Wavelength: 10 mm - 1 mm';
							break;
				case "IN": 	echo 'Frequency range: 300 GHz - 430 THz. Wavelength: 1 mm - 700 nm';
							break;
				case "VL":  echo 'Frequency range:  GHz -  GHz. Wavelength: 740 nm - 380 nm';
							break;
				case "UV": 	echo 'Frequency range:  GHz -  GHz. Wavelength: 400 nm - 10 mm';
							break;
				case "XR":	echo 'Frequency range:  GHz -  GHz. Wavelength: 10 nm - 0.01 nm';
							break;
							
				default: 	echo 'UNKNOWN frequency';
							break;
			}//switch		
		?>
	</h2>
</div>
<!-- Slide show information! to edit slide-show information, go edit it in frequency_data.txt -->
<div class="slidershowWrapper">
	<div class="emptyBox"></div>
	<!-- Add images to slide show-->
	<div id="s1" class="slideshow">    
		<?php  
			//A function that reads images location and description associate to that image and return them into array
			include 'getImagesNTexts.php';
			//Need to start a session because we can reuse the variable return from the function
			session_start();
			$_SESSION['imagesNTexts'] = getImagesNTexts();
			//The function return 2D arrays. 0 = image and 1 = description
			$image = 0;
			//noOfSlide is define by the size of either a image array or description array. Both of them will have the same size
			$noOfSlides = count($_SESSION['imagesNTexts'][$image]);
			//For each images, put them onto the slideshow. 
			for($i = 0; $i < $noOfSlides; $i++)
			{
			  echo '<img src="'.trim($_SESSION['imagesNTexts'][$image][$i]).'" alt="slide'.$i.'">';
			}//for
		?>
	</div>
	<!-- Add texts associate to an image to slide show-->
	<div id="s2" class="textSlideShow">
	  <?php
			//Same process as getting images above but this one is getting description of an image. 
			$description = 1;
			$noOfSlides = count($_SESSION['imagesNTexts'][$description]);
			for($i = 0; $i < $noOfSlides; $i++)
			{
			  echo '<p>'.$_SESSION['imagesNTexts'][$description][$i].'</p>';
			}//for
			session_destroy(); 
	  ?>
	</div>
</div>
<!-- Navigator bar below the slide show-->
<div class="buttonContainer" id="buttonContainer"></div>
<div class="moreInfo">
	<?php
		if($_GET["f"] == 'EHF' || $_GET["f"] == 'IN' || $_GET["f"] == 'VL' || $_GET["f"] == 'UV' || $_GET["f"] == 'XR')
		  echo '';
		else
		{
			echo '<b>Antenna length:</b><ul>';
			switch($_GET["f"])
			{
				case "ELF": echo '<li><a href="http://www.fas.org/nuke/guide/usa/c3i/fs_clam_lake_elf2003.pdf">The Clam Lake antenna</a>: 24 km.</li>
								  <li><a href="http://en.wikipedia.org/wiki/Ground_dipole"> The Republic antenna</a>: 48 km.</li>';
							break;
				case "SLF": echo '<li><a href="http://www.fas.org/nuke/guide/usa/c3i/fs_clam_lake_elf2003.pdf">The Clam Lake antenna</a>: 24 km.</li>
								  <li><a href="http://en.wikipedia.org/wiki/Ground_dipole"> The Republic antenna</a>: 48 km.</li>';
							break;
				case "ULF": echo 'Unavailable';
							break;
				case "VLF": echo '<li><a href="http://en.wikipedia.org/wiki/Grimeton_VLF_transmitter">Varberg Radio Station</a>: 1.9 km long</li>
								  <li><a href="http://en.wikipedia.org/wiki/File:Cutler_VLF_antenna_array.png">"Trideco" antenna tower array</a>
								  :298m tall, and the entire array is 1.871 km in diameter</li>';
							break;
				case "LF":	echo '<li><a href="http://en.wikipedia.org/wiki/Non-directional_beacon">Non-directional beacons: 10m</a></li>
								  <li><a href="http://en.wikipedia.org/wiki/Marcus_Island_LORAN-C_transmitter">Marcus Island LORAN-C transmitter</a>
								  213m</li>';
							break;
				case "MF":	echo '<li><a href="http://en.wikipedia.org/wiki/File:2008-07-28_Mast_radiator.jpg">
								  A quarter-wave antenna at MF can be physically large (25 to 250 metres) </a></li>';
							break;
				case "HF":  //echo 'HF';
							break;
				case "VHF": //echo 'VHF';
							break;
				case "UHF": echo '<li>TV antenna: 0.5-1m for both width and height</li>
								  <li>Cordless Telephone: 1-30 cm long</li>';
							break;
				case "SHF": echo '<li>Varies quite a bit depending on devices.</li>
								  <li>iphone 5 antenna: a few centimetres
								  <li><a href="http://en.wikipedia.org/wiki/Arecibo_Observatory">Arecibo Observatory, Puerto Rico</a>: Diameter: 305 meters</li>';
							break;
				case "EHF": //echo 'EHF';
							break;
				case "IN": 	//echo 'Infrared';
							break;
				case "VL":  //echo 'Visible Light';
							break;
				case "UV": 	//echo 'Ultra Violet';
							break;
				case "XR":	//echo 'X-Ray';
							break;
				default: 	echo 'UNKNOWN frequency';
							break;
			}//switch		
			echo '</ul>';
		}
	?>
	<b>For more info</b>
	<ul>
		<?php
			switch($_GET["f"])
			{
				case "ELF": echo '<li><a href="http://en.wikipedia.org/wiki/Extremely_Low_Frequency"> ELF wiki</a></li>
								  <li><a href="http://en.wikipedia.org/wiki/Ground_dipole"> Ground dipole</a></li>';
							break;
				case "SLF": echo '<li><a href="http://en.wikipedia.org/wiki/Super_low_frequency"> SLF wiki</a></li>';
							break;
				case "ULF": echo '<li><a href="http://en.wikipedia.org/wiki/Ultra_low_frequency"> ULF wiki</a></li>';
							break;
				case "VLF": echo '<li><a href="http://en.wikipedia.org/wiki/Very_low_frequency"> VLF wiki </a></li>';
							break;
				case "LF":	echo '<li><a href="http://en.wikipedia.org/wiki/Low_frequency"> LF wiki </a></li>';
							break;
				case "MF":	echo '<li><a href="http://en.wikipedia.org/wiki/Medium_frequency"> MF wiki </a></li>
								  <li><a href="http://en.wikipedia.org/wiki/Non-directional_beacon"> 
								  non-directional (radio) beacon (NDB)</a></li>';
							break;
				case "HF":  echo '<li><a href="http://en.wikipedia.org/wiki/High_frequency"> HF wiki </a></li>
								 <li><a href="http://en.wikipedia.org/wiki/Rhombic_antenna">Rhombic antenna</a></li>
								 <li><a href="http://en.wikipedia.org/wiki/Citizens%27_band_radio">Citizens Band radio</a></li>';
							break;
				case "VHF": echo '<li><a href="https://en.wikipedia.org/wiki/Very_high_frequency"> VHF wiki </a></li>';
							break;
				case "UHF": echo '<li><a href="http://en.wikipedia.org/wiki/Ultra_high_frequency"> UHF wiki </a></li>';
							break;
				case "SHF": echo '<li><a href="http://en.wikipedia.org/wiki/Super_high_frequency"> SHF wiki </a></li>';
							break;
				case "EHF": echo '<li><a href="http://en.wikipedia.org/wiki/Extremely_high_frequency"> EHF wiki </a></li>';
							break;
				case "IN": 	echo '<li><a href="http://en.wikipedia.org/wiki/Infrared#Telecommunication_bands_in_the_infrared">Infrared wiki</a></li>
								  <li><a href="http://en.wikipedia.org/wiki/Infrared_spectroscopy">Infrared spectroscopy</a></li>';
							break;
				case "VL":  echo '<li><a href="http://en.wikipedia.org/wiki/Light">Visible light wiki </a></li>';
							break;
				case "UV": 	echo '<li><a href="https://en.wikipedia.org/wiki/Ultraviolet">Ultra Violet wiki</a></li>';
							break;
				case "XR":	echo '<li><a href="https://en.wikipedia.org/wiki/X-ray">X-ray wiki</a></li>';
							break;
				default: 	echo 'UNKNOWN frequency';
							break;
			}//switch		
		?>
	</ul>
</div>
<?php include 'footer.html'; ?>