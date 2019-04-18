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

## Rate Limiting
The _Thrive Church Official API_ takes advantage of Rate Limiting on all requests, and will count rejected requests towards subsequent limits.

More information about how this API handles rate limiting can be found within [RateLimits.md](https://github.com/ThriveCommunityChurch/ThriveChurchOfficialAPI/blob/master/RateLimits.md)

## Contributing
Please create your own .NET Core 2.1 application on your machine. Once your application has been made, copy these files into your Solution. This is important because there are a number of files that are created when initializing a new .NET Application that you will need. Many of these files are settings, DLLs, Binaries, and other debugging files that have been ignored as to not clutter the repo. 

You will need to make sure you have the following settings in your `AppSettings.json`. 
  1. `EsvApiKey` - If you wish to connect to the ESV Api, you will need to request an Auth Token from [the ESV API website](https://api.esv.org/). Simply include this token without "Token" for this setting.
  2. `MongoConnectionString` - Use this for connecting to your MongoDB instance; should contain the prefix "mongodb://".
  3. `OverrideEsvApiKey` - Use this setting to override a check for your ESV API Key.
      - _Set to_ `"true"` _if you want to skip the ckeck for your API Key._
      - _Set to_ `"false"` _if you want to use the PassagesController and make requests. NOTE: If this setting is set to false and no API Key is found, the application will throw an exception._
  4. `IpRateLimiting` - Use this for setting your configurable Rate Limiting settings. See [RateLimits.md](https://github.com/ThriveCommunityChurch/ThriveChurchOfficialAPI/blob/master/RateLimits.md) for more information.
