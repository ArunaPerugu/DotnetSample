﻿// <SnippetAddUsings>
using System;
using System.IO;
using System.Linq;
using Microsoft.Data.DataView;
using Microsoft.ML;
using Microsoft.ML.Core.Data;
using Microsoft.ML.Data;
using Microsoft.ML.ImageAnalytics;
using Microsoft.ML.Trainers;
// </SnippetAddUsings>

namespace TransferLearningTF
{
    class Program
    {
        // <SnippetDeclareGlobalVariables>
        static string assetsPath = Path.Combine(Environment.CurrentDirectory, "assets");
        static string trainTagsTsv = Path.Combine(assetsPath, "inputs-train", "data", "tags.tsv");
        static string predictTagsTsv = Path.Combine(assetsPath, "inputs-predict", "data", "tags.tsv");
        static string trainImagesFolder = Path.Combine(assetsPath, "inputs-train", "data");
        static string predictImagesFolder = Path.Combine(assetsPath, "inputs-predict", "data");
        static string inceptionPb = Path.Combine(assetsPath, "inputs-train", "inception", "tensorflow_inception_graph.pb");
        static string inputImageClassifierZip = Path.Combine(assetsPath, "inputs-predict", "imageClassifier.zip");
        static string outputImageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");
        private static string LabelTokey = nameof(LabelTokey);
        private static string ImageReal = nameof(ImageReal);
        private static string PredictedLabelValue = nameof(PredictedLabelValue);
        // </SnippetDeclareGlobalVariables>

        static void Main(string[] args)
        {
            // Create MLContext to be shared across the model creation workflow objects 
            // <SnippetMLContext>
            MLContext mlContext = new MLContext();
            // </SnippetMLContext>

            // <SnippetCallBuildAndTrain>
            BuildAndTrainModel(mlContext, trainTagsTsv, trainImagesFolder, inceptionPb, outputImageClassifierZip);
            // </SnippetCallBuildAndTrain>

            // <SnippetCallClassifyImages>
            ClassifyImages(mlContext, predictTagsTsv, predictImagesFolder, outputImageClassifierZip);
            // </SnippetCallClassifyImages>
        }

        // <SnippetImageNetSettings>
        private struct ImageNetSettings
        {
            public const int imageHeight = 224;
            public const int imageWidth = 224;
            public const float mean = 117;
            public const float scale = 1;
            public const bool channelsLast = true;
        }
        // </SnippetImageNetSettings>

