#region Using directives

using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;

#endregion

namespace DC4Ever
{
    //lota params ehh??
    #region delegate
    public delegate void DynaRecCall(ref uint reg0 ,  ref uint reg1 ,  ref uint reg2 ,  
                                     ref uint reg3 ,  ref uint reg4 ,  ref uint reg5 ,  
                                     ref uint reg6 ,  ref uint reg7 ,  ref uint reg8 ,
                                     ref uint reg9, ref uint reg10, ref uint reg11,
                                     ref uint reg12, ref uint reg13, ref uint reg14,
                                     ref uint reg15, ref uint reg16, ref uint reg17,
                                     ref uint reg18, ref uint reg19, ref uint reg20,
                                     ref uint reg21, ref uint reg22, ref uint reg23,
                                     ref float reg24, ref float reg25, ref float reg26,
                                     ref float reg27, ref float reg28, ref float reg29,
                                     ref float reg30, ref float reg31, ref float reg32,
                                     ref float reg33, ref float reg34, ref float reg35,
                                     ref float reg36, ref float reg37, ref float reg38,
                                     ref float reg39, ref float reg40, ref float reg41,
                                     ref float reg42, ref float reg43, ref float reg44,
                                     ref float reg45, ref float reg46, ref float reg47,
                                     ref float reg48, ref float reg49, ref float reg50,
                                     ref float reg51, ref float reg52, ref float reg53,
                                     ref float reg54, ref float reg55, ref uint reg56,
                                     ref uint reg57, ref uint reg58, ref uint reg59,
                                     ref uint reg60, ref uint reg61, ref uint reg62,
                                     ref uint reg63, ref uint reg64, ref uint reg65,
                                     ref uint reg66, ref uint reg67, ref uint reg68);
    #endregion 
    public partial class emu
    {
        #region Dynarec Registers Param offsets
        //public static uint[] r = new uint[16];
        public const int rbase = 0;
        //public static uint[] r_bank = new uint[8];
        public const int r_bbase = rbase+16;
        //public static float[] fr=new float[16];//fp regs set 1
        public const int fr_base = r_bbase+ 8;
        //public static float[] xr=new float[16];//fp regs set 2
        public const int xr_base = fr_base+ 16;
        //gbr,ssr,spc,sgr,dbr,vbr;
        public const int gbri = xr_base+16;
        public const int ssri = gbri+1;
        public const int spci = ssri+1;
        public const int sgri = spci+1;
        public const int dbri = sgri+1;
        public const int vbri = dbri+1;
        //public static uint mach,macl,pr,fpul;
        public const int machi = vbri + 1;
        public const int macli = machi + 1;
        public const int pri = macli + 1;
        public const int fpuli = pri + 1;
        //public static uint pc;
        public const int pci = fpuli + 1;
        //sr and fpscr clases
        public const int sri = pci + 1;
        public const int fpscri = sri + 1;
        public const int pCount = fpscri + 1;//count of all ;)
        public const int tuint = pCount;
        public const int tflt = pCount+1;
        #endregion
        static uint ch,cm;  
        static uint br_8_b_level;
        public const uint br_8_b_level_max=1;//maximum8 conditional inlines
        static bool br_8_b_exit_all;//to exit all the inlined br's ;)
        static Random rnd = new Random();
        public const int RecMB = 2*dc.kb;//2k blocks 
        public static CodeCacheEntry[] DynaCache = new CodeCacheEntry[RecMB];//code cache
        public static MethodInfo writemeth= typeof(emu).GetMethod("write");
        public static MethodInfo readmeth = typeof(emu).GetMethod("read");
        public static MethodInfo Cosmeth = typeof(Math).GetMethod("Cos");
        public static MethodInfo Sinmeth = typeof(Math).GetMethod("Sin");
        public static bool RecBlockEnd;
        /// <summary>
        /// Code Cache index
        /// </summary>
        static int CCind = 0;
        /// <summary>
        /// Code Buffer id index
        /// </summary>
        static int CBind = 0;
        //    test.FinaliseIl();
        //    DynaRecCall tb=test.GetCodeBuffer();
        //    uint tst=0,nil=0;
        //
        //    tb(ref tst, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil,
        //       ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil,
        //       ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil,
        //       ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil,
        //       ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil, ref nil,
        //       ref nil, ref nil);

