# SimilarCode

A .NET-based tool for finding and analyzing similar code snippets, with a focus on processing and matching code from Stack Overflow posts.

## Features

- **Code Loading (`SimilarCode.Load`)**: Processes and loads code snippets from Stack Overflow posts into a SQLite database
- **Code Matching (`SimilarCode.Match`)**: Finds similar code snippets using optimized sequence alignment algorithms
- **Visualization (`VisualizeCloneReport`)**: Generates visual reports of code clone detection results
- **CLI Tools**: Command-line interfaces for both loading and matching operations
- **Benchmarking**: Performance benchmarking tools for code matching algorithms

## Project Structure

- `SimilarCode.Load`: Core library for processing and loading code snippets
- `SimilarCode.Load.Cli`: Command-line interface for the loading functionality
- `SimilarCode.Match`: Core library for code similarity matching
- `SimilarCode.Match.Cli`: Command-line interface for the matching functionality
- `SimilarCode.Load.Tests`: Unit tests for the loading functionality
- `SimilarCode.Benchmarks`: Performance benchmarking project
- `VisualizeCloneReport`: Tool for visualizing code clone detection results

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- Visual Studio 2022 or compatible IDE
- Stack Overflow data dump (Posts.xml)
- TXL installation for code analysis
- NiCad TXL grammars

### Obtaining Required Components

#### 1. Stack Overflow Data Dump

1. Visit [Stack Exchange Data Dump](https://archive.org/details/stackexchange) on Internet Archive
2. Download the `stackoverflow.com-Posts.7z` file
3. Either:
   - Use the 7z file directly (the tool supports reading from 7z files)
   - Extract the Posts.xml file from the archive

The Posts.xml file contains all Stack Overflow posts, including questions and answers. This tool processes only the answer posts (PostTypeId = 2) to extract code snippets.

#### 2. TXL Installation

1. Download TXL from [txl.ca](https://www.txl.ca/)
2. Decompress the directory
3. Installation is optional - you can use the binary directly

#### 3. NiCad Setup

Note: Only the NiCad TXL grammars are used from this application, not the NiCad matcher.

1. Go to [NiCad Download Page](https://www.txl.ca/txl-nicaddownload.html)
2. Select "gzipped tar (.tar.gz) source with Makefile"
3. Accept the terms and conditions
4. Download and decompress the .tar.gz file
5. Optional: In the decompressed folder at `NiCad-x.y/txl`, you can remove any programming language grammars you don't want to detect. This can greatly improve database creation performance, but those languages won't be parsed and will be marked as "Other".

### Data Processing Requirements

- For compressed format: Use the `.7z` file directly
- For uncompressed format: Extract `Posts.xml` from the archive
- Ensure you have sufficient disk space:
  - The 7z archive is typically several GB
  - The extracted Posts.xml is much larger
  - The resulting SQLite database will require additional space

### Building the Project

1. Clone the repository
2. Restore dependencies:
   ```powershell
   dotnet restore
   ```
3. Build the solution:
   ```powershell
   dotnet build
   ```
4. Run tests:
   ```powershell
   dotnet test
   ```

### Usage

The tool works in two main steps:

1. First, you need to load and process the Stack Overflow data into a SQLite database (this is a one-time operation that can take 8 hours to several days depending on your hardware)
2. Then you can use this database to find similar code snippets

#### Step 1: Loading Stack Overflow Posts into Database

Before you can search for similar code, you must first process the Stack Overflow data dump into a SQLite database. This is a one-time operation that creates a database you'll use for all subsequent searches.

```powershell
dotnet run --project SimilarCode.Load.Cli -- \
  -p <path-to-stackoverflow-posts> \
  -g <path-to-txl-grammars> \
  -o <output-database-path> \
  -t <number-of-threads>
```

Parameters:

- `-p, --posts`: Path to Stack Overflow posts file (either .7z or .xml)
- `-g, --grammars`: Path to TXL grammar files (from NiCad)
- `-o, --output`: Path where the SQLite database will be created
- `-t, --threads`: Number of threads to use for processing

Example:

```powershell
dotnet run --project SimilarCode.Load.Cli -- \
  -p "D:\data\stackoverflow.com-Posts.7z" \
  -g "C:\NiCad-6.2\txl" \
  -o "D:\output\SimilarCode.db" \
  -t 8
```

This process may take several hours to days depending on your hardware and the number of threads used. The resulting database will be used for all subsequent similarity searches.

#### Step 2: Finding Similar Code

Once you have generated the database, you can use it to find code snippets similar to your input:

```powershell
dotnet run --project SimilarCode.Match.Cli -- \
  -i <input-code-file> \
  -g <path-to-txl-grammars> \
  -o <output-report-path> \
  -t <number-of-threads>
```

Parameters:

- `-i, --input`: The code file you want to find matches for
- `-g, --grammars`: Same TXL grammar files used in Step 1
- `-o, --output`: Path to the database created in Step 1
- `-t, --threads`: Number of threads to use for matching

Example:

```powershell
dotnet run --project SimilarCode.Match.Cli -- \
  -i "D:\mycode\sample.cs" \
  -g "C:\txl\grammars" \
  -o "D:\output\SimilarCode.db" \
  -t 8
```

The tool will search through all processed Stack Overflow code snippets in the database and return matches sorted by similarity.

## Features in Detail

### Code Loading

- Processes Stack Overflow XML dumps
- Supports both compressed (.7z) and uncompressed XML formats
- Extracts code snippets with language detection
- Stores processed data in SQLite database
- Includes malware scanning capabilities (Windows only, via AMSI)

### Code Matching

- Uses optimized sequence alignment algorithms
- Supports parallel processing for improved performance
- Configurable similarity thresholds
- Progress tracking with ASCII progress bar

### Visualization

- Generates human-readable reports of code clone detection
- Supports XML output format
- Links similar code snippets to their Stack Overflow sources

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Uses the [TXL](https://www.txl.ca/) transformation language for code analysis
- Sequence alignment algorithm adapted from [GeeksForGeeks](https://www.geeksforgeeks.org/sequence-alignment-problem/)

## How It Works

The SimilarCode database is a SQLite database generated by Entity Framework Core. The database generation process involves:

1. **Code Extraction and Language Detection**:

   - Processes each Stack Overflow answer post
   - Extracts code from code blocks (marked with `<pre><code>` tags)
   - Each code block is treated as a separate snippet
   - Attempts to detect the programming language by parsing with TXL grammars
   - Snippets are deconstructed into functions where possible, or kept as code blocks if function parsing fails

2. **Code Processing**:

   - Only snippets above a certain line threshold are saved
   - Code is formatted using the TXL grammar's formatting guidelines
   - A snippet might match multiple programming language grammars
   - Code must be syntactically valid according to the TXL grammar
   - If language detection fails, the snippet is stored as-is with minimal whitespace adjustments

3. **Matching Process**:
   - Input code is parsed using TXL grammar for consistent formatting
   - If code can't be compiled or doesn't match the grammar, matching accuracy may be reduced
   - Uses sequence alignment algorithms to compare snippets
   - Returns top N matches with their Stack Overflow sources
   - Note: If TXL stacksize is exceeded, TXL may crash. The stacksize can be increased using the `editbin` command, but be cautious as large values may cause excessive CPU/memory usage with large or invalid files.
