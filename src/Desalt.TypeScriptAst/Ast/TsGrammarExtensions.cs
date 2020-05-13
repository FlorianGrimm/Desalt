// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsGrammarExtensions.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Collections.Generic;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Ast.Declarations;
    using Desalt.TypeScriptAst.Ast.Expressions;
    using Desalt.TypeScriptAst.Ast.Statements;
    using Desalt.TypeScriptAst.Ast.Types;
    using Desalt.TypeScriptAst.Emit;

    /// <summary>
    /// Contains static extension methods for <see cref="ITsAstNode"/> objects.
    /// </summary>
    public static class TsGrammarExtensions
    {
        /// <summary>
        /// Creates a new <see cref="ITsParenthesizedType"/> wrapping the specified type.
        /// </summary>
        /// <param name="type">The type to wrap inside of parentheses.</param>
        /// <returns>A new <see cref="ITsParenthesizedType"/> wrapping the specified type.</returns>
        public static ITsParenthesizedType WithParentheses(this ITsType type)
        {
            return new TsParenthesizedType(type);
        }

        /// <summary>
        /// Creates a new <see cref="ITsParenthesizedExpression"/> wrapping the specified expression.
        /// </summary>
        /// <param name="expression">The expression to wrap inside of parentheses.</param>
        /// <returns>A new <see cref="ITsParenthesizedExpression"/> wrapping the specified expression.</returns>
        public static ITsParenthesizedExpression WithParentheses(this ITsExpression expression)
        {
            return new TsParenthesizedExpression(expression);
        }

        /// <summary>
        /// Creates a new statement from the expression.
        /// </summary>
        public static ITsExpressionStatement ToStatement(this ITsExpression expression)
        {
            return new TsExpressionStatement(expression);
        }

        /// <summary>
        /// Wraps the statement in a block statement, unless it's already a <see cref="ITsBlockStatement"/>, in which
        /// case it is just returned.
        /// </summary>
        public static ITsBlockStatement ToBlock(this ITsStatement statement)
        {
            return statement is ITsBlockStatement blockStatement
                ? blockStatement
                : new TsBlockStatement(new[] { statement });
        }

        /// <summary>
        /// Converts the expression to a statement and wraps it in a block statement.
        /// </summary>
        public static ITsBlockStatement ToBlock(this ITsExpression expression)
        {
            return new TsBlockStatement(expression.ToStatement().ToSafeArray());
        }

        /// <summary>
        /// Converts the variable statement to an exported variable statement.
        /// </summary>
        public static ITsExportedVariableStatement ToExported(this ITsVariableStatement statement)
        {
            return new TsExportedVariableStatement(statement);
        }

        /// <summary>
        /// Converts the declaration to an exported declaration.
        /// </summary>
        public static ITsExportedDeclaration ToExported(this ITsDeclaration declaration)
        {
            return new TsExportedDeclaration(declaration);
        }

        /// <summary>
        /// Converts a unary operator to its code representation.
        /// </summary>
        public static string ToCodeDisplay(this TsUnaryOperator unaryOperator)
        {
            return unaryOperator switch
            {
                TsUnaryOperator.Delete => "delete",
                TsUnaryOperator.Void => "void",
                TsUnaryOperator.Typeof => "typeof",
                TsUnaryOperator.PrefixIncrement => "++",
                TsUnaryOperator.PostfixIncrement => "++",
                TsUnaryOperator.PrefixDecrement => "--",
                TsUnaryOperator.PostfixDecrement => "--",
                TsUnaryOperator.Plus => "+",
                TsUnaryOperator.Minus => "-",
                TsUnaryOperator.BitwiseNot => "~",
                TsUnaryOperator.LogicalNot => "!",
                TsUnaryOperator.Cast => throw new InvalidOperationException(
                    $"Use {nameof(TsCastExpression.Emit)} instead"),
                _ => throw new ArgumentOutOfRangeException(nameof(unaryOperator), unaryOperator, message: null)
            };
        }

        /// <summary>
        /// Converts a binary operator to its code representation.
        /// </summary>
        public static string ToCodeDisplay(this TsBinaryOperator binaryOperator)
        {
            return binaryOperator switch
            {
                TsBinaryOperator.Multiply => "*",
                TsBinaryOperator.Divide => "/",
                TsBinaryOperator.Modulo => "%",
                TsBinaryOperator.Add => "+",
                TsBinaryOperator.Subtract => "-",
                TsBinaryOperator.LeftShift => "<<",
                TsBinaryOperator.SignedRightShift => ">>",
                TsBinaryOperator.UnsignedRightShift => ">>>",
                TsBinaryOperator.LessThan => "<",
                TsBinaryOperator.GreaterThan => ">",
                TsBinaryOperator.LessThanEqual => "<=",
                TsBinaryOperator.GreaterThanEqual => ">=",
                TsBinaryOperator.InstanceOf => "instanceof",
                TsBinaryOperator.In => "in",
                TsBinaryOperator.Equals => "==",
                TsBinaryOperator.NotEquals => "!=",
                TsBinaryOperator.StrictEquals => "===",
                TsBinaryOperator.StrictNotEquals => "!==",
                TsBinaryOperator.BitwiseAnd => "&",
                TsBinaryOperator.BitwiseXor => "^",
                TsBinaryOperator.BitwiseOr => "|",
                TsBinaryOperator.LogicalAnd => "&&",
                TsBinaryOperator.LogicalOr => "||",
                _ => throw new ArgumentOutOfRangeException(nameof(binaryOperator), binaryOperator, null)
            };
        }

        /// <summary>
        /// Converts an assignment operator to its code representation.
        /// </summary>
        public static string ToCodeDisplay(this TsAssignmentOperator assignmentOperator)
        {
            return assignmentOperator switch
            {
                TsAssignmentOperator.SimpleAssign => "=",
                TsAssignmentOperator.MultiplyAssign => "*=",
                TsAssignmentOperator.DivideAssign => "/=",
                TsAssignmentOperator.ModuloAssign => "%=",
                TsAssignmentOperator.AddAssign => "+=",
                TsAssignmentOperator.SubtractAssign => "-=",
                TsAssignmentOperator.LeftShiftAssign => "<<=",
                TsAssignmentOperator.SignedRightShiftAssign => ">>=",
                TsAssignmentOperator.UnsignedRightShiftAssign => ">>>=",
                TsAssignmentOperator.BitwiseAndAssign => "&=",
                TsAssignmentOperator.BitwiseXorAssign => "^=",
                TsAssignmentOperator.BitwiseOrAssign => "|=",
                _ => throw new ArgumentOutOfRangeException(
                    nameof(assignmentOperator),
                    assignmentOperator,
                    message: null)
            };
        }

        /// <summary>
        /// Converts a <see cref="VariableDeclarationKind"/> to a code display representation, which
        /// includes a trailing space.
        /// </summary>
        public static string CodeDisplay(this VariableDeclarationKind declarationKind)
        {
            return declarationKind.ToString().ToLowerInvariant() + " ";
        }

        /// <summary>
        /// Emits a <see cref="VariableDeclarationKind"/> to the specified emitter, which includes a
        /// trailing space.
        /// </summary>
        public static void Emit(this VariableDeclarationKind declarationKind, Emitter emitter)
        {
            emitter.Write(declarationKind.CodeDisplay());
        }

        /// <summary>
        /// Emits a comma-separated list, but only if the items are not null. Shortcut for
        /// <see cref="Emitter.WriteCommaList"/>.
        /// </summary>
        public static void EmitCommaList(this IReadOnlyList<ITsAstNode> items, Emitter emitter)
        {
            if (items != null)
            {
                emitter.WriteCommaList(items);
            }
        }

        /// <summary>
        /// Writes a statement on a new line unless the statement is a block, in which case the block
        /// will start on the same line.
        /// </summary>
        /// <param name="statement">The statement to emit.</param>
        /// <param name="emitter">The emitter to write to.</param>
        /// <param name="prefixForIndentedStatement">
        /// If supplied, the prefix is written before the statement when it's a not a block statement.
        /// </param>
        /// <param name="prefixForBlock">
        /// If supplied, the prefix is written before the statement when it's a block statement.
        /// </param>
        /// <param name="newlineAfterBlock">
        /// Indicates whether to add a newline after the block if it's a block statement.
        /// </param>
        public static void EmitIndentedOrInBlock(
            this ITsStatement statement,
            Emitter emitter,
            string prefixForIndentedStatement = ")",
            string prefixForBlock = ") ",
            bool newlineAfterBlock = false)
        {
            bool isBlockStatement = statement is ITsBlockStatement;
            emitter.WriteStatementIndentedOrInBlock(
                statement, isBlockStatement, prefixForIndentedStatement, prefixForBlock, newlineAfterBlock);
        }

        /// <summary>
        /// Writes out a ": type" type annotation if the type is not null.
        /// </summary>
        /// <param name="type">The type annotation to write.</param>
        /// <param name="emitter">The emitter to write to.</param>
        public static void EmitOptionalTypeAnnotation(this ITsType type, Emitter emitter)
        {
            if (type != null)
            {
                emitter.Write(": ");
                type.Emit(emitter);
            }
        }

        /// <summary>
        /// Writes out a " = expression" assignment if the expression is not null.
        /// </summary>
        /// <param name="expression">The expression to assign.</param>
        /// <param name="emitter">The emitter to write to.</param>
        public static void EmitOptionalAssignment(this ITsExpression expression, Emitter emitter)
        {
            if (expression != null)
            {
                emitter.Write(" = ");
                expression.Emit(emitter);
            }
        }

        public static void EmitOptional(this TsAccessibilityModifier? accessibilityModifier, Emitter emitter)
        {
            if (accessibilityModifier != null)
            {
                emitter.Write(accessibilityModifier.Value.ToString().ToLowerInvariant());
                emitter.Write(" ");
            }
        }
    }
}
