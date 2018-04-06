# .NET wrapper for libjpeg-turbo
[![Build status](https://ci.appveyor.com/api/projects/status/rvwoc830lo643p61?svg=true)](https://ci.appveyor.com/project/qmfrederik/as-turbojpegwrapper)

libjpeg-turbo is a JPEG image codec that uses processor instructions to accelerate JPEG compression
and decompression.

This library provides .NET wrappers for libjpeg-turbo, allowing you to use it from any .NET language,
such as C#.


## Installation

Install using the command line:

```
Install-Package Quamotion.TurboJpegWrapper
```

#### OS X .NET Core
Two additional packages are required -

runtime.osx.10.10-x64.Quamotion.TurboJpegWrapper\
`dotnet add package runtime.osx.10.10-x64.Quamotion.TurboJpegWrapper`

System.Drawing.Common\
`dotnet add package System.Drawing.Common`

## Credits
The code in this repository is forked from https://bitbucket.org/Sergey_Terekhin/as.turbojpegwrapper/
