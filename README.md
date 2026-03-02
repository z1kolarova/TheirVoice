# What is _Their Voice_

- Unity application with a 3D environment and NPCs to have conversations with using ChatGPT and text-to-speech WhisperAI.
- A tool for practising outreach conversations in the _*Cube of Truth*_ format of _*Anonymous for the Voiceless*_.

# Platform & how it works

- To use the application, 2 separate builds are needed - a Server and Client, both running on Windows. For the time being, I only launch the Server when needed, but you CAN make your own server build and use your own API key - scene InitialLoading has an object to switch between Server and Client build in Unity Editor.
- Connection between Server and Client builds is done through Unity Lobbies & Relay. The Server acts as a middleman between Client and the APIs, providing the API key and prompts for the NPCs.

## Languages

Both the application's UI and NPC conversations are currently only in English, but infrastructure for conversations in multiple languages is in place. Conversations in more languages are planned, translation of UI elements is not a priority at the moment.
- **English** - the default (and currently only) language

# Data processing and retention

- User input is sent to ChatGPT Moderation check (currently not working).
- For development purposes, sharing inputs and outputs with OpenAI is currently enabled.
- For development purposes, conversations are currently saved on the Server end. The conversations aren't in any way associated with the person who produced them.

# License

[MIT](https://choosealicense.com/licenses/mit/)
