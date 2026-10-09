# Return Statements
In GFriend , we have 3 Returm statements which will returns at that statement and it wont executes the sub blocks from the libraries.
1.  Return With Pass(messgae)
2.  Return With Fail(messgae)
3.  Return With Error(messgae)

Return with Pass or Return with Fail or Return with Error should not be used from the test script file , it should be used from the library file. Instead we can use Pass() or Fail() or Error () in test script file.

Ex:

```javascript
using CustomLibrary_Level1.gflib
test
{   
    CustomLibrary_Level1.callFuntion
    Pass (last step1 from Sample_TestCase)
    Pass (last step2 from Sample_TestCase)
    Pass (last step3 from Sample_TestCase)
    Pass (last step4 from Sample_TestCase)
}
```

```javascript
using CustomLibrary_Level2.gflib

callFuntion
{  
    // Calling CustomLibrary_Level2 method CallLib2Function
    CustomLibrary_Level2.CallLib2Function
   
    If: Equals (0,1)
    {
        Repeat:5
        {
            Pass (Passing from CustomLibrary_Levdel1)
            sleep (1)
        }
        
    }
    Fail:
    {
        Sleep (6)
    }
    
    Pass (outside if block in CustomLibrary_Levdl1)
}
```

```javascript
CallLib2Function
{
     // As we are returning with Pass, the below IF condition(If block) will not be executed. 
     // The complete method in this library is returned as Pass
     Return With Pass (Returning with pass)

     If: Equals (0,1)
    {
        Repeat:5
        {
            Pass (Passing from CustomLibrary_Levdel1)
            sleep (1)
        }
        
    }
    Fail:
    {
        Sleep (10)
        
    }
    
    Pass (outside if block in CustomLibrary_Levdl2)
}

```
