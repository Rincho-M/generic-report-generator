# Generic Report Generator
Generic project to get me going and try some new stuff. It's more infrastructure oriented so I intentionally overengineered things here and there.

The project showcases:
- External cache communication *(Redis)*
- Service-To-Service message bus communication *(RabbitMq)*
- External API integration *(Weather API)*
- Asyncronus long time request processing *(Report generation)*
- Blob data storage and access *(Report files)*
- Multi-service app containerization *(Docker)*
- Container orchestration *(Docker Compose)*

## Migrations

Migrations are managed through `Microsoft.EntityFrameworkCore.Tools` package.
The recommended way is to use Package Manager Console in Visual Studio.

### Create

To create a migration use this command:

`Add-Migration -Name #DescriptiveName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context ApiDbContext`

*Replace #DescriptiveName# with actual migration name.*

### Remove
To remove not applied migration use this command:

`Remove-Migration -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context ApiDbContext`

*To remove already applied migration, you need to revert it in target database*

### Revert
To revert already applied migration use this command:

`Update-Database #TargetMigrationName# -Project GenericReportGenerator.Migrations -StartupProject GenericReportGenerator.Migrations -Context ApiDbContext`

*Replace #TargetMigrationName# with migration name you want to go to.*

### Apply

Migrations are applied by running `GenericReportGenerator.Migrations` project. In `appsettings.json` the project must have `ConnectionStrings:Database` pointing to a desired database.