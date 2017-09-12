using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;

namespace Learning_DRC
{
    internal sealed class BulkRun
    {
        private FileInfo _fileTokenCorpus;
        private FileInfo _fileTypeCorpus;
        private FileInfo _fileBulkRun;
        private readonly Dictionary<string, List<float>> _bulkRunParameters = new Dictionary<string, List<float>>();
        
        private readonly LearningDRC _model;
        
        private readonly DirectoryInfo _mainDirectory = new DirectoryInfo(".");
        private DirectoryInfo _workingSubDirectory;
        
        private readonly Stopwatch _sw = Stopwatch.StartNew();

        public BulkRun(LearningDRC model)
        {
            _model = model;
            GetFileNames();
            GetBulkRunParameters();
        }

        private void GetFileNames()
        {
            Console.WriteLine();
            Console.Write("Enter the filename for the bulkrun parameter variations: ");
            var bulkRun = Console.ReadLine();
            if (bulkRun != null)
                _fileBulkRun = new FileInfo(Path.Combine(_mainDirectory.FullName, bulkRun));
            else
                Environment.Exit(0);

            Console.WriteLine();
            Console.Write("Enter the filename for the token (training) corpus: ");
            var tokenCorpusName = Console.ReadLine();
            if (tokenCorpusName != null)
                _fileTokenCorpus = new FileInfo(Path.Combine(_mainDirectory.FullName, tokenCorpusName));
            else
                Environment.Exit(0);

            Console.WriteLine();
            Console.Write("Enter the filename for the type (testing) corpus: ");
            var typeCorpusName = Console.ReadLine();
            if (typeCorpusName != null)
                _fileTypeCorpus = new FileInfo(Path.Combine(_mainDirectory.FullName, typeCorpusName));
            else
                Environment.Exit(0);

            Console.WriteLine();
        }

        public void RunSimulations()
        {
            LoopOverParameterValues(_bulkRunParameters.Count - 1, "");
        }

        private void LoopOverParameterValues(int parameterIndexInDictionary, string subFolderName)
        {
            for (var j = 0; j < _bulkRunParameters[_bulkRunParameters.Keys.ElementAt(parameterIndexInDictionary)].Count; j++)
            {
                var paramName = _bulkRunParameters.Keys.ElementAt(parameterIndexInDictionary);
                var paramValue = _bulkRunParameters[_bulkRunParameters.Keys.ElementAt(parameterIndexInDictionary)][j];
                var subFolderNameLevelDown = new StringBuilder(subFolderName);

                _model.SetParameter(paramName, paramValue);
                subFolderNameLevelDown.AppendFormat($"{paramName}{paramValue}");

                if (parameterIndexInDictionary != 0)
                {
                    LoopOverParameterValues((parameterIndexInDictionary-1), subFolderNameLevelDown.ToString());
                }
                else
                {
                    _workingSubDirectory = _mainDirectory.CreateSubdirectory(subFolderNameLevelDown.ToString());
                    var heldContextInput2SemanticValue = _model.GetContextInput2SemanticValue();
                    _model.SetParameter("ContextInput2Semantic", (heldContextInput2SemanticValue/
                                                 _model.GetNumberOfSemanticRepsActivated()));
                    _model.ClearOrthographicLexicon();
                    RunTraining();
                    RunTesting();
                    _model.SetParameter("ContextInput2Semantic", heldContextInput2SemanticValue);
                }

            }
        }

