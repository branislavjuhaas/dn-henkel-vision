using DN_Henkel_Vision.Memory;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;

namespace DN_Henkel_Vision.Felber
{
    internal static class Felber
    {
        public static BackgroundWorker Analyzer = new();
        public static BackgroundWorker Classifier = new();

        public static bool Ready = false;

        private static string s_analyticDescription;

        public static void Initialize()
        {
            LoadModels();
            
            Analyzer.DoWork += Analyze;
            Analyzer.WorkerSupportsCancellation = true;
        }

        private static void Analyze(object sender, DoWorkEventArgs e)
        {
            string cause = PredictCause(s_analyticDescription);

            Manager.CurrentEditor.DispatcherQueue.TryEnqueue(() => { Manager.CurrentEditor.Felber_UpdateCause(cause); });
        }

        public static void EnqueueAnalyze(string description)
        {
            if (Analyzer.IsBusy || !Ready) { return; }

            s_analyticDescription = description;

            Analyzer.RunWorkerAsync();
        }

        /// <summary>
        /// Uses AI to predict the cause of the fault.
        /// </summary>
        /// <param name="description">Input description of the fault.</param>
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

        private static void LoadModels()
        {
            BackgroundWorker loader = new();

            loader.DoWork += (sender, e) => { PredictCause(""); Ready = true; };

            loader.RunWorkerAsync();
        }
    }
}
