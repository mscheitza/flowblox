using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace FlowBlox.UICore.Views;

/// <summary>Plays the intro, runs the startup hook, then holds the final image for 500 ms.</summary>
public partial class SplashWindow : Window
{
    private static readonly TimeSpan KeepTime = TimeSpan.FromMilliseconds(500);
    private readonly Action _initialize;
    private GifBitmapDecoder? _animation;
    private readonly BitmapSource _finalImage;
    private readonly TaskCompletionSource<bool> _animationCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly DispatcherTimer _timer;
    private readonly Stopwatch _elapsed = new();
    private readonly int[] _frameEnds;
    private bool _started;
    private bool _finished;

    public Exception? InitializationError { get; private set; }

    public SplashWindow(Action initialize)
    {
        _initialize = initialize ?? throw new ArgumentNullException(nameof(initialize));
        InitializeComponent();
        var uri = new Uri("pack://application:,,,/FlowBlox.UICore;component/Resources/Images/FlowBloxStartup.gif");
        _animation = new GifBitmapDecoder(uri, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
        _frameEnds = new int[_animation.Frames.Count];
        int duration = 0;
        for (int i = 0; i < _animation.Frames.Count; i++)
        {
            var metadata = _animation.Frames[i].Metadata as BitmapMetadata;
            int delay = metadata?.GetQuery("/grctlext/Delay") is ushort value ? value * 10 : 50;
            duration += delay > 0 ? delay : 50;
            _frameEnds[i] = duration;
            _animation.Frames[i].Freeze();
        }
        // Copy the last frame so it can be displayed independently of the GIF decoder.
        _finalImage = new WriteableBitmap(_animation.Frames[^1]);
        _finalImage.Freeze();
        Logo.Source = _animation.Frames[0];
        _timer = new DispatcherTimer(TimeSpan.FromMilliseconds(16), DispatcherPriority.Render, AdvanceFrame, Dispatcher);
        _timer.Stop();
        ContentRendered += StartInitialization;
        Closed += (_, _) => { _timer.Stop(); _timer.Tick -= AdvanceFrame; };
    }

    private void AdvanceFrame(object? sender, EventArgs e)
    {
        if (_animation == null) return;
        int frame = Array.FindIndex(_frameEnds, end => _elapsed.ElapsedMilliseconds < end);
        if (frame >= 0)
        {
            Logo.Source = _animation.Frames[frame];
            return;
        }

        _timer.Stop();
        _elapsed.Stop();
        Logo.Source = _finalImage;
        _animation = null;
        _animationCompleted.TrySetResult(true);
    }

    private async void StartInitialization(object? sender, EventArgs e)
    {
        if (_started) return;
        _started = true;
        _elapsed.Start();
        _timer.Start();
        try
        {
            await _animationCompleted.Task;
            await Task.Run(_initialize);
            await Task.Delay(KeepTime);
        }
        catch (Exception ex)
        {
            InitializationError = ex;
        }
        finally
        {
            _finished = true;
            DialogResult = InitializationError == null;
        }
    }

    private void Window_Closing(object? sender, CancelEventArgs e) => e.Cancel = !_finished;
}
