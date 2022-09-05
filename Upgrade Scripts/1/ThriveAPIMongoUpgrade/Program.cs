using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using ThriveAPIMongoUpgrade.Objects;

namespace ThriveAPIMongoUpgrade
{
    class Program : Base
    {
        static void Main(string[] args)
        {
            ReadOptions(args);

            var mongoUrl = MongoUrl.Create(ConnectionString);
            var mongoClient = new MongoClient(mongoUrl);

            var database = mongoClient.GetDatabase("SermonSeries");

            var messagesCollection = database.GetCollection<SermonMessage>("Messages");
            var seriesCollection = database.GetCollection<SermonSeries>("Series");
            var legacySeriesCollection = database.GetCollection<LegacySermonSeries>("Sermons");

            database.DropCollection("Series");
            database.DropCollection("Messages");

            var legacySeriesResponse = legacySeriesCollection.Find(Builders<LegacySermonSeries>.Filter.Empty);
            var legacySeriesList = legacySeriesResponse.ToList();

            var newSeries = new List<SermonSeries>();
            var newMessages = new List<SermonMessage>();

            if (legacySeriesList != null && legacySeriesList.Any())
            {
                foreach (var legacySeries in legacySeriesList)
                {
                    newSeries.Add(new SermonSeries
                    {
                        ArtUrl = legacySeries.ArtUrl,
                        CreateDate = legacySeries.LastUpdated,
                        EndDate = legacySeries.EndDate,
                        LastUpdated = DateTime.UtcNow,
                        Name = legacySeries.Name,
                        Id = legacySeries.Id,
                        Slug = legacySeries.Slug,
                        StartDate = legacySeries.StartDate,
                        Thumbnail = legacySeries.Thumbnail
                    });

                    foreach (var message in legacySeries.Messages)
                    {
                        newMessages.Add(new SermonMessage
                        {
                            AudioDuration = message.AudioDuration,
                            AudioFileSize = message.AudioFileSize,
                            AudioUrl = message.AudioUrl,
                            Date = message.Date,
                            LastUpdated = DateTime.UtcNow,
                            PassageRef = message.PassageRef,
                            PlayCount = 0,
                            SeriesId = legacySeries.Id,
                            Speaker = message.Speaker,
                            Title = message.Title,
                            VideoUrl = message.VideoUrl
                        });
                    }
                }
            }

            seriesCollection.InsertMany(newSeries);
            messagesCollection.InsertMany(newMessages);

        }
    }
}