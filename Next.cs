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
        private static Gesture _fingerSnapGesture;
        private static Gesture _rotateRightGesture;
        private static Gesture _rotateLeftGesture;
        private static Keyboard _keyboard;
        private static SpeechRecognizer _recognizer;


        static void Main(string[] args)
        {
            Console.Title = "Next! ServiceStatus [Initializing]";
            Console.WriteLine("\n     _   __                 __     __\n" +
                                "    / | / /  ___    _  __  / /_   / /\n" +
                                "   /  |/ /  / _ \\  | |/_/ / __/  / /\n" +
                                "  / /|  /  /  __/ _>  <  / /_   /_/\n" +
                                " /_/ |_/   \\___/ /_/|_|  \\__/  (_)\n");
            Console.WriteLine("Welcome to Next! Start your presentation and leave this program running in\n" +
                              "background. You can use both gestures and voice to change slide.\n" +
                              "Press the Escape (Esc) key to quit.");

            _keyboard = new Keyboard();
            StartSpeechRecognitionAsync().Wait();
            RegisterGestures().Wait();

            while (Console.ReadKey(true).Key != ConsoleKey.Escape);

            // Stop continuous speech recognition
            _recognizer.StopContinuousRecognitionAsync().Wait();
        }
        
        
        // It registers the all the gestures that can be used.
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


        // It registers the finger snap gesture.
        private static async Task RegisterFingerSnapGesture()
        {
            // Step 3: Register the gesture
            _fingerSnapGesture = new FingerSnapGesture("FingerSnap");
            _fingerSnapGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Green);

            // Registering the like gesture _globally_ (i.e. isGlobal:true), by global registration we mean this gesture will be 
            // detected even it was initiated not by this application or if the this application isn't in focus
            await _gesturesService.RegisterGesture(_fingerSnapGesture, isGlobal: true);
        }


        // It defines and registers the Rotate Right Gesture.
        private static async Task RegisterRotateRightGesture()
        {
            // Start with defining the first pose
            var hold = new HandPose("Hold", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                            new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                            new FingertipPlacementRelation(Finger.Index, RelativePlacement.Right, Finger.Thumb));
            // define the second pose
            var rotate = new HandPose("Rotate", new FingerPose(new[] { Finger.Thumb, Finger.Index }, FingerFlexion.Open, PoseDirection.Forward),
                                                new FingertipDistanceRelation(Finger.Index, RelativeDistance.NotTouching, Finger.Thumb),
                                                new FingertipPlacementRelation(Finger.Index, RelativePlacement.Below, Finger.Thumb));

            // define the gesture using the hand pose objects defined above forming a simple state machine
            _rotateRightGesture = new Gesture("RotateRight", hold, rotate);
            _rotateRightGesture.Triggered += (s, e) => OnGestureDetected(s, e, ConsoleColor.Blue);
         
            await _gesturesService.RegisterGesture(_rotateRightGesture, isGlobal: true);
        }


        // It defines and registers the Rotate Left Gesture.
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


        // Method called when a gesture is detected. 
        // Based on the kind of gesture, an action is performed.
        private static void OnGestureDetected(object sender, GestureSegmentTriggeredEventArgs args, ConsoleColor foregroundColor)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("Gesture detected: ");
            Console.ForegroundColor = foregroundColor;
            Console.WriteLine(args.GestureSegment.Name);
            Console.ResetColor();

            if (args.GestureSegment.Name == "FingerSnap")
                _keyboard.Send(Keyboard.VirtualKeyShort.RIGHT);
            else if (args.GestureSegment.Name == "RotateRight")
                _keyboard.Send(Keyboard.VirtualKeyShort.RIGHT);
            else if (args.GestureSegment.Name == "RotateLeft")
                _keyboard.Send(Keyboard.VirtualKeyShort.LEFT);
        }


        // This method initializes the speech recognition service and looks for a command.
        private static async Task StartSpeechRecognitionAsync()
        {
            var config = SpeechConfig.FromSubscription("7f4b0ded1b7b41d2aff19883627722ab", "westeurope");
            _recognizer = new SpeechRecognizer(config);

            // Subscribe to event
            _recognizer.Recognized += (s, e) =>
            {
                if (e.Result.Reason == ResultReason.RecognizedSpeech)
                {
                    // Check if the recognized text is a command
                    var rgxNext = new Regex(@"(^|\p{P})\s?[Nn]ext slide($|\p{P})");
                    var rgxPrevious = new Regex(@"(^|\p{P})\s?[Pp]revious slide($|\p{P})");

                    if (rgxNext.IsMatch(e.Result.Text))
                    {
                        LogRecognizedText(e.Result.Text, rgxNext);
                        _keyboard.Send(Keyboard.VirtualKeyShort.RIGHT);
                    }
                    else if (rgxPrevious.IsMatch(e.Result.Text))
                    {
                        LogRecognizedText(e.Result.Text, rgxPrevious);
                        _keyboard.Send(Keyboard.VirtualKeyShort.LEFT);
                    }
                }
            };

            // Start continuous speech recognition
            await _recognizer.StartContinuousRecognitionAsync();
        }


        // It prints the detected command on the console.
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
