using System;
using System.Windows.Forms;

namespace DigitalVibrance.Core
{
    public class VibranceChangedEventArgs : EventArgs
    {
        public int PreviousValue { get; private set; }
        public int NewValue { get; private set; }

        public VibranceChangedEventArgs(int prev, int next)
        {
            PreviousValue = prev;
            NewValue = next;
        }
    }

    static class VibranceController
    {
        public static event EventHandler<VibranceChangedEventArgs> ValueChanged;
        public static event EventHandler AnimationStarted;
        public static event EventHandler AnimationCompleted;

        static Timer _animTimer;
        static int _animCurrent;
        static int _animTarget;
        static int _animDirection;

        public static int CurrentValue
        {
            get { return Native.NvApi.GetCurrentVibrance(); }
        }

        public static bool IsAnimating
        {
            get { return _animTimer != null && _animTimer.Enabled; }
        }

        public static bool Initialize()
        {
            return Native.NvApi.Initialize();
        }

        public static bool SetVibrance(int level)
        {
            int speed = SettingsManager.Current.TransitionSpeed;

            if (speed > 0 && !IsAnimating)
            {
                StartAnimation(level, speed);
                return true;
            }

            return ApplyVibrance(level);
        }

        static bool ApplyVibrance(int level)
        {
            int clamped = Math.Max(0, Math.Min(100, level));
            int previous = CurrentValue;

            bool result = Native.NvApi.SetVibrance(clamped);
            if (result)
            {
                SettingsManager.Current.LastVibrance = clamped;
                SettingsManager.Save();
                if (ValueChanged != null)
                    ValueChanged(null, new VibranceChangedEventArgs(previous, clamped));
            }
            return result;
        }

        static void StartAnimation(int target, int speed)
        {
            int clampedTarget = Math.Max(0, Math.Min(100, target));
            int current = Native.NvApi.GetCurrentVibrance();

            if (current == clampedTarget) return;

            _animCurrent = current;
            _animTarget = clampedTarget;
            _animDirection = (clampedTarget > current) ? 1 : -1;

            int step = Math.Max(1, Math.Abs(clampedTarget - current) / 10);
            int interval = Math.Max(10, speed);

            if (AnimationStarted != null)
                AnimationStarted(null, EventArgs.Empty);

            _animTimer = new Timer();
            _animTimer.Interval = interval;
            _animTimer.Tick += OnAnimationTick;
            _animTimer.Start();
        }

        static void OnAnimationTick(object sender, EventArgs e)
        {
            int step = Math.Max(1, Math.Abs(_animTarget - _animCurrent) / 5);
            if (step < 1) step = 1;

            _animCurrent += _animDirection * step;

            if ((_animDirection > 0 && _animCurrent >= _animTarget) ||
                (_animDirection < 0 && _animCurrent <= _animTarget))
            {
                _animCurrent = _animTarget;
                _animTimer.Stop();
                _animTimer.Dispose();
                _animTimer = null;

                ApplyVibrance(_animTarget);

                if (AnimationCompleted != null)
                    AnimationCompleted(null, EventArgs.Empty);
            }
            else
            {
                Native.NvApi.SetVibrance(_animCurrent);
                if (ValueChanged != null)
                    ValueChanged(null, new VibranceChangedEventArgs(_animCurrent - _animDirection * step, _animCurrent));
            }
        }

        public static string GetDisplayInfo()
        {
            var displays = Native.NvApi.Displays;
            if (displays == null || displays.Count == 0)
                return "No NVIDIA displays detected";

            string result = "";
            for (int i = 0; i < displays.Count; i++)
            {
                var d = displays[i];
                result += string.Format("Monitor #{0}: {1}% (range {2}-{3})",
                    i + 1, d.Current, d.Min, d.Max);
                if (i < displays.Count - 1) result += "\n";
            }
            return result;
        }

        public static void Shutdown()
        {
            if (_animTimer != null)
            {
                _animTimer.Stop();
                _animTimer.Dispose();
                _animTimer = null;
            }
            Native.NvApi.Shutdown();
        }

        public static bool IsAvailable
        {
            get { return Native.NvApi.IsAvailable; }
        }
    }
}