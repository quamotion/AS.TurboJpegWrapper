# .NET wrapper for libjpeg-turbo
[![Build Status](https://dev.azure.com/qmfrederik/AS.TurboJpegWrapper/_apis/build/status/qmfrederik.AS.TurboJpegWrapper)](https://dev.azure.com/qmfrederik/AS.TurboJpegWrapper/_build/latest?definitionId=2)

libjpeg-turbo is a JPEG image codec that uses processor instructions to accelerate JPEG compression
and decompression.

This library provides .NET wrappers for libjpeg-turbo, allowing you to use it from any .NET language,
such as C#.

For Windows (32-bit and 64-bit), macOS (all versions) and Linux (Ubuntu 16.04 only), the NuGet package
includes libjpeg-turbo. For other Linux distributions, you need to install libjpeg-turbo using your
package manager.


## Installation

Install using the command line:

```
Install-Package Quamotion.TurboJpegWrapper
```

#### macOS - .NET Core
Make sure you also include a reference to System.Drawing.Common:
`dotnet add package System.Drawing.Common`

## Credits
The code in this repository is forked from https://bitbucket.org/Sergey_Terekhin/as.turbojpegwrapper/
