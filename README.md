N3DSCmbViewer
=============

An unfinished model viewer for The Legend of Zelda: Ocarina of Time 3D and Majora's Mask 3D, written in C#.

Screenshots
===========

![N3DSCmbViewer](http://i.imgur.com/uyXvgSI.png)
![N3DSCmbViewer](http://i.imgur.com/A0fSt2h.png)


How to build
=======
Add and build the [Aglex repository](https://github.com/NishaWolfe/Aglex)

Install OpenTK 3 for .NET Framework 4.x. (Tools --> NuGet Package Manager --> Package Manager Console --> `Install-Package OpenTK -Version 3.3.1`)



Warning
=======

__This is unfinished, experimental code.__ You _will_ encounter glitches, broken functionality, etc., etc. Do not _expect_ anything related to OoT3D/MM3D (models, animation, archives, etc.) to work and be happy if something _does_. There should, however, _not_ be any problems just running the application.

Also note that this repository supersedes [the old source code archive](http://magicstone.de/dzd/random/3ds/N3DSCmbViewer-bin-src.rar) posted in 2014, before this repository existed.

Acknowledgements
================

* ZAR archive format reverse-engineered and documented by Twili
* Various additional research by Twili
* COLLADA exporter written with a lot of help from Peardian
* LZSS decompression code adapted from [C++ code by ShimmerFairy](https://github.com/ShimmerFairy/MM3D/)
* Additional modifications by NishaWolfe
