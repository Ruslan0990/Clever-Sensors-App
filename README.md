# Clever-Sensors-App
Xamarin Android app that tracks the acceleration and orientation sensors in a foreground service,  plots the in real-time and saves the data to a local database.

Tested on Android 8 and 9.

## Requirements
- Xamarin Android ([install guide](https://docs.microsoft.com/en-us/xamarin/android/get-started/installation/android-sdk?tabs=windows))

## Credits
- The real-time charts are generated using  [MPAndroidChart](https://github.com/PhilJay/MPAndroidChart) library
- The data is stored on device using [Realm](https://realm.io/docs/dotnet/latest/) database.
- The custom start button is a Xamarin implementation of [dimorinny/floating-text-button](https://github.com/dimorinny/floating-text-button)
