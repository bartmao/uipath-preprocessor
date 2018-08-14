using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Communicator
{
    class Program
    {
        static void Main(string[] args)
        {
            Log(string.Join(",", args));

            var f = args[0]; // file name
            var o = args[1]; // w/r
            string l = null;
            string msg = null; // msg
            if (o == "w")
            {
                l = args[2]; // level
                msg = args[3]; // msg
            }
            //File = @"C:\Personal\communicator.data";

            EventWaitHandle locker;
            EventWaitHandle readLocker;
            EventWaitHandle writeLocker;
            var succeed = EventWaitHandle.TryOpenExisting("Communicator", out locker);
            if (!succeed)
            {
                Log("Creating Communicator");
                locker = new EventWaitHandle(true, EventResetMode.AutoReset, "Communicator");
            }
            else
            {
                Log("Opening Communicator");
            }
            succeed = EventWaitHandle.TryOpenExisting("Communicator_Read", out readLocker);
            if (!succeed)
            {
                Log("Create read mutex");
                readLocker = new EventWaitHandle(false, EventResetMode.AutoReset, "Communicator_Read");
            }
            succeed = EventWaitHandle.TryOpenExisting("Communicator_Write", out writeLocker);
            if (!succeed)
            {
                Log("Create write mutex");
                writeLocker = new EventWaitHandle(false, EventResetMode.AutoReset, "Communicator_Write");
            }

            try
            {
                Operation:
                if (!locker.WaitOne(3000)) throw new Exception("MMF file occupied");

                var q = new SharedMessageQueue(f);
                if (o == "w")
                {
                    var enqueued = q.Enqueue(int.Parse(l), msg);
                    if (!enqueued)
                    {
                        locker.Set();
                        if (!readLocker.WaitOne(60000))
                        {
                            Log("Failed to wait a read operation in 60s");
                        }
                        goto Operation;
                    }
                    writeLocker.Set();
                }
                else
                {
                    var outMsg = q.Dequeue();
                    if (outMsg == null)
                    {
                        locker.Set();
                        if (!writeLocker.WaitOne(60000))
                        {
                            Log("Failed to wait a write operation in 60s");
                        }
                        goto Operation;
                    }
                    Console.WriteLine(outMsg.Level);
                    Console.WriteLine(outMsg.Timestamp);
                    Console.WriteLine(outMsg.Message);
                    readLocker.Set();
                }
            }
            finally
            {
                locker.Set();
            }
        }

        private static void Log(string msg)
        {
            Console.WriteLine("PID " + Process.GetCurrentProcess().Id + "=>" + msg);
        }
    }

    class SharedMessageQueue : IDisposable
    {

        string File { get; set; }

        MemoryMappedViewAccessor Accessor { get; set; }

        MemoryMappedFile MMF { get; set; }

        int MaxSize { get; set; } = 100000;

        int ReservedSize = 18;

        int HeaderSize = 16;

        int Size { get; set; }

        long Front
        {
            get
            {
                var front = Accessor.ReadInt64(0);
                return front;
            }
            set
            {
                Accessor.Write(0, value);
            }
        }

        long Rear
        {
            get
            {
                var rear = Accessor.ReadInt64(8);
                return rear;
            }
            set
            {
                Accessor.Write(8, value);
            }
        }

        bool IsEmpty
        {
            get
            {
                var isEmpty = Accessor.ReadBoolean(16);
                return isEmpty;
            }
            set
            {
                Accessor.Write(16, value);
            }
        }

        bool IsFull
        {
            get
            {
                var isFull = Accessor.ReadBoolean(17);
                return isFull;
            }
            set
            {
                Accessor.Write(17, value);
            }
        }

        public SharedMessageQueue(string file)
        {
            File = file;

            if (System.IO.File.Exists(File))
            {
                Log("Opening Queue File");
                MMF = MemoryMappedFile.CreateFromFile(File, FileMode.Open, "communicator", MaxSize);
                Accessor = MMF.CreateViewAccessor(0, ReservedSize);
                Log("Opened Queue File");
            }
            else
            {
                Log("Creating Queue File");
                MMF = MemoryMappedFile.CreateFromFile(File, FileMode.Create, "communicator", MaxSize);
                Accessor = MMF.CreateViewAccessor(0, ReservedSize);
                Front = ReservedSize;
                Rear = ReservedSize;
                IsEmpty = true;
                IsFull = false;
                Log("Created Queue File");
            }

            Log("Front:{0}, Rear:{1}", Front, Rear);
        }

        public bool Enqueue(int level, string msg)
        {
            var c = Encoding.UTF8.GetBytes(msg);
            var data = new byte[c.Length + HeaderSize];
            var s = new MemoryStream(data);
            using (var sw = new BinaryWriter(s))
            {
                sw.Write(level); // 4
                sw.Write(DateTime.Now.Ticks); // 8
                sw.Write(c.Length); // 4
                sw.Write(c);
            }

            using (var stream = MMF.CreateViewStream())
            {
                stream.Position = Front;
                if (IsFull)
                {
                    // No space
                    Log("No space for writing data");
                    return false;
                }
                else
                {
                    if (Front + data.Length < stream.Length)
                    {
                        // Enough space to go
                        stream.Write(data, 0, data.Length);
                        Front = stream.Position;
                        Log("Enough space, enqueue data");
                    }
                    else
                    {
                        // No enough space, break into two parts
                        var firstLen = (int)(stream.Length - Front);
                        var secondLen = data.Length - firstLen;
                        stream.Write(data, 0, firstLen);
                        stream.Position = ReservedSize; // reset to begin
                        stream.Write(data, firstLen - 1, secondLen);
                        Front = stream.Position;
                        Log("Split data and enqueue");
                    }

                    IsEmpty = false;
                }

                // 1. Front is catching up Rear in next cycle
                // 2. Front is near the end of the stream, and remaining data in the head is not enough
                if ((Rear > Front && Rear - Front < HeaderSize) || (stream.Length - Front - 1 + (Rear - ReservedSize)) < HeaderSize)
                {
                    IsFull = true;
                }

                Log("Front:{0}, Rear:{1}", Front, Rear);
                return true;
            }

        }

        public SharedMessage Dequeue()
        {
            using (var stream = MMF.CreateViewStream())
            {
                if (IsEmpty)
                {
                    Log("No Data");
                    return null;
                }
                else
                {
                    using (var sr = new BinaryReader(stream))
                    {
                        stream.Position = Rear;
                        var header = new byte[HeaderSize];
                        if (stream.Length - Rear >= HeaderSize)
                        {
                            header = sr.ReadBytes(HeaderSize);
                        }
                        else
                        {
                            var firstLen = (int)(stream.Length - stream.Position);
                            var secondLen = HeaderSize - firstLen;
                            var header1 = sr.ReadBytes(firstLen);
                            stream.Position = ReservedSize;
                            var header2 = sr.ReadBytes(secondLen);
                            Array.Copy(header1, header, firstLen);
                            Array.Copy(header2, 0, header, firstLen - 1, secondLen);
                        }
                        var level = BitConverter.ToInt32(header, 0);
                        var time = new DateTime(BitConverter.ToInt64(header, 4));
                        var len = BitConverter.ToInt32(header, 12);

                        var msg = new byte[len];
                        if (stream.Position + len <= stream.Length)
                        {
                            msg = sr.ReadBytes(len);
                        }
                        else
                        {
                            var firstLen = (int)(stream.Length - stream.Position);
                            var secondLen = len - firstLen;
                            var msg1 = sr.ReadBytes(firstLen);
                            stream.Position = ReservedSize;
                            var msg2 = sr.ReadBytes(secondLen);
                            Array.Copy(msg1, msg, firstLen);
                            Array.Copy(msg2, 0, msg, firstLen - 1, secondLen);
                        }

                        Rear = stream.Position;
                        IsFull = false;

                        // 1. Rear is catching up Front
                        // 2. Rear is near the end of the stream, the remaining data is not enough
                        if (Front - Rear < HeaderSize || (stream.Length - Rear - 1 + (Front - ReservedSize)) < HeaderSize)
                        {
                            IsEmpty = true;
                        }

                        Log("Front:{0}, Rear:{1}", Front, Rear);
                        var message = Encoding.UTF8.GetString(msg);
                        Log("Level:{0}, Time:{1}, Message:{2}", level, time, message);
                        return new SharedMessage()
                        {
                            Level = level,
                            Timestamp = time,
                            Message = message
                        };
                    }

                }
            }

        }

        public void Dispose()
        {
            if (MMF != null)
                MMF.Dispose();
        }

        private void Log(string msg, params object[] args)
        {
            Console.WriteLine("PID " + Process.GetCurrentProcess().Id + "=>" + msg, args);
        }
    }


    class SharedMessage
    {
        public int Level { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }
    }
}
