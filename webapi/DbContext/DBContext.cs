using System.Data;
using System.Data.SqlClient;

namespace webapi.DBContext {
     public class DBContext {
          private SqlConnection sqlConnection;
          public DBContext(string connectionString) {
               sqlConnection = new SqlConnection(connectionString);
          }
          
          public SqlConnection Connection {get => sqlConnection;}

          public SqlConnection OpenConnection() {
               if(sqlConnection.State != ConnectionState.Open) {
                    sqlConnection.Open();
               }
               return sqlConnection;
          }

          public SqlConnection CloseConnection() {
               if(sqlConnection.State != ConnectionState.Closed) {
                    sqlConnection.Close();
               }
               return sqlConnection;
          }
     }

}