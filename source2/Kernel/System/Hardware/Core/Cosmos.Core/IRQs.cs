﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Cosmos.Kernel;
using Cosmos.Hardware2;

namespace Cosmos.Core {
    public class IRQs {
        // TODO: Protect IRQs like memory and ports are
        // TODO: Make IRQs so they are not hookable, and instead release high priority threads like FreeBSD (When we get threading)

        [StructLayout(LayoutKind.Explicit, Size = 0x68)]
        public struct TSS {
            [FieldOffset(0)]
            public ushort Link;
            [FieldOffset(4)]
            public uint ESP0;
            [FieldOffset(8)]
            public ushort SS0;
            [FieldOffset(12)]
            public uint ESP1;
            [FieldOffset(16)]
            public ushort SS1;
            [FieldOffset(20)]
            public uint ESP2;
            [FieldOffset(24)]
            public ushort SS2;
            [FieldOffset(28)]
            public uint CR3;
            [FieldOffset(32)]
            public uint EIP;
            [FieldOffset(36)]
            public EFlagsEnum EFlags;
            [FieldOffset(40)]
            public uint EAX;
            [FieldOffset(44)]
            public uint ECX;
            [FieldOffset(48)]
            public uint EDX;
            [FieldOffset(52)]
            public uint EBX;
            [FieldOffset(56)]
            public uint ESP;
            [FieldOffset(60)]
            public uint EBP;
            [FieldOffset(64)]
            public uint ESI;
            [FieldOffset(68)]
            public uint EDI;
            [FieldOffset(72)]
            public ushort ES;
            [FieldOffset(76)]
            public ushort CS;
            [FieldOffset(80)]
            public ushort SS;
            [FieldOffset(84)]
            public ushort DS;
            [FieldOffset(88)]
            public ushort FS;
            [FieldOffset(92)]
            public ushort GS;
            [FieldOffset(96)]
            public ushort LDTR;
            [FieldOffset(102)]
            public ushort IOPBOffset;
        }

        [StructLayout(LayoutKind.Explicit, Size = 512)]
        public struct MMXContext {
        }

        [StructLayout(LayoutKind.Explicit, Size = 80)]
        public struct IRQContext {
            [FieldOffset(0)]
            public unsafe MMXContext* MMXContext;

            [FieldOffset(4)]
            public uint EDI;

            [FieldOffset(8)]
            public uint ESI;

            [FieldOffset(12)]
            public uint EBP;

            [FieldOffset(16)]
            public uint ESP;

            [FieldOffset(20)]
            public uint EBX;

            [FieldOffset(24)]
            public uint EDX;

            [FieldOffset(28)]
            public uint ECX;

            [FieldOffset(32)]
            public uint EAX;

            [FieldOffset(36)]
            public uint Interrupt;

            [FieldOffset(40)]
            public uint Param;

            [FieldOffset(44)]
            public uint EIP;

            [FieldOffset(48)]
            public uint CS;

            [FieldOffset(52)]
            public EFlagsEnum EFlags;

            [FieldOffset(56)]
            public uint UserESP;
        }

        private static InterruptDelegate[] mIRQ_Handlers = new InterruptDelegate[256];

        // We used to use:
        //Interrupts.IRQ01 += HandleKeyboardInterrupt;
        // But at one point we had issues with multi cast delegates, so we changed to this single cast option.
        // [1:48:37 PM] Matthijs ter Woord: the issues were: "they didn't work, would crash kernel". not sure if we still have them..
        public static void SetHandler(byte aIrqNo, InterruptDelegate aHandler) {
            mIRQ_Handlers[aIrqNo] = aHandler;
        }

        private static void IRQ(uint irq, ref IRQContext aContext) {
            var xCallback = mIRQ_Handlers[irq];
            if (xCallback != null) {
                xCallback(ref aContext);
            }
        }

        public static void HandleInterrupt_Default(ref IRQContext aContext) {
            if (aContext.Interrupt >= 0x20 && aContext.Interrupt <= 0x2F) {
                if (aContext.Interrupt >= 0x28) {
                    PIC.SignalSecondary();
                } else {
                    PIC.SignalPrimary();
                }
            }
        }

        public delegate void InterruptDelegate(ref IRQContext aContext);
        public delegate void ExceptionInterruptDelegate(ref IRQContext aContext, ref bool aHandled);

