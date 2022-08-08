# Logistics

## Building the application

In order to build the application, you'll need to install the following:

- [.NET LTS](https://www.microsoft.com/net/download)


Restore packages for API

```bash
cd Logistics
dotnet restore
```

## Running the application

Start the API:

```bash
cd Logistics/Logistics.API
dotnet run
```

Open a browser and enter the URL [http://localhost:5000](http://localhost:5000) to view the site

## Production Deployment
For the best performance publish the application in release mode

```bash
dotnet publish -c Release -o Deploy --self-contained false
```
This will create a folder called Deploy with all the necessary files to run the application.
You can use the following command to run the published application:

```bash
cd Deploy
dotnet Logistics.API.dll
```

## Running scripts on the database

Create a new collection `cities` using the following aggregation on `worldcities` collection

```
[{
 $match: {
  population: {
   $gt: 1000
  }
 }
}, {
 $sort: {
  population: -1
 }
}, {
 $group: {
  _id: '$country',
  cities: {
   $push: '$$ROOT'
  }
 }
}, {
 $project: {
  _id: '$_id',
  cities15: {
   $slice: [
    '$cities',
    15
   ]
  }
 }
}, {
 $unwind: {
  path: '$cities15',
  preserveNullAndEmptyArrays: false
 }
}, {
 $project: {
  _id: {
   $concat: [
    {
     $replaceAll: {
      input: '$cities15.country',
      find: '/',
      replacement: '%2F'
     }
    },
    ' - ',
    {
     $replaceAll: {
      input: '$cities15.city_ascii',
      find: '/',
      replacement: '%2F'
     }
    }
   ]
  },
  position: [
   '$cities15.lng',
   '$cities15.lat'
  ],
  country: '$cities15.country',
  createdAt: new Date(),
  metadata: {
   schemaType: 'city',
   schemaVersion: '1.0',
  }
 }
}, {
 $out: 'cities'
}]
```

Create a new collection `planes` using the following aggregation on `cities` collection

```
[{
 $sample: {
  size: 200
 }
}, {
 $group: {
  _id: null,
  planes: {
   $push: {
    currentLocation: '$position',
    departed: '$_id'
   }
  }
 }
}, {
 $unwind: {
  path: '$planes',
  includeArrayIndex: 'id'
 }
}, {
 $project: {
  _id: {
   $concat: [
    'CARGO',
    {
     $toString: '$id'
    }
   ]
  },
  currentLocation: '$planes.currentLocation',
  departed: '$planes.departed',
  heading: {
   $literal: 0
  },
  route: [],
  createdAt: new Date(),
  metadata: {
   schemaType: 'plane',
   schemaVersion: '1.0'
  }
 }
}, {
 $out: 'planes'
}]
```
## Indexes

The application automatically creates 2DSphere indexes for collections `cities` and `planes` on `position` and `currentLocation` fields respectively.
Additionally, indexes can be created on `location`  for `planes` collection and `status`, `location` on cargos collection.

```bash
db.planes.createIndex({location:1})
db.cargos.createIndex({status:1,location:1})
```

## Document Helpers
- Document helpers for `city`, `cargo` and `plane` documents.
- Provides `Get` and `Set` methods for flexible schemaless retrieval and assignment of value
```csharp
public BsonValue Get(string field)
{
    return Document.GetValue(field);
}

public void Set(string field, BsonValue value)
{
    Document.Set(field, value);
}
```
- Provide `get` and `set` accessor method for type safe retrieval and assignment of value
```csharp
public string Country
{
    get => Document.GetValueAsString(City.Country);
    set => Document.Set(City.Country, value);
}
```

## Command And Query Helpers
- Command And Query helpers are provide for segragating the command and query logic from the application logic. Queries can be separately optimized in this way.
```csharp
public static async Task<IEnumerable<BsonDocument>> FindAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, ProjectionDefinition<BsonDocument> projection = default, int limit = default, CancellationToken cancellationToken = default)
{
    var options = new FindOptions<BsonDocument, BsonDocument>()
    {
        Limit = limit,
        Projection = projection,
    };

    return (await collection.FindAsync<BsonDocument>(filter, options, cancellationToken)).ToEnumerable(CancellationToken.None);
}
```
```csharp
public static async Task<UpdateResult> UpdateOneAsync(IMongoCollection<BsonDocument> collection, FilterDefinition<BsonDocument> filter, UpdateDefinition<BsonDocument> update, CancellationToken cancellationToken = default)
{
    update.CurrentDate(UpdatedAt);
    return await collection.UpdateOneAsync(filter, update, default, cancellationToken);
}
```

## Development Notes
- Open API integration [Swagger](http://localhost:5000/swagger/index.html) and static file serving.
- Configuration and constants used where appropriate.
- Asynchronous endpoints and function calls. Ceremony is kept to a minimum. Third party libraries are avoided to minimise attack surface.
- Service layer is implemented using dependency injection.
- No POJO/POCO, mappers or repositories are used in this application. Responses are served using into appropriate response classes. The application is designed to be easily extensible.
- Schemaless flexible design and type safety both are considered in this application. Nested objects and suitable datatypes are used to best model the documents.
- Both Read and Write Concern are `majority` throughout the application.
- Read Preference is `secondary` for `city` collection as the data updates to this collection are less frequent.
- Logging and error handling is implemented. Use of functional Result type is encouraged.
- Atlas used for database and change stream capabilities. Change stream are used to track changes for `planes` collection and update nested statistics document.
- Calculation of total distance travelled by a plane, total airtime for a plane, distance since last maintenance and maintenance requirement are tracked in the using change stream.
- Builder pattern is used to generate queries which are compile time checked for correctness ensuring confidence in the application refactoring.
- Indexes are created for collections to improve performance. GeoSpatial indexes are created for `cities` and `planes` collections at application startup.
- Aggregations include `metadata` which holds schema type and version, these are populated when a collection is created. Some fields like `departed` are populated while creating `planes` collection to save computations.

```
createdAt: new Date(),
metadata: {
    schemaType: 'plane',
    schemaVersion: '1.0',
}
```

## Future Work
- There are many ways to improve the application. For example the nearby cities can be pre-computed and stored in the database. Old data can be archived and frequently accessed data can be served via inmemory storage engine.
