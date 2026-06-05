using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace YourCompany.EFCore.PostgreSQL.CustomConfigurationMigrations
{
    /// <inheritdoc />
    public partial class ItsAlive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
<<<<<<< HEAD
                name: "eodehsotric_Entity",
=======
                name: "e_con_e_h_m_r_Entity",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<int>(type: "integer", nullable: false),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<string>(type: "text", nullable: true),
                    Column4 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
<<<<<<< HEAD
                    table.PrimaryKey("eodehsotric_PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eodehsotric_ReusableEntity",
=======
                    table.PrimaryKey("e_con_e_h_m_r_PK_Entity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "e_con_e_h_m_r_ReusableEntity",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    WithCrossCuttingFeatureData = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
<<<<<<< HEAD
                    table.PrimaryKey("eodehsotric_PK_ReusableEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eodehsotric_EntityMixin",
=======
                    table.PrimaryKey("e_con_e_h_m_r_PK_ReusableEntity", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "e_con_e_h_m_r_EntityMixin",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    Column1 = table.Column<string>(type: "text", nullable: true),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<string>(type: "text", nullable: true),
                    Column4 = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
<<<<<<< HEAD
                    table.PrimaryKey("eodehsotric_PK_EntityMixin", x => x.EntityId);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityMixin_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "eodehsotric_Entity",
=======
                    table.PrimaryKey("e_con_e_h_m_r_PK_EntityMixin", x => x.EntityId);
                    table.ForeignKey(
                        name: "e_con_e_h_m_r_FK_EntityMixin_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "e_con_e_h_m_r_Entity",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
<<<<<<< HEAD
                name: "eodehsotric_EntityOwnedCollection",
=======
                name: "e_con_e_h_m_r_EntityOwnedCollection",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Column1 = table.Column<string>(type: "text", nullable: true),
                    Column2 = table.Column<string>(type: "text", nullable: true),
                    Column3 = table.Column<string>(type: "text", nullable: true),
                    Column4 = table.Column<string>(type: "text", nullable: true),
                    EntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
<<<<<<< HEAD
                    table.PrimaryKey("eodehsotric_PK_EntityOwnedCollection", x => x.Id);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityOwnedCollection_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "eodehsotric_Entity",
=======
                    table.PrimaryKey("e_con_e_h_m_r_PK_EntityOwnedCollection", x => x.Id);
                    table.ForeignKey(
                        name: "e_con_e_h_m_r_FK_EntityOwnedCollection_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "e_con_e_h_m_r_Entity",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
<<<<<<< HEAD
                name: "eodehsotric_EntityLinkToReusableEntity",
=======
                name: "e_con_e_h_m_r_EntityLinkToReusableEntity",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                columns: table => new
                {
                    EntityId = table.Column<int>(type: "integer", nullable: false),
                    ReusableEntityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
<<<<<<< HEAD
                    table.PrimaryKey("eodehsotric_PK_EntityLinkToReusableEntity", x => x.EntityId);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityLinkToReusableEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "eodehsotric_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "eodehsotric_FK_EntityLinkToReusableEntity_ReusableEntiCB4546B",
                        column: x => x.ReusableEntityId,
                        principalTable: "eodehsotric_ReusableEntity",
=======
                    table.PrimaryKey("e_con_e_h_m_r_PK_EntityLinkToReusableEntity", x => x.EntityId);
                    table.ForeignKey(
                        name: "e_con_e_h_m_r_FK_EntityLinkToReusableEntity_Entity_EntityId",
                        column: x => x.EntityId,
                        principalTable: "e_con_e_h_m_r_Entity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "e_con_e_h_m_r_FK_EntityLinkToReusableEntity_ReusableEntiCB4546B",
                        column: x => x.ReusableEntityId,
                        principalTable: "e_con_e_h_m_r_ReusableEntity",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
<<<<<<< HEAD
                name: "eodehsotric_IX_EntityLinkToReusableEntity_ReusableEntityId",
                table: "eodehsotric_EntityLinkToReusableEntity",
                column: "ReusableEntityId");

            migrationBuilder.CreateIndex(
                name: "eodehsotric_IX_EntityOwnedCollection_EntityId",
                table: "eodehsotric_EntityOwnedCollection",
=======
                name: "e_con_e_h_m_r_IX_EntityLinkToReusableEntity_ReusableEntityId",
                table: "e_con_e_h_m_r_EntityLinkToReusableEntity",
                column: "ReusableEntityId");

            migrationBuilder.CreateIndex(
                name: "e_con_e_h_m_r_IX_EntityOwnedCollection_EntityId",
                table: "e_con_e_h_m_r_EntityOwnedCollection",
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
                column: "EntityId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
<<<<<<< HEAD
                name: "eodehsotric_EntityLinkToReusableEntity");

            migrationBuilder.DropTable(
                name: "eodehsotric_EntityMixin");

            migrationBuilder.DropTable(
                name: "eodehsotric_EntityOwnedCollection");

            migrationBuilder.DropTable(
                name: "eodehsotric_ReusableEntity");

            migrationBuilder.DropTable(
                name: "eodehsotric_Entity");
=======
                name: "e_con_e_h_m_r_EntityLinkToReusableEntity");

            migrationBuilder.DropTable(
                name: "e_con_e_h_m_r_EntityMixin");

            migrationBuilder.DropTable(
                name: "e_con_e_h_m_r_EntityOwnedCollection");

            migrationBuilder.DropTable(
                name: "e_con_e_h_m_r_ReusableEntity");

            migrationBuilder.DropTable(
                name: "e_con_e_h_m_r_Entity");
>>>>>>> refs/rewritten/2-persistman--efcore-hosting-migration-run
        }
    }
}
