using Nmea;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace NmeaViewer.Services;

public class PlaybackService : INotifyPropertyChanged
{
    private IDispatcherTimer? _timer;
    private IReadOnlyList<Epoch> _epochs = [];
    private int _currentIndex;
    private double _playbackSpeed = 1;
    private bool _isPlaying;
    private bool _skipNoPosition;

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action? EpochChanged;

    public IReadOnlyList<Epoch> Epochs => _epochs;
    public int EpochCount => _epochs.Count;

    public int CurrentIndex
    {
        get => _currentIndex;
        set
        {
            if (_epochs.Count == 0)
            {
                return;
            }

            int clamped = Math.Clamp(value, 0, _epochs.Count - 1);
            if (_currentIndex == clamped)
            {
                return;
            }

            _currentIndex = clamped;
            NotifyEpochChanged();
        }
    }

    public Epoch? CurrentEpoch => _epochs.Count > 0 ? _epochs[_currentIndex] : null;

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            if (_isPlaying == value)
            {
                return;
            }

            _isPlaying = value;
            OnPropertyChanged();
        }
    }

    public bool SkipNoPosition
    {
        get => _skipNoPosition;
        set
        {
            if (_skipNoPosition == value)
            {
                return;
            }

            _skipNoPosition = value;
            OnPropertyChanged();
        }
    }

    public double PlaybackSpeed
    {
        get => _playbackSpeed;
        set
        {
            if (Math.Abs(_playbackSpeed - value) < 0.001)
            {
                return;
            }

            _playbackSpeed = value;
            UpdateTimerInterval();
            OnPropertyChanged();
        }
    }

    public SpeedUnit SpeedUnit { get; set; } = SpeedUnit.MilesPerHour;

    public void Initialize(IDispatcher dispatcher)
    {
        if (_timer is not null)
        {
            return;
        }

        _timer = dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += OnTimerTick;
    }

    public void LoadEpochs(IReadOnlyList<Epoch> epochs)
    {
        Stop();
        _epochs = epochs;
        _currentIndex = 0;
        OnPropertyChanged(nameof(Epochs));
        OnPropertyChanged(nameof(EpochCount));
        NotifyEpochChanged();
    }

    public void TogglePlayPause()
    {
        if (_epochs.Count == 0 || _timer is null)
        {
            return;
        }

        if (IsPlaying)
        {
            Stop();
        }
        else
        {
            IsPlaying = true;
            UpdateTimerInterval();
            _timer.Start();
        }
    }

    public void Stop()
    {
        _timer?.Stop();
        IsPlaying = false;
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        if (_currentIndex >= _epochs.Count - 1)
        {
            Stop();
            return;
        }

        int next = _currentIndex + 1;
        if (SkipNoPosition)
        {
            while (next < _epochs.Count - 1 && !_epochs[next].HasPosition)
            {
                next++;
            }
        }

        CurrentIndex = next;
    }

    private void UpdateTimerInterval()
    {
        if (_timer is not null)
        {
            _timer.Interval = TimeSpan.FromSeconds(1 / _playbackSpeed);
        }
    }

    private void NotifyEpochChanged()
    {
        OnPropertyChanged(nameof(CurrentIndex));
        OnPropertyChanged(nameof(CurrentEpoch));
        EpochChanged?.Invoke();
    }

    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
