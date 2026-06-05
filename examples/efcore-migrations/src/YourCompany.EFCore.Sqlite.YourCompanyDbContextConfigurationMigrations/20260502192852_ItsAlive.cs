using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YourCompany.EFCore.Sqlite.YourCompanyDbContextConfigurationMigrations
{
    /// <inheritdoc />
    public partial class ItsAlive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_Entity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Column1 = table.Column<int>(type: "INTEGER", nullable: false),
                    Column2 = table.Column<string>(type: "TEXT", nullable: true),
                    Column3 = table.Column<string>(type: "TEXT", nullable: true),
                    Column4 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_ReusableEntity",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    WithCrossCuttingFeatureData = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_PK_ReusableEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityMixin",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    Column1 = table.Column<string>(type: "TEXT", nullable: true),
                    Column2 = table.Column<string>(type: "TEXT", nullable: true),
                    Column3 = table.Column<string>(type: "TEXT", nullable: true),
                    Column4 = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_PK_EntityMixin", x => x.EntityId);
                    table.ForeignKey(
                        name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_FK_EntityMixin_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityOwnedCollection",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Column1 = table.Column<string>(type: "TEXT", nullable: true),
                    Column2 = table.Column<string>(type: "TEXT", nullable: true),
                    Column3 = table.Column<string>(type: "TEXT", nullable: true),
                    Column4 = table.Column<string>(type: "TEXT", nullable: true),
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_PK_EntityOwnedCollection", x => x.Id);
                    table.ForeignKey(
                        name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_FK_EntityOwnedCollection_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityLinkToReusableEntity",
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "INTEGER", nullable: false),
                    ReusableEntityId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_PK_EntityLinkToReusableEntity", x => x.EntityId);
                    table.ForeignKey(
                        name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_FK_EntityLinkToReusableEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_FK_EntityLinkToReusableEntity_ReusableEntity_ReusableEntityId",
                        column: x => x.ReusableEntityId,
                        principalTable: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_ReusableEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_IX_EntityLinkToReusableEntity_ReusableEntityId",
                table: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityLinkToReusableEntity",
                column: "ReusableEntityId");

            migrationBuilder.CreateIndex(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_IX_EntityOwnedCollection_EntityId",
                table: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityOwnedCollection",
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityLinkToReusableEntity");

            migrationBuilder.DropTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityMixin");

            migrationBuilder.DropTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_EntityOwnedCollection");

            migrationBuilder.DropTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_ReusableEntity");

            migrationBuilder.DropTable(
                name: "examples_oltp_di_efcore_handle_specifications_only_the_rest_is_covered_Entity");
        }
    }
}
