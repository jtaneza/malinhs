using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MalikongkongNHS.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use raw SQL with IF NOT EXISTS so tables that already exist are safely skipped
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'GradeLevels', N'U') IS NULL
                BEGIN
                    CREATE TABLE [GradeLevels] (
                        [GradeLevelId] int NOT NULL IDENTITY,
                        [Name] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_GradeLevels] PRIMARY KEY ([GradeLevelId])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Sections', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Sections] (
                        [SectionId] int NOT NULL IDENTITY,
                        [SectionName] nvarchar(max) NOT NULL,
                        [GradeLevel] nvarchar(max) NULL,
                        [Adviser] nvarchar(max) NULL,
                        [Capacity] int NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_Sections] PRIMARY KEY ([SectionId])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Teachers', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Teachers] (
                        [TeacherId] int NOT NULL IDENTITY,
                        [SubjectId] int NULL,
                        [FullName] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Teachers] PRIMARY KEY ([TeacherId])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Users', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Users] (
                        [UserId] int NOT NULL IDENTITY,
                        [Username] nvarchar(max) NOT NULL,
                        [Password] nvarchar(max) NOT NULL,
                        [Role] nvarchar(max) NOT NULL,
                        CONSTRAINT [PK_Users] PRIMARY KEY ([UserId])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'Students', N'U') IS NULL
                BEGIN
                    CREATE TABLE [Students] (
                        [StudentId] int NOT NULL IDENTITY,
                        [StudentNo] nvarchar(max) NULL,
                        [LRN] nvarchar(max) NULL,
                        [FirstName] nvarchar(max) NULL,
                        [MiddleName] nvarchar(max) NULL,
                        [LastName] nvarchar(max) NULL,
                        [BirthDate] datetime2 NULL,
                        [Gender] nvarchar(max) NULL,
                        [Address] nvarchar(max) NULL,
                        [ContactNumber] nvarchar(max) NULL,
                        [GuardianName] nvarchar(max) NULL,
                        [SectionId] int NULL,
                        [IsActive] bit NOT NULL,
                        CONSTRAINT [PK_Students] PRIMARY KEY ([StudentId]),
                        CONSTRAINT [FK_Students_Sections_SectionId] FOREIGN KEY ([SectionId]) REFERENCES [Sections] ([SectionId])
                    );
                END
            ");

            migrationBuilder.Sql(@"
                IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Students_SectionId' AND object_id = OBJECT_ID(N'Students'))
                BEGIN
                    CREATE INDEX [IX_Students_SectionId] ON [Students] ([SectionId]);
                END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GradeLevels");

            migrationBuilder.DropTable(
                name: "Students");

            migrationBuilder.DropTable(
                name: "Teachers");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Sections");
        }
    }
}
