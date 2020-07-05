﻿using System;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Gestures.Stock.Gestures;
using Microsoft.CognitiveServices.Speech;
using System.Text.RegularExpressions;

namespace ConsoleManaged
{
    class Program
    {
        private static GesturesServiceEndpoint _gesturesService;
        private static Gesture _snapGesture;
        private static Gesture _swipeGesture;
        private static Keyboard keyboard;
        private static SpeechRecognizer recognizer;

        static async Task Main(string[] args)
        {
            Console.Title = "GesturesServiceStatus[Initializing]";
            Console.WriteLine("Execute one of the following gestures: Like, Drop-the-Mic, Rotate-Right ! press 'ctrl+c' to exit");

            // One can optionally pass the hostname/IP address where the gestures service is hosted
            var gesturesServiceHostName = !args.Any() ? "localhost" : args[0];
            RegisterGestures(gesturesServiceHostName).Wait();
            keyboard = new Keyboard();
            await RecognizeSpeechAsync();
            Console.ReadKey();

            // Stop continuous speech recognition
            await recognizer.StopContinuousRecognitionAsync();
        }

        private static async Task RegisterGestures(string gesturesServiceHostName)
        {
            // Step 1: Connect to Microsoft Gestures service            
            _gesturesService = GesturesServiceEndpointFactory.Create(gesturesServiceHostName);
            _gesturesService.StatusChanged += (s, arg) => Console.Title = $"GesturesServiceStatus [{arg.Status}]";
            await _gesturesService.ConnectAsync();

            // Step 2: Define bunch of custom Gestures, each detection of the gesture will emit some message into the console
            await RegisterFingerSnapGesture();
            await RegisterSwipeGesure("SwipeLeftGesture", PoseDirection.Left);
        }

        private static async Task RegisterFingerSnapGesture()
        {
            // Start with defining the first pose, ...
            // ... define the second pose, ...
            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine
            _snapGesture = new FingerSnapGesture();
            _snapGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(_snapGesture, isGlobal: true);
        }

        private static async Task RegisterSwipeGesure(string name, PoseDirection direction)
        {
            // Start with defining the first pose, ..
            var fingerSet = new HandPose("FingersSet", new PalmPose(new AnyHandContext(), direction, orientation: PoseDirection.Forward),
                                                       new FingertipDistanceRelation(Finger.Middle, RelativeDistance.Touching, new[] { Finger.Index, Finger.Ring }),
                                                       new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring }, PoseDirection.Forward));

            // ... define the second pose, ...
            var fingersBent = new HandPose("FingersBent", new PalmPose(new AnyHandContext(), direction, orientation: PoseDirection.Forward),
                                                          new FingertipDistanceRelation(Finger.Middle, RelativeDistance.Touching, new[] { Finger.Index, Finger.Ring }),
                                                          new FingerPose(new[] { Finger.Index, Finger.Middle, Finger.Ring }, direction | PoseDirection.Backward));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine
            _swipeGesture = new Gesture(name, fingerSet, fingersBent);
            _swipeGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Blue);

            // Step 3: Register the gesture             
            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(_swipeGesture, isGlobal: true);
        }

        private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Gesture detected! : ");
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(args.GestureSegment.Name);
            Console.ResetColor();
            if (args.GestureSegment.Name == "FingerSnapGesture")
                keyboard.Send(Keyboard.ScanCodeShort.RIGHT);
            else if (args.GestureSegment.Name == "SwipeLeftGesture")
                keyboard.Send(Keyboard.ScanCodeShort.LEFT);
        }

        private static async Task RecognizeSpeechAsync()
        {
            var config =
                SpeechConfig.FromSubscription(
                    "7f4b0ded1b7b41d2aff19883627722ab",
                    "westeurope");

            recognizer = new SpeechRecognizer(config);

            // Subscribe to event
            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Do something with the recognized text
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"We recognized: ");
                    Console.ResetColor();
                    Console.WriteLine(e.Result.Text);

                    var rgxNext = new Regex(@"(^|\p{P})\s?next slide($|\p{P})", RegexOptions.IgnoreCase);
                    var rgxPrevious = new Regex(@"(^|\p{P})\s?previous slide($|\p{P})", RegexOptions.IgnoreCase);
                    if (rgxNext.IsMatch(e.Result.Text))
                        keyboard.Send(Keyboard.ScanCodeShort.RIGHT);
                    else if (rgxPrevious.IsMatch(e.Result.Text))
                        keyboard.Send(Keyboard.ScanCodeShort.LEFT);
                }
            };

            // Start continuous speech recognition
            await recognizer.StartContinuousRecognitionAsync();
        }
    }
}
