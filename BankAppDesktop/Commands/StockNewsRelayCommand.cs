namespace BankAppDesktop.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Initializes a new instance of the <see cref="StockNewsRelayCommand"/> class.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">A function that determines whether the command can execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="execute"/> parameter is null.</exception>
    public partial class StockNewsRelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Action execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<bool>? canExecute = canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="StockNewsRelayCommand"/> class.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter passed to the command. This parameter is not used in this implementation.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter) => this.canExecute?.Invoke() ?? true;

        /// <summary>
        /// Invokes the <see cref="Execute"/> method on the command.
        /// </summary>
        /// <param name="parameter">The parameter passed to the command. This parameter is not used in this implementation.</param>
        public void Execute(object? parameter) => this.execute();

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the command's ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}