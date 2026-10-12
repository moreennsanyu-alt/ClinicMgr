using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace ClinicManager.Win.Features.Infrastructure.Authentication
{
    public class ChangePasswordViewModel : INotifyPropertyChanged
    {
        private string _currentPassword = string.Empty;
        private string _newPassword = string.Empty;
        private string _confirmPassword = string.Empty;
        private string _errorMessage = string.Empty;
        private bool _isBusy;

        public string CurrentPassword
        {
            get => _currentPassword;
            set => SetField(ref _currentPassword, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetField(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetField(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetField(ref _errorMessage, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                if (SetField(ref _isBusy, value))
                {
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ICommand SubmitCommand { get; }

        /// <summary>
        /// Raised when the password change completes successfully.
        /// </summary>
        public event EventHandler? PasswordChanged;

        public ChangePasswordViewModel()
        {
            SubmitCommand = new RelayCommand(_ => ExecuteSubmit(), _ => CanSubmit());
        }

        private bool CanSubmit()
        {
            return !IsBusy
                   && !string.IsNullOrWhiteSpace(CurrentPassword)
                   && !string.IsNullOrWhiteSpace(NewPassword)
                   && NewPassword == ConfirmPassword;
        }

        private void ExecuteSubmit()
        {
            IsBusy = true;
            ErrorMessage = string.Empty;

            try
            {
                // TODO: replace with a real password-change service call
                PasswordChanged?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Minimal ICommand implementation local to this view model.
        /// </summary>
        private sealed class RelayCommand : ICommand
        {
            private readonly Action<object?> _execute;
            private readonly Predicate<object?>? _canExecute;

            public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public event EventHandler? CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

            public void Execute(object? parameter) => _execute(parameter);
        }
    }
}
