using DN_Henkel_Vision.Memory;
using DN_Henkel_Vision.Interface;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Data.Text;
using Windows.Media.Capture;

namespace DN_Henkel_Vision.Felber
{
    internal static class Felber
    {
        public static BackgroundWorker Analyzer = new();
        public static BackgroundWorker Classifier = new();

        public static bool Ready = false;

        private static string s_analyticDescription;

        private static string s_orderNumber;

        public static string Version = "1.3.2";

        /// <summary>
        /// Loads the AI models and initializes the background workers.
        /// </summary>
        public static void Initialize()
        {
            LoadModels();
            
            Analyzer.DoWork += Analyze;
            Analyzer.WorkerSupportsCancellation = true;

            Classifier.DoWork += Classify;
            Classifier.WorkerSupportsCancellation = true;
        }

        #region Classify

        /// <summary>
        /// Classifies the fault description and predicts all the properties of the fault.
        /// </summary>
        /// <param name="sender">Sender of the event</param>
        /// <param name="e">Event arguments</param>
        /// <exception cref="NotImplementedException"></exception>
        private static void Classify(object sender, DoWorkEventArgs e)
        {            
            while (Manager.Selected.PendingFaults.Count > 0)
            {
                Fault output = PredictFaultProperties(Manager.Selected.PendingFaults[0]);

                Manager.CurrentEditor.DispatcherQueue.TryEnqueue(() => { Manager.CurrentEditor.Felber_UpdateFault(output, s_orderNumber); });

                Manager.Selected.PendingFaults.RemoveAt(0);
            }
        }

        /// <summary>
        /// Requeues the fault for classification.
        /// </summary>
        public static void Requeue()
        {           
            if (Classifier.IsBusy || !Ready) { return; }

            s_orderNumber = Manager.Selected.OrderNumber;

            Classifier.RunWorkerAsync();
        }

        /// <summary>
        /// Predicts and assigns properties of the fault.
        /// </summary>
        /// <param name="input">Fault with description and cause</param>
        /// <returns>Fault with predicted properties</returns>
        private static Fault PredictFaultProperties(Fault input)
        {
            if (string.IsNullOrEmpty(input.Cause) || input.Cause == "Cause")
            {
                if (s_orderNumber.StartsWith("20") && Settings.SetAutoTesting) { input.Cause = "Testing"; }
                else if (s_orderNumber.StartsWith("20")){ return PredictLimitedFault(input); }
                else { input.Cause = PredictCause(input.Description); }
            }
            
            string description = Capitalize(input.Description);

            Fault output = new(description, input.Cause);

            output.Classification = PredictClassification(output.Description, output.Cause);
            output.Type = PredictType(output.Description, output.Cause, output.Classification);
            output.Component = PredictComponent(output.Description);

            if (s_orderNumber.StartsWith("20") && input.Placement != string.Empty)
            {
                 output.Placement = input.Placement;
            }
            else if (!s_orderNumber.StartsWith("20"))
            {
                output.Placement = Cache.LastPlacement;
            }


            output.ClassIndexes = AssignIndexes(output.Cause, output.Classification, output.Type);

            output.UserTime = input.UserTime;
            output.MachineTime = MachineTime(output.UserTime, output.Description.Length);

            return output;
        }

        private static Fault PredictLimitedFault(Fault input)
        {
            Fault output = new(input.Description);

            output.Component = PredictComponent(output.Description);

            if (input.Placement != string.Empty)
            {
                output.Placement = input.Placement;
            }

            return output;
        }
        
        /// <summary>
        /// Assigns the indexes of the cause, classification and type of the fault.
        /// </summary>
        /// <param name="cause">Cause of the fault</param>
        /// <param name="classification">Classification of the fault</param>
        /// <param name="type">Type of the fault</param>
        /// <returns>Array of indexes</returns>
        private static int[] AssignIndexes(string cause, string classification, string type)
        {
            int[] output = { -1, -1, -1 };

            if (!Memory.Classification.Causes.Contains(cause)) { return output; }

            output[0] = Array.FindIndex(Memory.Classification.Causes, x => x == cause);
            
            if (!Memory.Classification.Classifications[output[0]].Contains(classification)) { return output; }

            output[1] = Array.FindIndex(Memory.Classification.Classifications[output[0]], x => x == classification);

            if (!Memory.Classification.Types[Memory.Classification.ClassificationsPointers[output[0]][output[1]]].Contains(type)) { return output; }

            output[2] = Array.FindIndex(Memory.Classification.Types[Memory.Classification.ClassificationsPointers[output[0]][output[1]]], x => x == type);

            return output;
        }
        
        #endregion

        #region Analyze

