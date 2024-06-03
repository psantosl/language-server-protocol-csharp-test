Very simple example that shows how to use the Language Server Protocol from C# code to parse a Python file

# Setup
You need pylsp, which is a server that implements the LSP protocol to parse Python code.

To install it, simply:

```pip install python-lsp-server[all]```

Note: in macOS and Linux you probably need to do this instead:

```pip install 'python-lsp-server[all]'```

If you already have a working Python environment. In my case, I'm using MiniConda, and did this before to have it separated from the global environment:

```
conda create -n semantic_test  
conda activate semantic_test
conda install python=3.11
conda install setuptools
```

# Run it

Then you can simply run the "parser" like this:

```
> dotnet run sample.py
[Class] Name: Example. Start: 0,0. End: 8, 0
[Method] Name: method1. Start: 1,4. End: 4, 0
[Method] Name: method2. Start: 5,4. End: 8, 0
[Variable] Name: i. Start: 6,8. End: 8, 0
[Function] Name: function1. Start: 9,0. End: 12, 0
Folding Range. Start: 0, . End: 7,
Folding Range. Start: 1, . End: 3,
Folding Range. Start: 2, . End: 3,
Folding Range. Start: 5, . End: 7,
Folding Range. Start: 6, . End: 7,
Folding Range. Start: 9, . End: 11,
Folding Range. Start: 10, . End: 11,
Class 'Example' Folding Range: Start Line 0, Start Character 0, End Line 8, End Character 0
Method 'method1' Folding Range: Start Line 1, Start Character 4, End Line 4, End Character 0
Method 'method2' Folding Range: Start Line 5, Start Character 4, End Line 8, End Character 0
Function 'function1' Folding Range: Start Line 9, Start Character 0, End Line 12, End Character 0
```

And see how it basically locates all the symbols with their locations, and also "folding ranges" (there are more, but I'm printing only the ones that are not variables):

![image](https://github.com/psantosl/language-server-protocol-csharp-test/assets/380766/973ea180-51aa-4475-9916-eb873e33d782)

I played with the "folding ranges" but actually just the symbols are good enough to find where each method starts and ends, which is basically what I need to add Python support to SemanticMerge: https://blog.plasticscm.com/2015/09/custom-languages-in-semantic-version.html

# Available Python LSP servers
I'm using pylsp in the basic example, but I found that VSCode shows me different foldings, like for the imports, that this LSP server I'm using doesn't provide. Maybe with other server it happens.

## pylsp (Python Language Server)
* Maintained by the Palantir team and the community.
* Supports various linters, formatters, and code intelligence features.
* Install via: pip install 'python-lsp-server[all]'.

## pyright
* Developed by Microsoft.
* Focuses on type checking and is very fast.
* Can be used as a standalone LSP server or through vscode-pylance.
* Install via: npm install -g pyright.

## Jedi Language Server
* Based on the Jedi library.
* Provides autocompletion, function signatures, and other code intelligence features.
* Install via: pip install jedi-language-server.

# Other LSP servers
According to ChatGPT, looks like we could easily reuse this very same code to parse almost any file if we run the right LSP server:

## TypeScript/JavaScript
* typescript-language-server
* Install via: npm install -g typescript-language-server typescript.

## C/C++
* clangd
* Part of the LLVM project.
* Install via: brew install llvm (or download from LLVM's website).

## Java
* Eclipse JDT Language Server
* Part of the Eclipse IDE.
* Install via Eclipse IDE or use the jdtls project.

## Go
* gopls
* Developed by the Go team.
* Install via: go install golang.org/x/tools/gopls@latest.

## Rust
* rust-analyzer
* A new and experimental LSP server for Rust.
* Install via: cargo install rust-analyzer.

## PHP
* intelephense
* Provides comprehensive PHP language support.
* Install via: npm install -g intelephense.

## Ruby
* solargraph
* Provides code completion, documentation, and more for Ruby.
* Install via: gem install solargraph.

## JavaScript/TypeScript
* typescript-language-server
* For TypeScript and JavaScript.
* Install via: npm install -g typescript typescript-language-server.
