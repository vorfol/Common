using System;
using System.Collections.Generic;

namespace Vorfol.Data {

    public class ParsedValue {
        public readonly byte id = 0;
        public readonly byte size = 0;
        public readonly int offset = 0;
        public readonly bool valid = false;
        private byte[] baseData = null;
        private int start = 0;
        public ParsedValue(byte[] baseData, int start) {
            if (baseData != null) {
                this.baseData = baseData;
                if (start >= 0 && start < baseData.Length ) {
                    this.start = start;
                    this.id = baseData[start];
                    if ((this.id & 0x80) != 0) {
                        this.id = (byte)(this.id ^ 0xFF);
                        this.offset = sizeof(byte);
                        this.valid = true;
                    } else {
                        this.size = baseData[start + sizeof(byte)];
                        this.offset = sizeof(byte)*2 + size;
                        if (this.start + this.offset <= this.baseData.Length) {
                            this.valid = true;
                        }
                    }
                }
            }
        }
        public byte[] data {
            get  {
                byte[] retData = new byte[this.size];
                if (this.valid && this.size > 0) {
                    Array.Copy(this.baseData, this.start + sizeof(byte)*2, retData, 0, retData.Length);
                }
                return retData;
            }
        }
    }

    public class ParsedBlock {
        public readonly Dictionary<byte, List<ParsedValue>> values = new Dictionary<byte, List<ParsedValue>>(); 
        public ParsedBlock(byte[] data) {
            int offset = 0;
            ParsedValue parsedValue = new ParsedValue(data, offset);
            while (parsedValue.valid) {
                List<ParsedValue> entries;
                if (!this.values.TryGetValue(parsedValue.id, out entries)) {
                    entries = new List<ParsedValue>();
                    this.values.Add(parsedValue.id, entries);
                }
                entries.Add(parsedValue);
                offset += parsedValue.offset;
                parsedValue = new ParsedValue(data, offset);
            }
        }
    }
    public class BlockToSend {
        private List<byte> sendData;
        public BlockToSend() {
            this.sendData = new List<byte>();
        }
        public void push(byte id, byte[] data) {
            int size = data.Length;
            int pos = 0;
            while(size > 255) {
                this.sendData.Add(id);
                this.sendData.Add(255);
                this.sendData.AddRange(new ArraySegment<byte>(data, pos, 255));
                pos += 255;
                size -= 255;
            }
            this.sendData.Add(id);
            this.sendData.Add((byte)size);
            this.sendData.AddRange(data);
        }
        public byte[] data {
            get {
                return this.sendData.ToArray();
            }
        }
    }
}