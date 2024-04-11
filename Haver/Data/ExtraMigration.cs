using Microsoft.EntityFrameworkCore.Migrations;

namespace Haver.Data
{
    public static class ExtraMigration
    {
        public static void Steps(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetNCRTimestampOnUpdate
                    AFTER UPDATE ON NCRs
                    BEGIN
                        UPDATE NCRs
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetNCRTimestampOnInsert
                    AFTER INSERT ON NCRs
                    BEGIN
                        UPDATE NCRs
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetEmployeeTimestampOnUpdate
                    AFTER UPDATE ON Employees
                    BEGIN
                        UPDATE Employees
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");
            migrationBuilder.Sql(
                @"
                    CREATE TRIGGER SetEmployeeNCRTimestampOnInsert
                    AFTER INSERT ON Employees
                    BEGIN
                        UPDATE Employees
                        SET RowVersion = randomblob(8)
                        WHERE rowid = NEW.rowid;
                    END
                ");

        }
    }
}
