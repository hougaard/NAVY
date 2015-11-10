using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NAVYlib
{
    public enum CodeunitMode
    {
        Backup,
        Restore
    }
    public class Operation
    {
        public int TableNo { get; set; }
        public int FieldNo { get; set; }
    }
    public class Codeunit
    {
        CodeunitMode mode;
        public Codeunit(CodeunitMode mode)
        {
            this.mode = mode;
            Operations = new List<Operation>();
        }
        List<Operation> Operations;
        public void AddOperation(int TableNo, int FieldNo)
        {
            Operations.Add(new Operation { TableNo = TableNo, FieldNo = FieldNo });
        }
        public string ExportCode(string ObjectNoForBackup)
        {
            string InsertedCode = "";
            if (mode == CodeunitMode.Backup)
                InsertedCode += "            Data.DELETEALL;\r\n";
            foreach (Operation o in Operations)
            {
                if (mode == CodeunitMode.Backup)
                {
                    InsertedCode += "            BackupData(" +
                        o.TableNo.ToString(CultureInfo.InvariantCulture) +
                        "," +
                        o.FieldNo.ToString(CultureInfo.InvariantCulture) +
                        ");\r\n";
                }
                else
                {
                    InsertedCode += "            RestoreData(" +
                        o.TableNo.ToString(CultureInfo.InvariantCulture) +
                        "," +
                        o.FieldNo.ToString(CultureInfo.InvariantCulture) +
                        ");\r\n";

                }
            }
            InsertedCode += "            SuccessFile.WriteAllText('"+ Directory.GetCurrentDirectory()+"\\NAVY.success','SUCCESS');\r\n";
            return CodeunitSource.code.Replace("            #", InsertedCode).Replace("50000",ObjectNoForBackup);
        }
    }
    static class CodeunitSource
    {
        public static string code =
@"OBJECT Codeunit 50000 NAVY Tools
{
  OBJECT-PROPERTIES
  {
    Date=03-11-15;
    Time=13:10:29;
    Modified=Yes;
    Version List=NAVY;
  }
  PROPERTIES
  {
    OnRun=VAR
            Data@1160530000 : Record 50000;
            SuccessFile@1160530001 : DotNet " + "\"" + @"'mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089'.System.IO.File" + "\"" + @"RUNONCLIENT;
          BEGIN
            #
          END;

  }
  CODE
  {

    LOCAL PROCEDURE BackupData@1160530000(TableNo@1160530000 : Integer;FieldNo@1160530001 : Integer);
    VAR
      Company@1160530006 : Record 2000000006;
      Objects@1160530005 : Record 2000000038;
      RecRef@1160530004 : RecordRef;
      Field@1160530003 : FieldRef;
    BEGIN
      Company.FINDSET;
      REPEAT
        RecRef.OPEN(TableNo,FALSE,Company.Name);
        IF RecRef.FINDSET THEN
          REPEAT
            Field := RecRef.FIELD(FieldNo);
            IF FORMAT(Field.CLASS) <> 'FlowField' THEN
              BackupField(Company.Name,TableNo,FieldNo,RecRef,Field);
          UNTIL RecRef.NEXT = 0;
        RecRef.CLOSE;
      UNTIL Company.NEXT = 0;
    END;

    LOCAL PROCEDURE RestoreData@1160530001(TableNo@1160530001 : Integer;FieldNo@1160530000 : Integer);
    VAR
      Company@1160530005 : Record 2000000006;
      Objects@1160530004 : Record 2000000038;
      RecRef@1160530003 : RecordRef;
      Field@1160530002 : FieldRef;
    BEGIN
      Company.FINDSET;
      REPEAT
        RecRef.OPEN(TableNo,FALSE,Company.Name);
        IF RecRef.FINDSET THEN
          REPEAT
            Field := RecRef.FIELD(FieldNo);
            IF FORMAT(Field.CLASS) <> 'FlowField' THEN
              RestoreField(Company.Name,TableNo,FieldNo,RecRef,Field);
          UNTIL RecRef.NEXT = 0;
        RecRef.CLOSE;
      UNTIL Company.NEXT = 0;
    END;

    LOCAL PROCEDURE BackupField@1160530002(CompanyName@1160530004 : Text;TableNo@1160530002 : Integer;FieldNo@1160530003 : Integer;RecRef@1160530005 : RecordRef;FieldRef@1160530000 : FieldRef);
    VAR
      Data@1160530001 : Record 50000;
    BEGIN
      Data.INIT;
      Data.Company := CompanyName;
      Data.TableNo := TableNo;
      Data.REC := RecRef.RECORDID;
      Data.FieldNo := FieldNo;
      CASE FORMAT(FieldRef.TYPE) OF
        'Code',
        'Text' : Data.String := FieldRef.VALUE;
        'Decimal' : Data.Decimal := FieldRef.VALUE;
        'Integer' : Data.Integer := FieldRef.VALUE;
        'DateTime' : Data.DataTime := FieldRef.VALUE;
        'Date' : Data.Date := FieldRef.VALUE;
        'Time' : Data.Time := FieldRef.VALUE;
      END;
      Data.INSERT;
    END;

    LOCAL PROCEDURE RestoreField@1160530004(CompanyName@1160530004 : Text;TableNo@1160530002 : Integer;FieldNo@1160530003 : Integer;RecRef@1160530005 : RecordRef;FieldRef@1160530000 : FieldRef);
    VAR
      Data@1160530001 : Record 50000;
    BEGIN
      Data.SETRANGE(Company,CompanyName);
      Data.SETRANGE(TableNo,TableNo);
      Data.SETRANGE(REC,RecRef.RECORDID);
      Data.SETRANGE(FieldNo,FieldNo);
      IF Data.FINDFIRST THEN BEGIN
        CASE FORMAT(FieldRef.TYPE) OF
          'Code',
          'Text' : FieldRef.VALUE := Data.String;
          'Decimal' : FieldRef.VALUE := Data.Decimal;
          'Integer' : FieldRef.VALUE := Data.Integer;
          'DateTime' : FieldRef.VALUE := Data.DataTime;
          'Date' : FieldRef.VALUE := Data.Date;
          'Time' : FieldRef.VALUE := Data.Time;
        END;
        RecRef.MODIFY;
      END;
    END;

    BEGIN
    END.
  }
}


OBJECT Table 50000 NAVY Archived Data
{
  OBJECT-PROPERTIES
  {
    Date=03-11-15;
    Time=13:20:34;
    Modified=Yes;
    Version List=NAVY;
  }
  PROPERTIES
  {
    DataPerCompany=No;
  }
  FIELDS
  {
    { 1   ;   ;Company             ;Text30         }
    { 2   ;   ;TableNo             ;Integer        }
    { 3   ;   ;REC                 ;RecordID       }
    { 4   ;   ;FieldNo             ;Integer        }
    { 5   ;   ;String              ;Text250        }
    { 6   ;   ;Decimal             ;Decimal        }
    { 7   ;   ;Integer             ;Integer        }
    { 8   ;   ;DataTime            ;DateTime       }
    { 9   ;   ;Date                ;Date           }
    { 10  ;   ;Time                ;Time           }
  }
  KEYS
  {
    {    ;Company,TableNo,REC,FieldNo             ;Clustered=Yes }
  }
  FIELDGROUPS
  {
  }
  CODE
  {

    BEGIN
    END.
  }
}
";
    }
}
