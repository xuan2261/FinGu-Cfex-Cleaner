using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Serilog;
using Serilog.Core;
using System.Reflection;

namespace FinGu_Cleaner
{
    public class Context
    {
        /// <summary>
        /// Module Path To Get Saved & Loaded
        /// </summary>
        public string ModulePath { set; get; }
        /// <summary>
        /// Module We Load To Clean
        /// </summary>
        public ModuleDefMD Module { set; get; }
        /// <summary>
        /// Load Module In Reflection Way To Use Invoke
        /// </summary>
        public Assembly Ass { set; get; }
        /// <summary>
        /// Using SeriLog For Logging/Debugging
        /// </summary>
        public Logger Log { set; get; }
        /// <summary>
        /// Initialize Context & Loading
        /// </summary>
        /// <param name="Args"> Arguments That Have Path </param>
        public Context(string[] Args)
        {
            if (Args.Length == 1) { ModulePath = Args[0]; }
            if (Args.Length == 0) { System.Console.Write("Enter Path : "); ModulePath = @System.Console.ReadLine().Replace("\"", ""); }
            Module = ModuleDefMD.Load(ModulePath);
            // due to some shitty error nvm plz xD ' UnsafeLoadFrom '
            Ass = Assembly.UnsafeLoadFrom(ModulePath);
            Log = new LoggerConfiguration()
                .WriteTo.Console(theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Grayscale)
                .CreateLogger();
        }
        public void Save()
        {
            if (Module.IsILOnly)
            {
                var Options = new ModuleWriterOptions(Module)
                {
                    Logger = DummyLogger.NoThrowInstance
                };
                var NewPath = Module.Kind.Equals(ModuleKind.Dll) ? ModulePath.Replace(".dll", "-Cleaned.dll") : ModulePath.Replace(".exe", "-Cleaned.exe");
                Module.Write(NewPath, Options);
                Log.Information($"Module Saved : {NewPath}");
                System.Console.ReadKey();
            }
            else
            {
                var Options = new NativeModuleWriterOptions(Module, false)
                {
                    Logger = DummyLogger.NoThrowInstance
                };
                var NewPath = Module.Kind.Equals(ModuleKind.Dll) ? ModulePath.Replace(".dll", "-Cleaned.dll") : ModulePath.Replace(".exe", "-Cleaned.exe");
                Module.NativeWrite(NewPath, Options);
                Log.Information($"Module Saved : {NewPath}");
                System.Console.ReadKey();
            }
        }
    }
}