/// <VERSIONDESCRIPTION>
/// V5.4 170117 Keeping processing identical, but adding in batch training to speed up the running of multiple simulations.
/// Batch training will allow parameter modification, but also run both the learning and testing phases (learning on and off,
/// token corpus for learning and type corpus for training.
/// V5.4 150618 Just turned printing of all random contexts back on, it had been turned off for some reason.
/// V5.4 130621
/// Based on 5.3 (not 5.2).
/// Note: V5.3 Was used for the Pritchard et al. 2013 L-DRC publication.
/// Note: V4.4 Was used for the Stephen Pritchard's PhD thesis.
/// Changed the name of "SpokenWordRecognisedThreshold" to "SpokenWordRecognizedThreshold". Note US spelling of "recognized"
/// Changed the name of "PrintedWordRecognisedThreshold" to "PrintedWordRecognizedThreshold". Note US spelling of "recognized"
/// Changed the name of "PrintedWordFrequencyMultiplier" to "PrintedWordFrequencyMultiplier".
/// Deleted "for Australian English" from the learning parameters file header comment.
/// Deleted all code relevant to the disused complex version of L-DRC.
/// Changed all uses of "written" in variable names to "printed".
/// Changed ProcessSingleStep Method to ProcessSingleCycle
/// Renamed actContextNode variable to actSemanticNode
/// General code and comment cleanup (no effect on performance).
/// /// V5.3 130311
/// Note: V5.3 Does not include the batch-processing changes in 5.2. It is a modified version of 5.1.
/// Corrected a minor bug that resulted in a crash if no GPC rules were applied (e.g., if there were no GPC rules in
/// the gpcrules file). Noticed this bug while running a version of L-DRC that only used outrules, no other GPCs.
/// To correct, needed to set currentRightMostPhoneme to 0 instead of -1, if no rule had been applied.
/// V5.1 130124
/// Modified log file printing so that it also prints the various randomly selected contexts that were activated during processing.
/// Most of the logfile content is now in the learningDRC class instead of in the main program class, though the latter still holds
/// code for printing the simulation time.
/// V5.0 130111
/// Changing the operation of context. Now, different levels of contextual ambiguity will be simulated by modifying then number
/// of words that receive activation via context. A new learning parameter has been introduced: NumberOfSemanticRepsActivated.
/// During a simulation, the semantic node corresponding to the input stimulus word will be activated, along with a random
/// assortment of other words, so that the total number of semantic nodes (including the correct one) receiving activation will
/// be equal to NumberOfSemanticRepsActivated. The choosing of a node will occur predominantly in the phonolex, with input from
/// the GPC route helping to select the correct node, despite ambiguous excitation from the semantic layer to multiple phonolex nodes.
/// the "context" variable is now a string array, instead of just a string.
/// V4.5 121125
/// Changed parameter names for the semantic layer and contextual input, to more properly describe these.
/// Previous names referred to the semantic layer as the "context layer".
/// Both the parameter names in the learningparameters file, in addition to the names of the variables within the code to hold these
/// parameter values were changed.
/// V4.4 120808
/// Changed unsupported phoneme decay threshold back to 0.00000000f. and its now less than or equal to. Did this because, after testing
/// both less than 0.00000001f, and lessthanorequalto 0.00000000f, the latter gave a better match for nonword latencies.
/// V4.3 120807
/// Added code to program.cs and learningDRC.cs to allow nonwords to be processed without the batchfile containing a context or frequency.
/// Fixed error where split grapheme rules weren't processed in the middle position if the last letter of the grapheme was the 
/// last letter of the stimulus, even though the phoneme is in the middle (with the phoneme for the enclosed grapheme coming at the end).
/// V4.2 120806
/// Fixed error where normMultiplier was being calcualted as having value 0.0 if less than 1. Fixed by ensuring that the integers
/// from which normMultiplier is calculated were first cast as floats.
/// Added code so that stimulusLength would have a max value of nLetterSlots, and would not be made equal to nLetterSlots + 1.
/// Fixed special end rules such as .GE so that they now apply even for 8 letter words like SCROUNGE with no null char present.
/// 
/// V4.1 120803
/// * Noticed an error in word position calc for split graphemes. After processing a split grapheme and its contained
/// letter, need to modify word position by a number equal to the number of letters in the split grapheme after the split.
/// * Adjusted unsupported phoneme decay. Check used to be less than or equal to 0f. But now, in accordance with
/// * drc-1.2.1 on pc, that is now less than 0.00000001f. This avoids an issue with floating point calc mechanics.
/// * Changed code to ensure that an end or middle rule will not be applied when still at the beginning position,
/// even if a middle or end rule spans the whole word. (e.g. (e)RE->/r/ should not be used, for the word RE. Instead (A)r->/r/ and
/// (e)e->/i/ should be used. This is in accordance with drc-1.2.1
/// * Fixed error where rules starting with a '.' (e.g., .CE) were excluding the activation of the GPC for the '+' at the end.
/// * Removed normalisation from L2O inhibition. Normalisation now only applies to L2O excitation.
/// * Renamed normMultipler to normMultiplier.
/// G* ot rid of 3 lines of superfluous code that had been left in there by mistake - it was used for setting break points within loops,
/// and had no function, and was no longer required.
/// 
/// V4.0 120305
/// Getting rid of internal switches, and instead putting them in a learning.parameters file. This will mean I need to tinker in the code
/// less often, and will prevent accidently setting switches incorrectly while conducting simulations.
/// 
/// V3.1 120305
/// Had failed to correctly carry over the normalisation multiplier when moving from v 1.1 to v 2.0. Even when the normalisation
/// flag had been set, the net input from the letter to the ortholex layer was not being multiplied by the norm multiplier.
/// This is corrected in v3.1
/// 
/// V3.0 120223
/// It seems that the GPC route has not been functioning correctly. The word AYE is being processed as /12/ by the GPC route
/// instead of /1I/. And the GPC route does not seem to cope with 8-letter long words.
/// Have fixed the processing of 8-letter long words. The model now recognises that if there are 8 letters and the rightmost phoneme
/// is active enough to trigger a call for another letter, then the model will use this as a cue that the last letter added is in fact
/// the last letter, and will apply end-of-word rules. Made changes in the methods for outrules handling to be able to deal with this as
/// well. Also ensured that the application of outrules would now be printed in the activations file.
/// Have fixed the handling of split graphemes. Now, after a split-grapheme has been processed, a flag is set, so that the 
/// next letter to be processed (will only be a single letter (either a context rule with a 1-letter target, an mphon or single-letter rule.
/// 
/// V2.2 120217
/// Fixed the way that homographs are handled. Had not done it right in V2.1, which was causing multiple connections to be learned
/// 
/// V2.1 120210
/// Had not properly included code to handle homographs in V2.0. Adding this code now.
///  
/// V2.0 120209
/// Simple learning. When a new orthographic node is learned, it is learned with maximal DRC connection strengths. There is no training 
/// of connection strengths. Instead, subsequent exposure to the word increments the frequency value for that node.
/// 
/// V1.1 120127
/// 
/// There was an error in the processing of slots after the end of the stimulus. I had not been clearing unused slots after the blank,
/// so they were retaining the visual feature patterns of previous words. e.g. if the word MORE+ was processed, then next word was A+,
/// the visual feature layer would be left with the pattern A+RE+. The fix involved resetting all unused slots.
/// 
/// V1.0 120127
/// Although there have been versions previous to this one, this is the beginning of actually
/// documenting version information here within the code itself. Should have started this earlier!
/// This version includes a full DRC non-lexical route, with a lexical route that includes:
/// - contextual input to the phonological lexicon via semantics
/// - learning of new orthographic nodes
/// - learning of connection weights between the orthographic lexicon (OL), the letter level, and the phonological lexicon (PL).
/// 
/// This version also includes a number of switches/settings to enable variations on the theme. These variations include:
/// - Turn learning on or off
/// - Fast learning (multiply by frequency) or normal learning (frequent words need multiple exposures)
/// - Additional inhibition learning (e.g. to and from inactive OL nodes to active nodes in L layer and PL layer)
/// - Bound learn weights (e.g. bewteen -1.0 and 1.0) or else let them grow infinitely
/// - Weight decay on/off
/// - print activations file on/off
/// - L2O normalisation on/off
/// - number of orthographic blanks (0, 1 or many)
/// 
/// New in this version compared to previous versions is the treatment of orthographic blanks. Previously I had implemented that OL nodes
/// are only connected to however many blanks set with the ORTHOGRAPHIC_BLANKS switch. However, I was still having all of the blanks
/// being excited by the visual feature level. This version alters this, so that blanks are only excited by the visual feature level
/// in accordance with the setting of the ORTHOGRAPHIC_BLANKS switch (e.g the stimulus "to" will only excite the following letters from
/// the visual feature level: TO+
/// </VERSIONDESCRIPTION>

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Learning_DRC
{
    internal sealed class LearningDRC
    {
        #region FIELDS

        /// <summary>
        /// RANDOM NUMBER GENERATOR
        /// </summary>
        private static readonly Random rnd = new Random();           // used to select random contextual options to activate

        /// <summary>
        /// CONSTANTS
        /// </summary>
        private const char BLANKCHAR = '+';         // choose arbitrary ascii character to act as the blank character.      

        /// <summary>
        /// PROPERTY FIELDS -
        /// read from "properties" file.
        /// </summary>
        private string name;                        // Data from DRC-1.2.1
        private string version;                     // Data from DRC-1.2.1
        private string releaseDate;                 // Data from DRC-1.2.1
        private string creator;                     // Data from DRC-1.2.1
        private string url;                         // Data from DRC-1.2.1
        private int nLetterSlots;                   // Number of letter level slots
        private int nPhonemeSlots;                  // Number of phoneme level slots

        /// <summary>
        /// LETTERS, PHONEMES, VOCAB FIELDS
        /// </summary>
        private int[][] letterFeatures;                          // visual feature patterns for each letter
        private Dictionary<int, char> letters;                   // orthographic letters, with index as key
        private Dictionary<char, int> lettersReverseDictionary;  // orthographic letters, with letter as key
        private Dictionary<int, char> phonemes;                  // phonemes, with index as key
        private Dictionary<char, int> phonemesReverseDictionary; // phonemes, with phoneme as key
        private bool[] vowelStatus;                              // whether each letter is a vowel or not
        private List<string> printedWords;                       // orthographic lexicon words
        private List<int> printedWordFreq;                       // orthographic word frequencies
        private int maxPrintedWordFreq;                          // frequency of the most frequent orthographic word
        private string[] spokenWords;                            // phonological lexicon words
        private int[] spokenWordFreq;                            // phonological word frequencies
        private int maxSpokenWordFreq;                           // frequency of most frequent phonological word
        private float[] printedCFS;                              // orthographic constant frequency scaling (CFS) values (see Coltheart et al. (2001) p.216)
        private float[] spokenCFS;                               // phonological constant frequency scaling (CFS) values (see Coltheart et al. (2001) p.216)
        private List<List<int>> spokenWordsForEachPrintedWord;   // phonological words excited by each orthographic word (e.g. /b5/ and /b6/ excited by BOW)
        private List<int>[] printedWordsForEachSpokenWord;       // learned orthographic words excited by each phonological word (e.g. SALE and SAIL excited by /s1l/)

        /// <summary>
        /// GPC FIELDS
        /// </summary>
        private GPCRule[] bodyRules;               
        private GPCRule[] multiRules;              
        private GPCRule[] twoRules;                
        private GPCRule[] mphonRules;              
        private GPCRule[] contextRules;            
        private GPCRule[] singleRules;             
        private GPCRule[] outRules;                

        private string currentGPCInput;            // stores the letters currently available to the GPC route.
        private int currentRightmostPhoneme;       // the right-most phoneme slot receiving activation from the GPC route last cycle.
        private bool lastSlotSeen;                 // set to true if the GPCRoute tries to include input from another letter slot,
                                                   // but can't because already at the last slot.

        /// <summary>
        /// FILE NAME FIELDS
        /// </summary>
        private readonly FileInfo fileProperties = new FileInfo("properties");
        private readonly FileInfo fileParameters = new FileInfo("default.parameters");
        private readonly FileInfo fileLearningParameters = new FileInfo("learning.parameters");
        private readonly FileInfo fileLetters = new FileInfo("letters");
        private readonly FileInfo fileVocab = new FileInfo("vocabulary");
        private readonly FileInfo filePhonemes = new FileInfo("phonemes");
        private readonly FileInfo fileGPCRules = new FileInfo("gpcrules");
        private readonly FileInfo fileActs = new FileInfo("activations.txt");
        private const string orthographicKnowledgeFilename = "orthographicknowledge.txt";

        /// <summary>
        /// WEIGHT MATRICES
        /// </summary>
        private float[][] vF1ToLetterWeights;       // stores excitation/inhibition from each VF to each letter
        private float[][] vF0ToLetterWeights;       // stores excitation/inhibition from each inverse VF to each letter

        /// <summary>
        /// NETWORK STATE FIELDS
        /// </summary>

        private string stimulus;                    // used in Simulate and ClampVisualFeatures methods
        private int stimulusLength;                 // used in both the Simulate and Search4Multi methods
        private float normMultiplier;               // L2O normalisation multiplier. Equals nLetterSlots / stimulusLength, 
                                                    // or 1 if L2O_NORMALISATION = false,
        private float[] outVisualFeatureLayer1;     // stores VF state according to the input stimulus
        private float[] outVisualFeatureLayer0;     // stores VF state according to the input stimulus

        private float[] netInputLetterLayer;        // arraysize equal to nSlots * nLetters to store net input to each letter node
        private float[] netInputOrtholexLayer;      // store net input to each learned OL word node. Resized each time new node learned.
        private float[] netInputPhonolexLayer;      // stores net input to each PL word node
        private float[] netInputPhonemeLayer;       // arraysize equal to nSlots * nPhonemes to store net input to each phoneme node

        private float[] actLetterLayer;             // arraysize equal to nSlots * nLetters to store net input to each letter node
        private float[] actOrtholexLayer;           // store activation of each learned OL word node. Resized each time new node learned.
        private float[] actPhonolexLayer;           // stores activation of each PL word node
        private float[] actPhonemeLayer;            // arraysize equal to nSlots * nPhonemes to store activation to each phoneme node

        private float actSemanticNode;              // activation of the semantic node (which excites the semantic layer).
                                                    // When random nodes are co-activated, they are assumed to have the same activation
                                                    // as the correct node, and there is no feedback to the semantic layer,
                                                    // so only keeping track of one value here.

        /// <summary>
        /// SIMPLE LEARNING PARAMETER FIELDS
        /// </summary>

        private bool learningOn;                    // turn off to prevent learning
        private bool l2o_normalisation;             // set to true to normalise input from L to O based on length of stimulus+blanks
        private int orthographicBlanks;             // 0 = no blanks, 1 = 1 blank, 2 = enough blanks so that each Onode is 8 letters long.
                                                    // Examples: 0: "CAT", 1: "CAT+", 2: "CAT+++++"
        
        private int nvFeatures;                     // number of visual features per letter
        private int maxCycles;                      // max number of cycles before timeout

        private bool printActivations;              // set to true to print an activations file. slows down the simulation.     
        private float minActivationReport;          // minimum activation to get reported in the activations.txt file.

        private float printedWordRecogThreshold;    // OL node must be above this value for a printed word to be "recognized".
        private float spokenWordRecogThreshold;     // PL node mus be above this value for a spoken word to be "recognized".
        private float semantic2PhonolexExcitation;
        private float semantic2PhonolexInhibition;
        private float contextInput2Semantic;        // Analogous to visual features, but for context. "Contextual feature".
        private int printedWordFreqMultiplier;      // Controls the rate at which printedWordFreq values are increased per exposure.
        private int numberOfSemanticRepsActivated;  // Controls the number of semantic nodes (including the correct one corresponding to the input word)
                                                    //  that are activated for a simulation. E.g., if value is 3, then the correct semantic node plus
                                                    //  two additional random words will receive activation.

        /// <summary>
        /// DRC-1.2.1 PARAMETER FIELDS
        /// </summary>

        //General Parameters
        private float activationRate;
        private float frequencyScale;
        private float minReadingPhonology;

        //Feature Level Parameters
        private float featureLetterExcitation;
        private float featureLetterInhibition;

        //Letter level Parameters
        private float letterOrthLexExcitation;
        private float letterOrthLexInhibition;
        private float letterLateralInhibition;

        //Orthographic Lexicon (OrthLex) Parameters
        private float orthLexPhonLexExcitation;
        private float orthLexPhonLexInhibition;
        private float orthLexLetterExcitation;
        private float orthLexLetterInhibition;
        private float orthLexLateralInhibition;

        //Phonological Lexicon (Phonlex) Parameters
        private float phonLexPhonemeExcitation;
        private float phonLexPhonemeInhibition;
        private float phonLexOrthLexExcitation;
        private float phonLexOrthLexInhibition;
        private float phonLexLateralInhibition;

        //Phoneme Level Parameters
        private float phonemePhonLexExcitation;
        private float phonemePhonLexInhibition;
        private float phonemeLateralInhibition;
        private float phonemeUnsupportedDecay;

        //GPC Route Parameters
        private float gpcPhonemeExcitation;
        private float gpcCriticalPhonology;
        private int gpcOnset;


        /// <summary>
        /// COUNT FIELDS
        /// These avoid the need to recalculate the
        /// same values over and over.
        /// </summary>
        private int lettersLength;      // number of letters + 1 for blankChar = 27 typically
        private int phonemesLength;     // number of phonemes + 1 for blankChar = 45 typically
        private int printedWordsCount;  // count of the printedWords list (must be recalculated before the start of each new simulated word.
        private int spokenWordsLength;  // length of the spokenWords array (number of known spoken words)
        private int currentVFNode;      // to avoid calculating slot*nVFeatures + currentFeature repeatedly
        private int currentLetterNode;  // to avoid calculating slot*lettersLength + currentLetter repeatedly
        private int currentPhonemeNode; // to avoid calculating slot*phonemesLength + currentPhoneme repeatedly

        /// <summary>
        /// LEARNING FIELDS
        /// </summary>
        private string[] context;         // holds the input context

        #endregion

        #region CONSTRUCTOR

        public LearningDRC(DirectoryInfo workingSubDir)
        {
            LoadProperties();
            LoadParameters();
            LoadLearningParameters();
            LoadLetters();
            LoadPhonemes();
            LoadVocabulary();
            LoadGPCs(fileGPCRules);
            InitialiseStateFields();
            InitialiseVF2LetterWeights();
            LoadOrthographicLexicon(workingSubDir);
            CreateCFSArrays();
        }


        /// <summary>
        /// Load drc parameters from the file
        /// default.parameters
        /// </summary>
        private void LoadParameters()
        {
            StreamReader streamR = null;
            try
            {
                streamR = fileParameters.OpenText();
            }
            catch
            {
                System.Console.WriteLine("There was a problem opening the default.parameters file.");
                System.Console.Write("Press any key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            string line;
            string[] splitline;

            do
            {
                line = streamR.ReadLine();

                if ((line == null) || (line == "") || (line[0] == '#'))
                    continue;

                splitline = line.Split(new char[] { ' ' });

                if (splitline.Length < 2)
                    continue;

                try
                {
                    switch (splitline[0])
                    {
                        case "ActivationRate":
                            activationRate = float.Parse(splitline[1]);
                            break;
                        case "FrequencyScale":
                            frequencyScale = float.Parse(splitline[1]);
                            break;
                        case "MinReadingPhonology":
                            minReadingPhonology = float.Parse(splitline[1]);
                            break;
                        case "FeatureLetterExcitation":
                            featureLetterExcitation = float.Parse(splitline[1]);
                            break;
                        case "FeatureLetterInhibition":
                            featureLetterInhibition = -float.Parse(splitline[1]);
                            break;
                        case "LetterOrthlexExcitation":
                            letterOrthLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "LetterOrthlexInhibition":
                            letterOrthLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "LetterLateralInhibition":
                            letterLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "OrthlexPhonlexExcitation":
                            orthLexPhonLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "OrthlexPhonlexInhibition":
                            orthLexPhonLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "OrthlexLetterExcitation":
                            orthLexLetterExcitation = float.Parse(splitline[1]);
                            break;
                        case "OrthlexLetterInhibition":
                            orthLexLetterInhibition = -float.Parse(splitline[1]);
                            break;
                        case "OrthlexLateralInhibition":
                            orthLexLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonlexPhonemeExcitation":
                            phonLexPhonemeExcitation = float.Parse(splitline[1]);
                            break;
                        case "PhonlexPhonemeInhibition":
                            phonLexPhonemeInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonlexOrthlexExcitation":
                            phonLexOrthLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "PhonlexOrthlexInhibition":
                            phonLexOrthLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonlexLateralInhibition":
                            phonLexLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonemePhonlexExcitation":
                            phonemePhonLexExcitation = float.Parse(splitline[1]);
                            break;
                        case "PhonemePhonlexInhibition":
                            phonemePhonLexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonemeLateralInhibition":
                            phonemeLateralInhibition = -float.Parse(splitline[1]);
                            break;
                        case "PhonemeUnsupportedDecay":
                            phonemeUnsupportedDecay = 1.0f - float.Parse(splitline[1]);
                            break;
                        case "GPCPhonemeExcitation":
                            gpcPhonemeExcitation = float.Parse(splitline[1]);
                            break;
                        case "GPCCriticalPhonology":
                            gpcCriticalPhonology = float.Parse(splitline[1]);
                            break;
                        case "GPCOnset":
                            gpcOnset = int.Parse(splitline[1]);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    System.Console.WriteLine("Had problems reading data out of default.parameters.");
                    System.Console.Write("Press a key to exit. ");
                    System.Console.ReadKey();
                    Environment.Exit(0);
                }

            } while (line != null);

            streamR.Close();
        }


        /// <summary>
        /// Load learning parameters from the file
        /// learning.parameters
        /// </summary>
        private void LoadLearningParameters()
        {
            StreamReader streamR = null;
            try
            {
                streamR = fileLearningParameters.OpenText();
            }
            catch
            {
                System.Console.WriteLine("There was a problem opening the learning.parameters file.");
                System.Console.Write("Press any key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            string line;
            string[] splitline;

            do
            {
                line = streamR.ReadLine();

                if ((line == null) || (line == "") || (line[0] == '#'))
                    continue;

                splitline = line.Split(new char[] { ' ' });

                if (splitline.Length < 2)
                    continue;

                try
                {
                    switch (splitline[0])
                    {
                        case "LearningOn":
                            if (int.Parse(splitline[1]) == 1)
                                learningOn = true;
                            else
                                learningOn = false;
                            break;
                        case "L2ONormalisation":
                            if (int.Parse(splitline[1]) == 1)
                                l2o_normalisation = true;
                            else
                                l2o_normalisation = false;
                            break;
                        case "OrthographicBlanks":
                            orthographicBlanks = int.Parse(splitline[1]);
                            break;
                        case "NVFeatures":
                            nvFeatures = int.Parse(splitline[1]);
                            break;
                        case "MaxCycles":
                            maxCycles = int.Parse(splitline[1]);
                            break;
                        case "PrintActivations":
                            if (int.Parse(splitline[1]) == 1)
                                printActivations = true;
                            else
                                printActivations = false;
                            break;
                        case "MinimumActivationReport":
                            minActivationReport = float.Parse(splitline[1]);
                            break;
                        case "PrintedWordRecognizedThreshold":
                            printedWordRecogThreshold = float.Parse(splitline[1]);
                            break;
                        case "SpokenWordRecognizedThreshold":
                            spokenWordRecogThreshold = float.Parse(splitline[1]);
                            break;
                        case "Semantic2PhonolexExcitation":
                            semantic2PhonolexExcitation = float.Parse(splitline[1]);
                            break;
                        case "Semantic2PhonolexInhibition":
                            semantic2PhonolexInhibition = -float.Parse(splitline[1]);
                            break;
                        case "ContextInput2Semantic":
                            contextInput2Semantic = float.Parse(splitline[1]);
                            break;
                        case "PrintedWordFrequencyMultiplier":
                            printedWordFreqMultiplier = int.Parse(splitline[1]);
                            break;
                        case "NumberOfSemanticRepsActivated":
                            numberOfSemanticRepsActivated = int.Parse(splitline[1]);
                            break;
                        default:
                            break;
                    }
                }
                catch
                {
                    System.Console.WriteLine("Had problems reading data out of learning.parameters.");
                    System.Console.Write("Press a key to exit. ");
                    System.Console.ReadKey();
                    Environment.Exit(0);
                }

            } while (line != null);

            streamR.Close();
        }


        /// <summary>
        /// Load drc properties from file.
        /// </summary>
        private void LoadProperties()
        {
            StreamReader streamR = null;
            string line;

            try
            {
                streamR = fileProperties.OpenText();
            }
            catch
            {
                System.Console.WriteLine("Could not open properties file.");
                System.Console.Write("Press a key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            string[] splitline;

            do
            {
                line = streamR.ReadLine();
                if (line == null)
                    continue;

                splitline = line.Split(new char[] { '=' });

                switch (splitline[0])
                {
                    case "Name":
                        name = splitline[1];
                        break;
                    case "Version":
                        version = splitline[1];
                        break;
                    case "ReleaseDate":
                        releaseDate = splitline[1];
                        break;
                    case "Creator":
                        creator = splitline[1];
                        break;
                    case "Url":
                        url = splitline[1];
                        break;
                    case "DefaultNumOrthAnalysisUnits":
                        nLetterSlots = int.Parse(splitline[1]);
                        break;
                    case "DefaultNumPhonemeUnits":
                        nPhonemeSlots = int.Parse(splitline[1]);
                        break;
                    default:
                        break;
                }

            } while (line != null);

            streamR.Close();
        }


        /// <summary>
        /// Load letters from the file "letters".
        /// </summary>
        private void LoadLetters()
        {
            List<int[]> lstLetterFeatures = new List<int[]>();     // list to load visual features for each letter from file
            List<char> lstLetters = new List<char>();              // list to load letters from file
            List<bool> lstVowelStatus = new List<bool>();         // list to load letter vowel status from file

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = fileLetters.OpenText();
            }
            catch
            {
                System.Console.WriteLine("There was a problem opening the letters file.");
                System.Console.Write("Press any key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                if ((line == null) || (line[0] == '#'))
                    continue;

                lstLetterFeatures.Add(new int[nvFeatures]);

                for (int i = 0; i < nvFeatures; i++)  // storing feature string for each letter
                {
                    if (line[(2 * i) + 6] == '1')
                    {
                        lstLetterFeatures.Last()[i] = 1;
                    }
                    else
                    {
                        lstLetterFeatures.Last()[i] = 0;
                    }
                }

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
                    System.Console.WriteLine("Error loading letter vowel status from file. Exiting...");
                    System.Console.ReadKey();
                    Environment.Exit(0);
                }

            } while (line != null);

            streamR.Close();

            // add in the blank slot char blankChar
            lstLetters.Add(BLANKCHAR);
            lstLetterFeatures.Add(new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

            //create letterfeature array and letter dictionaries, and store lengths
            letters = new Dictionary<int, char>();
            lettersReverseDictionary = new Dictionary<char, int>();
            vowelStatus = new bool[lstVowelStatus.Count];
            lettersLength = lstLetters.Count;
            letterFeatures = new int[lettersLength][];
            for (int j = 0; j < lettersLength; j++)
            {
                letterFeatures[j] = lstLetterFeatures[j];
                letters.Add(j, lstLetters[j]);
                lettersReverseDictionary.Add(lstLetters[j], j);
            }

            for (int j = 0; j < lstVowelStatus.Count; j++)
            {
                vowelStatus[j] = lstVowelStatus[j];
            }
        }


        /// <summary>
        /// Load phonemes from the file phonemes.
        /// </summary>
        private void LoadPhonemes()
        {
            List<char> lstPhonemes = new List<char>();             // list to load phonemes from file

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = filePhonemes.OpenText();
            }
            catch
            {
                System.Console.WriteLine("There was a problem opening the phonemes file.");
                System.Console.Write("Press a key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                if (line == null)
                    continue;

                lstPhonemes.Add(line[0]);

            } while (line != null);

            lstPhonemes.Add(BLANKCHAR);

            streamR.Close();

            //create phoneme array, and store length
            phonemes = new Dictionary<int, char>();
            phonemesReverseDictionary = new Dictionary<char, int>();
            phonemesLength = lstPhonemes.Count;
            for (int j = 0; j < phonemesLength; j++)
            {
                phonemes.Add(j, lstPhonemes[j]);
                phonemesReverseDictionary.Add(lstPhonemes[j], j);
            }
        }


        /// <summary>
        /// Load vocabulary from file vocabulary.
        /// Note: L-DRC uses the same vocabulary file as drc-1.2.1.
        /// The printed words are ignored by L-DRC, just looking
        /// to load the spoken words and their frequencies.
        /// NOTE: drc-1.2.1 ignores words that are >8letters in length.
        /// L-DRC, however, includes the spokenforms of these words in
        /// its phonological lexicon (an oversight, since the 9-letter
        /// orthographic forms are never presented during learning).
        /// </summary>
        private void LoadVocabulary()
        {
            // The format of each line in the vocabulary file is:
            // 0.printedWord 1.spokenWord 2."OP" 3.printedFreq 4.spokenFreq

            Dictionary<string, int> loadedSpokenWords = new Dictionary<string, int>();      // list to load spoken word vocabulary
            Dictionary<int, string> reverseLoadedSpokenWords = new Dictionary<int, string>();
            List<int> lstSpokenWordFreq = new List<int>();         // list to load spoken word frequencies

            StreamReader streamR = null;
            string line;
            string[] splitline;

            try
            {
                streamR = fileVocab.OpenText();
            }
            catch
            {
                System.Console.WriteLine("There was a problem opening the vocabulary file.");
                System.Console.Write("Press a key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            do
            {
                line = streamR.ReadLine();
                int indexSpoken;
                
                if (line == null)
                    continue;

                splitline = line.Split(new char[] { ' ' });

                if (loadedSpokenWords.ContainsKey(splitline[1])) // =true
                {
                    indexSpoken = loadedSpokenWords[splitline[1]];
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

            spokenWords = new string[loadedSpokenWords.Count];
            spokenWordsLength = spokenWords.Length;
            spokenWordFreq = new int[lstSpokenWordFreq.Count];

            for (int m = 0; m < spokenWordsLength; m++)
            {
                spokenWords[m] = reverseLoadedSpokenWords[m];
                spokenWordFreq[m] = lstSpokenWordFreq[m];
            }

            // find max spoken word frequency
            maxSpokenWordFreq = GetMaxIntFromArray(spokenWordFreq);

            // Need to add a single end of word character to each of the spoken words.
            for (int m = 0; m < spokenWords.Length; m++)
            {
                if (spokenWords[m].Length < nLetterSlots)
                {
                    spokenWords[m] = spokenWords[m] + "+";
                }
            }
        }


        /// <summary>
        /// Helper method to get the maximum integer from a list of integers.
        /// </summary>
        /// <param name="ary"></param>
        /// <returns></returns>
        private int GetMaxIntFromArray(int[] ary)
        {
            if (ary.Length == 0)  // catch empty array
                return 0;

            int max = ary[0];

            for (int i = 1; i < ary.Length; i++)
            {
                if (ary[i] > max)
                    max = ary[i];
            }
            return max;
        }


        /// <summary>
        ///  Load GPCs from file gpcrules.
        /// </summary>
        public void LoadGPCs(FileInfo gpcFile)
        {
            List<GPCRule> lstBodyRules = new List<GPCRule>();
            List<GPCRule> lstMultiRules = new List<GPCRule>();
            List<GPCRule> lstTwoRules = new List<GPCRule>();
            List<GPCRule> lstContextRules = new List<GPCRule>();
            List<GPCRule> lstMPhonRules = new List<GPCRule>();
            List<GPCRule> lstSingleRules = new List<GPCRule>();
            List<GPCRule> lstOutRules = new List<GPCRule>();

            StreamReader streamR = null;
            string line;

            try
            {
                streamR = gpcFile.OpenText();
            }
            catch
            {
                System.Console.WriteLine("Could not open gpcrules file.");
                System.Console.Write("Press a key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            string[] splitline;

            do
            {
                line = streamR.ReadLine();
                if ((line == null) || (line == ""))
                    continue;

                splitline = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

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

            bodyRules = new GPCRule[lstBodyRules.Count];
            for (int z = 0; z < bodyRules.Length; z++)
            {
                bodyRules[z] = lstBodyRules[z];
            }

            multiRules = new GPCRule[lstMultiRules.Count];
            for (int z = 0; z < multiRules.Length; z++)
            {
                multiRules[z] = lstMultiRules[z];
            }

            twoRules = new GPCRule[lstTwoRules.Count];
            for (int z = 0; z < twoRules.Length; z++)
            {
                twoRules[z] = lstTwoRules[z];
            }

            contextRules = new GPCRule[lstContextRules.Count];
            for (int z = 0; z < lstContextRules.Count; z++)
            {
                contextRules[z] = lstContextRules[z];
            }

            mphonRules = new GPCRule[lstMPhonRules.Count];
            for (int z = 0; z < mphonRules.Length; z++)
            {
                mphonRules[z] = lstMPhonRules[z];
            }

            // Add a rule for a blank letter activating the 
            // blank phoneme in the final position.
            lstSingleRules.Add(new GPCRule(new string[6] { "e", "sing", "+", "+", "u", "1.0" }));
            singleRules = new GPCRule[lstSingleRules.Count];
            for (int z = 0; z < lstSingleRules.Count; z++)
            {
                singleRules[z] = lstSingleRules[z];
            }

            outRules = new GPCRule[lstOutRules.Count];
            for (int z = 0; z < outRules.Length; z++)
            {
                outRules[z] = lstOutRules[z];
            }
        }


        /// <summary>
        /// Create arrays to contain network state fields for activations, net inputs etc.
        /// Initialise values to zero.
        /// </summary>
        private void InitialiseStateFields()
        {
            outVisualFeatureLayer1 = new float[nLetterSlots * nvFeatures];
            outVisualFeatureLayer0 = new float[nLetterSlots * nvFeatures];

            netInputLetterLayer = new float[nLetterSlots * lettersLength];
            netInputPhonolexLayer = new float[spokenWordsLength];
            netInputPhonemeLayer = new float[nPhonemeSlots * phonemesLength];

            // Note: ortholex netinput and activation arrays are resized
            // after every simulation to take account of any orthographic learning,
            // which is done in ResizeOrtholexArrays, not here.

            actLetterLayer = new float[nLetterSlots * lettersLength];
            actPhonolexLayer = new float[spokenWordsLength];
            actPhonemeLayer = new float[nPhonemeSlots * phonemesLength];
        }


        /// <summary>
        /// Create 2d arrays to contain connection strengths (weights)
        /// Set values for weights according to VFs, letters for those 2d arrays.
        /// </summary>
        private void InitialiseVF2LetterWeights()
        {
            vF0ToLetterWeights = new float[(nLetterSlots * nvFeatures)][];
            for (int i = 0; i < vF0ToLetterWeights.Length; i++)
            {
                vF0ToLetterWeights[i] = new float[(nLetterSlots * lettersLength)];
            }

            vF1ToLetterWeights = new float[(nLetterSlots * nvFeatures)][];
            for (int i = 0; i < vF1ToLetterWeights.Length; i++)
            {
                vF1ToLetterWeights[i] = new float[(nLetterSlots * lettersLength)];
            }

            // Loop across letter slots
            for (int i = 0; i < nLetterSlots; i++)
            {
                // Loop across the number of letters in each slot (27)
                for (int j = 0; j < lettersLength; j++)
                {
                    currentLetterNode = (i * lettersLength) + j;

                    // Loop across the number of visual features in each slot (14).
                    for (int k = 0; k < nvFeatures; k++)
                    {
                        currentVFNode = (i * nvFeatures) + k;

                        if (letterFeatures[j][k] == 1)
                        {
                            vF1ToLetterWeights[currentVFNode][currentLetterNode] = featureLetterExcitation;
                            vF0ToLetterWeights[currentVFNode][currentLetterNode] = featureLetterInhibition;
                        }
                        else
                        {
                            vF1ToLetterWeights[currentVFNode][currentLetterNode] = featureLetterInhibition;
                            vF0ToLetterWeights[currentVFNode][currentLetterNode] = featureLetterExcitation;
                        }
                    } // nvFeatures loop
                } // lettersLength loop
            } // nLetterSlots loop
        }


        public void ClearOrthographicLexicon()
        {
            printedWords = new List<string>();
            printedWordFreq = new List<int>();
            spokenWordsForEachPrintedWord = new List<List<int>>();
            printedWordsForEachSpokenWord = new List<int>[spokenWordsLength];
            printedWordsCount = 0;
            for (var m = 0; m < spokenWordsLength; m++)
            {
                printedWordsForEachSpokenWord[m] = new List<int>();
            }
        }

        /// <summary>
        /// Loads previously learned printed words and the spoken words to which they link from file.
        /// Format for orthographicknowledge.txt file:
        /// 1. "WORD"  2. printedword  3. printedwrd index  4. printedwrd freq  5. spokenword  6. spokenwrd index
        /// </summary>
        public void LoadOrthographicLexicon(DirectoryInfo workingSubDir)
        {
            StreamReader streamR;
            string line;
            var fileOrthographicKnowledge = new FileInfo(Path.Combine(workingSubDir.FullName, orthographicKnowledgeFilename));
            printedWords = new List<string>();
            printedWordFreq = new List<int>();
            spokenWordsForEachPrintedWord = new List<List<int>>();
            printedWordsForEachSpokenWord = new List<int>[spokenWordsLength];

            for (var m = 0; m < spokenWordsLength; m++)
            {
                printedWordsForEachSpokenWord[m] = new List<int>();
            }

            try
            {
                streamR = fileOrthographicKnowledge.OpenText();
            }

            catch
            {
                System.Console.WriteLine("No 'orthographicknowledge.txt' file found.");
                System.Console.Write("Creating new network... ");
                return;
            }

            do
            {

                line = streamR.ReadLine();

                if (line == null)
                    continue;

                var splitline = line.Split(new char[] { ' ' });

                if ((splitline[0]) == "#")
                    continue;

                switch (splitline[0])
                {
                    case "WORD":
                        try
                        {
                            printedWords.Add(splitline[1]);
                            printedWordFreq.Add(int.Parse(splitline[3]));

                            spokenWordsForEachPrintedWord.Add(new List<int>());

                            int sWord = 5;

                            do
                            {
                                printedWordsForEachSpokenWord[int.Parse(splitline[sWord])].Add(int.Parse(splitline[2]));
                                spokenWordsForEachPrintedWord[spokenWordsForEachPrintedWord.Count - 1].Add(int.Parse(splitline[sWord]));
                                sWord += 2;
                            } while (sWord < splitline.Length);
                        }
                        catch
                        {
                            System.Console.WriteLine("Problem loading known printed words.");
                            System.Console.Write("Press a key to exit. ");
                            System.Console.ReadKey();
                            Environment.Exit(0);
                        }
                        break;

                    default:
                        break;
                }
            } while (line != null);

            printedWordsCount = printedWords.Count;
            streamR.Close();
        }

        
        /// <summary>
        /// Creates the CFS Arrays and populates them using printedWordFreq and spokenWordFreq
        /// </summary>
        public void CreateCFSArrays()
        {
            printedCFS = new float[printedWordsCount];
            spokenCFS = new float[spokenWordsLength];

            maxPrintedWordFreq = GetMaxIntFromArray(printedWordFreq.ToArray());

            for (int l = 0; l < printedWordsCount; l++)
            {
                printedCFS[l] = (float)((Math.Log10(printedWordFreq[l] + 1) / Math.Log10(maxPrintedWordFreq + 1)) - 1) * frequencyScale;
            }

            maxSpokenWordFreq = GetMaxIntFromArray(spokenWordFreq.ToArray());

            for (int m = 0; m < spokenWordsLength; m++)
            {
                spokenCFS[m] = (float)((Math.Log10(spokenWordFreq[m] + 1) / Math.Log10(maxSpokenWordFreq + 1)) - 1) * frequencyScale;
            }
        }

        #endregion

        #region GENERAL METHODS

        /// <summary>
        /// PRIMARY method to execute a reading aloud simulation, to find
        /// the output from a given stimulus string.
        /// Learning is also called from here.
        /// </summary>
        /// <param name="newStimulus"></param>
        /// <param name="contextualInput"></param>
        /// <param name="workingSubDir"></param>
        /// <returns></returns>
        public string[] Simulate(string newStimulus, string contextualInput, DirectoryInfo workingSubDir)
        {
            var cycles = 0;
            var output = new string[2 + numberOfSemanticRepsActivated];
            var completed = false;

            stimulus = newStimulus;
            
            // Determine length of stimulus + blanks, for normalisation purposes.
            switch (orthographicBlanks)
            {
                case 0:
                    stimulusLength = stimulus.Length;
                    break;
                case 1:
                    if (stimulus.Length < nLetterSlots) // don't add 1 if the stimuli already takes up all slots.
                        stimulusLength = stimulus.Length + 1;
                    else
                        stimulusLength = stimulus.Length;
                    break;
                default:
                    stimulusLength = nLetterSlots;
                    break;
            }

            // Set L2O normalisation multiplier, based on stimulus length and whether or not normalisation is being used.
            if (l2o_normalisation == false)
            {
                normMultiplier = 1;
            }
            else
            {
                normMultiplier = (float)nLetterSlots / (float)stimulusLength;
            }

            // Called at the start of every new stimulus simulation, to account for any learning
            // that may have been done with previous stimuli.
            ResizeOrtholexArrays();

            ResetActivations(); // (set them to 0)
            ResetGPCRoute();

            ClampVisualFeatures();

            //Create the correct phonological lexicon context by determining what
            //random semantic nodes to activate, and adding a blank to the input correct context.
            if ((numberOfSemanticRepsActivated == 0) || (contextInput2Semantic <= 0.0001))
            {
                context = new string[1];
                context[0] = "+";
            }
            else
            {
                context = new string[numberOfSemanticRepsActivated];
                context[0] = string.Concat(contextualInput, "+");
                for (var wrd = 1; wrd < context.Length; wrd++)
                {
                    var randomWord = rnd.Next(spokenWords.Length);
                    context[wrd] = spokenWords[randomWord];

                    // make sure all context words are exclusive.
                    for (var wrdCheck = 0; wrdCheck < wrd; wrdCheck++)
                    {
                        if (context[wrd] == context[wrdCheck])
                        {
                            wrd = wrd - 1;
                        }
                    }
                }
            }

            // Main loop to run the reading alound simulation
            do
            {
                cycles++;

                ProcessSingleCycle(cycles);
                completed = CheckCompletion() || cycles == maxCycles;

                // terminate simulation if maxCycles reached.
            } while (completed == false);

            // Learning is done here
            if (learningOn == true)
            {
                Learning(workingSubDir);
                SaveLearnedOrthographicVocabulary(workingSubDir);
            }

            // Return output of reading aloud simulation
            output[0] = cycles.ToString();
            output[1] = GetOutput();

            for (var i = 0; i < numberOfSemanticRepsActivated; i++)
            {
                if ((numberOfSemanticRepsActivated == 0 || contextInput2Semantic <= 0.0001)) break;
                output[2 + i] = context[i];
            }

            return output;
        }


        /// <summary>
        /// Resize netInputOrtholexLayer and actOrtholexLayer arrays
        /// based on whether or not new words were added in previous sim.
        /// </summary>
        private void ResizeOrtholexArrays()
        {
            // Important: recalculating printedWordsCount
            // This needs to be done at the start of each new
            // simulation, since a new printedWord may have been
            // learned last simulation.
            printedWordsCount = printedWords.Count;

            //Important: resize ortholex netinput and activation arrays
            // at the start of each new simulation

            netInputOrtholexLayer = new float[printedWordsCount];
            actOrtholexLayer = new float[printedWordsCount];
        }


        /// <summary>
        /// Set all activations to zero, for the start of a simulation.
        /// </summary>
        private void ResetActivations()
        {
            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    actLetterLayer[(i * lettersLength) + j] = 0f;
                }
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                actPhonolexLayer[m] = 0f;
            }

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    actPhonemeLayer[(i * phonemesLength) + j] = 0f;
                }
            }
            actSemanticNode = 0f;
        }


        /// <summary>
        /// Clamp visual features to 0s and 1s according
        /// to the stimulus.
        /// </summary>
        /// <param name="stimulus"></param>
        private void ClampVisualFeatures()
        {
            int index;
            for (int i = 0; i < stimulus.Length; i++)
            {
                index = lettersReverseDictionary[stimulus[i]];

                for (int k = 0; k < nvFeatures; k++)
                {
                    currentVFNode = (i * nvFeatures) + k;

                    if (letterFeatures[index][k] == 1)
                    {
                        outVisualFeatureLayer1[currentVFNode] = 1;
                        outVisualFeatureLayer0[currentVFNode] = 0;
                    }
                    else // if letterFeatures[index][j] == 0
                    {
                        outVisualFeatureLayer1[currentVFNode] = 0;
                        outVisualFeatureLayer0[currentVFNode] = 1;
                    }
                }
            }

            //set any empty slots to blanks, depending on how many blanks to include
            // otherwise, clear visual features.

            // No blanks case
            if (orthographicBlanks == 0)
            // clear any empty slots of any visual feature activity
            {
                if (stimulus.Length < nLetterSlots)
                {
                    for (int i = stimulus.Length; i < nLetterSlots; i++)
                    {
                        for (int k = 0; k < nvFeatures; k++)
                        {
                            outVisualFeatureLayer1[(i * nvFeatures + k)] = 0;
                            outVisualFeatureLayer0[(i * nvFeatures + k)] = 0;
                        }
                    }
                }
            }

            // 1 blank case
            else if (orthographicBlanks == 1)
            {
                // activate visual features for a single extra blank
                // and clear visual features in any remaining slots
                // of all activity
                if (stimulus.Length < nLetterSlots)
                {
                    for (int i = stimulus.Length; i < nLetterSlots; i++)
                    {
                        if (i == stimulus.Length)
                        {
                            for (int k = 0; k < nvFeatures; k++)
                            {
                                outVisualFeatureLayer1[(i * nvFeatures + k)] = 0;
                                outVisualFeatureLayer0[(i * nvFeatures + k)] = 1;
                            }
                        }
                        else
                        {
                            for (int k = 0; k < nvFeatures; k++)
                            {
                                outVisualFeatureLayer1[(i * nvFeatures + k)] = 0;
                                outVisualFeatureLayer0[(i * nvFeatures + k)] = 0;
                            }
                        }
                    }
                }
            }

            // enough blanks to make up nLetterSlots case
            else if (orthographicBlanks == 2)
            {
                // activate visual features for a blank char in all remaining slots
                if (stimulus.Length < nLetterSlots)
                {
                    for (int i = stimulus.Length; i < nLetterSlots; i++)
                    {
                        for (int k = 0; k < nvFeatures; k++)
                        {
                            outVisualFeatureLayer1[(i * nvFeatures + k)] = 0;
                            outVisualFeatureLayer0[(i * nvFeatures + k)] = 1;
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Called by the Simulate method. Used to process a single cycle
        /// of the simulation, calculating net inputs, epsilons and
        /// activations across the whole model for a single cycle.
        /// </summary>
        private void ProcessSingleCycle(int cycles)
        {
            ProcessNetInputs();
            ProcessGPCRoute(cycles);
            ProcessNewActivations();
            ClipActivations();
            if (printActivations == true)
            {
                PrintRelevantActivationsToFile(cycles);
            }
        }


        /// <summary>
        /// Gets the output phoneme string by choosing
        /// the maximally activated phoneme from each slot.
        /// </summary>
        /// <returns>output phoneme string</returns>
        private string GetOutput()
        {
            StringBuilder output = new StringBuilder();
            int maxPhonemeIndex;
            float maxPhonemeAct;

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                maxPhonemeIndex = 0;
                maxPhonemeAct = 0f;

                for (int j = 0; j < phonemesLength; j++)
                {
                    currentPhonemeNode = (i * phonemesLength) + j;

                    if (actPhonemeLayer[currentPhonemeNode] > maxPhonemeAct)
                    {
                        maxPhonemeAct = actPhonemeLayer[currentPhonemeNode];
                        maxPhonemeIndex = j;
                    }
                }

                // check if end of word, and, if so, return output
                if (maxPhonemeIndex == phonemesReverseDictionary[BLANKCHAR])
                {
                    return output.ToString();
                }
                // else add maximally activated phoneme in the slot to the
                // output string, but add a space if there is no activated phoneme
                else
                {
                    if (maxPhonemeAct == 0f)
                    {
                        output.Append(' ');
                    }
                    else
                    {
                        output.Append(phonemes[maxPhonemeIndex]);
                    }
                }
            }
            return output.ToString();
        }


        /// <summary>
        /// Checks maximally activated phonemes in each slot,
        /// Returns true if each is above minReadingPhonology.
        /// </summary>
        /// <returns>Returns true/false, simulation complete</returns>
        private bool CheckCompletion()
        {
            int maxPhonemeIndex;
            float maxPhonemeAct;

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                maxPhonemeIndex = 0;
                maxPhonemeAct = 0f;

                for (int j = 0; j < phonemesLength; j++)
                {
                    currentPhonemeNode = (i * phonemesLength) + j;

                    if (actPhonemeLayer[currentPhonemeNode] > maxPhonemeAct)
                    {
                        maxPhonemeAct = actPhonemeLayer[currentPhonemeNode];
                        maxPhonemeIndex = j;
                    }
                }

                // check if max phoneme in slot is above minReadingPhonology.
                // If not, return false.
                if (maxPhonemeAct < minReadingPhonology)
                {
                    return false;
                }

                // if at end-of-word phoneme character, and all slots so far
                // have been above minreadingphonology, then break, return true.
                if (phonemes[maxPhonemeIndex] == BLANKCHAR)
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
                System.Console.WriteLine("There was a problem creating or opening the file for printing parameters.");
                System.Console.Write("Press any key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            streamW.WriteLine($"NumberOfLetterSlots:\t{nLetterSlots}");
            streamW.WriteLine($"NumberOfPhonemeSlots:\t{nPhonemeSlots}");
            streamW.WriteLine($"MaxPrintedWordFrequency:\t{maxPrintedWordFreq}");
            streamW.WriteLine($"MaxSpokenWordFrequency:\t{maxSpokenWordFreq}");
            streamW.WriteLine($"LearningOn:\t{learningOn}");
            streamW.WriteLine($"Letter2OLNormalisationOn:\t{l2o_normalisation}");
            streamW.WriteLine($"OrthographicBlanksCondition:\t{orthographicBlanks}");
            streamW.WriteLine($"VisualFeaturesPerLetter:\t{nvFeatures}");
            streamW.WriteLine($"MaximumCycles:\t{maxCycles}");
            streamW.WriteLine($"ThreholdForPrintedWordRecognition:\t{printedWordRecogThreshold}");
            streamW.WriteLine($"ThreholdForSpokenWordRecognition:\t{spokenWordRecogThreshold}");
            streamW.WriteLine($"Semantic2PhonolexExcitation:\t{semantic2PhonolexExcitation}");
            streamW.WriteLine($"Semantic2PhonolexInhibition:\t{semantic2PhonolexInhibition}");
            streamW.WriteLine($"ContextInput2Semantic(normalised):\t{contextInput2Semantic}");
            streamW.WriteLine($"NumberOfSemanticRepresentationsActivated:\t{numberOfSemanticRepsActivated}");
            streamW.WriteLine($"PrintedWordFrequencyTrainingMultiplier:\t{printedWordFreqMultiplier}");
            streamW.WriteLine($"ActivationRate:\t{activationRate}");
            streamW.WriteLine($"FrequencyScaling:\t{frequencyScale}");
            streamW.WriteLine($"MinimumPhonologyForReadingAloud:\t{minReadingPhonology}");
            streamW.WriteLine($"Feature2LetterExcitation:\t{featureLetterExcitation}");
            streamW.WriteLine($"Feature2LetterInhibition:\t{featureLetterInhibition}");
            streamW.WriteLine($"Letter2OrtholexExcitation:\t{letterOrthLexExcitation}");
            streamW.WriteLine($"Letter2OrtholexInhibition:\t{letterOrthLexInhibition}");
            streamW.WriteLine($"LetterLateralInhibition:\t{letterLateralInhibition}");
            streamW.WriteLine($"Ortholex2PhonolexExcitation:\t{orthLexPhonLexExcitation}");
            streamW.WriteLine($"Ortholex2PhonolexInhibition:\t{orthLexPhonLexInhibition}");
            streamW.WriteLine($"Ortholex2LetterExcitation:\t{orthLexLetterExcitation}");
            streamW.WriteLine($"Ortholex2LetterInhibition:\t{orthLexLetterInhibition}");
            streamW.WriteLine($"OrtholexLateralInhibition:\t{orthLexLateralInhibition}");
            streamW.WriteLine($"Phonolex2PhonemeExcitation:\t{phonLexPhonemeExcitation}");
            streamW.WriteLine($"Phonolex2PhonemeInhibition:\t{phonLexPhonemeInhibition}");
            streamW.WriteLine($"PhonoLlex2OrtholexExcitation:\t{phonLexOrthLexExcitation}");
            streamW.WriteLine($"PhonoLlex2OrtholexInhibition:\t{phonLexOrthLexInhibition}");
            streamW.WriteLine($"PhonoLlexLateralInhibition:\t{phonLexLateralInhibition}");
            streamW.WriteLine($"Phoneme2PhonolexExcitation:\t{phonemePhonLexExcitation}");
            streamW.WriteLine($"Phoneme2PhonolexInhibition:\t{phonemePhonLexInhibition}");
            streamW.WriteLine($"PhonemeLateralInhibition:\t{phonemeLateralInhibition}");
            streamW.WriteLine($"PhonemeUnsupportedDecay:\t{phonemeUnsupportedDecay}");
            streamW.WriteLine($"GPC2PhonemeExcitation:\t{gpcPhonemeExcitation}");
            streamW.WriteLine($"GPCCriticalPhonology:\t{gpcCriticalPhonology}");
            streamW.WriteLine($"GPCOnset:\t{gpcOnset}");

            streamW.Close();
        }
  


        /// <summary>
        /// Method prints activations of nodes that are above a particular threshhold
        /// each cycle. Also reports GPC route activity.
        /// </summary>
        /// <param name="cycles"></param>
        private void PrintRelevantActivationsToFile(int cycles)
        {
            StreamWriter streamW = null;

            try
            {
                streamW = new StreamWriter(fileActs.FullName, true);
            }
            catch
            {
                System.Console.WriteLine("There was a problem creating or opening the activations.txt file.");
                System.Console.Write("Press any key to exit. ");
                System.Console.ReadKey();
                Environment.Exit(0);
            }

            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    if (actLetterLayer[(i * lettersLength) + j] > minActivationReport)
                    {
                        streamW.WriteLine("Cycle{0}\tL{1} {2}\t{3}", cycles, i, actLetterLayer[(i * lettersLength) + j], letters[j]);
                    }
                }
            }

            for (int l = 0; l < printedWordsCount; l++)
            {
                if (actOrtholexLayer[l] > minActivationReport)
                {
                    streamW.WriteLine("Cycle{0}\tOrth {1}\t{2}", cycles, actOrtholexLayer[l], printedWords[l]);
                }
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                if (actPhonolexLayer[m] > minActivationReport)
                {
                    streamW.WriteLine("Cycle{0}\tPhon {1}\t{2}", cycles, actPhonolexLayer[m], spokenWords[m]);
                }
            }

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    if (actPhonemeLayer[(i * phonemesLength) + j] > minActivationReport)
                    {
                        streamW.WriteLine("Cycle{0}\tP{1} {2}\t{3}", cycles, i, actPhonemeLayer[(i * phonemesLength) + j], phonemes[j]);
                    }
                }
            }
            streamW.Close();
        }

        public float GetContextInput2SemanticValue()
        {
            return contextInput2Semantic;
        }

        public int GetNumberOfSemanticRepsActivated()
        {
            return numberOfSemanticRepsActivated;
        }

        public void SetParameter(string parameterName, float parameterValue)
        {
            switch (parameterName)
            {
                case "ActivationRate":
                    activationRate = parameterValue;
                    break;
                case "FrequencyScale":
                    frequencyScale = parameterValue;
                    break;
                case "MinReadingPhonology":
                    minReadingPhonology = parameterValue;
                    break;
                case "FeatureLetterExcitation":
                    featureLetterExcitation = parameterValue;
                    break;
                case "FeatureLetterInhibition":
                    featureLetterInhibition = -parameterValue;
                    break;
                case "LetterOrthlexExcitation":
                    letterOrthLexExcitation = parameterValue;
                    break;
                case "LetterOrthlexInhibition":
                    letterOrthLexInhibition = -parameterValue;
                    break;
                case "LetterLateralInhibition":
                    letterLateralInhibition = -parameterValue;
                    break;
                case "OrthlexPhonlexExcitation":
                    orthLexPhonLexExcitation = parameterValue;
                    break;
                case "OrthlexPhonlexInhibition":
                    orthLexPhonLexInhibition = -parameterValue;
                    break;
                case "OrthlexLetterExcitation":
                    orthLexLetterExcitation = parameterValue;
                    break;
                case "OrthlexLetterInhibition":
                    orthLexLetterInhibition = -parameterValue;
                    break;
                case "OrthlexLateralInhibition":
                    orthLexLateralInhibition = -parameterValue;
                    break;
                case "PhonlexPhonemeExcitation":
                    phonLexPhonemeExcitation = parameterValue;
                    break;
                case "PhonlexPhonemeInhibition":
                    phonLexPhonemeInhibition = -parameterValue;
                    break;
                case "PhonlexOrthlexExcitation":
                    phonLexOrthLexExcitation = parameterValue;
                    break;
                case "PhonlexOrthlexInhibition":
                    phonLexOrthLexInhibition = -parameterValue;
                    break;
                case "PhonlexLateralInhibition":
                    phonLexLateralInhibition = -parameterValue;
                    break;
                case "PhonemePhonlexExcitation":
                    phonemePhonLexExcitation = parameterValue;
                    break;
                case "PhonemePhonlexInhibition":
                    phonemePhonLexInhibition = -parameterValue;
                    break;
                case "PhonemeLateralInhibition":
                    phonemeLateralInhibition = -parameterValue;
                    break;
                case "PhonemeUnsupportedDecay":
                    phonemeUnsupportedDecay = 1.0f - parameterValue;
                    break;
                case "GPCPhonemeExcitation":
                    gpcPhonemeExcitation = parameterValue;
                    break;
                case "GPCCriticalPhonology":
                    gpcCriticalPhonology = parameterValue;
                    break;
                case "GPCOnset":
                    gpcOnset = (int)(parameterValue);
                    break;
                case "LearningOn":
                    learningOn = (int)parameterValue == 1;
                    break;
                case "PrintActivations":
                    printActivations = (int)parameterValue == 1;
                    break;
                case "L2ONormalisation":
                    l2o_normalisation = (int)parameterValue == 1;
                    break;
                case "OrthographicBlanks":
                    orthographicBlanks = (int)parameterValue;
                    break;
                case "NVFeatures":
                    nvFeatures = (int)parameterValue;
                    break;
                case "MaxCycles":
                    maxCycles = (int)parameterValue;
                    break;
                case "PrintedWordRecognizedThreshold":
                    printedWordRecogThreshold = parameterValue;
                    break;
                case "SpokenWordRecognizedThreshold":
                    spokenWordRecogThreshold = parameterValue;
                    break;
                case "Semantic2PhonolexExcitation":
                    semantic2PhonolexExcitation = parameterValue;
                    break;
                case "Semantic2PhonolexInhibition":
                    semantic2PhonolexInhibition = -parameterValue;
                    break;
                case "ContextInput2Semantic":
                    contextInput2Semantic = parameterValue;
                    break;
                case "PrintedWordFrequencyMultiplier":
                    printedWordFreqMultiplier = (int)parameterValue;
                    break;
                case "NumberOfSemanticRepsActivated":
                    numberOfSemanticRepsActivated = (int)parameterValue;
                    break;
                default:
                    break;
            }
        }

        #endregion

        #region LEXICAL ROUTE METHODS

        /// <summary>
        /// Calculate net inputs for a single cycle.
        /// </summary>
        private void ProcessNetInputs()
        {
            float netLetterToOrthlexInhibition = 0f;
            float netOrthlexToLetterInhibition = 0f;
            float netOtoPInhibition = 0f;
            float netPtoOInhibition = 0f;

            float[] netPhonemeToPhonlexInhibition = new float[nPhonemeSlots];
            float[] netPhonLexToPhonemeInhibition = new float[nPhonemeSlots];

            float[] netLetterLatInhibition = new float[nLetterSlots];
            float netOrtholexLatInhibition = 0f;
            float netPhonolexLatInhibition = 0f;
            float[] netPhonemeLatInhibition = new float[nPhonemeSlots];

            /// RESET NET INPUTS, MUST BE DONE EVERY CYCLE
            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    netInputLetterLayer[(i * lettersLength) + j] = 0f;
                }
            }

            for (int l = 0; l < printedWordsCount; l++)
            {
                netInputOrtholexLayer[l] = 0f;
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                netInputPhonolexLayer[m] = 0f;
            }

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    netInputPhonemeLayer[(i * phonemesLength) + j] = 0f;
                }
            }

            /// ADD IN WORD FREQUENCY CONTRIBUTIONS
            for (int l = 0; l < printedWordsCount; l++)
            {
                netInputOrtholexLayer[l] += printedCFS[l];
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                netInputPhonolexLayer[m] += spokenCFS[m];
            }

            /// PROCESS NET INPUT FROM THE VF LAYER
            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    currentLetterNode = (i * lettersLength) + j;

                    for (int k = 0; k < nvFeatures; k++)
                    {
                        currentVFNode = (i * nvFeatures) + k;

                        netInputLetterLayer[currentLetterNode] +=
                        (
                            outVisualFeatureLayer1[currentVFNode] * vF1ToLetterWeights[currentVFNode][currentLetterNode] +
                            outVisualFeatureLayer0[currentVFNode] * vF0ToLetterWeights[currentVFNode][currentLetterNode]
                        );
                    }
                }
            }

            /// PROCESS NET INPUT FROM THE LETTER TO ORTHOLEX LEVEL, AND VICE VERSA.

            // First, calculate the net L2Oinhibition by iterating across every letter node activation
            for (int letterLayerAbsolutePosition = 0; letterLayerAbsolutePosition < actLetterLayer.Length; letterLayerAbsolutePosition++)
            {
                netLetterToOrthlexInhibition += actLetterLayer[letterLayerAbsolutePosition] * letterOrthLexInhibition;
            }

            // Next iterate over every OL word node...
            for (int l = 0; l < printedWordsCount; l++)
            {
                // and add the netL2Oinhibition to its net input...
                netInputOrtholexLayer[l] += netLetterToOrthlexInhibition;

                // then, iterate across the letters in the current OL word node at position 'l'
                for (int i = 0; i < nLetterSlots; i++)
                {
                    // stop if the OL word length is < number of letter slots
                    if (i >= printedWords[l].Length)
                    {
                        break;
                    }
                    else
                    {
                        // find the position of letter 'i' in OL word node 'l' in the letter layer activation array.
                        int letterLayerAbsolutePosition = (i * lettersLength) + lettersReverseDictionary[printedWords[l][i]];

                        // and add the excitation from this node, and take away the inhibition from this node
                        // (because the inhibition was added when calculating net L2Oinhibition).
                        // The excitation is the learned value, and is found in the excitationsL2O list,
                        // at OL word node 'l', and position 'i'.
                        netInputOrtholexLayer[l] += actLetterLayer[letterLayerAbsolutePosition]
                                                                       * ((letterOrthLexExcitation * normMultiplier) - letterOrthLexInhibition);

                        // Do same for O2L excitation
                        netInputLetterLayer[letterLayerAbsolutePosition] +=
                                                    actOrtholexLayer[l] * (orthLexLetterExcitation - orthLexLetterInhibition);
                    }
                }
            }

            // By this point, the net input from the Letter to the OL layer has been calculated,
            // and the excitations from L2O and O2L have been calculated.
            // Now just need to calculate the rest of the O2L inhibition.

            // calculate net O2L inhibition by iterating over all OL word nodes
            for (int l = 0; l < printedWordsCount; l++)
            {
                netOrthlexToLetterInhibition += actOrtholexLayer[l] * orthLexLetterInhibition;
            }

            // iterate across every letter node, and add the net O2L inhibition to each node's net input.
            for (int letterLayerAbsolutePosition = 0; letterLayerAbsolutePosition < netInputLetterLayer.Length; letterLayerAbsolutePosition++)
            {
                netInputLetterLayer[letterLayerAbsolutePosition] += netOrthlexToLetterInhibition;
            }

            /// LATERAL INHIBITION IN THE LETTER LAYER
            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    currentLetterNode = (i * lettersLength) + j;
                    netLetterLatInhibition[i] += (actLetterLayer[currentLetterNode] * letterLateralInhibition);
                }
            }

            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    currentLetterNode = (i * lettersLength) + j;
                    netInputLetterLayer[currentLetterNode] += (netLetterLatInhibition[i] - actLetterLayer[currentLetterNode] * letterLateralInhibition);
                }
            }

            /// NET INPUT FROM OL to PL and VICE VERSA.
            for (int l = 0; l < printedWordsCount; l++)
            {
                netOtoPInhibition += actOrtholexLayer[l] * orthLexPhonLexInhibition;
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                netPtoOInhibition += actPhonolexLayer[m] * phonLexOrthLexInhibition;
            }

            for (int l = 0; l < printedWordsCount; l++)
            {
                netInputOrtholexLayer[l] += netPtoOInhibition;
                for (int x = 0; x < spokenWordsForEachPrintedWord[l].Count; x++)
                {
                    int currentSpokenWord = spokenWordsForEachPrintedWord[l][x];
                    // For each spoken word linked to a printed word, check that the printed
                    // word is also linked to the spoken word
                    int currentPrintedWordIndex = -1;
                    for (int x1 = 0; x1 < printedWordsForEachSpokenWord[currentSpokenWord].Count; x1++)
                    {
                        if (printedWordsForEachSpokenWord[currentSpokenWord][x1] == l)
                        {
                            currentPrintedWordIndex = x1;
                        }
                    }

                    if (currentPrintedWordIndex != -1)
                    {
                        netInputOrtholexLayer[l] += actPhonolexLayer[spokenWordsForEachPrintedWord[l][x]] * (phonLexOrthLexExcitation - phonLexOrthLexInhibition);
                    }
                }
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                netInputPhonolexLayer[m] += netOtoPInhibition;
                for (int x = 0; x < printedWordsForEachSpokenWord[m].Count; x++)
                {
                    int currentPrintedWord = printedWordsForEachSpokenWord[m][x];
                    // For each printed word linked to a spoken word, check that spoken
                    // word is also linked to the printed word
                    int currentSpokenWordIndex = -1;
                    for (int x1 = 0; x1 < spokenWordsForEachPrintedWord[currentPrintedWord].Count; x1++)
                    {
                        if (spokenWordsForEachPrintedWord[currentPrintedWord][x1] == m)
                        {
                            currentSpokenWordIndex = x1;
                        }
                    }

                    if (currentSpokenWordIndex != -1)
                    {
                        netInputPhonolexLayer[m] += actOrtholexLayer[printedWordsForEachSpokenWord[m][x]] * (orthLexPhonLexExcitation - orthLexPhonLexInhibition);
                    }
                }
            }

            /// LATERAL INHIBITION IN THE OL LAYER
            for (int l = 0; l < printedWordsCount; l++)
            {
                netOrtholexLatInhibition += (actOrtholexLayer[l] * orthLexLateralInhibition);
            }

            for (int l = 0; l < printedWordsCount; l++)
            {
                netInputOrtholexLayer[l] += (netOrtholexLatInhibition - actOrtholexLayer[l] * orthLexLateralInhibition);
            }

            /// LATERAL INHIBITION IN THE PL LAYER
            for (int m = 0; m < spokenWordsLength; m++)
            {
                netPhonolexLatInhibition += (actPhonolexLayer[m] * phonLexLateralInhibition);
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                netInputPhonolexLayer[m] += (netPhonolexLatInhibition - actPhonolexLayer[m] * phonLexLateralInhibition);
            }

            /// PROCESS NET INPUT FROM PHONLEX TO PHONEME LAYER, AND VICE VERSA.            
            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    netPhonemeToPhonlexInhibition[i] += (actPhonemeLayer[(i * phonemesLength) + j] * phonemePhonLexInhibition);
                }
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                //PL nodes are only connected to phoneme nodes for the same number of slots
                // as phonemes in the spoken word. e.g., for "k{t+", only activations from the
                // first 4 phoneme slots are considered.
                for (int i = 0; i < spokenWords[m].Length; i++)
                {
                    netInputPhonolexLayer[m] += netPhonemeToPhonlexInhibition[i];
                }

                for (int i = 0; i < nPhonemeSlots; i++)
                {
                    if (i >= spokenWords[m].Length)
                    {
                        break;
                    }
                    else
                    {
                        int phonemeLayerAbsolutePosition = (i * phonemesLength) + phonemesReverseDictionary[spokenWords[m][i]];

                        netInputPhonolexLayer[m] += (actPhonemeLayer[phonemeLayerAbsolutePosition]
                                                                       * (phonemePhonLexExcitation - phonemePhonLexInhibition));

                        netInputPhonemeLayer[phonemeLayerAbsolutePosition] +=
                                                    (actPhonolexLayer[m] * (phonLexPhonemeExcitation - phonLexPhonemeInhibition));
                    }
                }
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                //PL nodes are only connected to phoneme nodes for the same number of slots
                // as phonemes in the spoken word. e.g., for "k{t+", only the
                // first 4 phoneme slots receive inhibition from the "k{t+" PL node.
                for (int i = 0; i < spokenWords[m].Length; i++)
                {
                    netPhonLexToPhonemeInhibition[i] += (actPhonolexLayer[m] * phonLexPhonemeInhibition);
                }
            }

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    netInputPhonemeLayer[(i * nPhonemeSlots) + j] += netPhonLexToPhonemeInhibition[i];
                }
            }

            /// LATERAL INHIBITION IN THE PHONEME LAYER            
            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    currentPhonemeNode = (i * phonemesLength) + j;
                    netPhonemeLatInhibition[i] += (actPhonemeLayer[currentPhonemeNode] * phonemeLateralInhibition);
                }
            }

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    currentPhonemeNode = (i * phonemesLength) + j;
                    netInputPhonemeLayer[currentPhonemeNode] += (netPhonemeLatInhibition[i] - actPhonemeLayer[currentPhonemeNode] * phonemeLateralInhibition);
                }
            }

            /// CONTEXTUAL INPUT TO PHONOLOGICAL LEXICON LAYER
            for (int m = 0; m < spokenWordsLength; m++)
            {

                if (context.Contains(spokenWords[m]))
                {
                    netInputPhonolexLayer[m] += (actSemanticNode * semantic2PhonolexExcitation);
                }
                else
                {
                    netInputPhonolexLayer[m] += (actSemanticNode * semantic2PhonolexInhibition);
                }
            }
        }


        /// <summary>
        /// Calculate new activation levels after a single cycle.
        /// </summary>
        private void ProcessNewActivations()
        {
            float[] epsilonLetterLayer = new float[nLetterSlots * lettersLength];
            float[] epsilonOrtholexLayer = new float[printedWordsCount];
            float[] epsilonPhonolexLayer = new float[spokenWordsLength];
            float[] epsilonPhonemeLayer = new float[nPhonemeSlots * phonemesLength];
            float epsilonContextNode;

            /// LETTER LAYER
            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    currentLetterNode = (i * lettersLength) + j;

                    if (netInputLetterLayer[currentLetterNode] >= 0f)
                    {
                        epsilonLetterLayer[currentLetterNode] = netInputLetterLayer[currentLetterNode] * (1 - actLetterLayer[currentLetterNode]);
                    }
                    else
                    {
                        epsilonLetterLayer[currentLetterNode] = netInputLetterLayer[currentLetterNode] * actLetterLayer[currentLetterNode];
                    }

                    actLetterLayer[currentLetterNode] += (epsilonLetterLayer[currentLetterNode] * activationRate);
                }
            }

            /// ORTHOGRAPHIC LEXICON LAYER
            for (int l = 0; l < printedWordsCount; l++)
            {
                if (netInputOrtholexLayer[l] >= 0f)
                {
                    epsilonOrtholexLayer[l] = netInputOrtholexLayer[l] * (1 - actOrtholexLayer[l]);
                }
                else
                {
                    epsilonOrtholexLayer[l] = netInputOrtholexLayer[l] * actOrtholexLayer[l];
                }
                actOrtholexLayer[l] += (epsilonOrtholexLayer[l] * activationRate);
            }

            /// PHONOLOGICAL LEXICON LAYER
            for (int m = 0; m < spokenWordsLength; m++)
            {
                if (netInputPhonolexLayer[m] >= 0f)
                {
                    epsilonPhonolexLayer[m] = netInputPhonolexLayer[m] * (1 - actPhonolexLayer[m]);
                }
                else
                {
                    epsilonPhonolexLayer[m] = netInputPhonolexLayer[m] * actPhonolexLayer[m];
                }
                actPhonolexLayer[m] += (epsilonPhonolexLayer[m] * activationRate);
            }

            /// PHONEME LAYER
            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    currentPhonemeNode = (i * phonemesLength) + j;

                    // phoneme unsupported decay applied here
                    if (netInputPhonemeLayer[currentPhonemeNode] <= 0.00000000f)
                    {
                        actPhonemeLayer[currentPhonemeNode] = phonemeUnsupportedDecay * actPhonemeLayer[currentPhonemeNode];
                    }

                    // calculate epsilons
                    if (netInputPhonemeLayer[currentPhonemeNode] >= 0f)
                    {
                        epsilonPhonemeLayer[currentPhonemeNode] = netInputPhonemeLayer[currentPhonemeNode] * (1 - actPhonemeLayer[currentPhonemeNode]);
                    }
                    else
                    {
                        epsilonPhonemeLayer[currentPhonemeNode] = netInputPhonemeLayer[currentPhonemeNode] * actPhonemeLayer[currentPhonemeNode];
                    }

                    actPhonemeLayer[currentPhonemeNode] += (epsilonPhonemeLayer[currentPhonemeNode] * activationRate);

                }
            }

            /// SEMANTIC NODES
            epsilonContextNode = contextInput2Semantic * (1 - actSemanticNode);
            actSemanticNode += (epsilonContextNode * activationRate);
        }


        /// <summary>
        /// Ensures all activations stay between 0 and 1.
        /// </summary>
        private void ClipActivations()
        {
            for (int i = 0; i < nLetterSlots; i++)
            {
                for (int j = 0; j < lettersLength; j++)
                {
                    if (actLetterLayer[(i * lettersLength) + j] > 1.0f)
                    {
                        actLetterLayer[(i * lettersLength) + j] = 1.0f;
                    }
                    else if (actLetterLayer[(i * lettersLength) + j] < 0.0f)
                    {
                        actLetterLayer[(i * lettersLength) + j] = 0.0f;
                    }
                }
            }

            for (int l = 0; l < printedWordsCount; l++)
            {
                if (actOrtholexLayer[l] > 1.0f)
                {
                    actOrtholexLayer[l] = 1.0f;
                }
                else if (actOrtholexLayer[l] < 0.0f)
                {
                    actOrtholexLayer[l] = 0.0f;
                }
            }

            for (int m = 0; m < spokenWordsLength; m++)
            {
                if (actPhonolexLayer[m] > 1.0f)
                {
                    actPhonolexLayer[m] = 1.0f;
                }
                else if (actPhonolexLayer[m] < 0.0f)
                {
                    actPhonolexLayer[m] = 0.0f;
                }
            }

            for (int i = 0; i < nPhonemeSlots; i++)
            {
                for (int j = 0; j < phonemesLength; j++)
                {
                    if (actPhonemeLayer[(i * phonemesLength) + j] > 1.0f)
                    {
                        actPhonemeLayer[(i * phonemesLength) + j] = 1.0f;
                    }
                    else if (actPhonemeLayer[(i * phonemesLength) + j] < 0.0f)
                    {
                        actPhonemeLayer[(i * phonemesLength) + j] = 0.0f;
                    }
                }
            }
        }

