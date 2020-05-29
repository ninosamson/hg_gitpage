﻿//-------------------------------------------------------------------------
// Copyright © 2019 Province of British Columbia
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//-------------------------------------------------------------------------
// <auto-generated />
using Microsoft.EntityFrameworkCore.Migrations;

namespace HealthGateway.Database.Migrations
{
    public partial class PhoneBugfix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                schema: "gateway",
                table: "UserProfile",
                newName: "SMSNumber");
            migrationBuilder.AlterColumn<string>(
                 name: "SMSNumber",
                 schema: "gateway",
                 table: "UserProfile",
                 maxLength: 10,
                 nullable: true,
                 oldClrType: typeof(string),
                 oldType: "character varying(20)",
                 oldMaxLength: 20,
                 oldNullable: true);
            migrationBuilder.AddColumn<string>(
                name: "SMSNumber",
                schema: "gateway",
                table: "UserProfileHistory",
                maxLength: 10,
                nullable: true);
            
            string schema = "gateway";
            string triggerFunction = @$"
CREATE or REPLACE FUNCTION {schema}.""UserProfileHistoryFunction""()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    NOT LEAKPROOF
AS $BODY$
BEGIN
    IF(TG_OP = 'DELETE') THEN
        INSERT INTO {schema}.""UserProfileHistory""(""UserProfileHistoryId"", ""Operation"", ""OperationDateTime"",
                    ""UserProfileId"", ""AcceptedTermsOfService"", ""Email"", ""ClosedDateTime"", ""IdentityManagementId"",
                    ""EncryptionKey"", ""LastLoginDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"", ""SMSNumber"") 
		VALUES(uuid_generate_v4(), TG_OP, now(),
               old.""UserProfileId"", old.""AcceptedTermsOfService"", old.""Email"", old.""ClosedDateTime"", old.""IdentityManagementId"",
               old.""EncryptionKey"", old.""LastLoginDateTime"", old.""CreatedBy"", old.""CreatedDateTime"", old.""UpdatedBy"", old.""UpdatedDateTime"", old.""SMSNumber"");
        RETURN old;
    END IF;
END;$BODY$;";

            migrationBuilder.Sql(triggerFunction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "SMSNumber",
                schema: "gateway",
                newName: "PhoneNumber",
                table: "UserProfile");
            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "gateway",
                table: "UserProfile",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10,
                oldNullable: true
            );
            migrationBuilder.DropColumn(
                name: "SMSNumber",
                schema: "gateway",
                table: "UserProfileHistory");
            string schema = "gateway";
            string triggerFunction = @$"
CREATE or REPLACE FUNCTION {schema}.""UserProfileHistoryFunction""()
    RETURNS trigger
    LANGUAGE 'plpgsql'
    NOT LEAKPROOF
AS $BODY$
BEGIN
    IF(TG_OP = 'DELETE') THEN
        INSERT INTO {schema}.""UserProfileHistory""(""UserProfileHistoryId"", ""Operation"", ""OperationDateTime"",
                    ""UserProfileId"", ""AcceptedTermsOfService"", ""Email"", ""ClosedDateTime"", ""IdentityManagementId"",
                    ""EncryptionKey"", ""LastLoginDateTime"", ""CreatedBy"", ""CreatedDateTime"", ""UpdatedBy"", ""UpdatedDateTime"") 
		VALUES(uuid_generate_v4(), TG_OP, now(),
               old.""UserProfileId"", old.""AcceptedTermsOfService"", old.""Email"", old.""ClosedDateTime"", old.""IdentityManagementId"",
               old.""EncryptionKey"", old.""LastLoginDateTime"", old.""CreatedBy"", old.""CreatedDateTime"", old.""UpdatedBy"", old.""UpdatedDateTime"");
        RETURN old;
    END IF;
END;$BODY$;";

            migrationBuilder.Sql(triggerFunction);
        }
    }
}
