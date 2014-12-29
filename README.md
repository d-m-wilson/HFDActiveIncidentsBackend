# HFDActiveIncidentsBackend
The backend components of a web application to visualize [active incidents data](https://github.com/d-m-wilson/HFDActiveIncidentsBackend/) from the [Houston Fire Department](http://houstontx.gov/fire/). A client-side-Javascript-driven web application that makes use of these components can be found [here](https://github.com/d-m-wilson/HFDActiveIncidentsWeb/).

* HFDActiveIncidentsDB - A SQL Server (SSDT) database project.
* HFDActiveIncidents.API - An ASP.NET Web API 2 application that exposes the database as a RESTful web service.
* HFDActiveIncidents.Domain - A class library containing EF entity models and other classes used by the API and Service projects.
* HFDActiveIncidents.Service - A custom ETL application implemented as a Windows Service. Retrieves data from the [HFD Active Incidents web service](http://data.ohouston.org/dataset/hfd-active-incidents), transforms and normalizes it, and stores it in a SQL Server database.

Copyright © 2014 [David M. Wilson](https://twitter.com/dmwilson_dev)  
Licensed under the [GNU GPL v3](https://github.com/d-m-wilson/HFDActiveIncidentsWeb/blob/master/LICENSE)
