# Thrive Church Official API
This API is primarily used by the Thrive Church Official App for serving users with content for both the Bible and our recorded / live sermons.

[![Codacy Badge](https://api.codacy.com/project/badge/Grade/02b020659d344b9883ec20221c6f6c7e)](https://www.codacy.com/app/wyattbaggett/ThriveChurchOfficialAPI?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=ThriveCommunityChurch/ThriveChurchOfficialAPI&amp;utm_campaign=Badge_Grade)

## Stack
- C# .NET Core 2.1
- MongoDB

## API Documentation & Swagger UI
Visiting `~/swagger/index.html` in your browser will allow you to view the Swagger UI for the API and easily send API requests as well as view response objects.

## Caching
*Note, the Caching that this application uses is **NOT** persistent, and will expire thirty (30) seconds after an initial request.*

If you wish to poll this API for whether or not the `LiveSermon` object is still active, the response will contain the Expiration Time (in **UTC**) for the `LiveSermon` object. Using this you will be able to gague how much longer the stream will be active. This will prevent having to poll the route in a loop to determine if right now the time has passed.

## Contributing
Please create your own .NET Core 2.1 application on your machine. Once your application has been made, copy these files into your Solution. This is important because there are a number of files that are created when initializing a new .NET Application that you will need. Many of these files are settings, DLLs, Binaries, and other debugging files that have been ignored as to not clutter the repo. 

You will need to make sure you have the following settings in your `AppSettings.json`. 
  1. `EsvApiKey` - If you wish to connect to the ESV Api, you will need to request an Auth Token from [the ESV API website](https://api.esv.org/). Simply include this token without "Token" for this setting.
  2. `MongoConnectionString` - Use this for connecting to your MongoDB instance; should contain the prefix "mongodb://"
