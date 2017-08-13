// ---------------------------------------------------------------------------------------------------------------------
// <copyright file="TypeScriptEmitterTests.SourceFile.cs" company="Justin Rockwood">
//   Copyright (c) Justin Rockwood. All Rights Reserved. Licensed under the Apache License, Version 2.0. See
//   LICENSE.txt in the project root for license information.
// </copyright>
// ---------------------------------------------------------------------------------------------------------------------

namespace Desalt.TypeScript.Tests.Ast
{
    using Desalt.Core.Extensions;
    using Desalt.TypeScript.Ast;
    using Desalt.TypeScript.Ast.Expressions;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Factory = Desalt.TypeScript.Ast.TsAstFactory;

    public partial class TsEmitTests
    {
        [TestMethod]
        public void Emit_an_ImplementationScript()
        {
            // ReSharper disable InconsistentNaming
            var Animal = Factory.Identifier("Animal");
            var animal = Factory.Identifier("animal");
            var AnimalRef = Factory.TypeReference(Animal);
            var feed = Factory.Identifier("feed");
            var Food = Factory.Identifier("Food");
            var food = Factory.Identifier("food");
            var FoodRef = Factory.TypeReference(Food);
            var isFull = Factory.Identifier("isFull");
            var Monkey = Factory.Identifier("Monkey");
            var name = Factory.Identifier("name");
            // ReSharper restore InconsistentNaming

            VerifyOutput(
                Factory.ImplementationScript(
                    Factory.NamespaceDeclaration(
                        Factory.QualifiedName("Zoo"),
                        Factory.ExportedDeclaration(
                            Factory.InterfaceDeclaration(
                                Animal,
                                Factory.ObjectType(
                                    Factory.PropertySignature(name, Factory.StringType),
                                    Factory.MethodSignature(
                                        isFull,
                                        isOptional: false,
                                        callSignature: Factory.CallSignature(null, Factory.BooleanType)),
                                    Factory.MethodSignature(
                                        feed,
                                        isOptional: false,
                                        callSignature: Factory.CallSignature(
                                            Factory.ParameterList(Factory.BoundRequiredParameter(food, FoodRef)),
                                            Factory.VoidType)))))),
                        Factory.TypeAliasDeclaration(Factory.Identifier("Ape"), Factory.TypeReference(Monkey)),
                        Factory.ImportAliasDeclaration(Animal, Factory.QualifiedName("Zoo", "Animal")),
                        Factory.LexicalDeclaration(
                            true,
                            Factory.SimpleLexicalBinding(
                                Factory.Identifier("animals"),
                                Factory.ArrayType(AnimalRef),
                                Factory.Array((ITsExpression[])null))),
                        Factory.FunctionDeclaration(
                            Factory.Identifier("addAnimal"),
                            Factory.CallSignature(
                                Factory.ParameterList(
                                    Factory.BoundRequiredParameter(animal, AnimalRef)),
                                Factory.VoidType),
                            Factory.Call(
                                Factory.QualifiedName("animals.push"), Factory.ArgumentList(animal)).ToStatement()),
                        Factory.EnumDeclaration(
                            Food,
                            Factory.EnumMember(Factory.Identifier("Banana")),
                            Factory.EnumMember(Factory.Identifier("Meat"))),
                        Factory.ClassDeclaration(
                            Monkey,
                            heritage: Factory.ClassHeritage(implementsTypes: AnimalRef.ToSafeArray()),
                            classBody: new ITsClassElement[]
                            {
                                Factory.VariableMemberDeclaration(
                                    Factory.Identifier("_isFull"),
                                    TsAccessibilityModifier.Private,
                                    typeAnnotation: Factory.BooleanType),
                                Factory.VariableMemberDeclaration(
                                    name,
                                    TsAccessibilityModifier.Public,
                                    typeAnnotation: Factory.StringType),
                                Factory.FunctionMemberDeclaration(
                                    isFull,
                                    Factory.CallSignature(Factory.ParameterList(), Factory.BooleanType),
                                    TsAccessibilityModifier.Public,
                                    functionBody: new ITsStatementListItem[]
                                    {
                                        Factory.Return(Factory.MemberDot(Factory.This, "_isFull"))
                                    }),
                                Factory.FunctionMemberDeclaration(
                                    feed,
                                    Factory.CallSignature(
                                        Factory.ParameterList(Factory.BoundRequiredParameter(food, FoodRef)),
                                        Factory.VoidType),
                                    TsAccessibilityModifier.Public,
                                    functionBody: new ITsStatementListItem[]
                                    {
                                        Factory.Assignment(
                                            Factory.MemberDot(Factory.This, "_isFull"),
                                            TsAssignmentOperator.SimpleAssign,
                                            Factory.BinaryExpression(
                                                food,
                                                TsBinaryOperator.StrictEquals,
                                                Factory.MemberDot(Food, "Banana"))).ToStatement()
                                    })
                            })
                    ),
                @"namespace Zoo {
  export interface Animal {
    name: string;
    isFull(): boolean;
    feed(food: Food): void;
  }
}

type Ape = Monkey;

import Animal = Zoo.Animal;

const animals: Animal[] = [];

function addAnimal(animal: Animal): void {
  animals.push(animal);
}

enum Food {
  Banana,
  Meat,
}

class Monkey implements Animal {
  private _isFull: boolean;

  public name: string;

  public isFull(): boolean {
    return this._isFull;
  }

  public feed(food: Food): void {
    this._isFull = food === Food.Banana;
  }
}
".Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void Emit_an_ImplementationModule()
        {
            Assert.Inconclusive("Not implemented yet");
        }
    }
}
