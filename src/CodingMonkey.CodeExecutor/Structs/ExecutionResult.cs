﻿namespace CodingMonkey.CodeExecutor.Structs
{
    using System;

    public struct ExecutionResult
    {
        public bool Successful;
        public object Value;
        public Exception Error;
    }
}
