using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using ComicDownWpf.viewmodel;
using System.Collections.ObjectModel;
using System.Data.Common;

namespace ComicDownWpf.Utils
{
    class ColumnInfo
    {
        public string ColumnName { get; set; }
        public string Type { get; set; }
    }

    class SqliteHelper
    {
        private SQLiteConnection m_connect;

        public SqliteHelper(string path)
        {
            CreateOrOpenDataBase(path);
        }

        private SQLiteConnection CreateOrOpenDataBase(string path)//建立数据库
        {
            m_connect = new SQLiteConnection("data source=" + path);
            m_connect.Open();
            return m_connect;
        }

        public void CreateTable(string tableName, List<ColumnInfo> list)
        {
            string sql = "create table " + tableName + " (";
            string info = "";
            int pos = 0;
            int count = list.Count;


            foreach (var i in list)
            {
                info = info + i.ColumnName + " " + i.Type;
                if (pos + 1 < count)
                {
                    info = info + ",";
                }
                pos++;
            }

            sql = sql + info + ")";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();//执行建立表
        }

        public void DeleteOneRow(string tableName, string columnName, string columnValue, string hrefColumn, string href)
        {
            string sql = string.Format("delete from {0} where {1} = '{2}' and {3} = '{4}'", tableName, columnName, columnValue, hrefColumn, href);
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
        }

        public void DeleteTable(string tableName)
        {
            string sql = "drop table " + tableName;
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
        }

        public object MakeHistoryList(string tableName)
        {
            var list = new viewmodel.ComicBookList();
            list.List = new List<viewmodel.ComicBook>();
            string commandText = "select * from " + tableName;
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var book = new viewmodel.ComicBook();
                book.ComicName = reader[viewmodel.ComicBookShelf.comicName].ToString();
                book.ComicHref = reader[viewmodel.ComicBookShelf.urlName].ToString();
                book.ImgUrl = reader[viewmodel.ComicBookShelf.imgUrlName].ToString();
                book.CodeName = reader[viewmodel.ComicBookShelf.codeName].ToString();
                book.CharpterUrl = reader[viewmodel.ComicBookShelf.charpterUrlName].ToString();
                book.LastReadPage = Convert.ToInt32(reader[viewmodel.ComicBookShelf.lastReadPage].ToString());
                book.LastReadTime = Convert.ToUInt64(reader[viewmodel.ComicBookShelf.lastReadTime].ToString());
                list.List.Add(book);
            }

            return list;
        }

        public object MakeBooKList(string tableName)
        {
            var list = new viewmodel.ComicBookList();
            list.List = new List<viewmodel.ComicBook>();
            string commandText = "select * from " + tableName;
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var book = new viewmodel.ComicBook();            
                book.ComicName= reader[viewmodel.ComicBookShelf.comicName].ToString();
                book.ComicHref = reader[viewmodel.ComicBookShelf.urlName].ToString();
                book.ImgUrl = reader[viewmodel.ComicBookShelf.imgUrlName].ToString();
                book.CodeName = reader[viewmodel.ComicBookShelf.codeName].ToString();

                if(reader.FieldCount > 4)
                {
                    book.CharpterUrl = reader[viewmodel.ComicBookShelf.charpterUrlName].ToString();
                }
                
