using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace HP.GFriend.Keywords
{
    public enum ControlType
    {
        UNKNOWN,
        COMBOBOX,
        TEXTBOX,
        TOGGLE,
        RADIO
    }
    public class PrinterDriverOptions
    {
        public List<PrinterDriverOption> Options;

        public PrinterDriverOptions()
        {
            Options = new List<PrinterDriverOption>();
        }

        public PrinterDriverOption GetOption(string optionName)
        {
            return Options.Where(o => o.OptionName.Equals(optionName)).FirstOrDefault();
        }

        public void FromExcel(string menumapFilePath, string sheetName, string optionColumName="Feature", string valueColumnName="Option Level 1", string automationIdColumnName="Automation ID", string menuTypeColumnName="Menu Type")
        {
            Microsoft.Office.Interop.Excel.Application excelApp = new Microsoft.Office.Interop.Excel.Application();
            Workbook workbook = excelApp.Workbooks.Open(menumapFilePath, false, true);
            _Worksheet worksheet = workbook.Sheets[sheetName];
            Range dataRange = worksheet.UsedRange;

            Range optionCell = null;
            Range valueCell = null;
            Range automationIdCell = null;
            Range menuTypeCell = null;

            foreach(Range cell in dataRange.Cells)
            {
                if(cell.Text == optionColumName)
                {
                    optionCell = cell;
                }
                else if(cell.Text == valueColumnName)
                {
                    valueCell = cell;
                }
                else if(cell.Text == automationIdColumnName)
                {
                    automationIdCell = cell;
                }
                else if(cell.Text == menuTypeColumnName)
                {
                    menuTypeCell = cell;
                }

                if(optionCell != null && valueCell != null && automationIdCell != null && menuTypeCell != null)
                {
                    break;
                }
                
            }
            if (optionCell == null || valueCell == null || automationIdCell == null || menuTypeCell == null)
            {
                // Close Files
                Marshal.ReleaseComObject(dataRange);
                Marshal.ReleaseComObject(worksheet);

                //close and release
                workbook.Close(false);
                Marshal.ReleaseComObject(workbook);

                //quit and release
                excelApp.Quit();
                throw new NotSupportedException("Given menumap file is not supported");
            }

            // Read Data
            Options = new List<PrinterDriverOption>();
            PrinterDriverOption currentOption = null;
            
            foreach(Range row in dataRange.Rows)
            {
                if (row.Row > optionCell.Row)
                {
                    if (!string.IsNullOrEmpty(worksheet.Cells[row.Row, optionCell.Column].Text))
                    {
                        if (!string.IsNullOrEmpty(worksheet.Cells[row.Row, automationIdCell.Column].Text) &&
                            !string.IsNullOrEmpty(worksheet.Cells[row.Row, menuTypeCell.Column].Text))
                        {
                            currentOption = new PrinterDriverOption();
                            currentOption.OptionName = worksheet.Cells[row.Row, optionCell.Column].Text.Trim();
                            
                            if (Enum.TryParse<ControlType>(worksheet.Cells[row.Row, menuTypeCell.Column].Text.ToUpper().Trim(), out ControlType parsedControlType))
                            {
                                currentOption.OptionType = parsedControlType;
                            }
                            else
                            {
                                currentOption.OptionType = ControlType.UNKNOWN;
                            }
                            currentOption.AutomationId = worksheet.Cells[row.Row, automationIdCell.Column].Text.Trim();
                            Options.Add(currentOption);
                        }

                    }

                    if (currentOption != null &&
                        !string.IsNullOrEmpty(worksheet.Cells[row.Row, valueCell.Column].Text) &&
                        !string.IsNullOrEmpty(worksheet.Cells[row.Row, automationIdCell.Column].Text))
                    {
                        currentOption.Values.Add(worksheet.Cells[row.Row, valueCell.Column].Text.Trim(), worksheet.Cells[row.Row, automationIdCell.Column].Text.Trim());
                    }
                }
            }


            // Close Files
            Marshal.ReleaseComObject(dataRange);
            Marshal.ReleaseComObject(worksheet);

            //close and release
            workbook.Close(false);
            Marshal.ReleaseComObject(workbook);

            //quit and release
            excelApp.Quit();

        }
        public override string ToString()
        {
            string str = string.Empty;
            foreach(PrinterDriverOption option in Options)
            {
                str = string.Join(Environment.NewLine, str, option.ToString());
            }

            return str;
        }
    }
}
