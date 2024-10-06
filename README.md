# Lain

Lain is a CLI tool built in C# that communicates with the LM Studio API. It allows users to define and use subcommands, utilizing customizable templates for system and user messages. The tool is designed to be lightweight, flexible, and extendable, making it easy to create custom AI-powered interactions.

## Features

- **Subcommand-based**: Define and execute subcommands with ease.
- **Integration with LM Studio API**: Seamless communication with the LM Studio API.
- **Cross-platform**: Designed to run on Linux and macOS.

### Example Use Case

'''
lain doc main.py
'''

The `doc` command generates documentation for the given code (`main.py`) by sending a system message that explains that the AI is an assistant for generating documentation, followed by the user message containing the code.

'''
lain ask 'What is love?'
'''

The `ask` command sends the question to the API, attempting to answer it in a one-shot, low-token response.

'''
lain chat
'''

The `chat` command emulates a simple chat client, allowing a conversation with the LLM via the API.

### Customizable Templates

Users can define their own templates for system and user messages, providing flexibility to create tailored interactions.

## Installation

### Prerequisites

- .NET SDK 8.0
- LM Studio
- Git

### From Source (Manual)

1. Clone the repository:
   '''
   git clone https://github.com/tfgrass/lain.git
   '''
2. Install the .NET SDK 8.0 if you haven't already.
3. Build the project:
   '''
   dotnet build
   '''
4. Publish for Linux or macOS as a self-contained single-file executable:
   
   For Linux:
   '''
   dotnet publish -c Release -r linux-x64 --self-contained -p:PublishSingleFile=true -o ./publish
   '''
   
   For macOS (Intel):
   '''
   dotnet publish -c Release -r osx-x64 --self-contained -p:PublishSingleFile=true -o ./publish
   '''
   
   For macOS (M1/M2 Apple Silicon):
   '''
   dotnet publish -c Release -r osx-arm64 --self-contained -p:PublishSingleFile=true -o ./publish
   '''

5. Symlink the executable to your `$PATH`:

   For MacOS & Linux:
   '''
   sudo ln -s /path/to/lain/publish/lain /usr/local/bin/lain
   '''

   Alternatively, you can run `lain` directly from the `publish` directory:
   '''
   ./publish/lain [command]
   '''

## Usage

Start LM Studio, load a model, and start the server.

With LM Studio API running, you can run the `lain` command from your terminal:

'''
lain [command] [arguments]
'''

For example:

'''
lain doc myscript.py
'''

## License

Lain is released under the Artistic License 2.0. See the [LICENSE](LICENSE) file for more details.

## Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.
