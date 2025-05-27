namespace StockApp.Commands
{
    using System;
    using System.Windows.Input;
    using ABI.System;

    /// <summary>
    /// Initializes a new instance of the <see cref="RelayCommandGeneric{T}"/> class.
    /// </summary>
    /// <param name="execute">The action to execute when the command is invoked.</param>
    /// <param name="canExecute">A function that determines whether the command can execute.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="execute"/> is null.</exception>
    public partial class RelayCommandGeneric<T>(Action<T> execute, Func<T, bool>? canExecute = null) : ICommand
    {
        private readonly Action<T> execute = execute ?? throw new ArgumentNullException(nameof(execute));
        private readonly Func<T, bool>? canExecute = canExecute;

        /// <summary>
        /// Initializes a new instance of the <see cref="RelayCommandGeneric{T}"/> class.
        /// </summary>
        public event EventHandler? CanExecuteChanged;

        /// <summary>
        /// Determines whether the command can execute in its current state.
        /// </summary>
        /// <param name="parameter">The parameter to evaluate for execution.</param>
        /// <returns>True if the command can execute; otherwise, false.</returns>
        public bool CanExecute(object? parameter)
        {
            return (parameter != null || !typeof(T).IsValueType) && (this.canExecute == null || this.canExecute((T)(parameter ?? throw new ArgumentNullException($"Tried to cast {nameof(parameter)} to {nameof(T)}"))));
        }

        /// <summary>
        /// Invokes the <see cref="Execute"/> method on the command.
        /// </summary>
        /// <param name="parameter">The parameter to pass to the execute action.</param>
        public void Execute(object? parameter) => this.execute((T)(parameter ?? throw new ArgumentNullException($"Cannot cast null to {nameof(T)}")));

        /// <summary>
        /// Raises the <see cref="CanExecuteChanged"/> event to indicate that the command's ability to execute has changed.
        /// </summary>
        public void RaiseCanExecuteChanged() => this.CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}