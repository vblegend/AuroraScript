﻿using AuroraScript.Tokens;

namespace AuroraScript.Compiler
{
    public abstract class Token
    {
        public static Token EOF = new EndOfFileToken(); // end of file

        /// <summary>
        /// get token symbol object
        /// </summary>
        public Symbols Symbol
        {
            get;
            internal set;
        }

        /// <summary>
        /// get token string value
        /// </summary>
        public string Value
        {
            get;
            internal set;
        }


        public string UniqueValue => $"{Value}_{LineNumber}_{ColumnNumber}";



        /// <summary>
        /// get token line number
        /// </summary>
        internal int LineNumber
        {
            get;
            set;
        }

        /// <summary>
        /// get token column number
        /// </summary>
        internal int ColumnNumber
        {
            get;
            set;
        }

        public override string ToString()
        {
            return $"LineNumber:{LineNumber.ToString().PadLeft(4, '0')} ColumnNumber:{ColumnNumber.ToString().PadLeft(3, '0')} {GetType().Name.PadRight(15, ' ')} {Value}";
        }
    }
}