                list.List.Add(book);
            }
            return list;
        }

        public T GetAllBook<T>(string tableName)
        {
            Type t = typeof(T);

            if(t.FullName == "ComicDownWpf.viewmodel.ComicBookList")
            {
                return (T)MakeBooKList(tableName);
            }

            object o = new object();
            return (T)o;
        }

        public T GetHistory<T>(string tableName)
        {
            Type t = typeof(T);

            if (t.FullName == "ComicDownWpf.viewmodel.ComicBookList")
            {
                return (T)MakeHistoryList(tableName);
            }

            object o = new object();
            return (T)o;
        }

        public ObservableCollection<DownTaskRecord> GetDownAllRecord()
        {
            var list = new ObservableCollection<DownTaskRecord>();
            string commandText = "select * from " + ComicBookShelf.downLoadTable;
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var record = new viewmodel.DownTaskRecord();
                record.ComicName = reader[viewmodel.ComicBookShelf.comicName].ToString();
                record.CodeName = reader[viewmodel.ComicBookShelf.codeName].ToString();
                record.PageNum = Convert.ToInt32(reader[viewmodel.ComicBookShelf.totalPageName].ToString());
                record.Href = reader[viewmodel.ComicBookShelf.urlName].ToString();
                record.CurrentProgress = Convert.ToInt32(reader[viewmodel.ComicBookShelf.curDownPageName].ToString());
                record.SavePath = reader[viewmodel.ComicBookShelf.savePathName].ToString();
                record.ImageUrl = reader[viewmodel.ComicBookShelf.imgUrlName].ToString();
                record.ChapterName = reader[viewmodel.ComicBookShelf.charpterName].ToString();
                record.ComicInfoUrl = reader[viewmodel.ComicBookShelf.charpterUrlName].ToString();

                if (record.CurrentProgress == record.PageNum)
                {
                    record.DownCompleted = true;
                }

                list.Insert(0, record);
            }
            return list;
        }

        public ComicBook GetOneHistory(string tableName, string comicName, string codeName)
        {
            string commandText = string.Format("select * from {0} where {1} = '{2}' and {3} = '{4}'", tableName, ComicBookShelf.comicName,comicName, ComicBookShelf.codeName,codeName);
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var book = new viewmodel.ComicBook();
                book.ComicName = reader[viewmodel.ComicBookShelf.comicName].ToString();
                book.ComicHref = reader[viewmodel.ComicBookShelf.urlName].ToString();
                book.ImgUrl = reader[viewmodel.ComicBookShelf.imgUrlName].ToString();
                book.CodeName = reader[viewmodel.ComicBookShelf.codeName].ToString();
                book.CharpterUrl = reader[viewmodel.ComicBookShelf.charpterUrlName].ToString();
                book.LastReadPage = Convert.ToInt32(reader[viewmodel.ComicBookShelf.lastReadPage].ToString());
                book.LastReadTime = Convert.ToUInt64(reader[viewmodel.ComicBookShelf.lastReadTime].ToString());
                return book;
            }
            return null;
        }

        public void InsertData(string tableName, Dictionary<string, string> dict)
        {
            string sql = "insert into " + tableName + " ";
            string key = "";
            string val = "";
            int pos = 0;
            int count = dict.Count;

            foreach (var i in dict)
            {
                key +=  "'" + i.Key + "'";
                val += "'" + i.Value + "'";

                if (pos + 1 < count)
                {
                    key = key + ",";
                    val = val + ",";
                }
                pos++;

            }

            sql += "(" + key + ")";
            sql += " values " + " (" + val + ")";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
        }

        public void UpdateData(string tableName, Dictionary<string, string> setDict, Dictionary<string, string> columnDict)
        {
            string sql = "update " + tableName + " set ";
            int pos = 0;
            int count = setDict.Count;

            foreach (var i in setDict)
            {
                sql = sql + i.Key  + " = "  + "'" + i.Value + "'";

                if (pos + 1 < count)
                {
                    sql += ",";
                }
                pos++;

            }

            sql += " where ";
            pos = 0;
            count = columnDict.Count;

            foreach (var i in columnDict)
            {
                sql += i.Key + " = " + "'" + i.Value + "'";

                if (pos + 1 < count)
                {
                    sql += " and ";
                }

                pos++;
            }

            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            command.ExecuteNonQuery();
        }

        public bool IsHasTable(string tableName)//判断是否含有该表
        {
            bool isHas = false;
            string sql = "SELECT COUNT(*) FROM sqlite_master where type='table' and name=" + "'" + tableName + "'";
            SQLiteCommand command = new SQLiteCommand(sql, m_connect);
            int num = Convert.ToInt32(command.ExecuteScalar());

            if (num > 0)
            {
                isHas = true;
            }
            else
            {
                isHas = false;
            }
            return isHas;
        }

        public bool IfHasRecord(string tableName, Dictionary<string, string> dict)
        {
            string sql = "select ";
            List<string> list = new List<string>();

            foreach(var i in dict)
            {
                sql += i.Key + ",";
                list.Add(i.Key);
            }

            sql = sql.Substring(0, sql.LastIndexOf(","));

            sql += " from " + tableName;
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = sql;
            var reader = command.ExecuteReader();

            while(reader.Read())
            {
                bool isFind = true;
                foreach(var i in list)
                {
                    if (reader[i].ToString() != dict[i])
                    {
                        isFind = false;
                        break;
                    }
                }

                if(isFind)
                {
                    return true;
                }
               
            }

            return false;
        }
  
        public bool IfHasRecord(string tableName, string column, string columnValue)
        {
            string commandText = string.Format("select {0} from {1}", column, tableName);
            SQLiteCommand command = m_connect.CreateCommand();
            command.CommandText = commandText;
            var reader = command.ExecuteReader();

            while (reader.Read())
            {
               if(reader[column].ToString() == columnValue)
               {
                    return true;
               }
            }

            return false;
        }

        public void Close()
        {
            m_connect.Close();
        }

    }
}
