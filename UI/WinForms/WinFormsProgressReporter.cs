using Bundlingway.Core.Interfaces;
using Bundlingway.Utilities.Extensions;
using System;
using System.Threading.Tasks;

namespace Bundlingway.UI.WinForms
{
    /// <summary>
    /// WinForms implementation of progress reporter.
    /// Bridges core progress reporting to WinForms UI elements.
    /// </summary>
    public class WinFormsProgressReporter : IProgressReporter
    {
        private readonly frmLanding? _mainForm;
        private long _total;
        private long _current;
        private bool _isActive;

        public WinFormsProgressReporter(frmLanding? mainForm = null)
        {
            _mainForm = mainForm;
        }

        public bool IsProgressActive => _isActive;

        public event EventHandler<ProgressEventArgs>? ProgressUpdated;        public async Task StartProgressAsync(long total, string? description = null)
        {
            if (total <= 0)
                throw new ArgumentOutOfRangeException(nameof(total), "Total must be greater than zero.");
            _total = total;
            _current = 0;
            _isActive = true;

            if (_mainForm != null && !_mainForm.IsDisposed)
            {
                try
                {
                    // Use BeginInvoke for non-blocking UI thread marshaling
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.BeginInvoke(new Action(() => 
                        {
                            try
                            {
                                _mainForm.StartProgress(total, description);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[PROGRESS] Error in StartProgress: {ex.Message}");
                            }
                        }));
                    }
                    else
                    {
                        _mainForm.StartProgress(total, description);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PROGRESS] Error invoking StartProgress: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[PROGRESS] Starting: {description ?? "Operation"} (0/{total})");
            }

            ProgressUpdated?.Invoke(this, new ProgressEventArgs
            {
                Current = _current,
                Total = _total,
                Description = description,
                IsCompleted = false
            });
        }        public async Task UpdateProgressAsync(long current, string? description = null)
        {
            _current = current;

            if (_mainForm != null && !_mainForm.IsDisposed)
            {
                try
                {
                    // Use BeginInvoke for non-blocking UI thread marshaling
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.BeginInvoke(new Action(() => 
                        {
                            try
                            {
                                _mainForm.SetProgress(current);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[PROGRESS] Error in SetProgress: {ex.Message}");
                            }
                        }));
                    }
                    else
                    {
                        _mainForm.SetProgress(current);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PROGRESS] Error invoking SetProgress: {ex.Message}");
                }
            }
            else
            {
                var percentage = _total > 0 ? (double)current / _total * 100 : 0;
                Console.WriteLine($"[PROGRESS] {percentage:F1}% ({current}/{_total}): {description ?? ""}");
            }

            ProgressUpdated?.Invoke(this, new ProgressEventArgs
            {
                Current = _current,
                Total = _total,
                Description = description,
                IsCompleted = current >= _total
            });
        }

        public async Task UpdateProgressAsync(double percentage, string? description = null)
        {
            var current = _total > 0 ? (long)(percentage / 100.0 * _total) : 0;
            await UpdateProgressAsync(current, description);
        }        public async Task StopProgressAsync()
        {
            _isActive = false;

            if (_mainForm != null && !_mainForm.IsDisposed)
            {
                try
                {
                    // Use BeginInvoke for non-blocking UI thread marshaling
                    if (_mainForm.InvokeRequired)
                    {
                        _mainForm.BeginInvoke(new Action(() => 
                        {
                            try
                            {
                                _mainForm.StopProgress();
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"[PROGRESS] Error in StopProgress: {ex.Message}");
                            }
                        }));
                    }
                    else
                    {
                        _mainForm.StopProgress();
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[PROGRESS] Error invoking StopProgress: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine("[PROGRESS] Completed");
            }

            ProgressUpdated?.Invoke(this, new ProgressEventArgs
            {
                Current = _total,
                Total = _total,
                IsCompleted = true
            });
        }
    }
}
