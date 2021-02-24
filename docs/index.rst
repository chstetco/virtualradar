# Installation Guideline
This document guides you through all the necessary steps to run ViRa on your machine. 

- [Requirements](#requirements)
  - [Software Requirements](#software-requirements)
  - [Hardware Requirements](#hardware-requirements)
- [Installation Guide](#installation-guide)

## Requirements

### Software Requirements
Currently, we have tested the framework **only on 64-Bit Windows 10** operating systems. For ROS, we tested both ROS Kinetic and ROS Melodic. ROS can be either installed on the same Windows PC or on an external Linux PC. We have tested the framework using ROS on Windows 10 with success. In both cases, a rosbridge is needed for data transfer between Unity and ROS. 

The following software packages are mandatory

- Unity3D 2019.4.8 or greater
- ROS Kinetic or greater
- Microsoft Visual Studio 2019
- rosbridge_suite

### Hardware Requirements
We have tested the framework with the following setup

- Processor: Intel i7-4770 @ 3.4GHz (8 cores)
- RAM: 16GB
- GPU: GeForce RTX 2060 (6GB VRAM)

### Installation Guide
1. **Install Unity3D**. Download and install the Unity3D game engine environment V2019.4.8 or greater ([Link](https://unity3d.com/get-unity/download)). Note that the Microsoft Visual Studio environment comes with Unity3D.
2. **(Optional) Install ROS**. You can either use ROS on a separate PC or you can install ROS on your Windows computer [using this guideline](http://wiki.ros.org/Installation/Windows). 
3. **(Optional) Install ROS dependencies**. When you want to use the framework with ROS, you need the [rosbridge_suite](http://wiki.ros.org/rosbridge_suite) in order to communicate with the Unity3D environment.

If everything is installed, ...
