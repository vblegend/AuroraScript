﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Exceptions
{
    internal class LexerException : Exception
    {

        public String FileName { get; private set; }
        public Int32 LineNumber { get; private set; }
        public Int32 ColumnNumber { get; private set; }


        public LexerException(String fileName, Int32 lineNumber, Int32 columnNumber, String message) : base(message)
        {
            this.ColumnNumber = columnNumber;
            this.FileName = fileName;
            this.LineNumber = lineNumber;
        }


        public LexerException(String fileName, Token token, String message) : base(message)
        {
            this.ColumnNumber = token.ColumnNumber;
            this.FileName = fileName;
            this.LineNumber = token.LineNumber;
        }

    }
}
