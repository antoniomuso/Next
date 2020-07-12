# *Next!* A tool to remote control your presentations

*Next!* allows you to change slide without using mouse and keyboard or any other kind of device that requires physical contact. Exploiting the Microsoft Kinect, it enables you to use both gestures and voice commands to control your presentations.

**Watch a demo [here](https://youtu.be/r-WBsEpnS9Q).**

## Gestures
If you want to use gestures, there are two different gestures (*Finger Snap* or *Rotate right*) to go to the next slide and one (*Rotate left*) to go back to the previous one.

### Finger Snap Gesture
You can snap your finger to go on.

<p align="center"> <img src="images/finger_snap.gif"> </p>

### Rotating Gestures
Starting from an horizontal position, keep your thumb and index at the same distance and rotate either right, to go on, or left, to go back.

<p align="center"> <img src="images/rotate.gif"> </p>

## Voice
If you prefer vocal interaction, you can use the commands "*next slide*" or "*previous slide*" to move. In case these words are said inside a sentence, they will be ignored and no action will be performed.
For example as you can see in the [demo](https://youtu.be/r-WBsEpnS9Q), if you say "*Remember what we saw in the previous slide*", the slide will not be changed.

## Setup and Dependencies
In order to use *Next!* you need a Microsoft Kinect v2 and the [Project Gesture SDK](https://www.microsoft.com/en-us/research/project/gesture/). Note that, due to these dependencies, it runs only on Microsoft Windows.

 Voice recognition works with [Microsoft Azure Speech to Text](https://azure.microsoft.com/en-us/services/cognitive-services/speech-to-text/). It needs a subscription key for Azure Cognitive Services, that can be obtained through a Microsoft account.

Once cloned or dowloaded this repository, just open the [Next.sln](Next.sln) solution in Visual Studio and run it.

## Developement

## Authors

## License