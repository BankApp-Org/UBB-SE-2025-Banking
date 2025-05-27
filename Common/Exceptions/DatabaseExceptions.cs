namespace Common.Exceptions
{
    using System;

    public class DatabaseInitializationException : Exception
    {
        public DatabaseInitializationException(string message)
            : base(message)
        {
        }

        public DatabaseInitializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }

    public class SqlScriptMissingException : Exception
    {
        public string ScriptPath { get; }

        public SqlScriptMissingException(string path)
            : base($"SQL script file not found: {path}")
        {
            this.ScriptPath = path;
        }

        public SqlScriptMissingException(string path, Exception innerException)
            : base($"SQL script file not found: {path}", innerException)
        {
            this.ScriptPath = path;
        }
    }
}
