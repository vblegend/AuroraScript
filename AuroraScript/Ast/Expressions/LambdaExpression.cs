﻿using AuroraScript.Ast.Statements;
using AuroraScript.Stream;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Ast.Expressions
{
    internal class LambdaExpression : BinaryExpression
    {
        internal LambdaExpression(Operator @operator) : base(@operator)
        {
        }


        public Expression Declare;


        public Statement Block { get; set; }




        public override void GenerateCode(CodeWriter writer, Int32 depth = 0)
        {
            var isPriority = false;
            if (this.Parent is BinaryExpression parent)
            {
                isPriority = parent.Operator.Precedence > this.Operator.Precedence;
            }

            if (isPriority) writer.Write(Symbols.PT_LEFTPARENTHESIS.Name);

            this.Declare.GenerateCode(writer);

            writer.Write($" {this.Operator.Symbol.Name} ");
            if (this.Block != null)
            {
                // lambda
                this.Block.GenerateCode(writer);
            }
            else
            {
                // delegate
                this.Right.GenerateCode(writer);
            }
            if (isPriority) writer.Write(Symbols.PT_RIGHTPARENTHESIS.Name);
        }



    }
}
