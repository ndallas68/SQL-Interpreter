using Mongoose.IDO;
using Mongoose.IDO.DataAccess;
using System;
using System.Data;

namespace ue_STY_SQLInterpreter
{
    [IDOExtensionClass("ue_STY_SQLInterpreter")]
    public class ue_STY_SQLInterpreter : IDOExtensionClass
    {
        [IDOMethod(MethodFlags.CustomLoad)]
        public DataTable STY_ExecuteAndReturnTable(string sql)
        {
            IDataReader reader;
            DataTable outTable = new DataTable();
            DataSet outSet = new DataSet();
            sql = sql.ToLower();
            if (sql.Contains("insert") || sql.Contains("delete") || sql.Contains("update"))
            {
                outTable.Columns.Add("Error");
                outTable.Rows.Add("Cannot Insert, Delete, or Update");
                return outTable;
            }

            AppDB appDB = IDORuntime.Context.CreateAppDB();

            try
            {
                using (IDbCommand cmd = appDB.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;
                    reader = appDB.ExecuteReader(cmd);
                    outTable = ConvertReaderToDataTable(reader);
                }
            }
            catch (Exception ex)
            {
                outTable.Columns.Add("Error");
                outTable.Rows.Add(ex.Message + " - " + ex.StackTrace);
            }
            finally
            {
                appDB.Dispose();
            }
            return outTable;
        }

        public DataTable ConvertReaderToDataTable(IDataReader reader)
        {
            DataTable outTable = new DataTable();
            int countInFields = reader.FieldCount;
            for (int i = 0; i < countInFields; i++)
            {
                outTable.Columns.Add(reader.GetName(i), reader.GetFieldType(i));
            }

            outTable.BeginLoadData();
            Object [] objectValues = new object[countInFields];
            while (reader.Read())
            {
                reader.GetValues(objectValues);
                outTable.LoadDataRow(objectValues, true);
            }
            reader.Close();
            outTable.EndLoadData();
            return outTable;
        }
    }
}