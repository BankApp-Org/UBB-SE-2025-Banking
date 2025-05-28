namespace BankAppDesktop.Commands
{
    using System;
    using System.Windows.Input;

    /// <summary>
    /// Initializes a new instance of the <see cref="Command"/> class.
    /// </summary>
    /// <param name="execute"> The action to execute when the command is invoked.</param>
    /// <param name="canExecute"> The function that determines whether the command can execute.</param>
    /// <exception cref="ArgumentNullException"> Thrown when <paramref name="execute"/> is null.</exception>
    public partial class Command(Action execute, Func<bool>? canExecute = null) : ICommand
    {
        private readonly Action execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<bool>? canExecute = canExecute;

        /// <summary>
        /// Occurs when the <see cref="CanExecute"/> status changes.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">argument passed to the command.</param>
        /// <returns>
        /// if <see cref="canExecute"/> is null, it returns true; otherwise, it returns the result of <see cref="canExecute"/>.
        public bool CanExecute(object? parameter) => this.canExecute == null || this.canExecute();

        /// <summary>
        /// Invokes the <see cref="Execute"/> method on the command.
        /// </summary>
        /// <param name="parameter"> argument passed to the command.</param>
        public void Execute(object? parameter) => this.execute();

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the command's ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
