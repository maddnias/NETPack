using System;

namespace NETPack.Core.Injections
{
    internal class Decompressor
    {
        private static int h(byte[] y)
        {
            return ((y[0] & 2) == 2) ? 9 : 3;
        }

        public static int s(byte[] y)
        {
            if (h(y) == 9)
                return y[5] | (y[6] << 8) | (y[7] << 16) | (y[8] << 24);
            return y[2];
        }

        public static int c(byte[] y)
        {
            if (h(y) == 9)
                return y[1] | (y[2] << 8) | (y[3] << 16) | (y[4] << 24);
            return y[1];
        }

        public static byte[] D(byte[] y)
        {
// ReSharper disable InconsistentNaming
            int size = s(y);
            int src = h(y);
            int dst = 0;
            uint cword_val = 1;

            byte[] destination = new byte[size];
            int[] hashtable = new int[4096];
            byte[] hash_counter = new byte[4096];
            int last_matchstart = size - 6 - 4 - 1;
            int last_hashed = -1;
            uint fetch = 0;
// ReSharper restore InconsistentNaming

            int level = (y[0] >> 2) & 0x3;

            if ((y[0] & 1) != 1)
            {
                byte[] d2 = new byte[size];
                Array.Copy(y, h(y), d2, 0, size);
                return d2;
            }

            for (;;)
            {
                if (cword_val == 1)
                {
                    cword_val =
                        (uint)
                        (y[src] | (y[src + 1] << 8) | (y[src + 2] << 16) | (y[src + 3] << 24));
                    src += 4;
                    if (dst <= last_matchstart)
                    {
                        if (level == 1)
                            fetch = (uint) (y[src] | (y[src + 1] << 8) | (y[src + 2] << 16));
                        else
                            fetch =
                                (uint)
                                (y[src] | (y[src + 1] << 8) | (y[src + 2] << 16) |
                                 (y[src + 3] << 24));
                    }
                }

                int hash;
                if ((cword_val & 1) == 1)
                {
                    uint matchlen;
                    uint offset2;

                    cword_val = cword_val >> 1;

                    if (level == 1)
                    {
                        hash = ((int) fetch >> 4) & 0xfff;
                        offset2 = (uint) hashtable[hash];

                        if ((fetch & 0xf) != 0)
                        {
                            matchlen = (fetch & 0xf) + 2;
                            src += 2;
                        }
                        else
                        {
                            matchlen = y[src + 2];
                            src += 3;
                        }
                    }
                    else
                    {
                        uint offset;
                        if ((fetch & 3) == 0)
                        {
                            offset = (fetch & 0xff) >> 2;
                            matchlen = 3;
                            src++;
                        }
                        else if ((fetch & 2) == 0)
                        {
                            offset = (fetch & 0xffff) >> 2;
                            matchlen = 3;
                            src += 2;
                        }
                        else if ((fetch & 1) == 0)
                        {
                            offset = (fetch & 0xffff) >> 6;
                            matchlen = ((fetch >> 2) & 15) + 3;
                            src += 2;
                        }
                        else if ((fetch & 127) != 3)
                        {
                            offset = (fetch >> 7) & 0x1ffff;
                            matchlen = ((fetch >> 2) & 0x1f) + 2;
                            src += 3;
                        }
                        else
                        {
                            offset = (fetch >> 15);
                            matchlen = ((fetch >> 7) & 255) + 3;
                            src += 4;
                        }
                        offset2 = (uint) (dst - offset);
                    }

                    destination[dst + 0] = destination[offset2 + 0];
                    destination[dst + 1] = destination[offset2 + 1];
                    destination[dst + 2] = destination[offset2 + 2];

                    for (int i = 3; i < matchlen; i += 1)
                    {
                        destination[dst + i] = destination[offset2 + i];
                    }

                    dst += (int) matchlen;

                    if (level == 1)
                    {
                        fetch =
                            (uint)
                            (destination[last_hashed + 1] | (destination[last_hashed + 2] << 8) |
                             (destination[last_hashed + 3] << 16));
                        while (last_hashed < dst - matchlen)
                        {
                            last_hashed++;
                            hash = (int)(((fetch >> 12) ^ fetch) & (4096 - 1));
                            hashtable[hash] = last_hashed;
                            hash_counter[hash] = 1;
                            fetch = (uint) (fetch >> 8 & 0xffff | destination[last_hashed + 3] << 16);
                        }
                        fetch = (uint) (y[src] | (y[src + 1] << 8) | (y[src + 2] << 16));
                    }
                    else
                    {
                        fetch =
                            (uint)
                            (y[src] | (y[src + 1] << 8) | (y[src + 2] << 16) | (y[src + 3] << 24));
                    }
                    last_hashed = dst - 1;
                }
                else
                {
                    if (dst <= last_matchstart)
                    {
                        destination[dst] = y[src];
                        dst += 1;
                        src += 1;
                        cword_val = cword_val >> 1;

                        if (level == 1)
                        {
                            while (last_hashed < dst - 3)
                            {
                                last_hashed++;
                                int fetch2 = destination[last_hashed] | (destination[last_hashed + 1] << 8) |
                                             (destination[last_hashed + 2] << 16);
                                hash = ((fetch2 >> 12) ^ fetch2) & (4096 - 1);
                                hashtable[hash] = last_hashed;
                                hash_counter[hash] = 1;
                            }
                            fetch = (uint) (fetch >> 8 & 0xffff | y[src + 2] << 16);
                        }
                        else
                        {
                            fetch = (uint) (fetch >> 8 & 0xffff | y[src + 2] << 16 | y[src + 3] << 24);
                        }
                    }
                    else
                    {
                        while (dst <= size - 1)
                        {
                            if (cword_val == 1)
                            {
                                src += 4;
                                cword_val = 0x80000000;
                            }

                            destination[dst] = y[src];
                            dst++;
                            src++;
                            cword_val = cword_val >> 1;
                        }
                        return destination;
                    }
                }
            }

        }
    }
}