        // Build and train model
        public static void BuildAndTrainModel(MLContext mlContext, string dataLocation, string imagesFolder, string inputModelLocation, string outputModelLocation)
        {
            var featurizerModelLocation = inputModelLocation;

            Console.WriteLine("Read model");
            Console.WriteLine($"Model location: {featurizerModelLocation}");
            Console.WriteLine($"Images folder: {trainImagesFolder}");
            Console.WriteLine($"Training file: {dataLocation}");
            Console.WriteLine($"Default parameters: image size=({ImageNetSettings.imageWidth},{ImageNetSettings.imageHeight}), image mean: {ImageNetSettings.mean}");
            
            // <SnippetLoadData>
            var data = mlContext.Data.ReadFromTextFile<ImageNetData>(path: dataLocation, hasHeader: true);
            // </SnippetLoadData>

            // <SnippetMapValueToKey1>
            var pipeline = mlContext.Transforms.Conversion.MapValueToKey(outputColumnName: LabelTokey, inputColumnName: DefaultColumnNames.Label)
            // </SnippetMapValueToKey1>
                            // <SnippetImageTransforms>
                            .Append(mlContext.Transforms.LoadImages(trainImagesFolder, (ImageReal, nameof(ImageNetData.ImagePath))))
                            .Append(mlContext.Transforms.Resize(outputColumnName: ImageReal, imageWidth: ImageNetSettings.imageWidth, imageHeight: ImageNetSettings.imageHeight, inputColumnName: ImageReal))
                            .Append(mlContext.Transforms.ExtractPixels(new ImagePixelExtractorTransformer.ColumnInfo(name: "input", inputColumnName: ImageReal, interleave: ImageNetSettings.channelsLast, offset: ImageNetSettings.mean)))
                            // </SnippetImageTransforms>
                            // <SnippetScoreTensorFlowModel>
                            .Append(mlContext.Transforms.ScoreTensorFlowModel(modelLocation: featurizerModelLocation, outputColumnNames: new[] { "softmax2_pre_activation" }, inputColumnNames: new[] { "input" }))
                            // </SnippetScoreTensorFlowModel>
                            // <SnippetAddTrainer> 
                            .Append(mlContext.MulticlassClassification.Trainers.LogisticRegression(labelColumn: LabelTokey, featureColumn: "softmax2_pre_activation"))
                            // </SnippetAddTrainer>
                            // <SnippetMapValueToKey2>
                            .Append(mlContext.Transforms.Conversion.MapKeyToValue((PredictedLabelValue, DefaultColumnNames.PredictedLabel)));
                            // </SnippetMapValueToKey2>

            // Train the model
            Console.WriteLine("=============== Training classification model ===============");
            // Create and train the model based on the dataset that has been loaded, transformed.
            // <SnippetTrainModel>
            ITransformer model = pipeline.Fit(data);
            // </SnippetTrainModel>

            // Process the training data through the model
            // This is an optional step, but it's useful for debugging issues
            // <SnippetTransformData>
            var trainData = model.Transform(data);
            // </SnippetTransformData>

            // <SnippetEnumerateModel>
            var loadedModelOutputColumnNames = trainData.Schema
                .Where(column => !column.IsHidden).Select(column => column.Name);
            var trainData2 = mlContext.CreateEnumerable<ImageNetPipeline>(trainData, false, true).ToList();
            // </SnippetEnumerateModel>

            // <SnippetDisplayInfo>
            trainData2.ForEach(pr => Console.WriteLine($"ImagePath: {Path.GetFileName(pr.ImagePath)} predicted as: {pr.PredictedLabelValue} with score: {pr.Score.Max()}"));
            // </SnippetDisplayInfo>
            
            // Get some performance metrics on the model using training data
            Console.WriteLine("=============== Classification metrics ===============");   
 
            // <SnippetEvaluate>           
            var sdcaContext = new MulticlassClassificationCatalog(mlContext);

            var metrics = sdcaContext.Evaluate(trainData, label: LabelTokey, predictedLabel: DefaultColumnNames.PredictedLabel);
            // </SnippetEvaluate>
            
            //<SnippetDisplayMetrics>
            Console.WriteLine($"LogLoss is: {metrics.LogLoss}");
            Console.WriteLine($"PerClassLogLoss is: {String.Join(" , ", metrics.PerClassLogLoss.Select(c => c.ToString()))}");
            //</SnippetDisplayMetrics>

            // Save the model to assets/outputs
            Console.WriteLine("=============== Save model to local file ===============");

            // <SnippetSaveModel>
            using (var fileStream = new FileStream(outputModelLocation, FileMode.Create))
                mlContext.Model.Save(model, fileStream);
            // </SnippetSaveModel>

            Console.WriteLine($"Model saved: {outputModelLocation}");
        }

        public static void ClassifyImages(MLContext mlContext, string dataLocation, string imagesFolder, string outputModelLocation)
        {
            Console.WriteLine($"=============== Loading model ===============");
            Console.WriteLine($"Model loaded: {outputModelLocation}");

            // Load the model
            // <SnippetLoadModel>
            ITransformer loadedModel;
            using (var fileStream = new FileStream(outputModelLocation, FileMode.Open))
                loadedModel = mlContext.Model.Load(fileStream);
            // </SnippetLoadModel>

            // Make prediction function (input = ImageNetData, output = ImageNetPrediction)
            // <SnippetCreatePredictionEngine>  
            var predictor = loadedModel.CreatePredictionEngine<ImageNetData, ImageNetPrediction>(mlContext);
            // </SnippetCreatePredictionEngine> 

            // Read csv file into List<ImageNetData>
            // <SnippetReadFromCSV2> 
            var testData = ImageNetData.ReadFromCsv(dataLocation, imagesFolder).ToList();
            // </SnippetReadFromCSV2> 

            Console.WriteLine("=============== Making classifications ===============");
            // There is a bug (https://github.com/dotnet/machinelearning/issues/1138), 
            // that always buffers the response from the predictor
            // so we have to make a copy-by-value op everytime we get a response
            // from the predictor
            // <SnippetClassifications>            
            testData
                .Select(td => new { td, pred = predictor.Predict(td) })
                .Select(pr => (pr.td.ImagePath, pr.pred.PredictedLabelValue, pr.pred.Score))
                .ToList()
                .ForEach(pr => Console.WriteLine($"ImagePath: {Path.GetFileName(pr.ImagePath)} predicted as: {pr.PredictedLabelValue} with score: {pr.Score.Max()}"));
            // <SnippetClassifications>   
        }
    }

}
