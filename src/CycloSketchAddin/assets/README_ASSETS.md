Place your UI preview images in this folder.

Expected file names:
- preview_necessary_params.png
- preview_optional_params.png
- preview_detailed_settings.png

At runtime, the add-in looks in the DLL output folder:
- bin/Release/net48/assets/

You can copy the same files there after build, or update your build pipeline to copy them automatically.
