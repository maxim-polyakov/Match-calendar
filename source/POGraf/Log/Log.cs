using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Windows.Controls;

namespace Log
{
    public class rMsg
    {
        public int msgType = 0;
        public string msgContext = null;
        public string msg = null;

        public rMsg(int msg_type, string msg_context, string msg_text)
        {
            set(msg_type, msg_context, msg_text);
        }
        public rMsg(int msg_type, string msg_text)
        {
            set(msg_type, "", msg_text);
        }
        public rMsg(string msg_text)
        {
            set(0, "", msg_text);
        }
        public rMsg(rMsg r)
        {
            set(r.msgType, r.msgContext, r.msg);
        }
        public void set(int msg_type, string msg_context, string msg_text)
        {
            msgType = msg_type;
            msgContext = msg_context;
            msg = msg_text;
        }
    }

    public class MsgStr
    {
        public virtual string msg2str(rMsg r, bool add_title)
        {
            string s = "";
            if (add_title) s += msg2caption(r) + " (" + r.msgType.ToString() + ") - ";
            if (!string.IsNullOrEmpty(r.msgContext)) s += r.msgContext + ": ";
            if (!string.IsNullOrEmpty(r.msg)) s += r.msg;
            else if (r.msgType != 0) s += "Код: " + r.msgType.ToString();
            return s;
        }
        public virtual string msg2caption(rMsg r)
        {
            string s = "";
            if (r.msgType <= error_base()) s += "Ошибка";
            else if (r.msgType < info_base()) s += "Предупреждение";
            else if (r.msgType >= info_base()) s += "Информация";
            return s;
        }
        public virtual int error_base() { return -10; }
        public virtual int info_base() { return 0; }
    }

    public class MsgList
    {
        protected List<rMsg> ml = null;
        public int count
        {
            get { return ml.Count; }
        }
        public rMsg this[int i]
        {
            get { return ml[i]; }
        }
        public MsgList()
        {
            ml = new List<rMsg>();
        }
        public void clear() { ml.Clear(); }
        public void add(rMsg r) { ml.Add(r); }
        public rMsg extract(int i)
        {
            rMsg r = ml[i];
            ml.RemoveAt(i);
            return r;
        }
    }

    public abstract class MsgLogger
    {
        protected MsgStr ms = null;
        public abstract bool msg(rMsg r);
        public MsgLogger() { ms = new MsgStr(); }
        public bool msg(int msg_type, string msg_context, string msg_text)
        {
            rMsg r = new rMsg(msg_type, msg_context, msg_text);
            return msg(r);
        }
        public bool msg(int msg_type, string msg_text)
        {
            rMsg r = new rMsg(msg_type, msg_text);
            return msg(r);
        }
        public bool msg(string msg_text)
        {
            rMsg r = new rMsg(msg_text);
            return msg(r);
        }

        public int error_base() { return ms.error_base(); }
        public int info_base() { return ms.info_base(); }
    }

    public class LogObject
    {
        protected MsgLogger _log = null;
        public MsgLogger log
        {
            get { return _log; }
            set { _log = value; }
        }
        public LogObject()
        {
            _log = new NullLogger();
        }
    }

    public class MsgLoggerDecorator : MsgLogger
    {
        protected MsgLogger simple = null;
        public MsgLoggerDecorator(MsgLogger m)
            : base()
        {
            simple = m;
        }
        public override bool msg(rMsg r)
        {
            if (simple == null) return false;
            return simple.msg(r);
        }
    }

    public class MsgLoggerComposite : MsgLogger
    {
        protected List<MsgLogger> list = null;

        public MsgLoggerComposite()
            : base()
        {
            list = new List<MsgLogger>();
        }
        public void add(MsgLogger m) { list.Add(m); }
        public override bool msg(rMsg r)
        {
            if (list.Count <= 0) return false;
            bool cr, res = true;
            foreach (MsgLogger m in list)
            {
                cr = m.msg(r);
                res = cr && res;
            }
            return res;
        }
    }

