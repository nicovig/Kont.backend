using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kont.backend.Migrations
{
    /// <inheritdoc />
    public partial class Addmodels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "player",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    player_type = table.Column<string>(type: "text", nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    role_type = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "site",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    address = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    city = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    zip_code = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    country = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    state = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    logo = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_site", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subscription",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    subscription_type = table.Column<string>(type: "text", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subscription", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "event",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    event_link = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_event", x => x.id);
                    table.ForeignKey(
                        name: "fk_event_site_site_id",
                        column: x => x.site_id,
                        principalTable: "site",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "administrator",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: false),
                    subscription_id = table.Column<Guid>(type: "uuid", nullable: false),
                    manager_id = table.Column<Guid>(type: "uuid", nullable: true),
                    role_id = table.Column<Guid>(type: "uuid", nullable: false),
                    firstname = table.Column<string>(type: "text", nullable: false),
                    lastname = table.Column<string>(type: "text", nullable: false),
                    password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administrator", x => x.id);
                    table.ForeignKey(
                        name: "fk_administrator_administrator_manager_id",
                        column: x => x.manager_id,
                        principalTable: "administrator",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_administrator_role_role_id",
                        column: x => x.role_id,
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_administrator_subscription_subscription_id",
                        column: x => x.subscription_id,
                        principalTable: "subscription",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pool",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    qr_code = table.Column<string>(type: "text", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_all_players_present = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_pool", x => x.id);
                    table.ForeignKey(
                        name: "fk_pool_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    site_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    event_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_administrator_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "administrator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_event_event_id",
                        column: x => x.event_id,
                        principalTable: "event",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_activity_site_site_id",
                        column: x => x.site_id,
                        principalTable: "site",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "administrator_site",
                columns: table => new
                {
                    administrators_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sites_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_administrator_site", x => new { x.administrators_id, x.sites_id });
                    table.ForeignKey(
                        name: "fk_administrator_site_administrator_administrators_id",
                        column: x => x.administrators_id,
                        principalTable: "administrator",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_administrator_site_site_sites_id",
                        column: x => x.sites_id,
                        principalTable: "site",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_global_score",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_score = table.Column<double>(type: "double precision", nullable: false),
                    percentage = table.Column<double>(type: "double precision", nullable: false),
                    global_rank = table.Column<int>(type: "integer", nullable: false),
                    activities_played = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    player_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_global_score", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_global_score_player_player_entity_id",
                        column: x => x.player_entity_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_player_global_score_pool_pool_entity_id",
                        column: x => x.pool_entity_id,
                        principalTable: "pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "activity_summary_data",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    players_played = table.Column<int>(type: "integer", nullable: false),
                    best_score = table.Column<double>(type: "double precision", nullable: false),
                    average_score = table.Column<double>(type: "double precision", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    activity_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity_summary_data", x => x.id);
                    table.ForeignKey(
                        name: "fk_activity_summary_data_activity_activity_entity_id",
                        column: x => x.activity_entity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_activity_summary_data_pool_pool_entity_id",
                        column: x => x.pool_entity_id,
                        principalTable: "pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "game_session",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<string>(type: "text", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ended_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_game_session", x => x.id);
                    table.ForeignKey(
                        name: "fk_game_session_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_game_session_pool_pool_id",
                        column: x => x.pool_id,
                        principalTable: "pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_activity_score",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_score = table.Column<double>(type: "double precision", nullable: false),
                    percentage = table.Column<double>(type: "double precision", nullable: false),
                    rank = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    player_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_entity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_entity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_activity_score", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_activity_score_activity_activity_entity_id",
                        column: x => x.activity_entity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_player_activity_score_player_player_entity_id",
                        column: x => x.player_entity_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_player_activity_score_pool_pool_entity_id",
                        column: x => x.pool_entity_id,
                        principalTable: "pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "scoring_metric",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    unit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    higher_is_better = table.Column<bool>(type: "boolean", nullable: false),
                    coefficient = table.Column<double>(type: "double precision", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_scoring_metric", x => x.id);
                    table.ForeignKey(
                        name: "fk_scoring_metric_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_group",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    game_session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_number = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_group", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_group_game_session_game_session_id",
                        column: x => x.game_session_id,
                        principalTable: "game_session",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_score",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    scoring_metric_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<double>(type: "double precision", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_score", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_score_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_player_score_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_player_score_scoring_metric_scoring_metric_id",
                        column: x => x.scoring_metric_id,
                        principalTable: "scoring_metric",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "group_score",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    group_id = table.Column<Guid>(type: "uuid", nullable: false),
                    activity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    total_score = table.Column<double>(type: "double precision", nullable: false),
                    average_score = table.Column<double>(type: "double precision", nullable: false),
                    player_count = table.Column<int>(type: "integer", nullable: false),
                    calculated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_score", x => x.id);
                    table.ForeignKey(
                        name: "fk_group_score_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_group_score_player_group_group_id",
                        column: x => x.group_id,
                        principalTable: "player_group",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "player_registration",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pool_id = table.Column<Guid>(type: "uuid", nullable: false),
                    registered_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    checked_in_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    player_group_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_player_registration", x => x.id);
                    table.ForeignKey(
                        name: "fk_player_registration_player_group_player_group_id",
                        column: x => x.player_group_id,
                        principalTable: "player_group",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_player_registration_player_player_id",
                        column: x => x.player_id,
                        principalTable: "player",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_player_registration_pool_pool_id",
                        column: x => x.pool_id,
                        principalTable: "pool",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_created_by_id",
                table: "activity",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_event_id",
                table: "activity",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_site_id",
                table: "activity",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_summary_data_activity_entity_id",
                table: "activity_summary_data",
                column: "activity_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_activity_summary_data_pool_entity_id",
                table: "activity_summary_data",
                column: "pool_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_administrator_email",
                table: "administrator",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_administrator_manager_id",
                table: "administrator",
                column: "manager_id");

            migrationBuilder.CreateIndex(
                name: "ix_administrator_role_id",
                table: "administrator",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "ix_administrator_subscription_id",
                table: "administrator",
                column: "subscription_id");

            migrationBuilder.CreateIndex(
                name: "ix_administrator_site_sites_id",
                table: "administrator_site",
                column: "sites_id");

            migrationBuilder.CreateIndex(
                name: "ix_event_site_id",
                table: "event",
                column: "site_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_activity_id",
                table: "game_session",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_game_session_pool_id",
                table: "game_session",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_score_activity_id",
                table: "group_score",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_group_score_group_id",
                table: "group_score",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_email",
                table: "player",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_username",
                table: "player",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_player_activity_score_activity_entity_id",
                table: "player_activity_score",
                column: "activity_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_activity_score_player_entity_id",
                table: "player_activity_score",
                column: "player_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_activity_score_pool_entity_id",
                table: "player_activity_score",
                column: "pool_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_global_score_player_entity_id",
                table: "player_global_score",
                column: "player_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_global_score_pool_entity_id",
                table: "player_global_score",
                column: "pool_entity_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_group_game_session_id",
                table: "player_group",
                column: "game_session_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_registration_player_group_id",
                table: "player_registration",
                column: "player_group_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_registration_player_id",
                table: "player_registration",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_registration_pool_id",
                table: "player_registration",
                column: "pool_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_score_activity_id",
                table: "player_score",
                column: "activity_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_score_player_id",
                table: "player_score",
                column: "player_id");

            migrationBuilder.CreateIndex(
                name: "ix_player_score_scoring_metric_id",
                table: "player_score",
                column: "scoring_metric_id");

            migrationBuilder.CreateIndex(
                name: "ix_pool_event_id",
                table: "pool",
                column: "event_id");

            migrationBuilder.CreateIndex(
                name: "ix_scoring_metric_activity_id",
                table: "scoring_metric",
                column: "activity_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity_summary_data");

            migrationBuilder.DropTable(
                name: "administrator_site");

            migrationBuilder.DropTable(
                name: "group_score");

            migrationBuilder.DropTable(
                name: "player_activity_score");

            migrationBuilder.DropTable(
                name: "player_global_score");

            migrationBuilder.DropTable(
                name: "player_registration");

            migrationBuilder.DropTable(
                name: "player_score");

            migrationBuilder.DropTable(
                name: "player_group");

            migrationBuilder.DropTable(
                name: "player");

            migrationBuilder.DropTable(
                name: "scoring_metric");

            migrationBuilder.DropTable(
                name: "game_session");

            migrationBuilder.DropTable(
                name: "activity");

            migrationBuilder.DropTable(
                name: "pool");

            migrationBuilder.DropTable(
                name: "administrator");

            migrationBuilder.DropTable(
                name: "event");

            migrationBuilder.DropTable(
                name: "role");

            migrationBuilder.DropTable(
                name: "subscription");

            migrationBuilder.DropTable(
                name: "site");
        }
    }
}
