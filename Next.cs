using System;
using Microsoft.Gestures.Endpoint;
using Microsoft.Gestures;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Gestures.Stock.Gestures;
using Microsoft.CognitiveServices.Speech;
using System.Text.RegularExpressions;

namespace Next
{
    class Next
    {
        private static GesturesServiceEndpoint _gesturesService;
        private static Gesture _snapGesture;
        private static Gesture _rotateRightGesture;
        private static Gesture _rotateLeftGesture;
        private static Keyboard keyboard;
        private static SpeechRecognizer recognizer;

        static void Main(string[] args)
        {
            Console.Title = "Next! ServiceStatus[Initializing]";

            Console.WriteLine("\n .-----------------. .----------------.  .----------------.  .----------------.\n" + 
                              "| .--------------. || .--------------. || .--------------. || .--------------. |\n" +
                              "| | ____  _____  | || |  _________   | || |  ____  ____  | || |  _________   | |\n" +
                              "| ||_   \\|_   _| | || | |_   ___  |  | || | |_  _||_  _| | || | |  _   _  |  | |\n" +
                              "| |  |   \\ | |   | || |   | |_  \\_|  | || |   \\ \\  / /   | || | |_/ | | \\_|  | |\n" +
                              "| |  | |\\ \\| |   | || |   |  _|  _   | || |    > `' <    | || |     | |      | |\n" +
                              "| | _| |_\\   |_  | || |  _| |___/ |  | || |  _/ /'`\\ \\_  | || |    _| |_     | |\n" +
                              "| ||_____|\\____| | || | |_________|  | || | |____||____| | || |   |_____|    | |\n" +
                              "| |              | || |              | || |              | || |              | |\n" +
                              "| '--------------' || '--------------' || '--------------' || '--------------' |\n" +
                              " '----------------'  '----------------'  '----------------'  '----------------' \n");

            Console.WriteLine("Welcome to Next! Start your presentation and leave this program running in\n" +
                              "background. You can use both gestures and voice to change slide.");
            Console.WriteLine("Press the Escape(Esc) key to quit.");

            keyboard = new Keyboard();
            StartSpeechRecognitionAsync().Wait();

            RegisterGestures().Wait();

            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            // Stop continuous speech recognition
            recognizer.StopContinuousRecognitionAsync().Wait();
        }

        private static async Task RegisterGestures()
        {
            // Step 1: Connect to Microsoft Gestures service            
            _gesturesService = GesturesServiceEndpointFactory.Create();
            _gesturesService.StatusChanged += (s, arg) => Console.Title = $"Next! ServiceStatus [{arg.Status}]";
            await _gesturesService.ConnectAsync();

            // Step 2: Define bunch of custom Gestures
            await RegisterFingerSnapGesture();
            await RegisterRotateRightGesture();
            await RegisterRotateLeftGesture();
        }

        private static async Task RegisterFingerSnapGesture()
        {
            // Step 3: Register the gesture
            _snapGesture = new FingerSnapGesture();
            _snapGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Green);

            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(_snapGesture, isGlobal: true);
        }

        private static async Task RegisterRotateRightGesture()
        {
            // Start with defining the first pose, ...
            var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                            new FingertipPlacementRelation(Finger.Index, RelativePlacement.Right, Finger.Thumb));
            // ... define the second pose, ...
            var rotate = new HandPose("Rotate", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                                new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                                new FingertipPlacementRelation(Finger.Index, RelativePlacement.Below, Finger.Thumb));

            // ... finally define the gesture using the hand pose objects defined above forming a simple state machine
            _rotateRightGesture = new Gesture("RotateRight", hold, rotate);
            _rotateRightGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Blue);
         
            await _gesturesService.RegisterGesture(_rotateRightGesture, isGlobal: true);
        }

        private static async Task RegisterRotateLeftGesture()
        {
            var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                            new FingertipPlacementRelation(Finger.Index, RelativePlacement.Right, Finger.Thumb));

            var rotate = new HandPose("Rotate", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                                new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                                new FingertipPlacementRelation(Finger.Index, RelativePlacement.Above, Finger.Thumb));

            _rotateLeftGesture = new Gesture("RotateLeft", hold, rotate);
            _rotateLeftGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Yellow);

            await _gesturesService.RegisterGesture(_rotateLeftGesture, isGlobal: true);
        }

        private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Gesture detected: ");
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(args.GestureSegment.Name);
            Console.ResetColor();

            if (args.GestureSegment.Name == "FingerSnapGesture")
                keyboard.Send(Keyboard.VirtualKeyShort.RIGHT);
            else if (args.GestureSegment.Name == "RotateRight")
                keyboard.Send(Keyboard.VirtualKeyShort.RIGHT);
            else if (args.GestureSegment.Name == "RotateLeft")
                keyboard.Send(Keyboard.VirtualKeyShort.LEFT);
        }

        private static async Task StartSpeechRecognitionAsync()
        {
            var config = SpeechConfig.FromSubscription("7f4b0ded1b7b41d2aff19883627722ab", "westeurope");
            recognizer = new SpeechRecognizer(config);

            // Subscribe to event
            recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Check if the recognized text is a command
                    var rgxNext = new Regex(@"(^|\p{P})\s?[Nn]ext slide($|\p{P})");
                    var rgxPrevious = new Regex(@"(^|\p{P})\s?[Pp]revious slide($|\p{P})");

                    if (rgxNext.IsMatch(e.Result.Text))
                    {
                        LogRecognizedText(e.Result.Text, rgxNext);
                        keyboard.Send(Keyboard.VirtualKeyShort.RIGHT);
                    }
                    else if (rgxPrevious.IsMatch(e.Result.Text))
                    {
                        LogRecognizedText(e.Result.Text, rgxPrevious);
                        keyboard.Send(Keyboard.VirtualKeyShort.LEFT);
                    }
                }
            };

            // Start continuous speech recognition
            await recognizer.StartContinuousRecognitionAsync();
        }

        private static void LogRecognizedText(string text, Regex rgx)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Voice command detected: ");
            Console.ResetColor();

            var substrings = Regex.Split(text, @"([Nn]ext slide)|([Pp]revious slide)");
            foreach (var str in substrings)
            {
                if (rgx.IsMatch(str))
                {
                    Console.ForegroundColor = ConsoleColor.Magenta;
                    Console.Write(str);
                    Console.ResetColor();
                }
                else
                    Console.Write(str);
            }
            Console.WriteLine();
        }
    }
}
