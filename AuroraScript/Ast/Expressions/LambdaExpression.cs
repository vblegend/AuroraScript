﻿using System;
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
    }
}
