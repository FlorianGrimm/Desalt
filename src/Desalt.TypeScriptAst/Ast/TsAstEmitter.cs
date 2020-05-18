// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TsAstEmitter.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

#pragma warning disable IDE0060 // Remove unused parameter

namespace Desalt.TypeScriptAst.Ast
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Desalt.CompilerUtilities.Extensions;
    using Desalt.TypeScriptAst.Emit;

    internal static class TsAstEmitter
    {
        public static Emitter Write(this Emitter emitter, ITsAstNode node)
        {
            node.Emit(emitter);
            return emitter;
        }

        public static void EmitWhitespaceTrivia(Emitter emitter, ITsWhitespaceTrivia whitespaceTrivia)
        {
            if (whitespaceTrivia.IsNewline)
            {
                emitter.WriteLine();
            }
            else
            {
                emitter.Write(whitespaceTrivia.Text);
            }
        }

        public static void EmitMultiLineComment(Emitter emitter, ITsMultiLineComment multiLineComment)
        {
            string prefix = multiLineComment.IsJsDoc ? "/**" : "/*";
            string space = multiLineComment.PreserveSpacing ? string.Empty : " ";

            int count = multiLineComment.Lines.Length;
            if (count == 0)
            {
                emitter.Write($"{prefix}*/");
            }
            else if (count == 1 && !multiLineComment.IsJsDoc)
            {
                emitter.Write($"{prefix}{space}{multiLineComment.Lines.First()}{space}*/");
            }
            else
            {
                if (multiLineComment.IsJsDoc)
                {
                    emitter.WriteLine(prefix);
                    emitter.WriteLine($" * {multiLineComment.Lines.First()}");
                }
                else
                {
                    emitter.WriteLine($"{prefix}{space}{multiLineComment.Lines.First()}");
                }

                foreach (string line in multiLineComment.Lines.Skip(1))
                {
                    emitter.Write(" * ");
                    emitter.WriteLine(line);
                }

                emitter.WriteLine(" */");
            }
        }

        public static void EmitSingleLineComment(Emitter emitter, ITsSingleLineComment singleLineComment)
        {
            string space = singleLineComment.PreserveSpacing ? string.Empty : " ";

            if (singleLineComment.OmitNewLineAtEnd)
            {
                emitter.Write($"//{space}{singleLineComment.Text}");
            }
            else
            {
                emitter.WriteLine($"//{space}{singleLineComment.Text}");
            }
        }

        public static void EmitIdentifier(Emitter emitter, ITsIdentifier identifier)
        {
            emitter.Write(identifier.Text);
        }

        public static void EmitThis(Emitter emitter, ITsThis @this)
        {
            emitter.Write("this");
        }

        public static void EmitParenthesizedExpression(Emitter emitter, ITsParenthesizedExpression expression)
        {
            emitter.Write("(").Write(expression.Expression).Write(")");
        }

        public static void EmitNullLiteral(Emitter emitter, ITsNullLiteral nullLiteral)
        {
            emitter.Write("null");
        }

        public static void EmitBooleanLiteral(Emitter emitter, ITsBooleanLiteral booleanLiteral)
        {
            emitter.Write(booleanLiteral.Value ? "true" : "false");
        }

        public static void EmitNumericLiteral(Emitter emitter, ITsNumericLiteral numericLiteral)
        {
            double value = numericLiteral.Value;
            string number = numericLiteral.Kind switch
            {
                TsNumericLiteralKind.Decimal => value.ToString(CultureInfo.InvariantCulture),
                TsNumericLiteralKind.BinaryInteger => "0b" + Convert.ToString((long)value, 2),
                TsNumericLiteralKind.OctalInteger => "0o" + Convert.ToString((long)value, 8),
                TsNumericLiteralKind.HexInteger => "0x" + Convert.ToString((long)value, 16),
                _ => throw new ArgumentOutOfRangeException(nameof(numericLiteral.Kind))
            };

            emitter.Write(number);
        }

        public static void EmitStringLiteral(Emitter emitter, ITsStringLiteral stringLiteral)
        {
            string quoteChar = stringLiteral.QuoteKind == StringLiteralQuoteKind.SingleQuote ? "'" : "\"";
            emitter.Write($"{quoteChar}{stringLiteral.Value.Replace(quoteChar, "\\" + quoteChar)}{quoteChar}");
        }

        public static void EmitRegularExpressionLiteral(
            Emitter emitter,
            ITsRegularExpressionLiteral regularExpressionLiteral)
        {
            emitter.Write($"/{regularExpressionLiteral.Body}/{regularExpressionLiteral.Flags ?? string.Empty}");
        }

        public static void EmitArrayLiteral(Emitter emitter, ITsArrayLiteral arrayLiteral)
        {
            emitter.WriteList(arrayLiteral.Elements, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }

        public static void EmitArrayElement(Emitter emitter, ITsArrayElement arrayElement)
        {
            emitter.WriteIf(arrayElement.IsSpreadElement, "...").Write(arrayElement.Expression);
        }

        public static void EmitObjectLiteral(Emitter emitter, ITsObjectLiteral objectLiteral)
        {
            emitter.WriteCommaNewlineSeparatedBlock(objectLiteral.PropertyDefinitions);
        }

        public static void EmitCoverInitializedName(Emitter emitter, ITsCoverInitializedName coverInitializedName)
        {
            coverInitializedName.Identifier.Emit(emitter);
            coverInitializedName.Initializer.EmitOptionalAssignment(emitter);
        }

        public static void EmitPropertyAssignment(Emitter emitter, ITsPropertyAssignment propertyAssignment)
        {
            emitter.Write(propertyAssignment.PropertyName).Write(": ").Write(propertyAssignment.Initializer);
        }

        public static void EmitComputedPropertyName(Emitter emitter, ITsComputedPropertyName computedPropertyName)
        {
            emitter.Write("[").Write(computedPropertyName.Expression).Write("]");
        }

        public static void EmitTemplatePart(Emitter emitter, ITsTemplatePart templatePart)
        {
            emitter.Write(templatePart.Template);

            if (templatePart.Expression != null)
            {
                emitter.Write("${").Write(templatePart.Expression).Write("}");
            }
        }

        public static void EmitTemplateLiteral(Emitter emitter, ITsTemplateLiteral templateLiteral)
        {
            emitter.Write("`").WriteList(templateLiteral.Parts, indent: false).Write("`");
        }

        public static void EmitMemberBracketExpression(
            Emitter emitter,
            ITsMemberBracketExpression memberBracketExpression)
        {
            emitter.Write(memberBracketExpression.LeftSide)
                .Write("[")
                .Write(memberBracketExpression.BracketContents)
                .Write("]");
        }

        public static void EmitMemberDotExpression(Emitter emitter, ITsMemberDotExpression memberDotExpression)
        {
            emitter.Write(memberDotExpression.LeftSide).Write($".{memberDotExpression.DotName}");
        }

        public static void EmitSuperBracketExpression(Emitter emitter, ITsSuperBracketExpression superBracketExpression)
        {
            emitter.Write("super[").Write(superBracketExpression.BracketContents).Write("]");
        }

        public static void EmitSuperDotExpression(Emitter emitter, ITsSuperDotExpression superDotExpression)
        {
            emitter.Write($"super.{superDotExpression.DotName}");
        }

        public static void EmitNewTargetExpression(Emitter emitter, ITsNewTargetExpression newTargetExpression)
        {
            emitter.Write("new.target");
        }

        public static void EmitCallExpression(Emitter emitter, ITsCallExpression callExpression)
        {
            emitter.Write(callExpression.LeftSide).Write(callExpression.ArgumentList);
        }

        public static void EmitNewCallExpression(Emitter emitter, ITsNewCallExpression newCallExpression)
        {
            emitter.Write("new ").Write(newCallExpression.LeftSide).Write(newCallExpression.ArgumentList);
        }

        public static void EmitSuperCallExpression(Emitter emitter, TsSuperCallExpression superCallExpression)
        {
            emitter.Write("super").Write(superCallExpression.ArgumentList);
        }

        public static void EmitArgumentList(Emitter emitter, ITsArgumentList argumentList)
        {
            if (!argumentList.TypeArguments.IsEmpty)
            {
                emitter.WriteList(
                    argumentList.TypeArguments,
                    indent: false,
                    prefix: "<",
                    suffix: ">",
                    itemDelimiter: ", ");
            }

            emitter.WriteParameterList(argumentList.Arguments);
        }

        public static void EmitArgument(Emitter emitter, ITsArgument argument)
        {
            emitter.WriteIf(argument.IsSpreadArgument, "... ").Write(argument.Expression);
        }

        public static void EmitUnaryExpression(Emitter emitter, ITsUnaryExpression unaryExpression)
        {
            switch (unaryExpression.Operator)
            {
                case TsUnaryOperator.Delete:
                case TsUnaryOperator.Void:
                case TsUnaryOperator.Typeof:
                    emitter.Write($"{unaryExpression.Operator.ToCodeDisplay()} ");
                    unaryExpression.Operand.Emit(emitter);
                    break;

                case TsUnaryOperator.PrefixIncrement:
                case TsUnaryOperator.PrefixDecrement:
                case TsUnaryOperator.Plus:
                case TsUnaryOperator.Minus:
                case TsUnaryOperator.BitwiseNot:
                case TsUnaryOperator.LogicalNot:
                    emitter.Write(unaryExpression.Operator.ToCodeDisplay());
                    unaryExpression.Operand.Emit(emitter);
                    break;

                case TsUnaryOperator.PostfixIncrement:
                case TsUnaryOperator.PostfixDecrement:
                    unaryExpression.Operand.Emit(emitter);
                    emitter.Write(unaryExpression.Operator.ToCodeDisplay());
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(unaryExpression.Operator));
            }
        }

        public static void EmitCastExpression(Emitter emitter, ITsCastExpression castExpression)
        {
            emitter.Write("<").Write(castExpression.CastType).Write(">").Write(castExpression.Expression);
        }

        public static void EmitBinaryExpression(Emitter emitter, ITsBinaryExpression binaryExpression)
        {
            emitter.Write(binaryExpression.LeftSide)
                .Write($" {binaryExpression.Operator.ToCodeDisplay()} ")
                .Write(binaryExpression.RightSide);
        }

        public static void EmitConditionalExpression(Emitter emitter, ITsConditionalExpression conditionalExpression)
        {
            emitter.Write(conditionalExpression.Condition)
                .Write(" ? ")
                .Write(conditionalExpression.WhenTrue)
                .Write(" : ")
                .Write(conditionalExpression.WhenFalse);
        }

        public static void EmitAssignmentExpression(Emitter emitter, ITsAssignmentExpression assignmentExpression)
        {
            emitter.Write(assignmentExpression.LeftSide)
                .Write($" {assignmentExpression.Operator.ToCodeDisplay()} ")
                .Write(assignmentExpression.RightSide);
        }

        public static void EmitCommaExpression(Emitter emitter, ITsCommaExpression commaExpression)
        {
            emitter.WriteList(commaExpression.Expressions, indent: false, itemDelimiter: ", ");
        }

        public static void EmitBlockStatement(Emitter emitter, ITsBlockStatement blockStatement)
        {
            emitter.WriteBlock(blockStatement.Statements, skipNewlines: true);
        }

        public static void EmitLexicalDeclaration(Emitter emitter, ITsLexicalDeclaration lexicalDeclaration)
        {
            emitter.Write(lexicalDeclaration.IsConst ? "const " : "let ")
                .WriteList(lexicalDeclaration.Declarations, indent: false, itemDelimiter: ", ")
                .WriteLine(";");
        }

        public static void EmitVariableStatement(Emitter emitter, ITsVariableStatement variableStatement)
        {
            emitter.Write("var ")
                .WriteList(variableStatement.Declarations, indent: false, itemDelimiter: ", ")
                .WriteLine(";");
        }

        public static void EmitObjectBindingPattern(Emitter emitter, ITsObjectBindingPattern objectBindingPattern)
        {
            emitter.WriteList(
                objectBindingPattern.Properties,
                indent: false,
                prefix: "{",
                suffix: "}",
                itemDelimiter: ", ",
                emptyContents: "{}");
        }

        public static void EmitArrayBindingPattern(Emitter emitter, ITsArrayBindingPattern arrayBindingPattern)
        {
            if (arrayBindingPattern.RestElement != null)
            {
                emitter.Write("[")
                    .WriteList(
                        arrayBindingPattern.Elements,
                        indent: false,
                        itemDelimiter: ", ",
                        delimiterAfterLastItem: true)
                    .Write("...")
                    .Write(arrayBindingPattern.RestElement)
                    .Write("]");
            }
            else
            {
                emitter.WriteList(
                    arrayBindingPattern.Elements,
                    indent: false,
                    prefix: "[",
                    suffix: "]",
                    itemDelimiter: ", ",
                    emptyContents: "[]");
            }
        }

        public static void EmitSingleNameBinding(Emitter emitter, ITsSingleNameBinding singleNameBinding)
        {
            singleNameBinding.Name.Emit(emitter);
            singleNameBinding.DefaultValue?.EmitOptionalAssignment(emitter);
        }

        public static void EmitPropertyNameBinding(Emitter emitter, ITsPropertyNameBinding propertyNameBinding)
        {
            emitter.Write(propertyNameBinding.PropertyName).Write(": ").Write(propertyNameBinding.BindingElement);
        }

        public static void EmitPatternBinding(Emitter emitter, ITsPatternBinding patternBinding)
        {
            patternBinding.BindingPattern.Emit(emitter);
            patternBinding.Initializer?.EmitOptionalAssignment(emitter);
        }

        public static void EmitEmptyStatement(Emitter emitter, ITsEmptyStatement emptyStatement)
        {
            emitter.WriteLine(";");
        }

        public static void EmitExpressionStatement(Emitter emitter, ITsExpressionStatement expressionStatement)
        {
            emitter.Write(expressionStatement.Expression).WriteLine(";");
        }

        public static void EmitIfStatement(Emitter emitter, ITsIfStatement ifStatement)
        {
            emitter.Write("if (");
            ifStatement.IfCondition.Emit(emitter);
            ifStatement.IfStatement.EmitIndentedOrInBlock(
                emitter,
                newlineAfterBlock: ifStatement.ElseStatement == null);

            if (ifStatement.ElseStatement == null)
            {
                return;
            }

            if (ifStatement.IfStatement is ITsBlockStatement)
            {
                emitter.Write(" ");
            }

            emitter.Write("else");
            ifStatement.ElseStatement.EmitIndentedOrInBlock(
                emitter,
                prefixForIndentedStatement: "",
                prefixForBlock: " ",
                newlineAfterBlock: true);
        }

        public static void EmitDoWhileStatement(Emitter emitter, ITsDoWhileStatement doWhileStatement)
        {
            emitter.Write("do");
            doWhileStatement.DoStatement.EmitIndentedOrInBlock(
                emitter,
                prefixForIndentedStatement: "",
                prefixForBlock: " ");

            emitter.Write(doWhileStatement.DoStatement is ITsBlockStatement ? " while (" : "while (");
            doWhileStatement.WhileCondition.Emit(emitter);
            emitter.WriteLine(");");
        }

        public static void EmitWhileStatement(Emitter emitter, ITsWhileStatement whileStatement)
        {
            emitter.Write("while (").Write(whileStatement.WhileCondition);
            whileStatement.WhileStatement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }

        public static void EmitForStatement(Emitter emitter, ITsForStatement forStatement)
        {
            emitter.Write("for (");

            if (forStatement.Initializer != null)
            {
                forStatement.Initializer.Emit(emitter);
                emitter.Write("; ");
            }
            else if (forStatement.InitializerWithVariableDeclarations?.Length > 0)
            {
                emitter.Write("var ");
                emitter.WriteList(forStatement.InitializerWithVariableDeclarations, indent: false, itemDelimiter: ", ");
                emitter.Write("; ");
            }
            else
            {
                // Normally a lexical declaration ends in a newline, but we don't want that in our
                // for loop. This is kind of clunky, but we'll create a temporary emitter for it
                // to use with spaces instead of newlines.
                using var memoryStream = new MemoryStream();
                using var tempEmitter = new Emitter(memoryStream, emitter.Encoding, emitter.Options.WithNewline(" "));
                forStatement.InitializerWithLexicalDeclaration?.Emit(tempEmitter);
                emitter.Write(memoryStream.ReadAllText(emitter.Encoding));
            }

            forStatement.Condition?.Emit(emitter);
            emitter.Write("; ");

            forStatement.Incrementor?.Emit(emitter);

            forStatement.Statement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }

        public static void EmitForInStatement(Emitter emitter, ITsForInStatement forInStatement)
        {
            EmitForInOfStatement(
                emitter,
                ofLoop: false,
                forInStatement.Initializer,
                forInStatement.DeclarationKind,
                forInStatement.Declaration,
                forInStatement.RightSide,
                forInStatement.Statement);
        }

        public static void EmitForOfStatement(Emitter emitter, ITsForOfStatement forOfStatement)
        {
            EmitForInOfStatement(
                emitter,
                ofLoop: true,
                forOfStatement.Initializer,
                forOfStatement.DeclarationKind,
                forOfStatement.Declaration,
                forOfStatement.RightSide,
                forOfStatement.Statement);
        }

        private static void EmitForInOfStatement(
            Emitter emitter,
            bool ofLoop,
            ITsExpression? initializer,
            VariableDeclarationKind? declarationKind,
            ITsBindingIdentifierOrPattern? declaration,
            ITsExpression rightSide,
            ITsStatement statement)
        {
            emitter.Write("for (");

            if (initializer != null)
            {
                initializer.Emit(emitter);
            }
            else
            {
                declarationKind?.Emit(emitter);
                declaration?.Emit(emitter);
            }

            emitter.Write(ofLoop ? " of " : " in ");
            rightSide.Emit(emitter);

            statement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }

        public static void EmitContinueStatement(Emitter emitter, ITsContinueStatement continueStatement)
        {
            emitter.Write("continue");

            if (continueStatement.Label != null)
            {
                emitter.Write(" ");
                continueStatement.Label.Emit(emitter);
            }

            emitter.WriteLine(";");
        }

        public static void EmitBreakStatement(Emitter emitter, ITsBreakStatement breakStatement)
        {
            emitter.Write("break");

            if (breakStatement.Label != null)
            {
                emitter.Write(" ");
                breakStatement.Label.Emit(emitter);
            }

            emitter.WriteLine(";");
        }

        public static void EmitReturnStatement(Emitter emitter, ITsReturnStatement returnStatement)
        {
            emitter.Write("return");

            if (returnStatement.Expression != null)
            {
                emitter.Write(" ");
                returnStatement.Expression.Emit(emitter);
            }

            emitter.WriteLine(";");
        }

        public static void EmitWithStatement(Emitter emitter, ITsWithStatement withStatement)
        {
            emitter.Write("with (");
            withStatement.Expression.Emit(emitter);
            withStatement.Statement.EmitIndentedOrInBlock(emitter, newlineAfterBlock: true);
        }

        public static void EmitSwitchStatement(Emitter emitter, ITsSwitchStatement switchStatement)
        {
            emitter.Write("switch (");
            switchStatement.Condition.Emit(emitter);
            emitter.WriteLine(") {");

            emitter.IndentLevel++;
            for (int i = 0; i < switchStatement.Clauses.Length; i++)
            {
                ITsCaseOrDefaultClause clause = switchStatement.Clauses[i];
                clause.Emit(emitter);

                // Don't write newlines between empty clauses or after the last item
                if (!clause.Statements.IsEmpty && i < switchStatement.Clauses.Length - 1)
                {
                    emitter.WriteLineWithoutIndentation();
                }
            }

            emitter.IndentLevel--;
            emitter.WriteLine("}");
        }

        public static void EmitCaseClause(Emitter emitter, ITsCaseClause caseClause)
        {
            emitter.Write("case ").Write(caseClause.Expression).WriteLine(":");

            emitter.IndentLevel++;
            foreach (ITsStatementListItem statement in caseClause.Statements)
            {
                statement.Emit(emitter);
            }

            emitter.IndentLevel--;
        }

        public static void EmitDefaultClause(Emitter emitter, ITsDefaultClause defaultClause)
        {
            emitter.WriteLine("default:");

            emitter.IndentLevel++;
            foreach (ITsStatementListItem statement in defaultClause.Statements)
            {
                statement.Emit(emitter);
            }

            emitter.IndentLevel--;
        }

        public static void EmitLabeledStatement(Emitter emitter, ITsLabeledStatement labeledStatement)
        {
            int currentIndentLevel = emitter.IndentLevel;

            // write the label one indentation level out from the rest of the body
            emitter.IndentLevel = Math.Max(emitter.IndentLevel - 1, 0);
            labeledStatement.Label.Emit(emitter);
            emitter.WriteLine(":");
            emitter.IndentLevel = currentIndentLevel;

            labeledStatement.Statement?.Emit(emitter);
            labeledStatement.FunctionDeclaration?.Emit(emitter);
        }

        public static void EmitThrowStatement(Emitter emitter, ITsThrowStatement throwStatement)
        {
            emitter.Write("throw ");
            throwStatement.Expression.Emit(emitter);
            emitter.WriteLine(";");
        }

        public static void EmitTryStatement(Emitter emitter, ITsTryStatement tryStatement)
        {
            emitter.Write("try ");
            tryStatement.TryBlock.Emit(emitter);

            if (tryStatement.CatchBlock != null)
            {
                if (tryStatement.CatchParameter != null)
                {
                    emitter.Write(" catch (");
                    tryStatement.CatchParameter.Emit(emitter);
                    emitter.Write(") ");
                }
                else
                {
                    emitter.Write(" catch ");
                }

                tryStatement.CatchBlock.Emit(emitter);
            }

            if (tryStatement.FinallyBlock != null)
            {
                emitter.Write(" finally ");
                tryStatement.FinallyBlock.Emit(emitter);
            }

            emitter.WriteLine();
        }

        public static void EmitDebuggerStatement(Emitter emitter, ITsDebuggerStatement debuggerStatement)
        {
            emitter.WriteLine("debugger;");
        }

        public static void EmitArrowFunction(Emitter emitter, ITsArrowFunction arrowFunction)
        {
            if (arrowFunction.SingleParameterName != null)
            {
                arrowFunction.SingleParameterName.Emit(emitter);
            }
            else
            {
                arrowFunction.CallSignature?.Emit(emitter);
            }

            emitter.Write(" => ");

            if (arrowFunction.BodyExpression != null)
            {
                arrowFunction.BodyExpression.Emit(emitter);
            }
            else
            {
                emitter.WriteBlock(arrowFunction.Body!, skipNewlines: true);
            }
        }

        public static void EmitClassExpression(Emitter emitter, ITsClassExpression classExpression)
        {
            emitter.Write("class ");
            classExpression.ClassName?.Emit(emitter);
            classExpression.Heritage?.Emit(emitter);

            emitter.WriteBlock(classExpression.ClassBody);
        }

        public static void EmitImportDeclaration(Emitter emitter, ITsImportDeclaration importDeclaration)
        {
            emitter.Write("import ");

            if (importDeclaration.Module != null)
            {
                importDeclaration.Module.Emit(emitter);
            }
            else
            {
                importDeclaration.ImportClause?.Emit(emitter);
                emitter.Write(" ");
                importDeclaration.FromClause?.Emit(emitter);
            }

            emitter.WriteLine(";");
        }

        public static void EmitImportClause(Emitter emitter, ITsImportClause importClause)
        {
            if (importClause.DefaultBinding != null)
            {
                importClause.DefaultBinding.Emit(emitter);
                if (importClause.NamespaceBinding != null || !importClause.NamedImports.IsEmpty)
                {
                    emitter.Write(", ");
                }
            }

            if (importClause.NamespaceBinding != null)
            {
                emitter.Write("* as ");
                importClause.NamespaceBinding.Emit(emitter);
            }

            if (!importClause.NamedImports.IsEmpty)
            {
                emitter.WriteList(
                    importClause.NamedImports,
                    indent: false,
                    prefix: "{ ",
                    suffix: " }",
                    itemDelimiter: ", ");
            }
        }

        public static void EmitFromClause(Emitter emitter, ITsFromClause fromClause)
        {
            emitter.Write("from ");
            fromClause.Module.Emit(emitter);
        }

        public static void EmitImportSpecifier(Emitter emitter, ITsImportSpecifier importSpecifier)
        {
            importSpecifier.Name.Emit(emitter);
            if (importSpecifier.AsName != null)
            {
                emitter.Write(" as ");
                importSpecifier.AsName.Emit(emitter);
            }
        }

        public static void EmitTypeParameters(Emitter emitter, ITsTypeParameters typeParameters)
        {
            emitter.WriteList(
                typeParameters.TypeParameters,
                indent: false,
                prefix: "<",
                suffix: ">",
                itemDelimiter: ", ",
                emptyContents: "");
        }

        public static void EmitTypeParameter(Emitter emitter, ITsTypeParameter typeParameter)
        {
            typeParameter.TypeName.Emit(emitter);

            if (typeParameter.Constraint != null)
            {
                emitter.Write(" extends ");
                typeParameter.Constraint.Emit(emitter);
            }
        }

        public static void EmitParenthesizedType(Emitter emitter, ITsParenthesizedType parenthesizedType)
        {
            emitter.Write("(").Write(parenthesizedType.Type).Write(")");
        }

        public static void EmitTypeReference(Emitter emitter, ITsTypeReference typeReference)
        {
            typeReference.TypeName.Emit(emitter);

            if (typeReference.TypeArguments.Length > 0)
            {
                emitter.WriteList(
                    typeReference.TypeArguments,
                    indent: false,
                    prefix: "<",
                    suffix: ">",
                    itemDelimiter: ", ");
            }
        }

        public static void EmitQualifiedName(Emitter emitter, ITsQualifiedName qualifiedName)
        {
            foreach (ITsIdentifier left in qualifiedName.Left)
            {
                left.Emit(emitter);
                emitter.Write(".");
            }

            qualifiedName.Right.Emit(emitter);
        }

        public static void EmitGenericTypeName(Emitter emitter, ITsGenericTypeName genericTypeName)
        {
            EmitQualifiedName(emitter, genericTypeName);

            if (genericTypeName.TypeArguments.Length > 0)
            {
                emitter.Write("<");
                emitter.WriteCommaList(genericTypeName.TypeArguments);
                emitter.Write(">");
            }
        }

        public static void EmitObjectType(Emitter emitter, ITsObjectType objectType)
        {
            bool multiLine = !objectType.ForceSingleLine;
            emitter.WriteList(
                objectType.TypeMembers,
                indent: true,
                prefix: objectType.ForceSingleLine ? "{ " : "{",
                suffix: objectType.ForceSingleLine ? " }" : "}",
                itemDelimiter: ";" + emitter.Options.Newline,
                delimiterAfterLastItem: multiLine,
                newLineAfterPrefix: multiLine,
                newLineAfterLastItem: multiLine,
                emptyContents: "{}");
        }

        public static void EmitArrayType(Emitter emitter, ITsArrayType arrayType)
        {
            // We need to use the long-hand Array<> style if there are any spaces or other expression breaks. For
            // example, an array of function without the Array<> would be '(x: string) => boolean[]', which is a
            // function that returns a boolean array instead of an array of functions. Or a union type 'x | y[]', which
            // is a union type of 'x' and 'y[]' instead of an array of union types.
            //
            // To determine which syntax to use, we can emit the type to a temporary string and check for any spaces or
            // other problematic characters.
            using var memoryStream = new MemoryStream();
            using var tempEmitter = new Emitter(memoryStream, emitter.Encoding, emitter.Options);
            arrayType.Type.Emit(tempEmitter);
            string typeEmit = memoryStream.ReadAllText(emitter.Encoding);

            if (typeEmit.Any(char.IsWhiteSpace) || typeEmit.Any(c => c.IsOneOf('&', '|', ':', '=', '-', '+', ';')))
            {
                emitter.Write("Array<").Write(typeEmit).Write(">");
            }
            else
            {
                emitter.Write(typeEmit).Write("[]");
            }
        }

        public static void EmitTupleType(Emitter emitter, ITsTupleType tupleType)
        {
            emitter.WriteList(tupleType.ElementTypes, indent: false, prefix: "[", suffix: "]", itemDelimiter: ", ");
        }

        public static void EmitUnionType(Emitter emitter, ITsUnionType unionType)
        {
            emitter.WriteList(unionType.Types, indent: false, itemDelimiter: " | ");
        }

        public static void EmitIntersectionType(Emitter emitter, ITsIntersectionType intersectionType)
        {
            emitter.WriteList(intersectionType.Types, indent: false, itemDelimiter: " & ");
        }

        public static void EmitFunctionType(Emitter emitter, ITsFunctionType functionType)
        {
            EmitFunctionOrConstructorType(
                emitter,
                functionType.TypeParameters,
                functionType.Parameters,
                functionType.ReturnType,
                isConstructorType: false);
        }

        public static void EmitConstructorType(Emitter emitter, ITsConstructorType constructorType)
        {
            EmitFunctionOrConstructorType(
                emitter,
                constructorType.TypeParameters,
                constructorType.Parameters,
                constructorType.ReturnType,
                isConstructorType: true);
        }

        private static void EmitFunctionOrConstructorType(
            Emitter emitter,
            ITsTypeParameters? typeParameters,
            ITsParameterList? parameterList,
            ITsType returnType,
            bool isConstructorType)
        {
            if (isConstructorType)
            {
                emitter.Write("new ");
            }

            typeParameters?.Emit(emitter);
            emitter.Write("(");
            parameterList?.Emit(emitter);
            emitter.Write(")");

            emitter.Write(" => ");
            returnType.Emit(emitter);
        }

        public static void EmitTypeQuery(Emitter emitter, ITsTypeQuery typeQuery)
        {
            emitter.Write("typeof ").Write(typeQuery.Query);
        }

        public static void EmitThisType(Emitter emitter, ITsThisType thisType)
        {
            emitter.Write("this");
        }

        public static void EmitPropertySignature(Emitter emitter, ITsPropertySignature propertySignature)
        {
            if (propertySignature.IsReadOnly)
            {
                emitter.Write("readonly ");
            }

            propertySignature.PropertyName.Emit(emitter);
            if (propertySignature.IsOptional)
            {
                emitter.Write("?");
            }

            propertySignature.PropertyType?.EmitOptionalTypeAnnotation(emitter);
        }

        public static void EmitCallSignature(Emitter emitter, ITsCallSignature callSignature)
        {
            callSignature.TypeParameters?.Emit(emitter);
            emitter.Write("(");
            callSignature.Parameters?.Emit(emitter);
            emitter.Write(")");

            callSignature.ReturnType?.EmitOptionalTypeAnnotation(emitter);
        }

        public static void EmitParameterList(Emitter emitter, ITsParameterList parameterList)
        {
            if (parameterList.RequiredParameters.Length > 0)
            {
                emitter.WriteList(parameterList.RequiredParameters, indent: false, itemDelimiter: ", ");
            }

            if (parameterList.RequiredParameters.Length > 0 &&
                (parameterList.OptionalParameters.Length > 0 || parameterList.RestParameter != null))
            {
                emitter.Write(", ");
            }

            if (parameterList.OptionalParameters.Length > 0)
            {
                emitter.WriteList(parameterList.OptionalParameters, indent: false, itemDelimiter: ", ");
            }

            if (parameterList.OptionalParameters.Length > 0 && parameterList.RestParameter != null)
            {
                emitter.Write(", ");
            }

            parameterList.RestParameter?.Emit(emitter);
        }

        public static void EmitBoundRequiredParameter(Emitter emitter, ITsBoundRequiredParameter boundRequiredParameter)
        {
            boundRequiredParameter.Modifier.EmitOptional(emitter);

            boundRequiredParameter.ParameterName.Emit(emitter);
            boundRequiredParameter.ParameterType?.EmitOptionalTypeAnnotation(emitter);
        }

        public static void EmitBoundOptionalParameter(Emitter emitter, ITsBoundOptionalParameter optionalParameter)
        {
            optionalParameter.Modifier.EmitOptional(emitter);

            optionalParameter.ParameterName.Emit(emitter);
            if (optionalParameter.Initializer == null)
            {
                emitter.Write("?");
            }

            optionalParameter.ParameterType?.EmitOptionalTypeAnnotation(emitter);

            optionalParameter.Initializer?.EmitOptionalAssignment(emitter);
        }

        public static void EmitStringRequiredParameter(
            Emitter emitter,
            ITsStringRequiredParameter stringRequiredParameter)
        {
            emitter.Write(stringRequiredParameter.ParameterName)
                .Write(": ")
                .Write(stringRequiredParameter.StringLiteral);
        }

        public static void EmitStringOptionalParameter(
            Emitter emitter,
            ITsStringOptionalParameter stringOptionalParameter)
        {
            emitter.Write(stringOptionalParameter.ParameterName)
                .Write("?: ")
                .Write(stringOptionalParameter.StringLiteral);
        }

        public static void EmitRestParameter(Emitter emitter, ITsRestParameter restParameter)
        {
            emitter.Write("...");
            restParameter.ParameterName.Emit(emitter);
            restParameter.ParameterType?.EmitOptionalTypeAnnotation(emitter);
        }

        public static void EmitConstructSignature(Emitter emitter, ITsConstructSignature constructSignature)
        {
            emitter.Write("new ");
            constructSignature.TypeParameters?.Emit(emitter);
            emitter.Write("(");
            constructSignature.Parameters?.Emit(emitter);
            emitter.Write(")");

            constructSignature.ReturnType?.EmitOptionalTypeAnnotation(emitter);
        }

        public static void EmitIndexSignature(Emitter emitter, ITsIndexSignature indexSignature)
        {
            emitter.Write("[");
            indexSignature.ParameterName.Emit(emitter);
            emitter.Write(": ");
            emitter.Write(indexSignature.IsParameterNumberType ? "number" : "string");
            emitter.Write("]: ");
            indexSignature.ReturnType.Emit(emitter);
        }

        public static void EmitMethodSignature(Emitter emitter, ITsMethodSignature methodSignature)
        {
            emitter.Write(methodSignature.PropertyName)
                .WriteIf(methodSignature.IsOptional, "?")
                .Write(methodSignature.CallSignature);
        }

        public static void EmitTypeAliasDeclaration(Emitter emitter, ITsTypeAliasDeclaration typeAliasDeclaration)
        {
            emitter.Write("type ");
            typeAliasDeclaration.AliasName.Emit(emitter);
            typeAliasDeclaration.TypeParameters?.Emit(emitter);
            emitter.Write(" = ");
            typeAliasDeclaration.Type.Emit(emitter);
            emitter.WriteLine(";");
        }
    }
}
