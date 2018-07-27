using System;
using System.Collections.Generic;
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

            var q = new SharedMessageQueue(f);
            if (o == "w")
                q.Enqueue(int.Parse(l), msg);
            else
            {
                var outMsg = q.Dequeue();
                Console.WriteLine(outMsg.Level);
                Console.WriteLine(outMsg.Timestamp);
                Console.WriteLine(outMsg.Message);
            }
        }
    }

    class SharedMessageQueue : IDisposable
    {
        private Mutex locker;
        private ManualResetEvent readLocker;
        private ManualResetEvent writeLocker;

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
                locker.WaitOne();
                var front = Accessor.ReadInt64(0);
                locker.ReleaseMutex();
                return front;
            }
            set
            {
                locker.WaitOne();
                Accessor.Write(0, value);
                locker.ReleaseMutex();
            }
        }

        long Rear
        {
            get
            {
                locker.WaitOne();
                var rear = Accessor.ReadInt64(8);
                locker.ReleaseMutex();
                return rear;
            }
            set
            {
                locker.WaitOne();
                Accessor.Write(8, value);
                locker.ReleaseMutex();
            }
        }

        bool IsEmpty
        {
            get
            {
                locker.WaitOne();
                var isEmpty = Accessor.ReadBoolean(16);
                locker.ReleaseMutex();
                return isEmpty;
            }
            set
            {
                locker.WaitOne();
                Accessor.Write(16, value);
                locker.ReleaseMutex();
            }
        }

        bool IsFull
        {
            get
            {
                locker.WaitOne();
                var isFull = Accessor.ReadBoolean(17);
                locker.ReleaseMutex();
                return isFull;
            }
            set
            {
                locker.WaitOne();
                Accessor.Write(17, value);
                locker.ReleaseMutex();
            }
        }

        public SharedMessageQueue(string file)
        {
            File = file;
            var succeed = Mutex.TryOpenExisting("Communicator", out locker);
            if (!succeed)
            {
                locker = new Mutex(false, "Communicator");
            }

            locker.WaitOne();
            if (System.IO.File.Exists(File))
            {
                MMF = MemoryMappedFile.CreateFromFile(File, FileMode.Open, "communicator", MaxSize);
                Accessor = MMF.CreateViewAccessor(0, ReservedSize);
            }
            else
            {
                MMF = MemoryMappedFile.CreateFromFile(File, FileMode.Create, "communicator", MaxSize);
                Accessor = MMF.CreateViewAccessor(0, ReservedSize);
                Front = ReservedSize;
                Rear = ReservedSize;
                IsEmpty = true;
                IsFull = false;
            }

            locker.ReleaseMutex();
            readLocker = new ManualResetEvent(false);
            writeLocker = new ManualResetEvent(false);
            Console.WriteLine("Front:{0}, Rear:{1}", Front, Rear);
        }

        public void Enqueue(int level, string msg)
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
                WriteData0:
                if (IsFull)
                {
                    // No space
                    Console.WriteLine("No space for writing data");
                    readLocker.WaitOne(60000);
                    goto WriteData0;
                }
                else
                {
                    if (Front + data.Length < stream.Length)
                    {
                        // Enough space to go
                        locker.WaitOne();
                        stream.Write(data, 0, data.Length);
                        Front = stream.Position;
                        Console.WriteLine("Enough space, enqueue data");
                        locker.ReleaseMutex();
                    }
                    else
                    {
                        // No enough space, break into two parts
                        var firstLen = (int)(stream.Length - Front);
                        var secondLen = data.Length - firstLen;
                        locker.WaitOne();
                        stream.Write(data, 0, firstLen);
                        stream.Position = ReservedSize; // reset to begin
                        stream.Write(data, firstLen - 1, secondLen);
                        Front = stream.Position;
                        locker.ReleaseMutex();
                        writeLocker.Set();
                        Console.WriteLine("Split data and enqueue");
                    }

                    IsEmpty = false;
                }

                // 1. Front is catching up Rear in next cycle
                // 2. Front is near the end of the stream, and remaining data in the head is not enough
                if ((Rear > Front && Rear - Front < HeaderSize) || (stream.Length - Front - 1 + (Rear - ReservedSize)) < HeaderSize)
                {
                    IsFull = true;
                }

                Console.WriteLine("Front:{0}, Rear:{1}", Front, Rear);
            }

        }

        public SharedMessage Dequeue()
        {
            using (var stream = MMF.CreateViewStream())
            {
                ReadData:
                if (IsEmpty)
                {
                    Console.WriteLine("No Data");
                    writeLocker.WaitOne(60000);
                    goto ReadData;
                }
                else
                {
                    using (var sr = new BinaryReader(stream))
                    {
                        locker.WaitOne();
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
                        locker.ReleaseMutex();
                        readLocker.Set();

                        // 1. Rear is catching up Front
                        // 2. Rear is near the end of the stream, the remaining data is not enough
                        if (Front - Rear < HeaderSize || (stream.Length - Rear - 1 + (Front - ReservedSize)) < HeaderSize)
                        {
                            IsEmpty = true;
                        }

                        Console.WriteLine("Front:{0}, Rear:{1}", Front, Rear);
                        var message = Encoding.UTF8.GetString(msg);
                        Console.WriteLine("Level:{0}, Time:{1}, Message:{2}", level, time, message);
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
    }


    class SharedMessage
    {
        public int Level { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }
    }
}
