using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;

namespace Lea.Build
{
    public static class PublicCompiler
    {
        public static int Run(string response)
        {
            var args = CSharpCommandLineParser.Default.Parse(new[] { "@" + response },
                Path.GetDirectoryName(response), sdkDirectory: null, additionalReferenceDirectories: null);
            foreach (var error in args.Errors) Console.Error.WriteLine(error.ToString());
            if (args.Errors.Any(error => error.Severity == DiagnosticSeverity.Error)) return 1;
            if (args.AnalyzerReferences.Length != 0 || args.AnalyzerConfigPaths.Length != 0 ||
                args.AdditionalFiles.Length != 0 || args.EmbeddedFiles.Length != 0 ||
                args.SourceLink != null || args.AppConfigPath != null || args.OutputRefFilePath != null ||
                args.DocumentationPath != null || args.Win32ResourceFile != null ||
                args.CompilationOptions.CryptoKeyFile != null || args.CompilationOptions.CryptoKeyContainer != null ||
                args.CompilationOptions.DelaySign == true || args.EmitOptions.EmitMetadataOnly ||
                args.DisplayHelp || args.DisplayVersion || args.DisplayLangVersions ||
                args.InteractiveMode || args.ScriptArguments.Length != 0)
                throw new NotSupportedException("Response requires an unsupported compiler service; refusing partial compilation.");
            if (args.OutputFileName == null || args.SourceFiles.Length == 0)
                throw new InvalidDataException("Explicit output and source files required.");

            var trees = new List<SyntaxTree>();
            foreach (var source in args.SourceFiles)
            {
                using (var input = File.OpenRead(source.Path))
                {
                    var text = SourceText.From(input, args.Encoding, args.ChecksumAlgorithm);
                    trees.Add(CSharpSyntaxTree.ParseText(text, args.ParseOptions, source.Path));
                }
            }
            var references = new List<MetadataReference>();
            foreach (var reference in args.MetadataReferences)
            {
                if (!Path.IsPathRooted(reference.Reference))
                    throw new NotSupportedException("Only explicitly resolved absolute metadata references are supported.");
                references.Add(MetadataReference.CreateFromFile(reference.Reference, reference.Properties));
            }
            var options = args.CompilationOptions
                .WithSourceReferenceResolver(new SourceFileResolver(args.SourcePaths, args.BaseDirectory, args.PathMap))
                .WithXmlReferenceResolver(new XmlFileResolver(args.BaseDirectory))
                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);
            var compilation = CSharpCompilation.Create(args.CompilationName, trees, references, options);
            var output = Path.Combine(args.OutputDirectory, args.OutputFileName);
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var emit = args.EmitOptions;
            var pdbPath = args.PdbPath ?? Path.ChangeExtension(output, ".pdb");
            if (args.EmitPdb) emit = emit.WithPdbFilePath(pdbPath);
            var app = options.OutputKind == OutputKind.ConsoleApplication || options.OutputKind == OutputKind.WindowsApplication;
            using (var manifest = args.Win32Manifest == null ? null : File.OpenRead(args.Win32Manifest))
            using (var icon = args.Win32Icon == null ? null : File.OpenRead(args.Win32Icon))
            using (var native = compilation.CreateDefaultWin32Resources(true, args.NoWin32Manifest || !app, manifest, icon))
            using (var pe = new FileStream(output, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var pdb = args.EmitPdb && emit.DebugInformationFormat != DebugInformationFormat.Embedded
                ? new FileStream(pdbPath, FileMode.Create, FileAccess.Write, FileShare.None) : null)
            {
                var result = compilation.Emit(pe, pdb, win32Resources: native,
                    manifestResources: args.ManifestResources, options: emit);
                foreach (var diagnostic in result.Diagnostics) Console.Error.WriteLine(diagnostic.ToString());
                return result.Success ? 0 : 1;
            }
        }
    }
}
