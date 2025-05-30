﻿using AuroraScript.Compiler;
using System;

namespace AuroraScript.Ast.Expressions
{
    public class ArrayLiteralExpression : OperatorExpression
    {
        internal ArrayLiteralExpression() : base(Operator.ArrayLiteral)
        {
        }

        public override void Accept(IAstVisitor visitor)
        {
            visitor.AcceptArrayExpression(this);
        }

        public override string ToString()
        {
            return $"[{String.Join(", ", ChildNodes)}]";
        }


    }
}