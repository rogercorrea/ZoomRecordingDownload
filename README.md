# ZoomRecordingDownload

The doc of the Zoom is not easy (for me was confused and I needed of the many time to understand). 
My intention with the code is direction help when anyony need.

Project maked in C# and VS 2022 to get and delete Zoom recording files in the cloud.

To work code you need sett token and meeting variables (in Program.cs file).

## Process:
1) Get recording file available (using token and meeting ID).
2) Remove password (if existing)
3) Get file and save in folder
4) Remove file

## ATTENTION:
The code is in console mode (static and sync). To the best experience in API you can transform to async method.