        //IRQ 0 - System timer. Reserved for the system. Cannot be changed by a user.
        public static void HandleInterrupt_20(ref IRQContext aContext) {
            //TODO New Kernel
            //PIT.HandleInterrupt();
            PIC.SignalPrimary();
        }

        //public static InterruptDelegate IRQ01;
        //IRQ 1 - Keyboard. Reserved for the system. Cannot be altered even if no keyboard is present or needed.
        public static void HandleInterrupt_21(ref IRQContext aContext) {
            ////Change area
            ////
            //// Triggers IL2CPU error
            //DebugUtil.LogInterruptOccurred(ref aContext);
            IRQ(1, ref aContext);
            ////mIRQ_Handlers[1](ref aContext);
            ////
            //// Old keyboard
            ////Cosmos.Hardware2.Keyboard.HandleKeyboardInterrupt();
            ////
            //// New Keyboard
            ////Cosmos.Hardware2.PC
            ////
            //// - End change area
            //Console.WriteLine("Signal PIC primary");
            PIC.SignalPrimary();
        }

        //IRQ 5 - (Added for ES1370 AudioPCI)
        //public static InterruptDelegate IRQ05;

        public static void HandleInterrupt_25(ref IRQContext aContext) {
            IRQ(5, ref aContext);

            PIC.SignalSecondary();
        }

        //IRQ 09 - (Added for AMD PCNet network card)
        //public static InterruptDelegate IRQ09;

        public static void HandleInterrupt_29(ref IRQContext aContext) {
            IRQ(9, ref aContext);
            PIC.SignalSecondary();
        }

        //IRQ 10 - (Added for VIA Rhine network card)
        //public static InterruptDelegate IRQ10;

        public static void HandleInterrupt_2A(ref IRQContext aContext) {
            //Debugging....
            //DebugUtil.LogInterruptOccurred_Old(aContext);
            //Cosmos.Debug.Debugger.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            //Console.WriteLine("IRQ 11 raised!");

            IRQ(10, ref aContext);

            PIC.SignalSecondary();
        }

        //IRQ 11 - (Added for RTL8139 network card)
        //public static InterruptDelegate IRQ11;

        public static void HandleInterrupt_2B(ref IRQContext aContext) {
            //Debugging....
            //DebugUtil.LogInterruptOccurred_Old(aContext);
            //Cosmos.Debug.Debugger.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            //Console.WriteLine("IRQ 11 raised!");

            IRQ(11, ref aContext);

            PIC.SignalSecondary();
        }

        public static void HandleInterrupt_2C(ref IRQContext aContext) {
            //Debugging....
            //DebugUtil.LogInterruptOccurred_Old(aContext);
            //Cosmos.Debug.Debugger.SendMessage("Interrupts", "Interrupt 2B handler (for RTL)");
            //A> Vermeulen
            //Commented out below
            //Console.WriteLine("IRQ 12 raised!");

            IRQ(12, ref aContext);

            PIC.SignalSecondary();
        }

        //IRQ 14 - Primary IDE. If no Primary IDE this can be changed
        public static void HandleInterrupt_2E(ref IRQContext aContext) {
            Cosmos.Debug.Debugger.SendMessage("IRQ",
                                                  "Primary IDE");
            //Storage.ATAOld.HandleInterruptPrimary();
            //Storage.ATA.ATA.HandleInterruptPrimary();
            PIC.SignalSecondary();
        }

        public static event InterruptDelegate Interrupt30;
        // Interrupt 0x30, enter VMM
        public static void HandleInterrupt_30(ref IRQContext aContext) {
            if (Interrupt30 != null) {
                Interrupt30(ref aContext);
            }
        }

        public static void HandleInterrupt_35(ref IRQContext aContext) {
            Cosmos.Debug.Debugger.SendMessage("Interrupts",
                                                  "Interrupt 35 handler");
            aContext.EAX *= 2;
            aContext.EBX *= 2;
            aContext.ECX *= 2;
            aContext.EDX *= 2;
        }

        //IRQ 15 - Secondary IDE
        public static void HandleInterrupt_2F(ref IRQContext aContext) {
            //Storage.ATA.ATA.HandleInterruptSecondary();
            Cosmos.Debug.Debugger.SendMessage("IRQ",
                                                  "Secondary IDE");
            PIC.SignalSecondary();
        }

