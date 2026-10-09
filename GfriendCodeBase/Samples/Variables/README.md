# GFriend Variables

## User defined variables

User can define their own variables in the script as well as variable file. During parsing the script (which is the very first step of execution) GFriend replaces variables to their values.

### Variables Declaration

Note : Do not add any space before and after = 

Example for valid declaration : 
```javascript
${Message}=INC12345 is created//valid declaration 

MessageContains
{
    Contains (${Message},INC)
    //Contains keyword is passed as ${Message} contains the text INC
    Pass (${Message})
    //Pass(${Message}) writes the text "INC12345 is created" in the report file
}
```
1. Simple User Defined variable example

    ```javascript
    using Web

    ${UserID}=Doe.John@example.com
    ${UserPW}=NoOneCantKnow

    Login Test
    {
        Web.Open With Chrome(http://www.testyou.in/Login.aspx)
        Web.Set Text(//*[@id="ctl00_CPHContainer_txtUserLogin"],${UserID})
        Web.Set Text(//*[@id="ctl00_CPHContainer_txtPassword"],${UserPW})
        Web.Click Object (//*[@id="ctl00_CPHContainer_btnLoginn"])
        
        If: Web.Wait For Text (Userid or Password did Not Match !!,3)
        {
            Fail(Password not match)
        }
    }
    ```

1. User Defiend variable with variable file

    Following example just same as script in above example. You can split variables into independant file. It helps you to manage frequently changed values in the script. Also one variable file can be referenced by multiple scripts.

    **_UserInfo.gfvar_**
    ```javascript
    ${UserID}=Doe.John@example.com
    ${UserPW}=NoOneCantKnow
    ```

    **_LoginTest.txt_**
    ```javascript
    using Web
    using UserInfo.gfvar

    Login Test
    {
        Web.Open With Chrome(http://www.testyou.in/Login.aspx)
        Web.Set Text(//*[@id="ctl00_CPHContainer_txtUserLogin"],${UserID})
        Web.Set Text(//*[@id="ctl00_CPHContainer_txtPassword"],${UserPW})
        Web.Click Object (//*[@id="ctl00_CPHContainer_btnLoginn"])
        
        If: Web.Wait For Text (Userid or Password did Not Match !!,3)
        {
            Fail(Password not match)
        }
    }
    ```

## Dynamic variables

User defined variables are replaced with its values before running the script. But dynamic variables work different.

```javascript
using Web

Get Web Text Test
{
    Web.Open With Chrome(google.com)
    Web.Get Text (//*[@id="gbw"]/div/div/div[1]/div[1]/a,${GMAIL})
    Equals (${GMAIL},Gmail)
}
```
Above test script get the textt from web into variable(${GMAIL}) and compare with expected value.


## System defined variables

GFriend also provide pre-defined variables for testing. Following example is connecting EWS page with device ip address. (Click [here](../../GFCore/README.md#a_sys_var) to see the list of system defined variables.)


```javascript
using JediOmni
using Web

Open EWS Page
{
    Web.Open With Chrome(${DUT_Address})
}
```

## Advanced Examples

Since user defined variables are replaced with their value before test running, you can make test script as follow with variables.

```javascript
${Browser} = FireFox // Can be Chrome, IE or Edge

using Web

Compatibility Test
{
    Web.Open With ${Browser}(google.com)
}
```

Also you can concatenate variable values as example follow:

```javascript
${Target} = google
${SiteLocation}=co.kr // can be com.uk or other

using Web

Compatibility Test
{
    Web.Open With Chrome(${Target}.${SiteLocation})
}
```

