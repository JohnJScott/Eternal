/*******************************************************************************

Copyright (c) 2010, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: p4-map-api.cc
 *
 * Author	: dbb
 *
 * Description	: A "Flat C" interface for the MapApi object in the Perforce 
 *        API. Used to provide simple access for C#.NET using P/Invoke and 
 *		  dllimport.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace Perforce.P4
{
    /// <summary>
    /// P4MapApi: .NET wrapper for the MapApi object in the p4api dll.
    /// </summary>
    public class P4MapApi : IDisposable
    {
        /// <summary>
        /// Type of the map entry, Include, Exclude, Overlay.
        /// </summary>
		[Flags]
        public enum Type : int 
        { 
            /// <summary>
            ///  Include the indicated mapping
            /// </summary>
            Include = 0x0000,
            /// <summary>
            /// Exclude the indicated mapping
            /// </summary>
            Exclude = 0x0001, 
            /// <summary>
            /// Overlay the indicated mapping
            /// </summary>
            Overlay = 0x0002
        }
        /// <summary>
        /// Specify the direction to perform the mapping.
        /// </summary>
		[Flags]
        public enum Direction : int  
        { 
            /// <summary>
            /// Map from left to right
            /// </summary>
            LeftRight = 0x000, 
            /// <summary>
            /// Map from right to left
            /// </summary>
            RightLeft  = 0x001
        }

        private IntPtr pMapApi = IntPtr.Zero;
        private P4Server pServer = null; // needed to perform string translations 

        /// <summary>
        /// Create a new P4MapApi
        /// </summary>
        /// <param name="pserver">The P4Server on which the map will be used</param>
        /// <remarks>
        /// The server is needed to know whether or not it is necessary to translate
        /// strings to/from Unicode </remarks>
        public P4MapApi(P4Server pserver)
        {
            pServer = pserver;
            pMapApi = CreateMapApi();
        }


        /// <summary>
        /// Wrap the pointer to MapApi object in a P4MapApi
        /// </summary>
        /// <param name="pserver">The P4Server on which the map will be used</param>
        /// <param name="pNew">The MapApi pointer to wrap</param>
        /// <remarks>
        /// The server is needed to know whether or not it is necessary to translate
        /// strings to/from Unicode </remarks>

        internal P4MapApi(P4Server pserver, IntPtr pNew)
        {
            pServer = pserver;
            pMapApi = pNew;
        }

        /// <summary>
        /// Delete the P4MapApi object and free the native object from the dll.
        /// </summary>
        ~P4MapApi()
        {
            Dispose();
        }

        /******************************************************************************
         * 'Flat' C interface for the dll. This interface will be imported into C# 
         *    using P/Invoke 
        ******************************************************************************/

        /// <summary>
        /// Helper function to create a new MapApi object.
        /// </summary>
        /// <returns>IntPtr to the new object</returns>
        /// <remarks>Call DeletMapApi() on the returned pointer to free the object</remarks>
        [DllImport("p4bridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr CreateMapApi();

        /// <summary>
        ///  Helper function to delete a MapApi object allocated by CreateMApApi().
        /// </summary>
        /// <param name="pMap">IntPtr for the object to be deleted</param>
        [DllImport("p4bridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeleteMapApi(IntPtr pMap);

        /**************************************************************************
        *
        * P4BridgeServer functions
        *
        *    These are the functions that use a MapApi* to access an object 
        *      created in the dll.
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Clear(IntPtr pMap);

        /// <summary>
        /// Clear all the data in the map
        /// </summary>
        public void Clear() { Clear(pMapApi); }

        [DllImport("p4bridge.dll", EntryPoint = "Count",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetCount(IntPtr pMap);

        /// <summary>
        /// The number of entries in the map
        /// </summary>
        public int Count { get { return GetCount(pMapApi); } }

        [DllImport("p4bridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetLeft(IntPtr pMap, int i);

        /// <summary>
        ///  Return the left side of the specified entry in the map
        /// </summary>
        /// <param name="idx">Index of the desired entry</param>
        /// <returns>String representing the left side of the entry</returns>
        public String GetLeft(int idx)
        {
            IntPtr pStr = GetLeft(pMapApi, idx);

            return pServer.MarshalPtrToString(pStr);
        }

        /**************************************************************************
        *
        *  GetRight: Return the right side of the specified entry in the map
        *
        *   pMap:	 Pointer to the P4MapApi instance 
        *   i:		 Index of the desired entry
        *
        *   Returns: char *, a string representing the right side of the entry.
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetRight(IntPtr pMap, int i);

        /// <summary>
        /// Return the right side of the specified entry in the map
        /// </summary>
        /// <param name="idx">Index of the desired entry</param>
        /// <returns>String representing the right side of the entry</returns>
        public String GetRight(int idx)
        {
            IntPtr pStr = GetRight(pMapApi, idx);

            return pServer.MarshalPtrToString(pStr);
        }

        [DllImport("p4bridge.dll", CallingConvention = CallingConvention.Cdecl)]
        private static extern int GetType(IntPtr pMap, int i);

        /// <summary>
        /// Return the type of the specified entry in the map
        /// </summary>
        /// <param name="idx">Index of the desired entry</param>
        /// <returns>The P4MapApi.Type enumeration for the type of the entry</returns>
        public P4MapApi.Type GetType(int idx)
        {
            return (P4MapApi.Type) GetType(pMapApi, idx);
        }

        /**************************************************************************
        *
        *  Insert: Adds a new entry in the map
        *
        *   pMap:	 Pointer to the P4MapApi instance 
        *   lr:		 String representing both the the left and right sides of the 
        *				new entry
        *   t:		 Type of the new entry
        *
        *   Returns: void
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", EntryPoint = "Insert1",
            CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void InsertA(IntPtr pMap, String lr, P4MapApi.Type t);

        [DllImport("p4bridge.dll", EntryPoint = "Insert1",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void InsertW(IntPtr pMap, IntPtr lr, P4MapApi.Type t);

        /// <summary>
        /// Adds a new entry in the map
        /// </summary>
        /// <param name="lr">String representing both the the left and right sides of 
        /// the new entry</param>
        /// <param name="t">Type of the new entry</param>
        public void Insert( String lr, P4MapApi.Type t )
        {
            if (pServer.UseUnicode)
            {
                using (PinnedByteArray plr = pServer.MarshalStringToIntPtr(lr))
                {
                    InsertW(pMapApi, plr, t);
                }
            }
            else
                InsertA(pMapApi, lr, t);
        }

        /**************************************************************************
        *
        *  Insert: Adds a new entry in the map
        *
        *   pMap:	 Pointer to the P4MapApi instance 
        *   l:		 String representing the the left side of the new entry
        *   r:		 String representing the the right side of the new entry
        *   t:		 Type of the new entry
        *
        *   Returns: void
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", EntryPoint = "Insert2",
            CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern void InsertA(IntPtr pMap, String l,
                                           String r, P4MapApi.Type t);

        [DllImport("p4bridge.dll", EntryPoint = "Insert2",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern void InsertW(IntPtr pMap, IntPtr l,
                                           IntPtr r, P4MapApi.Type t);

        /// <summary>
        /// Adds a new entry in the map
        /// </summary>
        /// <param name="left">String representing the the left side of the new entry</param>
        /// <param name="right">String representing the the right side of the new entry</param>
        /// <param name="type">Type of the new entry</param>
        public void Insert(String left, String right, P4MapApi.Type type)
        {
            if (pServer.UseUnicode)
            {
                using (PinnedByteArray pLeft = pServer.MarshalStringToIntPtr(left),
                                       pRight = pServer.MarshalStringToIntPtr(right))
                {
                    InsertW(pMapApi, pLeft, pRight, type);
                }
            }
            else
                InsertA(pMapApi, left, right, type);
        }
        /**************************************************************************
        *
        *  Join: Combine two MapApis to create a new MapApi
        *
        *   pLeft:	 Pointer to the first map
        *   pRight:	 Pointer to the second map
        *
        *   Returns: MapApi * to the new map
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", EntryPoint = "Join1",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Join(IntPtr pLeft, IntPtr pRight);

        /// <summary>
        ///  Combine two MapApis to create a new MapApi
        /// </summary>
        /// <param name="left">Pointer to the first map</param>
        /// <param name="right">Pointer to the second map</param>
        /// <returns></returns>
        public static P4MapApi Join(P4MapApi left, P4MapApi right)
        {
            IntPtr pNewMap = Join(left.pMapApi, right.pMapApi);
            if (pNewMap == IntPtr.Zero)
                return null;
            return new P4MapApi(left.pServer, pNewMap);
        }

        /**************************************************************************
        *
        *  Join: Combine two MapApis to create a new MapApi
        *
        *   pLeft:	  Pointer to the first map
        *   leftDir:  orientation of the first map
        *   pRight:	  Pointer to the second map
        *   rightDir: orientation of the second map
        *
        *   Returns: MapApi * to the new map
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", EntryPoint = "Join2",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr Join(IntPtr pLeft, Direction ld,
                                          IntPtr pRight, Direction rd);

        /// <summary>
        /// Combine two MapApis to create a new MapApi
        /// </summary>
        /// <param name="left">Pointer to the first map</param>
        /// <param name="leftDir">Orientation of the first map</param>
        /// <param name="right">Pointer to the second map</param>
        /// <param name="rightDir">Orientation of the second map</param>
        /// <returns></returns>
        public static P4MapApi Join( P4MapApi left, Direction leftDir,
                                        P4MapApi right, Direction rightDir)
        {
            IntPtr pNewMap = Join(left.pMapApi, leftDir, right.pMapApi, rightDir);
            if (pNewMap == IntPtr.Zero)
                return null;
            return new P4MapApi(left.pServer, pNewMap);
        }

        /**************************************************************************
        *
        *  Translate: Translate a file path from on side of the mapping to the 
        *				other.
        *
        *   pMap:	 Pointer to the P4MapApi instance 
        *   p:		 String representing the the path to translate
        *   d:		 The direction to perform the translation L->R or R->L
        *
        *   Returns: char * String translate path, NULL if translation failed
        *
        **************************************************************************/

        [DllImport("p4bridge.dll", EntryPoint = "Translate",
            CharSet = CharSet.Ansi, CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr TranslateA(IntPtr pMap, String p, Direction d);

        [DllImport("p4bridge.dll", EntryPoint = "Translate",
            CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr TranslateW(IntPtr pMap, IntPtr p, Direction d);

        [DllImport("p4bridge.dll",  CallingConvention = CallingConvention.Cdecl)]
        private static extern void DeletePtr( IntPtr p );

        /// <summary>
        ///  Translate a file path from on side of the mapping to the other
        /// </summary>
        /// <param name="path">The path to translate</param>
        /// <param name="direction">The direction to perform the translation L->R or R->L</param>
        /// <returns>Translated path, Null if no translation (path is not mapped)</returns>
        public String Translate(String path, Direction direction)
        { 
            IntPtr pStr = IntPtr.Zero;

            if (pServer.UseUnicode)
            {
                using (PinnedByteArray pPath = pServer.MarshalStringToIntPtr(path))
                {
                    pStr = TranslateW(pMapApi, pPath, direction);
                }
            }
            else 
                pStr = TranslateA(pMapApi, path, direction);

            if (pStr != IntPtr.Zero)
            {
                String translation = pServer.MarshalPtrToString(pStr);
                DeletePtr(pStr);
                return translation;
            }
            return null;
        }

        #region IDisposable Members

        /// <summary>
        /// Free the wrapped native object
        /// </summary>
        public void Dispose()
        {
            if (pMapApi != IntPtr.Zero)
                DeleteMapApi(pMapApi);

            pMapApi = IntPtr.Zero;
        }

        #endregion
    }
}
