//using System;
//using System.Collections.Generic;
//using System.ComponentModel;
//using System.Data;
//using System.Globalization;
//using System.Linq;
//using System.Text;
//using System.IO;

//public interface ILog
//{
//    void Info(string format, params object[] args);
//    bool IsInfoEnabled { get; }
//    void Debug(string format, params object[] args);
//    bool IsDebugEnabled { get; }
//    void Warn(string format, params object[] args);
//    bool IsWarnEnabled { get; }
//    void Fatal(string format, params object[] args);
//    bool IsFatalEnabled { get; }
//    void Error(Exception ex);
//    void Error(Exception ex, string format, params object[] args);
//    void Error(string format, params object[] args);
//    bool IsErrorEnabled { get; }
//    void Sql(object queryObject, Dictionary<string, object> parameters);
//    void Sql(IDbCommand cmd);
//    void Sql(double elapsedMilliseconds);
//    bool IsSqlEnabled { get; }

//    string GetSqlLog(object queryObject, Dictionary<string, object> parameters);
//    string GetSqlLog(IDbCommand cmd);
//}

//public enum LogType
//{
//    DEBUG,
//    SQL,
//    INFO,
//    WARN,
//    ERROR,
//    FATAL
//}

//public static class LogManager
//{
//    private static readonly object Lock = new object();

//    public static long MaximumFileLength { get; set; }

//    private static bool _useLog4Net = false;

//    //private static StreamWriter _sw = null;
//    private static string _directory = null;
//    private static string _filename = null;
//    private static string _logFilename = null;

//    static LogManager()
//    {
//        MaximumFileLength = 10000000;
//    }

//    public static ILog Global { get { return Log.My; } }

//    public static ILog GetLogger<T>()
//    {
//        return GetLogger(typeof(T));
//    }
//    public static ILog GetLogger(Type type)
//    {
//        return new Logger(type);
//    }
//    public static ILog GetLogger(string name)
//    {
//        return new Logger(name);
//    }

//    public delegate bool CanLogInfoHandler();
//    public delegate bool CanLogDebugHandler();
//    public delegate bool CanLogWarnHandler();
//    public delegate bool CanLogFatalHandler();
//    public delegate bool CanLogErrorHandler();
//    public delegate bool CanLogSqlHandler();
//    public delegate void OnLogHandler(string name, LogType type, string format, params object[] args);

//    public static event CanLogInfoHandler OnCanLogInfo;
//    public static event CanLogDebugHandler OnCanLogDebug;
//    public static event CanLogWarnHandler OnCanLogWarn;
//    public static event CanLogFatalHandler OnCanLogFatal;
//    public static event CanLogErrorHandler OnCanLogError;
//    public static event CanLogSqlHandler OnCanLogSql;
//    public static event OnLogHandler OnLog;

//    public static bool InvokeOnCanLogInfo()
//    {
//        if(OnCanLogInfo == null) return true;
//        return OnCanLogInfo();
//    }
//    public static bool InvokeOnCanLogDebug()
//    {
//        if(OnCanLogDebug == null) return true;
//        return OnCanLogDebug();
//    }
//    public static bool InvokeOnCanLogWarn()
//    {
//        if(OnCanLogWarn == null) return true;
//        return OnCanLogWarn();
//    }
//    public static bool InvokeOnCanLogFatal()
//    {
//        if(OnCanLogFatal == null) return true;
//        return OnCanLogFatal();
//    }
//    public static bool InvokeOnCanLogError()
//    {
//        if(OnCanLogError == null) return true;
//        return OnCanLogError();
//    }
//    public static bool InvokeOnCanLogSql()
//    {
//        if(OnCanLogSql == null) return true;
//        return OnCanLogSql();
//    }
//    public static void InvokeOnLog(string name, LogType logType, string format, params object[] args)
//    {
//        if(OnLog != null)
//            OnLog(name, logType, format, args);
//    }

//    public static void InitializeLog4Net()
//    {
//        lock(Lock)
//        {
//            _useLog4Net = true;
//            log4net.Config.XmlConfigurator.Configure();
//        }
//    }

//    public static string GetCurrentLogFile()
//    {
//        return _directory + _filename + ".log";
//    }

//    private static void ArchiveCurrentLogFile()
//    {
//        var currentLogFile = _directory + _filename + ".log";
        
//        var logFiles = Directory.GetFiles(_directory, "*.log").OrderBy(x => x).ToArray();

