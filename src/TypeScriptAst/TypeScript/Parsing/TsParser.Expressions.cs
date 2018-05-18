// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsParser.Expressions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.Core.TypeScript.Parsing
{
    using System.Collections.Generic;
    using System.Linq;
    using Desalt.Core.Extensions;
    using Desalt.Core.TypeScript.Ast;
    using Desalt.Core.TypeScript.Ast.Expressions;
    using Factory = Ast.TsAstFactory;

    public partial class TsParser
    {
        private static readonly Dictionary<TsTokenCode, TsBinaryOperator> s_binaryOperatorMap =
            new Dictionary<TsTokenCode, TsBinaryOperator>
            {
                [TsTokenCode.EqualsEquals] = TsBinaryOperator.Equals,
                [TsTokenCode.ExclamationEquals] = TsBinaryOperator.NotEquals,
                [TsTokenCode.EqualsEqualsEquals] = TsBinaryOperator.StrictEquals,
                [TsTokenCode.ExclamationEqualsEquals] = TsBinaryOperator.StrictNotEquals,
                [TsTokenCode.LessThan] = TsBinaryOperator.LessThan,
                [TsTokenCode.GreaterThan] = TsBinaryOperator.GreaterThan,
                [TsTokenCode.LessThanEquals] = TsBinaryOperator.LessThanEqual,
                [TsTokenCode.GreaterThanEquals] = TsBinaryOperator.GreaterThanEqual,
                [TsTokenCode.Instanceof] = TsBinaryOperator.InstanceOf,
                [TsTokenCode.In] = TsBinaryOperator.In,
                [TsTokenCode.LessThanLessThan] = TsBinaryOperator.LeftShift,
                [TsTokenCode.GreaterThanGreaterThan] = TsBinaryOperator.SignedRightShift,
                [TsTokenCode.GreaterThanGreaterThanGreaterThan] = TsBinaryOperator.UnsignedRightShift,
                [TsTokenCode.Plus] = TsBinaryOperator.Add,
                [TsTokenCode.Minus] = TsBinaryOperator.Subtract,
                [TsTokenCode.Asterisk] = TsBinaryOperator.Multiply,
                [TsTokenCode.Slash] = TsBinaryOperator.Divide,
                [TsTokenCode.Percent] = TsBinaryOperator.Modulo,
            };

        private static readonly Dictionary<TsTokenCode, TsUnaryOperator> s_unaryOperatorMap =
            new Dictionary<TsTokenCode, TsUnaryOperator>
            {
                [TsTokenCode.Delete] = TsUnaryOperator.Delete,
                [TsTokenCode.Void] = TsUnaryOperator.Void,
                [TsTokenCode.Typeof] = TsUnaryOperator.Typeof,
                [TsTokenCode.PlusPlus] = TsUnaryOperator.PrefixIncrement,
                [TsTokenCode.MinusMinus] = TsUnaryOperator.PrefixDecrement,
                [TsTokenCode.Plus] = TsUnaryOperator.Plus,
                [TsTokenCode.Minus] = TsUnaryOperator.Minus,
                [TsTokenCode.Tilde] = TsUnaryOperator.BitwiseNot,
                [TsTokenCode.Exclamation] = TsUnaryOperator.LogicalNot,
            };

        private static readonly Dictionary<TsTokenCode, TsAssignmentOperator> s_assignmentOperatorMap =
            new Dictionary<TsTokenCode, TsAssignmentOperator>
            {
                [TsTokenCode.Equals] = TsAssignmentOperator.SimpleAssign,
                [TsTokenCode.AsteriskEquals] = TsAssignmentOperator.MultiplyAssign,
                [TsTokenCode.SlashEquals] = TsAssignmentOperator.DivideAssign,
                [TsTokenCode.PercentEquals] = TsAssignmentOperator.ModuloAssign,
                [TsTokenCode.PlusEquals] = TsAssignmentOperator.AddAssign,
                [TsTokenCode.MinusEquals] = TsAssignmentOperator.SubtractAssign,
                [TsTokenCode.LessThanLessThanEquals] = TsAssignmentOperator.LeftShiftAssign,
                [TsTokenCode.GreaterThanGreaterThanEquals] = TsAssignmentOperator.SignedRightShiftAssign,
                [TsTokenCode.GreaterThanGreaterThanGreaterThanEquals] = TsAssignmentOperator.UnsignedRightShiftAssign,
                [TsTokenCode.AmpersandEquals] = TsAssignmentOperator.BitwiseAndAssign,
                [TsTokenCode.CaretEquals] = TsAssignmentOperator.BitwiseXorAssign,
                [TsTokenCode.PipeEquals] = TsAssignmentOperator.BitwiseOrAssign,
            };

        /// <summary>
        /// Parses an expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Expression:
        ///     AssignmentExpression
        ///     Expression, AssignmentExpression
        /// ]]></code></remarks>
        private ITsExpression ParseExpression()
        {
            var expressions = new List<ITsExpression>();

            do
            {
                ITsExpression expression = ParseAssignmentExpression();
                expressions.Add(expression);
            }
            while (!_reader.IsAtEnd && _reader.ReadIf(TsTokenCode.Comma));

            if (expressions.Count > 1)
            {
                return Factory.CommaExpression(expressions.ToArray());
            }

            return expressions.Single();
        }

        private bool IsStartOfExpression()
        {
            TsTokenCode tokenCode = _reader.Peek().TokenCode;
            switch (tokenCode)
            {
                case TsTokenCode.Plus:
                case TsTokenCode.Minus:
                case TsTokenCode.Tilde:
                case TsTokenCode.Exclamation:
                case TsTokenCode.Delete:
                case TsTokenCode.Typeof:
                case TsTokenCode.Void:
                case TsTokenCode.PlusPlus:
                case TsTokenCode.MinusMinus:
                case TsTokenCode.LessThan:
                case TsTokenCode.Await:
                case TsTokenCode.Yield:
                    return true;
            }

            return IsStartOfIdentifier(tokenCode) || IsStartOfLeftHandSideExpression();
        }

        /// <summary>
        /// Parses an assignment expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// AssignmentExpression:
        ///     ConditionalExpression
        ///     YieldExpression (not yet supported)
        ///     ArrowFunction
        ///     LeftHandSideExpression = AssignmentExpression
        ///     LeftHandSideExpression AssignmentOperator AssignmentExpression
        ///
        /// ConditionalExpression:
        ///     LogicalORExpression
        ///     LogicalORExpression ? AssignmentExpression : AssignmentExpression
        /// ]]></code></remarks>
        private ITsExpression ParseAssignmentExpression()
        {
            // we don't currently support yield expressions - we can add it when we need it
            if (_reader.Peek().TokenCode == TsTokenCode.Yield)
            {
                throw NotYetImplementedException("yield expressions are not yet supported");
            }

            // arrow expressions can start the same way as a left hand side expression, so we need to
            // try parsing one and see what happens
            if (TryParse(ParseArrowFunction, out ITsArrowFunction arrowFunction))
            {
                return arrowFunction;
            }

            // ConditionalExpression starts with a LogigalORExpression while an assignment expression
            // can only start with LeftHandSideExpression. We have to start by parsing a
            // LogicalORExpression and then seeing what the next tokens are.
            ITsExpression expression = ParseLogicalOrExpression();

            // see if we have a ConditionalExpression
            if (_reader.ReadIf(TsTokenCode.Question))
            {
                ITsExpression condition = expression;
                ITsExpression whenTrue = ParseAssignmentExpression();
                Read(TsTokenCode.Colon);
                ITsExpression whenFalse = ParseAssignmentExpression();

                expression = Factory.Conditional(condition, whenTrue, whenFalse);
            }

            // see if we have an assignment expression, which must start with a LeftHandSideExpression
            if (IsLeftHandSideExpression(expression) && IsAssignmentOperator(_reader.Peek().TokenCode))
            {
                TsAssignmentOperator assignmentOperator = s_assignmentOperatorMap[_reader.Read().TokenCode];
                ITsExpression rightSide = ParseAssignmentExpression();
                expression = Factory.Assignment(expression, assignmentOperator, rightSide);
            }

            return expression;
        }

        private static bool IsAssignmentOperator(TsTokenCode tokenCode) =>
            s_assignmentOperatorMap.ContainsKey(tokenCode);

        /// <summary>
        /// Parses a logical OR expression of the form x || y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// LogicalORExpression:
        ///     LogicalANDExpression
        ///     LogicalORExpression || LogicalANDExpression
        /// ]]></code></remarks>
        private ITsExpression ParseLogicalOrExpression()
        {
            ITsExpression expression = ParseLogicalAndExpression();
            if (_reader.ReadIf(TsTokenCode.PipePipe))
            {
                ITsExpression leftSide = expression;
                ITsExpression rightSide = ParseLogicalOrExpression();
                expression = Factory.BinaryExpression(leftSide, TsBinaryOperator.LogicalOr, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a logical AND expression of the form x &amp;&amp; y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// LogicalANDExpression:
        ///     BitwiseORExpression
        ///     LogicalANDExpression && BitwiseORExpression
        /// ]]></code></remarks>
        private ITsExpression ParseLogicalAndExpression()
        {
            ITsExpression expression = ParseBitwiseOrExpression();
            if (_reader.ReadIf(TsTokenCode.AmpersandAmpersand))
            {
                ITsExpression leftSide = expression;
                ITsExpression rightSide = ParseLogicalAndExpression();
                expression = Factory.BinaryExpression(leftSide, TsBinaryOperator.LogicalAnd, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a bitwise OR expression of the form x | y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BitwiseORExpression:
        ///     BitwiseXORExpression
        ///     BitwiseORExpression | BitwiseXORExpression
        /// ]]></code></remarks>
        private ITsExpression ParseBitwiseOrExpression()
        {
            ITsExpression expression = ParseBitwiseXorExpression();
            if (_reader.ReadIf(TsTokenCode.Pipe))
            {
                ITsExpression leftSide = expression;
                ITsExpression rightSide = ParseBitwiseOrExpression();
                expression = Factory.BinaryExpression(leftSide, TsBinaryOperator.BitwiseOr, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a bitwise XOR expression of the form x ^ y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BitwiseXORExpression:
        ///     BitwiseANDExpression
        ///     BitwiseXORExpression ^ BitwiseANDExpression
        /// ]]></code></remarks>
        private ITsExpression ParseBitwiseXorExpression()
        {
            ITsExpression expression = ParseBitwiseAndExpression();
            if (_reader.ReadIf(TsTokenCode.Caret))
            {
                ITsExpression leftSide = expression;
                ITsExpression rightSide = ParseBitwiseXorExpression();
                expression = Factory.BinaryExpression(leftSide, TsBinaryOperator.BitwiseXor, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a bitwise AND expression of the form x &amp; y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// BitwiseANDExpression:
        ///     EqualityExpression
        ///     BitwiseANDExpression & EqualityExpression
        /// ]]></code></remarks>
        private ITsExpression ParseBitwiseAndExpression()
        {
            ITsExpression expression = ParseEqualityExpression();
            if (_reader.ReadIf(TsTokenCode.Ampersand))
            {
                ITsExpression leftSide = expression;
                ITsExpression rightSide = ParseBitwiseAndExpression();
                expression = Factory.BinaryExpression(leftSide, TsBinaryOperator.BitwiseAnd, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses an equality expression of the form x == y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// EqualityExpression:
        ///     RelationalExpression
        ///     EqualityExpression == RelationalExpression
        ///     EqualityExpression != RelationalExpression
        ///     EqualityExpression === RelationalExpression
        ///     EqualityExpression !== RelationalExpression
        /// ]]></code></remarks>
        private ITsExpression ParseEqualityExpression()
        {
            ITsExpression expression = ParseRelationalExpression();
            if (_reader.Peek()
                .TokenCode.IsOneOf(
                    TsTokenCode.EqualsEquals,
                    TsTokenCode.ExclamationEquals,
                    TsTokenCode.EqualsEqualsEquals,
                    TsTokenCode.ExclamationEqualsEquals))
            {
                ITsExpression leftSide = expression;
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                TsBinaryOperator op = s_binaryOperatorMap[tokenCode];
                ITsExpression rightSide = ParseEqualityExpression();
                expression = Factory.BinaryExpression(leftSide, op, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a relational expression of the form x &lt; y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// RelationalExpression:
        ///     ShiftExpression
        ///     RelationalExpression < ShiftExpression
        ///     RelationalExpression > ShiftExpression
        ///     RelationalExpression <= ShiftExpression
        ///     RelationalExpression >= ShiftExpression
        ///     RelationalExpression instanceof ShiftExpression
        ///     RelationalExpression in ShiftExpression
        /// ]]></code></remarks>
        private ITsExpression ParseRelationalExpression()
        {
            ITsExpression expression = ParseShiftExpression();
            if (_reader.Peek()
                .TokenCode.IsOneOf(
                    TsTokenCode.LessThan,
                    TsTokenCode.GreaterThan,
                    TsTokenCode.LessThanEquals,
                    TsTokenCode.GreaterThanEquals,
                    TsTokenCode.Instanceof,
                    TsTokenCode.In))
            {
                ITsExpression leftSide = expression;
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                TsBinaryOperator op = s_binaryOperatorMap[tokenCode];
                ITsExpression rightSide = ParseRelationalExpression();
                expression = Factory.BinaryExpression(leftSide, op, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a shift expression of the form x &lt;&lt; y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ShiftExpression:
        ///     AdditiveExpression
        ///     ShiftExpression << AdditiveExpression
        ///     ShiftExpression >> AdditiveExpression
        ///     ShiftExpression >>> AdditiveExpression
        /// ]]></code></remarks>
        private ITsExpression ParseShiftExpression()
        {
            ITsExpression expression = ParseAdditiveExpression();
            if (_reader.Peek()
                .TokenCode.IsOneOf(
                    TsTokenCode.LessThanLessThan,
                    TsTokenCode.GreaterThanGreaterThan,
                    TsTokenCode.GreaterThanGreaterThanGreaterThan))
            {
                ITsExpression leftSide = expression;
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                TsBinaryOperator op = s_binaryOperatorMap[tokenCode];
                ITsExpression rightSide = ParseShiftExpression();
                expression = Factory.BinaryExpression(leftSide, op, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a shift expression of the form x + y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// AdditiveExpression:
        ///     MultiplicativeExpression
        ///     AdditiveExpression + MultiplicativeExpression
        ///     AdditiveExpression - MultiplicativeExpression
        /// ]]></code></remarks>
        private ITsExpression ParseAdditiveExpression()
        {
            ITsExpression expression = ParseMultiplicativeExpression();
            if (_reader.Peek().TokenCode.IsOneOf(TsTokenCode.Plus, TsTokenCode.Minus))
            {
                ITsExpression leftSide = expression;
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                TsBinaryOperator op = s_binaryOperatorMap[tokenCode];
                ITsExpression rightSide = ParseAdditiveExpression();
                expression = Factory.BinaryExpression(leftSide, op, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a shift expression of the form x * y.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// MultiplicativeExpression:
        ///     UnaryExpression
        ///     MultiplicativeExpression MultiplicativeOperator UnaryExpression
        ///
        /// MultiplicativeOperator: one of
        ///     * / %
        /// ]]></code></remarks>
        private ITsExpression ParseMultiplicativeExpression()
        {
            ITsExpression expression = ParseUnaryExpression();
            if (_reader.Peek().TokenCode.IsOneOf(TsTokenCode.Asterisk, TsTokenCode.Slash, TsTokenCode.Percent))
            {
                ITsExpression leftSide = expression;
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                TsBinaryOperator op = s_binaryOperatorMap[tokenCode];
                ITsExpression rightSide = ParseMultiplicativeExpression();
                expression = Factory.BinaryExpression(leftSide, op, rightSide);
            }

            return expression;
        }

        /// <summary>
        /// Parses a unary expression of the form ++x.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// UnaryExpression:
        ///     PostfixExpression
        ///     delete UnaryExpression
        ///     void UnaryExpression
        ///     typeof UnaryExpression
        ///     ++ UnaryExpression
        ///     -- UnaryExpression
        ///     + UnaryExpression
        ///     - UnaryExpression
        ///     ~ UnaryExpression
        ///     ! UnaryExpression
        ///     < Type > UnaryExpression
        /// ]]></code></remarks>
        private ITsExpression ParseUnaryExpression()
        {
            ITsExpression expression;

            if (_reader.Peek()
                .TokenCode.IsOneOf(
                    TsTokenCode.Delete,
                    TsTokenCode.Void,
                    TsTokenCode.Typeof,
                    TsTokenCode.PlusPlus,
                    TsTokenCode.MinusMinus,
                    TsTokenCode.Plus,
                    TsTokenCode.Minus,
                    TsTokenCode.Tilde,
                    TsTokenCode.Exclamation))
            {
                TsTokenCode tokenCode = _reader.Read().TokenCode;
                TsUnaryOperator op = s_unaryOperatorMap[tokenCode];
                ITsExpression operand = ParseUnaryExpression();
                expression = Factory.UnaryExpression(operand, op);
            }
            else if (_reader.ReadIf(TsTokenCode.LessThan))
            {
                ITsType castType = ParseType();
                Read(TsTokenCode.GreaterThan);
                expression = ParseUnaryExpression();
                expression = Factory.Cast(castType, expression);
            }
            else
            {
                expression = ParsePostfixExpression();
            }

            return expression;
        }

        /// <summary>
        /// Parses a postfix expression of the form x++.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// PostfixExpression:
        ///     LeftHandSideExpression
        ///     LeftHandSideExpression [no LineTerminator here] ++
        ///     LeftHandSideExpression [no LineTerminator here] --
        /// ]]></code></remarks>
        private ITsExpression ParsePostfixExpression()
        {
            ITsExpression expression = ParseLeftHandSideExpression();

            if (_reader.ReadIf(TsTokenCode.PlusPlus))
            {
                expression = Factory.UnaryExpression(expression, TsUnaryOperator.PostfixIncrement);
            }
            else if (_reader.ReadIf(TsTokenCode.MinusMinus))
            {
                expression = Factory.UnaryExpression(expression, TsUnaryOperator.PostfixDecrement);
            }

            return expression;
        }

        /// <summary>
        /// Parses a left-hand-side expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// LeftHandSideExpression:
        ///     NewExpression
        ///     CallExpression
        ///
        /// NewExpression:
        ///     MemberExpression
        ///     new NewExpression
        ///
        /// CallExpression:
        ///     MemberExpression Arguments
        ///     SuperCall                           (starts with 'super')
        ///     CallExpression Arguments
        ///     CallExpression [ Expression ]
        ///     CallExpression . IdentifierName
        ///     CallExpression TemplateLiteral
        /// ]]></code></remarks>
        private ITsExpression ParseLeftHandSideExpression()
        {
            // we have to peek ahead two characters since it can be super[x] or super.x, which are
            // parsed as part of a member expression
            if (_reader.IsNext(TsTokenCode.Super, TsTokenCode.LeftParen))
            {
                return ParseSuperCall();
            }

            // NewExpression and CallExpression both start with a MemberExpression
            // (the 'new NewExpression' production is handled as part of the member expression)
            var expression = ParseMemberExpression();
            if (_reader.IsNext(TsTokenCode.LeftParen))
            {
                ITsArgumentList arguments = ParseArguments();
                expression = Factory.Call(expression, arguments);
            }

            // note that this is not an else if clause since we want to allow arguments to be parsed
            // first above and then check for brackets and dots
            while (_reader.IsNext(TsTokenCode.LeftBracket) || _reader.IsNext(TsTokenCode.Dot))
            {
                if (_reader.ReadIf(TsTokenCode.LeftBracket))
                {
                    ITsExpression leftSide = expression;
                    ITsExpression bracketContents = ParseExpression();
                    Read(TsTokenCode.RightBracket);
                    expression = Factory.MemberBracket(leftSide, bracketContents);
                }
                else if (_reader.ReadIf(TsTokenCode.Dot))
                {
                    ITsIdentifier identifier = ParseIdentifierOrKeyword();
                    expression = Factory.MemberDot(expression, identifier.Text);
                }
            }

            // template literals aren't supported yet

            return expression;
        }

        private bool IsStartOfLeftHandSideExpression()
        {
            switch (_reader.Peek().TokenCode)
            {
                // LeftHandSideExpression -> NewExpression
                case TsTokenCode.New:

                // LeftHandSideExpression -> CallExpression -> SuperCall
                case TsTokenCode.Super:
                    return true;

                default:
                    return IsStartOfPrimaryExpression();
            }
        }

        private static bool IsLeftHandSideExpression(ITsExpression expression)
        {
            switch (expression)
            {
                // LeftHandSideExpression
                case ITsNewCallExpression _:

                // SuperCall and CallExpression
                case ITsSuperDotExpression _:
                case ITsSuperBracketExpression _:
                case ITsSuperCallExpression _:
                case ITsCallExpression _:
                case ITsMemberBracketExpression _:
                case ITsMemberDotExpression _:

                // MemberExpression
                case ITsArrayLiteral _:
                case ITsParenthesizedExpression _:
                case ITsObjectLiteral _:
                case ITsClassExpression _:
                case ITsFunctionExpression _:
                case ITsIdentifier _:
                case ITsRegularExpressionLiteral _:
                case ITsNumericLiteral _:
                case ITsStringLiteral _:
                case ITsBooleanLiteral _:
                case ITsNullLiteral _:
                case ITsThis _:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Parses a super call.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// SuperCall:
        ///     super Arguments
        /// ]]></code></remarks>
        private ITsExpression ParseSuperCall()
        {
            Read(TsTokenCode.Super);
            ITsArgumentList arguments = ParseArguments();
            return Factory.SuperCall(arguments);
        }

        /// <summary>
        /// Parses arguments.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// Arguments: (see TypeScript grammar copied below)
        ///     ( )
        ///     ( ArgumentList )
        ///
        /// Arguments: (Modified)
        ///     TypeArgumentsOpt ( ArgumentListOpt )
        ///
        /// ArgumentList:
        ///     AssignmentExpression
        ///     ... AssignmentExpression
        ///     ArgumentList , AssignmentExpression
        ///     ArgumentList , ... AssignmentExpression
        /// ]]></code></remarks>
        private ITsArgumentList ParseArguments()
        {
            ITsType[] typeArguments = ParseOptionalTypeArguments();

            Read(TsTokenCode.LeftParen);

            // read an empty argument list
            if (_reader.ReadIf(TsTokenCode.RightParen))
            {
                return Factory.ArgumentList(typeArguments);
            }

            // parse each argument
            var arguments = new List<ITsArgument>();
            do
            {
                bool isSpreadArgument = _reader.ReadIf(TsTokenCode.DotDotDot);
                ITsExpression expression = ParseAssignmentExpression();
                ITsArgument argument = Factory.Argument(expression, isSpreadArgument);
                arguments.Add(argument);

                if (isSpreadArgument)
                {
                    break;
                }
            }
            while (_reader.ReadIf(TsTokenCode.Comma));

            Read(TsTokenCode.RightParen);

            return Factory.ArgumentList(typeArguments, arguments.ToArray());
        }

        /// <summary>
        /// Parses a member expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// MemberExpression:
        ///     PrimaryExpression
        ///     MemberExpression [ Expression ]
        ///     MemberExpression . IdentifierName
        ///     MemberExpression TemplateLiteral
        ///     SuperProperty
        ///     MetaProperty
        ///     new MemberExpression Arguments
        /// ]]></code></remarks>
        private ITsExpression ParseMemberExpression()
        {
            if (_reader.ReadIf(TsTokenCode.New))
            {
                // MetaProperty: NewTarget
                // NewTarget: new . target
                if (_reader.IsNext(TsTokenCode.Dot))
                {
                    throw NotYetImplementedException("MetaProperty productions are not yet supported");
                }

                ITsExpression leftSide = ParseMemberExpression();
                ITsArgumentList arguments = ParseArguments();
                return Factory.NewCall(leftSide, arguments);
            }

            if (_reader.IsNext(TsTokenCode.Super))
            {
                return ParseSuperProperty();
            }

            ITsExpression expression = ParsePrimaryExpression();

            while (_reader.IsNext(TsTokenCode.LeftBracket) || _reader.IsNext(TsTokenCode.Dot))
            {
                if (_reader.ReadIf(TsTokenCode.LeftBracket))
                {
                    ITsExpression leftSide = expression;
                    ITsExpression bracketContents = ParseExpression();
                    Read(TsTokenCode.RightBracket);
                    expression = Factory.MemberBracket(leftSide, bracketContents);
                }
                else if (_reader.ReadIf(TsTokenCode.Dot))
                {
                    ITsIdentifier identifier = ParseIdentifierOrKeyword();
                    expression = Factory.MemberDot(expression, identifier.Text);
                }
            }

            // template literals aren't supported yet

            return expression;
        }

        /// <summary>
        /// Parses a super property.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// SuperProperty:
        ///     super [ Expression ]
        ///     super . IdentifierName
        /// ]]></code></remarks>
        private ITsExpression ParseSuperProperty()
        {
            Read(TsTokenCode.Super);
            if (_reader.ReadIf(TsTokenCode.LeftBracket))
            {
                ITsExpression bracketContents = ParseExpression();
                Read(TsTokenCode.RightBracket);
                return Factory.SuperBracket(bracketContents);
            }

            Read(TsTokenCode.Dot);
            ITsIdentifier identifier = ParseIdentifierOrKeyword();
            return Factory.SuperDot(identifier.Text);
        }

        private bool IsStartOfPrimaryExpression()
        {
            switch (_reader.Peek().TokenCode)
            {
                case TsTokenCode.This:

                // IdentifierReference
                case TsTokenCode.Identifier:
                // ReSharper disable once PatternAlwaysMatches
                case TsTokenCode tc when IsStartOfIdentifier(tc):

                // Literal
                case TsTokenCode.Null:
                case TsTokenCode.True:
                case TsTokenCode.False:
                case TsTokenCode.DecimalLiteral:
                case TsTokenCode.BinaryIntegerLiteral:
                case TsTokenCode.OctalIntegerLiteral:
                case TsTokenCode.HexIntegerLiteral:
                case TsTokenCode.StringLiteral:

                // ArrayLiteral
                case TsTokenCode.LeftBracket:

                // ObjectLiteral
                case TsTokenCode.LeftBrace:

                // FunctionExpression and GeneratorExpression
                case TsTokenCode.Function:

                // ClassExpression
                case TsTokenCode.Class:

                // RegularExpressionLiteral:
                case TsTokenCode.Slash:

                // TemplateLiteral:
                case TsTokenCode.TemplateLiteral:

                // ParenthesizedExpression
                case TsTokenCode.LeftParen:
                    return true;

                default:
                    return false;
            }
        }

        /// <summary>
        /// Parses a primary expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// PrimaryExpression:
        ///     this
        ///     IdentifierReference
        ///     Literal
        ///     ArrayLiteral
        ///     ObjectLiteral
        ///     FunctionExpression
        ///     ClassExpression
        ///     GeneratorExpression
        ///     RegularExpressionLiteral
        ///     TemplateLiteral
        ///     CoverParenthesizedExpressionAndArrowParameterList
        ///
        /// When processing the production
        /// PrimaryExpression: CoverParenthesizedExpressionAndArrowParameterList the interpretation of
        /// CoverParenthesizedExpressionAndArrowParameterList is refined using the following grammar:
        ///
        /// ParenthesizedExpression:
        ///     ( Expression )
        /// ]]></code></remarks>
        private ITsExpression ParsePrimaryExpression()
        {
            switch (_reader.Peek().TokenCode)
            {
                case TsTokenCode.This:
                    _reader.Skip();
                    return Factory.This;

                case TsTokenCode.Null:
                case TsTokenCode.True:
                case TsTokenCode.False:
                case TsTokenCode.DecimalLiteral:
                case TsTokenCode.BinaryIntegerLiteral:
                case TsTokenCode.OctalIntegerLiteral:
                case TsTokenCode.HexIntegerLiteral:
                case TsTokenCode.StringLiteral:
                    return ParseLiteral();

                case TsTokenCode.LeftBracket:
                    return ParseArrayLiteral();

                case TsTokenCode.LeftBrace:
                    return ParseObjectLiteral();

                case TsTokenCode.LeftParen:
                    return ParseParenthesizedExpression();

                case TsTokenCode.Function:
                    if (_reader.IsNext(TsTokenCode.Function, TsTokenCode.Asterisk))
                    {
                        return ParseGeneratorExpression();
                    }
                    return ParseFunctionExpression();

                case TsTokenCode.Class:
                    return ParseClassExpression();

                case TsTokenCode.TemplateLiteral:
                    return ParseTemplateLiteral();

                default:
                    return ParseIdentifierReference();
            }
        }

        /// <summary>
        /// Parses a generator expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// GeneratorExpression:
        ///     function * BindingIdentifierOpt ( FormalParameters ) { GeneratorBody }
        /// ]]></code></remarks>
        private ITsExpression ParseGeneratorExpression()
        {
            throw NotYetImplementedException("Generator expressions are not yet supported");
        }

        /// <summary>
        /// Parses a class expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ClassExpression:
        ///     class BindingIdentifierOpt ClassTail
        ///
        /// ClassTail:
        ///     ClassHeritageOpt { ClassBodyOpt }
        ///
        /// ClassHeritage: ( Modified )
        ///     ClassExtendsClauseOpt ImplementsClauseOpt
        ///
        /// ClassExtendsClause:
        ///     extends ClassType
        ///
        /// ClassType:
        ///     TypeReference
        ///
        /// ImplementsClause:
        ///     implements ClassOrInterfaceTypeList
        ///
        /// ClassBody:
        ///     ClassElementList
        ///
        /// ClassElementList:
        ///     ClassElement
        ///     ClassElementList ClassElement
        ///
        /// ClassElement: ( Modified )
        ///     ConstructorDeclaration
        ///     PropertyMemberDeclaration
        ///     IndexMemberDeclaration
        /// ]]></code></remarks>
        private ITsClassExpression ParseClassExpression()
        {
            throw NotYetImplementedException("Class expressions are not yet supported");
        }

        /// <summary>
        /// Parses a parenthesized expression.
        /// </summary>
        /// <remarks><code><![CDATA[
        /// ParenthesizedExpression:
        ///     ( Expression )
        /// ]]></code></remarks>
        private ITsParenthesizedExpression ParseParenthesizedExpression()
        {
            Read(TsTokenCode.LeftParen);
            ITsExpression expression = ParseExpression();
            Read(TsTokenCode.RightParen);

            return Factory.ParenthesizedExpression(expression);
        }
    }
}
