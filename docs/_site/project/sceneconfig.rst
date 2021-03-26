.. _sceneconfig:

***********************************
Scene Configuration
***********************************

.. _rosbridge: http://wiki.ros.org/rosbridge_suite

Radar Model
==================

The framework provides the radar sensor in Unity3D as a prefab model which can directly be imported and is linked to the radar scripts. In the following, we elaborate the most important settings of the radar sensor prefab.

 1. Optionselection. Allows to transmit radar data either via ROS as a rostopic or directly via TCP/IP (e.g. for Matlab, Python, ...)
 2. Fov. Defines the field-of-view of the radar sensor
 3. Chirps. Defines the number of chirps of the radar signal
 4. Samples. Defines the number of samples per radar chirp
 5. Antennas. Defines the number of receiver antenna of the radar sensor.
 6. Sampling Frequency. Defines the sampling frequency of the radar sensor.
 7. Lower Frequency. Defines the lower chirp frequency of the radar sensor.
 8. Bandwidth. Defines the bandwidth of the radar chirp.
 9. Radiation Pattern Mask. Defines a radiation pattern mask of the radar. Chose None if not needed.


Setting up ROS connection 
==================

To correctly set up the ROS connection to the Unity3D environment, the `rosbridge` package is mandatory and should be compiled in your ROS workspace.

In Unity3D, do the following

- Create a new GameObject 
- To the created GameObject, add the *Ros Connector* script which interfaces to rosbridge.
- The ROS connector needs to be configured to the rosbridge server IP address which should be configured in the field *Ros Bridge Server Url*
- Map the GameObject to the radar sensor, i.e. in the prefab model assign the GameObject to the RosConnector field.
- Launch the rosbridge server using *roslaunch rosbridge_server rosbridge_websocket.launch*
- Start the simulation.
