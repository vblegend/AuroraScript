﻿using AuroraScript.Ast;
using AuroraScript.Ast.Expressions;
using AuroraScript.Ast.Statements;
using AuroraScript.Exceptions;
using AuroraScript.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuroraScript.Analyzer
{
    internal class AuroraParser
    {
        public AuroraLexer lexer { get; private set; }
        public AuroraParser(AuroraLexer lexer)
        {
            this.lexer = lexer;
        }

        public AstNode Parse()
        {
            var scope = new Scope(this, null);
            Statement result = new BlockStatement(scope);
            while (true)
            {
                var token = this.lexer.LookAtHead();
                if (token.Symbol == Symbols.KW_EOF)
                {
                    this.lexer.Next();
                    break;
                }
                var node = ParseStatement(scope);
                result.AddNode(node);
                if (node == null) break;
            }
            return result;
        }



        /// <summary>
        /// Parse a statement ending in “;” or a statement block wrapped by “{}”
        /// </summary>
        /// <param name="currentScope"></param>
        /// <param name="endSymbols"></param>
        /// <returns></returns>
        /// <exception cref="ParseException"></exception>
        private Statement ParseStatement(Scope currentScope)
        {
            var token = this.lexer.LookAtHead();
            if (token == null) throw new ParseException(this.lexer.FullPath, token, "Invalid keywords appear in ");
            //if (token.Symbol == Symbols.KW_EOF) throw new ParseException(this.lexer.FullPath, token, "Unclosed scope ");
            if (token.Symbol == Symbols.PT_LEFTBRACE) return this.ParseBlock(currentScope);
            if (token.Symbol == Symbols.KW_IMPORT) return this.ParseImport();
            if (token.Symbol == Symbols.KW_EXPORT) return this.ParseExportStatement(currentScope);
            if (token.Symbol == Symbols.KW_FUNCTION) return this.ParseFunctionDeclaration(currentScope, Symbols.KW_SEALED);
            if (token.Symbol == Symbols.KW_VAR) return this.ParseVariableDeclaration(currentScope);
            if (token.Symbol == Symbols.KW_FOR) return this.ParseForBlock(currentScope);
            if (token.Symbol == Symbols.KW_IF) return this.ParseIfBlock(currentScope);
            if (token.Symbol == Symbols.KW_CONTINUE) return this.ParseContinueStatement(currentScope);
            if (token.Symbol == Symbols.KW_BREAK) return this.ParseBreakStatement(currentScope);
            if (token.Symbol == Symbols.KW_RETURN) return this.ParseReturnStatement(currentScope);
            //this.lexer.RollBack();
            var statement = new Statement();
            statement.AddNode(this.ParseExpression(currentScope));
            return statement;
        }

        /// <summary>
        /// parse return expression
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseReturnStatement(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_RETURN);
            var exp = this.ParseExpression(currentScope);
            return new ReturnStatement(exp);
        }


        /// <summary>
        /// parse continue expression
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseContinueStatement(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_CONTINUE);
            this.lexer.NextOfKind(Symbols.PT_SEMICOLON);
            var statement = new ContinueStatement();

            return statement;
        }

        /// <summary>
        /// parse break expression
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseBreakStatement(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_BREAK);
            this.lexer.NextOfKind(Symbols.PT_SEMICOLON);
            var statement = new BreakStatement();
            return statement;
        }

        /// <summary>
        /// Parse a block surrounded by {  }
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseBlock(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.PT_LEFTBRACE);
            var scope = new Scope(this, currentScope);
            var result = new BlockStatement(scope);
            while (true)
            {
                var token = this.lexer.LookAtHead();
                // Check for the end brace (}).
                if (token.Symbol == Symbols.PT_RIGHTBRACE) break;
                // Parse a single statement.
                result.AddNode(this.ParseStatement(scope));
            }
            // Consume the end brace.
            this.lexer.NextOfKind(Symbols.PT_RIGHTBRACE);
            return result;
        }




        /// <summary>
        /// parse for block
        /// starting with “for”
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseForBlock(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_FOR);
            this.lexer.NextOfKind(Symbols.PT_LEFTPARENTHESIS);
            // parse for initializer
            var initializer = this.ParseStatement(currentScope);
            // parse for condition
            var condition = this.ParseExpression(currentScope);
            // parse for incrementor
            var incrementor = this.ParseExpression(currentScope, Symbols.PT_RIGHTPARENTHESIS);
            // Determine whether the body is single-line or multi-line 
            AstNode body = this.ParseStatement(currentScope);
            // parse for body
            return new ForStatement()
            {
                Condition = condition,
                Incrementor = incrementor,
                Initializer = initializer,
                Body = body
            };

        }







        /// <summary>
        /// parse variable declaration
        /// starting with “var”
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseVariableDeclaration(Scope currentScope)
        {
            Expression initializer = null;
            this.lexer.NextOfKind(Symbols.KW_VAR);
            var varName = this.lexer.NextOfKind<IdentifierToken>();
            List<Token> varNames = new List<Token>() { varName };

            while (true)
            {
                var nextToken = this.lexer.Next();
                // ;
                if (nextToken.Symbol == Symbols.PT_SEMICOLON)
                {
                    break;
                }
                // =
                else if (nextToken.Symbol == Symbols.OP_ASSIGNMENT)
                {
                    initializer = this.ParseExpression(currentScope, Symbols.PT_SEMICOLON);
                    break;
                }
                // ,
                else if (nextToken.Symbol == Symbols.PT_COMMA)
                {
                    varName = this.lexer.NextOfKind<IdentifierToken>();
                    varNames.Add(varName);
                }
                else
                {
                    throw this.InitParseException("Invalid keywords appear in var declaration.", nextToken);
                }

            }
            //this.lexer.NextOfKind(Symbols.OP_ASSIGNMENT);
            var expression = new VariableDeclaration();
            expression.Variables.AddRange(varNames);
            expression.Initializer = initializer;
            //this.lexer.NextOfKind(Symbols.KW_VAR);
            return expression;
        }


        private Exception InitParseException(String message, Token token)
        {
            Console.WriteLine($"{token} {message}");
            return new ParseException(this.lexer.FullPath, token, message);
        }





        /// <summary>
        /// parse if block
        /// starting with “if”
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseIfBlock(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_IF);
            this.lexer.NextOfKind(Symbols.PT_LEFTPARENTHESIS);
            var condition = this.ParseExpression(currentScope, Symbols.PT_RIGHTPARENTHESIS);
            // Determine whether the body is single-line or multi-line 
            // parse if body
            Statement body = this.ParseStatement(currentScope);
            var ifStatement = new IfStatement() { Condition = condition };
            ifStatement.Body.Add(body);
            var nextToken = this.lexer.LookAtHead();
            // 处理 else
            if (nextToken.Symbol == Symbols.KW_ELSE)
            {
                ifStatement.Else = this.ParseElseBlock(currentScope);
            }
            return ifStatement;
        }

        /// <summary>
        /// parse else block
        /// starting with “else”
        /// </summary>
        /// <param name="currentScope"></param>
        /// <returns></returns>
        private Statement ParseElseBlock(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_ELSE);
            var nextToken = this.lexer.LookAtHead();
            BlockStatement block = new BlockStatement(currentScope);
            if (nextToken.Symbol == Symbols.KW_IF)
            {
                this.lexer.Next();
                var statement = this.ParseIfBlock(currentScope);
                block.AddNode(statement);
            }
            else
            {
                var expression = this.ParseStatement(currentScope);
                block.AddNode(expression);
            }
            return block;
        }


        /// <summary>
        /// parse an expression 
        /// ending symbol in endSymbols or “;”; 
        /// </summary>
        /// <param name="currentScope"></param>
        /// <param name="endSymbols"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        private Expression ParseExpression(Scope currentScope, params Symbols[] endSymbols)
        {
            Expression rootExpression = new Expression();
            Expression lastExpression = null;
            Operator lastOperator = null;
            while (true)
            {
                var token = this.lexer.Next();
                // over statement
                if (token.Symbol == Symbols.PT_SEMICOLON)
                {
                    break;
                }
                // encountered the specified symbol, the analysis is complete, jump out 
                if (token.Symbol != null && endSymbols.Contains(token.Symbol))
                {
                    break;
                }
                // ===========================================================
                // ==== parse begin
                // ===========================================================
                Expression tmpexp = null;
                // value Operand
                if (token is ValueToken) tmpexp = new ValueExpression(token);
                // identifier Operand
                if (token is IdentifierToken) tmpexp = new NameExpression() { Identifier = token };
                // keywords should not appear here 
                if (token is KeywordToken) throw this.InitParseException("Keyword appears in the wrong place ", token);
                // punctuator
                if (token is PunctuatorToken)
                {
                    var previousToken = this.lexer.Previous();
                    //                   
                    var leftIsOperand = (lastOperator != null && lastOperator.IsOperand) || previousToken is IdentifierToken || previousToken is ValueToken; // 
                    var _operator = Operator.FromSymbols(token.Symbol, leftIsOperand);
                    if (_operator == null)
                    {
                        if (endSymbols.Contains(token.Symbol))
                        {
                            break;
                        }
                        //if (endSymbols.Contains(Symbols.PT_SEMICOLON))
                        //{
                        //    break;
                        //}
                    }
                    else
                    {
                        tmpexp = this.createExpression(_operator, previousToken);
                    }

                }
                if (tmpexp == null) throw this.InitParseException("Invalid token {token} appears in expression {pos}", token);
                // ==============================================
                // 
                // ==============================================
                // this is Operator
                if (tmpexp is OperatorExpression operatorExpression)
                {
                    // this operator has secondary sybmol
                    if (operatorExpression.Operator.SecondarySymbols != null)
                    {
                        // this operator is function call
                        if (tmpexp is CallExpression callExpression)
                        {
                            while (true)
                            {
                                // parse function call arguments
                                var argument = this.ParseExpression(currentScope, operatorExpression.Operator.SecondarySymbols, Symbols.PT_COMMA);
                                if (argument != null) callExpression.Arguments.Add(argument);
                                var last = this.lexer.Previous(1);
                                // If the symbol ends with a closing parenthesis, the parsing is complete 
                                if (last.Symbol == Symbols.PT_RIGHTPARENTHESIS) break;
                            }
                        }
                        // expressions wrapped in parentheses 
                        else if (tmpexp is GroupExpression groupExpression)
                        {
                            // Parse() block, from here recursively parse expressions to minor symbols 
                            tmpexp = this.ParseExpression(currentScope, operatorExpression.Operator.SecondarySymbols);

                            var group = new GroupExpression(Operator.Grouping);
                            group.AddNode(tmpexp);
                            tmpexp = group;
                        }
                    }
                    //else
                    //{
                    //    // this is Ordinary operation operator + - * / ....
                    //    tmpexp.AddNode(tmpexp);
                    //}
                }
                // token is not an operator, that is the operand 
                else if (lastOperator != null)
                {
                    // 添加操作数到操作符表达式中
                    lastExpression.AddNode(tmpexp);

                    // 前缀表达式操作符
                    if (lastOperator.placement == OperatorPlacement.Prefix)
                    {


                    }

                }
                // 不是操作符 且 上一个 Token也不是操作符
                else
                {
                    if (rootExpression.ChildNodes.Count() > 0)
                        throw this.InitParseException("Invalid token {token} appears in expression {pos}", token);
                }

                if (tmpexp is OperatorExpression)
                {
                    // 查找操作符优先级 更新树位置
                    if(lastExpression != null)
                    {
                        var node = lastExpression;
                        while (node.)
                        {

                        }






                        var parent = lastExpression.Parent;
                        lastExpression.Remove();
                        tmpexp.AddNode(lastExpression);
                        parent.AddNode(tmpexp);
                    }
                }
                lastOperator = (tmpexp is OperatorExpression f) ? f.Operator : null;
                lastExpression = tmpexp;
                if (rootExpression.ChildNodes.Count() == 0) rootExpression.AddNode(tmpexp);
            }


            return rootExpression.Length > 0 ? rootExpression.Pop() : null ;
        }




        private Expression createExpression(Operator _operator, Token previousToken)
        {
            // Assignment Expression
            if (_operator == Operator.Assignment) return new AssignmentExpression(_operator);
            if (_operator == Operator.CompoundAdd) return new AssignmentExpression(_operator);
            if (_operator == Operator.CompoundSubtract) return new AssignmentExpression(_operator);
            if (_operator == Operator.CompoundDivide) return new AssignmentExpression(_operator);
            if (_operator == Operator.CompoundModulo) return new AssignmentExpression(_operator);
            if (_operator == Operator.CompoundMultiply) return new AssignmentExpression(_operator);

            // new expression
            if (_operator == Operator.New) return new BinaryExpression(_operator);

            // function call expression
            if (_operator == Operator.FunctionCall && !(previousToken is PunctuatorToken)) return new CallExpression(_operator);
            // Grouping expression
            if (_operator == Operator.Grouping && previousToken is PunctuatorToken) return new GroupExpression(_operator);
            // member access expression
            if (_operator == Operator.Index) return new MemberAccessExpression(_operator);
            // member access expression
            if (_operator == Operator.MemberAccess) return new MemberAccessExpression(_operator);

            // binary expression
            if (_operator == Operator.Add) return new BinaryExpression(_operator);
            if (_operator == Operator.Divide) return new BinaryExpression(_operator);
            if (_operator == Operator.Equal) return new BinaryExpression(_operator);
            if (_operator == Operator.LeftShift) return new BinaryExpression(_operator);
            if (_operator == Operator.LessThan) return new BinaryExpression(_operator);
            if (_operator == Operator.LessThanOrEqual) return new BinaryExpression(_operator);
            if (_operator == Operator.GreaterThan) return new BinaryExpression(_operator);
            if (_operator == Operator.GreaterThanOrEqual) return new BinaryExpression(_operator);
            if (_operator == Operator.BitwiseAnd) return new BinaryExpression(_operator);
            if (_operator == Operator.BitwiseOr) return new BinaryExpression(_operator);
            if (_operator == Operator.BitwiseXor) return new BinaryExpression(_operator);
            if (_operator == Operator.LogicalAnd) return new BinaryExpression(_operator);
            if (_operator == Operator.LogicalNot) return new BinaryExpression(_operator);
            if (_operator == Operator.LogicalOr) return new BinaryExpression(_operator);
            if (_operator == Operator.Modulo) return new BinaryExpression(_operator);
            if (_operator == Operator.Multiply) return new BinaryExpression(_operator);
            if (_operator == Operator.NotEqual) return new BinaryExpression(_operator);
            if (_operator == Operator.SignedRightShift) return new BinaryExpression(_operator);
            if (_operator == Operator.Subtract) return new BinaryExpression(_operator);

            // Prefix expression
            if (_operator == Operator.BitwiseNot) return new PrefixUnaryExpression(_operator);
            if (_operator == Operator.Minus) return new PrefixUnaryExpression(_operator);
            if (_operator == Operator.PreDecrement) return new PrefixUnaryExpression(_operator);
            if (_operator == Operator.PreIncrement) return new PrefixUnaryExpression(_operator);

            // Postfix expression
            if (_operator == Operator.PostDecrement) return new PostfixExpression(_operator);
            if (_operator == Operator.PostIncrement) return new PostfixExpression(_operator);
            return null;
        }




        /// <summary>
        /// Parse Function Arguments
        /// starting with “arg1: number, argn: string)”
        /// </summary>
        /// <returns></returns>
        private List<ParameterDeclaration> ParseFunctionArguments(Scope currentScope)
        {
            var arguments = new List<ParameterDeclaration>();
            while (true)
            {
                // Parsing modifier 
                // .......
                // ==================
                var nextToken = this.lexer.LookAtHead();
                if (nextToken.Symbol == Symbols.PT_RIGHTPARENTHESIS)
                {
                    this.lexer.Next();
                    break;
                }
                var varname = this.lexer.NextOfKind<IdentifierToken>();
                this.lexer.NextOfKind(Symbols.PT_COLON);
                var typed = this.lexer.NextOfKind<TypedToken>();
                var nexttoken = this.lexer.LookAtHead();
                Expression defaultValue = null;
                if (nexttoken != null && nexttoken.Symbol == Symbols.OP_ASSIGNMENT)
                {
                    this.lexer.Next();
                    defaultValue = this.ParseExpression(currentScope, Symbols.PT_COMMA, Symbols.PT_RIGHTPARENTHESIS);
                }
                var declaration = new ParameterDeclaration()
                {
                    Variable = varname,
                    DefaultValue = defaultValue,
                    Typed = typed
                };
                arguments.Add(declaration);
                nexttoken = this.lexer.LookAtHead();
                if (nexttoken.Symbol == Symbols.KW_EOF)
                {
                    throw this.InitParseException("Parameter declaration is not closed ", nexttoken);
                }
                if (nexttoken is PunctuatorToken && nexttoken.Symbol == Symbols.PT_COMMA)
                {
                    // Drop the comma
                    this.lexer.Next();
                }
                if (nexttoken is PunctuatorToken && nexttoken.Symbol == Symbols.PT_RIGHTPARENTHESIS)
                {
                    this.lexer.NextOfKind(Symbols.PT_RIGHTPARENTHESIS);
                    // Parameter parsing is complete 
                    break;
                }
            }
            return arguments;
        }





        /// <summary>
        /// parse function block
        /// starting with “(” arguments declarations
        /// </summary>
        /// <param name="functionName"></param>
        /// <param name="parentScope"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        private FunctionDeclaration ParseFunction(IdentifierToken functionName, Scope currentScope, Symbols access = null)
        {
            // next token (
            this.lexer.NextOfKind(Symbols.PT_LEFTPARENTHESIS);
            // parse arguments
            var arguments = this.ParseFunctionArguments(currentScope);
            // next token : Typed
            this.lexer.NextOfKind(Symbols.PT_COLON);
            var returnType = this.lexer.NextOfKind<TypedToken>();
            // create function scope
            var scope = new Scope(this, currentScope);
            // declare arguments variable
            scope.DeclareVariable(arguments);
            // parse function body
            var body = this.ParseBlock(scope);
            return new FunctionDeclaration()
            {
                Access = access,
                Body = body,
                Identifier = functionName,
                Parameters = arguments,
                Type = returnType
            };
        }

        /// <summary>
        /// analyze function declarations 
        /// starting with “function” 
        /// </summary>
        /// <param name="currentScope"></param>
        /// <param name="access"></param>
        /// <returns></returns>
        private Statement ParseFunctionDeclaration(Scope currentScope, Symbols access = null)
        {
            this.lexer.NextOfKind(Symbols.KW_FUNCTION);
            var functionName = this.lexer.NextOfKind<IdentifierToken>();
            // 校验 方法名是否有效
            return this.ParseFunction(functionName, currentScope, access);
        }

        /// <summary>
        /// parse export object
        /// starting with “export”
        /// </summary>
        /// <returns></returns>
        /// <exception cref="LexerException"></exception>
        private Statement ParseExportStatement(Scope currentScope)
        {
            this.lexer.NextOfKind(Symbols.KW_EXPORT);
            var token = this.lexer.LookAtHead();
            if (token is KeywordToken && token.Symbol == Symbols.KW_FUNCTION)
            {
                //this.lexer.NextOfKind(Symbols.KW_FUNCTION);
                return ParseFunctionDeclaration(currentScope, Symbols.KW_EXPORT);
            }
            throw this.InitParseException("Invalid keywords appear in export declaration .", token);
        }


        /// <summary>
        /// parse import module
        /// starting with “import”
        /// </summary>
        /// <returns></returns>
        private Statement ParseImport()
        {
            this.lexer.NextOfKind(Symbols.KW_IMPORT);
            var token = this.lexer.NextOfKind<StringToken>();
            this.lexer.NextOfKind(Symbols.PT_SEMICOLON);
            return new ImportDeclaration() { File = token };
        }

    }
}
