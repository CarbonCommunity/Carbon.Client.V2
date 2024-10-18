using Carbon.Base;
using Carbon.Extensions;
using Il2CppInterop.Runtime.Injection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics.Internal;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

public class Compiler : BaseProcessor
{
    public FileSystemWatcher Watcher = new("BepInEx/mods", "*.cs");

    public override string GetName()
    {
        return "Roslyn Compiler";
    }

    public override void Setup()
    {
        base.Setup();

        Rate = 1f;
        Watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.LastAccess;
        Watcher.EnableRaisingEvents = true;
		Watcher.Created += (object sender, FileSystemEventArgs e) =>
		{
			var path = e.FullPath;
			AddProcess(path, new Plugin(path));
		};
		Watcher.Renamed += (object sender, RenamedEventArgs e) =>
		{
			if(BasePlugin.plugins.TryGetValue(e.OldFullPath, out var existent))
			{
				existent.Unload();
			}

			MarkDirty(e.FullPath);
		};
		Watcher.Changed += (object sender, FileSystemEventArgs e) =>
		{
			MarkDirty(e.FullPath);
		};
		Watcher.Deleted += (object sender, FileSystemEventArgs e) =>
		{
			if (BasePlugin.plugins.TryGetValue(e.FullPath, out var existent))
			{
				existent.Unload();
			}
		};

		Init();
    }

    public void Init()
    {
        foreach(var file in Directory.GetFiles(Watcher.Path, "*.cs"))
        {
            AddProcess(file, new Plugin(file));
        }
    }

    public class Plugin : BaseProcess
    {
        public string Name;
        public string FileName;
        public string FilePath;

        public Plugin(string path)
        {
            Name = Path.GetFileNameWithoutExtension(path);
            FileName = Path.GetFileName(path);
            FilePath = Path.GetFullPath(path);
            IsDirty = true;
        }

        public override void Do()
        {
            Compilation.Compile(FilePath);
        }
    }

    public struct Compilation
    {
        public string fileName;
        public string filePath;
        public string source;
        public float CompileTime;
        public Assembly assembly;

        public static Compilation Compile(string path)
        {
            Compilation compilation = default;
            compilation.fileName = Path.GetFileName(path);
            compilation.filePath = path;
            compilation.source = OsEx.File.ReadText(path);
            compilation.Compile();
            return compilation;
        }

        private static List<MetadataReference> references = new();
        private static bool hasReferences;

        private static List<SyntaxTree> trees = new();

        public static void HandleReferences()
        {
            if (hasReferences)
            {
                return;
            }

            hasReferences = true;

            ProcessDirectory("BepInEx/system");
            ProcessDirectory("BepInEx/plugins");
            ProcessDirectory("BepInEx/core");
            ProcessDirectory("BepInEx/interop");

            static void ProcessDirectory(string directory)
            {
                foreach(var file in Directory.GetFiles(directory, "*.dll"))
                {
                    if (file.Contains("dobby"))
                    {
                        continue;
                    }

                    try
                    {
                        references.Add(MetadataReference.CreateFromFile(file));
                        Debug.LogWarning($"Loaded reference: {Path.GetFileName(file)}");
                    }
                    catch(Exception ex)
                    {
                        Debug.LogError($"Failed loading reference: {file} ({ex.Message})\n{ex.StackTrace}");
                    }
                }
            }
        }

        public void Compile()
        {
            HandleReferences();

            var parseOptions = new CSharpParseOptions(LanguageVersion.Preview);
            var tree = CSharpSyntaxTree.ParseText(source, options: parseOptions);

            trees.Clear();
            trees.Add(tree);

            var options = new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                deterministic: true,
                warningLevel: 4,
                allowUnsafe: true);
            var compilation = CSharpCompilation.Create(
                $"Script.{fileName}.{Guid.NewGuid():N}", trees, references, options);

            using var dllStream = new MemoryStream();
            var emit = compilation.Emit(dllStream);

            foreach (var error in emit.Diagnostics)
            {
                var span = error.Location.GetMappedLineSpan().Span;
            
                switch (error.Severity)
                {
                    case DiagnosticSeverity.Error:
                        Debug.LogWarning($"Failed compiling ({fileName}):{span.Start.Character + 1} line {span.Start.Line + 1} [{error.Id}]: {error.GetMessage(CultureInfo.InvariantCulture)}");
                        break;
                }
            }

            if (emit.Success)
            {
                var assembly = dllStream.ToArray();

                if (assembly != null)
                {
                    this.assembly = Assembly.Load(assembly);
                    OnComplete();
                }
                else
                {
                    OnFail();
                }
            }
            else
            {
                OnFail();
            }
        }

        public void OnComplete()
        {
            foreach (var type in assembly.GetTypes())
            {
                var info = type.GetCustomAttribute<InfoAttribute>();

                if (info == null)
                {
                    continue;
                }

                var plugin = Activator.CreateInstance(type) as BasePlugin;
                plugin.Name = info.Title;
                plugin.Path = filePath;
                plugin.Load();
            }
        }

        public void OnFail()
        {

        }
    }
}
