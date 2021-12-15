﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Ast.Expressions
{
    /// <summary>
    /// 前缀表达式
    /// </summary>
    internal class PrefixUnaryExpression : Expression
    {
        /// <summary>
        /// -5
        /// !name
        /// ++n
        /// --n
        /// </summary>
        public Token Operator { get; set; }

        public AstNode Operand { get; set; }

    }
}