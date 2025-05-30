﻿using AuroraScript.Ast;
using AuroraScript.Ast.Expressions;
using AuroraScript.Compiler.Emits;
using AuroraScript.Compiler.Exceptions;
using System;
using System.Collections.Generic;

namespace AuroraScript.Core
{
    public enum DeclareType
    {
        /// <summary>
        /// 模块的属性或全局变量属性
        /// </summary>
        Property,

        /// <summary>
        /// 代码段变量
        /// </summary>
        Variable,

        /// <summary>
        /// 闭包捕获的变量
        /// </summary>
        Captured,
    }


    public class ResolveValue
    {
        public DeclareType Type;
        public int Index;
    }


    public class DeclareObject
    {
        public DeclareObject(CodeScope scope, String name, String alias, DeclareType type, Int32 index)
        {
            Name = name;
            Alias = alias;
            Type = type;
            Index = index;
            Scope = scope;
        }

        public readonly String Name;
        public readonly String Alias;
        public readonly CodeScope Scope;
        public readonly DeclareType Type;
        public readonly int Index;
    }


    public enum DomainType
    {
        Global,
        Module,
        Function,
        Code
    }


    public class CodeScope
    {

        public CodeScope _parent { get; private set; }

        public int ScopeDepth { get; private set; } = 0;



        private int _variableBaseCount = 0;


        public readonly List<DeclareObject> _variables = new List<DeclareObject>();

        public readonly StringList _stringSet;
        public DomainType Domain { get; private set; }


        public CodeScope(CodeScope current, DomainType domain, StringList stringSet)
        {
            _parent = current;
            _stringSet = stringSet;
            if (_parent != null)
            {
                ScopeDepth = _parent.ScopeDepth + 1;

                if (domain == DomainType.Code)
                {
                    _variableBaseCount = _parent._variableBaseCount;
                }
            }

            Domain = domain;
        }


        /// <summary>
        /// 进入新的范围
        /// </summary>
        /// <returns></returns>

        public CodeScope Enter(StringList stringSet, DomainType domain)
        {
            var scope = new CodeScope(this, domain, stringSet);
            return scope;
        }

        /// <summary>
        /// 离开范围
        /// </summary>
        /// <returns></returns>
        public CodeScope Leave()
        {
            return _parent;
        }



        private DeclareObject findByName(String name)
        {
            for (int i = 0; i < _variables.Count; i++)
            {
                if (_variables[i].Name == name) return _variables[i];
            }
            return null;
        }



        public int Declare(DeclareType type, FunctionDeclaration func)
        {
            var name = func.Name.Value;
            var alias = name;
            var dobjeect = findByName(name);

            if (dobjeect != null)
            {
                throw new AuroraCompilerException("", "域内变量名重复");
            }
            if (Resolve(name, out var _))
            {
                // 父scope下有重名变量或属性，增加别名
            }
            int slot = 0;
            if (type == DeclareType.Property)
            {
                if (Resolve(name, out var _))
                {
                    // 父scope下有重名变量或属性，增加别名
                    alias = name + "_" + func.Name.LineNumber + "_" + func.Name.ColumnNumber;
                }
                slot = _stringSet.GetSlot(alias);
            }
            else
            {
                slot = _variableBaseCount++;
            }



            //var slot = _stringSet.GetSlot(alias);
            var declare = new DeclareObject(this, name, alias, type, slot);
            _variables.Add(declare);
            return slot;
        }



        public int Declare(DeclareType type, ImportDeclaration func)
        {
            var name = func.Name.Value;
            var alias = name;
            var dobjeect = findByName(name);
            if (dobjeect != null)
            {
                throw new AuroraCompilerException("", "域内变量名重复");
            }
            if (Resolve(name, out var _))
            {
                // 父scope下有重名变量或属性，增加别名
                alias = name + "_" + func.Name.LineNumber + "_" + func.Name.ColumnNumber;
            }
            var slot = _stringSet.GetSlot(alias);
            var declare = new DeclareObject(this, name, alias, type, slot);
            _variables.Add(declare);
            return slot;
        }



        public int Declare(DeclareType type, VariableDeclaration variable)
        {
            var name = variable.Name.Value;
            var alias = name;
            int slot = 0;
            if (type == DeclareType.Property)
            {
                if (Resolve(name, out var _))
                {
                    // 父scope下有重名变量或属性，增加别名
                    alias = name + "_" + variable.Name.LineNumber + "_" + variable.Name.ColumnNumber;
                }
                slot = _stringSet.GetSlot(alias);
            }
            else
            {
                slot = _variableBaseCount++;
            }
            var declare = new DeclareObject(this, name, alias, type, slot);
            _variables.Add(declare);
            return slot;
        }




        public int Declare(DeclareType type, string name)
        {
            var val = findByName(name);
            if (val != null)
            {
                throw new AuroraCompilerException("", "域内变量名重复");
            }
            int slot = 0;
            var alias = name;
            if (type == DeclareType.Property)
            {

                if (Resolve(name, out var _))
                {
                    // 父scope下有重名变量或属性，增加别名
                    //alias = name + "_" + func.Name.LineNumber + "_" + func.Name.ColumnNumber;
                }
            }
            else
            {
                slot = _variableBaseCount++;
            }
            var declare = new DeclareObject(this, name, name, type, slot);
            _variables.Add(declare);
            return slot;
        }


        // public Boolean Resolve(string name, out DeclareObject value)
        // {
        //     value = findByName(name);
        //     if (value != null) return true;
        //     if (_parent != null) return _parent.Resolve(name, out value);
        //     return false;
        // }


        public bool Resolve(string name, out ResolveValue value)
        {
            value = null;
            var val = findByName(name);
            if (val != null)
            {
                value = new ResolveValue() { Type = val.Type, Index = val.Index };
                return true;
            }
            if (_parent != null)
            {
                return _parent.Resolve(name, out value);
            }
            return false;
        }


        private bool FindInParent(string name, int depth, out ResolveValue value)
        {
            value = null;
            var val = this.findByName(name);
            if (val != null)
            {
                var offset = val.Index - _variableBaseCount;
                value = new ResolveValue() { Type = val.Type, Index = offset - depth };
                return true;
            }

            if (this._parent != null)
            {
                return this._parent.FindInParent(name, depth + _variableBaseCount, out value);
            }

            return false;
        }



    }
}

