# Lain

Lain is a CLI tool built in Raku that communicates with the LM Studio API. It allows users to define and use subcommands, utilizing customizable templates for system and user messages. The tool is designed to be lightweight, flexible, and extendable, making it easy to create custom AI-powered interactions.

## Features

- **Subcommand-based**: Define and execute subcommands with ease.
- **Template-driven**: System and user messages are template-based, allowing customization.
- **Integration with LM Studio API**: Seamless communication with the LM Studio API.
- **Cross-platform**: Designed to run on Linux and macOS.

### Example Use Case

```
lain doc main.py
```

The `doc` command generates documentation for the given code (`main.py`) by sending a system message that explains that the AI is an assistant for generating documentation, followed by the user message containing the code.

```
lain ask 'What is love?'
```

The `ask` command sends the question to the api, trying to answer it one-shot in low-token response.

```
lain chat
```

The `chat` command emulates a simple chat client, allowing a conversation with the llm via the api.

### Customizable Templates

Users can define their own templates for system and user messages, providing flexibility to create tailored interactions.

## Installation

### Prerequisites

- Raku programming language 
- Zefs
- Git
- LM Studio 


### From Source (Manual)

1. Clone the repository:
   ```
   git clone https://github.com/tfgrass/lain.git
   ```
2. Install dependencies via Zef.
3. Create a symlink to put lain into your $PATH
   - alternativly just use bin/lain inside the repo
## Usage
Start LM Studio, load a model and start the server. 

With LM Studio API running, you can run the `lain` command from your terminal:

```
lain [command] [arguments]
```

For example:

```
lain doc myscript.py
```

## License

Lain is released under the Artistic License 2.0. See the [LICENSE](LICENSE) file for more details.

## Contributing

Contributions are welcome! Feel free to open an issue or submit a pull request.