        private void RunTraining()
        {
            _model.LoadGPCs(new FileInfo(Path.Combine(_mainDirectory.FullName, "gpcrules")));
            _model.SetParameter("LearningOn", 1);
            _model.SetParameter("PrintActivations", 0);
            var fileTrainingLog = new FileInfo(Path.Combine(_workingSubDirectory.FullName, "trainingLog.txt"));
            var fileTrainingParameters = new FileInfo(Path.Combine(_workingSubDirectory.FullName, "trainingParameters.txt"));
            _model.PrintParametersToFile(fileTrainingParameters);

            StreamReader streamR = null;
            try
            {
                streamR = _fileTokenCorpus.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the token corpus file.");
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

                //code to handle stimuli presented either with or without context.
                string[] output;
                if (splitline.Length == 2)
                {
                    _sw.Start();
                    output = _model.Simulate(splitline[0], splitline[1], _workingSubDirectory);
                    _sw.Stop();

                    var streamW = new StreamWriter(fileTrainingLog.FullName, true);

                    streamW.Write("RT: {0}  Input: {1}  Context: ", output[0], splitline[0]);
                    if (output.Length > 2)
                    {
                        for (var i = 2; i < output.Length; i++)
                        {
                            streamW.Write(" {0}", output[i]);
                        }
                    }
                    streamW.Write("  Output: {0}  ", output[1]);
                    Console.Write($"RT: {output[0]}  Input: {splitline[0]} Context: ");
                    if (output.Length > 2)
                    {
                        for (var i = 2; i < output.Length; i++)
                        {
                            Console.Write($" {output[i]}");
                        }
                    }
                    Console.Write($"  Output: {output[1]}  ");


                    streamW.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    streamW.Close();
                    Console.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    _sw.Reset();
                }
                else if (splitline.Length == 1)
                {
                    _sw.Start();
                    output = _model.Simulate(splitline[0], "no_context", _workingSubDirectory);
                    _sw.Stop();

                    var streamW = new StreamWriter(fileTrainingLog.FullName, true);

                    streamW.Write("RT: {0}  Input: {1}  Context: ", output[0], splitline[0]);
                    streamW.Write(" no_context");

                    streamW.Write("  Output: {0}  ", output[1]);
                    Console.Write($"RT: {output[0]}  Input: {splitline[0]} Context: ");
                    streamW.Write(" no_context");
                    Console.Write($"  Output: {output[1]}  ");


                    streamW.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    streamW.Close();
                    Console.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    _sw.Reset();
                }
                else
                {
                    var streamW = new StreamWriter(fileTrainingLog.FullName, true);
                    streamW.WriteLine("Bad input line skipped.");
                    streamW.Close();
                    Console.WriteLine("Bad input line skipped.");
                }


            } while (line != null);
            streamR.Close();
        }


        private void RunTesting()
        {
            _model.LoadGPCs(new FileInfo(Path.Combine(_mainDirectory.FullName, "gpcrules_default")));
            _model.SetParameter("LearningOn", 0);
            _model.SetParameter("PrintActivations", 0);
            _model.SetParameter("ContextInput2Semantic", 0);
            var fileTestingResults = new FileInfo(Path.Combine(_workingSubDirectory.FullName, "testingResults.txt"));
            var fileTestingParameters = new FileInfo(Path.Combine(_workingSubDirectory.FullName, "testingParameters.txt"));
            _model.PrintParametersToFile(fileTestingParameters);

            StreamReader streamR = null;
            try
            {
                streamR = _fileTypeCorpus.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the type corpus file.");
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

                //code to handle stimuli presented either with or without context.
                string[] output;
                if (splitline.Length == 2)
                {
                    _sw.Start();
                    output = _model.Simulate(splitline[0], splitline[1], _workingSubDirectory);
                    _sw.Stop();

                    var streamW = new StreamWriter(fileTestingResults.FullName, true);
                    streamW.Write("RT: {0}  Input: {1}  Context: <none>", output[0], splitline[0]);
                    streamW.Write("  Output: {0}  ", output[1]);
                    Console.Write($"RT: {output[0]}  Input: {splitline[0]} Context: <none>");
                    Console.Write($"  Output: {output[1]}  ");


                    streamW.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    streamW.Close();
                    Console.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    _sw.Reset();
                }
                else if (splitline.Length == 1)
                {
                    _sw.Start();
                    output = _model.Simulate(splitline[0], "no_context", _workingSubDirectory);
                    _sw.Stop();

                    var streamW = new StreamWriter(fileTestingResults.FullName, true);
                    streamW.Write("RT: {0}  Input: {1}  Context: ", output[0], splitline[0]);
                    streamW.Write(" no_context");
                    
                    streamW.Write("  Output: {0}  ", output[1]);
                    Console.Write($"RT: {output[0]}  Input: {splitline[0]} Context: ");
                    streamW.Write(" no_context");
                    Console.Write($"  Output: {output[1]}  ");

                    streamW.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    streamW.Close();
                    Console.WriteLine($"Sim_time: {_sw.ElapsedMilliseconds} ms");
                    _sw.Reset();
                }
                else
                {
                    var streamW = new StreamWriter(fileTestingResults.FullName, true);
                    streamW.WriteLine("Bad input line skipped.");
                    streamW.Close();
                    Console.WriteLine("Bad input line skipped.");
                }

            } while (line != null);
            streamR.Close();
        }


