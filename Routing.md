## REST Routing

### Sermons Controller
- `GET /api/sermons/` - Returns a collection of all Sermons.
- `GET /api/sermons/live` - Returns an object that contains information about the live-stream for a given sermon message. Also includes a direct link to the video itself.
- `PUT /api/sermons/live` - Updates the live-streaming object to a new title or video url. This can be used when a livestream is active.
- `PUT /api/sermons/live/special` - Updates the live-streaming object to create a special event broadcast.

### Bible Controller
- `GET /api/passage/{searchCriteria}` - Returns an object containing a formatted string that can be used for serving users the text of their specified search criteria.
