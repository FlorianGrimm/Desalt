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
            // ReSharper disable InconsistentNaming
            var existsSync = Factory.Identifier("existsSync");
            var fileKey = Factory.Identifier("fileKey");
            var fileMapping = Factory.Identifier("fileMapping");
            var fileName = Factory.Identifier("fileName");
            var filePath = Factory.Identifier("filePath");
            var FileSystem = Factory.Identifier("FileSystem");
            var FileSystemRef = Factory.TypeReference(FileSystem);
            var fsExtra = Factory.Identifier("fsExtra");
            var MockFileSystemEntry = Factory.Identifier("MockFileSystemEntry");
            var MockFileSystemTree = Factory.Identifier("MockFileSystemTree");
            var MockFileSystemTreeRef = Factory.TypeReference(MockFileSystemTree);
            var MockFileSystemWriteFileCallback = Factory.Identifier("MockFileSystemWriteFileCallback");
            var ObjectKeys = Factory.MemberDot(Factory.Identifier("Object"), "keys");
            var path = Factory.Identifier("path");
            var this_fileMapping = Factory.MemberDot(Factory.This, "fileMapping");
            // ReSharper restore InconsistentNaming

            VerifyOutput(
                Factory.ImplementationModule(
                    Factory.ImportDeclaration(
                        Factory.ImportClauseNamespaceBinding(fsExtra),
                        Factory.FromClause(Factory.String("fs-extra"))),
                    Factory.ImportRequireDeclaration(path, Factory.String("path")),
                    Factory.FunctionDeclaration(
                        Factory.Identifier("makeRelativeToCurrentWorkingDirectory"),
                        Factory.CallSignature(
                            Factory.ParameterList(
                                Factory.BoundRequiredParameter(filePath, Factory.StringType)),
                            Factory.StringType),
                        Factory.Return(
                            Factory.Call(
                                Factory.MemberDot(path, "relative"),
                                Factory.ArgumentList(
                                    Factory.Argument(
                                        Factory.Call(Factory.MemberDot(Factory.Identifier("process"), "cwd"))),
                                    Factory.Argument(filePath))))),
                    Factory.ExportImplementationElement(
                        Factory.InterfaceDeclaration(
                            FileSystem,
                            Factory.ObjectType(
                                Factory.MethodSignature(
                                    existsSync,
                                    isOptional: false,
                                    callSignature: Factory.CallSignature(
                                        Factory.ParameterList(
                                            Factory.BoundRequiredParameter(filePath, Factory.StringType)),
                                        Factory.BooleanType))))),
                    Factory.ExportImplementationElement(
                        Factory.LexicalDeclaration(
                            isConst: true,
                            declarations: new ITsLexicalBinding[]
                            {
                                Factory.SimpleLexicalBinding(
                                    Factory.Identifier("OsFileSystem"),
                                    FileSystemRef,
                                    Factory.Object(
                                        Factory.PropertyAssignment(
                                            existsSync,
                                            Factory.ArrowFunction(
                                                Factory.CallSignature(
                                                    Factory.ParameterList(
                                                        Factory.BoundRequiredParameter(s_p, Factory.StringType))),
                                                Factory.Call(
                                                    Factory.MemberDot(fsExtra, "existsSync"),
                                                    Factory.ArgumentList(s_p))))))
                            })),
                    Factory.ExportImplementationElement(
                        Factory.TypeAliasDeclaration(
                            MockFileSystemWriteFileCallback,
                            Factory.FunctionType(
                                Factory.ParameterList(
                                    Factory.BoundRequiredParameter(Factory.Identifier("contents"), Factory.StringType)),
                                Factory.VoidType))),
                    Factory.ExportImplementationElement(
                        Factory.TypeAliasDeclaration(
                            MockFileSystemEntry,
                            Factory.UnionType(
                                Factory.StringType,
                                Factory.TypeReference(Factory.Identifier("Object")),
                                Factory.TypeReference(MockFileSystemWriteFileCallback)))),
                    Factory.ExportImplementationElement(
                        Factory.TypeAliasDeclaration(
                            MockFileSystemTree,
                            Factory.ObjectType(
                                Factory.IndexSignature(
                                    Factory.Identifier("key"),
                                    isParameterNumberType: false,
                                    returnType: Factory.TypeReference(MockFileSystemEntry))))),
                    Factory.ExportImplementationElement(
                        Factory.ClassDeclaration(
                            Factory.Identifier("MockFileSystem"),
                            heritage: Factory.ClassHeritage(implementsTypes: FileSystemRef.ToSafeArray()),
                            classBody: new ITsClassElement[]
                            {
                                Factory.VariableMemberDeclaration(
                                    fileMapping,
                                    TsAccessibilityModifier.Private,
                                    typeAnnotation: MockFileSystemTreeRef,
                                    initializer: Factory.EmptyObject),
                                Factory.ConstructorDeclaration(
                                    TsAccessibilityModifier.Public,
                                    Factory.ParameterList(
                                        optionalParameters: Factory.BoundOptionalParameter(
                                            fileMapping,
                                            parameterType: MockFileSystemTreeRef,
                                            initializer: Factory.EmptyObject).ToSafeArray()),
                                    functionBody: new ITsStatementListItem[]
                                    {
                                        Factory.Call(
                                            Factory.MemberDot(
                                                Factory.Call(ObjectKeys, Factory.ArgumentList(fileMapping)),
                                                "forEach"),
                                            Factory.ArgumentList(
                                                Factory.Argument(
                                                    Factory.ArrowFunction(
                                                        fileKey,
                                                        body: new ITsStatementListItem[]
                                                        {
                                                            Factory.Call(
                                                                Factory.MemberDot(Factory.This, "addFileContents"),
                                                                Factory.ArgumentList(
                                                                    Factory.Argument(fileKey),
                                                                    Factory.Argument(
                                                                        Factory.MemberBracket(fileMapping, fileKey))))
                                                            .ToStatement()
                                                        }))))
                                        .ToStatement()
                                    }),
                                Factory.GetAccessorMemberDeclaration(
                                    Factory.GetAccessor(
                                        Factory.Identifier("mappingKeys"),
                                        Factory.ArrayType(Factory.StringType),
                                        Factory.Return(
                                            Factory.Call(
                                                ObjectKeys,
                                                Factory.ArgumentList(Factory.Argument(this_fileMapping))))),
                                    TsAccessibilityModifier.Public),
                                Factory.FunctionMemberDeclaration(
                                    existsSync,
                                    Factory.CallSignature(
                                        Factory.ParameterList(
                                            Factory.BoundRequiredParameter(filePath, Factory.StringType)),
                                        Factory.BooleanType),
                                    TsAccessibilityModifier.Public,
                                    functionBody: new ITsStatementListItem[]
                                    {
                                        Factory.LexicalDeclaration(
                                            isConst: true,
                                            declarations: new ITsLexicalBinding[]
                                            {
                                                Factory.SimpleLexicalBinding(
                                                    fileName,
                                                    Factory.StringType,
                                                    Factory.Call(
                                                        Factory.Identifier("makeRelativeToCurrentWorkingDirectory"),
                                                        Factory.ArgumentList(filePath)))
                                            }),
                                        Factory.Return(
                                            Factory.BinaryExpression(
                                                Factory.Call(
                                                    Factory.MemberDot(
                                                        Factory.Call(
                                                            ObjectKeys,
                                                            Factory.ArgumentList(Factory.Argument(this_fileMapping))),
                                                        "indexOf"),
                                                    Factory.ArgumentList(fileName)),
                                                TsBinaryOperator.GreaterThanEqual,
                                                Factory.Zero))
                                    })
                            }))),
                @"import * as fsExtra from 'fs-extra';

import path = require('path');

function makeRelativeToCurrentWorkingDirectory(filePath: string): string {
  return path.relative(process.cwd(), filePath);
}

export interface FileSystem {
  existsSync(filePath: string): boolean;
}

export const OsFileSystem: FileSystem = {
  existsSync: (p: string) => fsExtra.existsSync(p)
};

export type MockFileSystemWriteFileCallback = (contents: string) => void;

export type MockFileSystemEntry = string | Object | MockFileSystemWriteFileCallback;

export type MockFileSystemTree = {
  [key: string]: MockFileSystemEntry;
};

export class MockFileSystem implements FileSystem {
  private fileMapping: MockFileSystemTree = {};

  public constructor(fileMapping: MockFileSystemTree = {}) {
    Object.keys(fileMapping).forEach(fileKey => {
      this.addFileContents(fileKey, fileMapping[fileKey]);
    });
  }

  public get mappingKeys(): string[] {
    return Object.keys(this.fileMapping);
  }

  public existsSync(filePath: string): boolean {
    const fileName: string = makeRelativeToCurrentWorkingDirectory(filePath);
    return Object.keys(this.fileMapping).indexOf(fileName) >= 0;
  }
}
".Replace("\r\n", "\n"));
        }
    }
}
