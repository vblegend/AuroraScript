﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Ast.Expressions
{
    internal class ArrayExpression : Expression
    {
        public List<Expression> Elements { get; set; }

    }
}