        private void GetBulkRunParameters()
        {
            StreamReader streamR = null;
            try
            {
                streamR = _fileBulkRun.OpenText();
            }
            catch
            {
                Console.WriteLine("There was a problem opening the bulkrun parameters file.");
                Console.Write("Press any key to exit. ");
                Console.ReadKey();
                Environment.Exit(0);
            }

            string line;

            do
            {
                line = streamR.ReadLine();

                if ((line == null) || (line == "") || (line[0] == '#'))
                    continue;

                var splitline = line.Split(' ');

                if (splitline.Length < 2)
                    continue;

                _bulkRunParameters.Add(splitline[0], new List<float>());
                try
                {
                    for (var i = 1; i < splitline.Length; i++)
                    {
                        _bulkRunParameters[splitline[0]].Add(float.Parse(splitline[i]));
                    }
                }
                catch
                {
                    Console.WriteLine("Had problems reading data out of the bulkrun parameters file.");
                    Console.Write("Press a key to exit. ");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                
            } while (line != null);
        }


        private string GetParameterFieldName(string parameterName)
        {
            switch (parameterName)
            {
                case "ActivationRate":
                    return "activationRate";
                case "FrequencyScale":
                    return "frequencyScale";
                case "MinReadingPhonology":
                    return "minReadingPhonology";
                case "FeatureLetterExcitation":
                    return "featureLetterExcitation";
                case "FeatureLetterInhibition":
                    return "featureLetterInhibition";
                case "LetterOrthlexExcitation":
                    return "letterOrthLexExcitation";
                case "LetterOrthlexInhibition":
                    return "letterOrthLexInhibition";
                case "LetterLateralInhibition":
                    return "letterLateralInhibition";
                case "OrthlexPhonlexExcitation":
                    return "orthLexPhonLexExcitation";
                case "OrthlexPhonlexInhibition":
                    return "orthLexPhonLexInhibition";
                case "OrthlexLetterExcitation":
                    return "orthLexLetterExcitation";
                case "OrthlexLetterInhibition":
                    return "orthLexLetterInhibition";
                case "OrthlexLateralInhibition":
                    return "orthLexLateralInhibition";
                case "PhonlexPhonemeExcitation":
                    return "phonLexPhonemeExcitation";
                case "PhonlexPhonemeInhibition":
                    return "phonLexPhonemeInhibition";
                case "PhonlexOrthlexExcitation":
                    return "phonLexOrthLexExcitation";
                case "PhonlexOrthlexInhibition":
                    return "phonLexOrthLexInhibition";
                case "PhonlexLateralInhibition":
                    return "phonLexLateralInhibition";
                case "PhonemePhonlexExcitation":
                    return "phonemePhonLexExcitation";
                case "PhonemePhonlexInhibition":
                    return "phonemePhonLexInhibition";
                case "PhonemeLateralInhibition":
                    return "phonemeLateralInhibition";
                case "PhonemeUnsupportedDecay":
                    return "phonemeUnsupportedDecay";
                case "GPCPhonemeExcitation":
                    return "gpcPhonemeExcitation";
                case "GPCCriticalPhonology":
                    return "gpcCriticalPhonology";
                case "GPCOnset":
                    return "gpcOnset";
                case "LearningOn":
                    return "learningOn";
                case "L2ONormalisation":
                    return "l2o_normalisation";
                case "OrthographicBlanks":
                    return "orthographicBlanks";
                case "NVFeatures":
                    return "nvFeatures";
                case "MaxCycles":
                    return "maxCycles";
                case "PrintedWordRecognizedThreshold":
                    return "printedWordRecogThreshold";
                case "SpokenWordRecognizedThreshold":
                    return "spokenWordRecogThreshold";
                case "Semantic2PhonolexExcitation":
                    return "semantic2PhonolexExcitation";
                case "Semantic2PhonolexInhibition":
                    return "semantic2PhonolexInhibition";
                case "ContextInput2Semantic":
                    return "contextInput2Semantic";
                case "PrintedWordFrequencyMultiplier":
                    return "printedWordFreqMultiplier";
                case "NumberOfSemanticRepsActivated":
                    return "numberOfSemanticRepsActivated";
            }
            return "";
        }
    }
    
    //private void CreateSubFolder()
    //{
    //    foreach (var parameterName in _bulkRunParameters.Keys)
    //    {
    //        _subFolderName.Append($"{parameterName}{_model.GetType().GetField(parameterName).GetValue(this)}");
    //    }
    //    _workingSubDirectory = _mainDirectory.CreateSubdirectory(_subFolderName.ToString());
    //}


}
