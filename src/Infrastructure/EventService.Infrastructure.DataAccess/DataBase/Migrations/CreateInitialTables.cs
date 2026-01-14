using FluentMigrator;

namespace EventService.Infrastructure.DataAccess.DataBase.Migrations;

[Migration(202601140001)]
public class CreateInitialTables : Migration
{
    public override void Up()
    {
        Create.Table("categories")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("name")
            .AsString(150)
            .NotNullable()
            .Unique();

        Create.Table("venues")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("name")
            .AsString(200)
            .NotNullable()
            .WithColumn("address")
            .AsString(400)
            .NotNullable();

        Create.Table("hall_schemes")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("name")
            .AsString(200)
            .NotNullable()
            .WithColumn("rows")
            .AsInt32()
            .NotNullable()
            .WithColumn("columns")
            .AsInt32()
            .NotNullable()
            .WithColumn("venue_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("venues", "id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);

        Create.Table("artists")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("name")
            .AsString(200)
            .NotNullable()
            .WithColumn("bio")
            .AsString(int.MaxValue)
            .Nullable();

        Create.Table("events")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("title")
            .AsString(200)
            .NotNullable()
            .WithColumn("description")
            .AsString(int.MaxValue)
            .Nullable()
            .WithColumn("start_date")
            .AsDateTime()
            .NotNullable()
            .WithColumn("end_date")
            .AsDateTime()
            .NotNullable()
            .WithColumn("category_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("categories", "id")
            .OnDeleteOrUpdate(System.Data.Rule.None)
            .WithColumn("venue_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("venues", "id")
            .OnDeleteOrUpdate(System.Data.Rule.None);

        Create.Table("event_artists")
            .WithColumn("event_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("events", "id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("artist_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("artists", "id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        Create.PrimaryKey("PK_event_artists").OnTable("event_artists").Columns("event_id", "artist_id");

        Create.Table("organizers")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("name")
            .AsString(200)
            .NotNullable();

        Create.Table("event_organizers")
            .WithColumn("id")
            .AsInt64()
            .PrimaryKey()
            .Identity()
            .WithColumn("event_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("events", "id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("organizer_id")
            .AsInt64()
            .NotNullable()
            .ForeignKey("organizers", "id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade);
        Create.UniqueConstraint("UQ_event_organizers").OnTable("event_organizers").Columns("event_id", "organizer_id");

        Create.Table("seats")
            .WithColumn("id").AsInt64().PrimaryKey().Identity()
            .WithColumn("hall_scheme_id").AsInt64().NotNullable()
            .ForeignKey("hall_schemes", "id")
            .OnDeleteOrUpdate(System.Data.Rule.Cascade)
            .WithColumn("row_number").AsInt32().NotNullable()
            .WithColumn("seat_number").AsInt32().NotNullable()
            .WithColumn("status").AsString(10).NotNullable().WithDefaultValue("Free");
        Create.UniqueConstraint("UQ_seats").OnTable("seats").Columns("hall_scheme_id", "row_number", "seat_number");
    }

    public override void Down()
    {
        Delete.Table("seats");
        Delete.Table("event_organizers");
        Delete.Table("organizers");
        Delete.Table("event_artists");
        Delete.Table("events");
        Delete.Table("artists");
        Delete.Table("hall_schemes");
        Delete.Table("venues");
        Delete.Table("categories");
    }
}