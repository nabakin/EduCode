﻿using System;
using System.Net;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using Contoso.NoteTaker.Services.Ink;
using Windows.Graphics.Display;
using Windows.UI.Core;

using System.Diagnostics;
using Windows.Storage;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace NoteTaker
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NoteTaker : Page
    {
        InkRecognizer inkRecognizer;
        DisplayInformation displayInfo;

        private readonly DispatcherTimer dispatcherTimer;

        // Time to wait before triggering ink recognition operation
        const double IDLE_WAITING_TIME = 1000;

        public NoteTaker()
        {
            this.InitializeComponent();

            // Replace the subscriptionKey string value with your valid subscription key.
            Config c = new Config();
            string subscriptionKey = c.ApiKey;

            // URI information for ink recognition:
            const string endpoint = "https://api.cognitive.microsoft.com";
            const string inkRecognitionUrl = "/inkrecognizer/v1.0-preview/recognize";

            var inkPresenter = inkCanvas.InkPresenter;
            inkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Pen | Windows.UI.Core.CoreInputDeviceTypes.Mouse;

            inkPresenter.StrokeInput.StrokeStarted += InkPresenter_StrokeInputStarted;
            inkPresenter.StrokeInput.StrokeEnded += InkPresenter_StrokeInputEnded;
            inkPresenter.StrokesCollected += InkPresenter_StrokesCollected;
            inkPresenter.StrokesErased += InkPresenter_StrokesErased;

            inkRecognizer = new InkRecognizer(subscriptionKey, endpoint, inkRecognitionUrl);

            displayInfo = DisplayInformation.GetForCurrentView();
            inkRecognizer.SetDisplayInformation(displayInfo);

            /*
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Interval = TimeSpan.FromMilliseconds(IDLE_WAITING_TIME);
            */
        }

        private void InkPresenter_StrokeInputStarted(Windows.UI.Input.Inking.InkStrokeInput sender, PointerEventArgs args)
        {
            //StopTimer();
        }

        private void InkPresenter_StrokeInputEnded(Windows.UI.Input.Inking.InkStrokeInput sender, PointerEventArgs args)
            {
            //StartTimer();
        }

        private void InkPresenter_StrokesCollected(Windows.UI.Input.Inking.InkPresenter sender, Windows.UI.Input.Inking.InkStrokesCollectedEventArgs args)
        {
            //StopTimer();

            foreach (var stroke in args.Strokes)
            {
                inkRecognizer.AddStroke(stroke);
            }

            //StartTimer();
        }

        private void InkPresenter_StrokesErased(Windows.UI.Input.Inking.InkPresenter sender, Windows.UI.Input.Inking.InkStrokesErasedEventArgs args)
        {
            //StopTimer();

            foreach (var stroke in args.Strokes)
            {
                inkRecognizer.RemoveStroke(stroke.Id);
            }

            //StartTimer();
        }
        /*
        private async void DispatcherTimer_Tick(object sender, object e)
        {
            //StopTimer();

            try
            {
                var status = await inkRecognizer.RecognizeAsync();
                if (status == HttpStatusCode.OK)
                {
                    var root = inkRecognizer.GetRecognizerRoot();
                    if (root != null)
                    {
                        output.Text = OutputWriter.Print(root);
                    }
                }
            }
            catch(Exception ex)
            {
                output.Text = OutputWriter.PrintError(ex.Message);
            }
        }
        */
        private async void OnClickRecognize(object Sender, RoutedEventArgs e)
        {
            try
            {
                var status = await inkRecognizer.RecognizeAsync();
                if (status == HttpStatusCode.OK)
                {
                    var root = inkRecognizer.GetRecognizerRoot();
                    if (root != null)
                    {
                        //output.Text = OutputWriter.Print(root) + inkRecognizer.json;
                        output.Text = OutputWriter.Print(root);
                        Debug.Write(inkRecognizer.json);
                        StorageFile outputFile = await DownloadsFolder.CreateFileAsync("intermediary.json");
                        if (outputFile != null)
                        {
                            // Store file permissions for future access
                            Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(outputFile);

                            // Convert to Binary Buffer
                            var buffer = Windows.Security.Cryptography.CryptographicBuffer.ConvertStringToBinary(inkRecognizer.json, Windows.Security.Cryptography.BinaryStringEncoding.Utf8);
                            await Windows.Storage.FileIO.WriteBufferAsync(outputFile, buffer);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                output.Text = OutputWriter.PrintError(ex.Message);
            }
        }

        private void Interpret()
        {

        }
        //public void StartTimer() => dispatcherTimer.Start();
        //public void StopTimer() => dispatcherTimer.Stop();
    }
}
