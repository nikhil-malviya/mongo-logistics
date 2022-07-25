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


```json
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

```json
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