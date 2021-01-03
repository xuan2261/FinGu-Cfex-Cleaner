using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System.Linq;
using System.Runtime.InteropServices;
using System;

namespace FinGu_Cleaner
{
    class Protections
    {
        public static void Run(Context Context)
        {
            CleanMath(Context);
            CleanSizeOf(Context);
            CleanParsing(Context);
            CleanEmptyTypes(Context);
            Context.Save();
        }
        static void CleanMath(Context Context)
        {
            foreach (var TypeDef in Context.Module.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Ldc_R8 &&
                             IL[x + 1].OpCode == OpCodes.Call && IL[x + 1].Operand.ToString().Contains("Math::"))
                        {
                            // Get Math. Method By MDToken In Reflection Way To Invoke
                            var MInvoke = Context.Ass.ManifestModule.ResolveMethod((int)((IMethod)IL[x + 1].Operand).MDToken.Raw);
                            // Insert Params For Invoke Math. Method
                            object[] MParam = new object[] { float.Parse(IL[x].Operand.ToString()) };
                            // Get Returned Value From (Math.Ceiling or Math.Sqrt) etc.
                            var MVal = MInvoke.Invoke(null, MParam);
                            // Remove Math Param
                            IL.RemoveAt(x);
                            // Make Instruction Opcode Ldc_R8
                            IL[x].OpCode = OpCodes.Ldc_R8;
                            // Insert New Operand That We Get On Invoking
                            IL[x].Operand = MVal;
                            // Log Info
                            Context.Log.Information($"{MInvoke.Name} -> {MVal}");
                        }
                    }
                }
            }
        }
        static void CleanSizeOf(Context Context)
        {
            foreach (var TypeDef in Context.Module.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Sizeof &&
                            IL[x].Operand is IDnlibDef)
                        {
                            // Get Struct Type/Size
                            var Type = Context.Ass.ManifestModule.ResolveType((int)((IDnlibDef)IL[x].Operand).MDToken.Raw);
                            // Get SizeOf By Marshal / Cuz Its Use Types For Sizeof
                            var NewOperand = Marshal.SizeOf(Type);
                            // Convert Instruction Opcode
                            IL[x].OpCode = OpCodes.Ldc_I4;
                            // Set New Operand independ on NewOperand
                            IL[x].Operand = NewOperand;
                            // Log Info ! 
                            Context.Log.Information($"Fixed Struct Size Of -> {NewOperand}");
                        }
                    }
                }
            }
        }
        static void CleanParsing(Context Context)
        {
            foreach (var TypeDef in Context.Module.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        // Check If its double.parse(...);
                        if (IL[x].OpCode == OpCodes.Ldstr &&
                            IL[x + 1].OpCode == OpCodes.Call && IL[x + 1].Operand.ToString().Contains("Double::Parse"))
                        {
                            // Calculate It
                            var Result = double.Parse(IL[x].Operand.ToString());
                            // Remove Ldstr/String
                            IL.RemoveAt(x);
                            // Convert Call To Double
                            IL[x].OpCode = OpCodes.Ldc_R8;
                            // Finally Set Operand
                            IL[x].Operand = Result;
                            // Log xD
                            Context.Log.Information($"Fixed Double.Parse -> {Result}");
                        }
                        // Check If its Int.Parse(...);
                        if (IL[x].OpCode == OpCodes.Ldstr &&
                            IL[x + 1].OpCode == OpCodes.Call && IL[x + 1].Operand.ToString().Contains("Int32::Parse"))
                        {
                            // Calculate It
                            var Result = int.Parse(IL[x].Operand.ToString());
                            // Remove Ldstr/String
                            IL.RemoveAt(x);
                            // Convert Call To Int32
                            IL[x].OpCode = OpCodes.Ldc_I4;
                            // Finally Set Operand
                            IL[x].Operand = Result;
                            // Log xD
                            Context.Log.Information($"Fixed Int32.Parse -> {Result}");
                        }
                    }
                }
            }
        }
        static void CleanEmptyTypes(Context Context)
        {
            foreach (var TypeDef in Context.Module.Types.Where(x => x.HasMethods))
            {
                foreach (var MethodDef in TypeDef.Methods.Where(x => x.HasBody))
                {
                    var IL = MethodDef.Body.Instructions;
                    for (int x = 0; x < IL.Count; x++)
                    {
                        if (IL[x].OpCode == OpCodes.Ldsfld && IL[x].Operand.ToString().Contains("Type::EmptyTypes") &&
                            IL[x + 1].OpCode == OpCodes.Ldlen)
                        {
                            // Get Orignal Val
                            var EmptyTypesLen = int.Parse(Type.EmptyTypes.Length.ToString());
                            // Remove Field
                            IL.RemoveAt(x);
                            // Convert Ldlen to int
                            IL[x].OpCode = OpCodes.Ldc_I4;
                            // Set Operand
                            IL[x].Operand = EmptyTypesLen;
                            // Log
                            Context.Log.Information($"Fixed EmptyTypes -> {EmptyTypesLen}");
                        }
                    }
                }
            }
        }
    }
}