        /// <summary>
        /// Outputs the msil code for 1 opcode...
        /// </summary>
        /// <param name="Opcode to generate"></param>
        /// <param name="IlCoder object to use for generation"></param>
        /// <returns>Returns number of cycles that the opcode \n Given takes to execure.\n If 0 then the buffer must be finalized after this call </returns>
        public static unsafe uint RecSingle(uint opcode, IlCoder to)
        {
            #region opcode msil emit
            switch (opcode >> 12)//proc opcode
            {
                case 0x0://finished rec
                    #region case 0x0

                    switch (opcode & 0xf)
                    {
                        case 0x0://0000
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                        case 0x1://0001
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                        case 0x2://0010
                            #region case 0x2 multi opcodes
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000
                                    //i0000_nnnn_0000_0010();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] = sr.reg; 
                                    to.il.Emit(OpCodes.Ldloc_S, sri);//load sr
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//store it to rn
                                    return ccount[opcode];
                                    break;
                                case 0x1://0001
                                    //i0000_nnnn_0001_0010();
                                    break;
                                case 0x2://0010
                                    //i0000_nnnn_0010_0010();
                                    break;
                                case 0x3://0011
                                    //i0000_nnnn_0011_0010();
                                    break;
                                case 0x4://0100
                                    //i0000_nnnn_0100_0010();
                                    break;
                                case 0x8://1000
                                    //i0000_nnnn_1000_0010();
                                    break;
                                case 0x9://1001
                                    //i0000_nnnn_1001_0010();
                                    break;
                                case 0xA://1010
                                    //i0000_nnnn_1010_0010();
                                    break;
                                case 0xB://1011
                                    //i0000_nnnn_1011_0010();
                                    break;
                                case 0xC://1100
                                    //i0000_nnnn_1100_0010();
                                    break;
                                case 0xD://1101
                                    //i0000_nnnn_1101_0010();
                                    break;
                                case 0xE://1110
                                    //i0000_nnnn_1110_0010();
                                    break;
                                case 0xF://1111
                                    //i0000_nnnn_1111_0010();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x3://0011
                            #region case 0x3 multi opcodes
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000
                                    //i0000_nnnn_0000_0011();
                                    break;
                                case 0x2://0010
                                    //i0000_nnnn_0010_0011();
                                    n = (uint)(opcode >> 8) & 0x0F;
                                    //delayslot = r[n] + pc + 4;
                                    //pc_funct = 2;//delay 1
                                    to.il.Emit(OpCodes.Ldc_I4, pc + 4);   //load the value that we must jump , part 1
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);   //load the value that we must jump , part 2 and add them
                                    to.il.Emit(OpCodes.Add);                //add them 
                                    to.il.Emit(OpCodes.Stloc_S, pci);     //store the result to pc register
                                    uint temp = RecSingle(RecNextOpcode(), to);// do the delayslot instruction
                                    RecBlockEnd = true;
                                    return (uint)(temp + ccount[opcode]);
                                    break;
                                case 0x8://1000
                                    //i0000_nnnn_1000_0011();
                                    break;
                                case 0x9://1001
                                    //i0000_nnnn_1001_0011();
                                    break;
                                case 0xA://1010
                                    //i0000_nnnn_1010_0011();
                                    break;
                                case 0xB://1011
                                    //i0000_nnnn_1011_0011();
                                    break;
                                case 0xC://1100
                                    //i0000_nnnn_1100_0011();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x4://0100
                            //i0000_nnnn_mmmm_0100();
                            break;
                        case 0x5://0101
                            //i0000_nnnn_mmmm_0101();
                            n = (uint)(opcode >> 8) & 0x0F;
                            m = (uint)(opcode >> 4) & 0x0F;
                            //write(r[0] + r[n], r[m] & 0xFFFF, 2);
                            to.il.Emit(OpCodes.Ldloc_S, rbase);//values to add
                            to.il.Emit(OpCodes.Ldloc_S, rbase+n);
                            to.il.Emit(OpCodes.Add);//add them , this is param 1
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//param 2
                            to.il.Emit(OpCodes.Ldc_I4_2); //param 3
                            to.il.Emit(OpCodes.Call, writemeth);
                            return ccount[opcode];
                            break;
                        case 0x6://0110
                            //i0000_nnnn_mmmm_0110();
                            break;
                        case 0x7://0111
                            //i0000_nnnn_mmmm_0111();
                            n = (uint)(opcode >> 8) & 0x0F;
                            m = (uint)(opcode >> 4) & 0x0F;
                            //macl = (uint)(((int)r[n] * (int)r[m]) & 0xFFFFFFFF);
                            to.il.Emit(OpCodes.Ldloc_S, rbase+n);//values to mul
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);
                            to.il.Emit(OpCodes.Mul);//mul them , push rez
                            to.il.Emit(OpCodes.Stloc_S, macli);//store to macl
                            return ccount[opcode];
                            break;
                        case 0x8://1000
                            #region case 0x8 multi opcodes
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000
                                    ////i0000_0000_0000_1000();
                                    break;
                                case 0x1://0001
                                    ////i0000_0000_0001_1000();
                                    break;
                                case 0x2://0010
                                    ////i0000_0000_0010_1000();
                                    break;
                                case 0x3://0011
                                    //i0000_0000_0011_1000();
                                    break;
                                case 0x4://0100
                                    //i0000_0000_0100_1000();
                                    break;
                                case 0x5://0101
                                    //i0000_0000_0101_1000();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x9://1001
                            #region case 0x9 multi opcodes
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000 heeheheeh nop
                                    //i0000_0000_0000_1001();
                                    return ccount[opcode];
                                    break;
                                case 0x1://0001
                                    //i0000_0000_0001_1001();
                                    break;
                                case 0x2://0010
                                    //i0000_nnnn_0010_1001();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0xA://1010
                            #region case 0xA multi opcodes
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000
                                    //i0000_nnnn_0000_1010();
                                    break;
                                case 0x1://0001
                                    //i0000_nnnn_0001_1010();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] = macl;
                                    to.il.Emit(OpCodes.Ldloc_S, macli);//push macl
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//pop it to reg n
                                    return ccount[opcode];
                                    break;
                                case 0x2://0010
                                    //i0000_nnnn_0010_1010();
                                    break;
                                case 0x5://0101
                                    //i0000_nnnn_0101_1010();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] = (uint)(int)fpul;
                                    to.il.Emit(OpCodes.Ldloc_S, fpuli);//push fpul
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//pop it to reg n
                                    return ccount[opcode];
                                    break;
                                case 0x6://0110
                                    //i0000_nnnn_0110_1010();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0xB://1011
                            #region case 0xB multi opcodes
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000
                                    //i0000_0000_0000_1011();
                                    //delayslot = pr;
                                    //pc_funct = 2;//delay slot 1
                                    to.il.Emit(OpCodes.Ldloc_S, pri);//push pr
                                    to.il.Emit(OpCodes.Stloc_S,pci);//pop it to pc
                                    RecBlockEnd = true;//end of block
                                    return RecSingle(RecNextOpcode(), to) +ccount[opcode];// do the delayslot instruction and ret
                                    break;
                                case 0x1://0001
                                    //i0000_0000_0001_1011();
                                    break;
                                case 0x2://0010
                                    //i0000_0000_0010_1011();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0xC://1100
                            //i0000_nnnn_mmmm_1100();
                            n = (uint)(opcode >> 8) & 0x0F;
                            m = (uint)(opcode >> 4) & 0x0F;
                            //r[n] = (uint)(sbyte)read(r[0] + r[m], 1);
                            to.il.Emit(OpCodes.Ldloc_S, rbase );//push r0
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//push rm
                            to.il.Emit(OpCodes.Add);            //add them and push rez
                            to.il.Emit(OpCodes.Conv_I1);        //convert to singed
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//save it to rn
                            return ccount[opcode];
                            break;
                        case 0xD://1101
                            //i0000_nnnn_mmmm_1101();
                            break;
                        case 0xE://1110
                            //i0000_nnnn_mmmm_1110();
                            break;
                        case 0xF://1111
                            //i0000_nnnn_mmmm_1111();
                            break;
                    }
                    #endregion
                    break;
                case 0x1://finished rec
                    //i0001_nnnn_mmmm_iiii();
                    n = (uint)(opcode >> 8) & 0x0F;
                    m = (uint)(opcode >> 4) & 0x0F;
                    disp = opcode & 0x0F;
                    //write(r[n] + disp * 4, r[m], 4); do this in msil
                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load register
                    to.il.Emit(OpCodes.Ldc_I4, disp * 4);//load value to add
                    to.il.Emit(OpCodes.Add);//add them 
                    to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load value to write (param #2)
                    to.il.Emit(OpCodes.Ldc_I4_4);//load 4 for 4 bytes write (param #3)
                    to.il.Emit(OpCodes.Call,writemeth );//call write
                    return ccount[opcode];
                    break;
                case 0x2://finished rec
                    #region case 0x2
                    switch (opcode & 0xf)
                    {
                        case 0x0://0000
                            //i0010_nnnn_mmmm_0000();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //write(r[n], r[m] & 0xFF, 1);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Conv_U1);           //convert to byte
                            to.il.Emit(OpCodes.Ldc_I4_1);//load 1
                            to.il.Emit(OpCodes.Call, writemeth);///do the write
                            return ccount[opcode];//return
                            break;
                        case 0x1://0001
                            //i0010_nnnn_mmmm_0001();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //write(r[n], r[m] & 0xFFFF, 2);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Conv_U2);           //convert to word
                            to.il.Emit(OpCodes.Ldc_I4_2);//load 2 (word write)
                            to.il.Emit(OpCodes.Call, writemeth);//do the write
                            return ccount[opcode];//return
                            break;
                        case 0x2://0010
                            //i0010_nnnn_mmmm_0010();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //write(r[n], r[m], 4);//at r[n],r[m]
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Ldc_I4_4);//load 4 (dword)
                            to.il.Emit(OpCodes.Call, writemeth);//do the write
                            return ccount[opcode];//return
                            break;
                        case 0x4://0100
                            //i0010_nnnn_mmmm_0100();
                            break;
                        case 0x5://0101
                            //i0010_nnnn_mmmm_0101();
                            break;
                        case 0x6://0110
                            //i0010_nnnn_mmmm_0110();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] -= 4;
                            //write(r[n], r[m], 4);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldc_I4_S, 4);       //load -4 
                            to.il.Emit(OpCodes.Sub);               //do the -
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//store n

                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Ldc_I4_4);//load 4 (dword)
                            to.il.Emit(OpCodes.Call, writemeth);//do the write
                            return ccount[opcode];//return
                            break;
                        case 0x7://0111
                            //i0010_nnnn_mmmm_0111();
                            break;
                        case 0x8://1000
                            //i0010_nnnn_mmmm_1000();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //dc.dcon.WriteLine("non emulated opcode on recompiler mode...");
                            //if ((r[n] & r[m]) > 0)
                            //    sr.T = 0;
                            //else
                            //    sr.T = 1;
                            Label T0 = to.il.DefineLabel();
                            Label fend = to.il.DefineLabel();
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.And);//bitwise and , pushed result
                            to.il.Emit(OpCodes.Brtrue_S , T0);// if not 0 then them jusm T0
                            to.il.Emit(OpCodes.Ldc_I4, sr_T_bit_set);
                            to.il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                            to.il.Emit(OpCodes.Or);          //bitwise or to set T in sr
                            to.il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
                            to.il.Emit(OpCodes.Br_S, fend);//finished , goto to end
                            to.il.MarkLabel(T0);//set t to 0
                            to.il.Emit(OpCodes.Ldc_I4, sr_T_bit_reset);
                            to.il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                            to.il.Emit(OpCodes.And);          //bitwise And to unset T in sr
                            to.il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
                            to.il.MarkLabel(fend);//function end
                            return ccount[opcode];//return
                            break;
                        case 0x9://1001
                            //i0010_nnnn_mmmm_1001();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] &= r[m];
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.And);               //bitwise and
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//store n
                            return ccount[opcode];                 //return
                            break;
                        case 0xA://1010
                            //i0010_nnnn_mmmm_1010();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] ^= r[m];
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Xor);               //bitwise Xor
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//store n
                            return ccount[opcode];                 //return
                            break;
                        case 0xB://1011
                            //i0010_nnnn_mmmm_1011();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] |= r[m];
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Or);               //bitwise or
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//store n
                            return ccount[opcode];                 //return
                            break;
                        case 0xC://1100
                            //i0010_nnnn_mmmm_1100();
                            break;
                        case 0xD://1101
                            //i0010_nnnn_mmmm_1101();
                            break;
                        case 0xE://1110
                            //i0010_nnnn_mmmm_1110();
                            break;
                        case 0xF://1111
                            //i0010_nnnn_mmmm_1111();
                            break;
                        default:
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                case 0x3://finished rec
                    #region case 0x3
                    switch (opcode & 0xf)
                    {
                        case 0x0://0000
                            //i0011_nnnn_mmmm_0000();
                            break;
                        case 0x2://0010
                            //i0011_nnnn_mmmm_0010();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //if (r[n] >= r[m])
                            //    sr.T = 1;
                            //else
                            //    sr.T = 0;
                            Label T0 = to.il.DefineLabel();
                            Label fend = to.il.DefineLabel();
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Bge_S, T0);// if > then set t=1
                            to.il.Emit(OpCodes.Ldc_I4, sr_T_bit_set);
                            to.il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                            to.il.Emit(OpCodes.Or);          //bitwise or to set T in sr
                            to.il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
                            to.il.Emit(OpCodes.Br_S, fend);//finished , goto to end
                            to.il.MarkLabel(T0);//set t to 0
                            to.il.Emit(OpCodes.Ldc_I4, sr_T_bit_reset);
                            to.il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                            to.il.Emit(OpCodes.And);          //bitwise And to unset T in sr
                            to.il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
                            to.il.MarkLabel(fend);//function end
                            return ccount[opcode];//return
                            break;
                        case 0x3://0011
                            //i0011_nnnn_mmmm_0011();
                            break;
                        case 0x4://0100
                            //i0011_nnnn_mmmm_0100();
                            break;
                        case 0x5://0101
                            //i0011_nnnn_mmmm_0101();
                            break;
                        case 0x6://0110
                            //i0011_nnnn_mmmm_0110();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;

                            //if (r[n] > r[m])
                            //    sr.T = 1;
                            //else
                             //    sr.T = 0;
                            T0 = to.il.DefineLabel();
                            fend = to.il.DefineLabel();
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Bgt_S, T0);// if > then set t=1
                            to.il.Emit(OpCodes.Ldc_I4, sr_T_bit_set);
                            to.il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                            to.il.Emit(OpCodes.Or);          //bitwise or to set T in sr
                            to.il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
                            to.il.Emit(OpCodes.Br_S, fend);//finished , goto to end
                            to.il.MarkLabel(T0);//set t to 0
                            to.il.Emit(OpCodes.Ldc_I4, sr_T_bit_reset);
                            to.il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                            to.il.Emit(OpCodes.And);          //bitwise And to unset T in sr
                            to.il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
                            to.il.MarkLabel(fend);//function end
                            return ccount[opcode];//return
                            break;
                        case 0x7://0111
                            //i0011_nnnn_mmmm_0111();
                            break;
                        case 0x8://1000
                            //i0011_nnnn_mmmm_1000();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //rn = (int)r[n];
                            //rm = (int)r[m];
                            //rn -= rm;
                            //r[n] = (uint)rn;
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Sub);// substarct them
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//store n
                            return ccount[opcode];//return
                            break;
                        case 0xA://1010
                            //i0011_nnnn_mmmm_1010();
                            break;
                        case 0xB://1011
                            //i0011_nnnn_mmmm_1011();
                            break;
                        case 0xC://1100
                            //i0011_nnnn_mmmm_1100();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //rm = (int)r[m];
                            //rn = (int)r[n];
                            //rn += rm;
                            //r[n] = (uint)rn;
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load n
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load m
                            to.il.Emit(OpCodes.Add);// substarct them
                            to.il.Emit(OpCodes.Stloc_S, rbase + n);//store n
                            return ccount[opcode];//return
                            break;
                        case 0xD://1101
                            //i0011_nnnn_mmmm_1101();
                            break;
                        case 0xE://1110
                            //i0011_nnnn_mmmm_1110();
                            break;
                        case 0xF://1111
                            //i0011_nnnn_mmmm_1111();
                            break;
                        default:
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                case 0x4://finished rec
                    #region case 0x4
                    switch (opcode & 0xf)
                    {
                        case 0x0://0000 rec
                            #region 0x0 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0000
                                    //i0100_nnnn_0000_0000();
                                    break;
                                case 0x1://0100_xxxx_0001_0000
                                    //i0100_nnnn_0001_0000();
                                    n = (opcode >> 8) & 0x0F;
                                    //rn = (int)(r[n]);
                                    //--rn;
                                    //if (rn == 0)
                                    //    sr.T = 1;
                                    //else
                                    //    sr.T = 0;
                                    //r[n] = (uint)rn;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_1);         //load 1  
                                    to.il.Emit(OpCodes.Sub);              //do the sub  
                                    to.il.Emit(OpCodes.Dup);              //duplicate klas stack item
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//store
                                    to.il.Emit(OpCodes.Ldc_I4_0);         // if register=0
                                    SetTeq(to.il, 1, 0);                  //then t=1 else t=0  
                                    return ccount[opcode];//return
                                    break;
                                case 0x2://0100_xxxx_0010_0000
                                    //i0100_nnnn_0010_0000();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x1://0001 rec
                            #region 0x1 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0001
                                    //i0100_nnnn_0000_0001();
                                    n = (opcode >> 8) & 0x0F;
                                    //sr.T = r[n] & 0x1;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_1);         //load 1
                                    to.il.Emit(OpCodes.And);              //And them
                                    to.il.Emit(OpCodes.Ldc_I4_0);         //load 0
                                    SetTeq(to.il, 1, 0);                  // if and=1 then t=1 else t=0
                                    //r[n] >>= 1;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_1);         //by 1  
                                    to.il.Emit(OpCodes.Shr_Un);           //unsined shift right by 1  
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//store the register
                                    return ccount[opcode];//return
                                    break;
                                case 0x1://0100_xxxx_0001_0001
                                    //i0100_nnnn_0001_0001();
                                    break;
                                case 0x2://0100_xxxx_0010_0001
                                    //i0100_nnnn_0010_0001();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x2://0010 rec
                            #region 0x2 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0010
                                    //i0100_nnnn_0000_0010();
                                    break;
                                case 0x1://0100_xxxx_0001_0010
                                    //i0100_nnnn_0001_0010();
                                    break;
                                case 0x2://0100_xxxx_0010_0010
                                    //i0100_nnnn_0010_0010();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] -= 4;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);//load the 4
                                    to.il.Emit(OpCodes.Sub);           //sub them
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//store the register
                                    //write(r[n], pr, 4);
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //load the register
                                    to.il.Emit(OpCodes.Ldloc_S, pri);       //load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);//load the 4
                                    to.il.Emit(OpCodes.Call, writemeth);    //call teh function
                                    return ccount[opcode];//return
                                    break;
                                case 0x5://0100_xxxx_0101_0010
                                    //i0100_nnnn_0101_0010();
                                    break;
                                case 0x6://0100_xxxx_0110_0010
                                    //i0100_nnnn_0110_0010();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] -= 4;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);//load the 4
                                    to.il.Emit(OpCodes.Sub);           //sub them
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//store the register
                                    //write(r[n], fpscr.reg, 4);
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //load the register
                                    to.il.Emit(OpCodes.Ldloc_S, fpscri );   //load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);           //load the 4
                                    to.il.Emit(OpCodes.Call, writemeth);    //call the function
                                    return ccount[opcode];//return
                                    break;
                                case 0x8://0100_xxxx_1000_0010
                                    //i0100_nnnn_1000_0010();
                                    break;
                                case 0x9://0100_xxxx_1001_0010
                                    //i0100_nnnn_1001_0010();
                                    break;
                                case 0xA://0100_xxxx_1010_0010
                                    //i0100_nnnn_1010_0010();
                                    break;
                                case 0xB://0100_xxxx_1011_0010
                                    //i0100_nnnn_1011_0010();
                                    break;
                                case 0xC://0100_xxxx_1100_0010
                                    //i0100_nnnn_1100_0010();
                                    break;
                                case 0xD://0100_xxxx_1101_0010
                                    //i0100_nnnn_1101_0010();
                                    break;
                                case 0xE://0100_xxxx_1110_0010
                                    //i0100_nnnn_1110_0010();
                                    break;
                                case 0xF://0100_xxxx_1111_0010
                                    //i0100_nnnn_1111_0010();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x3://0011 rec
                            #region 0x3 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0011
                                    //i0100_nnnn_0000_0011();
                                    break;
                                case 0x1://0100_xxxx_0001_0011
                                    //i0100_nnnn_0001_0011();
                                    break;
                                case 0x2://0100_xxxx_0010_0011
                                    //i0100_nnnn_0010_0011();
                                    break;
                                case 0x3://0100_xxxx_0011_0011
                                    //i0100_nnnn_0011_0011();
                                    break;
                                case 0x4://0100_xxxx_0100_0011
                                    //i0100_nnnn_0100_0011();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x4://0100 rec
                            #region 0x4 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0100
                                    //i0100_nnnn_0000_0100();
                                    break;
                                case 0x2://0100_xxxx_0010_0100
                                    //i0100_nnnn_0010_0100();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x5://0101 rec
                            #region 0x5 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0101
                                    //i0100_nnnn_0000_0101();
                                    break;
                                case 0x1://0100_xxxx_0001_0101
                                    //i0100_nnnn_0001_0101();
                                    break;
                                case 0x2://0100_xxxx_0010_0101
                                    //i0100_nnnn_0010_0101();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x6://0110 rec
                            #region 0x6 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0110
                                    //i0100_nnnn_0000_0110();
                                    break;
                                case 0x1://0100_xxxx_0001_0110
                                    //i0100_nnnn_0001_0110();
                                    break;
                                case 0x2://0100_xxxx_0010_0110
                                    //i0100_nnnn_0010_0110();
                                    m = (opcode >> 8) & 0x0F;
                                    //pr = read(r[m], 4);
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);          //load the 4
                                    to.il.Emit(OpCodes.Call ,readmeth);    //call read
                                    to.il.Emit(OpCodes.Stloc_S, pri);      //store the return val
                                    //r[m] += 4;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);          //load the 4
                                    to.il.Emit(OpCodes.Add );              //Add them
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//load the register
                                    return ccount[opcode];//return
                                    break;
                                case 0x5://0100_xxxx_0101_0110
                                    //i0100_nnnn_0101_0110();
                                    break;
                                case 0x6://0100_xxxx_0110_0110
                                    //i0100_nnnn_0110_0110();
                                    m = (opcode >> 8) & 0x0F;
                                    //fpscr.reg = read(r[m], 4);
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);          //load the 4
                                    to.il.Emit(OpCodes.Call, readmeth);    //call read
                                    to.il.Emit(OpCodes.Stloc_S, fpscri);      //store the return val
                                    //r[m] += 4;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load the register
                                    to.il.Emit(OpCodes.Ldc_I4_4);          //load the 4
                                    to.il.Emit(OpCodes.Add);              //Add them
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//load the register
                                    return ccount[opcode];//return
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x7://0111 rec
                            #region 0x7 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_0111
                                    //i0100_nnnn_0000_0111();
                                    break;
                                case 0x1://0100_xxxx_0001_0111
                                    //i0100_nnnn_0001_0111();
                                    break;
                                case 0x2://0100_xxxx_0010_0111
                                    //i0100_nnnn_0010_0111();
                                    break;
                                case 0x3://0100_xxxx_0011_0111
                                    //i0100_nnnn_0011_0111();
                                    break;
                                case 0x4://0100_xxxx_0100_0111
                                    //i0100_nnnn_0100_0111();
                                    break;
                                case 0x8://0100_xxxx_1000_0111
                                    //i0100_nnnn_1000_0111();
                                    break;
                                case 0x9://0100_xxxx_1001_0111
                                    //i0100_nnnn_1001_0111();
                                    break;
                                case 0xA://0100_xxxx_1010_0111
                                    //i0100_nnnn_1010_0111();
                                    break;
                                case 0xB://0100_xxxx_1011_0111
                                    //i0100_nnnn_1011_0111();
                                    break;
                                case 0xC://0100_xxxx_1100_0111
                                    //i0100_nnnn_1100_0111();
                                    break;
                                case 0xD://0100_xxxx_1101_0111
                                    //i0100_nnnn_1101_0111();
                                    break;
                                case 0xE://0100_xxxx_1110_0111
                                    //i0100_nnnn_1110_0111();
                                    break;
                                case 0xF://0100_xxxx_1111_0111
                                    //i0100_nnnn_1111_0111();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x8://1000 rec
                            #region 0x8 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_1000
                                    //i0100_nnnn_0000_1000();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] <<= 2;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //number
                                    to.il.Emit(OpCodes.Ldc_I4_2);           //by2
                                    to.il.Emit(OpCodes.Shl);                //swift
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store      
                                    return ccount[opcode];//return
                                    break;
                                case 0x1://0100_xxxx_0001_1000
                                    //i0100_nnnn_0001_1000();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] <<= 8;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //number
                                    to.il.Emit(OpCodes.Ldc_I4_8);           //by8
                                    to.il.Emit(OpCodes.Shl);                //swift
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store   
                                    return ccount[opcode];//return
                                    break;
                                case 0x2://0100_xxxx_0010_1000
                                    //i0100_nnnn_0010_1000();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] <<= 16;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //number
                                    to.il.Emit(OpCodes.Ldc_I4,16);           //by16
                                    to.il.Emit(OpCodes.Shl);                //swift
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store   
                                    return ccount[opcode];//return
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0x9://1001 rec
                            #region 0x9 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_1001
                                    //i0100_nnnn_0000_1001();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] >>= 2;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //number
                                    to.il.Emit(OpCodes.Ldc_I4_2);           //by2
                                    to.il.Emit(OpCodes.Shr_Un);                //shift
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store   
                                    return ccount[opcode];//return
                                    break;
                                case 0x1://0100_xxxx_0001_1001
                                    //i0100_nnnn_0001_1001();
                                    break;
                                case 0x2://0100_xxxx_0010_1001
                                    //i0100_nnnn_0010_1001();
                                    n = (opcode >> 8) & 0x0F;
                                    //r[n] >>= 16;
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n); //number
                                    to.il.Emit(OpCodes.Ldc_I4,16);           //by16
                                    to.il.Emit(OpCodes.Shr_Un);                //shift
                                    to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store   
                                    return ccount[opcode];//return
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;

                        case 0xA://1010 rec
                            #region 0x9 multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_1010
                                    //i0100_nnnn_0000_1010();
                                    break;
                                case 0x1://0100_xxxx_0001_1010
                                    //i0100_nnnn_0001_1010();
                                    break;
                                case 0x2://0100_xxxx_0010_1010
                                    //i0100_nnnn_0010_1010();
                                    m = (opcode >> 8) & 0x0F;
                                    //pr = r[m];
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load
                                    to.il.Emit(OpCodes.Stloc_S, rbase + pri); //and store  
                                    return ccount[opcode];//return
                                    break;
                                case 0x5://0100_xxxx_0101_1010
                                    //i0100_nnnn_0101_1010();
                                    m = (opcode >> 8) & 0x0F;
                                    //fpul = r[m];
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load
                                    to.il.Emit(OpCodes.Stloc_S, rbase + fpuli); //and store 
                                    return ccount[opcode];//return
                                    break;
                                case 0x6://0100_xxxx_0110_1010
                                    //i0100_nnnn_0110_1010();
                                    m = (opcode >> 8) & 0x0F;
                                    //fpscr.reg = r[m];
                                    dc.dcon.WriteLine("Write to fpscr is not totaly corect , not lanching event changes..");
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load
                                    to.il.Emit(OpCodes.Stloc_S, rbase + fpscri); //and store 
                                    return ccount[opcode];//return
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;

                        case 0xB://1011 rec
                            #region 0xB multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_1011
                                    //i0100_nnnn_0000_1011();
                                    n = (opcode >> 8) & 0x0F;
                                    //pr = pc + 4;
                                    //delayslot = r[n];
                                    //pc_funct = 2;//jump with delay
                                    to.il.Emit(OpCodes.Ldc_I4, pc + 4);   //load the value that we must return  
                                    to.il.Emit(OpCodes.Stloc_S, pri);   //store it to pr
                                    to.il.Emit(OpCodes.Ldloc_S, rbase +n);//load jump address                                    
                                    to.il.Emit(OpCodes.Stloc_S, pci);     //store it to the pc register
                                    uint temp = RecSingle(RecNextOpcode(), to);// do the delayslot instruction
                                    temp+=ccount[opcode];
                                    RecBlockEnd = true;
                                    return temp;
                                    break;
                                case 0x1://0100_xxxx_0001_1011
                                    //i0100_nnnn_0001_1011();
                                    break;
                                case 0x2://0100_xxxx_0010_1011
                                    //i0100_nnnn_0010_1011();
                                    n = (opcode >> 8) & 0x0F;
                                    //delayslot = r[n];
                                    //pc_funct = 2;//jump with delay 1
                                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load jump address                                    
                                    to.il.Emit(OpCodes.Stloc_S, pci);     //store it to the pc register
                                    uint temp2 = RecSingle(RecNextOpcode(), to);// do the delayslot instruction
                                    temp2 += ccount[opcode];
                                    RecBlockEnd = true;
                                    return temp2;
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0xC://1100 rec
                            //i0100_nnnn_mmmm_1100();
                            break;
                        case 0xD://1101 rec
                            //i0100_nnnn_mmmm_1101();
                            break;
                        case 0xE://1110 rec
                            #region 0xE multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0100_xxxx_0000_1110
                                    //i0100_nnnn_0000_1110();
                                    break;
                                case 0x1://0100_xxxx_0001_1110
                                    //i0100_nnnn_0001_1110();
                                    break;
                                case 0x2://0100_xxxx_0010_1110
                                    //i0100_nnnn_0010_1110();
                                    break;
                                case 0x3://0100_xxxx_0011_1110
                                    //i0100_nnnn_0011_1110();
                                    break;
                                case 0x4://0100_xxxx_0100_1110
                                    //i0100_nnnn_0100_1110();
                                    break;
                                case 0x8://0100_xxxx_1000_1110
                                    //i0100_nnnn_1000_1110();
                                    break;
                                case 0x9://0100_xxxx_1001_1110
                                    //i0100_nnnn_1001_1110();
                                    break;
                                case 0xA://0100_xxxx_1010_1110
                                    //i0100_nnnn_1010_1110();
                                    break;
                                case 0xB://0100_xxxx_1011_1110
                                    //i0100_nnnn_1011_1110();
                                    break;
                                case 0xC://0100_xxxx_1100_1110
                                    //i0100_nnnn_1100_1110();
                                    break;
                                case 0xD://0100_xxxx_1101_1110
                                    //i0100_nnnn_1101_1110();
                                    break;
                                case 0xE://0100_xxxx_1110_1110
                                    //i0100_nnnn_1110_1110();
                                    break;
                                case 0xF://0100_xxxx_1111_1110
                                    //i0100_nnnn_1111_1110();
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0xF://1111
                            //i0100_nnnn_mmmm_1111();
                            break;
                        default:
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                case 0x5://finished rec
                    //i0101_nnnn_mmmm_iiii();
                    n = (opcode >> 8) & 0x0F;
                    m = (opcode >> 4) & 0x0F;
                    disp = (opcode & 0x0F) * 4;
                    //r[n] = read(r[m] + disp, 4); : do this is msil
                    to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load register m
                    to.il.Emit(OpCodes.Ldc_I4, disp);       //load disp
                    to.il.Emit(OpCodes.Add);                //add them
                    to.il.Emit(OpCodes.Ldc_I4_4);           //push 4 (param #2)
                    to.il.Emit(OpCodes.Call, readmeth);//call read
                    to.il.Emit(OpCodes.Stloc_S, rbase + n); //store result onto register n
                    return ccount[opcode];
                    break;
                case 0x6://finished
                    #region case 0x6
                    switch (opcode & 0xf)
                    {
                        case 0x0://0000
                            //i0110_nnnn_mmmm_0000();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] = (uint)(sbyte)read(r[m], 1);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //add to read
                            to.il.Emit(OpCodes.Ldc_I4_1);           //1
                            to.il.Emit(OpCodes.Call,readmeth);      //call read
                            to.il.Emit(OpCodes.Conv_I1);            //make it sbyte extended
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0x1://0001
                            //i0110_nnnn_mmmm_0001();
                            break;
                        case 0x2://0010
                            //i0110_nnnn_mmmm_0010();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] = read(r[m], 4);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //add to read
                            to.il.Emit(OpCodes.Ldc_I4_4);           //4
                            to.il.Emit(OpCodes.Call, readmeth);     //call read
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0x3://0011
                            //i0110_nnnn_mmmm_0011();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] = r[m];
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load source register
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0x4://0100
                            //i0110_nnnn_mmmm_0100();
                            break;
                        case 0x5://0101
                            //i0110_nnnn_mmmm_0101();
                            break;
                        case 0x6://0110
                            //i0110_nnnn_mmmm_0110();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;

                            //r[n] = read(r[m], 4);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //add to read
                            to.il.Emit(OpCodes.Ldc_I4_4);           //4
                            to.il.Emit(OpCodes.Call, readmeth);      //call read
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            if (n != m)//if diferent reg then add 4 ;)
                            {
                                //r[m] += 4;
                                to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load reg
                                to.il.Emit(OpCodes.Ldc_I4_4);           //param #2 (4)
                                to.il.Emit(OpCodes.Add);                //add them
                                to.il.Emit(OpCodes.Stloc_S, rbase + m); //and store it back
                            }
                            return ccount[opcode];//return
                            break;
                        case 0x7://0111
                            //i0110_nnnn_mmmm_0111();
                            break;
                        case 0x8://1000
                            //i0110_nnnn_mmmm_1000();
                            break;
                        case 0x9://1001
                            //i0110_nnnn_mmmm_1001();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] = ((r[m] >> 16) & 0xFFFF) | ((r[m] << 16) & 0xFFFF0000);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load m 
                            to.il.Emit(OpCodes.Ldc_I4,16);          //16
                            to.il.Emit(OpCodes.Shr_Un);             //>>16
                            to.il.Emit(OpCodes.Ldc_I4, 0xFFFF);     //0xFFFF
                            to.il.Emit(OpCodes.And);                //and 0xFFFF
                            //par 1 finished
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n); //load n 
                            to.il.Emit(OpCodes.Ldc_I4, 16);         //16
                            to.il.Emit(OpCodes.Shl);                //<<16
                            to.il.Emit(OpCodes.Ldc_I4, 0xFFFF0000); //0xFFFF0000
                            to.il.Emit(OpCodes.And);                //and 0xFFFF0000
                            //par 2 finished
                            //now or them
                            to.il.Emit(OpCodes.Or);
                            //and finaly store it
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0xA://1010
                            //i0110_nnnn_mmmm_1010();
                            break;
                        case 0xB://1011
                            //i0110_nnnn_mmmm_1011();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //rm = (int)r[m];
                            //r[n] = (uint)-rm;
                            to.il.Emit(OpCodes.Ldc_I4_0);           //load 0
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load m register
                            to.il.Emit(OpCodes.Sub);                //sub them 
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0xC://1100
                            //i0110_nnnn_mmmm_1100();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] = r[m] & 0xFF;
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load m register
                            to.il.Emit(OpCodes.Ldc_I4, 0xFF);       //load 0xFF
                            to.il.Emit(OpCodes.And);                //and them 
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0xD://1101
                            //i0110_nnnn_mmmm_1101();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //r[n] = r[m] & 0x0000FFFF;
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m); //load m register
                            to.il.Emit(OpCodes.Ldc_I4, 0x0000FFFF); //load 0x0000FFFF
                            to.il.Emit(OpCodes.And);                //and them 
                            to.il.Emit(OpCodes.Stloc_S, rbase + n); //and store to dest register
                            return ccount[opcode];//return
                            break;
                        case 0xE://1110
                            //i0110_nnnn_mmmm_1110();
                            break;
                        case 0xF://1111
                            //i0110_nnnn_mmmm_1111();
                            break;
                        default:
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                case 0x7://finished rec
                    //i0111_nnnn_iiii_iiii();
                    n = (opcode >> 8) & 0x0F;
                    rm = (int)(sbyte)(opcode & 0xFF);
                    //rn = ((int)r[n]) + rm;  :Do this in msil
                    //r[n] = (uint)rn;
                    to.il.Emit(OpCodes.Ldloc_S, rbase + n);//load reg
                    to.il.Emit(OpCodes.Ldc_I4, rm); //load value to add (singed)
                    to.il.Emit(OpCodes.Add);        // add them 
                    to.il.Emit(OpCodes.Stloc_S, rbase + n);//save the result back
                    return ccount[opcode];
                    break;
                case 0x8://finished
                    #region case 0x8
                    switch ((opcode >> 8) & 0xf)
                    {
                        case 0x0://0000
                            //i1000_0000_mmmm_iiii();
                            break;
                        case 0x1://0001
                            //i1000_0001_mmmm_iiii();
                            break;
                        case 0x4://0100
                            //i1000_0100_mmmm_iiii();
                            break;
                        case 0x5://0101
                            //i1000_0101_mmmm_iiii();
                            break;
                        case 0x8://1000
                            //i1000_1000_iiii_iiii();
                            m = (uint)(sbyte)(opcode & 0xFF);
                            //if (r[0] == m)
                            //    sr.T = 1;
                            //else
                            //    sr.T = 0;
                            to.il.Emit(OpCodes.Ldloc_S,rbase);//load r[0]
                            to.il.Emit(OpCodes.Ldc_I4,m);//load m
                            SetTeq(to.il, 1, 0);// if true then t=1 else t=0
                            return ccount[opcode];//return
                            break;
                        case 0x9://1001
                            //i1000_1001_iiii_iiii();
                            //if (sr.T == 1)
                            Label t1 = to.il.DefineLabel();
                            GetTBit(to.il);                     //load tbit
                            to.il.Emit(OpCodes.Brfalse_S, t1);//if =0 then skip this
                            //{
                            //  delayslot = (uint)((sbyte)(opcode & 0xFF)) * 2 + pc + 4;
                            to.il.Emit(OpCodes.Ldc_I4, (uint)((sbyte)(opcode & 0xFF)) * 2 + pc + 4);
                            //  pc_funct = 1;//direct jump
                            to.il.Emit(OpCodes.Stloc_S, pci);//store delayslot to pc register
                            //}
                            to.il.MarkLabel(t1);//end if t=0
                            RecBlockEnd = true;
                            return ccount[opcode];//return
                            break;
                        case 0xB://1011
                            //i1000_1011_iiii_iiii();
                            //if (sr.T == 0)
                            t1 = to.il.DefineLabel();
                            uint ccnt = ccount[opcode];
                            //Label fend = to.il.DefineLabel();
                            GetTBit(to.il);                     //load tbit
                            to.il.Emit(OpCodes.Brtrue, t1);//if =1 then skip this
                            //{
                            //  delayslot = (uint)((sbyte)(opcode & 0xFF)) * 2 + 4 + pc;
                            uint oldpc = pc;
                            if (br_8_b_level < br_8_b_level_max)
                            {//ok we are not in the max inlining level , we can inline this one 
                                pc = (uint)((sbyte)(opcode & 0xFF)) * 2 + 4 + pc-2;//the -2 is for the recompiler ftech fix
                                br_8_b_level++;
                                while ((RecBlockEnd == false)&& (br_8_b_exit_all==false))
                                {
                                    ccnt += RecSingle(RecNextOpcode(), to);
                                }
                                br_8_b_level -= 1;//ok finished with this level
                                to.FinaliseIl();//an exeption to teh rule ;)
                                RecBlockEnd = false;//retore 
                                pc = oldpc;         //state and pc
                            }
                            else//..uff to much inlinng , time to stop it
                            {

                                to.il.Emit(OpCodes.Ldc_I4, (uint)((sbyte)(opcode & 0xFF)) * 2 + 4 + pc);
                                //  pc_funct = 1;//direct jump
                                to.il.Emit(OpCodes.Stloc_S, pci);//store delayslot to pc register
                                //}
                                RecBlockEnd = true;
                                //br_8_b_level -= 1;
                                br_8_b_exit_all = true;
                            }
                            to.il.MarkLabel(t1);//end if t=0
                            return ccnt;//return
                            break;
                        case 0xD://1101 inliner
                            //i1000_1101_iiii_iiii();
                            delayslot = (uint)((sbyte)(opcode & 0xFF)) * 2 + pc + 4; // antes era disp = ...
                            //to.il.Emit(OpCodes.Ldc_I4, (uint)((sbyte)(opcode & 0xFF)) * 2 + pc + 4);
                            //pc_funct = 2;//delay 1
                            //to.il.Emit(OpCodes.Stloc_S, pci);//store delayslot to pc register
                            uint temp = RecSingle(RecNextOpcode(), to);// do the delayslot instruction
                            temp += ccount[opcode];
                            //RecBlockEnd = true;
                            pc = delayslot-2;//since no conditional jump , we can inline it :)
                            return temp;//return
                            break;
                        case 0xF://1111
                            //i1000_1111_iiii_iiii();
                            //if (sr.T == 0)
                            t1 = to.il.DefineLabel();
                            GetTBit(to.il);                     //load tbit
                            to.il.Emit(OpCodes.Brtrue_S, t1);//if !=0 then skip this
                            //{
                            //  delayslot = (uint)((sbyte)(opcode & 0xFF)) * 2 + pc + 4;
                            to.il.Emit(OpCodes.Ldc_I4,(uint)((sbyte)(opcode & 0xFF)) * 2 + pc + 4);
                            //  pc_funct = 2;//delay 1 instruction
                            to.il.Emit(OpCodes.Stloc_S, pci);//store delayslot to pc register
                            //}
                            to.il.MarkLabel(t1);//end if t!=0
                            // do the delayslot instruction
                            uint temp3 = RecSingle(RecNextOpcode(), to);
                            temp3 += ccount[opcode];
                            RecBlockEnd = true;
                            return temp3;//return
                            break;
                        default:
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                case 0x9://finished rec
                    //i1001_nnnn_iiii_iiii();
                    n = (opcode >> 8) & 0x0F;
                    disp = (opcode & 0x00FF);
                    //r[n] = (uint)(short)read(disp * 2 + pc + 4, 2); do it in msil
                    to.il.Emit(OpCodes.Ldc_I4, 4 + disp * 2+pc);   //load the value addr 
                    to.il.Emit(OpCodes.Ldc_I4_2);               //2 bytes read (param #2)
                    to.il.Emit(OpCodes.Call, readmeth);//call read
                    to.il.Emit(OpCodes.Conv_I2);                //convert to int16s
                    to.il.Emit(OpCodes.Stloc_S, rbase + n);     //store the result to n
                    return ccount[opcode];
                    break;
                case 0xA://finished rec inliner
                    //i1010_iiii_iiii_iiii();
                    delayslot = (uint)(((short)((opcode & 0x0FFF) << 4)) >> 3) + pc + 4;//(short<<4,>>4(-1*2))
                    //pc_funct = 2;//jump delay 1
                    //to.il.Emit(OpCodes.Ldc_I4, delayslot);   //load the value that we must jump
                    //to.il.Emit(OpCodes.Stloc_S, pci);     //store the result to pc register
                    uint tmp=RecSingle(RecNextOpcode(), to);// do the delayslot instruction
                    //RecBlockEnd = true;
                    pc = delayslot-2; //since this is not a conditional jump , we can inline it ;)
                    return (uint)(tmp + ccount[opcode]);
                    break;
                case 0xB://finished rec
                    //i1011_iiii_iiii_iiii();
                    break;
                case 0xC://finished rec
                    #region case 0xC
                    switch ((opcode >> 8) & 0xf)
                    {
                        case 0x0://0000
                            //i1100_0000_iiii_iiii();
                            break;
                        case 0x1://0001
                            //i1100_0001_iiii_iiii();
                            break;
                        case 0x2://0010
                            //i1100_0010_iiii_iiii();
                            break;
                        case 0x3://0011
                            //i1100_0011_iiii_iiii();
                            break;
                        case 0x4://0100
                            //i1100_0100_iiii_iiii();
                            break;
                        case 0x5://0101
                            //i1100_0101_iiii_iiii();
                            break;
                        case 0x6://0110
                            //i1100_0110_iiii_iiii();
                            break;
                        case 0x7://0111
                            //i1100_0111_iiii_iiii();
                            disp = (opcode & 0x00FF) * 4 + ((pc + 4) & 0xFFFFFFFC);
                            //r[0] = disp;
                            to.il.Emit(OpCodes.Ldc_I4, disp);   //load the value that we calculated
                            to.il.Emit(OpCodes.Stloc_S,rbase );     //store the result to r0 register
                            return ccount[opcode];
                            break;
                        case 0x8://1000
                            //i1100_1000_iiii_iiii();
                            break;
                        case 0x9://1001
                            //i1100_1001_iiii_iiii();
                            break;
                        case 0xA://1010
                            //i1100_1010_iiii_iiii();
                            break;
                        case 0xB://1011
                            //i1100_1011_iiii_iiii();
                            break;
                        case 0xC://1100
                            //i1100_1100_iiii_iiii();
                            break;
                        case 0xD://1101
                            //i1100_1101_iiii_iiii();
                            break;
                        case 0xE://1110
                            //i1100_1110_iiii_iiii();
                            break;
                        case 0xF://1111
                            //i1100_1111_iiii_iiii();
                            break;
                        default:
                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                case 0xD://finished rec
                    //i1101_nnnn_iiii_iiii();
                    n = (opcode >> 8) & 0x0F;
                    disp = (opcode & 0xFF) << 2;
                    //r[n] = read(disp + (pc & 0xFFFFFFFC) + 4, 4);
                    to.il.Emit(OpCodes.Ldc_I4, disp + (pc & 0xFFFFFFFC) + 4);//param #1
                    to.il.Emit(OpCodes.Ldc_I4_4);//param #2
                    to.il.Emit(OpCodes.Call, readmeth);// call readmem
                    to.il.Emit(OpCodes.Stloc_S, rbase+n);     //store the result to rn register
                    return ccount[opcode];
                    break;
                case 0xE://finished rec
                    //i1110_nnnn_iiii_iiii();
                    n = (opcode >> 8) & 0x0F;
                    //r[n] = (uint)(sbyte)(opcode & 0xFF);//(uint)(sbyte)= signextend8 :)
                    to.il.Emit(OpCodes.Ldc_I4_S, opcode & 0xFF);//store it
                    to.il.Emit(OpCodes.Conv_I1);//convert it to s8
                    to.il.Emit(OpCodes.Stloc_S, rbase + n);     //store the result to rn register
                    return ccount[opcode];
                    break;
                case 0xF://finished - fix for fsca
                    #region case 0xf
                    switch (opcode & 0xf)
                    {
                        case 0x0://0000
                            //i1111_nnnn_mmmm_0000();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //fr[n] += fr[m];
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + n);
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + m);
                            to.il.Emit(OpCodes.Add);
                            to.il.Emit(OpCodes.Stloc_S, fr_base + n);
                            return ccount[opcode];
                            break;
                        case 0x1://0001
                            //i1111_nnnn_mmmm_0001();
                            break;
                        case 0x2://0010
                            //i1111_nnnn_mmmm_0010();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //fr[n] *= fr[m];
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + n);
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + m);
                            to.il.Emit(OpCodes.Mul);
                            to.il.Emit(OpCodes.Stloc_S, fr_base + n);
                            return ccount[opcode];
                            break;
                        case 0x3://0011
                            //i1111_nnnn_mmmm_0011();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //fr[n] /= fr[m];
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + n);
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + m);
                            to.il.Emit(OpCodes.Div);
                            to.il.Emit(OpCodes.Stloc_S, fr_base + n);
                            return ccount[opcode];
                            break;
                        case 0x4://0100
                            //i1111_nnnn_mmmm_0100();
                            break;
                        case 0x5://0101
                            //i1111_nnnn_mmmm_0101();
                            break;
                        case 0x6://0110
                            //i1111_nnnn_mmmm_0110();
                            break;
                        case 0x7://0111
                            //i1111_nnnn_mmmm_0111();
                            break;
                        case 0x8://1000
                            //i1111_nnnn_mmmm_1000();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            //uint ttmp = read(r[m], 4);
                            to.il.Emit(OpCodes.Ldloc_S, rbase + m);//load reg
                            to.il.Emit(OpCodes.Ldc_I4_4 );         //load 4
                            to.il.Emit(OpCodes.Call,readmeth);     //call read
                            //fr[n] = *(float*)&ttmp;
                            to.il.Emit(OpCodes.Stloc_S, tuint);        //save the rez into a temp local
                            u2f(to.il, tuint);                         //copy to to float 
                            to.il.Emit(OpCodes.Stloc_S, fr_base + n);
                            return ccount[opcode];
                            break;
                        case 0x9://1001
                            //i1111_nnnn_mmmm_1001();
                            break;
                        case 0xA://1010
                            //i1111_nnnn_mmmm_1010();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            
                            //param #1
                            to.il.Emit(OpCodes.Ldloc_S, rbase + n);//address

                            //fixed (float* p = &fr[m]) { write(r[n], *(uint*)p, 4); }
                            f2u(to.il,(int) (fr_base+m) );            //copy to to uint 
                            to.il.Emit(OpCodes.Ldc_I4_4);         //load 4
                            to.il.Emit(OpCodes.Call, writemeth);     //call write

                            return ccount[opcode];
                            break;
                        case 0xB://1011
                            //i1111_nnnn_mmmm_1011();
                            break;
                        case 0xC://1100
                            //i1111_nnnn_mmmm_1100();
                            n = (opcode >> 8) & 0x0F;
                            m = (opcode >> 4) & 0x0F;
                            fr[n] = fr[m];
                            to.il.Emit(OpCodes.Ldloc_S, fr_base + m);
                            to.il.Emit(OpCodes.Stloc_S, fr_base + n);
                            return ccount[opcode];
                            break;
                        case 0xD://1101
                            #region 0xD multi
                            switch ((opcode >> 4) & 0xf)
                            {
                                case 0x0://0000
                                    //i1111_nnnn_0000_1101(); 
                                    break;
                                case 0x1://0001
                                    //i1111_nnnn_0001_1101(); 
                                    break;
                                case 0x2://0010
                                    //i1111_nnnn_0010_1101(); 
                                    n = (opcode >> 8) & 0x0F;
                                    //fr[n] = (float)(int)fpul;
                                    to.il.Emit(OpCodes.Ldloc_S, fpuli);
                                    to.il.Emit(OpCodes.Conv_R4);
                                    to.il.Emit(OpCodes.Stloc_S, fr_base + n);
                                    return ccount[opcode];
                                    break;
                                case 0x3://0011
                                    //i1111_nnnn_0011_1101(); 
                                    m = (opcode >> 8) & 0x0F;
                                    //fpul = (uint)(int)fr[m];
                                    to.il.Emit(OpCodes.Ldloc_S, fr_base + m);
                                    to.il.Emit(OpCodes.Conv_I4);
                                    to.il.Emit(OpCodes.Stloc_S, fpuli);
                                    return ccount[opcode];
                                    break;
                                case 0x4://0100
                                    //i1111_nnnn_0100_1101(); 
                                    break;
                                case 0x5://0101
                                    //i1111_nnnn_0101_1101(); 
                                    break;
                                case 0x6://0110
                                    //i1111_nnnn_0110_1101(); 
                                    break;
                                case 0x8://1000
                                    //i1111_nnnn_1000_1101(); 
                                    break;
                                case 0x9://1001
                                    //i1111_nnnn_1001_1101(); 
                                    break;
                                case 0xA://1010
                                    //i1111_nnnn_1010_1101(); 
                                    break;
                                case 0xB://1011
                                    //i1111_nnnn_1011_1101(); 
                                    break;
                                case 0xF://1111_xxxx_1111_1101
                                    #region 0xf multi
                                    //we have :
                                    //1111_nnn0_1111_1101
                                    //1111_nn01_1111_1101
                                    //1111_1011_1111_1101
                                    //1111_0011_1111_1101
                                    switch ((opcode >> 8) & 0x1)
                                    {
                                        case 0x0://1111_nnn0_1111_1101 - fsca DC special
                                            //i1111_nnn0_1111_1101();
                                            n = (opcode >> 9) & 0x07;
                                            //float x = (float)(2 * pi * (float)fpul / 65536.0);
                                            //fr[n * 2] = (float)System.Math.Sin(x);
                                            //fr[n * 2 + 1] = (float)System.Math.Cos(x);

                                            to.il.Emit(OpCodes.Ldc_R4, (float)(2 * pi));
                                            to.il.Emit(OpCodes.Ldloc_S, fpuli);
                                            to.il.Emit(OpCodes.Conv_R4);
                                            to.il.Emit(OpCodes.Mul);
                                            to.il.Emit(OpCodes.Ldc_R4, (float)65536.0);
                                            to.il.Emit(OpCodes.Div);
                                            to.il.Emit(OpCodes.Dup);

                                            to.il.Emit(OpCodes.Call ,Sinmeth);
                                            to.il.Emit(OpCodes.Stloc_S, fr_base + n*2);

                                            to.il.Emit(OpCodes.Call, Cosmeth);
                                            to.il.Emit(OpCodes.Stloc_S, fr_base + n * 2 +1);

                                            return ccount[opcode];
                                            break;
                                        case 0x1://1111_xxy1_1111_1101
                                            //if (opcode==0xfffd) {dc.dcon.WriteLine("Warning:Invalid opcode at pc "+System.Convert.ToString(pc,16).ToUpper()+ " with code " +System.Convert.ToString(opcode,16).ToUpper());System.Windows.Forms.Application.DoEvents();break;}//1111_x111_1111_1101- invalid
                                            //1111_nn01_1111_1101
                                            //1111_1011_1111_1101
                                            //1111_0011_1111_1101
                                            if (((opcode >> 9) & 0x1) == 0)//1111_xxy1_1111_1101
                                            {
                                                break;//i1111_nn01_1111_1101();
                                            }
                                            else//1111_yy11_1111_1101
                                            {
                                                if (((opcode >> 10) & 0x3) == 0)//1111_yy11_1111_1101
                                                {
                                                    break;//i1111_0011_1111_1101();
                                                }
                                                else if (((opcode >> 10) & 0x3) == 2)//1111_yy11_1111_1101
                                                {
                                                    break;//i1111_1011_1111_1101();
                                                }
                                            }
                                            //1111_x111_1111_1101- invalid
                                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                            break;
                                        default:
                                            dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                            break;
                                    }
                                    #endregion
                                    break;
                                default:
                                    dc.dcon.WriteLine("Warning:Invalid opcode at pc " + System.Convert.ToString(pc, 16).ToUpper() + " with code " + System.Convert.ToString(opcode, 16).ToUpper()); System.Windows.Forms.Application.DoEvents();
                                    break;
                            }
                            #endregion
                            break;
                        case 0xE://1110
                            //i1111_nnnn_mmmm_1110();
                            break;
                        default:
                            //dc.dcon.WriteLine("Warning:Invalid opcode at pc "+System.Convert.ToString(pc,16).ToUpper()+ " with code " +System.Convert.ToString(opcode,16).ToUpper());
                            //System.Windows.Forms.Application.DoEvents();
                            break;
                    }
                    #endregion
                    break;
                default:
                    //handle any custom opcodes (>65535)
                    //bios hle ect
                    break;
            }
            return 0;
            #endregion
        }
        public static void SetTBit(ILGenerator il, uint nval)
        {
            if (nval != 0)
            {
                il.Emit(OpCodes.Ldc_I4, sr_T_bit_set);
                il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                il.Emit(OpCodes.Or);          //bitwise or to set T in sr
                il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
            }
            else
            {
                il.Emit(OpCodes.Ldc_I4, sr_T_bit_reset);
                il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
                il.Emit(OpCodes.And);          //bitwise And to unset T in sr
                il.Emit(OpCodes.Stloc_S, sri);//store the rez back to sr
            }
        }

        public static void GetTBit(ILGenerator il)
        {
            il.Emit(OpCodes.Ldc_I4, sr_T_bit_set);
            il.Emit(OpCodes.Ldloc_S, sri);//sr register has t in it
            il.Emit(OpCodes.And);          //bitwise and to get T from sr
                                           //we return the and result  
        }

        //MEmory copys from uint to float and reverse
        public static void f2u(ILGenerator il,int n)
        {
            il.Emit(OpCodes.Ldloca_S, n);//load pointer
            il.Emit(OpCodes.Ldobj, typeof(uint));//pop pointer , push *(uint*)pointer
        }

        public static void u2f(ILGenerator il,int n)
        {
            il.Emit(OpCodes.Ldloca_S, n);//load pointer
            il.Emit(OpCodes.Ldobj, typeof(float));//pop pointer , push *(float*)pointer
        }

        public static void SetTBT(ILGenerator il, uint num, uint tt, uint tf)
        {
            dc.dcon.WriteLine("UPS SetTBT is not implemented..");
        }

        public static void SetTLT(ILGenerator il, uint num, uint tt, uint tf)
        {
            dc.dcon.WriteLine("UPS SetTLT is not implemented..");
        }

        //if t =  val then set t = tt else set t=tf
        public static void SetTeq(ILGenerator il , uint tt, uint tf)
        {
            Label TT = il.DefineLabel();
            Label fend = il.DefineLabel();
            il.Emit(OpCodes.Beq_S, TT);// if p1= p1 set t=tt
            SetTBit(il,tf);// else set it to TF
            il.Emit(OpCodes.Br_S, fend);//finished , goto to end
            il.MarkLabel(TT);//oh we must set it to TT
            SetTBit(il,tt);//set t to TT
            il.MarkLabel(fend);//function end
        }

        //if t =  val then set t = tt else set t=tf
        /// <summary>
        /// push v0
        /// push v1
        /// push eq
        /// push neq
        /// </summary>
        /// <param name="il"></param>
        public static void SetPCeq(ILGenerator il)
        {
            Label TT = il.DefineLabel();
            Label fend = il.DefineLabel();
            il.Emit(OpCodes.Beq_S, TT);// if p1= p1 set t=tt
            il.Emit(OpCodes.Pop);
            il.Emit(OpCodes.Stloc_S,pci);//else set it to TF
            il.Emit(OpCodes.Br_S, fend);//finished , goto to end
            il.MarkLabel(TT);//oh we must set it to TT            
            il.Emit(OpCodes.Stloc_S, pci);//else set t to TT
            il.Emit(OpCodes.Pop);
            il.MarkLabel(fend);//function end
        }

        public static void DoutB(uint Value)
        {
            dc.dcon.WriteLine("Before " + Value.ToString());
        }

        public static void DoutA(uint Value)
        {
            dc.dcon.WriteLine("After " + Value.ToString());
        }

        public static uint RecNextOpcode()
        {
            pc += 2;//goto next opcode
            return read(pc, 2);//return it
        }
        public static uint RecExecuteBlock()
        {
            uint bid=FindBlock(pc);
            if (bid > 0)
            {//found , call it then
                uint srr = sr.reg;
                uint fpscrf = fpscr.reg;
                ch++;
                pc = DynaCache[bid].BufferEnd+2;
                //if (r[2] == 1)
                //{
                //    r[0] = 2768240648;
                //    r[1] = (uint)rnd.Next((int)0x5FFFFFFF);
                //    r[2] = 153598;
                //}
                DynaCache[bid].CodeBuffer(ref r[0], ref r[1], ref r[2], ref r[3], ref r[4], ref r[5], ref r[6],
                                                      ref r[7], ref r[8], ref r[9], ref r[10], ref r[11], ref r[12],
                                                      ref r[13], ref r[14], ref r[15], ref r_bank[0], ref r_bank[1], ref r_bank[2],
                                                      ref r_bank[3], ref r_bank[4], ref r_bank[5], ref r_bank[6], ref r_bank[7],
                                                      ref fr[0], ref fr[1], ref fr[2], ref fr[3], ref fr[4], ref fr[5], ref fr[6],
                                                      ref fr[7], ref fr[8], ref fr[9], ref fr[10], ref fr[11], ref fr[12],
                                                      ref fr[13], ref fr[14], ref fr[15], ref xr[0], ref xr[1], ref xr[2],
                                                      ref xr[3], ref xr[4], ref xr[5], ref xr[6], ref xr[7], ref xr[8], ref xr[9], ref xr[10],
                                                      ref xr[11], ref xr[12], ref xr[13], ref xr[14], ref xr[15], ref gbr,
                                                      ref ssr, ref spc, ref sgr, ref dbr, ref vbr, ref mach, ref macl,
                                                      ref pr, ref fpul, ref pc, ref srr, ref fpscrf);
                sr.reg=srr;
                fpscr.reg = fpscrf;
                //block executed , pc and registrs are updated
                //return the opcodes executed
                return DynaCache[bid].DurCucles;
            }
            //dc.dcon.WriteLine("Cache miss");
            //doh .. not found , we must make this block....
            cm++;
            uint ccnt = 0;
            uint baseA=pc;
            bid = FindEmpty();//find next empty block
            //generate msil code
            RecBlockEnd = false;
            pc -= 2;
            IlCoder cc = new IlCoder(CBind++);
            while ((RecBlockEnd == false))
            {
                ccnt += RecSingle(RecNextOpcode(), cc);
                //br_8_b_exit_all = false;
                //br_8_b_level = 0;
            }
            br_8_b_exit_all = false;
            br_8_b_level = 0;
            cc.FinaliseIl();//finalise the msil stream
            DynaCache[bid]= new CodeCacheEntry(cc.GetCodeBuffer(),baseA,pc,ccnt);

            uint srr2 = sr.reg;
            uint fpscrf2 = fpscr.reg;
            DynaCache[bid].CodeBuffer(ref r[0], ref r[1], ref r[2], ref r[3], ref r[4], ref r[5], ref r[6],
                                      ref r[7], ref r[8], ref r[9], ref r[10], ref r[11], ref r[12],
                                      ref r[13], ref r[14], ref r[15], ref r_bank[0], ref r_bank[1], ref r_bank[2],
                                      ref r_bank[3], ref r_bank[4], ref r_bank[5], ref r_bank[6], ref r_bank[7],
                                      ref fr[0], ref fr[1], ref fr[2], ref fr[3], ref fr[4], ref fr[5], ref fr[6],
                                      ref fr[7], ref fr[8], ref fr[9], ref fr[10], ref fr[11], ref fr[12],
                                      ref fr[13], ref fr[14], ref fr[15], ref xr[0], ref xr[1], ref xr[2],
                                      ref xr[3], ref xr[4], ref xr[5], ref xr[6], ref xr[7], ref xr[8], ref xr[9], ref xr[10],
                                      ref xr[11], ref xr[12], ref xr[13], ref xr[14], ref xr[15], ref gbr,
                                      ref ssr, ref spc, ref sgr, ref dbr, ref vbr, ref mach, ref macl,
                                      ref pr, ref fpul, ref pc, ref srr2, ref fpscrf2);
            sr.reg = srr2;
            fpscr.reg = fpscrf2;

            //block executed , pc and registrs are updated
            //return the opcodes executed
            return DynaCache[bid].DurCucles;
        }

        static uint FindBlock(uint baseAddress)
        {
            for (uint i = 0; i < RecMB; i++)
            {
                if (DynaCache[i].BaseAddress == baseAddress)
                    return i;
            }
            return 0;
        }

        static uint FindEmpty()
        {
            for (uint i = 0; i < RecMB; i++)
            {
                if (DynaCache[i].BaseAddress == 0)
                    return i;
            }
            return 0;
        }

    }

    public class IlCoder
    {
        public ILGenerator il;
        public DynamicMethod si;
        /// <summary>
        /// Creates the IlCoder Class and inits it.
        /// </summary>
        public IlCoder(int id)
        {
            Type[] siArgs=new Type[emu.pCount];
            for (int i = 0; i < emu.fr_base; i++)
            {
                siArgs[i] = typeof(uint).MakeByRefType();
            }
            for (int i = emu.fr_base; i < emu.gbri; i++)
            {
                siArgs[i] = typeof(float).MakeByRefType();
            }
            for (int i = emu.gbri; i < emu.pCount; i++)
            {
                siArgs[i] = typeof(uint).MakeByRefType();
            }
            //= { typeof(int).MakeByRefType(), typeof(int[]) }; 
            #if false 
		            ref uint reg0 ,  ref uint reg1 ,  ref uint reg2 ,  
                                                ref uint reg3 ,  ref uint reg4 ,  ref uint reg5 ,  
                                                ref uint reg6 ,  ref uint reg7 ,  ref uint reg8 ,
                                                ref uint reg9, ref uint reg10, ref uint reg11,
                                                ref uint reg12, ref uint reg13, ref uint reg14,
                                                ref uint reg15, ref uint reg16, ref uint reg17,
                                                ref uint reg18, ref uint reg19, ref uint reg20,
                                                ref uint reg21, ref uint reg22, ref uint reg23,
                                                ref float reg24, ref float reg25, ref float reg26,
                                                ref float reg27, ref float reg28, ref float reg29,
                                                ref float reg30, ref float reg31, ref float reg32,
                                                ref float reg33, ref float reg34, ref float reg35,
                                                ref float reg36, ref float reg37, ref float reg38,
                                                ref float reg39, ref float reg40, ref float reg41,
                                                ref float reg42, ref float reg43, ref float reg44,
                                                ref float reg45, ref float reg46, ref float reg47,
                                                ref float reg48, ref float reg49, ref float reg50,
                                                ref float reg51, ref float reg52, ref float reg53,
                                                ref float reg54, ref float reg55, ref uint reg56,
                                                ref uint reg57, ref uint reg58, ref uint reg59,
                                                ref uint reg60, ref uint reg61, ref uint reg62,
                                                ref uint reg63, ref uint reg64, ref uint reg65,
                                                ref uint reg66, ref uint reg67, ref uint reg68); /**/ 
	        #endif
            si = new DynamicMethod("DRM_" + id.ToString()  ,
                typeof(void) ,
                siArgs,
                typeof(emu).Module);
            il = si.GetILGenerator();
            //define all the params as locals...
            for (int i = 0; i < emu.fr_base; i++)
            {
                il.DeclareLocal(typeof(uint));
            }
            for (int i = emu.fr_base; i < emu.gbri; i++)
            {
                il.DeclareLocal(typeof(float));
            }
            for (int i = emu.gbri; i < emu.pCount; i++)
            {
                il.DeclareLocal(typeof(uint));
            }
            il.DeclareLocal(typeof(uint));//temp uint
            il.DeclareLocal(typeof(float));//temp float

            //load the params onto the locals
            for (int i = 0; i < emu.r_bbase; i++)//emu.fr_base
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldobj, typeof(uint));//pop pointer,push *pointer
                il.Emit(OpCodes.Stloc_S, i);//store it on the local
            }
            for (int i = emu.fr_base; i < emu.xr_base; i++)//emu.gbri
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldobj, typeof(float));//pop pointer,push *pointer
                il.Emit(OpCodes.Stloc_S, i);//store it on the local
            }
            for (int i = emu.gbri; i < emu.pCount; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldobj, typeof(uint));//pop pointer,push *pointer
                il.Emit(OpCodes.Stloc_S, i);//store it on the local
            }
        }
        /// <summary>
        /// Finalises the funtion code.
        /// After called no more emiting is permited.
        /// </summary>
        public void FinaliseIl()
        {
            //store all the local registers on the arguments
            for (int i = 0; i < emu.r_bbase; i++)//emu.fr_base
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldloc_S, i);//load tha value to write
                il.Emit(OpCodes.Stobj, typeof(uint));//store it in argument
            }
            for (int i = emu.fr_base; i < emu.xr_base; i++)//emu.gbri;
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldloc_S, i);//load tha value to write
                il.Emit(OpCodes.Stobj, typeof(float));//store it in argument
            }
            for (int i = emu.gbri; i < emu.pCount; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldloc_S, i);//load tha value to write
                il.Emit(OpCodes.Stobj, typeof(uint));//store it in argument
            }

            il.Emit(OpCodes.Ret); //yeah the end here -> return to function caller..
        }
        public void FinaliseIlNoRet()
        {
            //store all the local registers on the arguments
            for (int i = 0; i < emu.r_bbase; i++)//emu.fr_base
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldloc_S, i);//load tha value to write
                il.Emit(OpCodes.Stobj, typeof(uint));//store it in argument
            }
            for (int i = emu.fr_base; i < emu.xr_base; i++)//emu.gbri;
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldloc_S, i);//load tha value to write
                il.Emit(OpCodes.Stobj, typeof(float));//store it in argument
            }
            for (int i = emu.gbri; i < emu.pCount; i++)
            {
                il.Emit(OpCodes.Ldarg_S, i);//load the pointer on the stack
                il.Emit(OpCodes.Ldloc_S, i);//load tha value to write
                il.Emit(OpCodes.Stobj, typeof(uint));//store it in argument
            }
        }
        /// <summary>
        /// Gets the function buffer as a delegate.
        /// The buffer must be finalised
        /// </summary>
        public DynaRecCall GetCodeBuffer()
        {
            return (DynaRecCall)si.CreateDelegate(typeof(DynaRecCall));
        }
    }

    /// <summary>
    /// Stores info for a CodeCache table entry.
    /// </summary>
    public struct CodeCacheEntry
    {
        /// <summary>
        /// The Generated code.
        /// </summary>
        public DynaRecCall CodeBuffer;
        /// <summary>
        /// Number of the calls doe to this Entry.
        /// </summary>
        public uint nocs;       //number of calls
        /// <summary>
        /// Base Address of the code buffer(recoplie_start address)
        /// </summary>
        public uint BaseAddress;//base address of the code buffer(recoplie_start address)
        /// <summary>
        /// End Address of the code buffer
        /// </summary>
        public uint BufferEnd;  //end of the code buffer
        /// <summary>
        /// Lenght of the buffer(in DreamCast opcodes)
        /// </summary>
        public uint BufferLen;  //number of instructions..
        /// <summary>
        /// If this buffer is valid then this is not set.
        /// If it is set then this buffer should be considered as empty
        /// </summary>
        public bool dirty;      //Does not reflect the current memory data..
        /// <summary>
        /// How many cycles takes the DreamCast's SH4 cpu to execute this buffer
        /// </summary>
        public uint DurCucles;  //how many emulated cpu cycles the codebuffer executes..
        /// <summary>
        /// Creates and inits the CodeCacheEntry object
        /// </summary>
        /// <param name="Code Buffer"></param>
        /// <param name="Base Address"></param>
        /// <param name="End Address"></param>
        /// <param name="Buffer Length"></param>
        /// <param name="Sh4 cycles"></param>
        public CodeCacheEntry(DynaRecCall buf, uint ba, uint be,uint nCycles)
        {
            CodeBuffer = buf;
            nocs = 0;
            BaseAddress = ba;
            BufferEnd = be;
            BufferLen = be - ba+1;
            dirty = false;
            DurCucles = nCycles;
        }
    }
}

