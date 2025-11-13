using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecsGemLib.Core
{
    public static class MessageSchemaTable
    {
        public static readonly Dictionary<(byte S, byte F), ItemSchema> Schemas =
            new()
            {
            // ==================================================
            // STREAM 1 — Equipment Constant / Status
            // ==================================================

            // S1F1 – Are You There Request
            { (1,1), ItemSchema.L() },

            // S1F2 – Are You There Reply
            { (1,2), ItemSchema.L(
                ItemSchema.A(),  // MDLN
                ItemSchema.A()   // SOFTREV
            )},

            // S1F3 – Selected SVID Request
            { (1,3), ItemSchema.L(
                ItemSchema.U4().Repeat()  // SVID list
            )},

            // S1F4 – Selected Variable Value Reply
            { (1,4), ItemSchema.L(
                ItemSchema.L().Repeat()   // Each SV value inside a list
            )},

            // ==================================================
            // STREAM 2 — Collection Events / Reports
            // ==================================================

            // S2F13 — Define Variable Name
            { (2,13), ItemSchema.L(
                ItemSchema.U4(),  // DATAID
                ItemSchema.L(
                    ItemSchema.L(
                        ItemSchema.U4(), // SVID
                        ItemSchema.A()   // Name
                    ).Repeat()
                )
            )},

            // S2F15 — Get Variable Name
            { (2,15), ItemSchema.L(
                ItemSchema.U4().Repeat()  // SVID list
            )},

            // S2F17 — Variable Name Reply
            { (2,17), ItemSchema.L(
                ItemSchema.L(
                    ItemSchema.U4(),   // SVID
                    ItemSchema.A()     // Name
                ).Repeat()
            )},

            // S2F21 — Date/Time Request
            { (2,21), ItemSchema.L() },

            // S2F22 — Date/Time Reply
            { (2,22), ItemSchema.L(
                ItemSchema.A()    // ASCII Time
            )},

            // S2F23 — Set Date/Time Request
            { (2,23), ItemSchema.L(
                ItemSchema.A()  // ASCII Time
            )},

            // S2F24 — Set Date/Time Ack
            { (2,24), ItemSchema.L(
                ItemSchema.U1()  // TIACK
            )},

            // S2F33 — Define Report
            { (2,33), ItemSchema.L(
                ItemSchema.U4(),  // DATAID
                ItemSchema.L(     // Report list
                    ItemSchema.L(
                        ItemSchema.U2(),       // RPTID
                        ItemSchema.L(
                            ItemSchema.U2()    // VID
                        ).Repeat()
                    ).Repeat()
                )
            )},

            // S2F35 — Link Event Report (CEID ↔ RPTID)
            { (2,35), ItemSchema.L(
                ItemSchema.U2(),          // DATAID
                ItemSchema.L(
                    ItemSchema.L(
                        ItemSchema.U2(),  // CEID
                        ItemSchema.L(
                            ItemSchema.U2() // RPTID
                        ).Repeat()
                    ).Repeat()
                )
            )},

            // S2F37 — Enable/Disable Event
            { (2,37), ItemSchema.L(
                ItemSchema.U1(),      // CEED (0/1)
                ItemSchema.U2().Repeat()   // CEID list
            )},

            // S2F41 — Host Command Send
            { (2,41), ItemSchema.L(
                ItemSchema.A(),    // Command name
                ItemSchema.L(      // Parameter list
                    ItemSchema.L(
                        ItemSchema.A(), // Parameter name
                        ItemSchema.A()  // Parameter value
                    ).Repeat()
                )
            )},

            // S2F42 — Host Command Ack
            { (2,42), ItemSchema.L(
                ItemSchema.A(),   // Command name
                ItemSchema.U1()   // HCACK
            )},

            // ==================================================
            // STREAM 5 — Alarm
            // ==================================================

            // S5F1 — Alarm Report
            { (5,1), ItemSchema.L(
                ItemSchema.U2(), // ALID
                ItemSchema.BOOLEAN(), // ALCD
                ItemSchema.A()   // ALTX
            )},

            // S5F2 — Alarm Report Ack
            { (5,2), ItemSchema.L() },

            // S5F3 — Enable/Disable Alarm Request
            { (5,3), ItemSchema.L(
                ItemSchema.U1(),  // ALCD (enable/disable)
                ItemSchema.U2().Repeat()  // ALID list
            )},

            // S5F4 — Alarm Enable Ack
            { (5,4), ItemSchema.L(
                ItemSchema.U1() // ACKC5
            )},

            // ==================================================
            // STREAM 6 — Event Report
            // ==================================================

            // S6F1 — Trace Data Send
            { (6,1), ItemSchema.L(
                ItemSchema.U4(),  // TRID
                ItemSchema.U4(),  // SMPLN
                ItemSchema.L(     // Report list
                    ItemSchema.L().Repeat()
                )
            )},

            // S6F11 — Event Report
            { (6,11), ItemSchema.L(
                ItemSchema.U2(),  // CEID
                ItemSchema.L(
                    ItemSchema.L(
                        ItemSchema.U2(),  // RPTID
                        ItemSchema.L().Repeat()  // RPT DATA VALUES
                    ).Repeat()
                )
            )},

            // S6F12 — Event Report Ack
            { (6,12), ItemSchema.L() },

            // ==================================================
            // STREAM 7 — Process Program Management (Optional)
            // ==================================================

            // S7F1 — PP Request
            { (7,1), ItemSchema.L(
                ItemSchema.A() // PPID
            )},

            // S7F2 — PP Data Send
            { (7,2), ItemSchema.L(
                ItemSchema.A(), // PPID
                ItemSchema.B()  // Binary PP body
            )},

            // S7F3 — Send PP Download Request
            { (7,3), ItemSchema.L(
                ItemSchema.A() // PPID
            )},

            // S7F4 — PP Download Ack
            { (7,4), ItemSchema.L(
                ItemSchema.U1() // ACKC7
            )},

            // ==================================================
            // STREAM 9 — Error Messages
            // ==================================================

            // S9Fx — All S9 errors have no body
            { (9,1), ItemSchema.L() },
            { (9,3), ItemSchema.L() },
            { (9,5), ItemSchema.L() },
            { (9,7), ItemSchema.L() },
            { (9,9), ItemSchema.L() },
            { (9,11), ItemSchema.L() },
            { (9,13), ItemSchema.L() },
            { (9,15), ItemSchema.L() },

            // ==================================================
            // STREAM 10 — Terminal
            // ==================================================

            // S10F3 — Terminal Display
            { (10,3), ItemSchema.L(
                ItemSchema.A(),  // TEXT
                ItemSchema.U1()  // TID
            )},

            // S10F4 — Terminal Display Ack
            { (10,4), ItemSchema.L(
                ItemSchema.U1()  // TACK
            )},

            // ==================================================
            // STREAM 14 — Mode Control
            // ==================================================

            // S14F9 — PP Change Event
            { (14,9), ItemSchema.L(
                ItemSchema.A(),    // PPID
                ItemSchema.U2()    // PPChange
            )}
            };
    }
}
