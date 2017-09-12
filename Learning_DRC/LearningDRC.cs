using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Learning_DRC
{
    internal sealed class LearningDRC
    {
        private static readonly FileInfo FileBatch = new FileInfo("batchwords.txt");
        private static readonly FileInfo FileLog = new FileInfo("log.txt");
        public static readonly Stopwatch Timer = new Stopwatch();

        #region SAVE NETWORK METHODS

        /// <summary>
        ///     Save record of learned orthographic words, and to which phonological words
        ///     they are connected, in the file orthographicknowledge.txt
        /// </summary>
        /// <param name="workingSubDir"></param>
        private void SaveLearnedOrthographicVocabulary(FileSystemInfo workingSubDir)
        {
            var path = Path.Combine(workingSubDir.FullName, OrthographicKnowledgeFilename);
            var sw = new StreamWriter(path, false);

            sw.WriteLine("# orthographicknowledge.txt");
            sw.WriteLine("# This file records all known printed words, their indexes, and");
            sw.WriteLine("# the spoken words to which these printed words are connected..");
            sw.WriteLine("# FORMAT:");
            sw.WriteLine("# WORD <OWord> <OWordIndex> <OWordFreq> <SpokenWord1> <SpokenWord1Index> <SpokenWord2> ...");
            sw.WriteLine("#");
            sw.WriteLine(
                $"# Parameters: OWordThreshold: {_printedWordRecogThreshold}  PWordThreshold: {_spokenWordRecogThreshold}  C2PExcitation: {_semantic2PhonolexExcitation}  MinReadingPhonology: {_minReadingPhonology}");
            sw.WriteLine("#");

            // Write WORD lines
            var line = new StringBuilder();

            for (var l = 0; l < _printedWordsCount; l++)
            {
                line.Append($"WORD {_printedWords[l]} {l} {_printedWordFreq[l]}");
                for (var x1 = 0; x1 < _spokenWordsForEachPrintedWord[l].Count; x1++)
                    line.Append(
                        $" {_spokenWords[_spokenWordsForEachPrintedWord[l][x1]]} {_spokenWordsForEachPrintedWord[l][x1]}");
                sw.WriteLine(line);
                line.Length = 0;
            }
            sw.Close();
        }

        #endregion

        public static ConsoleKeyInfo GetExecutionMode()
        {
            Console.WriteLine("Do you want to run a batch of stimuli or do a bulk run?");
            Console.Write("Press b for batch, k for bulk run, or any other key for individual stimuli: ");
            var modeChoice = Console.ReadKey();
            Console.WriteLine();
            Console.WriteLine();
            return modeChoice;
        }

        public void ExecuteBasedOnMode(ConsoleKeyInfo executionMode)
        {
            switch (executionMode.KeyChar)
            {
                case 'b':
                case 'B':
                    BatchProcessing();
                    break;
                case 'k':
                case 'K':
                    BulkRun();
                    break;
                default:
                    ManualProcessing();
                    break;
            }
        }

        public void BulkRun()
        {
            var bulkRun = new BulkRun(this);
            bulkRun.RunSimulations();
        }

        public void BatchProcessing()
        {
            StreamReader streamR = null;
            try
            {
                streamR = FileBatch.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the batch file.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            string line;
            do
            {
                line = streamR.ReadLine();

                if (line == null)
                    continue;

                var splitline = line.Split(' ');

                switch (splitline.Length)
                {
                    case 2:
                        RunSimulationWithContext(splitline[0], splitline[1]);
                        break;
                    case 1:
                        RunSimulationWithoutContext(splitline[0]);
                        break;
                    default:
                        var streamW = new StreamWriter(FileLog.FullName, true);
                        streamW.WriteLine("Bad input line skipped.");
                        streamW.Close();
                        Console.WriteLine("Bad input line skipped.");
                        break;
                }
            } while (line != null);
            streamR.Close();
        }

        private void ManualProcessing()
        {
            var line = "";
            do
            {
                line = GetInputFromUser();
                var splitline = line?.Split(' ');
                if (line == "")
                    continue;

                RunSingleItemSimulation(splitline?[0], splitline?[1]);
            } while (line != "");
        }

        private static string GetInputFromUser()
        {
            Console.WriteLine("Enter stimulus <Printed Word> <Context> :");
            Console.WriteLine("Context is optional.");
            Console.WriteLine("(Note: Context is represented with phoneme symbols)");
            Console.WriteLine("Press ENTER to exit.");
            return Console.ReadLine();
        }

        private void RunSingleItemSimulation(string input, string context)
        {
            if (input == null)
                Console.WriteLine("Bad input line.");
            else if (context == null)
                RunSimulationWithoutContext(input);
            else
                RunSimulationWithContext(input, context);
        }

        private void RunSimulationWithContext(string input, string context)
        {
            Timer.Start();
            var output = Simulate(input, context,
                new DirectoryInfo(Directory.GetCurrentDirectory()));
            Timer.Stop();
            WriteResultToFileAndConsole(input, output);
            Timer.Reset();
        }

        private void RunSimulationWithoutContext(string input)
        {
            if (FileLog == null) throw new ArgumentNullException(nameof(FileLog));

            Timer.Start();
            var output = Simulate(input, "no_context",
                new DirectoryInfo(Directory.GetCurrentDirectory()));
            Timer.Stop();
            WriteResultToFileAndConsole(input, output);
            Timer.Reset();
        }

        private static void WriteResultToFileAndConsole(string input, IList<string> output)
        {
            var streamW = new StreamWriter(FileLog.FullName, true);
            WriteResultToTextWriter(input, output, streamW);
            WriteResultToConsole(input, output);
        }

        private static void WriteResultToTextWriter(string input, IList<string> output, TextWriter streamW)
        {
            streamW.Write("RT: {0}  Input: {1}  Context: ", output[0], input);
            if (output.Count > 2)
                for (var i = 2; i < output.Count; i++)
                    streamW.Write(" {0}", output[i]);
            else
                streamW.Write(" <none>");
            streamW.Write("  Output: {0}  ", output[1]);
            streamW.WriteLine("Sim_time: {0} ms", Timer.ElapsedMilliseconds);
            streamW.Close();
        }

        private static void WriteResultToConsole(string input, IList<string> output)
        {
            Console.Write($"RT: {output[0]}  Input: {input} Context: ");
            if (output.Count > 2)
                for (var i = 2; i < output.Count; i++)
                    Console.Write($" {output[i]}");
            else
                Console.Write(" <none>");
            Console.Write($"  Output: {output[1]}  ");
            Console.WriteLine("Sim_time: {0} ms", Timer.ElapsedMilliseconds);
        }

        #region FIELDS

        private readonly Random _rnd = new Random(); // used to select random contextual options to activate
        private const char Blankchar = '+'; // choose arbitrary ascii character to act as the blank character.      

        // PROPERTY FIELDS -
        // read from "properties" file.
        private string _name; // Data from DRC-1.2.1

        private string _version; // Data from DRC-1.2.1
        private string _releaseDate; // Data from DRC-1.2.1
        private string _creator; // Data from DRC-1.2.1
        private string _url; // Data from DRC-1.2.1
        private int _nLetterSlots; // Number of letter level slots
        private int _nPhonemeSlots; // Number of phoneme level slots


        // LETTERS, PHONEMES, VOCAB FIELDS
        private int[][] _letterFeatures; // visual feature patterns for each letter

        private Dictionary<int, char> _letters; // orthographic letters, with index as key
        private Dictionary<char, int> _lettersReverseDictionary; // orthographic letters, with letter as key
        private Dictionary<int, char> _phonemes; // phonemes, with index as key
        private Dictionary<char, int> _phonemesReverseDictionary; // phonemes, with phoneme as key
        private bool[] _vowelStatus; // whether each letter is a vowel or not
        private List<string> _printedWords; // orthographic lexicon words
        private List<int> _printedWordFreq; // orthographic word frequencies
        private int _maxPrintedWordFreq; // frequency of the most frequent orthographic word
        private string[] _spokenWords; // phonological lexicon words
        private int[] _spokenWordFreq; // phonological word frequencies
        private int _maxSpokenWordFreq; // frequency of most frequent phonological word

        private float[] _printedCFS;
        // orthographic constant frequency scaling (CFS) values (see Coltheart et al. (2001) p.216)

        private float[] _spokenCFS;
        // phonological constant frequency scaling (CFS) values (see Coltheart et al. (2001) p.216)

        private List<List<int>> _spokenWordsForEachPrintedWord;
        // phonological words excited by each orthographic word (e.g. /b5/ and /b6/ excited by BOW)

        private List<int>[] _printedWordsForEachSpokenWord;
        // learned orthographic words excited by each phonological word (e.g. SALE and SAIL excited by /s1l/)

        // GPC FIELDS
        private GPCRule[] _bodyRules;

        private GPCRule[] _multiRules;
        private GPCRule[] _twoRules;
        private GPCRule[] _mphonRules;
        private GPCRule[] _contextRules;
        private GPCRule[] _singleRules;
        private GPCRule[] _outRules;

        private string _currentGPCInput; // stores the letters currently available to the GPC route.

        private int _currentRightmostPhoneme;
        // the right-most phoneme slot receiving activation from the GPC route last cycle.

        private bool _lastSlotSeen;
        // set to true if the GPCRoute tries to include input from another letter slot,
        // but can't because already at the last slot.

        // FILE NAME FIELDS
        private readonly FileInfo _fileProperties = new FileInfo("properties");

        private readonly FileInfo _fileParameters = new FileInfo("default.parameters");
        private readonly FileInfo _fileLearningParameters = new FileInfo("learning.parameters");
        private readonly FileInfo _fileLetters = new FileInfo("letters");
        private readonly FileInfo _fileVocab = new FileInfo("vocabulary");
        private readonly FileInfo _filePhonemes = new FileInfo("phonemes");
        private readonly FileInfo _fileGPCRules = new FileInfo("gpcrules");
        private readonly FileInfo _fileActs = new FileInfo("activations.txt");
        private const string OrthographicKnowledgeFilename = "orthographicknowledge.txt";

        // WEIGHT MATRICES
        private float[][] _vF1ToLetterWeights; // stores excitation/inhibition from each VF to each letter

        private float[][] _vF0ToLetterWeights; // stores excitation/inhibition from each inverse VF to each letter

        // NETWORK STATE FIELDS
        private string _stimulus; // used in Simulate and ClampVisualFeatures methods
        private int _stimulusLength; // used in both the Simulate and Search4Multi methods
        private float _normMultiplier; // L2O normalisation multiplier. Equals nLetterSlots / stimulusLength, 
        // or 1 if L2O_NORMALISATION = false,
        private float[] _outVisualFeatureLayer1; // stores VF state according to the input stimulus

        private float[] _outVisualFeatureLayer0; // stores VF state according to the input stimulus

        private float[] _netInputLetterLayer;
        // arraysize equal to nSlots * nLetters to store net input to each letter node

        private float[] _netInputOrtholexLayer;
        // store net input to each learned OL word node. Resized each time new node learned.

        private float[] _netInputPhonolexLayer; // stores net input to each PL word node

        private float[] _netInputPhonemeLayer;
        // arraysize equal to nSlots * nPhonemes to store net input to each phoneme node

        private float[] _actLetterLayer; // arraysize equal to nSlots * nLetters to store net input to each letter node

        private float[] _actOrtholexLayer
            ; // store activation of each learned OL word node. Resized each time new node learned.

        private float[] _actPhonolexLayer; // stores activation of each PL word node

        private float[] _actPhonemeLayer
            ; // arraysize equal to nSlots * nPhonemes to store activation to each phoneme node

        private float _actSemanticNode; // activation of the semantic node (which excites the semantic layer).
        // When random nodes are co-activated, they are assumed to have the same activation
        // as the correct node, and there is no feedback to the semantic layer,
        // so only keeping track of one value here.

        // LEARNING PARAMETER FIELDS
        private bool _learningOn; // turn off to prevent learning

        private bool _l2ONormalisation; // set to true to normalise input from L to O based on length of stimulus+blanks

        private int _orthographicBlanks
            ; // 0 = no blanks, 1 = 1 blank, 2 = enough blanks so that each Onode is 8 letters long.
        // Examples: 0: "CAT", 1: "CAT+", 2: "CAT+++++"

        private int _nvFeatures; // number of visual features per letter
        private int _maxCycles; // max number of cycles before timeout

        private bool _printActivations; // set to true to print an activations file. slows down the simulation.     
        private float _minActivationReport; // minimum activation to get reported in the activations.txt file.

        private float _printedWordRecogThreshold;
        // OL node must be above this value for a printed word to be "recognized".

        private float _spokenWordRecogThreshold; // PL node mus be above this value for a spoken word to be "recognized".
        private float _semantic2PhonolexExcitation;
        private float _semantic2PhonolexInhibition;
        private float _contextInput2Semantic; // Analogous to visual features, but for context. "Contextual feature".

        private int _printedWordFreqMultiplier
            ; // Controls the rate at which printedWordFreq values are increased per exposure.

        private int _numberOfSemanticRepsActivated
            ; // Controls the number of semantic nodes (including the correct one corresponding to the input word)

        //  that are activated for a simulation. E.g., if value is 3, then the correct semantic node plus
        //  two additional random words will receive activation.
        private string[] _context; // holds the input context

        // DRC-1.2.1 PARAMETER FIELDS
        //General Parameters
        private float _activationRate;

        private float _frequencyScale;
        private float _minReadingPhonology;

        //Feature Level Parameters
        private float _featureLetterExcitation;

        private float _featureLetterInhibition;

        //Letter level Parameters
        private float _letterOrthLexExcitation;

        private float _letterOrthLexInhibition;
        private float _letterLateralInhibition;

        //Orthographic Lexicon (OrthLex) Parameters
        private float _orthLexPhonLexExcitation;

        private float _orthLexPhonLexInhibition;
        private float _orthLexLetterExcitation;
        private float _orthLexLetterInhibition;
        private float _orthLexLateralInhibition;

        //Phonological Lexicon (Phonlex) Parameters
        private float _phonLexPhonemeExcitation;
        private float _phonLexPhonemeInhibition;
        private float _phonLexOrthLexExcitation;
        private float _phonLexOrthLexInhibition;
        private float _phonLexLateralInhibition;

        //Phoneme Level Parameters
        private float _phonemePhonLexExcitation;

        private float _phonemePhonLexInhibition;
        private float _phonemeLateralInhibition;
        private float _phonemeUnsupportedDecay;

        //GPC Route Parameters
        private float _gpcPhonemeExcitation;

        private float _gpcCriticalPhonology;
        private int _gpcOnset;

        // COUNT FIELDS
        // These avoid the need to recalculate the
        // same values over and over.
        private int _lettersLength; // number of letters + 1 for blankChar = 27 typically

        private int _phonemesLength; // number of phonemes + 1 for blankChar = 45 typically

        private int _printedWordsCount;
        // count of the printedWords list (must be recalculated before the start of each new simulated word.

        private int _spokenWordsLength; // length of the spokenWords array (number of known spoken words)
        private int _currentVFNode; // to avoid calculating slot*nVFeatures + currentFeature repeatedly
        private int _currentLetterNode; // to avoid calculating slot*lettersLength + currentLetter repeatedly
        private int _currentPhonemeNode; // to avoid calculating slot*phonemesLength + currentPhoneme repeatedly

        #endregion

        #region CONSTRUCTOR

        public LearningDRC(DirectoryInfo workingSubDir)
        {
            Timer.Start();
            LoadProperties();
            LoadDrc121Parameters();
            LoadLearningParameters();
            LoadLetters();
            LoadPhonemes();
            LoadVocabulary();
            LoadGPCs(_fileGPCRules);
            InitNetInputAndActivationLayerArrays();
            InitVF2LetterWeights();
            LoadOrthographicLexiconFromFile(workingSubDir);
            CreateCFSArrays();
            Timer.Stop();
            Console.WriteLine($"Buildtime: {Timer.ElapsedMilliseconds} ms");
            Timer.Reset();
        }


        private void LoadDrc121Parameters()
        {
            StreamReader streamR = null;
            try
            {
                streamR = _fileParameters.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the default.parameters file.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            string line;

            do
            {
                line = streamR.ReadLine();

                if (line == null || line == "" || line[0] == '#')
                    continue;

                var splitline = line.Split(' ');

                if (splitline.Length < 2)
                    continue;

                try
                {
                    switch (splitline[0])
                    {
                        case "ActivationRate":
                            _activationRate = float.Parse(splitline[1]);
                            break;
                        case "FrequencyScale":
                            _frequencyScale = float.Parse(splitline[1]);
                            break;
                        case "MinReadingPhonology":
                            _minReadingPhonology = float.Parse(splitline[1]);
                            break;
                        case "FeatureLetterExcitation":
                            _featureLetterExcitation = float.Parse(splitline[1]);
                            break;
                        case "FeatureLetterInhibition":
                            _featureLetterInhibition = -float.Parse(splitline[1]);
                            break;
                        case "LetterOrthlexExcitation":
                            _letterOrthLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "LetterOrthlexInhibition":
                            _letterOrthLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "LetterLateralInhibition":
                            _letterLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "OrthlexPhonlexExcitation":
                            _orthLexPhonLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "OrthlexPhonlexInhibition":
                            _orthLexPhonLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "OrthlexLetterExcitation":
                            _orthLexLetterExcitation = float.Parse(splitline[1]);
                            break;
                        case "OrthlexLetterInhibition":
                            _orthLexLetterInhibition = -float.Parse(splitline[1]);
                            break;
                        case "OrthlexLateralInhibition":
                            _orthLexLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonlexPhonemeExcitation":
                            _phonLexPhonemeExcitation = float.Parse(splitline[1]);
                            break;
                        case "PhonlexPhonemeInhibition":
                            _phonLexPhonemeInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonlexOrthlexExcitation":
                            _phonLexOrthLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "PhonlexOrthlexInhibition":
                            _phonLexOrthLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonlexLateralInhibition":
                            _phonLexLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonemePhonlexExcitation":
                            _phonemePhonLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "PhonemePhonlexInhibition":
                            _phonemePhonLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonemeLateralInhibition":
                            _phonemeLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonemeUnsupportedDecay":
                            _phonemeUnsupportedDecay = 1.0f - float.Parse(splitline[1]);
                            break;
                        case "GPCPhonemeExcitation":
                            _gpcPhonemeExcitation = float.Parse(splitline[1]);
                            break;
                        case "GPCCriticalPhonology":
                            _gpcCriticalPhonology = float.Parse(splitline[1]);
                            break;
                        case "GPCOnset":
                            _gpcOnset = int.Parse(splitline[1]);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("Had problems reading data out of default.parameters.");
                    Console.Write("Press a key to exit. ");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            } while (line != null);

            streamR.Close();
        }


        private void LoadLearningParameters()
        {
            StreamReader streamR = null;
            try
            {
                streamR = _fileLearningParameters.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the learning.parameters file.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            string line;

            do
            {
                line = streamR.ReadLine();

                if (line == null || line == "" || line[0] == '#')
                    continue;

                var splitline = line.Split(' ');

                if (splitline.Length < 2)
                    continue;

                try
                {
                    switch (splitline[0])
                    {
                        case "LearningOn":
                            _learningOn = int.Parse(splitline[1]) == 1;
                            break;
                        case "L2ONormalisation":
                            _l2ONormalisation = int.Parse(splitline[1]) == 1;
                            break;
                        case "OrthographicBlanks":
                            _orthographicBlanks = int.Parse(splitline[1]);
                            break;
                        case "NVFeatures":
                            _nvFeatures = int.Parse(splitline[1]);
                            break;
                        case "MaxCycles":
                            _maxCycles = int.Parse(splitline[1]);
                            break;
                        case "PrintActivations":
                            _printActivations = int.Parse(splitline[1]) == 1;
                            break;
                        case "MinimumActivationReport":
                            _minActivationReport = float.Parse(splitline[1]);
                            break;
                        case "PrintedWordRecognizedThreshold":
                            _printedWordRecogThreshold = float.Parse(splitline[1]);
                            break;
                        case "SpokenWordRecognizedThreshold":
                            _spokenWordRecogThreshold = float.Parse(splitline[1]);
                            break;
                        case "Semantic2PhonolexExcitation":
                            _semantic2PhonolexExcitation = float.Parse(splitline[1]);
                            break;
                        case "Semantic2PhonolexInhibition":
                            _semantic2PhonolexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "ContextInput2Semantic":
                            _contextInput2Semantic = float.Parse(splitline[1]);
                            break;
                        case "PrintedWordFrequencyMultiplier":
                            _printedWordFreqMultiplier = int.Parse(splitline[1]);
                            break;
                        case "NumberOfSemanticRepsActivated":
                            _numberOfSemanticRepsActivated = int.Parse(splitline[1]);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    Console.WriteLine("Had problems reading data out of learning.parameters.");
                    Console.Write("Press a key to exit. ");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            } while (line != null);

            streamR.Close();
        }


        private void LoadProperties()
        {
            StreamReader streamR = null;
            string line;

            try
            {
                streamR = _fileProperties.OpenText();
            }
            catch
            {
                Console.WriteLine("Could not open properties file.");
                Console.Write("Press a key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                if (line == null)
                    continue;

                var splitline = line.Split('=');

                switch (splitline[0])
                {
                    case "Name":
                        _name = splitline[1];
                        break;
                    case "Version":
                        _version = splitline[1];
                        break;
                    case "ReleaseDate":
                        _releaseDate = splitline[1];
                        break;
                    case "Creator":
                        _creator = splitline[1];
                        break;
                    case "Url":
                        _url = splitline[1];
                        break;
                    case "DefaultNumOrthAnalysisUnits":
                        _nLetterSlots = int.Parse(splitline[1]);
                        break;
                    case "DefaultNumPhonemeUnits":
                        _nPhonemeSlots = int.Parse(splitline[1]);
                        break;
                    default:
                        break;
                }
            } while (line != null);

            streamR.Close();
        }


        private void LoadLetters()
        {
            var lstLetterFeatures = new List<int[]>(); // list to load visual features for each letter from file
            var lstLetters = new List<char>(); // list to load letters from file
            var lstVowelStatus = new List<bool>(); // list to load letter vowel status from file

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = _fileLetters.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the letters file.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                if (line == null || line[0] == '#')
                    continue;

                lstLetterFeatures.Add(new int[_nvFeatures]);

                for (var i = 0; i < _nvFeatures; i++) // storing feature string for each letter
                    if (line[2 * i + 6] == '1')
                        lstLetterFeatures.Last()[i] = 1;
                    else
                        lstLetterFeatures.Last()[i] = 0;

                lstLetters.Add(line[0]);

                if (line[2] == 'C')
                {
                    lstVowelStatus.Add(false);
                }
                else if (line[2] == 'V')
                {
                    lstVowelStatus.Add(true);
                }
                else
                {
                    Console.WriteLine("Error loading letter vowel status from file. Exiting...");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
            } while (line != null);

            streamR.Close();

            // add in the blank slot char blankChar
            lstLetters.Add(Blankchar);
            lstLetterFeatures.Add(new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});

            //create letterfeature array and letter dictionaries, and store lengths
            _letters = new Dictionary<int, char>();
            _lettersReverseDictionary = new Dictionary<char, int>();
            _vowelStatus = new bool[lstVowelStatus.Count];
            _lettersLength = lstLetters.Count;
            _letterFeatures = new int[_lettersLength][];
            for (var j = 0; j < _lettersLength; j++)
            {
                _letterFeatures[j] = lstLetterFeatures[j];
                _letters.Add(j, lstLetters[j]);
                _lettersReverseDictionary.Add(lstLetters[j], j);
            }

            for (var j = 0; j < lstVowelStatus.Count; j++)
                _vowelStatus[j] = lstVowelStatus[j];
        }


        private void LoadPhonemes()
        {
            var lstPhonemes = new List<char>(); // list to load phonemes from file

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = _filePhonemes.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the phonemes file.");
                Console.Write("Press a key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                if (line == null)
                    continue;

                lstPhonemes.Add(line[0]);
            } while (line != null);

            lstPhonemes.Add(Blankchar);

            streamR.Close();

            //create phoneme array, and store length
            _phonemes = new Dictionary<int, char>();
            _phonemesReverseDictionary = new Dictionary<char, int>();
            _phonemesLength = lstPhonemes.Count;
            for (var j = 0; j < _phonemesLength; j++)
            {
                _phonemes.Add(j, lstPhonemes[j]);
                _phonemesReverseDictionary.Add(lstPhonemes[j], j);
            }
        }


        // Load vocabulary from file vocabulary.
        // Note: L-DRC uses the same vocabulary file as drc-1.2.1.
        // The printed words are ignored by L-DRC, just looking
        // to load the spoken words and their frequencies.
        // NOTE: drc-1.2.1 ignores words that are >8letters in length.
        // L-DRC, however, includes the spokenforms of these words in
        // its phonological lexicon (an oversight, since the 9-letter
        // orthographic forms are never presented during learning).
        private void LoadVocabulary()
        {
            // The format of each line in the vocabulary file is:
            // 0.printedWord 1.spokenWord 2."OP" 3.printedFreq 4.spokenFreq

            // list to load spoken word vocabulary
            var loadedSpokenWords = new Dictionary<string, int>();
            var reverseLoadedSpokenWords = new Dictionary<int, string>();
            // list to load spoken word frequencies
            var lstSpokenWordFreq = new List<int>();

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = _fileVocab.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the vocabulary file.");
                Console.Write("Press a key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();

                if (line == null)
                    continue;

                var splitline = line.Split(' ');

                if (loadedSpokenWords.ContainsKey(splitline[1]))
                {
                    var indexSpoken = loadedSpokenWords[splitline[1]];
                    lstSpokenWordFreq[indexSpoken] += int.Parse(splitline[4]);
                }
                else
                {
                    loadedSpokenWords.Add(splitline[1], loadedSpokenWords.Count);
                    reverseLoadedSpokenWords.Add(reverseLoadedSpokenWords.Count, splitline[1]);
                    lstSpokenWordFreq.Add(int.Parse(splitline[4]));
                }
            } while (line != null);
            streamR.Close();

            _spokenWords = new string[loadedSpokenWords.Count];
            _spokenWordsLength = _spokenWords.Length;
            _spokenWordFreq = new int[lstSpokenWordFreq.Count];

            for (var m = 0; m < _spokenWordsLength; m++)
            {
                _spokenWords[m] = reverseLoadedSpokenWords[m];
                _spokenWordFreq[m] = lstSpokenWordFreq[m];
            }

            // find max spoken word frequency
            _maxSpokenWordFreq = GetMaxIntFromArray(_spokenWordFreq);

            // Need to add a single end of word character to each of the spoken words.
            for (var m = 0; m < _spokenWords.Length; m++)
                if (_spokenWords[m].Length < _nLetterSlots)
                    _spokenWords[m] = _spokenWords[m] + "+";
        }


        private static int GetMaxIntFromArray(IList<int> ary)
        {
            if (ary == null) return 0;
            if (ary.Count == 0) return 0;

            var max = ary[0];

            for (var i = 1; i < ary.Count; i++)
                if (ary[i] > max)
                    max = ary[i];
            return max;
        }


        public void LoadGPCs(FileInfo gpcFile)
        {
            var lstBodyRules = new List<GPCRule>();
            var lstMultiRules = new List<GPCRule>();
            var lstTwoRules = new List<GPCRule>();
            var lstContextRules = new List<GPCRule>();
            var lstMPhonRules = new List<GPCRule>();
            var lstSingleRules = new List<GPCRule>();
            var lstOutRules = new List<GPCRule>();

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = gpcFile.OpenText();
            }
            catch
            {
                Console.WriteLine("Could not open gpcrules file.");
                Console.Write("Press a key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                if (line == null || line == "")
                    continue;

                var splitline = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

                switch (splitline[1])
                {
                    case "body":
                        lstBodyRules.Add(new GPCRule(splitline));
                        break;
                    case "multi":
                        lstMultiRules.Add(new GPCRule(splitline));
                        break;
                    case "two":
                        lstTwoRules.Add(new GPCRule(splitline));
                        break;
                    case "mphon":
                        lstMPhonRules.Add(new GPCRule(splitline));
                        break;
                    case "cs":
                        lstContextRules.Add(new GPCRule(splitline));
                        break;
                    case "sing":
                        lstSingleRules.Add(new GPCRule(splitline));
                        break;
                    case "out":
                        lstOutRules.Add(new GPCRule(splitline));
                        break;
                    default:
                        break;
                }
            } while (line != null);
            streamR.Close();

            //Create arrays from the lists,
            //on the assumption they are quicker to work with.

            _bodyRules = new GPCRule[lstBodyRules.Count];
            for (var z = 0; z < _bodyRules.Length; z++)
                _bodyRules[z] = lstBodyRules[z];

            _multiRules = new GPCRule[lstMultiRules.Count];
            for (var z = 0; z < _multiRules.Length; z++)
                _multiRules[z] = lstMultiRules[z];

            _twoRules = new GPCRule[lstTwoRules.Count];
            for (var z = 0; z < _twoRules.Length; z++)
                _twoRules[z] = lstTwoRules[z];

            _contextRules = new GPCRule[lstContextRules.Count];
            for (var z = 0; z < lstContextRules.Count; z++)
                _contextRules[z] = lstContextRules[z];

            _mphonRules = new GPCRule[lstMPhonRules.Count];
            for (var z = 0; z < _mphonRules.Length; z++)
                _mphonRules[z] = lstMPhonRules[z];

            // Add a rule for a blank letter activating the 
            // blank phoneme in the final position.
            lstSingleRules.Add(new GPCRule(new string[6] {"e", "sing", "+", "+", "u", "1.0"}));
            _singleRules = new GPCRule[lstSingleRules.Count];
            for (var z = 0; z < lstSingleRules.Count; z++)
                _singleRules[z] = lstSingleRules[z];

            _outRules = new GPCRule[lstOutRules.Count];
            for (var z = 0; z < _outRules.Length; z++)
                _outRules[z] = lstOutRules[z];
        }


        private void InitNetInputAndActivationLayerArrays()
        {
            _outVisualFeatureLayer1 = new float[_nLetterSlots * _nvFeatures];
            _outVisualFeatureLayer0 = new float[_nLetterSlots * _nvFeatures];

            _netInputLetterLayer = new float[_nLetterSlots * _lettersLength];
            _netInputPhonolexLayer = new float[_spokenWordsLength];
            _netInputPhonemeLayer = new float[_nPhonemeSlots * _phonemesLength];

            // Note: ortholex netinput and activation arrays are resized
            // after every simulation to take account of any orthographic learning,
            // which is done in ResizeOrtholexArrays, not here.

            _actLetterLayer = new float[_nLetterSlots * _lettersLength];
            _actPhonolexLayer = new float[_spokenWordsLength];
            _actPhonemeLayer = new float[_nPhonemeSlots * _phonemesLength];
        }


        private void InitVF2LetterWeights()
        {
            _vF0ToLetterWeights = new float[_nLetterSlots * _nvFeatures][];
            for (var i = 0; i < _vF0ToLetterWeights.Length; i++)
                _vF0ToLetterWeights[i] = new float[_nLetterSlots * _lettersLength];

            _vF1ToLetterWeights = new float[_nLetterSlots * _nvFeatures][];
            for (var i = 0; i < _vF1ToLetterWeights.Length; i++)
                _vF1ToLetterWeights[i] = new float[_nLetterSlots * _lettersLength];

            // Loop across letter slots
            for (var i = 0; i < _nLetterSlots; i++)
                // Loop across the number of letters in each slot (27)
            for (var j = 0; j < _lettersLength; j++)
            {
                _currentLetterNode = i * _lettersLength + j;

                // Loop across the number of visual features in each slot (14).
                for (var k = 0; k < _nvFeatures; k++)
                {
                    _currentVFNode = i * _nvFeatures + k;

                    if (_letterFeatures[j][k] == 1)
                    {
                        _vF1ToLetterWeights[_currentVFNode][_currentLetterNode] = _featureLetterExcitation;
                        _vF0ToLetterWeights[_currentVFNode][_currentLetterNode] = _featureLetterInhibition;
                    }
                    else
                    {
                        _vF1ToLetterWeights[_currentVFNode][_currentLetterNode] = _featureLetterInhibition;
                        _vF0ToLetterWeights[_currentVFNode][_currentLetterNode] = _featureLetterExcitation;
                    }
                } // nvFeatures loop
            } // lettersLength loop
        }


        public void ClearOrthographicLexicon()
        {
            _printedWords = new List<string>();
            _printedWordFreq = new List<int>();
            _spokenWordsForEachPrintedWord = new List<List<int>>();
            _printedWordsForEachSpokenWord = new List<int>[_spokenWordsLength];
            _printedWordsCount = 0;
            for (var m = 0; m < _spokenWordsLength; m++)
                _printedWordsForEachSpokenWord[m] = new List<int>();
        }


        // Format for orthographicknowledge.txt file:
        // 1. "WORD"  2. printedword  3. printedwrd index  4. printedwrd freq  5. spokenword  6. spokenwrd index
        public void LoadOrthographicLexiconFromFile(DirectoryInfo workingSubDir)
        {
            StreamReader streamR;
            string line;
            var fileOrthographicKnowledge =
                new FileInfo(Path.Combine(workingSubDir.FullName, OrthographicKnowledgeFilename));
            _printedWords = new List<string>();
            _printedWordFreq = new List<int>();
            _spokenWordsForEachPrintedWord = new List<List<int>>();
            _printedWordsForEachSpokenWord = new List<int>[_spokenWordsLength];

            for (var m = 0; m < _spokenWordsLength; m++)
                _printedWordsForEachSpokenWord[m] = new List<int>();

            try
            {
                streamR = fileOrthographicKnowledge.OpenText();
            }

            catch
            {
                Console.WriteLine("No 'orthographicknowledge.txt' file found.");
                Console.Write("Creating new network... ");
                return;
            }

            do
            {
                line = streamR.ReadLine();

                if (line == null)
                    continue;

                var splitline = line.Split(' ');

                if (splitline[0] == "#")
                    continue;

                switch (splitline[0])
                {
                    case "WORD":
                        try
                        {
                            _printedWords.Add(splitline[1]);
                            _printedWordFreq.Add(int.Parse(splitline[3]));

                            _spokenWordsForEachPrintedWord.Add(new List<int>());

                            var sWord = 5;

                            do
                            {
                                _printedWordsForEachSpokenWord[int.Parse(splitline[sWord])].Add(int.Parse(splitline[2]));
                                _spokenWordsForEachPrintedWord[_spokenWordsForEachPrintedWord.Count - 1]
                                    .Add(int.Parse(splitline[sWord]));
                                sWord += 2;
                            } while (sWord < splitline.Length);
                        }
                        catch
                        {
                            Console.WriteLine("Problem loading known printed words.");
                            Console.Write("Press a key to exit. ");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                        break;

                    default:
                        break;
                }
            } while (line != null);

            _printedWordsCount = _printedWords.Count;
            streamR.Close();
        }


        public void CreateCFSArrays()
        {
            _printedCFS = new float[_printedWordsCount];
            _spokenCFS = new float[_spokenWordsLength];

            _maxPrintedWordFreq = GetMaxIntFromArray(_printedWordFreq.ToArray());

            for (var l = 0; l < _printedWordsCount; l++)
                _printedCFS[l] = (float) (Math.Log10(_printedWordFreq[l] + 1) / Math.Log10(_maxPrintedWordFreq + 1) - 1) *
                                _frequencyScale;

            _maxSpokenWordFreq = GetMaxIntFromArray(_spokenWordFreq.ToArray());

            for (var m = 0; m < _spokenWordsLength; m++)
                _spokenCFS[m] = (float) (Math.Log10(_spokenWordFreq[m] + 1) / Math.Log10(_maxSpokenWordFreq + 1) - 1) *
                               _frequencyScale;
        }

        #endregion

        #region GENERAL METHODS

        public string[] Simulate(string newStimulus, string contextualInput, DirectoryInfo workingSubDir)
        {
            var cycles = 0;
            var output = new string[2 + _numberOfSemanticRepsActivated];
            bool completed;

            _stimulus = newStimulus;

            // Determine length of stimulus + blanks, for normalisation purposes.
            switch (_orthographicBlanks)
            {
                case 0:
                    _stimulusLength = _stimulus.Length;
                    break;
                case 1:
                    if (_stimulus.Length < _nLetterSlots) // don't add 1 if the stimuli already takes up all slots.
                        _stimulusLength = _stimulus.Length + 1;
                    else
                        _stimulusLength = _stimulus.Length;
                    break;
                default:
                    _stimulusLength = _nLetterSlots;
                    break;
            }

            // Set L2O normalisation multiplier, based on stimulus length and whether or not normalisation is being used.
            if (_l2ONormalisation == false)
                _normMultiplier = 1;
            else
                _normMultiplier = _nLetterSlots / (float) _stimulusLength;

            // Called at the start of every new stimulus simulation, to account for any learning
            // that may have been done with previous stimuli.
            ResizeOrtholexArrays();

            ResetActivationsToZero();
            ResetGPCRoute();

            ClampVisualFeatures();

            //Create the correct phonological lexicon context by determining what
            //random semantic nodes to activate, and adding a blank to the input correct context.
            if (_numberOfSemanticRepsActivated == 0 || _contextInput2Semantic <= 0.0001)
            {
                _context = new string[1];
                _context[0] = "+";
            }
            else
            {
                _context = new string[_numberOfSemanticRepsActivated];
                _context[0] = string.Concat(contextualInput, "+");
                for (var wrd = 1; wrd < _context.Length; wrd++)
                {
                    var randomWord = _rnd.Next(_spokenWords.Length);
                    _context[wrd] = _spokenWords[randomWord];

                    // make sure all context words are exclusive.
                    for (var wrdCheck = 0; wrdCheck < wrd; wrdCheck++)
                        if (_context[wrd] == _context[wrdCheck])
                            wrd = wrd - 1;
                }
            }

            // Main loop to run the reading alound simulation
            do
            {
                cycles++;

                ProcessSingleCycle(cycles);
                completed = CheckCompletion() || cycles == _maxCycles;

                // terminate simulation if maxCycles reached.
            } while (completed == false);

            // Learning is done here
            if (_learningOn)
            {
                Learning(workingSubDir);
                SaveLearnedOrthographicVocabulary(workingSubDir);
            }

            // Return output of reading aloud simulation
            output[0] = cycles.ToString();
            output[1] = GetOutput();

            for (var i = 0; i < _numberOfSemanticRepsActivated; i++)
            {
                if (_numberOfSemanticRepsActivated == 0 || _contextInput2Semantic <= 0.0001) break;
                output[2 + i] = _context[i];
            }

            return output;
        }


        /// <summary>
        ///     Resize netInputOrtholexLayer and actOrtholexLayer arrays
        ///     based on whether or not new words were added in previous sim.
        /// </summary>
        private void ResizeOrtholexArrays()
        {
            // Important: recalculating printedWordsCount
            // This needs to be done at the start of each new
            // simulation, since a new printedWord may have been
            // learned last simulation.
            _printedWordsCount = _printedWords.Count;

            //Important: resize ortholex netinput and activation arrays
            // at the start of each new simulation
            _netInputOrtholexLayer = new float[_printedWordsCount];
            _actOrtholexLayer = new float[_printedWordsCount];
        }

        private void ResetActivationsToZero()
        {
            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
                _actLetterLayer[i * _lettersLength + j] = 0f;

            for (var m = 0; m < _spokenWordsLength; m++)
                _actPhonolexLayer[m] = 0f;

            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
                _actPhonemeLayer[i * _phonemesLength + j] = 0f;
            _actSemanticNode = 0f;
        }

        private void ClampVisualFeatures()
        {
            for (var i = 0; i < _stimulus.Length; i++)
            {
                var index = _lettersReverseDictionary[_stimulus[i]];

                for (var k = 0; k < _nvFeatures; k++)
                {
                    _currentVFNode = i * _nvFeatures + k;

                    if (_letterFeatures[index][k] == 1)
                    {
                        _outVisualFeatureLayer1[_currentVFNode] = 1;
                        _outVisualFeatureLayer0[_currentVFNode] = 0;
                    }
                    else // if letterFeatures[index][j] == 0
                    {
                        _outVisualFeatureLayer1[_currentVFNode] = 0;
                        _outVisualFeatureLayer0[_currentVFNode] = 1;
                    }
                }
            }

            //set any empty slots to blanks, depending on how many blanks to include
            // otherwise, clear visual features.

            // No blanks case
            if (_orthographicBlanks == 0)
                // clear any empty slots of any visual feature activity
            {
                if (_stimulus.Length < _nLetterSlots)
                    for (var i = _stimulus.Length; i < _nLetterSlots; i++)
                    for (var k = 0; k < _nvFeatures; k++)
                    {
                        _outVisualFeatureLayer1[i * _nvFeatures + k] = 0;
                        _outVisualFeatureLayer0[i * _nvFeatures + k] = 0;
                    }
            }

            // 1 blank case
            else if (_orthographicBlanks == 1)
            {
                // activate visual features for a single extra blank
                // and clear visual features in any remaining slots
                // of all activity
                if (_stimulus.Length < _nLetterSlots)
                    for (var i = _stimulus.Length; i < _nLetterSlots; i++)
                        if (i == _stimulus.Length)
                            for (var k = 0; k < _nvFeatures; k++)
                            {
                                _outVisualFeatureLayer1[i * _nvFeatures + k] = 0;
                                _outVisualFeatureLayer0[i * _nvFeatures + k] = 1;
                            }
                        else
                            for (var k = 0; k < _nvFeatures; k++)
                            {
                                _outVisualFeatureLayer1[i * _nvFeatures + k] = 0;
                                _outVisualFeatureLayer0[i * _nvFeatures + k] = 0;
                            }
            }

            // enough blanks to make up nLetterSlots case
            else if (_orthographicBlanks == 2)
            {
                // activate visual features for a blank char in all remaining slots
                if (_stimulus.Length < _nLetterSlots)
                    for (var i = _stimulus.Length; i < _nLetterSlots; i++)
                    for (var k = 0; k < _nvFeatures; k++)
                    {
                        _outVisualFeatureLayer1[i * _nvFeatures + k] = 0;
                        _outVisualFeatureLayer0[i * _nvFeatures + k] = 1;
                    }
            }
        }


        /// <summary>
        ///     Called by the Simulate method. Used to process a single cycle
        ///     of the simulation, calculating net inputs, epsilons and
        ///     activations across the whole model for a single cycle.
        /// </summary>
        /// <param name="cycles"></param>
        private void ProcessSingleCycle(int cycles)
        {
            ProcessNetInputs();
            ProcessGPCRoute(cycles);
            ProcessNewActivations();
            ClipActivations();
            if (_printActivations)
                PrintRelevantActivationsToFile(cycles);
        }


        /// <summary>
        ///     Gets the output phoneme string by choosing
        ///     the maximally activated phoneme from each slot.
        /// </summary>
        /// <returns>output phoneme string</returns>
        private string GetOutput()
        {
            var output = new StringBuilder();

            for (var i = 0; i < _nPhonemeSlots; i++)
            {
                var maxPhonemeIndex = 0;
                var maxPhonemeAct = 0f;

                for (var j = 0; j < _phonemesLength; j++)
                {
                    _currentPhonemeNode = i * _phonemesLength + j;

                    if (_actPhonemeLayer[_currentPhonemeNode] > maxPhonemeAct)
                    {
                        maxPhonemeAct = _actPhonemeLayer[_currentPhonemeNode];
                        maxPhonemeIndex = j;
                    }
                }

                // check if end of word, and, if so, return output
                if (maxPhonemeIndex == _phonemesReverseDictionary[Blankchar])
                    return output.ToString();
                // else add maximally activated phoneme in the slot to the
                // output string, but add a space if there is no activated phoneme
                output.Append(maxPhonemeAct == 0f ? ' ' : _phonemes[maxPhonemeIndex]);
            }
            return output.ToString();
        }


        /// <summary>
        ///     Checks maximally activated phonemes in each slot,
        ///     Returns true if each is above minReadingPhonology.
        /// </summary>
        /// <returns>Returns true/false, simulation complete</returns>
        private bool CheckCompletion()
        {
            for (var i = 0; i < _nPhonemeSlots; i++)
            {
                var maxPhonemeIndex = 0;
                var maxPhonemeAct = 0f;

                for (var j = 0; j < _phonemesLength; j++)
                {
                    _currentPhonemeNode = i * _phonemesLength + j;

                    if (_actPhonemeLayer[_currentPhonemeNode] > maxPhonemeAct)
                    {
                        maxPhonemeAct = _actPhonemeLayer[_currentPhonemeNode];
                        maxPhonemeIndex = j;
                    }
                }

                // check if max phoneme in slot is above minReadingPhonology.
                // If not, return false.
                if (maxPhonemeAct < _minReadingPhonology)
                    return false;

                // if at end-of-word phoneme character, and all slots so far
                // have been above minreadingphonology, then break, return true.
                if (_phonemes[maxPhonemeIndex] == Blankchar)
                    break;
            }
            return true;
        }


        public void PrintParametersToFile(FileInfo printFile)
        {
            StreamWriter streamW = null;

            try
            {
                streamW = new StreamWriter(printFile.FullName, true);
            }
            catch
            {
                Console.WriteLine("There was a problem creating or opening the file for printing parameters.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            streamW.WriteLine($"NumberOfLetterSlots:\t{_nLetterSlots}");
            streamW.WriteLine($"NumberOfPhonemeSlots:\t{_nPhonemeSlots}");
            streamW.WriteLine($"MaxPrintedWordFrequency:\t{_maxPrintedWordFreq}");
            streamW.WriteLine($"MaxSpokenWordFrequency:\t{_maxSpokenWordFreq}");
            streamW.WriteLine($"LearningOn:\t{_learningOn}");
            streamW.WriteLine($"Letter2OLNormalisationOn:\t{_l2ONormalisation}");
            streamW.WriteLine($"OrthographicBlanksCondition:\t{_orthographicBlanks}");
            streamW.WriteLine($"VisualFeaturesPerLetter:\t{_nvFeatures}");
            streamW.WriteLine($"MaximumCycles:\t{_maxCycles}");
            streamW.WriteLine($"ThreholdForPrintedWordRecognition:\t{_printedWordRecogThreshold}");
            streamW.WriteLine($"ThreholdForSpokenWordRecognition:\t{_spokenWordRecogThreshold}");
            streamW.WriteLine($"Semantic2PhonolexExcitation:\t{_semantic2PhonolexExcitation}");
            streamW.WriteLine($"Semantic2PhonolexInhibition:\t{_semantic2PhonolexInhibition}");
            streamW.WriteLine($"ContextInput2Semantic(normalised):\t{_contextInput2Semantic}");
            streamW.WriteLine($"NumberOfSemanticRepresentationsActivated:\t{_numberOfSemanticRepsActivated}");
            streamW.WriteLine($"PrintedWordFrequencyTrainingMultiplier:\t{_printedWordFreqMultiplier}");
            streamW.WriteLine($"ActivationRate:\t{_activationRate}");
            streamW.WriteLine($"FrequencyScaling:\t{_frequencyScale}");
            streamW.WriteLine($"MinimumPhonologyForReadingAloud:\t{_minReadingPhonology}");
            streamW.WriteLine($"Feature2LetterExcitation:\t{_featureLetterExcitation}");
            streamW.WriteLine($"Feature2LetterInhibition:\t{_featureLetterInhibition}");
            streamW.WriteLine($"Letter2OrtholexExcitation:\t{_letterOrthLexExcitation}");
            streamW.WriteLine($"Letter2OrtholexInhibition:\t{_letterOrthLexInhibition}");
            streamW.WriteLine($"LetterLateralInhibition:\t{_letterLateralInhibition}");
            streamW.WriteLine($"Ortholex2PhonolexExcitation:\t{_orthLexPhonLexExcitation}");
            streamW.WriteLine($"Ortholex2PhonolexInhibition:\t{_orthLexPhonLexInhibition}");
            streamW.WriteLine($"Ortholex2LetterExcitation:\t{_orthLexLetterExcitation}");
            streamW.WriteLine($"Ortholex2LetterInhibition:\t{_orthLexLetterInhibition}");
            streamW.WriteLine($"OrtholexLateralInhibition:\t{_orthLexLateralInhibition}");
            streamW.WriteLine($"Phonolex2PhonemeExcitation:\t{_phonLexPhonemeExcitation}");
            streamW.WriteLine($"Phonolex2PhonemeInhibition:\t{_phonLexPhonemeInhibition}");
            streamW.WriteLine($"PhonoLlex2OrtholexExcitation:\t{_phonLexOrthLexExcitation}");
            streamW.WriteLine($"PhonoLlex2OrtholexInhibition:\t{_phonLexOrthLexInhibition}");
            streamW.WriteLine($"PhonoLlexLateralInhibition:\t{_phonLexLateralInhibition}");
            streamW.WriteLine($"Phoneme2PhonolexExcitation:\t{_phonemePhonLexExcitation}");
            streamW.WriteLine($"Phoneme2PhonolexInhibition:\t{_phonemePhonLexInhibition}");
            streamW.WriteLine($"PhonemeLateralInhibition:\t{_phonemeLateralInhibition}");
            streamW.WriteLine($"PhonemeUnsupportedDecay:\t{_phonemeUnsupportedDecay}");
            streamW.WriteLine($"GPC2PhonemeExcitation:\t{_gpcPhonemeExcitation}");
            streamW.WriteLine($"GPCCriticalPhonology:\t{_gpcCriticalPhonology}");
            streamW.WriteLine($"GPCOnset:\t{_gpcOnset}");

            streamW.Close();
        }


        /// <summary>
        ///     Method prints activations of nodes that are above a particular threshhold
        ///     each cycle. Also reports GPC route activity.
        /// </summary>
        /// <param name="cycles"></param>
        private void PrintRelevantActivationsToFile(int cycles)
        {
            StreamWriter streamW = null;

            try
            {
                streamW = new StreamWriter(_fileActs.FullName, true);
            }
            catch
            {
                Console.WriteLine("There was a problem creating or opening the activations.txt file.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
                if (_actLetterLayer[i * _lettersLength + j] > _minActivationReport)
                    streamW.WriteLine("Cycle{0}\tL{1} {2}\t{3}", cycles, i, _actLetterLayer[i * _lettersLength + j],
                        _letters[j]);

            for (var l = 0; l < _printedWordsCount; l++)
                if (_actOrtholexLayer[l] > _minActivationReport)
                    streamW.WriteLine("Cycle{0}\tOrth {1}\t{2}", cycles, _actOrtholexLayer[l], _printedWords[l]);

            for (var m = 0; m < _spokenWordsLength; m++)
                if (_actPhonolexLayer[m] > _minActivationReport)
                    streamW.WriteLine("Cycle{0}\tPhon {1}\t{2}", cycles, _actPhonolexLayer[m], _spokenWords[m]);

            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
                if (_actPhonemeLayer[i * _phonemesLength + j] > _minActivationReport)
                    streamW.WriteLine("Cycle{0}\tP{1} {2}\t{3}", cycles, i, _actPhonemeLayer[i * _phonemesLength + j],
                        _phonemes[j]);
            streamW.Close();
        }

        public float GetContextInput2SemanticValue()
        {
            return _contextInput2Semantic;
        }

        public int GetNumberOfSemanticRepsActivated()
        {
            return _numberOfSemanticRepsActivated;
        }

        public void SetParameter(string parameterName, float parameterValue)
        {
            switch (parameterName)
            {
                case "ActivationRate":
                    _activationRate = parameterValue;
                    break;
                case "FrequencyScale":
                    _frequencyScale = parameterValue;
                    break;
                case "MinReadingPhonology":
                    _minReadingPhonology = parameterValue;
                    break;
                case "FeatureLetterExcitation":
                    _featureLetterExcitation = parameterValue;
                    break;
                case "FeatureLetterInhibition":
                    _featureLetterInhibition = -parameterValue;
                    break;
                case "LetterOrthlexExcitation":
                    _letterOrthLexExcitation = parameterValue;
                    break;
                case "LetterOrthlexInhibition":
                    _letterOrthLexInhibition = -parameterValue;
                    break;
                case "LetterLateralInhibition":
                    _letterLateralInhibition = -parameterValue;
                    break;
                case "OrthlexPhonlexExcitation":
                    _orthLexPhonLexExcitation = parameterValue;
                    break;
                case "OrthlexPhonlexInhibition":
                    _orthLexPhonLexInhibition = -parameterValue;
                    break;
                case "OrthlexLetterExcitation":
                    _orthLexLetterExcitation = parameterValue;
                    break;
                case "OrthlexLetterInhibition":
                    _orthLexLetterInhibition = -parameterValue;
                    break;
                case "OrthlexLateralInhibition":
                    _orthLexLateralInhibition = -parameterValue;
                    break;
                case "PhonlexPhonemeExcitation":
                    _phonLexPhonemeExcitation = parameterValue;
                    break;
                case "PhonlexPhonemeInhibition":
                    _phonLexPhonemeInhibition = -parameterValue;
                    break;
                case "PhonlexOrthlexExcitation":
                    _phonLexOrthLexExcitation = parameterValue;
                    break;
                case "PhonlexOrthlexInhibition":
                    _phonLexOrthLexInhibition = -parameterValue;
                    break;
                case "PhonlexLateralInhibition":
                    _phonLexLateralInhibition = -parameterValue;
                    break;
                case "PhonemePhonlexExcitation":
                    _phonemePhonLexExcitation = parameterValue;
                    break;
                case "PhonemePhonlexInhibition":
                    _phonemePhonLexInhibition = -parameterValue;
                    break;
                case "PhonemeLateralInhibition":
                    _phonemeLateralInhibition = -parameterValue;
                    break;
                case "PhonemeUnsupportedDecay":
                    _phonemeUnsupportedDecay = 1.0f - parameterValue;
                    break;
                case "GPCPhonemeExcitation":
                    _gpcPhonemeExcitation = parameterValue;
                    break;
                case "GPCCriticalPhonology":
                    _gpcCriticalPhonology = parameterValue;
                    break;
                case "GPCOnset":
                    _gpcOnset = (int) parameterValue;
                    break;
                case "LearningOn":
                    _learningOn = (int) parameterValue == 1;
                    break;
                case "PrintActivations":
                    _printActivations = (int) parameterValue == 1;
                    break;
                case "L2ONormalisation":
                    _l2ONormalisation = (int) parameterValue == 1;
                    break;
                case "OrthographicBlanks":
                    _orthographicBlanks = (int) parameterValue;
                    break;
                case "NVFeatures":
                    _nvFeatures = (int) parameterValue;
                    break;
                case "MaxCycles":
                    _maxCycles = (int) parameterValue;
                    break;
                case "PrintedWordRecognizedThreshold":
                    _printedWordRecogThreshold = parameterValue;
                    break;
                case "SpokenWordRecognizedThreshold":
                    _spokenWordRecogThreshold = parameterValue;
                    break;
                case "Semantic2PhonolexExcitation":
                    _semantic2PhonolexExcitation = parameterValue;
                    break;
                case "Semantic2PhonolexInhibition":
                    _semantic2PhonolexInhibition = -parameterValue;
                    break;
                case "ContextInput2Semantic":
                    _contextInput2Semantic = parameterValue;
                    break;
                case "PrintedWordFrequencyMultiplier":
                    _printedWordFreqMultiplier = (int) parameterValue;
                    break;
                case "NumberOfSemanticRepsActivated":
                    _numberOfSemanticRepsActivated = (int) parameterValue;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region LEXICAL ROUTE METHODS

        /// <summary>
        ///     Calculate net inputs for a single cycle.
        /// </summary>
        private void ProcessNetInputs()
        {
            var netOrthlexToLetterInhibition = 0f;

            var netPhonemeToPhonlexInhibition = new float[_nPhonemeSlots];
            var netPhonLexToPhonemeInhibition = new float[_nPhonemeSlots];

            var netLetterLatInhibition = new float[_nLetterSlots];
            var netPhonolexLatInhibition = 0f;
            var netPhonemeLatInhibition = new float[_nPhonemeSlots];

            // RESET NET INPUTS, MUST BE DONE EVERY CYCLE
            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
                _netInputLetterLayer[i * _lettersLength + j] = 0f;

            for (var l = _netInputOrtholexLayer.Length - 1; l >= 0; l--)
                _netInputOrtholexLayer[l] = 0f;

            for (var m = 0; m < _spokenWordsLength; m++)
                _netInputPhonolexLayer[m] = 0f;

            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
                _netInputPhonemeLayer[i * _phonemesLength + j] = 0f;

            // ADD IN WORD FREQUENCY CONTRIBUTIONS
            for (var l = 0; l < _printedWordsCount; l++)
                _netInputOrtholexLayer[l] += _printedCFS[l];

            for (var m = 0; m < _spokenWordsLength; m++)
                _netInputPhonolexLayer[m] += _spokenCFS[m];

            // PROCESS NET INPUT FROM THE VF LAYER
            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
            {
                _currentLetterNode = i * _lettersLength + j;

                for (var k = 0; k < _nvFeatures; k++)
                {
                    _currentVFNode = i * _nvFeatures + k;

                    _netInputLetterLayer[_currentLetterNode] +=
                        _outVisualFeatureLayer1[_currentVFNode] * _vF1ToLetterWeights[_currentVFNode][_currentLetterNode] +
                        _outVisualFeatureLayer0[_currentVFNode] * _vF0ToLetterWeights[_currentVFNode][_currentLetterNode];
                }
            }

            // PROCESS NET INPUT FROM THE LETTER TO ORTHOLEX LEVEL, AND VICE VERSA.

            // First, calculate the net L2Oinhibition by iterating across every letter node activation
            var netLetterToOrthlexInhibition = _actLetterLayer.Sum(t => t * _letterOrthLexInhibition);

            // Next iterate over every OL word node...
            for (var l = 0; l < _printedWordsCount; l++)
            {
                // and add the netL2Oinhibition to its net input...
                _netInputOrtholexLayer[l] += netLetterToOrthlexInhibition;

                // then, iterate across the letters in the current OL word node at position 'l'
                for (var i = 0; i < _nLetterSlots; i++)
                {
                    // stop if the OL word length is < number of letter slots
                    if (i >= _printedWords[l].Length)
                        break;
                    // find the position of letter 'i' in OL word node 'l' in the letter layer activation array.
                    var letterLayerAbsolutePosition = i * _lettersLength + _lettersReverseDictionary[_printedWords[l][i]];

                    // and add the excitation from this node, and take away the inhibition from this node
                    // (because the inhibition was added when calculating net L2Oinhibition).
                    // The excitation is the learned value, and is found in the excitationsL2O list,
                    // at OL word node 'l', and position 'i'.
                    _netInputOrtholexLayer[l] += _actLetterLayer[letterLayerAbsolutePosition]
                                                * (_letterOrthLexExcitation * _normMultiplier - _letterOrthLexInhibition);

                    // Do same for O2L excitation
                    _netInputLetterLayer[letterLayerAbsolutePosition] +=
                        _actOrtholexLayer[l] * (_orthLexLetterExcitation - _orthLexLetterInhibition);
                }
            }

            // By this point, the net input from the Letter to the OL layer has been calculated,
            // and the excitations from L2O and O2L have been calculated.
            // Now just need to calculate the rest of the O2L inhibition.

            // calculate net O2L inhibition by iterating over all OL word nodes
            for (var l = 0; l < _printedWordsCount; l++)
                netOrthlexToLetterInhibition += _actOrtholexLayer[l] * _orthLexLetterInhibition;

            // iterate across every letter node, and add the net O2L inhibition to each node's net input.
            for (var letterLayerAbsolutePosition = 0;
                letterLayerAbsolutePosition < _netInputLetterLayer.Length;
                letterLayerAbsolutePosition++)
                _netInputLetterLayer[letterLayerAbsolutePosition] += netOrthlexToLetterInhibition;

            // LATERAL INHIBITION IN THE LETTER LAYER
            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
            {
                _currentLetterNode = i * _lettersLength + j;
                netLetterLatInhibition[i] += _actLetterLayer[_currentLetterNode] * _letterLateralInhibition;
            }

            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
            {
                _currentLetterNode = i * _lettersLength + j;
                _netInputLetterLayer[_currentLetterNode] +=
                    netLetterLatInhibition[i] - _actLetterLayer[_currentLetterNode] * _letterLateralInhibition;
            }

            // NET INPUT FROM OL to PL and VICE VERSA.
            var netOtoPInhibition = _actOrtholexLayer.Sum(t => t * _orthLexPhonLexInhibition);
            var netPtoOInhibition = _actPhonolexLayer.Sum(t => t * _phonLexOrthLexInhibition);

            for (var l = 0; l < _printedWordsCount; l++)
            {
                _netInputOrtholexLayer[l] += netPtoOInhibition;
                for (var x = 0; x < _spokenWordsForEachPrintedWord[l].Count; x++)
                {
                    var currentSpokenWord = _spokenWordsForEachPrintedWord[l][x];
                    // For each spoken word linked to a printed word, check that the printed
                    // word is also linked to the spoken word
                    var currentPrintedWordIndex = -1;
                    for (var x1 = 0; x1 < _printedWordsForEachSpokenWord[currentSpokenWord].Count; x1++)
                        if (_printedWordsForEachSpokenWord[currentSpokenWord][x1] == l)
                            currentPrintedWordIndex = x1;

                    if (currentPrintedWordIndex != -1)
                        _netInputOrtholexLayer[l] += _actPhonolexLayer[_spokenWordsForEachPrintedWord[l][x]] *
                                                    (_phonLexOrthLexExcitation - _phonLexOrthLexInhibition);
                }
            }

            for (var m = 0; m < _spokenWordsLength; m++)
            {
                _netInputPhonolexLayer[m] += netOtoPInhibition;
                for (var x = 0; x < _printedWordsForEachSpokenWord[m].Count; x++)
                {
                    var currentPrintedWord = _printedWordsForEachSpokenWord[m][x];
                    // For each printed word linked to a spoken word, check that spoken
                    // word is also linked to the printed word
                    var currentSpokenWordIndex = -1;
                    for (var x1 = 0; x1 < _spokenWordsForEachPrintedWord[currentPrintedWord].Count; x1++)
                        if (_spokenWordsForEachPrintedWord[currentPrintedWord][x1] == m)
                            currentSpokenWordIndex = x1;

                    if (currentSpokenWordIndex != -1)
                        _netInputPhonolexLayer[m] += _actOrtholexLayer[_printedWordsForEachSpokenWord[m][x]] *
                                                    (_orthLexPhonLexExcitation - _orthLexPhonLexInhibition);
                }
            }

            // LATERAL INHIBITION IN THE OL LAYER
            var netOrtholexLatInhibition = _actOrtholexLayer.Sum(t => t * _orthLexLateralInhibition);

            for (var l = 0; l < _printedWordsCount; l++)
                _netInputOrtholexLayer[l] += netOrtholexLatInhibition - _actOrtholexLayer[l] * _orthLexLateralInhibition;

            // LATERAL INHIBITION IN THE PL LAYER
            for (var m = 0; m < _spokenWordsLength; m++)
                netPhonolexLatInhibition += _actPhonolexLayer[m] * _phonLexLateralInhibition;

            for (var m = 0; m < _spokenWordsLength; m++)
                _netInputPhonolexLayer[m] += netPhonolexLatInhibition - _actPhonolexLayer[m] * _phonLexLateralInhibition;

            // PROCESS NET INPUT FROM PHONLEX TO PHONEME LAYER, AND VICE VERSA.            
            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
                netPhonemeToPhonlexInhibition[i] += _actPhonemeLayer[i * _phonemesLength + j] * _phonemePhonLexInhibition;

            for (var m = 0; m < _spokenWordsLength; m++)
            {
                //PL nodes are only connected to phoneme nodes for the same number of slots
                // as phonemes in the spoken word. e.g., for "k{t+", only activations from the
                // first 4 phoneme slots are considered.
                for (var i = 0; i < _spokenWords[m].Length; i++)
                    _netInputPhonolexLayer[m] += netPhonemeToPhonlexInhibition[i];

                for (var i = 0; i < _nPhonemeSlots; i++)
                {
                    if (i >= _spokenWords[m].Length) break;

                    var phonemeLayerAbsolutePosition =
                        i * _phonemesLength + _phonemesReverseDictionary[_spokenWords[m][i]];

                    _netInputPhonolexLayer[m] += _actPhonemeLayer[phonemeLayerAbsolutePosition]
                                                * (_phonemePhonLexExcitation - _phonemePhonLexInhibition);

                    _netInputPhonemeLayer[phonemeLayerAbsolutePosition] +=
                        _actPhonolexLayer[m] * (_phonLexPhonemeExcitation - _phonLexPhonemeInhibition);
                }
            }

            for (var m = 0; m < _spokenWordsLength; m++)
                //PL nodes are only connected to phoneme nodes for the same number of slots
                // as phonemes in the spoken word. e.g., for "k{t+", only the
                // first 4 phoneme slots receive inhibition from the "k{t+" PL node.
            for (var i = 0; i < _spokenWords[m].Length; i++)
                netPhonLexToPhonemeInhibition[i] += _actPhonolexLayer[m] * _phonLexPhonemeInhibition;

            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
                _netInputPhonemeLayer[i * _nPhonemeSlots + j] += netPhonLexToPhonemeInhibition[i];

            // LATERAL INHIBITION IN THE PHONEME LAYER            
            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
            {
                _currentPhonemeNode = i * _phonemesLength + j;
                netPhonemeLatInhibition[i] += _actPhonemeLayer[_currentPhonemeNode] * _phonemeLateralInhibition;
            }

            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
            {
                _currentPhonemeNode = i * _phonemesLength + j;
                _netInputPhonemeLayer[_currentPhonemeNode] +=
                    netPhonemeLatInhibition[i] - _actPhonemeLayer[_currentPhonemeNode] * _phonemeLateralInhibition;
            }

            // CONTEXTUAL INPUT TO PHONOLOGICAL LEXICON LAYER
            for (var m = 0; m < _spokenWordsLength; m++)
                if (_context.Contains(_spokenWords[m]))
                    _netInputPhonolexLayer[m] += _actSemanticNode * _semantic2PhonolexExcitation;
                else
                    _netInputPhonolexLayer[m] += _actSemanticNode * _semantic2PhonolexInhibition;
        }


        /// <summary>
        ///     Calculate new activation levels after a single cycle.
        /// </summary>
        private void ProcessNewActivations()
        {
            var epsilonLetterLayer = new float[_nLetterSlots * _lettersLength];
            var epsilonOrtholexLayer = new float[_printedWordsCount];
            var epsilonPhonolexLayer = new float[_spokenWordsLength];
            var epsilonPhonemeLayer = new float[_nPhonemeSlots * _phonemesLength];

            // LETTER LAYER
            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
            {
                _currentLetterNode = i * _lettersLength + j;

                if (_netInputLetterLayer[_currentLetterNode] >= 0f)
                    epsilonLetterLayer[_currentLetterNode] =
                        _netInputLetterLayer[_currentLetterNode] * (1 - _actLetterLayer[_currentLetterNode]);
                else
                    epsilonLetterLayer[_currentLetterNode] =
                        _netInputLetterLayer[_currentLetterNode] * _actLetterLayer[_currentLetterNode];

                _actLetterLayer[_currentLetterNode] += epsilonLetterLayer[_currentLetterNode] * _activationRate;
            }

            // ORTHOGRAPHIC LEXICON LAYER
            for (var l = 0; l < _printedWordsCount; l++)
            {
                if (_netInputOrtholexLayer[l] >= 0f)
                    epsilonOrtholexLayer[l] = _netInputOrtholexLayer[l] * (1 - _actOrtholexLayer[l]);
                else
                    epsilonOrtholexLayer[l] = _netInputOrtholexLayer[l] * _actOrtholexLayer[l];
                _actOrtholexLayer[l] += epsilonOrtholexLayer[l] * _activationRate;
            }

            // PHONOLOGICAL LEXICON LAYER
            for (var m = 0; m < _spokenWordsLength; m++)
            {
                if (_netInputPhonolexLayer[m] >= 0f)
                    epsilonPhonolexLayer[m] = _netInputPhonolexLayer[m] * (1 - _actPhonolexLayer[m]);
                else
                    epsilonPhonolexLayer[m] = _netInputPhonolexLayer[m] * _actPhonolexLayer[m];
                _actPhonolexLayer[m] += epsilonPhonolexLayer[m] * _activationRate;
            }

            // PHONEME LAYER
            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
            {
                _currentPhonemeNode = i * _phonemesLength + j;

                // phoneme unsupported decay applied here
                if (_netInputPhonemeLayer[_currentPhonemeNode] <= 0.00000000f)
                    _actPhonemeLayer[_currentPhonemeNode] = _phonemeUnsupportedDecay * _actPhonemeLayer[_currentPhonemeNode];

                // calculate epsilons
                if (_netInputPhonemeLayer[_currentPhonemeNode] >= 0f)
                    epsilonPhonemeLayer[_currentPhonemeNode] =
                        _netInputPhonemeLayer[_currentPhonemeNode] * (1 - _actPhonemeLayer[_currentPhonemeNode]);
                else
                    epsilonPhonemeLayer[_currentPhonemeNode] =
                        _netInputPhonemeLayer[_currentPhonemeNode] * _actPhonemeLayer[_currentPhonemeNode];

                _actPhonemeLayer[_currentPhonemeNode] += epsilonPhonemeLayer[_currentPhonemeNode] * _activationRate;
            }

            // SEMANTIC NODES
            var epsilonContextNode = _contextInput2Semantic * (1 - _actSemanticNode);
            _actSemanticNode += epsilonContextNode * _activationRate;
        }


        /// <summary>
        ///     Ensures all activations stay between 0 and 1.
        /// </summary>
        private void ClipActivations()
        {
            for (var i = 0; i < _nLetterSlots; i++)
            for (var j = 0; j < _lettersLength; j++)
                if (_actLetterLayer[i * _lettersLength + j] > 1.0f)
                    _actLetterLayer[i * _lettersLength + j] = 1.0f;
                else if (_actLetterLayer[i * _lettersLength + j] < 0.0f)
                    _actLetterLayer[i * _lettersLength + j] = 0.0f;

            for (var l = 0; l < _printedWordsCount; l++)
                if (_actOrtholexLayer[l] > 1.0f)
                    _actOrtholexLayer[l] = 1.0f;
                else if (_actOrtholexLayer[l] < 0.0f)
                    _actOrtholexLayer[l] = 0.0f;

            for (var m = 0; m < _spokenWordsLength; m++)
                if (_actPhonolexLayer[m] > 1.0f)
                    _actPhonolexLayer[m] = 1.0f;
                else if (_actPhonolexLayer[m] < 0.0f)
                    _actPhonolexLayer[m] = 0.0f;

            for (var i = 0; i < _nPhonemeSlots; i++)
            for (var j = 0; j < _phonemesLength; j++)
                if (_actPhonemeLayer[i * _phonemesLength + j] > 1.0f)
                    _actPhonemeLayer[i * _phonemesLength + j] = 1.0f;
                else if (_actPhonemeLayer[i * _phonemesLength + j] < 0.0f)
                    _actPhonemeLayer[i * _phonemesLength + j] = 0.0f;
        }

        #endregion

        #region LEARNING METHODS

        /// <summary>
        ///     If the stimulus was a recognized spoken word, and the stimulus is novel, then
        ///     this method adds a new OL node and creates new connections between this node and the
        ///     PL layer and the Letter layer.
        ///     If the stimulus was a recognized spoken AND printed word, then this method
        ///     updates the excitations between the relevant OL node and the PL layer and
        ///     the letter layer.
        /// </summary>
        private void Learning(DirectoryInfo workingSubDir)
        {
            var maxActivatedOWord = 0;
            var maxActivatedPWord = 0;
            var maxActivationO = 0f;
            var maxActivationP = 0f;

            // find maximally activated OL word node
            for (var l = 0; l < _printedWordsCount; l++)
            {
                if (!(_actOrtholexLayer[l] > maxActivationO)) continue;
                maxActivationO = _actOrtholexLayer[l];
                maxActivatedOWord = l;
            }

            // find maximally activated PL word node
            for (var m = 0; m < _spokenWordsLength; m++)
            {
                if (!(_actPhonolexLayer[m] > maxActivationP)) continue;
                maxActivationP = _actPhonolexLayer[m];
                maxActivatedPWord = m;
            }

            // Don't undertake learning if there was no spoken
            // word sufficiently well recognized.
            // i.e. the stimulus is not a known word.
            if (maxActivationP < _spokenWordRecogThreshold) return;

            // If there is no sufficiently activated OL word node,
            // then learn a new OL word.
            // Otherwise, update excitations for an existing OL
            // word node.
            if (maxActivationO < _printedWordRecogThreshold)
                LearnNewOrthographicWord(maxActivatedPWord, workingSubDir);
            else
                LearnExistingOrthographicWord(maxActivatedOWord, maxActivatedPWord, workingSubDir);
        }

        /// <summary>
        ///     Subroutine of the Learning() method, that handles the learning of a new OL word.
        ///     Called if there is a PL node above the threshold, but no OL node above the
        ///     threshold (ie its a known spoken word, but there is no corresponding printed word.
        ///     To add a new OL word node, need to:
        ///     a) add the most activated letter from each slot as a new word to the PrintedWords list
        ///     b) Add a new item to printedWordsForEachSpokenWord, connecting the relevant spoken
        ///     word to the newly minted printed word
        ///     c) Add a new item to spokenWordsForEachPrintedWord, connecting the newly minted printed
        ///     word to the most active spoken word
        ///     d) Add a new item to printedWordFreq, with starting value of printedWordFreqMultiplier
        ///     e) Recalculate maxPrintedWordFreq, and printedCFS values
        /// </summary>
        /// <param name="maxActivatedPWord"></param>
        /// <param name="workingSubDir"></param>
        private void LearnNewOrthographicWord(int maxActivatedPWord, FileSystemInfo workingSubDir)
        {
            var path = Path.Combine(workingSubDir.FullName, "nodeUpdateLog.txt");
            var streamW = new StreamWriter(path, true);
            var blankSeen = false;
            var newPrintedWord = new StringBuilder();

            // a) Add the most active letters as a new printedWord

            // First, need to find maximally activated letter in each slot

            for (var i = 0; i < _nLetterSlots; i++)
            {
                if (blankSeen)
                    continue;
                var indexMaxLetter = GetMaxLetterNode(i);

                // skip all blanks if we don't want any orthographic blanks
                if (_orthographicBlanks == 0)
                {
                    if (_letters[indexMaxLetter] == Blankchar)
                    {
                        blankSeen = true;
                        continue;
                    }
                }
                // include 1 blank and skip the rest if we want only one orthographic blank
                else if (_orthographicBlanks == 1)
                {
                    if (_letters[indexMaxLetter] == Blankchar)
                        blankSeen = true;
                }
                newPrintedWord.Append(_letters[indexMaxLetter]);
            }

            var indexPrinted = _printedWords.Count;
            _printedWords.Add(newPrintedWord.ToString());
            _printedWordsCount = _printedWords.Count;

            streamW.WriteLine($"New O node created for the word: {_printedWords[indexPrinted]}");
            Console.WriteLine($"New O node created for the word: {_printedWords[indexPrinted]}");

            // b) and c) Add new items to printedWordsForEachSpokenWord, and printedWordsForEachPrintedWord

            _printedWordsForEachSpokenWord[maxActivatedPWord].Add(indexPrinted);
            _spokenWordsForEachPrintedWord.Add(new List<int>());
            _spokenWordsForEachPrintedWord[indexPrinted].Add(maxActivatedPWord);

            // d) Add a new item to printedWordFreq, with starting value of printedWordFreqMultiplier

            _printedWordFreq.Add(_printedWordFreqMultiplier);

            // e) Recalculate maxPrintedWordFreq, and printedCFS values

            _maxPrintedWordFreq = GetMaxIntFromArray(_printedWordFreq.ToArray());
            _printedCFS = new float[_printedWordsCount];

            for (var l = 0; l < _printedWordsCount; l++)
                _printedCFS[l] = (float) (Math.Log10(_printedWordFreq[l] + 1) / Math.Log10(_maxPrintedWordFreq + 1) - 1) *
                                _frequencyScale;

            streamW.Close();
        }


        /// <summary>
        ///     Subroutine of the Learning() method, that handles the updating of frequencies
        ///     when improving the learning of an existing word.
        ///     Called if there is a PL node above the threshold, and also an OL node about threshold.
        ///     Need to:
        ///     a) check if it is a novel homograph
        ///     b) update printedWordFreq
        ///     c) re-calculate maxPrintedWordFreq
        ///     d) re-calculate all printedCFSs
        /// </summary>
        /// <param name="maxActivatedOWord"></param>
        /// <param name="maxActivatedPWord"></param>
        /// <param name="workingSubDir"></param>
        private void LearnExistingOrthographicWord(int maxActivatedOWord, int maxActivatedPWord,
            FileSystemInfo workingSubDir)
        {
            var path = Path.Combine(workingSubDir.FullName, "nodeUpdateLog.txt");
            var streamW = new StreamWriter(path, true);
            var novelHomograph = true;

            // a) Check if it is a novel homograph
            if (_printedWordsForEachSpokenWord[maxActivatedPWord].Count != 0)
                for (var x1 = 0; x1 < _printedWordsForEachSpokenWord[maxActivatedPWord].Count; x1++)
                    if (_printedWordsForEachSpokenWord[maxActivatedPWord][x1] == maxActivatedOWord)
                    {
                        novelHomograph = false;
                        break;
                    }

            // If it is a novel homograph, make a new connection from the max activatedPWord
            // to the max activated OWord
            if (novelHomograph)
            {
                _printedWordsForEachSpokenWord[maxActivatedPWord].Add(maxActivatedOWord);
                _spokenWordsForEachPrintedWord[maxActivatedOWord].Add(maxActivatedPWord);
            }

            // b) update printedWordFreq

            _printedWordFreq[maxActivatedOWord] += _printedWordFreqMultiplier;

            // c) re-calculate maxPrintedWordFreq

            _maxPrintedWordFreq = GetMaxIntFromArray(_printedWordFreq.ToArray());

            // d) re-calculate all printedCFSs

            _printedCFS = new float[_printedWordsCount];

            for (var l = 0; l < _printedWordsCount; l++)
                _printedCFS[l] = (float) (Math.Log10(_printedWordFreq[l] + 1) / Math.Log10(_maxPrintedWordFreq + 1) - 1) *
                                _frequencyScale;

            streamW.WriteLine(
                $"Updated frequency for word {_printedWords[maxActivatedOWord]} to {_printedWordFreq[maxActivatedOWord]}");
            Console.WriteLine(
                $"Updated frequency for word {_printedWords[maxActivatedOWord]} to {_printedWordFreq[maxActivatedOWord]}");

            streamW.Close();
        }

        #endregion

        #region GPC ROUTE METHODS

        // The GPCRoute algorithms are the most difficult
        // to follow. Rough reading ahead.

        /// <summary>
        ///     Reset currentRightmostPhoneme and currentGPCInput,
        ///     ready to process a new stimulus.
        /// </summary>
        private void ResetGPCRoute()
        {
            _currentRightmostPhoneme = 0;
            _currentGPCInput = "";
            _lastSlotSeen = false;
        }

        /// <summary>
        ///     Called by ProcessSingleCycle(). Handles GPC Route processing,
        ///     including parsing, GPC Route output, and adding this output
        ///     to the netinput of phoneme level nodes.
        /// </summary>
        private void ProcessGPCRoute(int cycles)
        {
            // Don't start GPC route processing until a set number
            // of cycles (equal to gpcOnset) has passed.
            if (cycles >= _gpcOnset)
            {
                var rulesApplied = GraphemeParsing(cycles);
                AddGPCActivationToPhonemes(rulesApplied, cycles);
            }
        }


        /// <summary>
        ///     Parse currentGPCInput and determines the GPC rules that apply.
        /// </summary>
        /// <param name="cycles"></param>
        /// <returns>List of GPC Rules that apply.</returns>
        private List<GPCRule> GraphemeParsing(int cycles)
        {
            int maxLetterInSlot;
            var wordPosition = 0;
            var splitGraphemeSeen = false; // flags when a split-grapheme is seen, so the parser
            // will know to look for a single letter next
            var lettersAfterSplit =
                0; // keeps track of letters in a split grapheme after the '.', for word positioning purposes.
            var rulesApplied = new List<GPCRule>();

            // Add first letter
            if (_currentGPCInput == "")
            {
                maxLetterInSlot = GetMaxLetterNode(0);
                _currentGPCInput = _letters[maxLetterInSlot].ToString();
            }

            // Add another letter to the currentGPCInput if
            // the current rightmost excited phoneme activation is
            // above the gpcCriticalPhonology, and provided there are more
            // letters to add. Do not add another letter if the current
            // letter is a blank.
            var maxPhonemeInSlot = GetMaxPhonemeNode(_currentRightmostPhoneme);
            if (_actPhonemeLayer[_currentRightmostPhoneme * _phonemesLength + maxPhonemeInSlot] >= _gpcCriticalPhonology)
                if (_currentGPCInput.Length == _nLetterSlots)
                {
                    _lastSlotSeen = true;
                }
                else if (_currentGPCInput.Length < _nLetterSlots && _currentGPCInput.Last() != Blankchar)
                {
                    maxLetterInSlot = GetMaxLetterNode(_currentGPCInput.Length);
                    _currentGPCInput = string.Concat(_currentGPCInput, _letters[maxLetterInSlot].ToString());
                }

            var unconsumedLetters = _currentGPCInput;

            while (unconsumedLetters != "")
            {
                GPCRule appliedRule;
                if (splitGraphemeSeen == false) // skip past multirules if a split-grapheme
                    // rule has been seen.
                {
                    appliedRule = Search4Multi(unconsumedLetters, wordPosition);

                    if (appliedRule != null)
                    {
                        rulesApplied.Add(appliedRule);
                        wordPosition = UpdateWordPosition(appliedRule, wordPosition);
                        unconsumedLetters = RemoveLetters(appliedRule, unconsumedLetters);

                        // Check if the rule found is a split-grapheme rule (excludes rules that start with the '.')
                        if (appliedRule.RGrapheme.Contains('.') && appliedRule.RGrapheme[0] != '.')
                        {
                            splitGraphemeSeen = true;
                            lettersAfterSplit = GetLettersAfterSplit(appliedRule.RGrapheme);
                        }
                        continue;
                    }
                }

                appliedRule = Search4Context(unconsumedLetters, wordPosition, splitGraphemeSeen);

                if (appliedRule != null)
                {
                    if (splitGraphemeSeen)
                    {
                        wordPosition += lettersAfterSplit;
                        splitGraphemeSeen = false;
                        lettersAfterSplit = 0;
                    }

                    rulesApplied.Add(appliedRule);
                    wordPosition = UpdateWordPosition(appliedRule, wordPosition);
                    unconsumedLetters = RemoveLetters(appliedRule, unconsumedLetters);
                    // Check if the rule found is a split-grapheme rule (excludes rules that start with the '.')
                    if (appliedRule.RGrapheme.Contains('.') && appliedRule.RGrapheme[0] != '.')
                    {
                        splitGraphemeSeen = true;
                        lettersAfterSplit = GetLettersAfterSplit(appliedRule.RGrapheme);
                    }
                    continue;
                }

                if (splitGraphemeSeen == false) // skip past two-letter rules if a split-grapheme
                    // rule has been seen.
                {
                    appliedRule = Search4TwoLetter(unconsumedLetters, wordPosition);

                    if (appliedRule != null)
                    {
                        rulesApplied.Add(appliedRule);
                        wordPosition = UpdateWordPosition(appliedRule, wordPosition);
                        unconsumedLetters = RemoveLetters(appliedRule, unconsumedLetters);
                        // Check if the rule found is a split-grapheme rule (excludes rules that start with the '.')
                        if (appliedRule.RGrapheme.Contains('.') && appliedRule.RGrapheme[0] != '.')
                        {
                            splitGraphemeSeen = true;
                            lettersAfterSplit = GetLettersAfterSplit(appliedRule.RGrapheme);
                        }
                        continue;
                    }
                }

                appliedRule = Search4Mphon(unconsumedLetters, wordPosition, splitGraphemeSeen);

                if (appliedRule != null)
                {
                    if (splitGraphemeSeen)
                    {
                        wordPosition += lettersAfterSplit;
                        splitGraphemeSeen = false;
                        lettersAfterSplit = 0;
                    }

                    rulesApplied.Add(appliedRule);
                    wordPosition = UpdateWordPosition(appliedRule, wordPosition);
                    unconsumedLetters = RemoveLetters(appliedRule, unconsumedLetters);
                    continue;
                }

                appliedRule = Search4Single(unconsumedLetters, wordPosition, splitGraphemeSeen);

                if (appliedRule != null)
                {
                    if (splitGraphemeSeen)
                    {
                        wordPosition += lettersAfterSplit;
                        splitGraphemeSeen = false;
                        lettersAfterSplit = 0;
                    }

                    rulesApplied.Add(appliedRule);
                    wordPosition = UpdateWordPosition(appliedRule, wordPosition);
                    unconsumedLetters = RemoveLetters(appliedRule, unconsumedLetters);
                }
                else
                {
                    appliedRule = new GPCRule(new[] {"A", "sing", unconsumedLetters[0].ToString(), "?", "u", "1.0"});
                    // If at this point, no rule has been found. Need to add in an
                    // additional phoneme to sort this out - probably need the
                    // end of word character, so that end-position rules can
                    // be used.
                    unconsumedLetters = unconsumedLetters.Substring(1);
                }
            }

            // Update rightmosthphoneme. Count the number of phoneme slots in the rules applied
            // to determine the right-most one.
            _currentRightmostPhoneme = -1;
            // initialise to -1 so that if there is only 1 rule applied, it will be counted
            // and the rightmostphoneme will be set to slot 0.
            for (var z = 0; z < rulesApplied.Count; z++)
                _currentRightmostPhoneme += rulesApplied[z].RPhoneme.Length;

            // set currentRightmostPhoneme to 0 if no rule was found to apply.
            if (_currentRightmostPhoneme == -1)
                _currentRightmostPhoneme = 0;

            return rulesApplied;
        }

        /// <summary>
        ///     Returns the number of letters in a split grapheme after the split.
        ///     This method is used to assist in calculating wordPosition.
        /// </summary>
        /// <param name="grapheme"></param>
        /// <returns></returns>
        private static int GetLettersAfterSplit(string grapheme)
        {
            var lettersAfterSplit = 0;
            var dotSeen = false;
            for (var ltr = 0; ltr < grapheme.Length; ltr++)
                if (grapheme[ltr] == '.')
                    dotSeen = true;
                else if (dotSeen)
                    lettersAfterSplit++;
            return lettersAfterSplit;
        }


        /// <summary>
        ///     Using the list of GPC rules that apply, this method ensures that letter
        ///     activation is contributed to the phoneme level via the GPC route.
        ///     The logic in this method is complex. In summary: step through the rules, and
        ///     find the average activation of the letters in each rule, and contribute that
        ///     activation to the phoneme(s - will be more than one phoneme for mphon rules)
        ///     according to phoneme excitation = average letter act * gpcphonemeexcitation.
        ///     Need to ignore the letters in the grapheme of the rule that are either square
        ///     brackets (which denote context letters) or are inside the square brackets.
        ///     Also need to keep track of split graphemes - if a split grapheme is encountered,
        ///     the very next grapheme must be the single-letter grapheme that is encompassed by
        ///     the split grapheme. Set encompassedLetterPosition to the current letter slot
        ///     to make sure that the next grapheme is processed in the correct letter position,
        ///     before finishing off the rest of the letters in the split grapheme that come after
        ///     the '.'.
        /// </summary>
        /// <param name="rulesApplied"></param>
        private void AddGPCActivationToPhonemes(List<GPCRule> rulesApplied, int cycles)
        {
            bool
                insideContextBrackets; // keeps track of when stepping across context letters rather than grapheme letters in the rule.
            float averageLetterActivation; // average activation across letters in a grapheme
            float totalLetterActivation; // summed activation across letters in a grapheme
            int numberLettersInGrapheme; // counts the number of letters in a grapheme
            int currentPhonemeSlot; // used to keep track of the phoneme slots to which each grapheme corresponds.
            int currentLetterSlot; // used to keep track of the letter slot.
            int encompassedLetterPosition; // used to keep track while calculating splitGrapheme activations
            int encompassedContextLetterPosition;
            string phonemesActivated;

            insideContextBrackets = false;
            currentPhonemeSlot = 0;
            currentLetterSlot = 0;
            encompassedLetterPosition = 0;
            encompassedContextLetterPosition = 0;

            var sw = new StreamWriter(_fileActs.FullName, true);

            phonemesActivated = ApplyOutRules(rulesApplied, cycles, sw);

            for (var z = 0; z < rulesApplied.Count; z++)
            {
                var targetGraphemeSeen = false;
                var contextLetterCount = 0;
                totalLetterActivation = 0f;
                numberLettersInGrapheme = 0;

                // If the weird context rule involving silent h is present,
                // it will be the final rule, and can be ignored without
                // messing up the rest of the processing.
                if (rulesApplied[z].RPhoneme == "*" || rulesApplied[z].RPhoneme == "?")
                {
                    currentLetterSlot++;

                    if (_printActivations)
                        sw.WriteLine("Cycle{0}\tGPC{1}\t{2}\t{3}\t{4}\t{5}", cycles, z, 0, rulesApplied[z].GetPositionCharacter(),
                            rulesApplied[z].RGrapheme, rulesApplied[z].RPhoneme);

                    continue;
                }

                if (encompassedLetterPosition != 0)
                {
                    totalLetterActivation =
                        _actLetterLayer[
                            encompassedLetterPosition * _lettersLength +
                            _lettersReverseDictionary[rulesApplied[z].RGrapheme[0]]];
                    numberLettersInGrapheme = 1;
                    encompassedLetterPosition = 0;
                }
                else if (encompassedContextLetterPosition != 0)
                {
                    totalLetterActivation =
                        _actLetterLayer[
                            encompassedContextLetterPosition * _lettersLength +
                            _lettersReverseDictionary[rulesApplied[z].RGrapheme[0]]];
                    numberLettersInGrapheme = 1;
                    encompassedContextLetterPosition = 0;
                }
                else
                {
                    for (var ltr = 0; ltr < rulesApplied[z].RGrapheme.Length; ltr++)
                    {
                        switch (rulesApplied[z].RGrapheme[ltr])
                        {
                            case '[':
                                insideContextBrackets = true;
                                continue;
                            case ']':
                                insideContextBrackets = false;
                                if (ltr == rulesApplied[z].RGrapheme.Length - 1)
                                {
                                    encompassedContextLetterPosition = 0;
                                    currentLetterSlot -= contextLetterCount;
                                }
                                continue;
                            case '.':
                                if (ltr == 0)
                                    continue;
                                else
                                    encompassedLetterPosition = currentLetterSlot;
                                break;
                            default:
                                if (insideContextBrackets)
                                {
                                    if (targetGraphemeSeen &&
                                        rulesApplied[z].RGrapheme[ltr] != '\\' &&
                                        rulesApplied[z].RGrapheme[ltr] != '~')
                                    {
                                        encompassedContextLetterPosition = currentLetterSlot;
                                        contextLetterCount++;
                                        currentLetterSlot++;
                                    }
                                    continue;
                                }

                                targetGraphemeSeen = true;
                                totalLetterActivation +=
                                    _actLetterLayer[
                                        currentLetterSlot * _lettersLength +
                                        _lettersReverseDictionary[rulesApplied[z].RGrapheme[ltr]]];
                                numberLettersInGrapheme++;
                                break;
                        }
                        currentLetterSlot++;
                    }
                }
                averageLetterActivation = totalLetterActivation / numberLettersInGrapheme;

                if (_printActivations)
                    sw.WriteLine("Cycle{0}\tGPC{1}\t{2}\t{3}\t{4}\t{5}", cycles, z, averageLetterActivation,
                        rulesApplied[z].GetPositionCharacter(), rulesApplied[z].RGrapheme, rulesApplied[z].RPhoneme);

                foreach (var ph in rulesApplied[z].RPhoneme)
                {
                    _netInputPhonemeLayer[
                            currentPhonemeSlot * _phonemesLength +
                            _phonemesReverseDictionary[phonemesActivated[currentPhonemeSlot]]] +=
                        averageLetterActivation * _gpcPhonemeExcitation;
                    currentPhonemeSlot++;
                }
            }

            sw.Close();
        }


        /// <summary>
        ///     Searches the phonemes produced by the rules applied, looking for
        ///     instances where an outrule should be used to change activated phonemes.
        /// </summary>
        /// <param name="rulesApplied"></param>
        /// <param name="cycles"></param>
        /// <returns>string of phonemes to be activated in the phoneme layer</returns>
        private string ApplyOutRules(List<GPCRule> rulesApplied, int cycles, StreamWriter sw)
        {
            bool insideContextBrackets;
            bool preContextFound;
            bool postContextFound;
            bool affectedPhonemeSeen;
            bool affectedPhonemeMatch;
            bool outRuleApplied;
            var preOutRulesPhonemes = new StringBuilder();
            var afterOutRulesPhonemes = new StringBuilder();
            var thisPhoneme = 0;

            for (var z = 0; z < rulesApplied.Count; z++)
                if (rulesApplied[z].RPhoneme != "*")
                    preOutRulesPhonemes.Append(rulesApplied[z].RPhoneme);

            //loop through rules applied
            for (var z = 0; z < rulesApplied.Count; z++)
            {
                // If the weird context rule involving silent h is present,
                // it will be the last rule, and can be ignored without causing
                // problems with the rest of the processing.
                if (rulesApplied[z].RPhoneme == "*")
                    continue;
                // If the rule is protected, move on to phonemes after this rule.
                if (rulesApplied[z].RProtection)
                {
                    afterOutRulesPhonemes.Append(rulesApplied[z].RPhoneme);
                    thisPhoneme += rulesApplied[z].RPhoneme.Length;
                    continue;
                }

                //loop through phonemes in each rule applied
                for (var ph = 0; ph < rulesApplied[z].RPhoneme.Length; ph++)
                {
                    outRuleApplied = false;

                    //loop through outrules
                    for (var thisOutRule = 0; thisOutRule < _outRules.Length; thisOutRule++)
                    {
                        insideContextBrackets = false;
                        preContextFound = false;
                        postContextFound = false;
                        affectedPhonemeSeen = false;
                        affectedPhonemeMatch = false;

                        //loop through each char in the output rule input string
                        for (var thisOutRuleChar = 0;
                            thisOutRuleChar < _outRules[thisOutRule].RGrapheme.Length;
                            thisOutRuleChar++)
                            switch (_outRules[thisOutRule].RGrapheme[thisOutRuleChar])
                            {
                                case '[':
                                    insideContextBrackets = true;
                                    break;
                                case ']':
                                    insideContextBrackets = false;
                                    break;
                                default:
                                    if (insideContextBrackets)
                                    {
                                        // if not at start of phoneme string, and previous phoneme matches preContextPhoneme, then a preContext has been found
                                        if (thisPhoneme != 0)
                                            if (_outRules[thisOutRule].RGrapheme[thisOutRuleChar] ==
                                                preOutRulesPhonemes[thisPhoneme - 1] &&
                                                affectedPhonemeSeen == false)
                                                preContextFound = true;
                                        // if not at end of phoneme string, and the next phoneme matches a postContextPhoneme, then a postContext has been found
                                        if (thisPhoneme != preOutRulesPhonemes.Length - 1)
                                            if (_outRules[thisOutRule].RGrapheme[thisOutRuleChar] ==
                                                preOutRulesPhonemes[thisPhoneme + 1] &&
                                                affectedPhonemeSeen)
                                                postContextFound = true;
                                    }
                                    else
                                    {
                                        affectedPhonemeSeen = true;

                                        if (preOutRulesPhonemes[thisPhoneme] !=
                                            _outRules[thisOutRule].RGrapheme[thisOutRuleChar])
                                            continue;
                                        switch (_outRules[thisOutRule].RPosition)
                                        {
                                            case GPCRule.RulePosition.Beginning:
                                                if (thisPhoneme != 0)
                                                    continue;
                                                else
                                                    affectedPhonemeMatch = true;
                                                break;

                                            case GPCRule.RulePosition.Middle:
                                                if (thisPhoneme == 0 ||
                                                    preOutRulesPhonemes[thisPhoneme] == Blankchar ||
                                                    thisPhoneme == preOutRulesPhonemes.Length - 2 &&
                                                    preOutRulesPhonemes[thisPhoneme + 1] == Blankchar ||
                                                    thisPhoneme == preOutRulesPhonemes.Length - 1 && _lastSlotSeen)
                                                    continue;
                                                else
                                                    affectedPhonemeMatch = true;
                                                break;

                                            case GPCRule.RulePosition.End:
                                                if (thisPhoneme == preOutRulesPhonemes.Length - 2 &&
                                                    preOutRulesPhonemes[thisPhoneme + 1] == Blankchar ||
                                                    thisPhoneme == preOutRulesPhonemes.Length - 1 && _lastSlotSeen)
                                                    affectedPhonemeMatch = true;
                                                else
                                                    continue;
                                                break;

                                            default: // all positions
                                                affectedPhonemeMatch = true;
                                                break;
                                        }
                                    }
                                    break;
                            }

                        if (affectedPhonemeMatch &&
                            (preContextFound || postContextFound))
                        {
                            outRuleApplied = true;
                            afterOutRulesPhonemes.Append(_outRules[thisOutRule].RPhoneme);
                            // rule found and applied. stop looping through out rules, and move on to next phoneme.

                            if (_printActivations)
                                sw.WriteLine("Cycle{0}\tOUT{1}\t{2}\t{3}\t{4}", cycles, z,
                                    _outRules[thisOutRule].GetPositionCharacter(), _outRules[thisOutRule].RGrapheme,
                                    _outRules[thisOutRule].RPhoneme);
                            break;
                        }
                    }

                    if (outRuleApplied == false)
                        afterOutRulesPhonemes.Append(rulesApplied[z].RPhoneme[ph]);

                    thisPhoneme++;
                }
            }
            return afterOutRulesPhonemes.ToString();
        }


        /// <summary>
        ///     Counts relevant letters in the grapheme of the rule just applied,
        ///     and modifies the word position accordingly, so that for ongoing
        ///     parsing, the parser will know what part of the word it is up to
        ///     and know if it should be looking for beginning, middle, or end rules.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Integer for word position.</returns>
        private static int UpdateWordPosition(GPCRule rule, int wordPosition)
        {
            var insideContextBrackets = false;
            var wp = wordPosition;
            for (var ltr = 0; ltr < rule.RGrapheme.Length; ltr++)
                switch (rule.RGrapheme[ltr])
                {
                    case '[':
                        insideContextBrackets = true;
                        break;
                    case ']':
                        insideContextBrackets = false;
                        break;
                    case '.':
                        if (ltr == 0)
                            break;
                        else
                            ltr = rule.RGrapheme.Length;
                        break;
                    default:
                        if (insideContextBrackets)
                            continue;
                        wp++;
                        break;
                }
            return wp;
        }


        /// <summary>
        ///     Removes the letters in the grapheme of the identified GPC rule from unconsumedLetters.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="unconsumedLetters"></param>
        /// <returns>String - remaining unconsumed letters, if any ("" returned if none left).</returns>
        private static string RemoveLetters(GPCRule rule, string unconsumedLetters)
        {
            var newUnconsumedLetters = unconsumedLetters;
            var insideContextBrackets = false;
            var positionInUnCLetters = 0;
            var possibleMiddleContexts = 0;
            var targetGraphemeSeen = false;

            for (var i = 0; i < rule.RGrapheme.Length; i++)
            {
                if (newUnconsumedLetters == "")
                    return "";

                switch (rule.RGrapheme[i])
                {
                    case '[':
                        insideContextBrackets = true;
                        break;
                    case ']':
                        insideContextBrackets = false;
                        break;
                    case '.':
                        if (i == 0)
                            break;
                        else
                            positionInUnCLetters++;
                        break;
                    default:
                        if (insideContextBrackets)
                        {
                            if (targetGraphemeSeen &&
                                rule.RGrapheme[i] != '\\' &&
                                rule.RGrapheme[i] != '~')
                                possibleMiddleContexts++;
                            continue;
                        }
                        targetGraphemeSeen = true;
                        newUnconsumedLetters =
                            newUnconsumedLetters.Remove(0 + positionInUnCLetters + possibleMiddleContexts, 1);
                        break;
                }
            }
            return newUnconsumedLetters;
        }


        /// <summary>
        ///     Search for multi-rule in the remaining letters of the input.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Returns GPC rule if one is found, otherwise returns null</returns>
        private GPCRule Search4Multi(string unconsumedLetters, int wordPosition)
        {
            // if rule is too small to be a multi-rule, then return null.
            // this still allows for the strange rules whose graphemes start with .
            // (e.g. end .ge  _) which are found when only two letters are present,
            // because these rules are end rules, and so the blankChar will be present
            // to make up 3 characters. However, this is not the case when it the number
            // of letters in the stimulus = the number of letter slots - no room for a
            // blank then. need to include code for this special case.
            if (unconsumedLetters.Length < 3 && _stimulusLength != _nLetterSlots)
                return null;

            if (_stimulusLength == _nLetterSlots && unconsumedLetters.Length < 2)
                return null;

            //loop through all multirules, and return the first matching rule found.
            for (var z = 0; z < _multiRules.Length; z++)
            {
                var dotStep = 0;

                if (_stimulusLength != _nLetterSlots)
                {
                    if (_multiRules[z].RGrapheme.Length > unconsumedLetters.Length)
                        continue; // skip if grapheme is too big
                }
                else // if stimulus.Length = nLetterSlots
                {
                    if (_multiRules[z].RGrapheme.Length > unconsumedLetters.Length + 1)
                        continue; // for full-span stimuli, skip if grapheme is more than 1 letter too long
                    if (_multiRules[z].RGrapheme.Length == unconsumedLetters.Length + 1 &&
                        (_multiRules[z].RGrapheme[0] != '.' || wordPosition + unconsumedLetters.Length != _nLetterSlots))
                        continue; // for full-span stimuli that are 1-letter too long, skip if not at end or not a starting '.' rule
                }

                // we will be assuming that the current rule is an applicable rule, unless it fails to match
                // on a particular letter, in which case we know it is false and move on to the next rule.
                var ruleFound = true;

                for (var ltr = 0; ltr < _multiRules[z].RGrapheme.Length; ltr++)
                {
                    // move on to next letter if it is the space in a split-grapheme rule.
                    if (_multiRules[z].RGrapheme[ltr] == '.')
                    {
                        //check if . is at the start of the grapheme, and that wordPosition
                        // is not at the start of the word. otherwise, the weird rules where
                        // grapheme starts with . won't apply.
                        if (ltr == 0 && wordPosition != 0)
                            dotStep = 1;
                        continue;
                    }
                    if (_multiRules[z].RGrapheme[ltr] != unconsumedLetters[ltr - dotStep])
                    {
                        ruleFound = false;
                        break;
                    }
                }
                if (ruleFound)
                    if (wordPosition == 0)
                        switch (_multiRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.Beginning:
                                return _multiRules[z];

                            case GPCRule.RulePosition.Middle:
                                // candidate multirule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.End:
                                // candidate multirule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.All:
                                return _multiRules[z];
                        }
                    else
                        switch (_multiRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.Beginning:
                                //candidate multirule doesn't apply because we are no longer
                                // at the beginning position. Move on.
                                break;

                            case GPCRule.RulePosition.Middle:
                                //candidate multirule is ok in the middle position so long as
                                // it doesn't stretch all the way to the end position.
                                // split grapheme rules that include the last letter are ok if they
                                // correspond to a middle phoneme (graphemes starting with '.' not ok).
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (_multiRules[z].RGrapheme.Length < unconsumedLetters.Length - 1 ||
                                        _multiRules[z].RGrapheme.Length == unconsumedLetters.Length - 1 &&
                                        _multiRules[z].RGrapheme.Contains('.') && _multiRules[z].RGrapheme[0] != '.')
                                        return _multiRules[z];
                                    break;
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (_multiRules[z].RGrapheme.Length < unconsumedLetters.Length)
                                        return _multiRules[z];
                                    break;
                                }
                                else
                                {
                                    return _multiRules[z];
                                }

                            case GPCRule.RulePosition.End:
                                // candidate multirule is ok in the end position provided it
                                // stretches all the way to the end of the unconsumed letters.
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (_multiRules[z].RGrapheme.Length == unconsumedLetters.Length - 1 + dotStep)
                                        return _multiRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (_multiRules[z].RGrapheme.Length == unconsumedLetters.Length + dotStep)
                                        return _multiRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.All:
                                return _multiRules[z];
                        }
            }
            // if no rule has been returned via the logic above, and we are at this point, then no
            // multi-rule is applicable. return null.
            return null;
        }


        /// <summary>
        ///     Search for context-rule in the remaining letters of the input.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <param name="splitGraphemeSeen"></param>
        /// <returns>Returns GPC rule if one is found, otherwise returns null</returns>
        private GPCRule Search4Context(string unconsumedLetters, int wordPosition, bool splitGraphemeSeen)
        {
            // if rule is too small to be a context-rule, then return null.
            // smallest possible context rule is a single letter, with a single
            // letter context, plus brackets, = 4 characters.
            if (wordPosition + unconsumedLetters.Length < 2)
                return null;

            //loop through all contextrules, and return the first matching rule found.
            for (var z = 0; z < _contextRules.Length; z++)
            {
                var preContexts = new List<string>();
                var postContexts = new List<string>();
                var middleContexts = new List<string>();

                var preContextSB = new StringBuilder();
                var postContextSB = new StringBuilder();

                GPCRule candidateRule = null;

                var targetGraphemeSeen = false;
                var possibleMiddleContext = false;
                var definiteMiddleContext = false;
                var insideContextBrackets = false;
                var targetGrapheme = new StringBuilder();

                // Divide the GPC's grapheme into context and grapheme.
                for (var i = 0; i < _contextRules[z].RGrapheme.Length; i++)
                    switch (_contextRules[z].RGrapheme[i])
                    {
                        case '[':
                            insideContextBrackets = true;

                            // if a context is seen after a target grapheme has
                            // already been seen, then it is a possible middle context
                            // Not definite - it could be a post context.
                            if (targetGraphemeSeen)
                                possibleMiddleContext = true;
                            break;

                        case ']':
                            insideContextBrackets = false;
                            if (targetGraphemeSeen == false)
                            {
                                preContexts.Add(preContextSB.ToString());
                                preContextSB.Length = 0;
                            }
                            else
                            {
                                postContexts.Add(postContextSB.ToString());
                                postContextSB.Length = 0;
                            }
                            break;

                        default:
                            if (insideContextBrackets)
                            {
                                if (targetGraphemeSeen == false)
                                    preContextSB.Append(_contextRules[z].RGrapheme[i] == '\\'
                                        ? '>'
                                        : _contextRules[z].RGrapheme[i]);
                                else
                                    postContextSB.Append(_contextRules[z].RGrapheme[i] == '\\'
                                        ? '>'
                                        : _contextRules[z].RGrapheme[i]);
                            }
                            else
                            {
                                // if there is a possible middle context, and another letter
                                // for the target Grapheme is seen, then a post context is no longer
                                // possible, and it must definitely be a middle context.
                                // Middle contexts get stored in postContexts.
                                if (possibleMiddleContext)
                                {
                                    definiteMiddleContext = true;
                                    middleContexts.AddRange(postContexts);
                                    postContexts = new List<string>();

                                    // add '.' to the target grapheme equal to the number of middle
                                    // contexts present (will typically be only 1).
                                    for (var z1 = 0; z1 < middleContexts.Count; z1++)
                                        targetGrapheme.Append('.');
                                    possibleMiddleContext = false;
                                }
                                targetGraphemeSeen = true;
                                targetGrapheme.Append(_contextRules[z].RGrapheme[i]);
                            }
                            break;
                    }

                if (targetGrapheme.Length > unconsumedLetters.Length)
                    continue;

                if (targetGrapheme.Length > 1 && splitGraphemeSeen)
                    continue;

                // If at the start of the word, but the rule has a pre-context,
                // then it can't apply, move on to the next rule.
                if (wordPosition == 0 && preContexts.Count != 0)
                    continue;

                // If at the end of the word, but the rule has a post-context,
                // then it can't apply, move on to next rule.
                if (targetGrapheme.Length == _currentGPCInput.Length - wordPosition &&
                    unconsumedLetters.Last() == Blankchar &&
                    postContexts.Count != 0 && definiteMiddleContext == false)
                    continue;

                // we will be assuming that the current rule is an applicable rule, unless it fails to match
                // on a particular letter, in which case we know it is false and move on to the next rule.
                var targetGraphemeMatch = true;
                for (var i = 0; i < targetGrapheme.Length; i++)
                {
                    // move on to next letter if it is the space in a split-grapheme rule.
                    if (targetGrapheme[i] == '.')
                        continue;
                    if (targetGrapheme[i] != unconsumedLetters[i])
                    {
                        targetGraphemeMatch = false;
                        break;
                    }
                }

                if (targetGraphemeMatch)
                {
                    if (wordPosition == 0)
                        switch (_contextRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.Beginning:
                                candidateRule = _contextRules[z];
                                break;

                            case GPCRule.RulePosition.Middle:
                                // candidate context-rule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.End:
                                // candidate context-rule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.All:
                                candidateRule = _contextRules[z];
                                break;
                        }
                    else
                        switch (_contextRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.Beginning:
                                //candidate contextrule doesn't apply because we are no longer
                                // at the beginning position. Move on.
                                break;

                            case GPCRule.RulePosition.Middle:
                                //candidate contextrule is ok in the middle position so long as
                                // it doesn't stretch all the way to the end position.
                                // split grapheme rules that include the last letter are ok if they
                                // correspond to a middle phoneme (graphemes starting with '.' not ok).
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (targetGrapheme.Length < unconsumedLetters.Length - 1 ||
                                        targetGrapheme.Length == unconsumedLetters.Length - 1 &&
                                        targetGrapheme.ToString().Contains('.') && targetGrapheme[0] != '.')
                                        candidateRule = _contextRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (targetGrapheme.Length < unconsumedLetters.Length)
                                        candidateRule = _contextRules[z];
                                }
                                else
                                {
                                    candidateRule = _contextRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.End:
                                // candidate contextrule is ok in the end position provided it
                                // stretches all the way to the end of the unconsumed letters.
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (targetGrapheme.Length == unconsumedLetters.Length - 1)
                                        candidateRule = _contextRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (targetGrapheme.Length == unconsumedLetters.Length)
                                        candidateRule = _contextRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.All:
                                candidateRule = _contextRules[z];
                                break;
                        }

                    // if the rule found was in the wrong position, it can't apply,
                    // move on to the next rule.
                    if (candidateRule == null)
                        continue;

                    // If this point is reached, then: 1) a rule has been found (ruleFound = true),
                    // meaning that the target grapheme from the rule was found in unconsumedLetters,
                    // and 2) the rule applies in the current word position (candidateRule != null).
                    // Now work out whether the context (whether pre or post) applies.

                    // assume there is a match, until a contradiction while trying to match
                    // context means there is no match
                    var graphemeAndContextMatch = true;

                    // If pre context applies
                    if (preContexts.Count != 0)
                    {
                        //if there are more pre-contextual letters than
                        //letters prior to the current grapheme, then
                        // the rule can't apply, move on.
                        if (preContexts.Count > wordPosition)
                            continue;

                        for (var contextLetter = 0; contextLetter < preContexts.Count; contextLetter++)
                            if (preContexts[contextLetter] == ">V")
                            {
                                if (IsItAVowel(_currentGPCInput[wordPosition - preContexts.Count + contextLetter]) ==
                                    false)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else if (preContexts[contextLetter] == ">C")
                            {
                                if (IsItAVowel(_currentGPCInput[wordPosition - preContexts.Count + contextLetter]))
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else if (preContexts[contextLetter].Contains('~'))
                            {
                                if (preContexts[contextLetter]
                                    .Contains(_currentGPCInput[wordPosition - preContexts.Count + contextLetter]))
                                {
                                    // presence of '~' means that the context is ok provided none of the letters mentioned in the context
                                    // are present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (preContexts[contextLetter]
                                        .Contains(_currentGPCInput[wordPosition - preContexts.Count + contextLetter]) ==
                                    false)
                                {
                                    //context letter is not present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                        // Move on to next rule if context hasn't matched so far.
                        if (graphemeAndContextMatch == false)
                            continue;
                        return _contextRules[z];
                    }

                    if (postContexts.Count != 0)
                    {
                        //if there are more post-contextual letters than
                        //letters subsequent to the current grapheme, then
                        // the rule can't apply, move on.
                        if (postContexts.Count > _currentGPCInput.Length - wordPosition - targetGrapheme.Length)
                            continue;
                        if (postContexts.Count == _currentGPCInput.Length - wordPosition - targetGrapheme.Length &&
                            unconsumedLetters.Last() == Blankchar)
                            continue;

                        for (var contextLetter = 0; contextLetter < postContexts.Count; contextLetter++)
                            if (postContexts[contextLetter] == ">V")
                            {
                                if (IsItAVowel(_currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]) ==
                                    false)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else if (postContexts[contextLetter] == ">C")
                            {
                                if (IsItAVowel(_currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]))
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else if (postContexts[contextLetter].Contains('~'))
                            {
                                if (postContexts[contextLetter]
                                    .Contains(_currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]))
                                {
                                    // presence of '~' means that the context is ok provided none of the letters mentioned in the context
                                    // are present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else
                            {
                                if (postContexts[contextLetter]
                                        .Contains(_currentGPCInput[
                                            wordPosition + targetGrapheme.Length + contextLetter]) == false)
                                {
                                    //context letter is not present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                        // Move on to next rule if context hasn't matched so far.
                        if (graphemeAndContextMatch == false)
                            continue;
                        return _contextRules[z];
                    }

                    if (middleContexts.Count != 0)
                    {
                        int targetGraphemePreContextLetters;
                        for (targetGraphemePreContextLetters = 0;
                            targetGraphemePreContextLetters < targetGrapheme.Length;
                            targetGraphemePreContextLetters++)
                            if (targetGrapheme[targetGraphemePreContextLetters] == '.')
                                break;

                        //if the target grapheme (which includes '.'s for the middle context(s)
                        // is too big for remaining letters, move on.
                        if (targetGrapheme.Length > unconsumedLetters.Length)
                            continue;
                        if (targetGrapheme.Length == unconsumedLetters.Length &&
                            unconsumedLetters.Last() == Blankchar)
                            continue;

                        for (var contextLetter = 0; contextLetter < middleContexts.Count; contextLetter++)
                            if (middleContexts[contextLetter] == ">V")
                            {
                                if (IsItAVowel(_currentGPCInput[
                                        wordPosition + targetGraphemePreContextLetters + contextLetter]) == false)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else if (middleContexts[contextLetter] == ">C")
                            {
                                if (IsItAVowel(
                                    _currentGPCInput[wordPosition + targetGraphemePreContextLetters + contextLetter]))
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else if (middleContexts[contextLetter].Contains('~'))
                            {
                                if (middleContexts[contextLetter]
                                    .Contains(_currentGPCInput[
                                        wordPosition + targetGraphemePreContextLetters + contextLetter]))
                                {
                                    // presence of '~' means that the context is ok provided none of the letters mentioned in the context
                                    // are present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else
                            {
                                if (middleContexts[contextLetter]
                                        .Contains(_currentGPCInput[
                                            wordPosition + targetGraphemePreContextLetters + contextLetter]) == false)
                                {
                                    //context letter is not present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                        // Move on to next rule if context hasn't matched so far.
                        if (graphemeAndContextMatch == false)
                            continue;
                        return _contextRules[z];
                    }
                }
            }
            // if no rule has been returned via the logic above, and we are at this point, then no
            // context-rule is applicable. return null.
            return null;
        }


        /// <summary>
        ///     Searches for a two rule in the remaining letters.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Returns a two rule if one found, otherwise, returns null.</returns>
        private GPCRule Search4TwoLetter(string unconsumedLetters, int wordPosition)
        {
            // if unconsumed letters is too small to have a two-rule, then return null.
            if (unconsumedLetters.Length < 2)
                return null;

            //loop through all tworules, and return the first matching rule found.
            for (var z = 0; z < _twoRules.Length; z++)
            {
                // we will be assuming that the current rule is an applicable rule, unless it fails to match
                // on a particular letter, in which case we know it is false and move on to the next rule.
                var ruleFound = true;

                // If there are only two letters left, then split-grapheme rules can't apply.
                // move past them.
                if (unconsumedLetters.Length == 2 && _twoRules[z].RGrapheme.Contains('.'))
                    continue;

                for (var ltr = 0; ltr < _twoRules[z].RGrapheme.Length; ltr++)
                {
                    // move on to next letter if it is the space in a split-grapheme rule.
                    if (_twoRules[z].RGrapheme[ltr] == '.')
                        continue;
                    if (_twoRules[z].RGrapheme[ltr] != unconsumedLetters[ltr])
                    {
                        ruleFound = false;
                        break;
                    }
                }
                if (ruleFound)
                    if (wordPosition == 0)
                        switch (_twoRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.Beginning:
                                return _twoRules[z];

                            case GPCRule.RulePosition.Middle:
                                // candidate tworule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.End:
                                // candidate tworule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.All:
                                return _twoRules[z];
                        }
                    else
                        switch (_twoRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.Beginning:
                                //candidate tworule doesn't apply because we are no longer
                                // at the beginning position. Move on.
                                break;

                            case GPCRule.RulePosition.Middle:
                                //candidate tworule is ok in the middle position so long as
                                // it doesn't stretch all the way to the end position.
                                // split grapheme rules that include the last letter are ok if they
                                // correspond to a middle phoneme (graphemes starting with '.' not ok).
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (_twoRules[z].RGrapheme.Length < unconsumedLetters.Length - 1 ||
                                        _twoRules[z].RGrapheme.Length == unconsumedLetters.Length - 1 &&
                                        _twoRules[z].RGrapheme.Contains('.') && _twoRules[z].RGrapheme[0] != '.')
                                        return _twoRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (_twoRules[z].RGrapheme.Length < unconsumedLetters.Length)
                                        return _twoRules[z];
                                }
                                else
                                {
                                    return _twoRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.End:
                                // candidate tworule is ok in the end position provided it
                                // stretches all the way to the end of the unconsumed letters.
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (_twoRules[z].RGrapheme.Length == unconsumedLetters.Length - 1)
                                        return _twoRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (_twoRules[z].RGrapheme.Length == unconsumedLetters.Length)
                                        return _twoRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.All:
                                return _twoRules[z];
                        }
            }
            // if no rule has been returned via the logic above, and we are at this point, then no
            // two-rule is applicable. return null.
            return null;
        }


        /// <summary>
        ///     Searches for a single rule in first (or only) position of the remaining letters.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <param name="splitGraphemeSeen"></param>
        /// <returns>Returns a single rule if found, otherwise, returns null.</returns>
        private GPCRule Search4Single(string unconsumedLetters, int wordPosition, bool splitGraphemeSeen)
        {
            for (var z = 0; z < _singleRules.Length; z++)
            {
                if (_singleRules[z].RGrapheme[0] != unconsumedLetters[0])
                    continue;

                if (wordPosition == 0)
                    switch (_singleRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.Beginning:
                            return _singleRules[z];

                        case GPCRule.RulePosition.Middle:
                            // candidate singlerule doesn't apply if at the 0 position, move on.
                            break;

                        case GPCRule.RulePosition.End:
                            // even if it is an end rule, if there is only a single
                            // unconsumed letter, that means
                            // that the rule spans the entire stimulus, and is in the
                            // end position as well as the beginning position.
                            if (unconsumedLetters.Last() == Blankchar)
                            {
                                if (unconsumedLetters.Length == 2)
                                    return _singleRules[z];
                            }
                            else if (_lastSlotSeen)
                            {
                                if (unconsumedLetters.Length == 1)
                                    return _singleRules[z];
                            }
                            break;

                        case GPCRule.RulePosition.All:
                            return _singleRules[z];
                    }
                else
                    switch (_singleRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.Beginning:
                            //candidate singlerule doesn't apply because we are no longer
                            // at the beginning position. Move on.
                            break;

                        case GPCRule.RulePosition.Middle:
                            // candidate singlerule is ok in the middle position so long as
                            // there are additional letters, or if we are in the middle of
                            // a split grapheme.
                            if (splitGraphemeSeen)
                                return _singleRules[z];
                            if (unconsumedLetters.Last() == Blankchar)
                            {
                                if (unconsumedLetters.Length != 2)
                                    return _singleRules[z];
                            }
                            else if (_lastSlotSeen)
                            {
                                if (unconsumedLetters.Length != 1)
                                    return _singleRules[z];
                            }
                            else
                            {
                                return _singleRules[z];
                            }
                            break;

                        case GPCRule.RulePosition.End:
                            // candidate singlerule is ok in the end position provided there
                            // is only a single letter left in unconsumedLetters, and provided 
                            // the parser isn't currently in the middle of a split grapheme.
                            if (splitGraphemeSeen == false)
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (unconsumedLetters.Length == 2 ||
                                        unconsumedLetters.Length == 1
                                    ) // this is for when only the blank character is left.
                                        return _singleRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (unconsumedLetters.Length == 1)
                                        return _singleRules[z];
                                }
                            break;

                        case GPCRule.RulePosition.All:
                            return _singleRules[z];
                    }
            }
            // Shouldn't get to this point because there should always be a single rule in the
            // remaining unconsumed letters.
            return null;
        }


        /// <summary>
        ///     Searches for a multi-phonic rule in first (or only) position of the remaining letters.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <param name="splitGraphemeSeen"></param>
        /// <returns>Returns a single rule if found, otherwise, returns null.</returns>
        private GPCRule Search4Mphon(string unconsumedLetters, int wordPosition, bool splitGraphemeSeen)
        {
            for (var z = 0; z < _mphonRules.Length; z++)
            {
                if (_mphonRules[z].RGrapheme[0] != unconsumedLetters[0])
                    continue;

                if (wordPosition == 0)
                    switch (_mphonRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.Beginning:
                            return _mphonRules[z];

                        case GPCRule.RulePosition.Middle:
                            // candidate mphonrule doesn't apply if at the 0 position, move on.
                            break;

                        case GPCRule.RulePosition.End:
                            // even if it is an end rule, if there is only a mphon
                            // unconsumed letter, that means
                            // that the rule spans the entire stimulus, and is in the
                            // end position as well as the beginning position.
                            if (unconsumedLetters.Last() == Blankchar)
                            {
                                if (unconsumedLetters.Length == 2)
                                    return _mphonRules[z];
                            }
                            else if (_lastSlotSeen)
                            {
                                if (unconsumedLetters.Length == 1)
                                    return _mphonRules[z];
                            }
                            break;

                        case GPCRule.RulePosition.All:
                            return _mphonRules[z];
                    }
                else
                    switch (_mphonRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.Beginning:
                            //candidate mphonrule doesn't apply because we are no longer
                            // at the beginning position. Move on.
                            break;

                        case GPCRule.RulePosition.Middle:
                            // candidate mphonrule is ok in the middle position so long as
                            // there are additional letters, or the parser has just parsed
                            // a split grpaheme.
                            if (splitGraphemeSeen)
                                return _mphonRules[z];
                            if (unconsumedLetters.Last() == Blankchar)
                            {
                                if (unconsumedLetters.Length != 2)
                                    return _mphonRules[z];
                            }
                            else if (_lastSlotSeen)
                            {
                                if (unconsumedLetters.Length != 1)
                                    return _mphonRules[z];
                            }
                            else
                            {
                                return _mphonRules[z];
                            }
                            break;

                        case GPCRule.RulePosition.End:
                            // candidate mphonrule is ok in the end position provided there
                            // is only a mphon letter left in unconsumedLetters, and the parser
                            // has not just processed a split grapheme.
                            if (splitGraphemeSeen == false)
                                if (unconsumedLetters.Last() == Blankchar)
                                {
                                    if (unconsumedLetters.Length == 2)
                                        return _mphonRules[z];
                                }
                                else if (_lastSlotSeen)
                                {
                                    if (unconsumedLetters.Length == 1)
                                        return _mphonRules[z];
                                }
                            break;

                        case GPCRule.RulePosition.All:
                            return _mphonRules[z];
                    }
            }
            // If at this point, no mphon rule was found.
            return null;
        }


        /// <summary>
        ///     Returns highest activated letter in a slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private int GetMaxLetterNode(int slot)
        {
            var maxNode = 0;
            var maxAct = 0f;
            for (var j = 0; j < _lettersLength; j++)
                if (_actLetterLayer[slot * _lettersLength + j] > maxAct)
                {
                    maxAct = _actLetterLayer[slot * _lettersLength + j];
                    maxNode = j;
                }
            return maxNode;
        }


        /// <summary>
        ///     Returns highest activated phoneme in a slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private int GetMaxPhonemeNode(int slot)
        {
            var maxNode = 0;
            var maxAct = 0f;
            for (var j = 0; j < _phonemesLength; j++)
                if (_actPhonemeLayer[slot * _phonemesLength + j] > maxAct)
                {
                    maxAct = _actPhonemeLayer[slot * _phonemesLength + j];
                    maxNode = j;
                }
            return maxNode;
        }


        /// <summary>
        ///     Returns whether or not the input letter is a vowel
        /// </summary>
        /// <param name="letter"></param>
        /// <returns>true or false value</returns>
        private bool IsItAVowel(char letter)
        {
            return _vowelStatus[_lettersReverseDictionary[letter]];
        }

        #endregion
    }
}