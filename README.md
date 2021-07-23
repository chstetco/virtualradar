# Virtual Radar: Real-Time Millimeter-Wave Radar Sensor Simulation for Perception-driven Robotics

**[ViRa](https://aau.at/vira) is a real-time FMCW radar simulation frameworks aimed for robotic applications.**

<img src="https://github.com/chstetco/virtualradar/blob/main/readme_images/ICRA2021_snip01.gif" width="416" height="234" /> <img src="https://github.com/chstetco/virtualradar/blob/main/readme_images/ICRA2021_snip02.gif" width="416" height="234" />

The framework allows simulation of FMCW radar sensors with different configuration parameters in different scenarios embedded in the Unity3D game engine environment. 

## Main features

* Generation of radar **raw data** in real-time
* Fully customizable radar parameters
* Modelling of multi-antenna systems
* Modelling of wave penetration effects of non-conductive objects
* Modelling of antenna radiation patterns for beamforming

A [paper](https://ieeexplore.ieee.org/document/9387149) describing ViRa has been accepted for publication in the IEEE Robotics and Automation Letters (RA-L). 
A [video](https://www.youtube.com/watch?v=R3ZSykLs5iA) showcasing the frameworks capabilities in different scenarios can be found on our Youtube channel.

**NOTE:** We are continuosly working to improve and expand the ViRa framework. For most recent news, please refer to our [news section](https://github.com/chstetco/virtualradar/blob/main/docs/news.md).

<img src="https://www.aau.at/wp-content/uploads/2021/02/efrelogo.png" width="350" height="75" />        

![alt text](https://www.aau.at/wp-content/uploads/2021/02/KWF_FE_pos-aubergine-120x120.png)


## Installation and Usage

For a detailed instruction on how to install and use Vira on your platform, please refer to the links below.

* [Installation Guide](https://virtualradar.readthedocs.io/en/latest/_site/project/installation.html)
* [Framework Documentation](https://virtualradar.readthedocs.io)

## News

- 2021-23-07
  + We added some sample code for processing the output data of ViRa. 
  + We will soon release further implementations for multi-antenna systems. Stay tuned!

- 2021-02-03
  + ViRa's first release is online! V0.1.0 is now available for download.

- 2021-20-02
  + We are online! Creating documentation and installation guides.

- 2021-13-02
  + ViRa has been accepted for publication in the RA-L journal.


## Reference
Please cite our RA-L paper if you use this repository in your publications:
```
@ARTICLE{9387149,  
author={C. {Schöffmann} and B. {Ubezio} and C. {Böhm} and S. {Mühlbacher-Karrer} and H. {Zangl}},  
journal={IEEE Robotics and Automation Letters},   
title={Virtual Radar: Real-Time Millimeter-Wave Radar Sensor Simulation for Perception-Driven Robotics},   
year={2021},  
volume={6},  
number={3},  
pages={4704-4711},  
doi={10.1109/LRA.2021.3068916}
}
```
