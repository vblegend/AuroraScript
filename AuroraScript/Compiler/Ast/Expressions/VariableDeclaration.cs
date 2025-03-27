﻿using AuroraScript.Ast.Statements;
using AuroraScript.Compiler;


namespace AuroraScript.Ast.Expressions
{
    /// <summary>
    /// variable declaration
    /// </summary>
    public class VariableDeclaration : Statement
    {
        internal VariableDeclaration(Symbols access, Boolean isConst, Token nameToken)
        {
            Access = access;
            IsConst = isConst;
            Name = nameToken;
        }
        /// <summary>
        /// parameter Modifier  ....
        /// </summary>
        public Token Modifier { get; set; }

        /// <summary>
        /// variable names
        /// </summary>
        public Token Name { get; set; }

        /// <summary>
        /// var initialize statement
        /// </summary>
        public Expression Initializer => ChildNodes[0] as Expression;

        /// <summary>
        /// Function Access
        /// </summary>
        public Symbols Access { get; set; }

        /// <summary>
        /// this variable use const declare
        /// </summary>
        public bool IsConst { get; set; }

        public override void Accept(IAstVisitor visitor)
        {
            visitor.VisitVarDeclaration(this);
        }

        public override string ToString()
        {
            return $"var {Name.Value} = {Initializer}";
        }


    }
}