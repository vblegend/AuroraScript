﻿using AuroraScript.Ast.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Ast.Statements
{
    internal class ReturnStatement : Statement
    {
        public ReturnStatement(Expression expression)
        {
            this.expression = expression;
        }

        public Expression expression;

    }
}
