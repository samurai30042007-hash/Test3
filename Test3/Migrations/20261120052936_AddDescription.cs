using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Test3.Migrations
{
    /// <inheritdoc />
    public partial class AddDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<bool>(
                name: "iscompleted",
                schema: "public",
                table: "tasks",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValueSql: "false");

            migrationBuilder.AddColumn<string>(
                name: "description",
                schema: "public",
                table: "tasks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "description",
                schema: "public",
                table: "tasks");

            migrationBuilder.AlterColumn<bool>(
                name: "iscompleted",
                schema: "public",
                table: "tasks",
                type: "boolean",
                nullable: false,
                defaultValueSql: "false",
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);
        }
    }
}