//        if(logFiles.Contains(currentLogFile))
//        {
//            logFiles = logFiles.Where(x => x.Contains("\\" + _filename + ".log.")).ToArray();

//            var maxFileNumber = 0;
//            if(logFiles.Any())
//            {
//                maxFileNumber = logFiles.Select(logFile => int.Parse(logFile.GetLastSection('\\').Replace(_filename + ".log.", ""))).Max();
//            }

//            var archivedFile = currentLogFile + "." + string.Format("{0:0000}", maxFileNumber + 1);
//            try
//            {
//                File.Move(currentLogFile, archivedFile);
//            }
//            catch(Exception) { }
//        }
//    }

//    public static void Initialize(string directory, string filename, long? maximumFileLength = null)
//    {
//        lock(Lock)
//        {
//            _useLog4Net = false;

//            _directory = directory.EndWith("\\");
//            _filename = filename;

//            if(maximumFileLength != null)
//            {
//                MaximumFileLength = maximumFileLength.Value;
//            }

//            _logFilename = GetCurrentLogFile();

//            if(File.Exists(_logFilename) && new FileInfo(_logFilename).Length > MaximumFileLength)
//            {
//                ArchiveCurrentLogFile();
//            }

//            //try
//            //{
//            //    _sw = new StreamWriter(_logFilename, true);
//            //    _sw.AutoFlush = true;
//            //}
//            //catch (IOException ex)
//            //{
//            //    if (ex.Message.Contains("cannot access"))
//            //    {
//            //        _logFilename = SetCurrentFile(directory, filename + "_" + DateTime.Now.ToString("yyyyMMddHHmmss"), logFiles);
//            //        _sw = new StreamWriter(currentLogFile, true);
//            //        _sw.AutoFlush = true;
//            //        return;
//            //    }
//            //    throw;
//            //}
//        }
//    }

//    private class Logger : ILog
//    {
//        private readonly string _name;
//        private readonly log4net.ILog _log4NetLogger;

//        public Logger(Type type)
//        {
//            //if (_UseLog4Net == null) return; throw new Exception("LogManager must be initialized first");
//            _name = type.Name;
//            _log4NetLogger = _useLog4Net ? log4net.LogManager.GetLogger(type) : null;
//        }
//        public Logger(string name)
//        {
//            //if (_UseLog4Net == null) return; throw new Exception("LogManager must be initialized first");
//            _name = name;
//            _log4NetLogger = _useLog4Net ? log4net.LogManager.GetLogger(name) : null;
//        }

//        private void WriteLine(string format, params object[] args)
//        {
//            if(_logFilename == null || string.IsNullOrEmpty(format)) return;

//            while(true)
//            {
//                try
//                {
//                    using(var sw = new StreamWriter(_logFilename, true))
//                    {
//                        sw.WriteLine(format, args);
//                    }

//                    if (new FileInfo(_logFilename).Length > MaximumFileLength)
//                    {
//                        ArchiveCurrentLogFile();
//                    }
//                    return;
//                }
//                catch(IOException)
//                {
//                    continue;
//                    //if(ex.Message.Contains("cannot access"))
//                    //{
//                    //    continue;
//                    //}
//                    //throw;
//                }
//            }

//            //if (_sw == null)
//            //{
//            //    return;
//            //}

//            //if (string.IsNullOrEmpty(format)) return;

//            //lock(Lock)
//            //{
//            //    _sw.WriteLine(format, args);
//            //}
//        }


//        public void Info(string format, params object[] args)
//        {
//            if(!LogManager.InvokeOnCanLogInfo()) return;
//            LogManager.InvokeOnLog(_name, LogType.INFO, format, args);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.InfoFormat(format, args);
//                return;
//            }
//            WriteLine(LogType.INFO + "  - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, string.Format(CultureInfo.InvariantCulture, format.TrimEnd('\n').TrimEnd('\r'), args), _name);
//        }
//        public bool IsInfoEnabled
//        {
//            get
//            {
//                if(_log4NetLogger != null)
//                {
//                    return _log4NetLogger.IsInfoEnabled;
//                }
//                return InvokeOnCanLogInfo();
//            }
//        }

