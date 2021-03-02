.. _installation:

***********************************
Installation Guide
***********************************

.. _ViRa github: https://github.com/chstetco/virtualradar

Once you have checked the :ref:`requirements` you are ready to install ViRa on your platform. 

**Using ViRa in a new Unity3D project**

  - Clone the `ViRa github`_ repository
  - Start Unity3D and add a new project to the Unity Hub. Navigate to the cloned folder and add it
  - Start the project
  - That's it! After Unity3D loaded the new scripts, ViRa is ready to be used
  
**Using ViRa in an existing Unity3D project**

  - Copy the *ScreenSpaceRadarSimulation* folder and the *Prefab* folder into your existing Unity3D project folder
  - Optionally, copy the *RosSharp* folder into your existing Unity3D project folder if you need ROS support.
  - In Unity3D, go to *Edit -> Project Settings* and in the *Player* tab change the *Api Compatibility Level* to *.NET 4.x*. 
    Also, change the *Color Space* to *Linear*.
  - That's it! After Unity3D loaded the new scripts, ViRa is ready to be used.
