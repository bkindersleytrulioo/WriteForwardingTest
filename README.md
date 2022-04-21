## Database Setup

Steps from these AWS pages

* [Global Database](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/aurora-global-database-getting-started.html#aurora-global-database-creating)
   * Aurora MySQL 3.01.0 (compatible with MySQL 8.0.23)
   * db.r5.large
   * security group will all aurora & RDP inbound
   * default VPC and Subnet groups
   * default cluster & db parameter groups (default.aurora-mysql8.0)
   * perf insights, monitoring, deletion protection off
* [Secondary Region & Write Forwarding](https://docs.aws.amazon.com/AmazonRDS/latest/AuroraUserGuide/aurora-global-database-getting-started.html#aurora-global-database-attaching)
   * db.r5.large
   * default security group
   * default VPC and Subnet groups
   * default cluster & db parameter groups (default.aurora-mysql8.0)
   * perf insights, monitoring, deletion protection off

In the primary region, use a connection string like

`password={master_password};User Id={master_username};server={primary_writer_endpoint};port={cluster_port};charset=utf8;`

and run the application with option 's' to create schema and user. Alternatively, refer to the SQL in `SchemaCreator.cs` and your preferred method to run it in the primary region.

## Running the application

Build self contained executable:

```
dotnet publish --configuration Release -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:CopyOutputSymbolsToPublishDirectory=false -p:PublishReadyToRunShowWarnings=true -p:RuntimeIdentifier=win-x64 --self-contained --ignore-failed-sources
```

This should produce `WriteForwardingTest.exe` and `appsettings.json` in the `bin\Release\net6.0\win-x64\publish` directory.

(Or use the ones included in the repo.)


Launch Windows Server 2019 Base, t2.micro EC2 instance.

* Default VPC, Subnet
* Same security group as db

RDP to EC2 instance and copy `exe` and appsettings file to it. Modify `appsettings.json` entries; set `ConnectionString` like

```
`password=data123!#;User Id=test_user;server={secondary_reader_endpoint};port={cluster_port};database=test_db;charset=utf8;`
```

Set AuroraReadConsistency to one of `eventual`, `session`, `global`.

Run the `exe` using command line.

