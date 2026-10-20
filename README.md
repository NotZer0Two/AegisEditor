<div align="center">

![Logo](https://i.imgur.com/TjViDXW.png)

# Aegis Editor <sup>![Beta](https://img.shields.io/badge/-BETA-red?style=flat)</sup>

**Modern Editor for modern use**

</div>

> [!IMPORTANT]
> Need help? Join our [Discord](https://discord.gg/eeNZZfhWtE).

Aegis Editor is a schematic and prefab editor for SCP: Secret Laboratory servers (LabAPI), built on Unity. It is a port of MeowEditor, with a cleaner API and more features.

## Features

- **Asset bundles** - Schematics can be packed and compressed, replacing the classic `.json` format with smaller, faster-loading files.
- **Editor API** - An easy-to-use, powerful API for both the editor and in-game.
- **Client-side only** - Every spawnable item is client-side. Updates are real time, and you control exactly who can see and receive them.
- **Fully customizable** - Control exactly what gets sent to each player, which makes features like animations especially powerful.
- **Migration** - You can import MER schematics and many more format right out of the box.

## Installation

1. Download `AegisEditor.dll` and `AegisEditor.API.dll` from the [releases page](https://github.com/NotZer0Two/AegisEditor/releases).
2. Place `AegisEditor.API.dll` in the **dependencies** directory:
    - Linux: `~/.config/SCP Secret Laboratory/LabAPI/dependencies/<port>/`
    - Windows: `%appdata%/SCP Secret Laboratory/LabAPI/dependencies/<port>/`
3. Place `AegisEditor.dll` in the **plugins** directory:
    - Linux: `~/.config/SCP Secret Laboratory/LabAPI/plugins/<port>/`
    - Windows: `%appdata%/SCP Secret Laboratory/LabAPI/plugins/<port>/`
4. Restart the server.

## Contributing

Contributions are welcome. See [CONTRIBUTING.md](./CONTRIBUTING.md), and join the [Discord](https://discord.gg/eeNZZfhWtE) if you want to discuss an idea first.

## Security

See [SECURITY.md](./SECURITY.md).

## License

See [LICENSE](./LICENSE).