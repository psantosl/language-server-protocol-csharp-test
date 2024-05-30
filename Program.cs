using System.Diagnostics;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using StreamJsonRpc;

namespace LSPClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"Usage: {System.Diagnostics.Process.GetCurrentProcess().ProcessName} file");
                Console.WriteLine($"Where 'file' is a sample python file to parse");
                return;
            }


            var psi = new ProcessStartInfo
            {
                FileName = "pylsp",
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            var process = Process.Start(psi);

            if (process == null)
            {
                Console.WriteLine("Failed to start pylsp process.");
                return;
            }
            
            var repoUri = new Uri($"file:///{Path.GetDirectoryName(Path.GetFullPath(args[0])).Replace('\\','/')}");
            var fileUri = new Uri($"file:///{Path.GetFullPath(args[0]).Replace('\\','/')}");

            using (var writer = process.StandardInput.BaseStream)
            using (var reader = process.StandardOutput.BaseStream)
            using (var jsonRpc = new JsonRpc(new HeaderDelimitedMessageHandler(writer, reader)))
            {
                var lspClient = new LspClient();

                jsonRpc.AddLocalRpcTarget(lspClient);
                jsonRpc.StartListening();

                var initializeParams = new InitializeParams
                {
                    ProcessId = Process.GetCurrentProcess().Id,
                    RootUri = repoUri,
                    Capabilities = new ClientCapabilities()
                };

                var initializeResult = await jsonRpc.InvokeWithParameterObjectAsync<InitializeResult>("initialize", initializeParams);
                //Console.WriteLine($"Server capabilities: {initializeResult.Capabilities.ToString()}");

                var textDocumentItem = new TextDocumentItem
                {
                    Uri = fileUri,
                    LanguageId = "python",
                    Version = 1,
                    Text = File.ReadAllText(args[0])
                };

                var didOpenParams = new DidOpenTextDocumentParams
                {
                    TextDocument = textDocumentItem
                };

                await jsonRpc.NotifyWithParameterObjectAsync("textDocument/didOpen", didOpenParams);

                // Fetch folding ranges
                var foldingRangeParams = new FoldingRangeParams
                {
                    TextDocument = new TextDocumentIdentifier
                    {
                        Uri = fileUri
                    }
                };

                var foldingRanges = await jsonRpc.InvokeWithParameterObjectAsync<FoldingRange[]>("textDocument/foldingRange", foldingRangeParams);

                // Fetch document symbols
                var documentSymbolParams = new DocumentSymbolParams
                {
                    TextDocument = new TextDocumentIdentifier
                    {
                        Uri = fileUri
                    }
                };

                var symbols = await jsonRpc.InvokeWithParameterObjectAsync<SymbolInformation[]>("textDocument/documentSymbol", documentSymbolParams);

                foreach (var symbol in symbols)
                    Console.WriteLine($"[{symbol.Kind}] Name: {symbol.Name}. Start: {symbol.Location.Range.Start.Line},{symbol.Location.Range.Start.Character}. End: {symbol.Location.Range.End.Line}, {symbol.Location.Range.End.Character}");

                // Combine folding ranges and symbols
                var foldingElements = new List<(string Type, string Name, Microsoft.VisualStudio.LanguageServer.Protocol.Range Range)>();

                foreach (var foldingRange in foldingRanges)
                    Console.WriteLine($"Folding Range. Start: {foldingRange.StartLine}, {foldingRange.StartCharacter}. End: {foldingRange.EndLine}, {foldingRange.EndCharacter}");

                foreach (var symbol in symbols)
                {
                    if (symbol.Kind == SymbolKind.Variable)
                        continue;

                    var range = foldingRanges.FirstOrDefault(fr =>
                        fr.StartLine == symbol.Location.Range.Start.Line &&
                        fr.EndLine +1 == symbol.Location.Range.End.Line);

                    if (range != null)
                    {
                        foldingElements.Add((symbol.Kind.ToString(), symbol.Name, symbol.Location.Range));
                    }
                }

                // Output the combined information
                foreach (var element in foldingElements)
                {
                    Console.WriteLine($"{element.Type} '{element.Name}' Folding Range: Start Line {element.Range.Start.Line}, Start Character {element.Range.Start.Character}, End Line {element.Range.End.Line}, End Character {element.Range.End.Character}");
                }

                // Keep the process running to interact with the LSP server

                await jsonRpc.NotifyAsync("exit");
            }

            process.WaitForExit();
        }
    }

    public class LspClient
    {
        // Define any handlers for server requests here
    }
}