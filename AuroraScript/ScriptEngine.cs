﻿using AuroraScript.Ast;
using AuroraScript.Compiler;

namespace AuroraScript
{
    public class ScriptEngine
    {
        /// <summary>
        /// </summary>
        /// <param name="filename"></param>
        public void build(String filename)
        {
            var compiler = new ScriptCompiler();
            ModuleDeclaration root = compiler.buildAst(filename);
            compiler.PrintGenerateCode(root);
        }
    }
}