#endregion

        #region LEARNING METHODS

        /// <summary>
        /// If the stimulus was a recognized spoken word, and the stimulus is novel, then
        /// this method adds a new OL node and creates new connections between this node and the
        /// PL layer and the Letter layer.
        /// If the stimulus was a recognized spoken AND printed word, then this method
        /// updates the excitations between the relevant OL node and the PL layer and
        /// the letter layer.
        /// </summary>
        private void Learning(DirectoryInfo workingSubDir)
        {
            var maxActivatedOWord = 0;
            var maxActivatedPWord = 0;
            var maxActivationO = 0f;
            var maxActivationP = 0f;

            // find maximally activated OL word node
            for (var l = 0; l < printedWordsCount; l++)
            {
                if (!(actOrtholexLayer[l] > maxActivationO)) continue;
                maxActivationO = actOrtholexLayer[l];
                maxActivatedOWord = l;
            }

            // find maximally activated PL word node
            for (var m = 0; m < spokenWordsLength; m++)
            {
                if (!(actPhonolexLayer[m] > maxActivationP)) continue;
                maxActivationP = actPhonolexLayer[m];
                maxActivatedPWord = m;
            }

            // Don't undertake learning if there was no spoken
            // word sufficiently well recognized.
            // i.e. the stimulus is not a known word.
            if (maxActivationP < spokenWordRecogThreshold)
            {
                return;
            }

            // If there is no sufficiently activated OL word node,
            // then learn a new OL word.
            // Otherwise, update excitations for an existing OL
            // word node.
            if (maxActivationO < printedWordRecogThreshold)
            {
                LearnNewOrthographicWord(maxActivatedPWord, workingSubDir);
            }
            else
            {
                LearnExistingOrthographicWord(maxActivatedOWord, maxActivatedPWord, workingSubDir);
            }
        }

        /// <summary>
        /// Subroutine of the Learning() method, that handles the learning of a new OL word.
        /// Called if there is a PL node above the threshold, but no OL node above the
        /// threshold (ie its a known spoken word, but there is no corresponding printed word.
        /// To add a new OL word node, need to:
        /// a) add the most activated letter from each slot as a new word to the PrintedWords list
        /// b) Add a new item to printedWordsForEachSpokenWord, connecting the relevant spoken
        ///    word to the newly minted printed word
        /// c) Add a new item to spokenWordsForEachPrintedWord, connecting the newly minted printed
        ///    word to the most active spoken word
        /// d) Add a new item to printedWordFreq, with starting value of printedWordFreqMultiplier
        /// e) Recalculate maxPrintedWordFreq, and printedCFS values
        /// </summary>
        /// <param name="maxActivatedPWord"></param>
        /// <param name="workingSubDir"></param>
        private void LearnNewOrthographicWord(int maxActivatedPWord, DirectoryInfo workingSubDir)
        {
            var path = Path.Combine(workingSubDir.FullName, "nodeUpdateLog.txt");
            var streamW = new StreamWriter(path, true);
            var blankSeen = false;
            var newPrintedWord = new StringBuilder();

            // a) Add the most active letters as a new printedWord

            // First, need to find maximally activated letter in each slot

            for (var i = 0; i < nLetterSlots; i++)
            {
                if (blankSeen == true)
                {
                    continue;
                }
                var indexMaxLetter = GetMaxLetterNode(i);

                // skip all blanks if we don't want any orthographic blanks
                if (orthographicBlanks == 0)
                {
                    if (letters[indexMaxLetter] == BLANKCHAR)
                    {
                        blankSeen = true;
                        continue;
                    }
                }
                // include 1 blank and skip the rest if we want only one orthographic blank
                else if (orthographicBlanks == 1)
                {
                    if (letters[indexMaxLetter] == BLANKCHAR)
                    {
                        blankSeen = true;
                    }
                }
                newPrintedWord.Append(letters[indexMaxLetter]);
            }

            var indexPrinted = printedWords.Count;
            printedWords.Add(newPrintedWord.ToString());
            printedWordsCount = printedWords.Count;

            streamW.WriteLine($"New O node created for the word: {printedWords[indexPrinted]}");
            System.Console.WriteLine($"New O node created for the word: {printedWords[indexPrinted]}");

            // b) and c) Add new items to printedWordsForEachSpokenWord, and printedWordsForEachPrintedWord

            printedWordsForEachSpokenWord[maxActivatedPWord].Add(indexPrinted);
            spokenWordsForEachPrintedWord.Add(new List<int>());
            spokenWordsForEachPrintedWord[indexPrinted].Add(maxActivatedPWord);

            // d) Add a new item to printedWordFreq, with starting value of printedWordFreqMultiplier

            printedWordFreq.Add(printedWordFreqMultiplier);

            // e) Recalculate maxPrintedWordFreq, and printedCFS values

            maxPrintedWordFreq = GetMaxIntFromArray(printedWordFreq.ToArray());
            printedCFS = new float[printedWordsCount];

            for (int l = 0; l < printedWordsCount; l++)
            {
                printedCFS[l] = (float)((Math.Log10(printedWordFreq[l] + 1) / Math.Log10(maxPrintedWordFreq + 1)) - 1) * frequencyScale;
            }          

            streamW.Close();
        }


        /// <summary>
        /// Subroutine of the Learning() method, that handles the updating of frequencies
        /// when improving the learning of an existing word.
        /// Called if there is a PL node above the threshold, and also an OL node about threshold.
        /// Need to:
        /// a) check if it is a novel homograph
        /// b) update printedWordFreq
        /// c) re-calculate maxPrintedWordFreq
        /// d) re-calculate all printedCFSs
        /// </summary>
        /// <param name="maxActivatedOWord"></param>
        /// <param name="maxActivatedPWord"></param>
        private void LearnExistingOrthographicWord(int maxActivatedOWord, int maxActivatedPWord, DirectoryInfo workingSubDir)
        {
            var path = Path.Combine(workingSubDir.FullName, "nodeUpdateLog.txt");
            var streamW = new StreamWriter(path, true);
            var novelHomograph = true;

            // a) Check if it is a novel homograph
            if (printedWordsForEachSpokenWord[maxActivatedPWord].Count != 0)
            {
                for (int x1 = 0; x1 < printedWordsForEachSpokenWord[maxActivatedPWord].Count; x1++)
                {
                    if (printedWordsForEachSpokenWord[maxActivatedPWord][x1] == maxActivatedOWord)
                    {
                        novelHomograph = false;
                        break;
                    }
                }
            }

            // If it is a novel homograph, make a new connection from the max activatedPWord
            // to the max activated OWord
            if (novelHomograph == true)
            {
                printedWordsForEachSpokenWord[maxActivatedPWord].Add(maxActivatedOWord);
                spokenWordsForEachPrintedWord[maxActivatedOWord].Add(maxActivatedPWord);
            }

            // b) update printedWordFreq

            printedWordFreq[maxActivatedOWord] += printedWordFreqMultiplier;

            // c) re-calculate maxPrintedWordFreq

            maxPrintedWordFreq = GetMaxIntFromArray(printedWordFreq.ToArray());

            // d) re-calculate all printedCFSs

            printedCFS = new float[printedWordsCount];

            for (var l = 0; l < printedWordsCount; l++)
            {
                printedCFS[l] = (float)((Math.Log10(printedWordFreq[l] + 1) / Math.Log10(maxPrintedWordFreq + 1)) - 1) * frequencyScale;
            }

            streamW.WriteLine(
                $"Updated frequency for word {printedWords[maxActivatedOWord]} to {printedWordFreq[maxActivatedOWord]}");
            System.Console.WriteLine(
                $"Updated frequency for word {printedWords[maxActivatedOWord]} to {printedWordFreq[maxActivatedOWord]}");

            streamW.Close();
        }

        #endregion

        #region GPC ROUTE METHODS

        /// The GPCRoute algorithms are the most difficult
        /// to follow. Rough reading ahead.
        
        /// <summary>
        /// Reset currentRightmostPhoneme and currentGPCInput,
        /// ready to process a new stimulus.
        /// </summary>
        private void ResetGPCRoute()
        {
            currentRightmostPhoneme = 0;
            currentGPCInput = "";
            lastSlotSeen = false;
        }

        /// <summary>
        /// Called by ProcessSingleCycle(). Handles GPC Route processing,
        /// including parsing, GPC Route output, and adding this output
        /// to the netinput of phoneme level nodes.
        /// </summary>
        private void ProcessGPCRoute(int cycles)
        {
            List<GPCRule> rulesApplied;

            // Don't start GPC route processing until a set number
            // of cycles (equal to gpcOnset) has passed.
            if (cycles >= gpcOnset)
            {
                rulesApplied = GraphemeParsing(cycles);
                AddGPCActivationToPhonemes(rulesApplied, cycles);
            }
        }


        /// <summary>
        /// Parse currentGPCInput and determines the GPC rules that apply.
        /// </summary>
        /// <param name="cycles"></param>
        /// <returns>List of GPC Rules that apply.</returns>
        private List<GPCRule> GraphemeParsing(int cycles)
        {
            int maxLetterInSlot;
            int maxPhonemeInSlot;
            int wordPosition = 0;
            string unconsumedLetters;
            bool splitGraphemeSeen = false; // flags when a split-grapheme is seen, so the parser
                                            // will know to look for a single letter next
            int lettersAfterSplit = 0; // keeps track of letters in a split grapheme after the '.', for word positioning purposes.
            GPCRule appliedRule;
            List<GPCRule> rulesApplied = new List<GPCRule>();

            // Add first letter
            if (currentGPCInput == "")
            {
                maxLetterInSlot = GetMaxLetterNode(0);
                currentGPCInput = letters[maxLetterInSlot].ToString();
            }

            // Add another letter to the currentGPCInput if
            // the current rightmost excited phoneme activation is
            // above the gpcCriticalPhonology, and provided there are more
            // letters to add. Do not add another letter if the current
            // letter is a blank.
            maxPhonemeInSlot = GetMaxPhonemeNode(currentRightmostPhoneme);
            if (actPhonemeLayer[(currentRightmostPhoneme * phonemesLength) + maxPhonemeInSlot] >= gpcCriticalPhonology)
            {
                if (currentGPCInput.Length == nLetterSlots)
                {
                    lastSlotSeen = true;
                }
                else if ((currentGPCInput.Length < nLetterSlots) && (currentGPCInput.Last() != BLANKCHAR))
                {
                    maxLetterInSlot = GetMaxLetterNode(currentGPCInput.Length);
                    currentGPCInput = string.Concat(currentGPCInput, letters[maxLetterInSlot].ToString());
                }
            }

            unconsumedLetters = currentGPCInput;

            while (unconsumedLetters != "")
            {
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
                        if ((appliedRule.RGrapheme.Contains('.')) && (appliedRule.RGrapheme[0] != '.'))
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
                    if (splitGraphemeSeen == true)
                    {
                        wordPosition += lettersAfterSplit;
                        splitGraphemeSeen = false;
                        lettersAfterSplit = 0;
                    }

                    rulesApplied.Add(appliedRule);
                    wordPosition = UpdateWordPosition(appliedRule, wordPosition);
                    unconsumedLetters = RemoveLetters(appliedRule, unconsumedLetters);
                    // Check if the rule found is a split-grapheme rule (excludes rules that start with the '.')
                    if ((appliedRule.RGrapheme.Contains('.')) && (appliedRule.RGrapheme[0] != '.'))
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
                        if ((appliedRule.RGrapheme.Contains('.')) && (appliedRule.RGrapheme[0] != '.'))
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
                    if (splitGraphemeSeen == true)
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
                    if (splitGraphemeSeen == true)
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
                else
                {
                    appliedRule = new GPCRule(new string[] { "A", "sing", unconsumedLetters[0].ToString(), "?", "u", "1.0" });
                    // If at this point, no rule has been found. Need to add in an
                    // additional phoneme to sort this out - probably need the
                    // end of word character, so that end-position rules can
                    // be used.
                    unconsumedLetters = unconsumedLetters.Substring(1);
                    continue;
                }
            }

            // Update rightmosthphoneme. Count the number of phoneme slots in the rules applied
            // to determine the right-most one.
            currentRightmostPhoneme = -1;
            // initialise to -1 so that if there is only 1 rule applied, it will be counted
            // and the rightmostphoneme will be set to slot 0.
            for (int z = 0; z < rulesApplied.Count; z++)
            {
                currentRightmostPhoneme += rulesApplied[z].RPhoneme.Length;
            }

            // set currentRightmostPhoneme to 0 if no rule was found to apply.
            if (currentRightmostPhoneme == -1)
                currentRightmostPhoneme = 0;

            return rulesApplied;
        }

        /// <summary>
        /// Returns the number of letters in a split grapheme after the split.
        /// This method is used to assist in calculating wordPosition.
        /// </summary>
        /// <param name="grapheme"></param>
        /// <returns></returns>
        private int GetLettersAfterSplit(string grapheme)
        {
            int lettersAfterSplit = 0;
            bool dotSeen = false;
            for (int ltr = 0; ltr < grapheme.Length; ltr++)
            {
                if (grapheme[ltr] == '.')
                    dotSeen = true;
                else if (dotSeen == true)
                    lettersAfterSplit++;
            }
            return lettersAfterSplit;
        }


        /// <summary>
        /// Using the list of GPC rules that apply, this method ensures that letter
        /// activation is contributed to the phoneme level via the GPC route.
        /// The logic in this method is complex. In summary: step through the rules, and
        /// find the average activation of the letters in each rule, and contribute that
        /// activation to the phoneme(s - will be more than one phoneme for mphon rules)
        /// according to phoneme excitation = average letter act * gpcphonemeexcitation.
        /// Need to ignore the letters in the grapheme of the rule that are either square
        /// brackets (which denote context letters) or are inside the square brackets.
        /// Also need to keep track of split graphemes - if a split grapheme is encountered,
        /// the very next grapheme must be the single-letter grapheme that is encompassed by
        /// the split grapheme. Set encompassedLetterPosition to the current letter slot
        /// to make sure that the next grapheme is processed in the correct letter position,
        /// before finishing off the rest of the letters in the split grapheme that come after
        /// the '.'.
        /// </summary>
        /// <param name="rulesApplied"></param>
        private void AddGPCActivationToPhonemes(List<GPCRule> rulesApplied, int cycles)
        {
            bool insideContextBrackets;     // keeps track of when stepping across context letters rather than grapheme letters in the rule.
            float averageLetterActivation;  // average activation across letters in a grapheme
            float totalLetterActivation;    // summed activation across letters in a grapheme
            int numberLettersInGrapheme;    // counts the number of letters in a grapheme
            int currentPhonemeSlot;         // used to keep track of the phoneme slots to which each grapheme corresponds.
            int currentLetterSlot;          // used to keep track of the letter slot.
            int encompassedLetterPosition;  // used to keep track while calculating splitGrapheme activations
            int encompassedContextLetterPosition;
            string phonemesActivated;

            insideContextBrackets = false;
            currentPhonemeSlot = 0;
            currentLetterSlot = 0;
            encompassedLetterPosition = 0;
            encompassedContextLetterPosition = 0;

            StreamWriter sw = new StreamWriter(fileActs.FullName, true);

            phonemesActivated = ApplyOutRules(rulesApplied, cycles, sw);

            for (int z = 0; z < rulesApplied.Count; z++)
            {
                bool targetGraphemeSeen = false;
                int contextLetterCount = 0;
                totalLetterActivation = 0f;
                numberLettersInGrapheme = 0;

                // If the weird context rule involving silent h is present,
                // it will be the final rule, and can be ignored without
                // messing up the rest of the processing.
                if ((rulesApplied[z].RPhoneme == "*") || (rulesApplied[z].RPhoneme == "?"))
                {
                    currentLetterSlot++;

                    if (printActivations == true)
                    {
                        sw.WriteLine("Cycle{0}\tGPC{1}\t{2}\t{3}\t{4}\t{5}", cycles, z, 0, rulesApplied[z].GetPosChar(), rulesApplied[z].RGrapheme, rulesApplied[z].RPhoneme);
                    }

                    continue;
                }
                else if (encompassedLetterPosition != 0)
                {
                    totalLetterActivation = actLetterLayer[(encompassedLetterPosition * lettersLength) + lettersReverseDictionary[rulesApplied[z].RGrapheme[0]]];
                    numberLettersInGrapheme = 1;
                    encompassedLetterPosition = 0;
                }
                else if (encompassedContextLetterPosition != 0)
                {
                    totalLetterActivation = actLetterLayer[(encompassedContextLetterPosition * lettersLength) + lettersReverseDictionary[rulesApplied[z].RGrapheme[0]]];
                    numberLettersInGrapheme = 1;
                    encompassedContextLetterPosition = 0;
                }
                else
                {
                    for (int ltr = 0; ltr < rulesApplied[z].RGrapheme.Length; ltr++)
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
                                {
                                    continue;
                                }
                                else
                                {
                                    encompassedLetterPosition = currentLetterSlot;
                                }
                                break;
                            default:
                                if (insideContextBrackets == true)
                                {
                                    if ((targetGraphemeSeen == true) &&
                                        (rulesApplied[z].RGrapheme[ltr] != '\\') &&
                                        (rulesApplied[z].RGrapheme[ltr] != '~'))
                                    {
                                        encompassedContextLetterPosition = currentLetterSlot;
                                        contextLetterCount++;
                                        currentLetterSlot++;
                                    }
                                    continue;
                                }

                                targetGraphemeSeen = true;
                                totalLetterActivation += actLetterLayer[(currentLetterSlot * lettersLength) + lettersReverseDictionary[rulesApplied[z].RGrapheme[ltr]]];
                                numberLettersInGrapheme++;
                                break;
                        }
                        currentLetterSlot++;
                    }
                }
                averageLetterActivation = totalLetterActivation / numberLettersInGrapheme;

                if (printActivations == true)
                {
                    sw.WriteLine("Cycle{0}\tGPC{1}\t{2}\t{3}\t{4}\t{5}", cycles, z, averageLetterActivation, rulesApplied[z].GetPosChar(), rulesApplied[z].RGrapheme, rulesApplied[z].RPhoneme);
                }

                foreach (char ph in rulesApplied[z].RPhoneme)
                {
                    netInputPhonemeLayer[(currentPhonemeSlot * phonemesLength) + phonemesReverseDictionary[phonemesActivated[currentPhonemeSlot]]] += (averageLetterActivation * gpcPhonemeExcitation);
                    currentPhonemeSlot++;
                }
            }

            sw.Close();
        }


        /// <summary>
        /// Searches the phonemes produced by the rules applied, looking for
        /// instances where an outrule should be used to change activated phonemes.
        /// </summary>
        /// <param name="rulesApplied"></param>
        /// <returns>string of phonemes to be activated in the phoneme layer</returns>
        private string ApplyOutRules(List<GPCRule> rulesApplied, int cycles, StreamWriter sw)
        {
            bool insideContextBrackets;
            bool preContextFound;
            bool postContextFound;
            bool affectedPhonemeSeen;
            bool affectedPhonemeMatch;
            bool outRuleApplied;
            StringBuilder preOutRulesPhonemes = new StringBuilder();
            StringBuilder afterOutRulesPhonemes = new StringBuilder();
            int thisPhoneme = 0;

            for (int z = 0; z < rulesApplied.Count; z++)
            {
                if (rulesApplied[z].RPhoneme != "*")
                {
                    preOutRulesPhonemes.Append(rulesApplied[z].RPhoneme);
                }

            }

            //loop through rules applied
            for (int z = 0; z < rulesApplied.Count; z++)
            {
                // If the weird context rule involving silent h is present,
                // it will be the last rule, and can be ignored without causing
                // problems with the rest of the processing.
                if (rulesApplied[z].RPhoneme == "*")
                {
                    continue;
                }
                // If the rule is protected, move on to phonemes after this rule.
                if (rulesApplied[z].RProtection == true)
                {
                    afterOutRulesPhonemes.Append(rulesApplied[z].RPhoneme);
                    thisPhoneme += rulesApplied[z].RPhoneme.Length;
                    continue;
                }

                //loop through phonemes in each rule applied
                for (int ph = 0; ph < rulesApplied[z].RPhoneme.Length; ph++)
                {
                    outRuleApplied = false;

                    //loop through outrules
                    for (int thisOutRule = 0; thisOutRule < outRules.Length; thisOutRule++)
                    {
                        insideContextBrackets = false;
                        preContextFound = false;
                        postContextFound = false;
                        affectedPhonemeSeen = false;
                        affectedPhonemeMatch = false;

                        //loop through each char in the output rule input string
                        for (int thisOutRuleChar = 0; thisOutRuleChar < outRules[thisOutRule].RGrapheme.Length; thisOutRuleChar++)
                        {
                            switch (outRules[thisOutRule].RGrapheme[thisOutRuleChar])
                            {
                                case '[':
                                    insideContextBrackets = true;
                                    break;
                                case ']':
                                    insideContextBrackets = false;
                                    break;
                                default:
                                    if (insideContextBrackets == true)
                                    {
                                        // if not at start of phoneme string, and previous phoneme matches preContextPhoneme, then a preContext has been found
                                        if (thisPhoneme != 0)
                                        {
                                            if ((outRules[thisOutRule].RGrapheme[thisOutRuleChar] == preOutRulesPhonemes[thisPhoneme - 1]) &&
                                                (affectedPhonemeSeen == false))
                                            {
                                                preContextFound = true;
                                            }
                                        }
                                        // if not at end of phoneme string, and the next phoneme matches a postContextPhoneme, then a postContext has been found
                                        if (thisPhoneme != preOutRulesPhonemes.Length - 1)
                                        {
                                            if ((outRules[thisOutRule].RGrapheme[thisOutRuleChar] == preOutRulesPhonemes[thisPhoneme + 1]) &&
                                                (affectedPhonemeSeen == true))
                                            {
                                                postContextFound = true;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        affectedPhonemeSeen = true;

                                        if (preOutRulesPhonemes[thisPhoneme] != outRules[thisOutRule].RGrapheme[thisOutRuleChar])
                                        {
                                            continue;
                                        }
                                        else
                                        {
                                            switch (outRules[thisOutRule].RPosition)
                                            {
                                                case GPCRule.RulePosition.beginning:
                                                    if (thisPhoneme != 0)
                                                    {
                                                        //phoneme matches, but position does not
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        //phoneme and position match
                                                        affectedPhonemeMatch = true;
                                                    }
                                                    break;

                                                case GPCRule.RulePosition.middle:
                                                    if ((thisPhoneme == 0) ||
                                                        (preOutRulesPhonemes[thisPhoneme] == BLANKCHAR) ||
                                                        ((thisPhoneme == preOutRulesPhonemes.Length - 2) && (preOutRulesPhonemes[thisPhoneme + 1] == BLANKCHAR)) ||
                                                        ((thisPhoneme == preOutRulesPhonemes.Length - 1) && lastSlotSeen == true))
                                                    {
                                                        //phoneme matches, but position does not
                                                        continue;
                                                    }
                                                    else
                                                    {
                                                        //phoneme and position match
                                                        affectedPhonemeMatch = true;
                                                    }
                                                    break;

                                                case GPCRule.RulePosition.end:
                                                    if (((thisPhoneme == preOutRulesPhonemes.Length - 2) &&
                                                        (preOutRulesPhonemes[thisPhoneme + 1] == BLANKCHAR)) ||
                                                        ((thisPhoneme == preOutRulesPhonemes.Length - 1) && lastSlotSeen == true))
                                                    {
                                                        //phoneme and position match
                                                        affectedPhonemeMatch = true;
                                                    }
                                                    else
                                                    {
                                                        //phoneme matches, but position does not
                                                        continue;
                                                    }
                                                    break;

                                                default: // all positions
                                                    affectedPhonemeMatch = true;
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                            }
                        }

                        if ((affectedPhonemeMatch == true) &&
                            ((preContextFound == true) || (postContextFound == true)))
                        {
                            outRuleApplied = true;
                            afterOutRulesPhonemes.Append(outRules[thisOutRule].RPhoneme);
                            // rule found and applied. stop looping through out rules, and move on to next phoneme.

                            if (printActivations == true)
                            {
                                sw.WriteLine("Cycle{0}\tOUT{1}\t{2}\t{3}\t{4}", cycles, z, outRules[thisOutRule].GetPosChar(), outRules[thisOutRule].RGrapheme, outRules[thisOutRule].RPhoneme);
                            }
                            break;
                        }
                    }

                    if (outRuleApplied == false)
                    {
                        afterOutRulesPhonemes.Append(rulesApplied[z].RPhoneme[ph]);
                    }

                    thisPhoneme++;
                }
            }
            return afterOutRulesPhonemes.ToString();
        }


        /// <summary>
        /// Counts relevant letters in the grapheme of the rule just applied,
        /// and modifies the word position accordingly, so that for ongoing
        /// parsing, the parser will know what part of the word it is up to
        /// and know if it should be looking for beginning, middle, or end rules.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Integer for word position.</returns>
        private int UpdateWordPosition(GPCRule rule, int wordPosition)
        {
            bool insideContextBrackets = false;
            int wp = wordPosition;
            for (int ltr = 0; ltr < rule.RGrapheme.Length; ltr++)
            {
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
                        {
                            break;
                        }
                        else
                        {
                            ltr = rule.RGrapheme.Length;
                        }
                        break;
                    default:
                        if (insideContextBrackets == true)
                        {
                            continue;
                        }
                        wp++;
                        break;
                }
            }
            return wp;
        }


        /// <summary>
        /// Removes the letters in the grapheme of the identified GPC rule from unconsumedLetters.
        /// </summary>
        /// <param name="rule"></param>
        /// <param name="unconsumedLetters"></param>
        /// <returns>String - remaining unconsumed letters, if any ("" returned if none left).</returns>
        private string RemoveLetters(GPCRule rule, string unconsumedLetters)
        {
            string newUnconsumedLetters = unconsumedLetters;
            bool insideContextBrackets = false;
            int positionInUnCLetters = 0;
            int possibleMiddleContexts = 0;
            bool targetGraphemeSeen = false;

            for (int i = 0; i < rule.RGrapheme.Length; i++)
            {
                if (newUnconsumedLetters == "")
                {
                    return "";
                }

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
                        {
                            break;
                        }
                        else
                        {
                            positionInUnCLetters++;
                        }
                        break;
                    default:
                        if (insideContextBrackets == true)
                        {
                            if ((targetGraphemeSeen == true) &&
                                (rule.RGrapheme[i] != '\\') &&
                                (rule.RGrapheme[i] != '~'))
                            {
                                possibleMiddleContexts++;
                            }
                            continue;
                        }
                        targetGraphemeSeen = true;
                        newUnconsumedLetters = newUnconsumedLetters.Remove(0 + positionInUnCLetters + possibleMiddleContexts, 1);
                        break;
                }
            }
            return newUnconsumedLetters;
        }


        /// <summary>
        /// Search for multi-rule in the remaining letters of the input.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <param name="currentGPCInptLength"></param>
        /// <returns>Returns GPC rule if one is found, otherwise returns null</returns>
        private GPCRule Search4Multi(string unconsumedLetters, int wordPosition)
        {
            bool ruleFound;

            // if rule is too small to be a multi-rule, then return null.
            // this still allows for the strange rules whose graphemes start with .
            // (e.g. end .ge  _) which are found when only two letters are present,
            // because these rules are end rules, and so the blankChar will be present
            // to make up 3 characters. However, this is not the case when it the number
            // of letters in the stimulus = the number of letter slots - no room for a
            // blank then. need to include code for this special case.
            if ((unconsumedLetters.Length < 3) && (stimulusLength != nLetterSlots))
            {
                return null;
            }

            if ((stimulusLength == nLetterSlots) && (unconsumedLetters.Length < 2))
            {
                return null;
            }

            //loop through all multirules, and return the first matching rule found.
            for (int z = 0; z < multiRules.Length; z++)
            {
                int dotStep = 0;

                if (stimulusLength != nLetterSlots)
                {
                    if (multiRules[z].RGrapheme.Length > unconsumedLetters.Length)
                        continue; // skip if grapheme is too big
                }
                else // if stimulus.Length = nLetterSlots
                {
                    if (multiRules[z].RGrapheme.Length > (unconsumedLetters.Length + 1))
                    {
                        continue; // for full-span stimuli, skip if grapheme is more than 1 letter too long
                    }
                    if ((multiRules[z].RGrapheme.Length == unconsumedLetters.Length + 1) &&
                        ((multiRules[z].RGrapheme[0] != '.') || (wordPosition + unconsumedLetters.Length != nLetterSlots)))
                    {
                        continue; // for full-span stimuli that are 1-letter too long, skip if not at end or not a starting '.' rule
                    }
                }

                // we will be assuming that the current rule is an applicable rule, unless it fails to match
                // on a particular letter, in which case we know it is false and move on to the next rule.
                ruleFound = true;

                for (int ltr = 0; ltr < multiRules[z].RGrapheme.Length; ltr++)
                {
                    // move on to next letter if it is the space in a split-grapheme rule.
                    if (multiRules[z].RGrapheme[ltr] == '.')
                    {
                        //check if . is at the start of the grapheme, and that wordPosition
                        // is not at the start of the word. otherwise, the weird rules where
                        // grapheme starts with . won't apply.
                        if ((ltr == 0) && (wordPosition != 0))
                        {
                            dotStep = 1;
                        }
                        continue;
                    }
                    if (multiRules[z].RGrapheme[ltr] != unconsumedLetters[ltr - dotStep])
                    {
                        ruleFound = false;
                        break;
                    }
                }
                if (ruleFound == true)
                {
                    if (wordPosition == 0)
                    {
                        switch (multiRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.beginning:
                                return multiRules[z];

                            case GPCRule.RulePosition.middle:
                                // candidate multirule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.end:
                                // candidate multirule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.all:
                                return multiRules[z];
                        }
                    }
                    else
                    {
                        switch (multiRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.beginning:
                                //candidate multirule doesn't apply because we are no longer
                                // at the beginning position. Move on.
                                break;

                            case GPCRule.RulePosition.middle:
                                //candidate multirule is ok in the middle position so long as
                                // it doesn't stretch all the way to the end position.
                                // split grapheme rules that include the last letter are ok if they
                                // correspond to a middle phoneme (graphemes starting with '.' not ok).
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if ((multiRules[z].RGrapheme.Length < unconsumedLetters.Length - 1) ||
                                        ((multiRules[z].RGrapheme.Length == unconsumedLetters.Length - 1) &&
                                         (multiRules[z].RGrapheme.Contains('.')) && (multiRules[z].RGrapheme[0] != '.')))
                                    {
                                        return multiRules[z];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (multiRules[z].RGrapheme.Length < unconsumedLetters.Length)
                                    {
                                        return multiRules[z];
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    return multiRules[z];
                                }

                            case GPCRule.RulePosition.end:
                                // candidate multirule is ok in the end position provided it
                                // stretches all the way to the end of the unconsumed letters.
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if (multiRules[z].RGrapheme.Length == unconsumedLetters.Length - 1 + dotStep)
                                    {
                                        return multiRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (multiRules[z].RGrapheme.Length == unconsumedLetters.Length + dotStep)
                                    {
                                        return multiRules[z];
                                    }
                                }
                                break;

                            case GPCRule.RulePosition.all:
                                return multiRules[z];
                        }
                    }
                }
            }
            // if no rule has been returned via the logic above, and we are at this point, then no
            // multi-rule is applicable. return null.
            return null;
        }


        /// <summary>
        /// Search for context-rule in the remaining letters of the input.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <param name="currentGPCInptLength"></param>
        /// <returns>Returns GPC rule if one is found, otherwise returns null</returns>
        private GPCRule Search4Context(string unconsumedLetters, int wordPosition, bool splitGraphemeSeen)
        {
            // if rule is too small to be a context-rule, then return null.
            // smallest possible context rule is a single letter, with a single
            // letter context, plus brackets, = 4 characters.
            if (wordPosition + unconsumedLetters.Length < 2)
            {
                return null;
            }

            //loop through all contextrules, and return the first matching rule found.
            for (int z = 0; z < contextRules.Length; z++)
            {
                List<string> preContexts = new List<string>();
                List<string> postContexts = new List<string>();
                List<string> middleContexts = new List<string>();

                StringBuilder preContextSB = new StringBuilder();
                StringBuilder postContextSB = new StringBuilder();

                GPCRule candidateRule = null;

                bool targetGraphemeSeen = false;
                bool possibleMiddleContext = false;
                bool definiteMiddleContext = false;
                bool insideContextBrackets = false;
                StringBuilder targetGrapheme = new StringBuilder();
                bool targetGraphemeMatch = false;
                bool graphemeAndContextMatch = false;

                // Divide the GPC's grapheme into context and grapheme.
                for (int i = 0; i < contextRules[z].RGrapheme.Length; i++)
                {
                    switch (contextRules[z].RGrapheme[i])
                    {
                        case '[':
                            insideContextBrackets = true;

                            // if a context is seen after a target grapheme has
                            // already been seen, then it is a possible middle context
                            // Not definite - it could be a post context.
                            if (targetGraphemeSeen == true)
                            {
                                possibleMiddleContext = true;
                            }
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
                            if (insideContextBrackets == true)
                            {
                                if (targetGraphemeSeen == false)
                                {
                                    if (contextRules[z].RGrapheme[i] == '\\')
                                    {
                                        preContextSB.Append('>');
                                    }
                                    else
                                    {
                                        preContextSB.Append(contextRules[z].RGrapheme[i]);
                                    }
                                }
                                else
                                {
                                    if (contextRules[z].RGrapheme[i] == '\\')
                                    {
                                        postContextSB.Append('>');
                                    }
                                    else
                                    {
                                        postContextSB.Append(contextRules[z].RGrapheme[i]);
                                    }
                                }
                            }
                            else
                            {
                                // if there is a possible middle context, and another letter
                                // for the target Grapheme is seen, then a post context is no longer
                                // possible, and it must definitely be a middle context.
                                // Middle contexts get stored in postContexts.
                                if (possibleMiddleContext == true)
                                {
                                    definiteMiddleContext = true;
                                    foreach (string z1 in postContexts)
                                    {
                                        middleContexts.Add(z1);
                                    }
                                    postContexts = new List<string>();

                                    // add '.' to the target grapheme equal to the number of middle
                                    // contexts present (will typically be only 1).
                                    for (int z1 = 0; z1 < middleContexts.Count; z1++)
                                    {
                                        targetGrapheme.Append('.');
                                    }
                                    possibleMiddleContext = false;
                                }
                                targetGraphemeSeen = true;
                                targetGrapheme.Append(contextRules[z].RGrapheme[i]);
                            }
                            break;
                    }
                }

                if (targetGrapheme.Length > unconsumedLetters.Length)
                {
                    //target grapheme too big for unconsumed letters. Move on to next rule.
                    continue;
                }

                if ((targetGrapheme.Length > 1) && (splitGraphemeSeen == true))
                {
                    // if the parser has just processed a split grapheme,
                    // then only context rules where the target is a single
                    // letter could apply.
                    continue;
                }

                // If at the start of the word, but the rule has a pre-context,
                // then it can't apply, move on to the next rule.
                if ((wordPosition == 0) && (preContexts.Count != 0))
                {
                    continue;
                }

                // If at the end of the word, but the rule has a post-context,
                // then it can't apply, move on to next rule.
                if ((targetGrapheme.Length == currentGPCInput.Length - wordPosition) &&
                    (unconsumedLetters.Last() == BLANKCHAR) &&
                    (postContexts.Count != 0) && (definiteMiddleContext == false))
                {
                    continue;
                }

                // we will be assuming that the current rule is an applicable rule, unless it fails to match
                // on a particular letter, in which case we know it is false and move on to the next rule.
                targetGraphemeMatch = true;
                for (int i = 0; i < targetGrapheme.Length; i++)
                {

                    // move on to next letter if it is the space in a split-grapheme rule.
                    if (targetGrapheme[i] == '.')
                    {
                        continue;
                    }
                    if (targetGrapheme[i] != unconsumedLetters[i])
                    {
                        targetGraphemeMatch = false;
                        break;
                    }
                }

                if (targetGraphemeMatch == true)
                {
                    if (wordPosition == 0)
                    {
                        switch (contextRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.beginning:
                                candidateRule = contextRules[z];
                                break;

                            case GPCRule.RulePosition.middle:
                                // candidate context-rule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.end:
                                // candidate context-rule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.all:
                                candidateRule = contextRules[z];
                                break;
                        }
                    }
                    else
                    {
                        switch (contextRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.beginning:
                                //candidate contextrule doesn't apply because we are no longer
                                // at the beginning position. Move on.
                                break;

                            case GPCRule.RulePosition.middle:
                                //candidate contextrule is ok in the middle position so long as
                                // it doesn't stretch all the way to the end position.
                                // split grapheme rules that include the last letter are ok if they
                                // correspond to a middle phoneme (graphemes starting with '.' not ok).
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if ((targetGrapheme.Length < unconsumedLetters.Length - 1) ||
                                        ((targetGrapheme.Length == unconsumedLetters.Length - 1) &&
                                         (targetGrapheme.ToString().Contains('.')) && (targetGrapheme[0] != '.')))
                                    {
                                        candidateRule = contextRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (targetGrapheme.Length < unconsumedLetters.Length)
                                    {
                                        candidateRule = contextRules[z];
                                    }
                                }
                                else
                                {
                                    candidateRule = contextRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.end:
                                // candidate contextrule is ok in the end position provided it
                                // stretches all the way to the end of the unconsumed letters.
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if (targetGrapheme.Length == unconsumedLetters.Length - 1)
                                    {
                                        candidateRule = contextRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (targetGrapheme.Length == unconsumedLetters.Length)
                                    {
                                        candidateRule = contextRules[z];
                                    }
                                }
                                break;

                            case GPCRule.RulePosition.all:
                                candidateRule = contextRules[z];
                                break;
                        }
                    }

                    // if the rule found was in the wrong position, it can't apply,
                    // move on to the next rule.
                    if (candidateRule == null)
                    {
                        continue;
                    }

                    // If this point is reached, then: 1) a rule has been found (ruleFound = true),
                    // meaning that the target grapheme from the rule was found in unconsumedLetters,
                    // and 2) the rule applies in the current word position (candidateRule != null).
                    // Now work out whether the context (whether pre or post) applies.

                    // assume there is a match, until a contradiction while trying to match
                    // context means there is no match
                    graphemeAndContextMatch = true;

                    // If pre context applies
                    if (preContexts.Count != 0)
                    {
                        //if there are more pre-contextual letters than
                        //letters prior to the current grapheme, then
                        // the rule can't apply, move on.
                        if (preContexts.Count > wordPosition)
                        {
                            continue;
                        }

                        for (int contextLetter = 0; contextLetter < preContexts.Count; contextLetter++)
                        {
                            if (preContexts[contextLetter] == ">V")
                            {
                                if (IsItAVowel(currentGPCInput[wordPosition - preContexts.Count + contextLetter]) == false)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else if (preContexts[contextLetter] == ">C")
                            {
                                if (IsItAVowel(currentGPCInput[wordPosition - preContexts.Count + contextLetter]) == true)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else if (preContexts[contextLetter].Contains('~'))
                            {
                                if (preContexts[contextLetter].Contains(currentGPCInput[wordPosition - preContexts.Count + contextLetter]))
                                {
                                    // presence of '~' means that the context is ok provided none of the letters mentioned in the context
                                    // are present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else
                            {
                                if (preContexts[contextLetter].Contains(currentGPCInput[wordPosition - preContexts.Count + contextLetter]) == false)
                                {
                                    //context letter is not present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                        }

                        // Move on to next rule if context hasn't matched so far.
                        if (graphemeAndContextMatch == false)
                        {
                            continue;
                        }
                        else
                        {
                            return contextRules[z];
                        }
                    }

                    else if (postContexts.Count != 0)
                    {
                        //if there are more post-contextual letters than
                        //letters subsequent to the current grapheme, then
                        // the rule can't apply, move on.
                        if (postContexts.Count > currentGPCInput.Length - wordPosition - targetGrapheme.Length)
                        {
                            continue;
                        }
                        if ((postContexts.Count == currentGPCInput.Length - wordPosition - targetGrapheme.Length) &&
                            (unconsumedLetters.Last() == BLANKCHAR))
                        {
                            continue;
                        }

                        for (int contextLetter = 0; contextLetter < postContexts.Count; contextLetter++)
                        {
                            if (postContexts[contextLetter] == ">V")
                            {
                                if (IsItAVowel(currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]) == false)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else if (postContexts[contextLetter] == ">C")
                            {
                                if (IsItAVowel(currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]) == true)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else if (postContexts[contextLetter].Contains('~'))
                            {
                                if (postContexts[contextLetter].Contains(currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]))
                                {
                                    // presence of '~' means that the context is ok provided none of the letters mentioned in the context
                                    // are present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else
                            {
                                if (postContexts[contextLetter].Contains(currentGPCInput[wordPosition + targetGrapheme.Length + contextLetter]) == false)
                                {
                                    //context letter is not present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                        }

                        // Move on to next rule if context hasn't matched so far.
                        if (graphemeAndContextMatch == false)
                        {
                            continue;
                        }
                        else
                        {
                            return contextRules[z];
                        }
                    }

                    else if (middleContexts.Count != 0)
                    {

                        int targetGraphemePreContextLetters;
                        for (targetGraphemePreContextLetters = 0; targetGraphemePreContextLetters < targetGrapheme.Length; targetGraphemePreContextLetters++)
                        {
                            if (targetGrapheme[targetGraphemePreContextLetters] == '.')
                            {
                                break;
                            }
                        }

                        //if the target grapheme (which includes '.'s for the middle context(s)
                        // is too big for remaining letters, move on.
                        if (targetGrapheme.Length > unconsumedLetters.Length)
                        {
                            continue;
                        }
                        if ((targetGrapheme.Length == unconsumedLetters.Length) &&
                            (unconsumedLetters.Last() == BLANKCHAR))
                        {
                            continue;
                        }

                        for (int contextLetter = 0; contextLetter < middleContexts.Count; contextLetter++)
                        {
                            if (middleContexts[contextLetter] == ">V")
                            {
                                if (IsItAVowel(currentGPCInput[wordPosition + targetGraphemePreContextLetters + contextLetter]) == false)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                            else if (middleContexts[contextLetter] == ">C")
                            {
                                if (IsItAVowel(currentGPCInput[wordPosition + targetGraphemePreContextLetters + contextLetter]) == true)
                                {
                                    //rule doesn't apply, drop out without returning anything, move on to next rule.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else if (middleContexts[contextLetter].Contains('~'))
                            {
                                if (middleContexts[contextLetter].Contains(currentGPCInput[wordPosition + targetGraphemePreContextLetters + contextLetter]))
                                {
                                    // presence of '~' means that the context is ok provided none of the letters mentioned in the context
                                    // are present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }

                            else
                            {
                                if (middleContexts[contextLetter].Contains(currentGPCInput[wordPosition + targetGraphemePreContextLetters + contextLetter]) == false)
                                {
                                    //context letter is not present in the relevant position in currentGPCInput.
                                    graphemeAndContextMatch = false;
                                    break;
                                }
                            }
                        }

                        // Move on to next rule if context hasn't matched so far.
                        if (graphemeAndContextMatch == false)
                        {
                            continue;
                        }
                        else
                        {
                            return contextRules[z];
                        }
                    }
                }
            }
            // if no rule has been returned via the logic above, and we are at this point, then no
            // context-rule is applicable. return null.
            return null;
        }


        /// <summary>
        /// Searches for a two rule in the remaining letters.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Returns a two rule if one found, otherwise, returns null.</returns>
        private GPCRule Search4TwoLetter(string unconsumedLetters, int wordPosition)
        {
            bool ruleFound;

            // if unconsumed letters is too small to have a two-rule, then return null.
            if (unconsumedLetters.Length < 2)
            {
                return null;
            }

            //loop through all tworules, and return the first matching rule found.
            for (int z = 0; z < twoRules.Length; z++)
            {
                // we will be assuming that the current rule is an applicable rule, unless it fails to match
                // on a particular letter, in which case we know it is false and move on to the next rule.
                ruleFound = true;

                // If there are only two letters left, then split-grapheme rules can't apply.
                // move past them.
                if ((unconsumedLetters.Length == 2) && (twoRules[z].RGrapheme.Contains('.')))
                {
                    continue;
                }

                for (int ltr = 0; ltr < twoRules[z].RGrapheme.Length; ltr++)
                {
                    // move on to next letter if it is the space in a split-grapheme rule.
                    if (twoRules[z].RGrapheme[ltr] == '.')
                    {
                        continue;
                    }
                    if (twoRules[z].RGrapheme[ltr] != unconsumedLetters[ltr])
                    {
                        ruleFound = false;
                        break;
                    }
                }
                if (ruleFound == true)
                {
                    if (wordPosition == 0)
                    {
                        switch (twoRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.beginning:
                                return twoRules[z];

                            case GPCRule.RulePosition.middle:
                                // candidate tworule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.end:
                                // candidate tworule doesn't apply if at the 0 position, move on.
                                break;

                            case GPCRule.RulePosition.all:
                                return twoRules[z];
                        }
                    }
                    else
                    {
                        switch (twoRules[z].RPosition)
                        {
                            case GPCRule.RulePosition.beginning:
                                //candidate tworule doesn't apply because we are no longer
                                // at the beginning position. Move on.
                                break;

                            case GPCRule.RulePosition.middle:
                                //candidate tworule is ok in the middle position so long as
                                // it doesn't stretch all the way to the end position.
                                // split grapheme rules that include the last letter are ok if they
                                // correspond to a middle phoneme (graphemes starting with '.' not ok).
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if ((twoRules[z].RGrapheme.Length < unconsumedLetters.Length - 1) ||
                                        ((twoRules[z].RGrapheme.Length == unconsumedLetters.Length - 1) &&
                                         (twoRules[z].RGrapheme.Contains('.')) && (twoRules[z].RGrapheme[0] != '.')))
                                    {
                                        return twoRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (twoRules[z].RGrapheme.Length < unconsumedLetters.Length)
                                    {
                                        return twoRules[z];
                                    }
                                }
                                else
                                {
                                    return twoRules[z];
                                }
                                break;

                            case GPCRule.RulePosition.end:
                                // candidate tworule is ok in the end position provided it
                                // stretches all the way to the end of the unconsumed letters.
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if (twoRules[z].RGrapheme.Length == unconsumedLetters.Length - 1)
                                    {
                                        return twoRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (twoRules[z].RGrapheme.Length == unconsumedLetters.Length)
                                    {
                                        return twoRules[z];
                                    }
                                }
                                break;

                            case GPCRule.RulePosition.all:
                                return twoRules[z];
                        }
                    }
                }
            }
            // if no rule has been returned via the logic above, and we are at this point, then no
            // two-rule is applicable. return null.
            return null;
        }


        /// <summary>
        /// Searches for a single rule in first (or only) position of the remaining letters.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Returns a single rule if found, otherwise, returns null.</returns>
        private GPCRule Search4Single(string unconsumedLetters, int wordPosition, bool splitGraphemeSeen)
        {
            for (int z = 0; z < singleRules.Length; z++)
            {
                if (singleRules[z].RGrapheme[0] != unconsumedLetters[0])
                {
                    continue;
                }

                if (wordPosition == 0)
                {
                    switch (singleRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.beginning:
                            return singleRules[z];

                        case GPCRule.RulePosition.middle:
                            // candidate singlerule doesn't apply if at the 0 position, move on.
                            break;

                        case GPCRule.RulePosition.end:
                            // even if it is an end rule, if there is only a single
                            // unconsumed letter, that means
                            // that the rule spans the entire stimulus, and is in the
                            // end position as well as the beginning position.
                            if (unconsumedLetters.Last() == BLANKCHAR)
                            {
                                if (unconsumedLetters.Length == 2)
                                {
                                    return singleRules[z];
                                }
                            }
                            else if (lastSlotSeen == true)
                            {
                                if (unconsumedLetters.Length == 1)
                                {
                                    return singleRules[z];
                                }
                            }
                            break;

                        case GPCRule.RulePosition.all:
                            return singleRules[z];
                    }
                }
                else
                {
                    switch (singleRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.beginning:
                            //candidate singlerule doesn't apply because we are no longer
                            // at the beginning position. Move on.
                            break;

                        case GPCRule.RulePosition.middle:
                            // candidate singlerule is ok in the middle position so long as
                            // there are additional letters, or if we are in the middle of
                            // a split grapheme.
                            if (splitGraphemeSeen == true)
                            {
                                return singleRules[z];
                            }
                            if (unconsumedLetters.Last() == BLANKCHAR)
                            {
                                if (unconsumedLetters.Length != 2)
                                {
                                    return singleRules[z];
                                }
                            }
                            else if (lastSlotSeen == true)
                            {
                                if (unconsumedLetters.Length != 1)
                                {
                                    return singleRules[z];
                                }
                            }
                            else
                            {
                                return singleRules[z];
                            }
                            break;

                        case GPCRule.RulePosition.end:
                            // candidate singlerule is ok in the end position provided there
                            // is only a single letter left in unconsumedLetters, and provided 
                            // the parser isn't currently in the middle of a split grapheme.
                            if (splitGraphemeSeen == false)
                            {
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if ((unconsumedLetters.Length == 2) ||
                                        (unconsumedLetters.Length == 1)) // this is for when only the blank character is left.
                                    {
                                        return singleRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (unconsumedLetters.Length == 1)
                                    {
                                        return singleRules[z];
                                    }
                                }
                            }
                            break;

                        case GPCRule.RulePosition.all:
                            return singleRules[z];
                    }
                }
            }
            // Shouldn't get to this point because there should always be a single rule in the
            // remaining unconsumed letters.
            return null;
        }


        /// <summary>
        /// Searches for a multi-phonic rule in first (or only) position of the remaining letters.
        /// </summary>
        /// <param name="unconsumedLetters"></param>
        /// <param name="wordPosition"></param>
        /// <returns>Returns a single rule if found, otherwise, returns null.</returns>
        private GPCRule Search4Mphon(string unconsumedLetters, int wordPosition, bool splitGraphemeSeen)
        {
            for (int z = 0; z < mphonRules.Length; z++)
            {
                if (mphonRules[z].RGrapheme[0] != unconsumedLetters[0])
                {
                    continue;
                }

                if (wordPosition == 0)
                {
                    switch (mphonRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.beginning:
                            return mphonRules[z];

                        case GPCRule.RulePosition.middle:
                            // candidate mphonrule doesn't apply if at the 0 position, move on.
                            break;

                        case GPCRule.RulePosition.end:
                            // even if it is an end rule, if there is only a mphon
                            // unconsumed letter, that means
                            // that the rule spans the entire stimulus, and is in the
                            // end position as well as the beginning position.
                            if (unconsumedLetters.Last() == BLANKCHAR)
                            {
                                if (unconsumedLetters.Length == 2)
                                {
                                    return mphonRules[z];
                                }
                            }
                            else if (lastSlotSeen == true)
                            {
                                if (unconsumedLetters.Length == 1)
                                {
                                    return mphonRules[z];
                                }
                            }
                            break;

                        case GPCRule.RulePosition.all:
                            return mphonRules[z];
                    }
                }
                else
                {
                    switch (mphonRules[z].RPosition)
                    {
                        case GPCRule.RulePosition.beginning:
                            //candidate mphonrule doesn't apply because we are no longer
                            // at the beginning position. Move on.
                            break;

                        case GPCRule.RulePosition.middle:
                            // candidate mphonrule is ok in the middle position so long as
                            // there are additional letters, or the parser has just parsed
                            // a split grpaheme.
                            if (splitGraphemeSeen == true)
                            {
                                return mphonRules[z];
                            }
                            if (unconsumedLetters.Last() == BLANKCHAR)
                            {
                                if (unconsumedLetters.Length != 2)
                                {
                                    return mphonRules[z];
                                }
                            }
                            else if (lastSlotSeen == true)
                            {
                                if (unconsumedLetters.Length != 1)
                                {
                                    return mphonRules[z];
                                }
                            }
                            else
                            {
                                return mphonRules[z];
                            }
                            break;

                        case GPCRule.RulePosition.end:
                            // candidate mphonrule is ok in the end position provided there
                            // is only a mphon letter left in unconsumedLetters, and the parser
                            // has not just processed a split grapheme.
                            if (splitGraphemeSeen == false)
                            {
                                if (unconsumedLetters.Last() == BLANKCHAR)
                                {
                                    if (unconsumedLetters.Length == 2)
                                    {
                                        return mphonRules[z];
                                    }
                                }
                                else if (lastSlotSeen == true)
                                {
                                    if (unconsumedLetters.Length == 1)
                                    {
                                        return mphonRules[z];
                                    }
                                }
                            }
                            break;

                        case GPCRule.RulePosition.all:
                            return mphonRules[z];
                    }
                }
            }
            // If at this point, no mphon rule was found.
            return null;
        }


        /// <summary>
        /// Returns highest activated letter in a slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private int GetMaxLetterNode(int slot)
        {
            int maxNode = 0;
            float maxAct = 0f;
            for (int j = 0; j < lettersLength; j++)
            {
                if (actLetterLayer[(slot * lettersLength) + j] > maxAct)
                {
                    maxAct = actLetterLayer[(slot * lettersLength) + j];
                    maxNode = j;
                }
            }
            return maxNode;
        }


        /// <summary>
        /// Returns highest activated phoneme in a slot.
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        private int GetMaxPhonemeNode(int slot)
        {
            int maxNode = 0;
            float maxAct = 0f;
            for (int j = 0; j < phonemesLength; j++)
            {
                if (actPhonemeLayer[(slot * phonemesLength) + j] > maxAct)
                {
                    maxAct = actPhonemeLayer[(slot * phonemesLength) + j];
                    maxNode = j;
                }
            }
            return maxNode;
        }


        /// <summary>
        /// Returns whether or not the input letter is a vowel
        /// </summary>
        /// <param name="letter"></param>
        /// <returns>true or false value</returns>
        private bool IsItAVowel(char letter)
        {
            return vowelStatus[lettersReverseDictionary[letter]];
        }

        #endregion

        #region SAVE NETWORK METHODS

        /// <summary>
        /// Save record of learned orthographic words, and to which phonological words
        /// they are connected, in the file orthographicknowledge.txt
        /// </summary>
        private void SaveLearnedOrthographicVocabulary(FileSystemInfo workingSubDir)
        {
            var path = Path.Combine(workingSubDir.FullName, orthographicKnowledgeFilename);
            var sw = new StreamWriter(path, false);

            sw.WriteLine("# orthographicknowledge.txt");
            sw.WriteLine("# This file records all known printed words, their indexes, and");
            sw.WriteLine("# the spoken words to which these printed words are connected..");
            sw.WriteLine("# FORMAT:");
            sw.WriteLine("# WORD <OWord> <OWordIndex> <OWordFreq> <SpokenWord1> <SpokenWord1Index> <SpokenWord2> ...");
            sw.WriteLine("#");
            sw.WriteLine(
                $"# Parameters: OWordThreshold: {printedWordRecogThreshold}  PWordThreshold: {spokenWordRecogThreshold}  C2PExcitation: {semantic2PhonolexExcitation}  MinReadingPhonology: {minReadingPhonology}");
            sw.WriteLine("#");

            // Write WORD lines
            var line = new StringBuilder();

            for (var l = 0; l < printedWordsCount; l++)
            {
                line.Append($"WORD {printedWords[l]} {l} {printedWordFreq[l]}");
                for (var x1 = 0; x1 < spokenWordsForEachPrintedWord[l].Count; x1++)
                {
                    line.Append(
                        $" {spokenWords[spokenWordsForEachPrintedWord[l][x1]]} {spokenWordsForEachPrintedWord[l][x1]}");
                }
                sw.WriteLine(line);
                line.Length = 0;
            }
            sw.Close();
        }

        #endregion

    }
}
