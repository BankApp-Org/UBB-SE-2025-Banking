namespace BankAppDesktop.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Represents a command that can be bound to UI elements and executed with optional conditions.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="RelayCommand"/> class.
    /// </remarks>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">The predicate to determine if the command can execute. If null, the command can always execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when the execute parameter is null.</exception>
    public partial class RelayCommand(Action<object> execute, Predicate<object>? canExecute = null) : ICommand
    {
        private readonly Action<object> execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Predicate<object>? canExecute = canExecute;

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the command's ability to execute has changed.
        /// </summary>
        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter to evaluate for execution.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter) => this.canExecute?.Invoke(parameter ?? new object()) ?? true;

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to notify that the command's ability to execute has changed.
        /// </summary>
        public static void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        /// <summary>
        /// Invokes the <see cref="Execute"/> method on the command.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the execute action.</param>
        public void Execute(object? parameter) => this.execute(parameter ?? new object());
    }
}