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
 * Name		: p4base.h
 *
 * Author	: dbb
 *
 * Description	:  p4base is the base class for all classes which handles 
 *  (pointers)for instances of those classes are passed in and out if the DLL.
 *  It provides a mechanism to register the handles to those objects when they
 *  are created so they can be validated when passed in as parameters.
 *
 ******************************************************************************/
#include "StdAfx.h"
#include "p4base.h"
#include "lock.h"

/*******************************************************************************
* Keep a doubly linked list of handles for each type to be tracked.
*******************************************************************************/
#ifdef _DEBUG
p4base** p4base::pFirstItem = NULL;
p4base** p4base::pLastItem = NULL;
#endif

int p4base::InitP4BaseLocker()
{
	Locker.InitCritSection();
	return 1;
}

// Used to lock access for multi threading
ILockable p4base::Locker = ILockable();

int p4base::P4BaseLockerInit = p4base::InitP4BaseLocker();

/*******************************************************************************
* Constructor
*
*   Add this object to the correct list, based on its type.
*
*   nType: The type of this object. It is passed from the derived objects 
*       constructer so it can be determined at run time. A call to the virtual
*       function GetType() does not work here in the base class constructor.
*
*******************************************************************************/
p4base::p4base(int ntype)
{
    // save the type for use in the destructor when we can no longer use the
    //  virtual function GetType().
    type =  ntype;

#ifdef _DEBUG
	LOCK(&Locker); 

    // Check to see if we have allocated are array of lists yet. These will only
    //  be created when the first object is registered.
    if (!pFirstItem)
    {
        pFirstItem = new p4base*[p4typesCount];
        for (int i = 0; i < p4typesCount; i++)
        {
            pFirstItem[i] = NULL;
        }
    }
    if (!pLastItem)
    {
        pLastItem = new p4base*[p4typesCount];
        for (int i = 0; i < p4typesCount; i++)
        {
            pLastItem[i] = NULL;
        }
   }

    // Initialize the list pointers
    pNextItem = NULL;
    pPrevItem = NULL;

    // Add to the list of objects registered to be exported
    if(!pFirstItem[type])
    {
        // first object, initialize the list with this as the only element
        pFirstItem[type] = this;
        pLastItem[type] = this;
    }
    else
    {
        // add to the end of the list
        pLastItem[type]->pNextItem = this;
        pPrevItem = pLastItem[type];
        pLastItem[type] = this;
    }
#endif
}

/*******************************************************************************
* Destructor
*
*   Remove this object from the correct list, based on its type.
*
*******************************************************************************/
p4base::~p4base(void)
{
#ifdef _DEBUG
	LOCK(&Locker); 

    // Remove from the list
    if (!pPrevItem && !pNextItem)
    {
        // last object in the list, so NULL out the list head and tail pointers
        pFirstItem[type] = NULL;
        pLastItem[type] = NULL;
    }
    else if (!pPrevItem && pNextItem)
    {
        // first object in list, set the head to the next object in the list
        pFirstItem[type] = pNextItem;
        pNextItem->pPrevItem = NULL;
    }
    else if (pPrevItem && !pNextItem)
    {
        // last object, set the tail to the pervious object in the list
        pLastItem[type] = pPrevItem;
        pPrevItem->pNextItem = NULL;
    }
    else 
    {
        // in the middle of the list, so link the pointers for the previous 
        //  and next objects.
        pPrevItem->pNextItem = pNextItem;
        pNextItem->pPrevItem = pPrevItem;
    }
#endif
}

void p4base::Cleanup(void)
{
#ifdef _DEBUG
    if (pFirstItem)
    {
		delete[] pFirstItem;
    }
    if (pLastItem)
    {
		delete[] pLastItem;
   }
#endif
}

int p4base::ValidateHandle_Int( p4base* pObject, int type )
{
 	if (!pObject)
        return 0;

    p4base* pCur = NULL;

    // Use Windows Structured Exception Handling to detect memory violations
    __try
    {
        if ((type < 0) || (type >=p4typesCount) || (type != pObject->Type()))
        {
			return 0; // invalid type
		}
#ifndef _DEBUG
		return 1;
	}
    __except (1) //EXCEPTION_EXECUTE_HANDLER
    {
        // access violation, so definitely not valid.
        return 0;
    }
#else
        pCur = pFirstItem[type];
    }
    __except (1) //EXCEPTION_EXECUTE_HANDLER
    {
        // access violation, so definitely not valid.
        return 0;
    }
    while ( pCur != NULL)
    {
        if (pObject == pCur)  // pointers are the same
        {   
			return 1;
		}
        pCur = pCur->pNextItem;
    }
#endif
 
    return 0;
}

/*******************************************************************************
* ValidateHandle( p4base* pObject )
*
*   Static function to validate a handle
*
*   nType: The type of this object. It is passed from the derived objects 
*       constructer so it can be determined at run time. A call to the virtual
*       function GetType() does not work here in the base class constructor.
*
*******************************************************************************/
int p4base::ValidateHandle( p4base* pObject, int type )
{
	LOCK(&Locker);

	return ValidateHandle_Int( pObject, type );
}
