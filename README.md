# ClientPCMonitoring
As an Internal IT guy, when manage the application which the application would be run in a distribution mode, that the application would be running from the different place in whole country, but the support resouce is very limited. The face to face traning and debug is not realistic.

Above is reason why I wrote this tool. Both my previous company and my current could use this tool to track user's exception behavior.However, after serached the internet, it seems there is no tools in this area. 

Genernally, It is distributed system client monitoring, performance debug tool.

This is a tool that would including following feature:
* Video Capturing (Thanks to the library https://github.com/MathewSachin/Screna)
* PsPing
* Fiddler Traffic tracking (thanks to library FiddlerCore)

All of these would write into the file in C disk.