        public static void HandleInterrupt_00(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Divide by zero",
                            "EDivideByZero",
                            ref aContext);
        }

        public static void HandleInterrupt_06(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Invalid Opcode",
                            "EInvalidOpcode",
                            ref aContext);
        }

        public static event ExceptionInterruptDelegate GeneralProtectionFault;

        public static void HandleInterrupt_0D(ref IRQContext aContext) {
            bool xHandled = false;
            if (GeneralProtectionFault != null) {
                GeneralProtectionFault(ref aContext,
                                       ref xHandled);
            }
            if (!xHandled) {
                HandleException(aContext.EIP,
                                "General Protection Fault",
                                "GPF",
                                ref aContext);
            }
        }

        public static void HandleInterrupt_01(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Debug Exception",
                            "Debug Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_02(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Non Maskable Interrupt Exception",
                            "Non Maskable Interrupt Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_03(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Breakpoint Exception",
                            "Breakpoint Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_04(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Into Detected Overflow Exception",
                            "Into Detected Overflow Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_05(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Out of Bounds Exception",
                            "Out of Bounds Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_07(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "No Coprocessor Exception",
                            "No Coprocessor Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_08(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Double Fault Exception",
                            "Double Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_09(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Coprocessor Segment Overrun Exception",
                            "Coprocessor Segment Overrun Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0A(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Bad TSS Exception",
                            "Bad TSS Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0B(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Segment Not Present",
                            "Segment Not Present",
                            ref aContext);
        }

        public static void HandleInterrupt_0C(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Stack Fault Exception",
                            "Stack Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0E(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Page Fault Exception",
                            "Page Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_0F(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Unknown Interrupt Exception",
                            "Unknown Interrupt Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_10(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Coprocessor Fault Exception",
                            "Coprocessor Fault Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_11(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Alignment Exception",
                            "Alignment Exception",
                            ref aContext);
        }

        public static void HandleInterrupt_12(ref IRQContext aContext) {
            HandleException(aContext.EIP,
                            "Machine Check Exception",
                            "Machine Check Exception",
                            ref aContext);
        }

        private static void HandleException(uint aEIP,
                                            string aDescription,
                                            string aName,
                                            ref IRQContext ctx) {
            const string SysFault = "*** System Fault ***  ";

            while (true) {
                ;
            }
        }

        // This is to trick IL2CPU to compile it in
        //TODO: Make a new attribute that IL2CPU sees when scanning to force inclusion so we dont have to do this.
        // We dont actually need to cal this method
        public static void Dummy() {
            // Compiler magic
            bool xTest = false;
            if (xTest) {
                unsafe {
                    var xCtx = new IRQContext();
                    HandleInterrupt_Default(ref xCtx);
                    HandleInterrupt_00(ref xCtx);
                    HandleInterrupt_01(ref xCtx);
                    HandleInterrupt_02(ref xCtx);
                    HandleInterrupt_03(ref xCtx);
                    HandleInterrupt_04(ref xCtx);
                    HandleInterrupt_05(ref xCtx);
                    HandleInterrupt_06(ref xCtx);
                    HandleInterrupt_07(ref xCtx);
                    HandleInterrupt_08(ref xCtx);
                    HandleInterrupt_09(ref xCtx);
                    HandleInterrupt_0A(ref xCtx);
                    HandleInterrupt_0B(ref xCtx);
                    HandleInterrupt_0C(ref xCtx);
                    HandleInterrupt_0D(ref xCtx);
                    HandleInterrupt_0E(ref xCtx);
                    HandleInterrupt_0F(ref xCtx);
                    HandleInterrupt_10(ref xCtx);
                    HandleInterrupt_11(ref xCtx);
                    HandleInterrupt_12(ref xCtx);
                    HandleInterrupt_20(ref xCtx);
                    HandleInterrupt_21(ref xCtx);
                    HandleInterrupt_25(ref xCtx);
                    HandleInterrupt_29(ref xCtx);
                    HandleInterrupt_2A(ref xCtx);
                    HandleInterrupt_2B(ref xCtx);
                    HandleInterrupt_2C(ref xCtx);
                    HandleInterrupt_2E(ref xCtx);
                    HandleInterrupt_2F(ref xCtx);
                    HandleInterrupt_30(ref xCtx);
                    HandleInterrupt_35(ref xCtx);
                }
            }
        }

    }
}