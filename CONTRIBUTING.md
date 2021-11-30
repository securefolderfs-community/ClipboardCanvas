## Translate Clipboard Canvas

### Adding a new language
 - Download and install Visual Studio 2019 and [Multilingual App Toolkit extension](https://marketplace.visualstudio.com/items?itemName=MultilingualAppToolkit.MultilingualAppToolkit-18308).
 - Fork Clipboard Canvas and create new branch with your changes
 - Clone forked repo and open in Visual Studio 2019
 - Right click `ClipboardCanvas (Universal Windows)` and select Multilingual App Toolkit > Add translation language
 - (You might encounter an error "Translation Provider Manager issue" you can ignore it and click "OK")
 - Select the language you want to translate
 - After hitting "OK" an `.xlf` file should appear with the language under `MultilingualResources`
 - Continue steps with "Improving an existing language" listed below

### Improving an existing language
 - Download [Multilingual App Toolkit Editor](https://docs.microsoft.com/en-us/windows/apps/design/globalizing/multilingual-app-toolkit-editor-downloads).
 - From the `MultilingualResources` folder, open the `.xlf` language file in the Multilingual App Toolkit text editor you want to translate
 - Using the editor, edit "Translation" field with your translation. Hit "Next row" to move onto another string.
 - Once you're done, save changes, push them to branch and create pull request
