# Smash the glass

## Overview:  
An interactive webpage to simulate the effect of a 3D shattered glass smashed by the user.  
Users will click on the screen to shoot a bullet or smashing a hammer, the glass will be broken like the following image.  

![shattered glass](https://www.textures.com/system/gallery/photos/Windows/Broken%20Glass/22088/BrokenGlass0007_2_download600.jpg)  

This project will be using Voronoi diagram to imitate the desired effect.  

## Implementation:
The process of generating shattered glasses is divided into several phases as follows:

- User Interaction: 
The user clicks on any point on the one glass shown in front of them with their mouse. The intersection point is recorded and passed to the next phase.

- Random Point Generation: 
The program generate random points circling around the given hit point. These random points will be passed to the next phase.

- Fortune's Algorithm:
This phase first reads in all the random points and generates the corresponding vironoi diagram.
Lastly, it clips the voronoi diagram with four bounding lines to keep the sites/cells that lies within the range, and output each site's surrounding voronoi vertex in clock-wise order to the next phase.

- Glasses Generation:
With all the glass's points set up, it generates peices of the glass and apply force to the hit point, making the glasses falling to the ground.

- User Interface:
After the glass shattered to peices, there is a scroll bar below the screen for the user to rewind what just happened, in any speed they want. 
There is another feature on the right side of the screen called "Show Sites". After enabling it, the user is able to see the random points generated in the second phase.
## Language and tools:  
Unity (C#)  
[WebGL](http://learningwebgl.com/blog/?p=11)  

## References:  
[Voronoi diagram wiki](https://en.wikipedia.org/wiki/Voronoi_diagram)  

## Author:  
Jietong Chen, Hao Su
