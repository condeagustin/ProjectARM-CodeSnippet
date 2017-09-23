# ProjectARM-CodeSnippet
These are some code snippets taken from my indie game Project ARM made in Unity

**GrappleArm.cs** is the class that has all the gameplay logic for deploying an stretchable arm in order to hook onto special "hookable" objects and retract the player to such objects. Moreover, if the player holds the button, the character will stay attached and will drop when the button is released

**Laser.cs** is the class that handles all the logic for rotating, shooting and fading a Laser. You can see that it passes distortion magnitude and laser distance (distance from the origin of the laser to the point where it collides) as parameters to one of the Laser shaders (LaserMirage.shader)

**LaserMirage.shader** is the shaders that is applied to the second material of the line renderer that forms the laser. This shader captures what is already on screen (by using a GrabPass) and then applies to it a distortion that comes from a noise texture (\_BumpMap) times a distortion magnitude chosen from the editor. Such mix outputs a heat haze mirage effect. The laser distance passed by Laser.cs stretches the texture along X keeping a proportion in order to smooth the look and feel of the whole laser. 