        /// <summary>
        /// Analyzes the fault description and predicts the cause of the fault.
        /// </summary>
        /// <param name="sender">Sender of the event.</param>
        /// <param name="e">Event arguments.</param>
        private static void Analyze(object sender, DoWorkEventArgs e)
        {
            string cause = PredictCause(s_analyticDescription);

            Manager.CurrentEditor.DispatcherQueue.TryEnqueue(() => { Manager.CurrentEditor.Felber_UpdateCause(cause); });
        }
        
        /// <summary>
        /// Enqueues the fault description to be analyzed.
        /// </summary>
        /// <param name="description">Description of the fault.</param>
        public static void EnqueueAnalyze(string description)
        {
            if (Analyzer.IsBusy || !Ready) { return; }

            s_analyticDescription = description;

            Analyzer.RunWorkerAsync();
        }

        public static string Capitalize(string input)
        {
            string output = input;
            
            string[] words = input.Split(' ', '-');

            foreach (string word in words)
            {
                if (word.Any(char.IsDigit))
                {
                    input = input.Replace(word, word.ToUpper());
                }
            }

            return input;
        }

        #endregion

        #region Predictions

        /// <summary>
        /// Uses AI to predict the cause of the fault.
        /// </summary>
        /// <param name="description">Description of the fault.</param>
        /// <returns>Predicted cause of the fault.</returns>
        private static string PredictCause(string description)
        {
            Cause.ModelInput input = new()
            {
                Col0 = description,
            };

            Cause.ModelOutput output = Cause.Predict(input);

            return output.PredictedLabel;
        }

        /// <summary>
        /// Uses AI to predict the classification of the fault.
        /// </summary>
        /// <param name="description">Description of the fault.</param> 
        /// <param name="cause">Cause of the fault.</param>
        /// <returns>Predicted classification of the fault.</returns>
        private static string PredictClassification(string description, string cause)
        {
            Classification.ModelInput input = new()
            {
                Col0 = description,
                Col1 = cause,
            };

            Classification.ModelOutput output = Classification.Predict(input);

            return output.PredictedLabel;
        }

        /// <summary>
        /// Uses AI to predict the type of the fault.
        /// </summary>
        /// <param name="description">Description of the fault.</param>
        /// <param name="cause">Cause of the fault.</param>
        /// <param name="classification">Classification of the fault.</param>
        /// <returns>Predicted type of the fault.</returns>
        private static string PredictType(string description, string cause, string classification)
        {
            TypeModel.ModelInput input = new()
            {
                Col0 = description,
                Col1 = cause,
                Col2 = classification,
            };

            TypeModel.ModelOutput output = TypeModel.Predict(input);

            return output.PredictedLabel;
        }

        /// <summary>
        /// Uses AI to predict the component of the fault.
        /// </summary>
        /// <param name="description">Description of the fault.</param>
        /// <returns>Predicted component of the fault.</returns>
        private static string PredictComponent(string description)
        {
            char[] separators = new char[] { '/', '-', ':', ',', '.', ' ' };
            string[] words = description.Split(separators);
            List<string> potentionals = new();

            foreach (string word in words)
            {
                if (word.Any(char.IsDigit))
                {
                    potentionals.Add(word);
                }
            }

            if (potentionals.Count == 1)
            {
                return potentionals[0];
            }

            if (potentionals.Count > 1)
            {
                string component = "";
                float chance = 0f;

                // For each potentional component checks the probability by the AI model
                // and returns the one with the highest probability
                foreach (string word in potentionals)
                {                   
                    float current;

                    Component.ModelInput inputcomponent = new Component.ModelInput()
                    {
                        Col0 = word
                    };

                    Component.ModelOutput outputcomponent = Component.Predict(inputcomponent);

                    if (outputcomponent.PredictedLabel == 1)
                    {
                        current = outputcomponent.Score.Max();
                    }
                    else
                    {
                        current = outputcomponent.Score.Min();
                    }

                    if (current > chance)
                    {
                        chance = current;
                        component = word;
                    }
                }

                return component;
            }

            return words[0];
        }

        #endregion

        /// <summary>
        /// Loads the AI models in the background.
        /// </summary>
        private static void LoadModels()
        {
            PredictCause("");
            s_orderNumber = "38 000 000";
            PredictFaultProperties(new("DN7 HV7", "Preparation"));
            Ready = true;
        }

        private static float MachineTime(float user, int length)
        {
            float factor = (float)Math.Pow(1.12, (double)length / (double)Manager.AverageLength);

            float time = (Manager.AverageTime - ( user / factor ));

            if (time < 0) { return 0f; }

            time *= ((float)Random.Shared.Next(100, 120) / 100f);

            return time;
        }
    }
}
