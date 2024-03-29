using System;
using System.Threading.Tasks;
using System.Timers;
using hub.Shared.Bases;
using hub.Shared.Models.Countdown;
using hub.Shared.Tools;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace hub.Client.ViewModels.Countdown
{
    public interface ICountdownViewModel : INotifyStateChanged, IDisposable
    {
        ElementReference Audio { get; set; }
        Task ToggleAudio();
        string Icon { get; }
        CountdownModel CurrentModel { get; }
        void StartTimer();
        void GoToNextDisplay();
        void GoToPreviousDisplay();
        void SelectChristmas();
        void SelectFinalFantasy();
    }

    public class CountdownViewModel : BaseNotifyStateChanged, ICountdownViewModel
    {
        private readonly IJSRuntime _jsRuntime;
        private bool _isPlaying;
        private Timer _timer;

        private CountdownModel _christmasCountdown;
        private CountdownModel _finalFantasyCountdown;

        public CountdownViewModel(IJSRuntime jsRuntime, INowTimeProvider nowTimeProvider)
        {
            _jsRuntime = jsRuntime;
            _isPlaying = false;

            var christmas = new DateTime(nowTimeProvider.Now.Year, 12, 25, 00, 0, 0);
            var thisYearsAfterChristmas = new DateTime(nowTimeProvider.Now.Year, 12, 26, 00, 0, 0);

            if (nowTimeProvider.Now > thisYearsAfterChristmas)
            {
                christmas = christmas.AddYears(1);
            }

            _christmasCountdown = new CountdownModel(
                "Christmas",
                christmas, 
                "assets/countdown/christmas-tree.svg", 
                "https://www.cutthatdesign.com/free-svg-christmas-tree-with-lights-design/",
                "assets/countdown/silent-night.mp4",
                "https://soundcloud.com/e-soundtrax/christmas-silent-night-royalty",
                "assets/countdown/background.jpg",
                "https://wallpaperaccess.com/simple-winter",
                nowTimeProvider
            );
            
            _finalFantasyCountdown = new CountdownModel(
                "Dawntrail Early Access",
                new DateTime(2024, 6, 28, 6, 0, 0, 0), 
                "assets/countdown/dawntrail.png", 
                "https://finalfantasy.fandom.com/wiki/Final_Fantasy_XIV:_Dawntrail",
                "assets/countdown/dawntrail.mp3",
                "https://www.youtube.com/watch?v=lvRAmjuFSeI",
                "assets/countdown/dawntrailbg.png",
                "https://wall.alphacoders.com/big.php?i=1342074",
                nowTimeProvider
            );
            CurrentModel = _finalFantasyCountdown;
        }

        public void StartTimer()
        {
            _timer = new Timer(TimerDelay);
            _timer.Elapsed += (_, _) => NotifyStateChanged();
            _timer.AutoReset = true;
            _timer.Enabled = true;
        }

        private void ChangeDisplay(int diff)
        {
            _christmasCountdown.CycleDisplayType(diff);
            _finalFantasyCountdown.CycleDisplayType(diff);
            _timer.Stop();
            _timer.Interval = TimerDelay;
            _timer.Start();
            
            NotifyStateChanged();
        }

        public void GoToNextDisplay()
        {
           ChangeDisplay(1); 
        }

        public void GoToPreviousDisplay()
        {
            ChangeDisplay(-1);
        }

        private async Task SelectCountdown(CountdownModel model)
        {
            if (CurrentModel == model) return;
            CurrentModel = model;

            NotifyStateChanged();
            
            if (_isPlaying)
            {
                await _jsRuntime.InvokeVoidAsync("pauseAudio", Audio);
            }

            await _jsRuntime.InvokeVoidAsync("loadAudio", Audio);

            if (_isPlaying)
            {
                await _jsRuntime.InvokeVoidAsync("playAudio", Audio);
            }

        }
        
        public void SelectChristmas()
        {
            SelectCountdown(_christmasCountdown).ConfigureAwait(false);
        }

        public void SelectFinalFantasy()
        {
            SelectCountdown(_finalFantasyCountdown).ConfigureAwait(false);
        }

        public CountdownModel CurrentModel { get; private set; }
        public ElementReference Audio { get; set; }

        public async Task ToggleAudio()
        {
            if (_isPlaying)
            {
                await _jsRuntime.InvokeVoidAsync("pauseAudio", Audio);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("playAudio", Audio);
            }

            _isPlaying = !_isPlaying;
            NotifyStateChanged();
        }

        public string Icon => "oi " + (_isPlaying ? "oi-media-pause" : "oi-media-play");

        public void Dispose()
        {
            _timer.Dispose();
        }

        private int TimerDelay
        {
            get
            {
                return CurrentModel.DisplayType switch
                {
                    DisplayType.FortNight => 1000 * 60 * 60,
                    DisplayType.Weeks => 1000 * 60 * 60,
                    DisplayType.Days => 1000 * 60 * 60,
                    DisplayType.Hours => 1000 * 60,
                    DisplayType.Minutes => 1000 * 5,
                    DisplayType.Seconds => 1000,
                    DisplayType.Milliseconds => 100,
                    DisplayType.Nanoseconds => 100,
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }
}