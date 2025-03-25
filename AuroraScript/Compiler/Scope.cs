﻿using AuroraScript.Analyzer;
using AuroraScript.Ast;
using AuroraScript.Ast.Expressions;
using AuroraScript.Compiler.Exceptions;
using System.Text.Json.Serialization;

namespace AuroraScript.Compiler
{
    public enum ScopeType
    {
        UNKNOWN,
        MODULE,
        FOR,
        BLOCK,
        GROUP,
        FUNCTION,
        CONSTRUCTOR
    }

    /// <summary>
    /// statemtnt scope
    /// </summary>
    public class Scope
    {
        [JsonIgnore]
        public Scope Parent { get; private set; }

        internal AuroraParser Parser { get; private set; }
        public List<Scope> Childrens { get; private set; } = new List<Scope>();
        public Dictionary<string, VariableDeclaration> Variables { get; private set; } = new Dictionary<string, VariableDeclaration>();

        public ScopeType ScopeType { get; private set; }

        internal Scope(AuroraParser parser)
        {
            Parser = parser;
            ScopeType = ScopeType.MODULE;
        }

        public void FindToken(Token token)
        {
        }

        public Scope CreateScope(ScopeType scopeType)
        {
            var scope = new Scope(Parser);
            scope.Parent = this;
            scope.ScopeType = scopeType;
            Childrens.Add(scope);
            return scope;
        }

        internal void DeclareVariable(VariableDeclaration parameter)
        {
            var declarationScope = this;
            while (declarationScope != null)
            {
                if (declarationScope.Variables.ContainsKey(parameter.Variables[0].Value))
                {
                    throw new ParseException(Parser.lexer.FullPath, parameter.Variables[0], "Duplicate variable declaration in ");
                }
                declarationScope = declarationScope.Parent;
            }
            Variables.Add(parameter.Variables[0].Value, parameter);
        }

        internal void DefineVariable(VariableDeclaration parameter)
        {
        }

        internal void DeclareFunction(FunctionDeclaration parameter)
        {
        }

        internal void DefineFunction(FunctionDeclaration parameter)
        {
        }
    }
}