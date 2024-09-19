﻿using AuroraScript.Ast.Statements;
using AuroraScript.Stream;


namespace AuroraScript.Ast
{
    public class ImportDeclaration: Statement
    {
        internal ImportDeclaration()
        {

        }

        public Token Module { get; set; }
        public Token File { get; set; }


        public override void GenerateCode(CodeWriter writer, Int32 depth = 0)
        {
            if (this.Module != null)
            {
                writer.WriteLine($"{Symbols.KW_IMPORT.Name} {this.Module.Value} {Symbols.KW_FROM.Name} {this.File.Value};");
            }
            else
            {
                writer.WriteLine($"{Symbols.KW_IMPORT.Name} {this.File.Value};");
            }
        }


    }
}
