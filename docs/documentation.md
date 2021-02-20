# Usage Guideline

This document guides you through the ViRa framework and provides important information on how to use the framework. 

- [ViRa at a glance](#vira-at-a-glance)
- [Getting Started](#getting-started)
  - [Important Objects](#important-objects#)
  - [Setting up a Simulation](#setting-up-a-simulation)
  - [Important Parameter Settings](#important-parameter-settings)

## Getting Started

### Using ViRa in a new Unity3D project
1. Create a new Unity3D project using Unity's Built-In render pipeline
2. Clone the repository into the previously created project folder
3. That's it! After Unity3D loaded the new scripts, ViRa is ready to be used.

### Using ViRa in an existing Unity3D project
1. Copy the *ScreenSpaceRadarSimulation* folder and the *Prefab* into your existing Unity3D project folder
2. Optionally, copy the *RosSharp* folder into your existing Unity3D project folder
3. That's it! After Unity3D loaded the new scripts, ViRa is ready to be used.

### Important Objects
1. **MainCam**. The radar sensor in Unity3D is approximated using a camera element. To make things easier, we created a prefab `MainCam` which can be directly used 
in a new simulation scenario without the need of assigning any script functions. The `MainCam` object is linked to the `ScreenSpaceRadarControl` script which loads the radar configuration and fetches data from the GPU for further processing.
2. **RosConnector**. Unity3D does not support ROS out of the box. We therefore use the [rossharp](https://github.com/siemens/ros-sharp) library for ROS connection. Hence, a `RosConnector` object is needed in the simulation to set up the communication to ROS via rossharp. Detailed information about setting up a ROS communication with Unity3D is given [here](#link-to-howto).

### Important Parameter Settings 
The `MainCam` element features parameter settings as obtained by real-world FMCW radars. All parameters can be adjusted during runtime of the simulation.

1. **Optionselection**. Allows to transmit radar data either via ROS as a rostopic or directly via TCP/IP (e.g. for Matlab, Python, ...)
2. **Fov**. Defines the field-of-view of the radar sensor 
3. **Chirps**. Defines the number of chirps of the radar signal
4. **Samples**. Defines the number of samples per radar chirp
5. **Antennas**. Defines the number of receiver antenna of the radar sensor.
6. **Sampling Frequency**. Defines the sampling frequency of the radar sensor.
7. **Lower Frequency**. Defines the lower chirp frequency of the radar sensor.
8. **Bandwidth**. Defines the bandwidth of the radar chirp.
9. **Radiation Pattern Mask**. Defines a radiation pattern mask of the radar. Chose *None* if not needed.

Parameters (6)-(8) are important for correct radar operation and are often obtained in the datasheets of FMCW radar sensors.
