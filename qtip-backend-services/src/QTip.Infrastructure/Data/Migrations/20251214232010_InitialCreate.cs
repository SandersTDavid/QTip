using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QTip.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "classified_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    NormalizedValue = table.Column<string>(type: "text", nullable: false),
                    RawValue = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_classified_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenizedText = table.Column<string>(type: "text", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "submission_classifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubmissionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassifiedItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartIndex = table.Column<int>(type: "integer", nullable: false),
                    Length = table.Column<int>(type: "integer", nullable: false),
                    CreatedAtUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_submission_classifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_submission_classifications_classified_items_ClassifiedItemId",
                        column: x => x.ClassifiedItemId,
                        principalTable: "classified_items",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_submission_classifications_submissions_SubmissionId",
                        column: x => x.SubmissionId,
                        principalTable: "submissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_classified_items_Tag_NormalizedValue",
                table: "classified_items",
                columns: new[] { "Tag", "NormalizedValue" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_classified_items_Token",
                table: "classified_items",
                column: "Token",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_submission_classifications_ClassifiedItemId",
                table: "submission_classifications",
                column: "ClassifiedItemId");

            migrationBuilder.CreateIndex(
                name: "IX_submission_classifications_SubmissionId",
                table: "submission_classifications",
                column: "SubmissionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "submission_classifications");

            migrationBuilder.DropTable(
                name: "classified_items");

            migrationBuilder.DropTable(
                name: "submissions");
        }
    }
}
