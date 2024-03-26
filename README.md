Lucas Corey

Physics App: A hand-made 3D physics application in Unity

Simulation

Physics World drives Collision World and resolves collisions reported by collision world.

Unity 2022.3.17f1

No dependencies

Built for PC

* OrientedBoundingBox.cs - A custom OBB class with Handles tooling for live in-editor changes (like the native version)
* RigidBody.cs - A class that contains information for a bodies angular and linear movement, and methods to change those values based on different force types (like the native version)
* CollisionWorld.cs - A class that keeps track of all OBBs and checks collisions every frame using SAT. Adds collisions to a list that RigidBody.cs references. Once a collision is resolved, it is marked as so by RigidBody.cs, and CollisionWorld filters out resolved collisions every frame.
* PhysicsWorld.cs - A script that looks keeps track of each custom RigidBody in the world and drives the CollisionWorld class. It also resolves all collisions, but limited only to translational movement. I wish I could have added rotational resolution, but I couldn't find any resources that went in-depth enough to help. I always got stuck when having to find the collision point on a body to determine the appropriate torque impulse response vector. If you know anything that would help, please send it my way. I'd like to keep working on this thing.

Buttons in the top left are the only controls in the app
* Pause - Pauses and plays the game
* Reset - Reloads the scene for reset physics fun!
* Go Crazy - Applies random force and rotation impulses to every physics object
* Quit - Exits the application

Known Issues/Notes - Bugs , issues or limitations with potential workarounds
* Pause button does not toggle "pause"/"start" when clicked
* Plenty of tunneling due to my desperate focus and failure to implement rotational dynamics
* No rotational dynamics :(
* Physics objects are unresponsive when blue walls on all sides are touching (not sure why, but thats why there are gaps)
* GoCrazy random forces tend toward bottom left corner for some reason

Demo Video: https://drive.google.com/file/d/1dFIZg7JRMBJf3lGIJdSGUlq1BBetdERc/view?usp=drive_link
