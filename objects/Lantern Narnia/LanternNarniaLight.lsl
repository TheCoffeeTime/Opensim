// Name: Lantern Narnia Light
// Author: JPvdGiessen IT Consultancy
// Release: 1.00
// Description: This script is using the DayChecker, which can be downloaded on http://opensim-creations.com/2012/06/18/day-checker/
//              Put it in a small prim and place it in the Lantern Narnia

    integer Debug = FALSE ;
    integer Gchannel =10001;        // Channel for DayChecker

    vector color = <1,1,1>;  // Use to change the color of the light
    float intensity = 1.000; // Use to change the intensity of the light, from 0 to 1
    float radius = 5.000;  //  Use to change the radius of the light, from 0 to 20
    float falloff = 0.150;  //  Use to set the falloff of the light, from 0 to 2

startParticles()
{
    llParticleSystem([PSYS_PART_FLAGS, 0| PSYS_PART_EMISSIVE_MASK| PSYS_PART_BOUNCE_MASK| PSYS_PART_INTERP_COLOR_MASK| PSYS_PART_INTERP_SCALE_MASK| PSYS_PART_WIND_MASK| PSYS_PART_FOLLOW_VELOCITY_MASK,
                        PSYS_SRC_BURST_RATE, 0.02,
                        PSYS_SRC_BURST_RADIUS, 0.0,
                        PSYS_SRC_BURST_PART_COUNT, 03,
                        PSYS_SRC_ANGLE_BEGIN, 0.05, 
                        PSYS_SRC_ANGLE_END, 0.0,
                        PSYS_SRC_OMEGA, <0.0,0.0,0.0>,
                        PSYS_PART_MAX_AGE, 1.40,
                        PSYS_SRC_MAX_AGE, 0.0,
                        PSYS_SRC_BURST_SPEED_MIN, 0.05,
                        PSYS_SRC_BURST_SPEED_MAX, 0.07,
                        PSYS_SRC_TEXTURE,"",
                        PSYS_PART_START_ALPHA, 0.1,
                        PSYS_PART_END_ALPHA, 0.0,
                        PSYS_PART_START_COLOR, <1.0,1.0,0.0>,
                        PSYS_PART_END_COLOR, <0.4,0.0,0.0>,
                        PSYS_PART_START_SCALE, <0.3,0.3,0.0>,
                        PSYS_PART_END_SCALE, <0.5,0.5,0.0>, 
                        PSYS_SRC_ACCEL, <0.0,0.0,0.5>,
                        PSYS_SRC_PATTERN, PSYS_SRC_PATTERN_ANGLE_CONE
 ]);

}

stopParticles()
{
    llParticleSystem([]);
}

default
{
    state_entry()
    {
        llSay(0, "Script running");
        startParticles() ;
        llListen (Gchannel, "", "", "");
    }

    listen(integer channel, string name, key id, string message) 
    {
        if ( message == "Night")
        {
            startParticles() ;           
        } else if ( message == "Day") {
            stopParticles();
        }
    }
}