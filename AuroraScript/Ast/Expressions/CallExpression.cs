﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Ast.Expressions
{
    /// <summary>
    /// 函数调用
    /// </summary>
    internal class CallExpression:Expression
    {
        public List<AstNode> Arguments { get; set; }
        public MemberExpression Base { get; set; }
    }
}