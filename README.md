# MxPEG Repair
Repair an MxPEG video recorded by Synology Surveillance Station and converting it to MP4/H264/AC3

## Prerequisits
- [.NET 5.0 runtime](https://dotnet.microsoft.com/download/dotnet/5.0/runtime)
- [ffmpeg](https://www.gyan.dev/ffmpeg/builds/)

## Installation 
1. Download and extract release
2. Please ffmpeg.exe in the same folder
    
## Usage
Drag and drop broken MxPEG video file onto MxpegRepair.exe

## Documentation

Mobotix M26/T26 (MX-V5.4.0.49)
Synology Surveillance Station (05.18.2021)

Mobotix cameras use their own MJPEG standard called MxPEG.
MxPEG is an improvment over MJPEG and compliant with the JPEG standard.

Synology's Surveillance Station claims to be compatible with Mobotix and MJPEG.
This is true as long as you keep the video files inside the Surveillance Station and watch the recordings with their Surveillance Station Client.

The problem is that the stored recordings do not conform to the MxPEG standard anymore. 
The files are a MP4 container with a broken MxPEG video track and separate audio track.
These files cannot be played with any available video player (namely ffmpeg,VLAN).
Audio works but the video is just a black screen. All attempts to convert that video file with ffmpeg will fail.

Analyzing the contained MxPEG stream reveals an error of the APP13 block (audio).
The header (0xFF 0xED) and header data (timestamp) is missing but the audio is there.
This in itself would not be a problem, it would just not play any audio.
The problem is that the audio block sometimes contains JPEG markers. Those break the MxPEG parsing.

This tool removes all junk audio blocks from such an MxPEG file to make it valid again.
The audio is extracted via ffmpeg and then joined with the recovered video to a new, working file.

[Mobotix MxPEG format](https://developer.mobotix.com/docs/mxpeg_frame.html)
[JPEG file format] (https://de.wikipedia.org/wiki/JPEG_File_Interchange_Format)
  
## License
[![MIT License](https://img.shields.io/apm/l/atomic-design-ui.svg?)](LICENSE)

  