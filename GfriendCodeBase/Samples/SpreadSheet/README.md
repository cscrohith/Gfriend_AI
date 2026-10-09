## Spreadsheet Library Documentation
The Spreadsheet Library enables seamless interaction with Excel files—supporting file creation, data insertion, formatting, and visualization. Below is a detailed breakdown of its primary functions and their usage.

### 1. OpenExcelFile
Purpose:
The OpenExcelFile keyword opens an existing Excel file from a given file path. It is the first step in working with an existing Excel file, allowing further modifications and data manipulations.

Example:

```javascript

SpreadSheet.Open Excel File


```



### 2. Format Excel Cell(row, column, properties)
Purpose:
The Format Excel Cell keyword allows users to format specific cells in the spreadsheet by modifying their properties. This includes font style, font color, background color, border, and cell values.

##### Parameters:
row: The row number of the target cell.

column: The column number or letter of the target cell.

properties: A colon-separated string specifying the formatting properties for the cell. Properties can include:

Font Style: Set the font style (e.g., "Bold", "Italic").

Font Color: Set the font color (e.g., "Red", "Blue").

Background Color: Set the background color (e.g., "Yellow", "Green").

Border: Set the border style (e.g., "Solid", "Dashed").

Value: Set the value/content of the cell (e.g., "Hello", 123).


Example:
```javascript

SpreadSheet.FormatExcelCell(1, 1, "Bold:True:FontColor:Red:BackgroundColor:Yellow:Value:Hello")


```




### 3. Add Text to Given Position (text, row value, column value)
Purpose:
The Add Text to Given Position keyword allows users to add text to a specific cell at a given row and column.


#### Parameters:
text: The text to be added to the specified cell.

row: The row number where the text should be inserted.

column: The column number where the text should be inserted.

Example:
```javascript

SpreadSheet.AddTextToGivenPosition("Test Data", 2, 3)


```




### 4. Add Row Data With Style(rowData, style)
Purpose:
The Add Row Data With Style keyword allows users to add a new row of data to the Excel file with optional styling for the row.

#### Parameters:
rowData: A list or array of data values to be inserted as a new row.

style: Optional styling for the row, such as font style, font color, background color, etc. This is specified in a colon-separated format.

Example:
```javascript

SpreadSheet.AddRowDataWithStyle(["Name", "Age", "Occupation"], "Bold:True:FontColor:Blue")


```

### 5. Add Column Data (columnData)
Purpose:
The Add Column Data keyword  adds a set of data to a specified column in the Excel file.


#### Parameters:
columnData: A list or array of data values to be inserted into the column.

Example:
```javascript

SpreadSheet.AddColumnData([1, 2, 3, 4, 5])


```


### 6. Add Row Data()
Purpose:
The Add Row Data keyword  adds a new row of data to the spreadsheet without any styling applied.


#### Parameters:
rowData: A list or array of data values to be inserted as a new row.

Example:
```javascript

SpreadSheet.AddRowData(["Product", "Price", "Stock"])


```

### 7. Add Row Header()
Purpose:
The Add Row Header keyword adds a new header row to the Excel file. This function is typically used for adding column headers.

#### Parameters:
rowHeader: A list or array containing the column headers to be added.

Example:
```javascript

SpreadSheet.AddRowHeader(["Name", "Age", "City"])


```




### 8. Create New Spreadsheet
Purpose:
The Create New Spreadsheet keyword initializes a blank spreadsheet and saves it in the GFriend output directory.

Example:
```javascript

SpreadSheet.CreateNewSpreadsheet


```

### 9. Create New Spreadsheet With Path(Filepath)
Purpose:
The Create New Spreadsheet With Path keyword creates a new spreadsheet and saves it to the specified file path.

#### Parameters:
Filepath: The path where the new spreadsheet should be saved, including the file name and extension (e.g., C:\Users\Documents\newfile.xlsx).

Example:
```javascript

SpreadSheet.CreateNewSpreadsheetWithPath("C:\\Users\\Documents\\newfile.xlsx")


```

