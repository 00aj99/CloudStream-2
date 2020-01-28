﻿using CloudStreamForms.Models;
using Rg.Plugins.Popup.Services;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using static CloudStreamForms.App;
using static CloudStreamForms.CloudStreamCore;
using static CloudStreamForms.MainChrome;

namespace CloudStreamForms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChromeCastPage : ContentPage
    {
        public EpisodeResult episodeResult;
        public Movie chromeMovieResult;

        public string TitleName { set { NameLabel.Text = value; } }
        public string DescriptName { set { EpsodeName.Text = value; } }
        public string EpisodeTitleName { set { EpTitleLabel.Text = value; } }
        public string EpisodePosterUrl { set {/* EpisodePoster.Source = value; */} }
        public string EpisodeDescription { set { EpTitleDescript.Text = value; /* EpisodePoster.Source = value; */} }

        public string PosterUrl { set { Poster.Source = value; } }
        public int IconSize { set; get; } = 48;
        public int BigIconSize { set; get; } = 60;
        public int FastForwardTime
        {
            get { return Settings.LoadingChromeSec; }
        }
        public int BackForwardTime
        {
            get { return Settings.LoadingChromeSec; }
        }
        public float ScaleAll { set; get; } = 1.4f;
        public float ScaleAllBig { set; get; } = 2f;

        public static int currentSelected = 0;

        async void SelectMirror()
        {
            bool succ = false;
            currentSelected--;
            while (!succ) {
                currentSelected++;

                if (currentSelected >= episodeResult.Mirros.Count) {
                    succ = true;
                }
                else {
                    try {
                        DescriptName = episodeResult.Mirros[currentSelected];
                    }
                    catch (Exception) {

                    }

                    succ = await MainChrome.CastVideo(episodeResult.mirrosUrls[currentSelected], episodeResult.Mirros[currentSelected]);

                }
            }
            try {
                DescriptName = episodeResult.Mirros[currentSelected];
            }
            catch (Exception) {

            }

            // CastVideo(episodeResult.mirrosUrls[currentSelected], episodeResult.Mirros[currentSelected], CurrentTime);
        }

        void OnStop()
        {
            Navigation.PopModalAsync();
            isActive = false;
        }

        protected override bool OnBackButtonPressed()
        {
            isActive = false;
            return base.OnBackButtonPressed();
        }

        public static bool isActive = false;

        public ChromeCastPage()
        {
            isActive = true;
            episodeResult = MovieResult.chromeResult;
            chromeMovieResult = MovieResult.chromeMovieResult;

            InitializeComponent();
            BindingContext = this;
            TitleName = chromeMovieResult.title.name;
            EpisodeTitleName = episodeResult.Title;
            PosterUrl = chromeMovieResult.title.hdPosterUrl;
            EpisodePosterUrl = episodeResult.PosterUrl;
            EpisodeDescription = episodeResult.Description;

            try {
                DescriptName = episodeResult.Mirros[currentSelected];
            }
            catch (Exception) {

            }

            MainChrome.OnDisconnected += (o, e) => {
                OnStop();
            };

            MainChrome.OnPauseChanged += (o, e) => {
                SetPause(e);
            };

            //https://material.io/resources/icons/?style=baseline
            VideoSlider.DragStarted += (o, e) => {
                draging = true;
            };

            VideoSlider.DragCompleted += (o, e) => {
                MainChrome.SetChromeTime(VideoSlider.Value * CurrentCastingDuration);
                draging = false;
                UpdateTxt();
            };
            const bool rotateAllWay = true;
            const int rotate = 45;
            FastForward.Clicked += async (o, e) => {
                SeekMedia(FastForwardTime);
                FastForward.Rotation = 0;
                if (rotateAllWay) {
                    await FastForward.RotateTo(360, 200, Easing.SinOut);
                }
                else {
                    await FastForward.RotateTo(rotate, 50, Easing.SinOut);
                    await FastForward.RotateTo(0, 50, Easing.SinOut);
                }
            };

            BackForward.Clicked += async (o, e) => {
                SeekMedia(-BackForwardTime);
                BackForward.Rotation = 0;
                if (rotateAllWay) {
                    await BackForward.RotateTo(-360, 200, Easing.SinOut);
                }
                else {
                    await BackForward.RotateTo(-rotate, 50, Easing.SinOut);
                    await BackForward.RotateTo(0, 50, Easing.SinOut);
                }
            };

            StopAll.Clicked += (o, e) => {
                MainChrome.StopCast();
                OnStop();
            };

            SkipForward.Clicked += async (o, e) => {
                currentSelected++;
                if (currentSelected > episodeResult.Mirros.Count) { currentSelected = 0; }
                SelectMirror();
                await SkipForward.TranslateTo(6, 0, 50, Easing.SinOut);
                await SkipForward.TranslateTo(0, 0, 50, Easing.SinOut);
            };

            SkipBack.Clicked += async (o, e) => {
                currentSelected--;
                if (currentSelected < 0) { currentSelected = episodeResult.Mirros.Count - 1; }
                SelectMirror();
                await SkipBack.TranslateTo(-6, 0, 50, Easing.SinOut);
                await SkipBack.TranslateTo(0, 0, 50, Easing.SinOut);
            };

            PlayList.Clicked += async (o, e) => {
                //ListScale();
                string a = await DisplayActionSheet("Select Mirror", "Cancel", null, episodeResult.Mirros.ToArray());
                //ListScale();

                for (int i = 0; i < episodeResult.Mirros.Count; i++) {
                    if (a == episodeResult.Mirros[i]) {
                        currentSelected = i;
                        SelectMirror();
                        return;
                    }
                }
            };
            ConstUpdate();

            MainChrome.Volume = (MainChrome.Volume);

            /*
            LowVol.Source = GetImageSource("round_volume_down_white_48dp.png");
            MaxVol.Source = GetImageSource("round_volume_up_white_48dp.png");*/

            //   UserDialogs.Instance.TimePrompt(new TimePromptConfig() { CancelText = "Cancel", Title = "da", Use24HourClock = false, OkText = "OK", IsCancellable = true });

        }

        bool draging = false;
        public async void ConstUpdate()
        {
            while (true) {
                await Task.Delay(1000);
                UpdateTxt();
            }
        }

        public void UpdateTxt()
        {
            StartTxt.Text = ConvertTimeToString(CurrentTime);
            EndTxt.Text = ConvertTimeToString(CurrentCastingDuration - CurrentTime);
            if (CurrentCastingDuration - CurrentTime < -1) {
                OnStop();
            }
            if (!draging) {
                VideoSlider.Value = CurrentTime / CurrentCastingDuration;
            }
        }

        void SetPause(bool paused)
        {
            Pause.Source = paused ? GetImageSource("round_play_arrow_white_48dp.png") : GetImageSource("round_pause_white_48dp.png");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            PlayList.Source = GetImageSource("round_playlist_play_white_48dp.png");
            StopAll.Source = GetImageSource("round_stop_white_48dp.png");
            BackForward.Source = GetImageSource("round_replay_white_48dp.png");
            FastForward.Source = GetImageSource("round_replay_white_48dp_mirror.png");
            SkipBack.Source = GetImageSource("round_skip_previous_white_48dp.png");
            SkipForward.Source = GetImageSource("round_skip_next_white_48dp.png");
            Audio.Source = GetImageSource("round_volume_up_white_48dp.png");
            SetPause(IsPaused);
        }

        private void AudioClicked(object sender, EventArgs e)
        {
            PopupNavigation.Instance.PushAsync(new CloudStreamForms.MyPopupPage());

        }

        private void Pause_Clicked(object sender, EventArgs e)
        {
            SetPause(!IsPaused);
            PauseAndPlay(!IsPaused);
            PauseScale();
        }
        async void PauseScale()
        {
            Pause.Scale = 2.0;
            await Pause.ScaleTo(2.4, 50, Easing.SinOut);
            await Pause.ScaleTo(2, 50, Easing.SinOut);
        }
        async void ListScale()
        {
            PlayList.Scale = 1.4;
            await PlayList.ScaleTo(2, 50, Easing.SinOut);
            await PlayList.ScaleTo(1.4, 50, Easing.SinOut);
        }
    }
}