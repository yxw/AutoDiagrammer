using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace AutoDiagrammer
{
    /// <summary>
    /// A simple helper class, that one has one method, that
    /// is used to determine if an input file is an actual
    /// CLR type file.
    /// </summary>
    public class DotNetObject
    {
        #region Public Methods
        /// <summary>
        /// Return true if the file specified is a real CLR type, otherwise false is returned.
        /// False is also returned in the case of an exception being caught
        /// </summary>
        /// <param name="file">A string representing the file to check for CLR validity</param>
        /// <returns>True if the file specified is a real CLR type, otherwise false is returned.
        /// False is also returned in the case of an exception being caught</returns>
        public static bool IsValidDotNetAssembly(String file)
        {

            uint peHeader;
            uint peHeaderSignature;
            ushort machine;
            ushort sections;
            uint timestamp;
            uint pSymbolTable;
            uint noOfSymbol;
            ushort optionalHeaderSize;
            ushort characteristics;
            ushort dataDictionaryStart;
            uint[] dataDictionaryRVA = new uint[16];
            uint[] dataDictionarySize = new uint[16];

            //get the input stream
            Stream fs = new FileStream(@file, FileMode.Open, FileAccess.Read);

            try
            {

                BinaryReader reader = new BinaryReader(fs);
                //PE Header starts @ 0x3C (60). Its a 4 byte header.
                fs.Position = 0x3C;
                peHeader = reader.ReadUInt32();
                //Moving to PE Header start location...
                fs.Position = peHeader;
                peHeaderSignature = reader.ReadUInt32();
                //We can also show all these value, but we will be       
                //limiting to the CLI header test.
                machine = reader.ReadUInt16();
                sections = reader.ReadUInt16();
                timestamp = reader.ReadUInt32();
                pSymbolTable = reader.ReadUInt32();
                noOfSymbol = reader.ReadUInt32();
                optionalHeaderSize = reader.ReadUInt16();
                characteristics = reader.ReadUInt16();
                /*
                    Now we are at the end of the PE Header and from here, the
                    PE Optional Headers starts...
                    To go directly to the datadictionary, we'll increase the      
                    stream’s current position to with 96 (0x60). 96 because,
                    28 for Standard fields
                    68 for NT-specific fields
                    From here DataDictionary starts...and its of total 128 bytes. DataDictionay has 16 directories in total,
                    doing simple maths 128/16 = 8.
                    So each directory is of 8 bytes.
             
                    In this 8 bytes, 4 bytes is of RVA and 4 bytes of Size.
                    btw, the 15th directory consist of CLR header! if its 0, its not a CLR file :)

                    */
                dataDictionaryStart = Convert.ToUInt16(Convert.ToUInt16(fs.Position) + 0x60);
                fs.Position = dataDictionaryStart;
                for (int i = 0; i < 15; i++)
                {
                    dataDictionaryRVA[i] = reader.ReadUInt32();
                    dataDictionarySize[i] = reader.ReadUInt32();
                }
                if (dataDictionaryRVA[14] == 0)
                {
                    fs.Close();
                    return false;
                }
                else
                {
                    fs.Close();
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                fs.Close();
            }

        }


        /// <summary>
        /// Return true if t is wanted for diagram, at present the only thing
        /// not allowed are System namespaced Dlls
        /// </summary>
        public static bool IsWantedForDiagramType(Type t)
        {
            //check to see if the class lives in a namespace
            if (!string.IsNullOrEmpty(t.Namespace))
            {
                //dont really want user to trawl the standard System namespaces
                if (t.Namespace.StartsWith("System"))
                    return false;
            }

            return true;
        }
        #endregion
    }
}