//        public void Debug(string format, params object[] args)
//        {
//            if(!LogManager.InvokeOnCanLogDebug()) return;
//            LogManager.InvokeOnLog(_name, LogType.DEBUG, format, args);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.DebugFormat(format, args);
//                return;
//            }
//            WriteLine(LogType.DEBUG + " - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, string.Format(CultureInfo.InvariantCulture, format.TrimEnd('\n').TrimEnd('\r'), args), _name);
//        }
//        public bool IsDebugEnabled
//        {
//            get
//            {
//                if(_log4NetLogger != null)
//                {
//                    return _log4NetLogger.IsDebugEnabled;
//                }
//                return InvokeOnCanLogDebug();
//            }
//        }

//        public void Warn(string format, params object[] args)
//        {
//            if(!LogManager.InvokeOnCanLogWarn()) return;
//            LogManager.InvokeOnLog(_name, LogType.WARN, format, args);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.WarnFormat(format, args);
//                return;
//            }
//            WriteLine(LogType.WARN + "  - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, string.Format(CultureInfo.InvariantCulture, format.TrimEnd('\n').TrimEnd('\r'), args), _name);
//        }
//        public bool IsWarnEnabled
//        {
//            get
//            {
//                if(_log4NetLogger != null)
//                {
//                    return _log4NetLogger.IsWarnEnabled;
//                }
//                return InvokeOnCanLogWarn();
//            }
//        }

//        public void Fatal(string format, params object[] args)
//        {
//            if(!LogManager.InvokeOnCanLogFatal()) return;
//            LogManager.InvokeOnLog(_name, LogType.FATAL, format, args);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.FatalFormat(format, args);
//                return;
//            }
//            WriteLine(LogType.FATAL + " - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, string.Format(CultureInfo.InvariantCulture, format.TrimEnd('\n').TrimEnd('\r'), args), _name);
//        }
//        public bool IsFatalEnabled
//        {
//            get
//            {
//                if(_log4NetLogger != null)
//                {
//                    return _log4NetLogger.IsFatalEnabled;
//                }
//                return InvokeOnCanLogFatal();
//            }
//        }

//        //private string GetCurrentLocation()
//        //{
//        //    var st = new System.Diagnostics.StackTrace();
//        //    var sb = new StringBuilder();

//        //    foreach (var frame in st.GetFrames())
//        //    {
//        //        sb.Append(frame.ToString());
//        //    }

//        //    return sb.ToString();
//        //}

//        public void Error(Exception ex)
//        {
//            if(ex == null) return;
//            if(!LogManager.InvokeOnCanLogError()) return;
//            LogManager.InvokeOnLog(_name, LogType.ERROR, null, ex);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.Error(ex.Message, ex);
//                return;
//            }
//            WriteLine(LogType.ERROR + " - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, ex.Summary(), _name);
//            //if (!(ex is FileNotFoundException))
//            //    WriteLine("STACK - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, GetCurrentLocation(), name);
//        }

//        public void Error(Exception ex, string format, params object[] args)
//        {
//            if(ex == null) return;
//            if(!LogManager.InvokeOnCanLogError()) return;
//            LogManager.InvokeOnLog(_name, LogType.ERROR, null, ex);
//            LogManager.InvokeOnLog(_name, LogType.ERROR, format, args);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.Error(ex.Message, ex);
//                _log4NetLogger.ErrorFormat(format, args);
//                return;
//            }
//            WriteLine(LogType.ERROR + " - {0:dd/MM/yyyy HH:mm:ss} - [{3}] {1} - {2}", DateTime.Now, string.Format(CultureInfo.InvariantCulture, format, args), ex.Summary(), _name);
//            //if (!(ex is FileNotFoundException))
//            //    WriteLine("STACK - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, GetCurrentLocation(), name);
//        }

//        public void Error(string format, params object[] args)
//        {
//            if(!LogManager.InvokeOnCanLogError()) return;
//            LogManager.InvokeOnLog(_name, LogType.ERROR, format, args);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.ErrorFormat(format, args);
//                return;
//            }
//            WriteLine(LogType.ERROR + " - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, string.Format(CultureInfo.InvariantCulture, format, args), _name);
//            //if (!(ex is FileNotFoundException))
//            //    WriteLine("STACK - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, GetCurrentLocation(), name);
//        }
//        public bool IsErrorEnabled
//        {
//            get
//            {
//                if(_log4NetLogger != null)
//                {
//                    return _log4NetLogger.IsErrorEnabled;
//                }
//                return InvokeOnCanLogError();
//            }
//        }

