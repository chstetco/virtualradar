.. _structure:

***********************************
Project Structure
***********************************

.. _rossharp: https://github.com/siemens/ros-sharp

The ViRa project has the following folder structure 

.. figure:: vira_structure.PNG
    :scale: 40%
    :align: center
    :alt: ViRa directory tree.
    :figclass: align-center

    Figure 1. ViRa project directory tree.
    
- **Assets**. This is the main folder of the ViRa project. It contains all necessary functionalities for radar data generation. The asset folder also includes assets obtained from the asset store, e.g. environment models, animations or controllers.

    - **AuxiliaryScripts**. This folder contains axuiliary C# scripts for e.g. character movement.
    - **Materials**. Contains different material objects which can be applied to objects in Unity3D.
    - **Prefab**. The prefab folder containts prefabs for Unity3D. A prefab of the radar sensor is included.
    - **RosSharp**. This folder contains the library for ROS communication using `rossharp`_.
    - **Scenes**. The scenes folder contains all the scenes used by Unity3D.
    - **ScreenSpaceRadarSimulation**. This is the main folder of the proposed radar simulator. It contains all shaders and scripts for data generation.
    - **Textures**. Contains all textures used for rendering in Unity3D.
    
- **docs**. This folder contains the ViRa documentation generated with Sphinx.

- **Library**. Contains metadata files.

- **Logs**. This folder contains log files from Unity3D.

- **Packages**. This folder contains installed packages. 

- **ProjectSettings**. This folder contains the Unity3D project settings.