    public class SkipMsgLogger : MsgLoggerDecorator
    {
        protected int skipType = 0;
        public SkipMsgLogger(int SkipMsgType, MsgLogger m)
            : base(m)
        {
            skipType = SkipMsgType;
        }
        public override bool msg(rMsg r)
        {
            if (r.msgType == skipType) return true;
            return base.msg(r);
        }
    }

    public class SkipInfoLogger : SkipMsgLogger
    {
        public SkipInfoLogger(MsgLogger m)
            : base(0, m)
        {
        }
        public override bool msg(rMsg r)
        {
            if (r.msgType >= skipType) return true;
            return base.msg(r);
        }
    }

    public class NullLogger : MsgLogger
    {
        public NullLogger() : base() { }
        public override bool msg(rMsg r)
        {
            return true;
        }
    }

    public class FileLogger : MsgLogger
    {
        protected string fname = null;
        public string name
        {
            get { return fname; }
            set { fname = value; }
        }

        public FileLogger() : base() { }
        public FileLogger(string filename) : base() { name = filename; }
        public bool FileDelete()
        {
            File.Delete(name);
            try
            {
                File.Delete(name);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
        public override bool msg(rMsg r)
        {
            try
            {
                using (StreamWriter sw = File.AppendText(name))
                {
                    sw.WriteLine(ms.msg2str(r, true));
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public class FormLogger : MsgLogger
    {
        public FormLogger() : base() { }
        public override bool msg(rMsg r)
        {
            try
            {
                MessageBoxIcon ic;
                if (r.msgType >= info_base()) ic = MessageBoxIcon.Information;
                else if ((r.msgType < info_base()) && (r.msgType > error_base())) ic = MessageBoxIcon.Warning;
                else ic = MessageBoxIcon.Stop;
                MessageBox.Show(ms.msg2str(r, false), ms.msg2caption(r), MessageBoxButtons.OK, ic);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public class TextBoxLogger : MsgLogger
    {
        protected System.Windows.Controls.TextBox tb = null;
        public TextBoxLogger(System.Windows.Controls.TextBox text) : base() { tb = text; }
        public override bool msg(rMsg r)
        {
            try
            {
                if (tb != null) tb.AppendText(ms.msg2str(r, false) + Environment.NewLine);
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }
    }

    public class LogGlobal
    {
        protected static MsgLogger _log = null;
        public static MsgLogger log { get { return _log; } }
        public static void init(MsgLogger l) { _log = l; }
        public static bool msg(rMsg r)
        {
            if (_log == null) return false;
            return _log.msg(r);
        }
        public static bool msg(int msg_type, string msg_context, string msg_text) { return msg(new rMsg(msg_type, msg_context, msg_text)); }
        public static bool msg(int msg_type, string msg_text) { return msg(new rMsg(msg_type, msg_text)); }
        public static bool msg(string msg_text) { return msg(new rMsg(msg_text)); }

        public static int error_base() { return _log.error_base(); }
        public static int info_base() { return _log.info_base(); }

        public static void Join(string logName)
        {
            // Лог создаем
            MsgLoggerComposite l = new MsgLoggerComposite();
            FileLogger f = new FileLogger();
            SkipInfoLogger sl = new SkipInfoLogger(new FormLogger());
            //string s = Process.GetCurrentProcess().MainModule.FileName;
            //s = Path.GetDirectoryName(s);
            //f.name = s + "\\Plugins\\IntegratorTIS.log";
            f.name = logName;
            l.add(f);
            l.add(sl);
            LogGlobal.init(l);            
        }

        public static void Start()
        {
            // Вывод в лог
            LogGlobal.msg("");
            LogGlobal.msg("------------------------------------");
            LogGlobal.msg("Start at " + DateTime.Now.ToString());
            //LogGlobal.msg(0, "TIS-lib is opened");
            //LogGlobal.msg();
        }
    }


}