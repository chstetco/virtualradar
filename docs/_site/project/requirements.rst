.. _requirements:

***********************************
Hardware and Software Requirements
***********************************

First of all and before installation, your system needs to statisfy some hardware and software prerequisites:

1. **Software**

	Currently, we have tested the framework **only on 64-Bit Windows 10** operating systems. For ROS, we tested both ROS Kinetic and ROS Melodic. We have tested the framework using 	 ROS on Windows 10 with success. A rosbridge is needed for data transfer between Unity and ROS, hence ROS can also be used on an external Linux PC.

	The following software packages are mandatory

		1.1 **Unity3D 2019.4.8**. ViRa is built over the Unity3D game engine environment. ViRa was tested with Unity version 2019.4.8 which is the recommended version for use with ViRa. 

		1.2 **(Optionally) ROS Kinetic**. ROS is not mandatory for running the ViRa project but it is included in the workflow for robotic applications. ViRa was tested with ROS Kinetic and ROS Melodic which we recommend for using ViRa.

		1.3 **Microsoft Visual Studio 2019**. In order to compile the ViRa project, Visual Studio is needed. We recommend Visual Studio 2019. In general, installing Unity3D also implied installation of the Visual Studio suite, so you don't need to install it separately.

2. **Hardware**

	We have tested the framework with the following setup
	
	- Processor: Intel i7-4770 @ 3.4GHz (8 cores)
	- RAM: 16GB
	- GPU: GeForce RTX 2060 (6GB VRAM)

