# *Next!* A multimodal tool to remote control your presentations

*Next!* allows you to change slide without using mouse and keyboard or any other kind of device that requires physical contact. Exploiting the Microsoft Kinect, it enables you to use both gestures and voice commands to control your presentations.

**Watch a demo [here](https://youtu.be/r-WBsEpnS9Q).**

## Gestures
If you want to use gestural interaction, there are two different gestures (*Finger Snap* or *Rotate Right*) to go to the next slide and one (*Rotate Left*) to go back to the previous one. 
You can use both hands, but Kinect support for left one is experimental.

### Finger Snap Gesture
You can snap your finger to go on.

<p align="center"> <img src="images/finger_snap.gif"> </p>

### Rotate Gestures
Starting from a horizontal position, keep your thumb and index at the same distance and rotate either right, to go on, or left, to go back.

<p align="center"> <img src="images/rotate.gif"> </p>

## Voice
If you prefer vocal interaction, you can use the commands "*next slide*" or "*previous slide*" to move. In case these words are said inside a sentence, they will be ignored and no action will be performed.
For example, as you can see in the [demo](https://youtu.be/r-WBsEpnS9Q), if you say "*Remember what we saw in the previous slide*", the slide will not be changed. Currently, the only supported language is English.

## Setup and Dependencies
In order to use *Next!* you need a Microsoft Kinect v2 and the [Project Gesture SDK](https://www.microsoft.com/en-us/research/project/gesture/). Note that, due to these dependencies, it runs only on Microsoft Windows.
 Voice recognition works with [Microsoft Azure Speech to Text](https://azure.microsoft.com/en-us/services/cognitive-services/speech-to-text/). It needs a subscription key for Azure Cognitive Services, that can be obtained through a Microsoft account.

Once cloned or dowloaded this repository, just open the [`Next.sln`](Next.sln) solution in Visual Studio, insert your *subscription key* and *region identifier* in [`App.config`](App.config) and run the project. 

## Developement
Our software exploits Kinect's depth sensor to perform gesture recognition and its microphone array for capturing voice.

Once detected a command (either gestural or vocal), a keyboard signal is sent by the program to the application running in foreground. In case you intend to go on, then the `right arrow` key is emulated, otherwise the `left arrow` one. Due to this implementation, the program can be used with any application chosen by the user, without being strictly linked to a single one through an API.

### Gestural Interaction
Regarding the employed gestures, the *Finger Snap* gesture is a predefined one in the gestures SDK, while we had to define the *Rotate Right* and *Rotate Left* gestures.
The definition of a gesture is seen as a sequence of two poses.
We set a common initial pose, in which the index finger and the thumb are well spaced and in the horizontal position, while the other fingers are bent. In the second pose, instead, the disposition of the fingers is the same, but the hand is rotated by 90 degrees so that the thumb and the index are in the vertical position. The difference between the two gestures (right and left) is recognized because, when you use the right hand, the thumb is **above** the index in the former, vice-versa in the latter. In case you use the left hand, the role of the two fingers is inverted.

<p align="center"> <img src="images/gestures_recognition.gif"> </p>

Our choice fell on the rotating gestures because it is very difficult that they can be misunderstood. At the beginning, we thought about using the swipe gestures, because they would have been more natural, but the problem is that while you are presenting and explaining something you tend to gesticulate a lot and a random movement could generate a false positive. However, rotating right to go on and rotating left to go back is intuitive enough, tolerable and harder misunderstood.

Since going to the next slide is more frequent than going back, we decided to enhance interface flexibility and comfort adding also the *Finger Snap* gesture to go on. It is a very easy gesture and rather impossible to confuse.

### Vocal Interaction
Even if the gestural interaction is better to use because the voice channel is already employed for the presentation, it is important to say that it works only if you are in the field of view of Kinect sensors. For this reason, the use of vocal interaction in some cases could add flexibility giving you the possibility to change slides although you are not in a good position. On the other hand, if a malicious listener pronounces the command, it is recognized and generates an action undesired by the presenter. A possible solution could be using a speaker dependent system, so that the system is trained to recognize only the voice of the user. Microsoft Azure enables this [costumization](https://docs.microsoft.com/en-in/azure/cognitive-services/speech-service/how-to-custom-speech-test-and-train).

In order to avoid that the pronunciation of the words "*next slide*" or "*previous slide*" inside more complex sentences triggers a command, we used a regular expression which matches commands if and only if they are pronounced alone or syntactically separated from the rest of the sentence. This way, an expression such as "*Next slide, please*" is recognized as a command, while "*Next slide will show...*" is not.

## Authors
[Francesca Romana Mattei](https://github.com/francescaromana), [Antonio Musolino](https://github.com/antoniomuso) and [Davide Sforza](https://github.com/dsforza96).

## License
Our code is released under [MIT license](LICENSE).
