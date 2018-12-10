# Smash the glass

## Overview:  
An interactive webpage to simulate the effect of a 3D shattered glass smashed by the user.  
Users will click on the screen to shoot a bullet or smashing a hammer, the glass will be broken like the following image.  

![shattered glass](https://www.textures.com/system/gallery/photos/Windows/Broken%20Glass/22088/BrokenGlass0007_2_download600.jpg)  

This project will be using Voronoi diagram to imitate the desired effect.  

## User Interaction:  
When the webpage starts, there is going to be a glass showing in front of the camera. Users can click on any point of the glass to smash it.

- Rewind:  
After smashing the glass into peices, users can drag the scroll bar below the screen to see the process of it. Drag it slowly to make it an epic replay!

- Force:  
Users can decide how hard they want to smash the glass. With greater power, the glasses will fall further away.

- Standard Dev:  
Standard deviation controls how dense the force will be. When the number becomes less, the impact area will become smaller. Think of it as a bullet, the glass still shatters, but only the ones closer to the center will fall. When the number gets higher, the impact area will become larger, like throwing a basketball to the glass.

- Glass Strength:  
This parameter controls how huch the glass can withstand the impact. When the value goes higher, the more stable the glass will be.

- Show Sites:  
This check box is on the right side of the screen. After enabling it, users can see the random points generated by the program.

## Implementation:
The process of generating shattered glasses is divided into several phases as follows: 

- User Input:  
  The user clicks on any point on the one glass shown in front of them with their mouse. The intersection point is recorded and passed to the next phase.

- Random Point Generation:  
  The program generate random points circling around the given hit point. These random points will be passed to the next phase.

- Fortune's Algorithm:  
  This phase first reads in all the random points from the previous phase and stored in an event queue list which is sorted by the point's height.  
  It will then start scanning through the events stored in the queue. If an site event shows up, it will compute the parabola intersection to each existing sites in the beach line, and split the site where the parabola intersection point is the lowest and placed in between. Let's say the new site event is Ps, the splited site is Pj, and Pi, Pk are left and right of Pj. After splitting the site in the beach line, it will remove a circle event generated by PiPjPk from the event queue, see if PiPjPs, PsPjPk has any valid future circle events and add thoses events to the queue.  
  (Circle events are created by finding the center of a circle that contains the given three points).  
  If a circle event shows up, say PiPjPk, the beach line will find where Pj is and remove it. It will also generate edges for these three sites. Now that Pj is gone, the circle event generated by Pi-1PiPj and PjPkPk+1 will be removed from the event queue as well, while Pi-1PiPk and PiPkPk+1 will created new circle events for the queue.  
  If there are more than three points on the same circle, the beach line will find them all, sorted in clock-wise order, and create edges for each pair of them. After that, the beach line will clean itself so that the remaining sites are in correct order. Later on if a handled circle event is popping out, the beach line will simply ignore it.  
  Lastly, the program clips the voronoi diagram with four bounding lines to set up the open ended edges and keep the sites and its edges within a certain range, and store each site's surrounding voronoi vertex in clock-wise order for the next phase.

- Glasses Generation:  
  With all the glass's points set up, it generates peices of the glass and apply force to the hit point, making the glasses falling to the ground.

## Language and tools:  
Unity (C#)  
[WebGL](http://learningwebgl.com/blog/?p=11)  

## References:  
[Voronoi diagram wiki](https://en.wikipedia.org/wiki/Voronoi_diagram)  

## Author:  
Jietong Chen, Hao Su
