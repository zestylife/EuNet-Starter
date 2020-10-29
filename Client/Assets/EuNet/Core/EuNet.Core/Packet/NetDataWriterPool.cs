﻿using System;

namespace EuNet.Core
{
    public class NetDataWriterPool : ConcurrentObjectPool<NetDataWriter>
    {
        public override NetDataWriter Alloc()
        {
            var writer = base.Alloc();
            writer.Reset();
            return writer;
        }

        public void Use(Action<NetDataWriter> action)
        {
            var writer = NetPool.DataWriterPool.Alloc();
            try
            {
                action(writer);
            }
            finally
            {
                NetPool.DataWriterPool.Free(writer);
            }
        }
    }
}
