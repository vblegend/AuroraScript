﻿

using AuroraScript.Stream;

namespace AuroraScript.Ast.Statements
{
    public class BlockStatement : Statement
    {
        /// <summary>
        /// statement scope
        /// </summary>
        public Scope Scope { get; private set; }
        internal BlockStatement(Scope currentScope)
        {
            this.Scope = currentScope;
        }

        public new virtual List<AstNode> ChildNodes
        {
            get
            {
                return childrens;
            }
        }


        public override void GenerateCode(CodeWriter writer, Int32 depth = 0)
        {
            writer.WriteLine(Symbols.PT_LEFTBRACE.Name);
            using (writer.IncIndented())
            {
                this.writeParameters(writer, ChildNodes, "");
            }
            writer.Write(Symbols.PT_RIGHTBRACE.Name);
        }




    }
}