//        public string GetSqlLog(object queryObject, Dictionary<string, object> parameters)
//        {
//            if(queryObject == null) return null;
//            var query = queryObject.ToString();

//            var sql = new StringBuilder();
//            sql.AppendLine(query);
//            if(parameters != null && parameters.Any())
//            {
//                sql.AppendLine("Parameters:");
//                foreach(var item in parameters)
//                {
//                    sql.AppendInvariantLine("{0}: {1}", item.Key, item.Value);
//                    //sql = sql.Replace(":" + item.Key, item.Value == null || item.Value is DBNull ? "null" : "'" + item.Value + "'");
//                }
//            }

//            return sql.ToString();
//        }
//        public string GetSqlLog(IDbCommand cmd)
//        {
//            if(cmd == null) return null;
//            var query = cmd.CommandText;

//            var sql = new StringBuilder();
//            sql.AppendLine(query);
//            if(cmd.Parameters != null && cmd.Parameters.Count > 0)
//            {
//                sql.AppendLine("Parameters:");
//                foreach(var item in cmd.Parameters.Cast<IDbDataParameter>())
//                {
//                    sql.AppendInvariantLine("{0}: {1}", item.ParameterName, item.Value);
//                }
//            }

//            return sql.ToString();
//        }

//        public void Sql(object queryObject, Dictionary<string, object> parameters)
//        {
//            if(!LogManager.InvokeOnCanLogSql()) return;
            
//            var sql = GetSqlLog(queryObject, parameters);
//            if(sql == null) return;

//            LogManager.InvokeOnLog(_name, LogType.SQL, sql);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.Debug(sql);
//                return;
//            }
//            WriteLine(LogType.SQL + "   - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, sql, _name);
//        }
//        public void Sql(IDbCommand cmd)
//        {
//            if(!LogManager.InvokeOnCanLogSql()) return;

//            var sql = GetSqlLog(cmd);
//            if(sql == null) return;

//            LogManager.InvokeOnLog(_name, LogType.SQL, sql);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.Debug(sql);
//                return;
//            }
//            WriteLine(LogType.SQL + "   - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, sql, _name);
//        }

//        public void Sql(double elapsedMilliseconds)
//        {
//            if(!LogManager.InvokeOnCanLogSql()) return;

//            var sql = string.Format(CultureInfo.InvariantCulture, "Execution time (ms): {0}", elapsedMilliseconds);

//            LogManager.InvokeOnLog(_name, LogType.SQL, sql);
//            if(_log4NetLogger != null)
//            {
//                _log4NetLogger.Debug(sql);
//                return;
//            }
//            WriteLine(LogType.SQL + "   - {0:dd/MM/yyyy HH:mm:ss} - [{2}] {1}", DateTime.Now, sql, _name);
//        }
//        public bool IsSqlEnabled
//        {
//            get
//            {
//                if(_log4NetLogger != null)
//                {
//                    return _log4NetLogger.IsDebugEnabled;
//                }
//                return InvokeOnCanLogSql();
//            }
//        }
//    }
//}

//public class Log
//{
//    private static ILog _my;
//    internal static ILog My
//    {
//        get { return _my ?? (_my = LogManager.GetLogger("GLOBAL")); }
//    }

//    public static bool IsInfoEnabled { get { return _my.IsInfoEnabled; } }
//    public static bool IsDebugEnabled { get { return _my.IsDebugEnabled; } }
//    public static bool IsWarnEnabled { get { return _my.IsWarnEnabled; } }
//    public static bool IsFatalEnabled { get { return _my.IsFatalEnabled; } }
//    public static bool IsErrorEnabled { get { return _my.IsErrorEnabled; } }
//    public static bool IsSqlEnabled { get { return _my.IsSqlEnabled; } }

//    public static void Info(string format, params object[] args)
//    {
//        My.Info(format, args);
//    }

//    public static void Debug(string format, params object[] args)
//    {
//        My.Debug(format, args);
//    }

//    public static void Warn(string format, params object[] args)
//    {
//        My.Warn(format, args);
//    }

//    public static void Error(Exception ex)
//    {
//        My.Error(ex);
//    }

//    public static void Error(Exception ex, string format, params object[] args)
//    {
//        My.Error(ex, format, args);
//    }

//    public static void Error(string format, params object[] args)
//    {
//        My.Error(format, args);
//    }

//    public static void Sql(string query, Dictionary<string, object> parameters)
//    {
//        My.Sql(query, parameters);
//    }
//}