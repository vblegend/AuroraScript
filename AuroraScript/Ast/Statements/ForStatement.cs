﻿using AuroraScript.Ast.Expressions;

namespace AuroraScript.Ast.Statements
{
    public class ForStatement : Statement
    {
        internal ForStatement()
        {

        }
        public Expression Condition { get; set; }

        public AstNode Body { get; set; }


        /// <summary>
        /// for initializer
        /// may be assignment
        /// may be variable declaration
        /// </summary>
        public AstNode Initializer { get; set; }


        /// <summary>
        /// for incrementor
        /// contains multiple sentences 
        /// </summary>
        public AstNode Incrementor { get; set; }




    }
}
