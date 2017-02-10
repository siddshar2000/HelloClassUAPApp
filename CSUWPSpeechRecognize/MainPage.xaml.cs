using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.SpeechRecognition;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CSUWPSpeechRecognize
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CoreDispatcher dispatcher;
        private SpeechRecognizer speechRecognizer;
        private StringBuilder dictatedTextBuilder;
        private List<string> SampleData;

        public MainPageViewModel ViewModel { get; set; }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width < 500)
            {
                VisualStateManager.GoToState(this, "MinimalLayout", true);
            }
            else if (e.NewSize.Width < 700)
            {
                VisualStateManager.GoToState(this, "PortraitLayout", true);
            }
            else
            {
                VisualStateManager.GoToState(this, "DefaultLayout", true);
            }
        }

        public MainPage()
        {
            this.InitializeComponent();
            this.SampleData = GetSampleData();
            ViewModel = new MainPageViewModel();
            ViewModel.SourceData = SampleData;
            this.DataContext = this;
        }

        /// <summary>
        /// Initialize the speech recognizer
        /// </summary>
        /// <param name="e"></param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            dictatedTextBuilder = new StringBuilder();
            this.dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;
            speechRecognizer = new SpeechRecognizer(SpeechRecognizer.SystemSpeechLanguage);
            SpeechRecognitionCompilationResult result = await speechRecognizer.CompileConstraintsAsync();
            speechRecognizer.ContinuousRecognitionSession.ResultGenerated += ContinuousRecognitionSession_ResultGenerated;
            speechRecognizer.ContinuousRecognitionSession.Completed += ContinuousRecognitionSession_Completed;
            speechRecognizer.HypothesisGenerated += SpeechRecognizer_HypothesisGenerated;
            base.OnNavigatedTo(e);
        }

        private async void SpeechRecognizer_HypothesisGenerated(SpeechRecognizer sender, SpeechRecognitionHypothesisGeneratedEventArgs args)
        {
            string hypothesis = args.Hypothesis.Text;
            string textboxContent = dictatedTextBuilder.ToString() + " " + hypothesis + " ...";
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbSearch.Text = textboxContent;
            });
        }

        private async void ContinuousRecognitionSession_Completed(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionCompletedEventArgs args)
        {
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
               {                  
                   this.imgMode.Source = new BitmapImage(new Uri("ms-appx:///Assets/mic.png"));

				   string recongnizeText = tbSearch.Text.ToString();
				   System.Diagnostics.Debug.WriteLine("Speech recognize as text: " + recongnizeText);

				   if (!String.IsNullOrEmpty(recongnizeText))
					CallService.CallService.callService(recongnizeText);
			   });

			}
        }

        private async void ContinuousRecognitionSession_ResultGenerated(SpeechContinuousRecognitionSession sender, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            if (args.Result.Confidence == SpeechRecognitionConfidence.Medium ||
                 args.Result.Confidence == SpeechRecognitionConfidence.High)
            {
                dictatedTextBuilder.Append(args.Result.Text + " ");
            }
            await dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                tbSearch.Text = dictatedTextBuilder.ToString();
				
			});
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri((sender as HyperlinkButton).Tag.ToString()));
        }

        /// <summary>
        /// Start or cancel recognition
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            if (speechRecognizer.State == SpeechRecognizerState.Idle)
            {
                await speechRecognizer.ContinuousRecognitionSession.StartAsync();
                tbSearch.Text = string.Empty;
                dictatedTextBuilder.Clear();
                this.imgMode.Source = new BitmapImage(new Uri("ms-appx:///Assets/micDisabled.png"));
            }
            else
            {
                await speechRecognizer.ContinuousRecognitionSession.CancelAsync();
            }
        }

        /// <summary>
        /// filter data by text in the textbox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tbSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string searchText = this.tbSearch.Text.Trim('.',' ');
            {
                if (searchText.Length > 0)
                {
                    List<string> result = SampleData.Where(o => searchText.ToLower().Contains(o.ToLower())).ToList();
                    ViewModel.SourceData = result;
                }
                else { ViewModel.SourceData = SampleData; }
            }
        }

        /// <summary>
        /// sample data
        /// </summary>
        /// <returns></returns>
        private List<string> GetSampleData()
        {
            List<string> list = new List<string>();
            list.Add("Count To");
            list.Add("Set timer to");
            return list;
        }

    }

    /// <summary>
    /// View model
    /// </summary>
    public class MainPageViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private List<string> _sourceData;
        public List<string> SourceData
        {
            get { return _sourceData; }
            set { _sourceData = value;OnPropertyChanged(nameof(SourceData)); }
        }
        public MainPageViewModel()
        {
            SourceData = new List<string>();
        }
    }
}
