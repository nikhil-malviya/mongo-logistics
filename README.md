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
  country: '$cities15.country'
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
    previousLanded: '$_id'
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
  previousLanded: '$planes.previousLanded',
  heading: {
   $literal: 0
  },
  route: []
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
## Development Notes
- Open API integration [Swagger](http://localhost:5000/swagger/index.html) and static file serving
- Configuration and constants used where appropriate
- Asynchronous endpoints and function calls. Ceremony is kept to a minimum. Third party libraries are avoided to minimise attack surface.
- Service layer is implemented using dependency injection
- No mappers or repositories are used in this application. Responses are shaped into appropriate datatypes using custom JSON serializers (e.g.GeoJson2DCoordinates is serialized to double array) All models closely resemble the database documents. If required separate business models can be created. The application is designed to be easily extensible.
- Type safety is enforced in this application. Nested objects and suitable datatypes are used to best model the documents (e.g. coordinates are not double arrays which can have more than two values)
- Logging and error handling is implemented. Use of functional Result type is encouraged.
- Atlas used for database and change stream capabilities. Change stream are used to track changes for `planes` collection and update nested statistics document
- Builder pattern is used to generate queries which are compile time checked for correctness ensuring confidence in the application refactoring.
- Indexes are created for collections to improve performance.
-	Aggregations include previousLanded field populated while creating `planes` collection to save computations.

## Future Work
- There are many ways to improve the application. For example the nearby cities can be pre-computed and stored in the database. Old data can be archived and frequently accessed data can be served via inmemory storage engine.
