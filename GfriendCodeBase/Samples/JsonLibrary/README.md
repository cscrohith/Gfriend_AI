# Json Library Sample

Json library enables you to handing json formatted text in your script. To create json formatted text, use "Add Json Element" keyword, to get information from json formatted text, use "Load Json" and "Get Value" keywords.

## Creating Json Formatted Text
You can use those outputs of sample script with Rest library to send json based HTTP REST request.

### Simple key-value pair json

```javascript
using Json

Simple Key Value Pair Json
{
    Json.Add Json Element (${body},source,en)
    Json.Add Json Element (${body},target,de)
    Json.Add Json Element (${body},text,Hello GFriend)
}
```
Variable ${body} will be following after executing the script:
```json
{
    "source":"en",
    "target":"de",
    "text":"Hello GFriend"
}
```

### Array element

If you give array format in value, GFriend will automatically convert it to array type as example below (Please be aware of using comma with backslash):

```javascript
using Json

Arrary Element
{
    Json.Add Json Element (${body},source,en)
    Json.Add Json Element (${body},target,de)
    Json.Add Json Element (${body},text,["Hello GFriend"\,"I need automation test framework"] )
}
```
Variable ${body} will be following after executing the script:
```json
{
   "source":"en",
   "target":"de",
   "text":[
      "Hello GFriend",
      "I need automation test framework"
   ]
}
```

### Nested Json

You can also add element with json formatted. GFriend also will automatically identify if value is json formatted text.
```javascript
using Json

Nested Json
{
    Json.Add Json Element (${body},source,en)
    Json.Add Json Element (${body},target,de)
    Json.Add Json Element (${body},text,["Hello GFriend"\,"I need automation test framework"] )

    Json.Add Json Element(${newBody},request,translate)
    Json.Add Json Element(${newBody},data,${body})
}
```
Variable ${newBody} will be following after executing the script:
```json
{
   "request":"translate",
   "data":{
      "source":"en",
      "target":"de",
      "text":[
         "Hello GFriend",
         "I need automation test framework"
      ]
   }
}
```

## Querying value from Json formatted text

Before querying value from json formatted text, you must load json first by using Load Json keyword. After that you can query values by json path syntax with Get Value keyword. Refer https://restfulapi.net/json-jsonpath/v to more information of json path syntax.

By using this, you can get information from REST response.


```javascript
Json Query Example
{
    Json.Add Json Element (${body},source,en)
    Json.Add Json Element (${body},target,de)
    Json.Add Json Element (${body},text,["Hello GFriend"\,"I need automation test framework"] )

    Json.Add Json Element(${newBody},request,translate)
    Json.Add Json Element(${newBody},data,${body})
    
    Json.Load Json (${newBody})
    Json.Get Value (${target},$.data.target) // $. can be ommited. if you use data.target it will get same value.
    Json.Get Value (${text},data.text)
    Json.Get Value (${firstText},data.text[0])
}
```

After executing script above, variables will have following values:
```javascript
${target} = de
${text} = ["Hello GFriend","I need automation test framework"]
${firstText} = Hello GFriend
```