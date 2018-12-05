# ThriveChurchOfficialAPI 
This API is primarily used by the Thrive Church Official App for serving users with content for both the Bible and our recorded / live sermons.

## Stack
- C# .NET Core 2.1
- MongoDB

## Contributing
Please create your own .NET Core 2.1 application on your machine within this git directory. Once your application has been made. This is important because there are a number of files that are created when initializing a new .NET Application that you will need. Many of these files are settings, DLLs, Binaries, and other debugging files that have been ignored as to not clutter the repo. 

You will need to make sure you have the following settings in your `AppSettings.json`. 
  1. `EsvApiKey` - If you wish to connect to the ESV Api, you will need to request an Auth Token from [the ESV API website](https://api.esv.org/). Simply include this token without "Token" for this setting.
  2. `MongoConnectionString` - Use this for connecting to your MongoDB instance; should contain the prefix "mongodb://"
