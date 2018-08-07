# ViveTrack-Dynamo

ViveTrack is a [DynamoBIM](http://dynamobim.org/) package that allows real-time reading of HTC Vive spatial tracking data.

### Usage
If you have a working HTC Vive set, just download this package from Dynamo's package manager and look at the sample files.

In `01_Hello_Vive.dyn` you can find an example that tracks the position of all possible Vive objects and previews them in the background:

![](assets/ViveTrackDynamo_banner.png)

`02_Surface_Painting.dyn` uses controller tracking and trigger buttons to trace a series of lines in space and loft them into a surface, via the `DataRecorder` goodie:

<p align="center">
<a target="_blank" href="https://youtu.be/h64ZFtlqzC0"><img width="640" height="343" src="assets/ViveTrackDynamo_surface_sculpting.gif"></a>
</p>

### Projects

If you build something cool, please [tweet me](https://twitter.com/garciadelcast)!

Some projects built with ViveTrack by the community:

Lejla's uses trackers to [move a virtual camera inside a Revit model](http://bim-me-up.com/vivetrack-for-dynamo/) at Autodesk's VR Center in Munich.

<p align="center">
<a target="_blank" href="https://www.youtube.com/watch?v=WEIwpMWbxdc"><img src="assets/lejla_camera_tracking_revit.png"></a>
</p>


### Acknowledgments
ViveTrack for Dynamo is a port from the [awesome project](https://github.com/ccc159/ViveTrack) by [Chen Jingcheng](https://github.com/ccc159). It is built using the [OpenVR](https://github.com/ValveSoftware/openvr) runtime.

This package was developed at the Generative Design Group at [Autodesk](https://www.autodesk.com).

Feel free to report issues and contribute.

