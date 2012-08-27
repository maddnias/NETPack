using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;
using Mono.Cecil.Cil;

namespace NETPack.Core.Engine.Utils
{
    public static class CecilHelper
    {
        public static void RefreshTokens(ModuleDefinition mod)
        {
            int t = 1;
            int f = 1;
            int m = 1;
            foreach (TypeDefinition type in mod.Types)
            {
                RefreshType(ref t, ref f, ref m, type);
            }
        }

        private static void RefreshType(ref int type, ref int fld, ref int mtd, TypeDefinition typeDef)
        {
            typeDef.MetadataToken = new MetadataToken(TokenType.TypeDef, type);
            type++;
            foreach (FieldDefinition fldDef in typeDef.Fields)
            {
                fldDef.MetadataToken = new MetadataToken(TokenType.Field, fld);
                fld++;
            }
            foreach (MethodDefinition mtdDef in typeDef.Methods)
            {
                mtdDef.MetadataToken = new MetadataToken(TokenType.Method, mtd);
                mtd++;
            }
            foreach (TypeDefinition nestedDef in typeDef.NestedTypes)
            {
                RefreshType(ref type, ref fld, ref mtd, nestedDef);
            }
        }

        private static TypeReference ImportType(TypeReference typeRef, ModuleDefinition mod,
                                                MethodReference context,
                                                Dictionary<MetadataToken, IMemberDefinition> mems)
        {
            TypeReference ret = typeRef;
            if (typeRef is TypeSpecification)
            {
                if (typeRef is ArrayType)
                {
                    ArrayType _spec = typeRef as ArrayType;
                    ret = new ArrayType(ImportType(_spec.ElementType, mod, context, mems));
                    (ret as ArrayType).Dimensions.Clear();
                    foreach (var i in _spec.Dimensions) (ret as ArrayType).Dimensions.Add(i);
                }
                else if (typeRef is GenericInstanceType)
                {
                    GenericInstanceType _spec = typeRef as GenericInstanceType;
                    ret = new GenericInstanceType(ImportType(_spec.ElementType, mod, context, mems));
                    foreach (var i in _spec.GenericArguments)
                        (ret as GenericInstanceType).GenericArguments.Add(ImportType(i, mod, context, mems));
                }
                else if (typeRef is OptionalModifierType)
                {
                    ret =
                        new OptionalModifierType(
                            ImportType((typeRef as OptionalModifierType).ModifierType, mod, context, mems),
                            ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
                }
                else if (typeRef is RequiredModifierType)
                {
                    ret =
                        new RequiredModifierType(
                            ImportType((typeRef as RequiredModifierType).ModifierType, mod, context, mems),
                            ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
                }
                else if (typeRef is ByReferenceType)
                    ret = new ByReferenceType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
                else if (typeRef is PointerType)
                    ret = new PointerType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
                else if (typeRef is PinnedType)
                    ret = new PinnedType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
                else if (typeRef is SentinelType)
                    ret = new SentinelType(ImportType((typeRef as TypeSpecification).ElementType, mod, context, mems));
                else
                    throw new NotSupportedException();
            }
            else if (typeRef is GenericParameter)
            {
                if (context == null || (typeRef as GenericParameter).Owner is TypeReference ||
                    (typeRef as GenericParameter).Position >= context.GenericParameters.Count)
                    return typeRef;
                return context.GenericParameters[(typeRef as GenericParameter).Position];
            }
            else
            {
                if (mems != null && mems.ContainsKey(typeRef.MetadataToken))
                    ret = mems[typeRef.MetadataToken] as TypeReference;
                else if (!(ret is TypeDefinition) && typeRef.Scope.Name != "Confuser.Core.Injections.dll")
                    ret = mod.Import(ret);
            }
            return ret;
        }

        private static MethodReference ImportMethod(MethodReference mtdRef, ModuleDefinition mod,
                                                    MethodReference context,
                                                    Dictionary<MetadataToken, IMemberDefinition> mems)
        {
            MethodReference ret = mtdRef;
            if (mtdRef is GenericInstanceMethod)
            {
                GenericInstanceMethod _spec = mtdRef as GenericInstanceMethod;
                ret = new GenericInstanceMethod(ImportMethod(_spec.ElementMethod, mod, context, mems));
                foreach (var i in _spec.GenericArguments)
                    (ret as GenericInstanceMethod).GenericArguments.Add(ImportType(i, mod, context, mems));

                ret.ReturnType = ImportType(ret.ReturnType, mod, ret, mems);
                foreach (var i in ret.Parameters)
                    i.ParameterType = ImportType(i.ParameterType, mod, ret, mems);
            }
            else
            {
                if (mems != null && mems.ContainsKey(mtdRef.MetadataToken))
                    ret = mems[mtdRef.MetadataToken] as MethodReference;
                else if (mtdRef.DeclaringType.Scope.Name != "Confuser.Core.Injections.dll")
                {
                    ret = mod.Import(ret);
                    ret.ReturnType = ImportType(ret.ReturnType, mod, ret, mems);
                    foreach (var i in ret.Parameters)
                        i.ParameterType = ImportType(i.ParameterType, mod, ret, mems);
                }
            }
            if (!(mtdRef is MethodDefinition) && !(mtdRef is MethodSpecification))
                ret.DeclaringType = ImportType(mtdRef.DeclaringType, mod, context, mems);
            return ret;
        }

        private static FieldReference ImportField(FieldReference fldRef, ModuleDefinition mod,
                                                  Dictionary<MetadataToken, IMemberDefinition> mems)
        {
            if (mems != null && mems.ContainsKey(fldRef.MetadataToken))
                return mems[fldRef.MetadataToken] as FieldReference;
            else if (fldRef.DeclaringType.Scope.Name != "Confuser.Core.Injections.dll")
                return mod.Import(fldRef);
            else
                return fldRef;
        }

        public static TypeDefinition Inject(ModuleDefinition mod, TypeDefinition type)
        {
            type.Module.FullLoad();
            var dict = new Dictionary<MetadataToken, IMemberDefinition>();
            TypeDefinition ret = _Inject(mod, type, dict);
            PopulateDatas(mod, type, dict);
            return ret;
        }

        private static TypeDefinition _Inject(ModuleDefinition mod, TypeDefinition type,
                                              Dictionary<MetadataToken, IMemberDefinition> mems)
        {
            TypeDefinition ret = new TypeDefinition(type.Namespace, type.Name, type.Attributes);
            ret.Scope = mod;
            ret.ClassSize = type.ClassSize;
            ret.PackingSize = type.PackingSize;

            if (type.BaseType != null)
                ret.BaseType = mod.Import(type.BaseType);

            mems.Add(type.MetadataToken, ret);
            foreach (TypeDefinition ty in type.NestedTypes)
            {
                TypeDefinition t = _Inject(mod, ty, mems);
                ret.NestedTypes.Add(t);
            }
            foreach (FieldDefinition fld in type.Fields)
            {
                if (fld.IsLiteral) continue;
                FieldDefinition n = new FieldDefinition(fld.Name, fld.Attributes, mod.TypeSystem.Void);
                mems.Add(fld.MetadataToken, n);
                ret.Fields.Add(n);
            }
            foreach (MethodDefinition mtd in type.Methods)
            {
                MethodDefinition n = new MethodDefinition(mtd.Name, mtd.Attributes, mtd.ReturnType);
                mems.Add(mtd.MetadataToken, n);
                ret.Methods.Add(n);
            }

            return ret;
        }

        private static void PopulateDatas(ModuleDefinition mod, TypeDefinition type,
                                          Dictionary<MetadataToken, IMemberDefinition> mems)
        {
            TypeDefinition newType = mems[type.MetadataToken] as TypeDefinition;

            if (type.BaseType != null)
                newType.BaseType = ImportType(type.BaseType, mod, null, mems);

            foreach (TypeDefinition ty in type.NestedTypes)
            {
                PopulateDatas(mod, ty, mems);
            }
            foreach (FieldDefinition fld in type.Fields)
            {
                if (fld.IsLiteral) continue;
                (mems[fld.MetadataToken] as FieldDefinition).FieldType = ImportType(fld.FieldType, mod, null, mems);
            }
            foreach (MethodDefinition mtd in type.Methods)
            {
                PopulateMethod(mod, mtd, mems[mtd.MetadataToken] as MethodDefinition, mems);
            }
        }

        public static void PopulateMethod(ModuleDefinition mod, MethodDefinition mtd, MethodDefinition newMtd,
                                          Dictionary<MetadataToken, IMemberDefinition> mems)
        {
            mtd.Module.FullLoad();

            newMtd.Attributes = mtd.Attributes;
            newMtd.ImplAttributes = mtd.ImplAttributes;
            if (mtd.IsPInvokeImpl)
            {
                newMtd.PInvokeInfo = mtd.PInvokeInfo;
                bool has = false;
                foreach (ModuleReference modRef in mod.ModuleReferences)
                    if (modRef.Name == newMtd.PInvokeInfo.Module.Name)
                    {
                        has = true;
                        newMtd.PInvokeInfo.Module = modRef;
                        break;
                    }
                if (!has)
                    mod.ModuleReferences.Add(newMtd.PInvokeInfo.Module);
            }
            if (mtd.HasCustomAttributes)
            {
                foreach (CustomAttribute attr in mtd.CustomAttributes)
                {
                    CustomAttribute nAttr = new CustomAttribute(ImportMethod(attr.Constructor, mod, newMtd, mems),
                                                                attr.GetBlob());
                    newMtd.CustomAttributes.Add(nAttr);
                }
            }

            foreach (GenericParameter param in mtd.GenericParameters)
            {
                var p = new GenericParameter(param.Name, newMtd);
                if (param.HasCustomAttributes)
                {
                    foreach (CustomAttribute attr in param.CustomAttributes)
                    {
                        CustomAttribute nAttr = new CustomAttribute(mod.Import(attr.Constructor), attr.GetBlob());
                        p.CustomAttributes.Add(nAttr);
                    }
                }
                newMtd.GenericParameters.Add(p);
            }

            newMtd.ReturnType = ImportType(mtd.ReturnType, mod, newMtd, mems);

            foreach (ParameterDefinition param in mtd.Parameters)
            {
                var p = new ParameterDefinition(param.Name, param.Attributes,
                                                ImportType(param.ParameterType, mod, newMtd, mems));
                if (param.HasCustomAttributes)
                {
                    foreach (CustomAttribute attr in param.CustomAttributes)
                    {
                        CustomAttribute nAttr = new CustomAttribute(ImportMethod(attr.Constructor, mod, newMtd, mems),
                                                                    attr.GetBlob());
                        p.CustomAttributes.Add(nAttr);
                    }
                }
                newMtd.Parameters.Add(p);
            }

            if (mtd.HasBody)
            {
                MethodBody old = mtd.Body;
                MethodBody bdy = new MethodBody(newMtd);
                bdy.MaxStackSize = old.MaxStackSize;
                bdy.InitLocals = old.InitLocals;

                ILProcessor psr = bdy.GetILProcessor();

                foreach (VariableDefinition var in old.Variables)
                    bdy.Variables.Add(new VariableDefinition(var.Name, ImportType(var.VariableType, mod, newMtd, mems)));

                foreach (Instruction inst in old.Instructions)
                {
                    switch (inst.OpCode.OperandType)
                    {
                        case OperandType.InlineArg:
                        case OperandType.ShortInlineArg:
                            if (inst.Operand == old.ThisParameter)
                                psr.Emit(inst.OpCode, bdy.ThisParameter);
                            else
                            {
                                int param = mtd.Parameters.IndexOf(inst.Operand as ParameterDefinition);
                                psr.Emit(inst.OpCode, newMtd.Parameters[param]);
                            }
                            break;
                        case OperandType.InlineVar:
                        case OperandType.ShortInlineVar:
                            int var = old.Variables.IndexOf(inst.Operand as VariableDefinition);
                            psr.Emit(inst.OpCode, bdy.Variables[var]);
                            break;
                        case OperandType.InlineField:
                            psr.Emit(inst.OpCode, ImportField(inst.Operand as FieldReference, mod, mems));
                            break;
                        case OperandType.InlineMethod:
                            psr.Emit(inst.OpCode, ImportMethod(inst.Operand as MethodReference, mod, newMtd, mems));
                            break;
                        case OperandType.InlineType:
                            psr.Emit(inst.OpCode, ImportType(inst.Operand as TypeReference, mod, newMtd, mems));
                            break;
                        case OperandType.InlineTok:
                            if (inst.Operand is FieldReference)
                                psr.Emit(inst.OpCode, ImportField(inst.Operand as FieldReference, mod, mems));
                            else if (inst.Operand is MethodReference)
                                psr.Emit(inst.OpCode, ImportMethod(inst.Operand as MethodReference, mod, newMtd, mems));
                            else if (inst.Operand is TypeReference)
                                psr.Emit(inst.OpCode, ImportType(inst.Operand as TypeReference, mod, newMtd, mems));
                            break;
                        default:
                            psr.Append(inst);
                            break;
                    }
                }

                for (int i = 0; i < bdy.Instructions.Count; i++)
                {
                    Instruction inst = bdy.Instructions[i];
                    Instruction o = old.Instructions[i];

                    if (inst.OpCode.OperandType == OperandType.InlineSwitch)
                    {
                        Instruction[] olds = (Instruction[]) o.Operand;
                        Instruction[] news = new Instruction[olds.Length];

                        for (int ii = 0; ii < news.Length; ii++)
                            news[ii] = bdy.Instructions[old.Instructions.IndexOf(olds[ii])];

                        inst.Operand = news;
                    }
                    else if (inst.OpCode.OperandType == OperandType.ShortInlineBrTarget ||
                             inst.OpCode.OperandType == OperandType.InlineBrTarget)
                        inst.Operand = bdy.Instructions[old.Instructions.IndexOf(inst.Operand as Instruction)];
                }

                foreach (ExceptionHandler eh in old.ExceptionHandlers)
                {
                    ExceptionHandler neh = new ExceptionHandler(eh.HandlerType);
                    if (old.Instructions.IndexOf(eh.TryStart) != -1)
                        neh.TryStart = bdy.Instructions[old.Instructions.IndexOf(eh.TryStart)];
                    if (old.Instructions.IndexOf(eh.TryEnd) != -1)
                        neh.TryEnd = bdy.Instructions[old.Instructions.IndexOf(eh.TryEnd)];
                    if (old.Instructions.IndexOf(eh.HandlerStart) != -1)
                        neh.HandlerStart = bdy.Instructions[old.Instructions.IndexOf(eh.HandlerStart)];
                    if (old.Instructions.IndexOf(eh.HandlerEnd) != -1)
                        neh.HandlerEnd = bdy.Instructions[old.Instructions.IndexOf(eh.HandlerEnd)];

                    switch (eh.HandlerType)
                    {
                        case ExceptionHandlerType.Catch:
                            neh.CatchType = ImportType(eh.CatchType, mod, newMtd, mems);
                            break;
                        case ExceptionHandlerType.Filter:
                            neh.FilterStart = bdy.Instructions[old.Instructions.IndexOf(eh.FilterStart)];
                            //neh.FilterEnd = bdy.Instructions[old.Instructions.IndexOf(eh.FilterEnd)];
                            break;
                    }

                    bdy.ExceptionHandlers.Add(neh);
                }

                newMtd.Body = bdy;
            }
        }

        public static MethodDefinition Inject(ModuleDefinition mod, MethodDefinition mtd)
        {
            MethodDefinition ret = new MethodDefinition(mtd.Name, mtd.Attributes, mod.TypeSystem.Void);
            ret.Attributes = mtd.Attributes;
            ret.ImplAttributes = mtd.ImplAttributes;

            if (mtd.IsPInvokeImpl)
            {
                ret.PInvokeInfo = mtd.PInvokeInfo;
                bool has = false;
                foreach (ModuleReference modRef in mod.ModuleReferences)
                    if (modRef.Name == ret.PInvokeInfo.Module.Name)
                    {
                        has = true;
                        ret.PInvokeInfo.Module = modRef;
                        break;
                    }
                if (!has)
                    mod.ModuleReferences.Add(ret.PInvokeInfo.Module);
            }
            if (mtd.HasCustomAttributes)
            {
                foreach (CustomAttribute attr in mtd.CustomAttributes)
                {
                    CustomAttribute nAttr = new CustomAttribute(ImportMethod(attr.Constructor, mod, ret, null),
                                                                attr.GetBlob());
                    ret.CustomAttributes.Add(nAttr);
                }
            }

            foreach (GenericParameter param in mtd.GenericParameters)
            {
                var p = new GenericParameter(param.Name, ret);
                if (param.HasCustomAttributes)
                {
                    foreach (CustomAttribute attr in param.CustomAttributes)
                    {
                        CustomAttribute nAttr = new CustomAttribute(ImportMethod(attr.Constructor, mod, ret, null),
                                                                    attr.GetBlob());
                        p.CustomAttributes.Add(nAttr);
                    }
                }
                ret.GenericParameters.Add(p);
            }

            ret.ReturnType = ImportType(mtd.ReturnType, mod, ret, null);

            foreach (ParameterDefinition param in mtd.Parameters)
            {
                var p = new ParameterDefinition(param.Name, param.Attributes,
                                                ImportType(param.ParameterType, mod, ret, null));
                if (param.HasCustomAttributes)
                {
                    foreach (CustomAttribute attr in param.CustomAttributes)
                    {
                        CustomAttribute nAttr = new CustomAttribute(ImportMethod(attr.Constructor, mod, ret, null),
                                                                    attr.GetBlob());
                        p.CustomAttributes.Add(nAttr);
                    }
                }
                ret.Parameters.Add(p);
            }

            if (mtd.HasBody)
            {
                MethodBody old = mtd.Body;
                MethodBody bdy = new MethodBody(ret);
                bdy.MaxStackSize = old.MaxStackSize;
                bdy.InitLocals = old.InitLocals;

                ILProcessor psr = bdy.GetILProcessor();

                foreach (VariableDefinition var in old.Variables)
                    bdy.Variables.Add(new VariableDefinition(var.Name, ImportType(var.VariableType, mod, ret, null)));

                foreach (Instruction inst in old.Instructions)
                {
                    switch (inst.OpCode.OperandType)
                    {
                        case OperandType.InlineArg:
                        case OperandType.ShortInlineArg:
                            if (inst.Operand == old.ThisParameter)
                                psr.Emit(inst.OpCode, bdy.ThisParameter);
                            else
                            {
                                int param = mtd.Parameters.IndexOf(inst.Operand as ParameterDefinition);
                                psr.Emit(inst.OpCode, ret.Parameters[param]);
                            }
                            break;
                        case OperandType.InlineVar:
                        case OperandType.ShortInlineVar:
                            int var = old.Variables.IndexOf(inst.Operand as VariableDefinition);
                            psr.Emit(inst.OpCode, bdy.Variables[var]);
                            break;
                        case OperandType.InlineField:
                            psr.Emit(inst.OpCode, ImportField(inst.Operand as FieldReference, mod, null));
                            break;
                        case OperandType.InlineMethod:
                            if (inst.Operand == mtd)
                                psr.Emit(inst.OpCode, ret);
                            else
                                psr.Emit(inst.OpCode, ImportMethod(inst.Operand as MethodReference, mod, ret, null));
                            break;
                        case OperandType.InlineType:
                            psr.Emit(inst.OpCode, ImportType(inst.Operand as TypeReference, mod, ret, null));
                            break;
                        case OperandType.InlineTok:
                            if (inst.Operand is TypeReference)
                                psr.Emit(inst.OpCode, ImportType(inst.Operand as TypeReference, mod, ret, null));
                            else if (inst.Operand is FieldReference)
                                psr.Emit(inst.OpCode, ImportField(inst.Operand as FieldReference, mod, null));
                            else if (inst.Operand is MethodReference)
                                psr.Emit(inst.OpCode, ImportMethod(inst.Operand as MethodReference, mod, ret, null));
                            break;
                        default:
                            psr.Append(inst.Clone());
                            break;
                    }
                }

                for (int i = 0; i < bdy.Instructions.Count; i++)
                {
                    Instruction inst = bdy.Instructions[i];
                    Instruction o = old.Instructions[i];

                    if (inst.OpCode.OperandType == OperandType.InlineSwitch)
                    {
                        Instruction[] olds = (Instruction[]) o.Operand;
                        Instruction[] news = new Instruction[olds.Length];

                        for (int ii = 0; ii < news.Length; ii++)
                            news[ii] = bdy.Instructions[old.Instructions.IndexOf(olds[ii])];

                        inst.Operand = news;
                    }
                    else if (inst.OpCode.OperandType == OperandType.ShortInlineBrTarget ||
                             inst.OpCode.OperandType == OperandType.InlineBrTarget)
                        inst.Operand = bdy.Instructions[old.Instructions.IndexOf(inst.Operand as Instruction)];
                }

                foreach (ExceptionHandler eh in old.ExceptionHandlers)
                {
                    ExceptionHandler neh = new ExceptionHandler(eh.HandlerType);
                    if (old.Instructions.IndexOf(eh.TryStart) != -1)
                        neh.TryStart = bdy.Instructions[old.Instructions.IndexOf(eh.TryStart)];
                    if (old.Instructions.IndexOf(eh.TryEnd) != -1)
                        neh.TryEnd = bdy.Instructions[old.Instructions.IndexOf(eh.TryEnd)];
                    if (old.Instructions.IndexOf(eh.HandlerStart) != -1)
                        neh.HandlerStart = bdy.Instructions[old.Instructions.IndexOf(eh.HandlerStart)];
                    if (old.Instructions.IndexOf(eh.HandlerEnd) != -1)
                        neh.HandlerEnd = bdy.Instructions[old.Instructions.IndexOf(eh.HandlerEnd)];

                    switch (eh.HandlerType)
                    {
                        case ExceptionHandlerType.Catch:
                            neh.CatchType = ImportType(eh.CatchType, mod, ret, null);
                            break;
                        case ExceptionHandlerType.Filter:
                            neh.FilterStart = bdy.Instructions[old.Instructions.IndexOf(eh.FilterStart)];
                            //neh.FilterEnd = bdy.Instructions[old.Instructions.IndexOf(eh.FilterEnd)];
                            break;
                    }

                    bdy.ExceptionHandlers.Add(neh);
                }

                ret.Body = bdy;
            }

            return ret;
        }

        public static string GetNamespace(TypeDefinition typeDef)
        {
            while (typeDef.DeclaringType != null) typeDef = typeDef.DeclaringType;
            return typeDef.Namespace;
        }

        public static string GetName(TypeDefinition typeDef)
        {
            if (typeDef.DeclaringType == null) return typeDef.Name;

            StringBuilder ret = new StringBuilder();
            ret.Append(typeDef.Name);
            while (typeDef.DeclaringType != null)
            {
                typeDef = typeDef.DeclaringType;
                ret.Insert(0, typeDef.Name + "/");
            }
            return ret.ToString();
        }

        public static void Replace(MethodBody body, Instruction inst, Instruction[] news)
        {
            int instIdx = body.Instructions.IndexOf(inst);
            if (instIdx == -1) throw new InvalidOperationException();
            body.Instructions.RemoveAt(instIdx);
            for (int i = news.Length - 1; i >= 0; i--)
                body.Instructions.Insert(instIdx, news[i]);



            foreach (var i in body.Instructions)
            {
                if (i.Operand is Instruction && i.Operand == inst)
                    i.Operand = news[0];
                else if (i.Operand is Instruction[])
                {
                    Instruction[] insts = i.Operand as Instruction[];
                    int z;
                    if ((z = Array.IndexOf(insts, inst)) != -1)
                        insts[z] = news[0];
                }
            }
            foreach (var i in body.ExceptionHandlers)
            {
                if (i.TryStart == inst)
                    i.TryStart = news[0];
                if (i.TryEnd == inst)
                    i.TryEnd = news[0];
                if (i.HandlerStart == inst)
                    i.HandlerStart = news[0];
                if (i.HandlerEnd == inst)
                    i.HandlerEnd = news[0];
                if (i.FilterStart == inst)
                    i.FilterStart = news[0];
            }
        }

        public static string GetVersionName(this AssemblyDefinition asm)
        {
            return asm.Name.Name + ", Version=" + asm.Name.Version;
        }

        public static string GetVersionName(this AssemblyNameReference asmName)
        {
            return asmName.Name + ", Version=" + asmName.Version;
        }
    }
}